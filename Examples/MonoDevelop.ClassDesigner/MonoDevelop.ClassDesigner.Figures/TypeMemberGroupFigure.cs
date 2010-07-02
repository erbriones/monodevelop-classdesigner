// MonoDevelop ClassDesigner
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//  Evan Briones <erbriones@gmail.com>
//
// Copyright (C) 2009 Manuel Cerón
// Copyright (C) 2010 Evan Briones
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
using Gdk;
using MonoHotDraw.Figures;
using MonoHotDraw.Handles;
using MonoHotDraw.Locators;
using MonoHotDraw.Util;

namespace MonoDevelop.ClassDesigner.Figures {
	
	public class TypeMemberGroupFigure : VStackFigure
	{
		string _name;
		bool _collapsed;
		TextFigure compartmentName;
		VStackFigure membersStack;
		List<IMemberFigure> hidden;
		ToggleButtonHandle expandHandle;
			
		public TypeMemberGroupFigure (string name) : base ()
		{
			Spacing = 5;
			Alignment = VStackAlignment.Left;
			
			_name = name;
			hidden = new List<IMemberFigure> ();
			compartmentName = new TextFigure (name);
			compartmentName.Padding = 0;
			compartmentName.FontSize = 10;
			compartmentName.FontColor = new Cairo.Color(0.3, 0.0, 0.0);			
			
			Add (compartmentName);

			membersStack = new VStackFigure();
			membersStack.Spacing = 2;
			
			expandHandle = new ToggleButtonHandle(this, new AbsoluteLocator(-10, 7.5));
			expandHandle.Toggled += delegate(object sender, ToggleEventArgs e) {
				if (e.Active) {
					_collapsed = false;
					Add(membersStack);
				}
				else {
					_collapsed = true;
					Remove(membersStack);
				}
			};
			
			expandHandle.Active = true;
		}
		
		public override IEnumerable<IHandle> HandlesEnumerator {
			get {
				yield return expandHandle;
			}
		}
		
		public override RectangleD InvalidateDisplayBox {
			get {
				RectangleD rect = base.InvalidateDisplayBox;
				rect.Inflate(15, 0);
				return rect;
			}
		}
		
		public new IEnumerable<IMemberFigure> FiguresEnumerator {
			get {
				foreach (var f in membersStack.FiguresEnumerator)
					yield return (IMemberFigure) f;
				
				foreach (var f in hidden)
					yield return f;
			}
		}
				
		public string Name {
			get { return _name; }
		}
		
		public bool Collapsed {
			get { return _collapsed; }
			set {
				if (value)
					expandHandle.Active = false;
				else
					expandHandle.Active = true;
				
				_collapsed = value;
			}
		}
		
		public bool IsEmpty {
			get {
				return membersStack.FiguresEnumerator.ToFigures ().Count == 0;
			}
		}
				
		public void AddMembers (IEnumerable<IMemberFigure> members)
		{
			if (members == null)
				return;
			
			foreach (var member in members)
				AddMember (member);
		}
		
		public void AddMember(IMemberFigure member) {		
			if (member.Hidden)
				hidden.Add (member);
			else
				membersStack.Add ((IFigure) member);
		}
		
		public void Hide (IMemberFigure figure)
		{
			if (hidden.Contains (figure))
				return;
			
			figure.Hidden = true;
			membersStack.Remove ((IFigure) figure);
			hidden.Add (figure);
		}
		
		public void Show (IMemberFigure figure)
		{
			figure.Hidden = false;
			membersStack.Add ((IFigure) figure);
			
			if (hidden.Contains (figure))
				hidden.Remove (figure);
		}
		
		public override void Clear ()
		{
			membersStack.Clear ();
			hidden.Clear ();
		}
	}
}
