// MonoHotDraw. Diagramming Framework
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//
// Copyright (C) 2006, 2007, 2008, 2009 MonoUML Team (http://www.monouml.org)
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
using Cairo;
using MonoHotDraw.Util;

namespace MonoHotDraw.Figures
{	
	public enum VStackAlignment {
		Center,
		Left,
		Right,
	}
	
	public class VStackFigure : StackFigure
	{	
		public VStackFigure () : base ()
		{
			Alignment = VStackAlignment.Left;
		}
		
		public VStackAlignment Alignment { get; set; }
		
		protected override double CalculateHeight ()
		{
			int count = FigureCollection.Count ();
			double height = 0.0;
			
			if (count == 0)
				return 0.0;
			
			foreach (IFigure fig in FigureCollection)
				height += fig.DisplayBox.Height;
			
			return height + Spacing * (count - 1);
		}
		
		protected override double CalculateWidth ()
		{
			double width = 0.0;
			
			if (FigureCollection.Count () == 0)
				return 0.0;
			
			
			foreach (IFigure fig in FigureCollection)
				width = Math.Max (width, fig.DisplayBox.Width);
			
			return width;
		}
		
		protected override void UpdateFiguresPosition ()
		{
			var stackHeight = 0.0;
			var point = new PointD (0, 0);
			
			foreach (IFigure figure in FigureCollection) {
				
				point.X = CalculateFigureX (figure);
				point.Y = Position.Y + stackHeight;
						
				double dx = point.X - figure.DisplayBox.X;
				double dy = point.Y - figure.DisplayBox.Y;
				
				((AbstractFigure) figure).BasicMoveBy (dx, dy);
				stackHeight += figure.DisplayBox.Height + Spacing;
			}
		}

		private double CalculateFigureX (IFigure figure)
		{
			switch (Alignment) {
			case VStackAlignment.Center:
				return Position.X + (Width - figure.DisplayBox.Width)/2;
			case VStackAlignment.Left:
				return Position.X;
			case VStackAlignment.Right:
				return Position.X + (Width - figure.DisplayBox.Width);
			default:
				return Position.X;
			}
		}
	}
}
