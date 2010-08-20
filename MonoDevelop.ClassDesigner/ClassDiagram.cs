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
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.ClassDesigner.Visitor;

using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Ide;

using MonoHotDraw;
using MonoHotDraw.Figures;
using MonoHotDraw.Util;

namespace MonoDevelop.ClassDesigner
{
	public sealed class ClassDiagram : StandardDrawing
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
			typeCache = new Dictionary<string, IFigure> ();
			builderList = new LinkedList<IType> ();
			checkList = new List<IType> ();
			CreatedFigure += OnCreatedHandler;
		}
		
		public override void Dispose ()
		{
			CreatedFigure -= OnCreatedHandler;
		}

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
		
		bool HasTypeFigure (string fullName)
		{		
			return Figures.Where (f => f is TypeFigure)
				.Any (tf => ((TypeFigure) tf).Name.FullName == fullName);
		}
		
		public void BaseInheritanceLineFromDiagram (IType superClass)
		{
			var superFigure = GetFigure (superClass.FullName) as ClassFigure;
			var subFigure = GetFigure (superClass.BaseType.FullName) as ClassFigure;
			
			if(superFigure == null || subFigure == null)
				return;
			
			Add (new InheritanceConnectionFigure (subFigure, superFigure));
		}
		
		public void DerivedInheritanceLinesFromDiagram (IType subClass)
		{
			var tmp = new List<IFigure> ();
			var subFigure = GetFigure (subClass.FullName) as ClassFigure;
			
			if (subFigure == null)
				return;
			
			foreach (IFigure f in Figures) {
				var tf = f as ClassFigure;
				
				if (tf == null)
					continue;
				
				if (tf.Name.BaseType.FullName != subClass.FullName)
					continue;
				
				var line = new InheritanceConnectionFigure (subFigure, tf);
				Add (line);
			}
		}
		
		public IFigure CreateFigure (IType type)
		{
			TypeFigure figure;
			
			if (type == null)
				return null;
			
			if (HasTypeFigure (type.FullName))
				return null;
			
			if (type.ClassType == ClassType.Class) {
				Console.WriteLine ("Adding Class");
				figure = new ClassFigure (type);
			} else if (type.ClassType == ClassType.Enum) {
				Console.WriteLine ("Adding Enum");
				figure = new EnumFigure (type);
			} else if (type.ClassType == ClassType.Interface) {
				Console.WriteLine ("Adding Interface");
				figure = new InterfaceFigure (type);
			} else if (type.ClassType == ClassType.Struct) {
				Console.WriteLine ("Adding Struct");
				figure = new StructFigure (type);
			} else if (type.ClassType == ClassType.Delegate) {
				Console.WriteLine ("Adding Delegate");
				figure = new DelegateFigure (type);
			} else {
				return null;
			}
	
			
//			figure.Build ();
//			figure.Update (TypeFigure.UpdateStatus.ALL);
			Add (figure);
			
			return figure;
		}

		public TypeFigure GetFigure (string fullName)
		{			
			if (fullName == null)
				return null;
			
			var figure = Figures
				.Where (f => f is TypeFigure)
				.Where (tf => ((TypeFigure) tf).Name.FullName == fullName)
				.SingleOrDefault ();
			
			return (TypeFigure) figure;
		}
		
		public void Load (string fileName, ProjectDom dom)
		{
			var root = XElement.Load(fileName);
			var grouping = root.Attributes ()
				.Where (a => a.Name == "GroupingSetting")
				.SingleOrDefault ();
			
			if (grouping != null) {
				Console.WriteLine ("Set Group {0}: ", grouping.Value);
				
				if (grouping.Value == GroupingSetting.Alphabetical.ToString ())
					Grouping = GroupingSetting.Alphabetical;
				else if (grouping.Value == GroupingSetting.Kind.ToString ())
					Grouping = GroupingSetting.Kind;
				else
					Grouping = GroupingSetting.Member;
			} else  {
				Grouping = GroupingSetting.Member;
			}
			
			var format = root.Attributes ()
				.Where (a => a.Name == "MembersFormat")
				.SingleOrDefault ();
			
			if (format != null) {
				if (format.Value == MembersFormat.NameAndType.ToString ())
					Format = MembersFormat.NameAndType;
				else if (format.Value == MembersFormat.Name.ToString ())
					Format = MembersFormat.Name;
				else
					Format = MembersFormat.FullSignature;
			}
			
			var font = root.Elements ()
				.Where (e => e.Name == "Font")
				.SingleOrDefault ();
			
			if (font != null) {
				var attribute = font.Attributes ()
					.Where (a => a.Name == "Name")
					.SingleOrDefault ();
			
				if (attribute != null)
					AttributeFigure.SetDefaultAttribute (FigureAttribute.FontFamily, attribute.Value);
				
				attribute = font.Attributes ()
					.Where (a => a.Name == "Size")
					.SingleOrDefault ();
				
				if (attribute != null)
					AttributeFigure.SetDefaultAttribute (FigureAttribute.FontSize, Double.Parse(attribute.Value));	
			}
			
			foreach (var element in root.Elements ()) {				
				switch (element.Name.LocalName) {
					case "Class":
					case "Struct":
					case "Enum":
					case "Interface":
					case "Delegate":
						LoadType (element, dom);
						break;
					case "Comment":
						LoadComment (element);
						break;
					default:
						break;
				}
			}			
		}
		
		
		void LoadComment (XElement element)
		{
			IFigure figure;
			
			//
			// Figure Attributes
			//
			
			var attribute = element.Attribute ("CommentText");
			
			if (attribute == null)
				figure = new CommentFigure (String.Empty);
			else
				figure = new CommentFigure (attribute.Value);
			
			//
			// Position Element
			//
			
			var position = element.Elements ()
				.Where (e => e.Name == "Position")
				.SingleOrDefault ();
			
			PositionFigure (position, figure, true);
			
			Add (figure);
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
			figure = (TypeFigure) CreateFigure (type);
			
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
						
			PositionFigure (position, figure, false);
			
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
						startfig = GetFigure (property.ReturnType.FullName);
					else
						startfig = CreateFigure (property.ReturnType.Type);					
					
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
						startfig = GetFigure (property.ReturnType.FullName);
					else
						startfig = CreateFigure (property.ReturnType.Type);
					
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
					
					var nestedFigure = CreateFigure (dom.GetType(name.Value));
					
					if (nestedFigure == null)
						continue;
					
					nsupport.AddNestedType (nestedFigure);
				}
			}
		}
		
		// A position must have an x, y and width attribute.
		static void PositionFigure (XElement position, IFigure figure, bool hasHeightAttribute)
		{
			if (position == null)
				return;
			
			try {
				var x = Double.Parse (position.Attributes ()
					.Where (a => a.Name == "X")
				    .SingleOrDefault ().Value
				);
				
				var y = Double.Parse (position.Attributes ()
					.Where (a => a.Name == "Y")
				    .SingleOrDefault ().Value
				);
				
				var width = Double.Parse (position.Attributes ()
					.Where (a => a.Name == "Width")
				    .SingleOrDefault ().Value
				);
				
				figure.MoveTo (x, y);
				var height = figure.DisplayBox.Height;
				
				if (hasHeightAttribute) {
					height = InchesToPixels (
						Double.Parse (position.Attributes ()
							.Where (a => a.Name == "Height")
						    .SingleOrDefault ().Value
					    )
					);
				}
					
				figure.DisplayBox = new RectangleD (InchesToPixels (x),
				                                    InchesToPixels (y),
				                                    InchesToPixels (width),
				                                    height);
			} catch {
				Console.WriteLine ("The position element is malformed.");	
			}
		}
		
		public void Write (string fileName)
		{
			var root = new XElement ("ClassDiagram",
				new XAttribute ("MajorVersion", majorVersion),
			    new XAttribute ("MinorVersion", minorVersion),
			   	new XAttribute ("MembersFormat", membersFormat.ToString ()),
			    new XElement ("Font",
			    	new XAttribute ("Name", AttributeFigure.GetDefaultAttribute(FigureAttribute.FontFamily)),
			        new XAttribute ("Size", AttributeFigure.GetDefaultAttribute(FigureAttribute.FontSize))
			    )
			);

			foreach (IFigure figure in Figures) {
				XElement element;
				var tf = figure as TypeFigure;
				
				if (figure is ClassFigure)
					element = new XElement ("Class");
				else if (figure is StructFigure)
					element = new XElement ("Struct");
				else if (figure is EnumFigure)
					element = new XElement ("Enum");
				else if (figure is InterfaceFigure)
					element = new XElement ("Interface");
				else if (figure is DelegateFigure)
					element = new XElement ("Class");
				else if (figure is CommentFigure) {
					var comment = (CommentFigure) figure;
					
					element = new XElement ("Comment",
						new XAttribute ("CommentText", comment.Text)
					);
				} else 
					continue;
				
				if (tf != null) {
					element.Add (new XAttribute ("Name", ((TypeFigure) figure).Name.FullName));
						
					if (tf.IsCollapsed)
						element.Add (new XAttribute ("Collapsed", "true"));
				}
				
				if (figure is ClassFigure) {
					var cls = figure as ClassFigure;
					
					if (cls.HideInheritance)
						element.Add (new XAttribute ("HideInheritanceLine", "true"));
				}
				
				var position = new XElement ("Position",
						new XAttribute ("X", PixelsToInches (figure.DisplayBox.X)),
					    new XAttribute ("Y", PixelsToInches (figure.DisplayBox.Y)),
					    new XAttribute ("Width", PixelsToInches (figure.DisplayBox.Width))
				);
				
				if (figure is CommentFigure)
					position.Add (new XAttribute ("Height", PixelsToInches (figure.DisplayBox.Height)));
				
				element.Add (position);
				
				if (tf != null) {
					var clist = tf.Figures.OfType<CompartmentFigure>().Where (c => c.IsCollapsed);
										
					if (clist.Count () > 0) {
						foreach (var c in clist)
							Console.WriteLine (c.Name);
							
						var compartments = new XElement ("Compartments",
							from c in clist
						    select
							new XElement ("Compartment",
								new XAttribute ("Name", c.Name),
								new XAttribute ("Collapsed", "true") 
							)
						);		
						
						element.Add (compartments);
					}
					
					string name = null;
					var project = IdeApp.Workspace.GetProjectContainingFile (fileName);
					var dom = ProjectDomService.GetProjectDom (project);
					
					foreach (var pf in project.Files) {
						var parseDoc = ProjectDomService.GetParsedDocument (dom, pf.Name);
						
						if (!String.IsNullOrEmpty (name))
							break;
						
						if (parseDoc.CompilationUnit != null) {
							var exists = parseDoc.CompilationUnit.Types
								.Any (t => t.FullName == tf.Name.FullName);
							
							if (exists)
								name = pf.FilePath.FileName;
						}
					}
					
					var identifier = new XElement ("TypeIdentifier",
						new XElement ("HashCode", String.Format ("{0:X}", tf.GetHashCode ())),
					    new XElement ("FileName", name)
					);
					element.Add (identifier);
				}
				
				root.FirstNode.AddBeforeSelf (element);
			}

			root.Save (fileName);
		}
				
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

		public void AddToBuilder (IEnumerable<IType> types)
		{
			lock (checkList) {
				checkList.AddRange (types);
			}
			
			StartCheck ();
		}
		
		private void CheckTypes ()
		{
			IType[] typeList;
			int i = 0;
			
			lock (checkList) {
				typeList = checkList.ToArray ();
				checkList.Clear ();
			}
			
			
			foreach (IType type in typeList) {
				Console.WriteLine ("Checking {0}", type.FullName);
				if (HasTypeFigure (type.FullName))
					continue;
			
				lock (builderList)
					builderList.AddLast (new LinkedListNode<IType> (type));
				
				GLib.Idle.Add (FigureBuilder);				
			}
			
			diagramThread = null;
		}
		
		private bool FigureBuilder ()
		{	
			lock (builderList) {
				int max = Math.Min (builderList.Count, 10);
				
				for (int i = 0; i < max; builderList.RemoveFirst (), i++) {
					var type = builderList.First ();
					var figure = CreateFigure (type);
					
					if (figure == null)
						continue;
					
					Console.WriteLine ("Building {0}", type.FullName);
					
					OnCreated (new FigureEventArgs (figure, RectangleD.Empty));		
					
					var tf = figure as TypeFigure;
					if (tf == null)
						continue;
					
					foreach (IFigure fig in tf.Compartments)
						fig.AcceptVisitor (new GroupFormatVisitor (this, tf));
					
				}
				
				if (builderList.Count > 0)
					return true;
				
				return false;
			}
		}
		
		void StartCheck ()
		{
			if (diagramThread != null)
				return;
			
			var builder = new ThreadStart (CheckTypes);
			diagramThread = new Thread (builder) {
				Name = "Class Designer Figure Builder",
				IsBackground = true,
				Priority = ThreadPriority.Lowest,
			};
			
			diagramThread.Start ();
		}
		
		readonly int majorVersion;
		readonly int minorVersion;

		GroupingSetting groupSetting;		
		MembersFormat membersFormat;
		
		Dictionary <string,IFigure> typeCache;
		List<IType> checkList;
		LinkedList<IType> builderList;
		Thread diagramThread;
		#endregion

	}
}
