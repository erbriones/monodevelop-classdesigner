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

using Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using MonoHotDraw.Handles;
using MonoHotDraw.Tools;
using MonoHotDraw.Util;

namespace MonoHotDraw.Figures
{
	[Serializable]
	public abstract class CompositeFigure : AttributeFigure
	{
		protected CompositeFigure ()
		{
			FigureCollection = new FigureCollection ();
		}

		public event FigureEventHandler ChildAdded;
		public event FigureEventHandler ChildRemoved;
		
		public override Figure Container {
			get { return this; }
		}
		
		public sealed override IEnumerable <Figure> Figures {
			get { return FigureCollection; }
		}

		public override IEnumerable<IHandle> Handles {
			get {
				foreach (Figure fig in FigureCollection)
					foreach (IHandle handle in fig.Handles)
						yield return handle;
			}
		}
		
		public void AddRange (IEnumerable<Figure> figures)
		{
			foreach (Figure fig in figures)
				Add (fig);
		}

		public sealed override void Add (Figure figure)
		{
			if (FigureCollection.Contains (figure))
				return;
			
			if (figure.Parent != null) {
				throw new Exception ("Can't add a figure which already has a parent");
			}
			
			FigureCollection.Add (figure);
			figure.Parent = this;
			figure.FigureInvalidated += OnChildInvalidated;
			figure.Invalidate ();
			OnChildAdded (new FigureEventArgs (figure, figure.DisplayBox));
		}
		
		public void Clear ()	
		{
			var tmp = new List<Figure> (FigureCollection);
			RemoveRange (tmp);
		}

		public sealed override void Remove (Figure figure)
		{
			if (!FigureCollection.Contains (figure))
				return;
			
			figure.Parent = null;
			FigureCollection.Remove (figure);
			figure.FigureInvalidated -= OnChildInvalidated;
			figure.Invalidate ();
			OnChildRemoved (new FigureEventArgs (figure, figure.DisplayBox));
		}
		
		public void RemoveRange(IEnumerable<Figure> figures)
		{
			foreach (Figure figure in figures)
				Remove (figure);
		}

		public void BringToFront (Figure figure)
		{
			if (!Includes (figure))
				return;
			
			FigureCollection.Remove (figure);
			FigureCollection.Add (figure);
			figure.Invalidate ();
		}

		public override bool ContainsPoint (double x, double y)
		{
			return FigureCollection.Any (f => f.ContainsPoint (x, y));
		}

		public override ITool CreateFigureTool (IDrawingEditor editor, ITool dt)
		{
			return new CompositeFigureTool (editor, this, dt);
		}

		public Figure FindFigure (double x, double y)
		{
			return Figures.LastOrDefault (f => f.ContainsPoint (x, y));
		}

		public override bool Includes (Figure figure)
		{
			if (FigureCollection.Any (f => f.Includes (figure) || base.Includes (figure)))
				return true;
			
			return false;
		}
		
		public override Figure SelectableAt (double x, double y)
		{
			if (ContainsPoint (x, y)) {
				var sub = FindFigure (x, y);
				if (sub != null) {
					sub = sub.SelectableAt (x, y);
					if (sub != null) {
						return sub;
					}
				}
				
				var selectable = GetAttribute (FigureAttribute.Selectable);
				if (selectable != null && (bool) selectable) {
					return this;
				}
			}
			return null;
		}

		public void SendToBack (Figure figure)
		{
			if (!Includes (figure))
				return;
			
			FigureCollection.Remove (figure);
			FigureCollection.Insert (0, figure);
			figure.Invalidate ();
		}		

		protected override RectangleD BasicDisplayBox {
			get { 
				var rectangle = new RectangleD (0.0, 0.0);
				var first_flag = true;
				
				foreach (Figure figure in FigureCollection) {
					if (first_flag) {
						rectangle = figure.DisplayBox;
						first_flag = false;
					} else {
						rectangle.Add (figure.DisplayBox);
					}
				}
			
				return rectangle;
			} set {
				RectangleD r = DisplayBox;
				double dx = value.X - r.X;
				double dy = value.Y - r.Y;
				
				FigureCollection.ForEach (f => ((Figure) f).InternalMoveBy (dx, dy));
			}
		}

		protected FigureCollection FigureCollection { get; set; }

		protected override void BasicDraw (Context context)
		{
			FigureCollection.ForEach (f => f.Draw (context));
		}
		
		protected override void BasicDrawSelected (Context context)
		{
			FigureCollection.ForEach (f => f.DrawSelected (context));
		}

		protected virtual void OnChildInvalidated (object sender, FigureEventArgs args)
		{
			OnFigureInvalidated (new FigureEventArgs (this, args.Rectangle));
		}
		
		protected virtual void OnChildAdded (FigureEventArgs e)
		{
			var handler = ChildAdded;
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnChildRemoved (FigureEventArgs e)
		{
			var handler = ChildRemoved;
			if (handler != null)
				handler (this, e);
		}
	}
}
