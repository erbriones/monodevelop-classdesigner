// 
// DrawFigureVisitor.cs
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

using MonoHotDraw.Figures;
using MonoHotDraw.Handles;
using MonoHotDraw.Util;

namespace MonoHotDraw.Visitor
{
	public class DrawInRegionVisitor : IFigureVisitor
	{		
		Cairo.Context context;
		Gdk.Region region;
		IDrawingView view;
		List<Figure> figures;
		
		public DrawInRegionVisitor (Gdk.Region region, Cairo.Context context, IDrawingView view)
		{
			this.context = context;
			this.region = region;
			this.view = view;
			this.figures = new List<Figure> ();
		}
		
		public IEnumerable<Figure> AffectedFigures { 
			get { return figures; }
		}
		
		#region IFigureVisitor implementation		
		public void VisitFigure (Figure figure)
		{
			var point = view.DrawingToView (figure.DisplayBox.X, figure.DisplayBox.Y);
			var rect = new RectangleD (point.X, point.Y);
			rect.Width = figure.DisplayBox.Width;
			rect.Height = figure.DisplayBox.Height;
			
			if (!GdkCairoHelper.RectangleInsideGdkRegion (rect, region))
				return;
			
			if (view.SelectionEnumerator.Contains (figure))
				figure.DrawSelected (context);
			else
				figure.Draw (context);
			
			figures.Add (figure);
		}
		
		public void VisitHandle (IHandle handle)
		{
			if (view.SelectionEnumerator.Contains (handle.Owner)) {
				handle.Draw (context, view);

				foreach (IHandle childHandles in handle.Owner.Handles)
					childHandles.Draw (context, view);
			}
		}
		#endregion
	}
}