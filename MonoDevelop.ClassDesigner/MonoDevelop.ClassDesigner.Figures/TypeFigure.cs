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

using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Figures
{
	public abstract class TypeFigure : VStackFigure, ICollapsable, ISerializableFigure
	{
		VStackFigure memberCompartments;
		ArrayList compartments;
		Dictionary<string, IFigure> members;

		ToggleButtonHandle expandHandle;
		IType domType;
		
		public static TypeFigure FromType(IType type)
		{
			TypeFigure figure;
			
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
			compartments  = new ArrayList (12);
			members = new Dictionary<string, IFigure> ();
			Header = new HeaderFigure ();
			memberCompartments = new VStackFigure ();
			expandHandle = new ToggleButtonHandle (this, new AbsoluteLocator (10, 15));
			expandHandle.Toggled += OnToggled;
			
			Add (Header);
			Expand ();
		}
		
		public TypeFigure (IType domType) : this ()
		{
			DomType = domType;
		}

		#region ISerializableFigure implementation
		public virtual XElement Serialize ()
		{
			var xml = new XElement ("Type",
				new XAttribute ("Name", DomType.FullName),
				ClassDiagram.GetPositionData(this),
				new XElement ("TypeIdentifier",
					new XElement ("HashCode", String.Format ("{0:X}", GetHashCode ())),
				    new XElement ("FileName", DomType.CompilationUnit.FileName.FileName)
				)
			);
			
			if (IsCollapsed) {
				xml.Add (new XAttribute ("Collapsed", "true"));
			}
			
			// Get collapsed compartment info
			var clist = Figures.OfType<CompartmentFigure> ().Where (c => c.IsCollapsed );
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

		public virtual void Deserialize ()
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		public IEnumerable<IFigure> Compartments {
			get {
				foreach (object c in compartments)
					yield return (IFigure) c;
			}
		}
		
		public override RectangleD DisplayBox {
			get {
				RectangleD rect = base.DisplayBox;
				rect.X -= 20;
				rect.Y -= 10;
				rect.Width += 30;
				rect.Height += 20;
				return rect;
			}
			set { base.DisplayBox = value; }
		}

		public override IEnumerable<IHandle> Handles {
			get {
				foreach (IFigure fig in Figures)
					foreach (IHandle handle in fig.Handles)
						yield return handle;
				
				yield return expandHandle;
			}
		}
		
		public bool IsCollapsed {
			get { return !expandHandle.Active; }
		}
		
		public IDictionary<string, IFigure> Members {
			get { return members; }
		}

		public IType DomType {
			get { return domType; }
			set {	
				if (value == null || value.ClassType != this.ClassType)
					throw new ArgumentException ();
				
				domType = value;
				
				Clear ();
				
				Header.Name = domType.Name;
				Header.Namespace = domType.Namespace;
				Header.Type = domType.ClassType.ToString ();
				
				CreateCompartments ();
				BuildMembers ();
			}
		}
		
		#region Figure Drawing

		#endregion
		public void AddCompartment (CompartmentFigure compartment)
		{
			if (compartment.IsEmpty)
				return;
			
			memberCompartments.Add (compartment);
		}

		public new void Clear ()
		{
			foreach (object c in compartments)
				memberCompartments.Remove ((IFigure) c);
		}

		public void Collapse ()
		{
			if (!IsCollapsed)
				expandHandle.Active = false;
		}

		public override bool ContainsPoint (double x, double y)
		{
			return DisplayBox.Contains (x, y);
		}
		
		public void Expand ()
		{
			if (IsCollapsed)
				expandHandle.Active = true;
		}
			
		public void RemoveCompartment (CompartmentFigure compartment)
		{
			memberCompartments.Remove (compartment);
		}
		
		public void ShowAll ()
		{
			foreach (IFigure figure in members.
				
				
				Values) {
				var member = (MemberFigure) figure;
				member.Show ();
			}
		}
		
		protected HeaderFigure Header { get; set; }
		
		protected virtual ClassType ClassType {
			get { return ClassType.Unknown; }
		}


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
			if (e.Active)
				Add (memberCompartments);
			else
				Remove (memberCompartments);
		}
		
		private void BuildMembers ()
		{
			members.Clear ();
			foreach (IMember member in DomType.Members) {
				var icon = ImageService.GetPixbuf (member.StockIcon, IconSize.Menu);
				var figure = new MemberFigure (icon, member, false);
				
				members.Add (member.GetHashCode ().ToString (), figure);
			}
		}
		
		private void CreateCompartments ()
		{
			compartments.Clear ();
			
			// Default Group
			var parameters = new CompartmentFigure (GettextCatalog.GetString ("Parameters"));
			var fields = new CompartmentFigure (GettextCatalog.GetString ("Fields"));
			var properties = new CompartmentFigure (GettextCatalog.GetString ("Properties"));
			var methods = new CompartmentFigure (GettextCatalog.GetString ("Methods"));
			var events = new CompartmentFigure (GettextCatalog.GetString ("Events"));
			
			// Group Alphabetical
			var members = new CompartmentFigure (GettextCatalog.GetString ("Members"));
			
			// Group by Access
			var pub = new CompartmentFigure (GettextCatalog.GetString ("Public"));
			var priv = new CompartmentFigure (GettextCatalog.GetString ("Private"));
			var pro = new CompartmentFigure (GettextCatalog.GetString ("Protected"));
			var pro_intr = new CompartmentFigure (GettextCatalog.GetString ("Protected Internal"));
			var intr = new CompartmentFigure (GettextCatalog.GetString ("Internal"));
			
			// Other Groups
			var nestedTypes = new CompartmentFigure (GettextCatalog.GetString ("Nested Types", true));
			
			compartments.Add (parameters);
			compartments.Add (fields);
			compartments.Add (properties);
			compartments.Add (methods);
			compartments.Add (events);
			compartments.Add (members);
			compartments.Add (pub);
			compartments.Add (priv);
			compartments.Add (pro);
			compartments.Add (pro_intr);
			compartments.Add (intr);
			compartments.Add (nestedTypes);
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
