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
using MonoHotDraw.Visitor;

namespace MonoDevelop.ClassDesigner.Figures
{	
	public class CompartmentFigure : VStackFigure, ICollapsable
	{
		string _name;
		TextFigure compartmentName;
		VStackFigure membersStack;
		ToggleButtonHandle expandHandle;
			
		public CompartmentFigure (string name) : base ()
		{
			Spacing = 1;
			Alignment = VStackAlignment.Left;
			
			_name = name;
			compartmentName = new TextFigure (name);
			compartmentName.Padding = 0;
			compartmentName.FontSize = 10;
			compartmentName.FontColor = new Cairo.Color(0.3, 0.0, 0.0);			
			SetAttribute (FigureAttribute.Selectable, true);
			
			membersStack = new VStackFigure();
			membersStack.Spacing = 2;
			
			expandHandle = new ToggleButtonHandle (this, new AbsoluteLocator(-10, 7.5));
			expandHandle.Toggled += OnToggled;
			
			Add (compartmentName);
			Add (membersStack);
			Collapsed = false;
		}
		
		public override IEnumerable<IHandle> Handles {
			get { yield return expandHandle; }
		}
		
		public new void AddRange (IEnumerable<Figure> figures)
		{
			foreach (Figure figure in figures)
				membersStack.Add (figure);
		}
		
		public override RectangleD InvalidateDisplayBox {
			get {
				RectangleD rect = base.InvalidateDisplayBox;
				rect.Inflate (15, 0);
				return rect;
			}
		}
		
		public override bool Visible {
			get {
				return base.Visible && membersStack != null && membersStack.Figures.Any (f => f.Visible);
			}
			set {
				base.Visible = value;
			}
		}
		
		public string Name {
			get { return _name; }
		}
		
		#region ICollapsable implementation
		public bool Collapsed {
			get { return !expandHandle.Active; }
			set { expandHandle.Active = !value; }
		}
		
		void OnToggled (object o, ToggleEventArgs e)
		{
			membersStack.Visible = e.Active;
		}
		#endregion
	}
}
