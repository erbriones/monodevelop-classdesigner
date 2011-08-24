// MonoDevelop ClassDesigner
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//
// Copyright (C) 2009 Manuel Cerón
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Gtk;
using Gdk;
using Cairo;
using MonoHotDraw.Figures;
using MonoHotDraw.Handles;
using MonoHotDraw.Util;
using MonoHotDraw.Locators;
using MonoHotDraw.Visitor;

using MonoDevelop.ClassDesigner.Visitor;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;

namespace MonoDevelop.ClassDesigner.Figures
{
	public abstract class TypeFigure : VStackFigure, ICollapsable, ISerializableFigure
	{
		GroupingSetting grouping;
		List<MemberFigure> members;

		ToggleButtonHandle expandHandle;
		
		public static TypeFigure FromType(IType type)
		{
			if (type == null)
				return null;
			
			switch (type.ClassType) {
				case ClassType.Class:     return new ClassFigure (type);
				case ClassType.Delegate:  return new DelegateFigure (type);
				case ClassType.Enum:      return new EnumFigure (type);
				case ClassType.Interface: return new InterfaceFigure (type);
				case ClassType.Struct:    return new StructFigure (type);
				default: return null;
			}
		}

		public TypeFigure () : base ()
		{
			Spacing = 1.5;
			members = new List<MemberFigure> ();
			Header = new HeaderFigure ();
			MemberCompartments = new VStackFigure ();
			expandHandle = new ToggleButtonHandle (this, new AbsoluteLocator (10, 15));
			expandHandle.Toggled += OnToggled;
			SetAttribute (FigureAttribute.Draggable, true);
			SetAttribute (FigureAttribute.Selectable, true);
			grouping = GroupingSetting.Member;
			MembersFormat = MembersFormat.FullSignature;
			
			Add (Header);
			Add (MemberCompartments);
			Collapsed = true;
		}
		
		public TypeFigure (IType domType) : this ()
		{
			Rebuild(domType);
		}

		#region ISerializableFigure implementation
		public virtual XElement Serialize ()
		{
			var xml = new XElement ("Type",
				new XAttribute ("Name", PrettyFullName),
				this.SerializePosition (false),
				new XElement ("TypeIdentifier",
					new XElement ("HashCode", String.Format ("{0:X}", GetHashCode ())),
				    new XElement ("FileName", ContainerFilePath.FileName)
				)
			);
			
			if (Collapsed) {
				xml.Add (new XAttribute ("Collapsed", "true"));
			}
			
			// Get collapsed compartment info
			var clist = Figures.OfType<CompartmentFigure> ().Where (c => c.Collapsed );
			if (clist.Count() > 0) {
				xml.Add (new XElement ("Compartments",
					clist.Select(c => new XElement ("Compartment",
						new XAttribute ("Name", c.Name),
						new XAttribute ("Collapsed", "true")
					))
				));
			}
			
			return xml;
		}

		public virtual void Deserialize (XElement xml, ProjectDom dom)
		{
			var typeName = xml.Attribute ("Name");
			if (typeName == null) {
				throw new DeserializationException (xml.Name + " element with no \"Name\" attribute");
			}
			
			var domType = dom.GetType (DeserializeTypeName(typeName.Value));
			if (domType == null) {
				// TODO: Handle orphaned figures here..
				throw new NotImplementedException ();
			} else {
				Rebuild (domType);
				var members = xml.Element ("Members");
				if (members != null) {
					foreach (var memberElem in members.Elements ()) {
						var memberName = memberElem.Attribute ("Name");
						if (memberName == null) {
							throw new DeserializationException ("Member element with no name in " + domType.DecoratedFullName);
						}
						
						var member = domType.SearchMember (memberName.Value, true).SingleOrDefault ();
						if (member != null) {
							foreach (var c in Figures) {
								var memberFigure = c.Figures
									.OfType<MemberFigure> ()
									.Where (f => f.Name == member.Name)
									.SingleOrDefault ();
								
								if (memberFigure != null) {
									memberFigure.Visible = false;
								}
							}
						}
					}
			
					var compartmentsElem = xml.Element ("Compartments");
					if (compartmentsElem != null) {							
						foreach (var compartmentElem in compartmentsElem.Elements ("Compartment")) {
							var name = compartmentElem.Attribute ("Name");
							if (name == null)
								continue;
							
							var compartment = Figures
								.OfType<CompartmentFigure> ()
								.Where (c => c.Name == name.Value)
								.SingleOrDefault ();
							
							if (compartment == null)
								continue;
							
							var compartmentCollapsed = compartmentElem.Attribute ("Collapsed");
							compartment.Collapsed = compartmentCollapsed != null
									&& Boolean.Parse (compartmentCollapsed.Value);
						}
					}
				}
			}
			
			var collapsed = xml.Attribute ("Collapsed");
			Collapsed = collapsed != null && Boolean.Parse (collapsed.Value);
			
			var position = xml.Element ("Position");
			this.DeserializePosition (position);
		}
		
		public static string DeserializeTypeName (string name) {
			var i = name.IndexOf ('<');
			return (i == -1) ? name : name.Substring (0, i) + "`" + (name.Where (c => c == ',').Count () + 1);
		}
		#endregion

		protected VStackFigure MemberCompartments { get; private set; }
		
		public GroupingSetting Grouping {
			get { return grouping; }
			set {
				grouping = value;
				RebuildCompartments ();
			}
		}
		
		public bool HasHiddenMembers {
			get {
				return members.Any (m => !m.Visible);
			}
		}
		
		public MembersFormat MembersFormat { get; set; }

		public override IEnumerable<IHandle> Handles {
			get {
				foreach (IFigure fig in Figures)
					foreach (IHandle handle in fig.Handles)
						yield return handle;
				
				yield return expandHandle;
			}
		}
		
		public bool Collapsed {
			get { return !expandHandle.Active; }
			set { expandHandle.Active = !value; }
		}
		
		public IEnumerable<MemberFigure> Members {
			get { return members; }
		}
		
		#region Type Information
		public abstract ClassType ClassType { get; }
		public FilePath ContainerFilePath { get; private set; }
		public string DecoratedFullName { get; protected set; }
		public string Name { get; protected set; }
		public string Namespace { get; protected set; }
		
		public IEnumerable<string> TypeParameters { get; protected set; }
		public string TypeParametersString {
			get { return TypeParameters.Count () == 0 ? null : "<" + String.Join (",", TypeParameters) + ">"; }
		}
		
		public string PrettyFullName {
			get {
				var i = DecoratedFullName.IndexOf ('`');
				return i == -1 ? DecoratedFullName : DecoratedFullName.Substring (0, i) + TypeParametersString;
			}
		}
		
		public string PrettyName {
			get {
				return Name + TypeParametersString;
			}
		}
		#endregion
		
		#region Figure Drawing

		#endregion
		
		public override bool ContainsPoint (double x, double y)
		{
			return DisplayBox.Contains (x, y);
		}
		
		public virtual void Rebuild (IType domType)
		{
			if (domType == null || domType.ClassType != this.ClassType)
				throw new ArgumentException ();
			
			ContainerFilePath = domType.CompilationUnit == null ? null : domType.CompilationUnit.FileName;
			TypeParameters = domType.TypeParameters == null ? null : domType.TypeParameters.Select (tp => tp.Name);
			DecoratedFullName = domType.DecoratedFullName;
			Name = domType.Name;
			Namespace = domType.Namespace;
			
			RebuildHeader ();
			BuildMembers (domType);
			RebuildCompartments ();
		}
		
		public void ShowAll ()
		{
			foreach (var member in members) {
				member.Visible = true;
			}
		}
		
		protected override RectangleD BasicDisplayBox {
			get {
				RectangleD rect = base.BasicDisplayBox;
				rect.X -= 20;
				rect.Y -= 10;
				rect.Width += 30;
				rect.Height += 20;
				return rect;
			}
			set {
				value.X += 20;
				value.Y += 10;
				value.Width -= 30;
				value.Height -= 20;
				base.BasicDisplayBox = value;
			}
		}
		
		protected HeaderFigure Header { get; set; }

		protected override void BasicDraw (Cairo.Context context)
		{
			RectangleD rect = DisplayBox;
			
			CairoFigures.RoundedRectangle (context, rect, 6.25);
			
			DrawPattern (context);
			
			context.LineWidth = 1;
			context.Color = LineColor;
			context.Stroke ();
			
			base.BasicDraw (context);
		}
		
		protected override void BasicDrawSelected (Cairo.Context context)
		{
			RectangleD rect = DisplayBox;
			rect.OffsetDot5 ();
			CairoFigures.RoundedRectangle (context, rect, 6.25);
			
			DrawPattern (context);
			
			context.LineWidth = 3;
			context.Color = LineColor;
			context.Stroke ();
			
			base.BasicDraw (context);
		}

		protected void OnToggled (object o, ToggleEventArgs e)
		{
			MemberCompartments.Visible = e.Active;
		}
		
		protected virtual void RebuildCompartments ()
		{
			MemberCompartments.Clear ();
			var notNested = Members.Where (m => m.MemberInfo.MemberType != MemberType.Type);
			
			switch (Grouping) {
				case GroupingSetting.Alphabetical:
					var members = new CompartmentFigure (GettextCatalog.GetString ("Members"));
					members.AddRange (notNested.OrderBy (m => m.Name));
					MemberCompartments.Add (members);
					break;
				case GroupingSetting.Kind:
					var fields = new CompartmentFigure (GettextCatalog.GetString ("Fields"));
					fields.AddRange (notNested.Where (m => m.MemberInfo.MemberType == MemberType.Field));
					MemberCompartments.Add (fields);
				
					var properties = new CompartmentFigure (GettextCatalog.GetString ("Properties"));
					properties.AddRange (notNested.Where (m => m.MemberInfo.MemberType == MemberType.Property));
					MemberCompartments.Add (properties);
				
					var methods = new CompartmentFigure (GettextCatalog.GetString ("Methods"));
					methods.AddRange (notNested.Where (m => m.MemberInfo.MemberType == MemberType.Method));
					MemberCompartments.Add (methods);
				
					var events = new CompartmentFigure (GettextCatalog.GetString ("Events"));
					events.AddRange (notNested.Where (m => m.MemberInfo.MemberType == MemberType.Event));
					MemberCompartments.Add (events);
					break;
				case GroupingSetting.Member:
					var @public = new CompartmentFigure (GettextCatalog.GetString ("Public"));
					@public.AddRange (notNested.Where (m => m.MemberInfo.IsPublic));
					MemberCompartments.Add (@public);
				
					var @protected = new CompartmentFigure (GettextCatalog.GetString ("Protected"));
					@protected.AddRange (notNested.Where (m => m.MemberInfo.IsProtected));
					MemberCompartments.Add (@protected);
				
					var @protectedInternal = new CompartmentFigure (GettextCatalog.GetString ("Protected Internal"));
					@protectedInternal.AddRange (notNested.Where (m => m.MemberInfo.IsProtectedAndInternal));
					MemberCompartments.Add (@protectedInternal);
				
					var @internal = new CompartmentFigure (GettextCatalog.GetString ("Internal"));
					@internal.AddRange (notNested.Where (m => m.MemberInfo.IsInternal));
					MemberCompartments.Add (@internal);
				
					var @private = new CompartmentFigure (GettextCatalog.GetString ("Private"));
					@private.AddRange (notNested.Where (m => m.MemberInfo.IsPrivate));
					MemberCompartments.Add (@private);
					break;
			}
			
			var nested = new CompartmentFigure (GettextCatalog.GetString ("Nested Types"));
			nested.AddRange (
				Members
					.Where (m => m.MemberInfo.MemberType == MemberType.Type)
					.Select (m => {
						var dom = m.MemberInfo.SourceProjectDom;
						var type = dom.GetType (m.MemberInfo.FullName);
						return TypeFigure.FromType (type);
					})
			);
			MemberCompartments.Add (nested);
		}
		
		protected virtual void RebuildHeader ()
		{
			Header.Type = ClassType.ToString ();
			Header.Namespace = Namespace;
			Header.Name = PrettyName;
		}
		
		private void BuildMembers (IType domType)
		{
			members.Clear ();
			var visitor = new MemberFormatVisitor (MembersFormat);
			foreach (IMember member in domType.Members) {
				var icon = ImageService.GetPixbuf (member.StockIcon, IconSize.Menu);
				var figure = new MemberFigure (icon, member, false);
				figure.AcceptVisitor (visitor);
				
				members.Add (figure);
			}
		}

		private void DrawPattern (Cairo.Context context)
		{
			context.Save ();
			var pattern = new LinearGradient (DisplayBox.X, DisplayBox.Y, DisplayBox.X2, DisplayBox.Y2);
			pattern.AddColorStop (0, FillColor);
			context.Pattern = pattern;
			context.FillPreserve ();
			context.Restore ();
		}
	}
}
