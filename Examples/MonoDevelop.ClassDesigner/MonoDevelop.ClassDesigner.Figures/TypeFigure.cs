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
using System.Collections.Generic;
using Gtk;
using Gdk;
using Cairo;
using MonoHotDraw.Figures;
using MonoHotDraw.Handles;
using MonoHotDraw.Util;
using MonoHotDraw.Locators;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Figures {

	public abstract class TypeFigure: VStackFigure, ICollapsable
	{
		public static MembersFormat format = MembersFormat.FullSignature;
		public static GroupingSetting grouping = GroupingSetting.Member;

		VStackFigure memberGroups;
		List<TypeMemberGroupFigure> compartments;
		ToggleButtonHandle expandHandle;
		IType _domtype;
		bool collapsed;
		Cairo.Color color;
		
		public TypeFigure () : base ()
		{
			Spacing = 10.0;
			Header = new TypeHeaderFigure ();
			compartments = new List<TypeMemberGroupFigure> ();
			memberGroups = new VStackFigure ();
			Add (Header);

			expandHandle = new ToggleButtonHandle (this, new AbsoluteLocator (10, 20));
			expandHandle.Toggled += delegate (object sender, ToggleEventArgs e) {
				if (e.Active) {
					Add(memberGroups);
				}
				else {
					Remove(memberGroups);
				}
			};
			
			expandHandle.Active = true;
		}
		
		public TypeFigure (IType domtype) : this ()
		{
			if (domtype == null || domtype.ClassType != this.ClassType)
				throw new ArgumentException();
			
			_domtype = domtype;
			collapsed = false;
	
			Header.Name = _domtype.Name;
			Header.Namespace = _domtype.Namespace;
			Header.Type = _domtype.ClassType.ToString ();
			
			CreateCompartments ();
		}
		
		void DrawPattern (Cairo.Context context)
		{
			context.Save ();
			Gradient pattern = new LinearGradient (DisplayBox.X, DisplayBox.Y, DisplayBox.X2, DisplayBox.Y2);
			pattern.AddColorStop (0, FigureColor);
			context.Pattern = pattern;
			context.FillPreserve();
			context.Restore ();
		}
		
		public override void BasicDrawSelected (Cairo.Context context)
		{
			RectangleD rect = DisplayBox;
			rect.OffsetDot5 ();
			CairoFigures.RoundedRectangle (context, rect, 6.25);
		
			DrawPattern (context);
		
			context.LineWidth = 3.0;
			context.Color = new Cairo.Color(0.0, 0.0, 0.0, 1.0);
			context.Stroke();
			
			base.BasicDraw (context);
		}

		protected override void BasicDraw (Cairo.Context context)
		{
			RectangleD rect = DisplayBox;
			
			CairoFigures.RoundedRectangle (context, rect, 6.25);
		
			DrawPattern (context);
			
			context.LineWidth = 1.0;
			context.Color = new Cairo.Color (0.0, 0.0, 0.0, 1.0);
			context.Stroke ();
			
			base.BasicDraw (context);
		}
		
		public override bool ContainsPoint (double x, double y) {
			return DisplayBox.Contains (x, y);
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
			set {
				base.DisplayBox = value;
			}
		}

		public override IEnumerable<IHandle> HandlesEnumerator {
			get {
				yield return expandHandle;
				foreach (IHandle handle in base.HandlesEnumerator)
					yield return handle;
			}
		}
		
		public IType Name {
			get { return _domtype; }
		}
		
		public bool Expanded {
			get { return expandHandle.Active; }
		}
		
		protected Cairo.Color FigureColor {
			get { return color; }
			set {
				color = value;
			}
		}

		protected TypeHeaderFigure Header { get; set; }

		protected void AddCompartment (TypeMemberGroupFigure newCompartment)
		{
			if (compartments.Contains (newCompartment))
				return;
			
			compartments.Add (newCompartment);
		}
		
		protected void AddMemberGroup (TypeMemberGroupFigure compartment)
		{
			if (compartment.IsEmpty)
				return;
			
			memberGroups.Add (compartment);
		}
		
		protected void RemoveCompartment (string name)
		{
			var comp = compartments
				.Where (c => c.Name == name)
				.SingleOrDefault ();
			
			compartments.Remove (comp);
		}
		
		protected void RemoveMemberGroup (TypeMemberGroupFigure compartment)
		{
			memberGroups.Remove (compartment);
		}

		public virtual void Update ()
		{
			var members = new List<TypeMemberFigure> ();
			
			memberGroups.Clear ();
			
			compartments.ForEach (c => members.AddRange (c.FiguresEnumerator));
			compartments.ForEach (c => c.Clear ());
			
			if (members.Count () == 0) {			
				foreach (var member in _domtype.Members) {
					var icon = ImageService.GetPixbuf (member.StockIcon, IconSize.Menu);
					members.Add (new TypeMemberFigure (icon, member, false));
				}
			}
			
			if (grouping == GroupingSetting.Alphabetical) {
				// Alphabetical compartment
				var compartment = compartments
					.Where (c => c.Name == "Members")
					.SingleOrDefault ();
	
				compartment.AddMembers (members.OrderBy (m => m.Name));

				AddMemberGroup (compartment);		
			} else if (grouping == GroupingSetting.Access) {
				// Public compartment
				var compartment = compartments
					.Where (c => c.Name == "Public")
					.SingleOrDefault ();
					
				if (compartment != null) {
					compartment.AddMembers (members.Where (m => ((IMember) m.MemberInfo).IsPublic));
					AddMemberGroup (compartment);
				}
				
				// Private compartment
				compartment = compartments
					.Where (c => c.Name == "Private")
					.SingleOrDefault ();
				
				if (compartment != null) {
					compartment.AddMembers (members.Where (m => ((IMember) m.MemberInfo).IsPrivate));
					AddMemberGroup (compartment);
				}
				
				// Protected compartment
				compartment = compartments
					.Where (c => c.Name == "Protected")
					.SingleOrDefault ();
				
				if (compartment != null) {
					compartment.AddMembers (members.Where (m => ((IMember) m.MemberInfo).IsProtected));
					AddMemberGroup (compartment);
				}
				
				// Internal compartment
				compartment = compartments
					.Where (c => c.Name == "Internal")
					.SingleOrDefault ();
				
				if (compartment != null) {
					compartment.AddMembers (members.Where (m => ((IMember) m.MemberInfo).IsInternal));
					AddMemberGroup (compartment);
				}
				
				// ProtectedInternal compartment
				compartment = compartments
					.Where (c => c.Name == "Protected Internal")
					.SingleOrDefault ();
				
				if (compartment != null) {
					compartment.AddMembers (members.Where (m => ((IMember) m.MemberInfo).IsProtectedAndInternal));
					AddMemberGroup (compartment);
				}
				
			} else {
				// Fields compartment
				var compartment = compartments
					.Where (c => c.Name == "Fields")
					.SingleOrDefault ();
				
				if (compartment != null) {
					compartment.AddMembers (members.Where (m => m.MemberInfo.MemberType == MemberType.Field));
					AddMemberGroup (compartment);
				}
				
				// Properties compartment
				compartment = compartments
					.Where (c => c.Name == "Properties")
					.SingleOrDefault ();
				
				if (compartment != null) {
					compartment.AddMembers (members.Where (m => m.MemberInfo.MemberType == MemberType.Property));
					AddMemberGroup (compartment);
				}
				
				// Methods compartment
				compartment = compartments
					.Where (c => c.Name == "Methods")
					.SingleOrDefault ();
				
				if (compartment != null) {
					compartment.AddMembers (members.Where (m => m.MemberInfo.MemberType == MemberType.Method));
					AddMemberGroup (compartment);
				}
				// Events compartment
				compartment = compartments
					.Where (c => c.Name == "Events")
					.SingleOrDefault ();
				
				if (compartment != null) {
					compartment.AddMembers (members.Where (m => m.MemberInfo.MemberType == MemberType.Event));
					AddMemberGroup (compartment);
				}
	
				// NestedTypes compartment
				compartment = compartments
					.Where (c => c.Name == "Nested Types")
					.SingleOrDefault ();
				
				if (compartment != null) {
					compartment.AddMembers (members.Where (m => m.MemberInfo.MemberType == MemberType.Type));
					AddMemberGroup (compartment);
				}
			}
		}
				
		protected virtual void CreateCompartments ()
		{
			// Default Group
			var fields = new TypeMemberGroupFigure (GettextCatalog.GetString ("Fields"));
			var properties = new TypeMemberGroupFigure (GettextCatalog.GetString ("Properties"));
			var methods = new TypeMemberGroupFigure  (GettextCatalog.GetString ("Methods"));
			var events = new TypeMemberGroupFigure (GettextCatalog.GetString ("Events"));
			
			// Group Alphabetical
			var members = new TypeMemberGroupFigure (GettextCatalog.GetString ("Members"));
			
			// Group by Access
			var pub = new TypeMemberGroupFigure (GettextCatalog.GetString ("Public"));
			var priv = new TypeMemberGroupFigure (GettextCatalog.GetString ("Private"));
			var pro = new TypeMemberGroupFigure (GettextCatalog.GetString ("Protected"));
			var pro_intr = new TypeMemberGroupFigure (GettextCatalog.GetString ("Protected Internal"));
			var intr = new TypeMemberGroupFigure (GettextCatalog.GetString ("Internal"));
					
			// Other Groups
			var nestedTypes = new TypeMemberGroupFigure (GettextCatalog.GetString ("Nested Types", true));
			
			AddCompartment (fields);
			AddCompartment (properties);
			AddCompartment (methods);
			AddCompartment (events);
			AddCompartment (members);
			AddCompartment (pub);
			AddCompartment (priv);
			AddCompartment (pro);
			AddCompartment (pro_intr);
			AddCompartment (intr);
			AddCompartment (nestedTypes);
		}
		
		protected virtual ClassType ClassType {
			get {
				return ClassType.Unknown;
			}
		}
		
		public IEnumerable<TypeMemberGroupFigure> Compartments {
			get { return compartments; }
		}
		
		public bool Collapsed {
			get { return collapsed;}
			set {
				if (value)
					expandHandle.Active = false;
				else
					expandHandle.Active = true;

				collapsed = value;
			}
		}		
	}
}
