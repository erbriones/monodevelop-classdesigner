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
		
		public override IFigure Container {
			get { return this; }
		}
		
		public sealed override IEnumerable <IFigure> Figures {
			get { return FigureCollection; }
		}

		public IEnumerable<IFigure> FiguresReversed {
 			get { return FigureCollection.Reverse<IFigure> (); }
 		}

		public override IEnumerable<IHandle> Handles {
			get {
				foreach (IFigure fig in FigureCollection)
					foreach (IHandle handle in fig.Handles)
						yield return handle;
			}
		}
		
		public void AddRange (IEnumerable<IFigure> figures)
		{
			foreach (IFigure fig in figures)
				Add (fig);
		}

		public sealed override void Add (IFigure figure)
		{
			if (FigureCollection.Contains (figure))
				return;
			
			FigureCollection.Add (figure);
			figure.FigureInvalidated += OnChildInvalidated;
			figure.Invalidate ();
			OnChildAdded (new FigureEventArgs (figure, figure.DisplayBox));
		}
		
		public void Clear ()	
		{
			RemoveRange (FigureCollection);
		}

		public sealed override void Remove (IFigure figure)
		{
			if (!FigureCollection.Contains (figure))
				return;
			
			FigureCollection.Remove (figure);
			figure.FigureInvalidated -= OnChildInvalidated;
			figure.Invalidate ();
			OnChildRemoved (new FigureEventArgs (figure, figure.DisplayBox));
		}
		
		public void RemoveRange(IEnumerable<IFigure> figures)
		{
			foreach (IFigure figure in figures)
				Remove (figure);
		}

		public void BringToFront (IFigure figure)
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

		public IFigure FindFigure (double x, double y)
		{
			foreach (IFigure figure in FiguresReversed)
				if (figure.ContainsPoint (x, y))
					return figure;
			
			return null;
		}

		public override bool Includes (IFigure figure)
		{
			if (FigureCollection.Any (f => f.Includes (figure) || base.Includes (figure)))
				return true;
			
			return false;
		}

		public void SendToBack (IFigure figure)
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
				
				foreach (IFigure figure in FigureCollection) {
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
				
				FigureCollection.ForEach (f => ((AbstractFigure) f).BasicMoveBy (dx, dy));
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
