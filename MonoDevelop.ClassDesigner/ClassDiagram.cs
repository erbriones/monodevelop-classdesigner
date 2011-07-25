// 
// ClassDiagram.cs
//  
// Author:
//       Evan Briones <erbriones@gmail.com>
// 
// Copyright (c) 2010 Evan Briones
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

using Gdk;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;

using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.ClassDesigner.Visitor;

using MonoHotDraw;
using MonoHotDraw.Figures;
using MonoHotDraw.Util;

namespace MonoDevelop.ClassDesigner
{
	public sealed class ClassDiagram : StandardDrawing, ISerializableFigure
	{	
		public ClassDiagram () : this (GroupingSetting.Member, MembersFormat.FullSignature)
		{
		}
		
		public ClassDiagram (GroupingSetting grouping, MembersFormat format) : this (grouping, format, null)
		{
		}
		
		public ClassDiagram (GroupingSetting grouping, MembersFormat format, IEnumerable<IFigure> figures)
		{
			majorVersion = 1;
			minorVersion = 1;
			groupSetting = grouping;
			membersFormat = format;
			CreatedFigure += OnCreatedHandler;
			FigureAdded += OnFigureAddedInheritanceHandler;
			FigureRemoved += OnFigureRemovedInheritanceHandler;
		}
		
		public override void Dispose ()
		{
			CreatedFigure -= OnCreatedHandler;
			FigureAdded -= OnFigureAddedInheritanceHandler;
			FigureRemoved -= OnFigureRemovedInheritanceHandler;
		}
		
		#region ISerializableFigure implementation
		public XElement Serialize ()
		{
			var xml = new XElement ("ClassDiagram",
				new XAttribute ("MajorVersion", majorVersion),
			    new XAttribute ("MinorVersion", minorVersion),
			   	new XAttribute ("MembersFormat", membersFormat.ToString ()),
			    new XElement ("Font",
			    	new XAttribute ("Name", AttributeFigure.GetDefaultAttribute(FigureAttribute.FontFamily)),
			        new XAttribute ("Size", AttributeFigure.GetDefaultAttribute(FigureAttribute.FontSize))
			    )
			);
			
			xml.Add (Figures.OfType<ISerializableFigure> ().Select (f => f.Serialize ()));
			
			return xml;
		}

		public void Deserialize (XElement xml, ProjectDom dom)
		{
			DeserializeGrouping (xml.Attribute ("Grouping"));
			DeserializeMembersFormat (xml.Attribute ("MembersFormat"));
			
			foreach (var element in xml.Elements ()) {
				switch (element.Name.LocalName) {
					case "Class":
					case "Struct":
					case "Enum":
					case "Interface":
					case "Delegate":
						LoadType (element, dom);
						break;
					case "Comment":
						var comment = new CommentFigure ();
						comment.Deserialize (element, dom);
						Add (comment);
						break;
					case "Font":
						DeserializeFont (element);
						break;
					default:
						throw new DeserializationException ("Unknown XML element: " + element.Name);
				}
			}
		}
		
		private void DeserializeFont (XElement font)
		{
			if (font == null) {
				throw new ArgumentNullException("xml");
			}
			
			var name = font.Attribute ("Name");
			if (name != null && !String.IsNullOrWhiteSpace (name.Value)) {
				AttributeFigure.SetDefaultAttribute (FigureAttribute.FontFamily, name.Value);
			}
			
			var size = font.Attribute ("Size");
			if (size != null && !String.IsNullOrWhiteSpace (size.Value)) {
				try {
					AttributeFigure.SetDefaultAttribute (FigureAttribute.FontSize, Double.Parse(size.Value));
				}
				catch (Exception e) {
					throw new DeserializationException ("Couldn't parse font size value: " + size.Value, e);
				}
			}
		}
		
		private void DeserializeGrouping (XAttribute grouping)
		{
			if (grouping != null) {
				GroupingSetting value;
				if (Enum.TryParse (grouping.Value, out value)) {
					Grouping = value;
				} else {
					throw new DeserializationException ("Couldn't parse Grouping value: " + grouping.Value);
				}
			}
		}
		
		private void DeserializeMembersFormat (XAttribute format)
		{
			if (format != null) {
				MembersFormat value;
				if (Enum.TryParse (format.Value, out value)) {
					Format = value;
				} else {
					throw new DeserializationException ("Couldn't parse MembersFormat value: " + format.Value);
				}
			}
		}
		#endregion

		public MembersFormat Format {
			get { return membersFormat; }
			set {
				if (membersFormat == value)
					return;
				
				membersFormat = value;
				
				var visitor = new MemberFormatVisitor (this);
				foreach (var figure in Figures) {
					var tf = figure as TypeFigure;
					if (tf != null)
						visitor.VisitFigure (tf);
				}
			}	
		}
		
		public GroupingSetting Grouping {
			get { return groupSetting; }
			set {
				if (groupSetting == value)
					return;
				
				groupSetting = value;
				var visitor = new GroupFormatVisitor (this, null);
				
				foreach (var figure in Figures) {
					var tf = figure as TypeFigure;
					if (tf == null)
						continue;
					
					visitor.TypeFigure = tf;
				
					foreach (IFigure fig in tf.Compartments)
						fig.AcceptVisitor (visitor);
				}
			}
		}
		
		#region TypeFigure Add/Remove/Update
		public void Add (IType type)
		{
			if (!HasTypeFigure (type.FullName)) {
				CreateTypeFigure(type);
			}
		}
		
		public void AddRange (IEnumerable<IType> types)
		{
			foreach (var type in types) {
				Add (type);
			}
		}
		
		public void Remove (IType type)
		{
			var fig = GetTypeFigure(type.FullName);
			if (fig != null) {
				Remove(fig);
			}
		}
		
		public void RemoveRange (IEnumerable<IType> types)
		{
			foreach (var type in types) {
				Remove (type);
			}
		}
		
		public void Update (IType type)
		{
			var figure = GetTypeFigure(type.FullName) as TypeFigure;
			if (figure != null) {
				figure.Rebuild(type);
				
				foreach (var compartment in figure.Compartments) {
					compartment.AcceptVisitor (new GroupFormatVisitor (this, figure));
				}
			}
		}
		
		public void UpdateRange (IEnumerable<IType> types)
		{
			foreach (var type in types) {
				Update (type);
			}
		}
		#endregion
		
		public bool HasTypeFigure (string fullName)
		{
			return (String.IsNullOrEmpty (fullName)) ? false : Figures
				.OfType<TypeFigure> ()
				.Any (tf => tf.TypeFullName == fullName);
		}
		
		#region Inheritence line updaters
		void OnFigureAddedInheritanceHandler (object o, FigureEventArgs e)
		{
			var cf = e.Figure as ClassFigure;
			if (cf != null) {
				BaseInheritanceLineFromDiagram (cf);
				DerivedInheritanceLinesFromDiagram (cf);
			}
		}
		
		void OnFigureRemovedInheritanceHandler (object o, FigureEventArgs e)
		{
			var cf = e.Figure as ClassFigure;
			if (cf != null) {
				var toRemove = new List<InheritanceConnectionFigure> ();
				var lines = Figures
					.OfType<InheritanceConnectionFigure> ()
					.Where (l => l.ConnectionLine.StartFigure == e.Figure || l.ConnectionLine.EndFigure == e.Figure);
				foreach (var line in lines) {
					toRemove.Add (line);
				}
				toRemove.ForEach (line => Remove (line));
			}
		}
		
		public void BaseInheritanceLineFromDiagram (ClassFigure derivedFigure)
		{
			if (derivedFigure == null) {
				throw new ArgumentNullException ("derivedFigure");
			}
			
			var baseFigure = GetTypeFigure (derivedFigure.BaseTypeFullName) as ClassFigure;
			
			if (baseFigure != null) {
				Add (new InheritanceConnectionFigure (derivedFigure, baseFigure));
			}
		}
		
		public void DerivedInheritanceLinesFromDiagram (ClassFigure baseFigure)
		{
			if (baseFigure == null) {
				throw new ArgumentNullException ("baseFigure");
			}
			
			var lines = new List<InheritanceConnectionFigure> ();
			
			foreach (var cf in Figures.OfType<ClassFigure> ()) {
				if (cf.BaseTypeFullName == baseFigure.TypeFullName) {
					lines.Add (new InheritanceConnectionFigure (cf, baseFigure));
				}
			}
			
			lines.ForEach(line => Add (line));
		}
		#endregion
		
		// TODO: Kill this method off entirely.
		public TypeFigure CreateTypeFigure (IType type)
		{
			if (type == null || HasTypeFigure (type.FullName)) {
				return null;
			}
			
			var figure = TypeFigure.FromType (type);
			Add (figure);
			
			foreach (var fig in figure.Compartments) {
				fig.AcceptVisitor (new GroupFormatVisitor (this, figure));
			}
			
			OnCreated (new FigureEventArgs (figure, RectangleD.Empty));
			
			return figure;
		}

		public TypeFigure GetTypeFigure (string fullName)
		{
			return (String.IsNullOrEmpty (fullName)) ? null : Figures
				.OfType<TypeFigure> ()
				.SingleOrDefault (f => f.TypeFullName == fullName);
		}
		
		//FIXME: Add proper exceptions or dialog for missing types, etc..
		void LoadType (XElement element, ProjectDom dom)
		{	
			TypeFigure figure = null;
			
			//
			// Figure Attributes
			//
			
			var typeName = element.Attributes ()
				.Where (a => a.Name == "Name")
				.SingleOrDefault ();
			
			var type = dom.GetType(typeName.Value);
			figure = (TypeFigure) CreateTypeFigure (type);
			
			if (figure == null)
				return;
			
			if (figure is ClassFigure) {
				var cls = (ClassFigure) figure;
				var hideInheritance = element.Attributes ()
					.Where (a => a.Name == "HideInheritanceLine")
					.SingleOrDefault ();
				
				if (hideInheritance != null)
					cls.HideInheritance = Boolean.Parse (hideInheritance.Value);
			}
					
			var collapsed = element.Attributes ()
				.Where (a => a.Name == "Collapsed")
				.SingleOrDefault ();
			
			if (collapsed != null && !Boolean.Parse (collapsed.Value))
				figure.Collapse ();
	
			//
			// Position Element
			//
			
			var position = element.Elements ()
				.Where (e => e.Name == "Position")
				.SingleOrDefault ();
			
			figure.DeserializePosition (position);
			
			//
			// Members Element
			//
			
			var members = element.Elements ()
				.Where (e => e.Name == "Members")
				.SingleOrDefault ();
			
			if (members != null) {
				foreach (var e in members.Elements ()) {
					var name = e.Attribute ("Name");
					
					if (name == null)
						return;
					Console.WriteLine ("hide member {0}", name.Value);
					var member = type.SearchMember (name.Value, true)
						.SingleOrDefault ();
					
					if (member == null)
						return;
					
					foreach (var c in figure.Figures) {
						var memberFigure = c.Figures
							.OfType<MemberFigure> ()
							.Where (f => f.Name == member.Name)
							.SingleOrDefault ();
						
						if (memberFigure == null)
							continue;
						
						memberFigure.Hide ();
					}
				}
				// FIXME:
				// Make sure Empty Compartments Hidden
				// ie. hidelist = figure.Figures.Where (c == c.IsEmpty);
				// hidelist.ForEach (f => f.Hide ());
			}
			
			//
			// AssociationLine Element
			//
			
			var association_lines = element.Elements ()
				.Where (e =>element.Name == "AssociationLine");
			
			if (association_lines != null) {
				foreach (var e in association_lines) {
					var name = e.Attribute ("Name");
					var t = e.Attribute ("Type");
					
				}
			}
			
			//
			// InheritanceLine
			//
			
			//
			// Associations Element
			//
			
			var associations = element.Elements ()
				.Where (e => e.Name == "ShowAsAssociation")
				.SingleOrDefault ();
			
			if (associations != null) {
				foreach (var e in associations.Elements ("Property")) {
					var name = e.Attribute ("Name");
										
					if (name == null)
						continue;
					
					var property = type.Properties
						.Where (p => p.Name == name.Value)
						.SingleOrDefault ();
					
					if (property == null)
						continue;
					
					IFigure startfig;
										
					if (HasTypeFigure (property.ReturnType.FullName))
						startfig = GetTypeFigure (property.ReturnType.FullName);
					else
						startfig = CreateTypeFigure (property.ReturnType.Type);					
					
					if (startfig == null)
						continue;
					
					Add (new AssociationConnectionFigure (property, ConnectionType.Association, startfig, figure));
				}
			}
			
			//
			// Association Collections Element
			//
			
			var collection = element.Elements ()
				.Where (e => e.Name == "ShowAsAssociationCollection")
				.SingleOrDefault ();
			
			if (collection != null) {
					foreach (var e in collection.Elements ("Property")) {
					var name = e.Attribute ("Name");
										
					if (name == null)
						continue;
					
					var property = type.Properties
						.Where (p => p.Name == name.Value)
						.SingleOrDefault ();
					
					if (property == null)
						continue;
					
					IFigure startfig;
										
					if (HasTypeFigure (property.ReturnType.FullName))
						startfig = GetTypeFigure (property.ReturnType.FullName);
					else
						startfig = CreateTypeFigure (property.ReturnType.Type);
					
					if (startfig is System.Collections.ICollection)
						Add (new AssociationConnectionFigure (property, ConnectionType.CollectionAssociation,
						                                  startfig, figure));
					else
						throw new ArgumentException ("The type is not a valid collection.");
				}
			}
			
			//
			// Compartments Element
			//
			
			var compartments = element.Elements ()
				.Where (e => e.Name == "Compartments")
				.SingleOrDefault ();
			
			if (compartments != null) {							
				foreach (var e in compartments.Elements ("Compartment")) {
					var name = e.Attribute ("Name");
					
					if (name == null)
						continue;
					
					var compartment = figure.Figures
						.OfType<CompartmentFigure> ()
						.Where (c => c.Name == name.Value)
						.SingleOrDefault ();
					
					if (compartment == null)
						continue;
					
					collapsed = e.Attribute ("Collapsed");
					
					if (collapsed == null)
						continue;
					
					if (Boolean.Parse (collapsed.Value))
						compartment.Hide ();
				}
			}
			
			//
			// NestedTypes Element
			//
			
			var nestedTypes = element.Elements ()
				.Where (e => e.Name == "NestedTypes")
				.SingleOrDefault ();
			
			if (nestedTypes != null) {
				var nsupport = figure as INestedTypeSupport;
					
				//FIXME: probably should give a dialog or exception.
				if (nsupport == null)
					return;
				
				foreach (var nt in nestedTypes.Elements ("NestedType")) {
					var name = nt.Attribute("Name");
					
					if (name == null)
						continue;
					
					var nestedFigure = CreateTypeFigure (dom.GetType(name.Value));
					
					if (nestedFigure == null)
						continue;
					
					nsupport.AddNestedType (nestedFigure);
				}
			}
		}
		
		// TODO: Move these method somewhere far more sensible
		public static double InchesToPixels (double inches)
		{
			if (Screen.Default.Resolution == -1)
				return inches * 60.0;
			
			return inches * Gdk.Screen.Default.Resolution;
		}
		
		public static double PixelsToInches (double pixels)
		{
			if (Screen.Default.Resolution == -1)
				return pixels / 60.0;
			
			return pixels / Gdk.Screen.Default.Resolution;
		}
				
		protected void OnMembersFormatChanged (DiagramEventArgs e)
		{
			var handler = MembersFormatChanged;
			
				if (handler != null)
					handler (this, e);
		}
		
		protected void OnGroupSettingChanged (DiagramEventArgs e)
		{
			var handler = GroupSettingChanged;
			
				if (handler != null)
					handler (this, e);
		}
		
		protected void OnCreatedHandler (object o, FigureEventArgs e)
		{
			Add (e.Figure);
			IFigure last = FiguresReversed.LastOrDefault ();
			
			if (last == null)
				return;
	
			RectangleD rect = last.DisplayBox;
			rect.Inflate (25 + e.Figure.DisplayBox.Width, 25 + e.Figure.DisplayBox.Height);
			
			if (rect.X2 < BasicDisplayBox.X2 + 100)
				e.Figure.MoveTo (rect.X2, rect.Y);	
			else
				e.Figure.MoveTo (rect.Y2 + 25, 0);
		}
		
		protected void OnCreated (FigureEventArgs e)
		{
			var handler = CreatedFigure;
			if (handler != null)
				handler (e.Figure, e);
		}

		public event EventHandler<DiagramEventArgs> MembersFormatChanged;
		public event EventHandler<DiagramEventArgs> GroupSettingChanged;
		public event FigureEventHandler CreatedFigure;
		
		#region Private Member
		readonly int majorVersion;
		readonly int minorVersion;

		GroupingSetting groupSetting;		
		MembersFormat membersFormat;
		#endregion
	}
}
