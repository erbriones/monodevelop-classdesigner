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

// # define DEBUG_SHOW_FPS
// # define DEBUG_SHOW_VISIBLE_AREA

using Cairo;
using Gdk;
using Gtk;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using MonoHotDraw.Commands;
using MonoHotDraw.Figures;
using MonoHotDraw.Handles;
using MonoHotDraw.Util;
using MonoHotDraw.Visitor;

namespace MonoHotDraw
{
	public class StandardDrawingView : ContainerCanvas, IDrawingView
	{
		public event EventHandler VisibleAreaChanged;
		
		public StandardDrawingView (IDrawingEditor editor) : base ()
		{
			Editor = editor;
			Drawing = new StandardDrawing ();
			ScaleRange = new ScaleRange (5, 0.25, 0.1);	
			Scale = 0.75;
			
			selection = new FigureCollection ();
			DebugCreateTimer ();
		}

		protected StandardDrawingView (IntPtr raw) : base (raw)
		{
		}
		
		#region View Api
		public IDrawing Drawing {
			set {
				if (value == _drawing)
					return;

				if (_drawing != null) {
					_drawing.DrawingInvalidated -= OnDrawingInvalidated;
					_drawing.SizeAllocated -= OnDrawingSizeAllocated;
				}

				_drawing = value;
				_drawing.DrawingInvalidated += OnDrawingInvalidated;
				_drawing.SizeAllocated += OnDrawingSizeAllocated;
			}
			get { return  _drawing; }
		}

		public IDrawingEditor Editor { get; set; }
		
		public double Scale {
			get { return _scale; }
			set { 
				if (value > ScaleRange.Maximum)
					_scale = ScaleRange.Maximum;
				else if (value < ScaleRange.Minimum)
					_scale = ScaleRange.Minimum;
				else
					_scale = value;
				
				QueueDraw ();
			}
		}
		
		public ScaleRange ScaleRange {
			get { 
				if (_range == null)
					_range = new ScaleRange (5, 0.25, 0.1);
				
				return _range;
			} set { _range = value; }
		}
		
		
		public RectangleD VisibleArea {
			get {
				return new RectangleD {
					X = Hadjustment.Value,
					Y = Vadjustment.Value,
					Width = Allocation.Width / Scale,
					Height = Allocation.Height / Scale
				};
			}
		}
		#endregion
		
		#region Figure and Handle Members

		public void Add (Figure figure)
		{
			Drawing.Add (figure);
		}
		
		public void AddRange (IEnumerable<Figure> figures)
		{
			foreach (Figure figure in figures)
				Drawing.Add (figure);
		}
		
		
		public IHandle FindHandle (double x, double y)
		{
			foreach (IHandle handle in SelectionHandles)
				if (handle.ContainsPoint (x, y))
					return handle;

			return null;
		}

		public FigureCollection InsertFigures (FigureCollection figures, double dx, double dy, bool check)
		{
			var visitor = new InsertIntoDrawingVisitor (Drawing);
			
			foreach (Figure figure in figures) {	
				figure.MoveBy (dx, dy);
				visitor.VisitFigure (figure);
			}
			
			AddToSelection (visitor.AddedFigures);
		
			return visitor.AddedFigures;
		}

		
		public void Remove (Figure figure)
		{
			Drawing.Remove (figure);
		}
		
		public void RemoveRange (IEnumerable<Figure> figures)
		{
			foreach (Figure figure in figures)
				Drawing.Remove (figure);
		}
		
		public PointD ViewToDrawing (double x, double y)
		{
			return new PointD {
				X = (x/Scale + VisibleArea.X),
				Y = (y/Scale + VisibleArea.Y)
			};
		}
		
		public PointD DrawingToView (double x, double y)
		{
			return new PointD {
					X = (x - VisibleArea.X) * Scale,
					Y = (y - VisibleArea.Y) * Scale
			};
		}

		
		public void ScrollToMakeVisible (PointD point)
		{
			RectangleD visible = VisibleArea;
			
			if (visible.Contains (point.X, point.Y))
				return;
			
			Hadjustment.Lower = Math.Min (Hadjustment.Lower, point.X);
			Hadjustment.Upper = Math.Max (Hadjustment.Upper, point.X);
			Hadjustment.Change ();
			
			Vadjustment.Lower = Math.Min (Vadjustment.Lower, point.Y);			
			Vadjustment.Upper = Math.Max (Vadjustment.Upper, point.Y);
			Vadjustment.Change ();
			
			if (point.X < visible.X)
				Hadjustment.Value = Math.Round (point.X, 0);
			else if (point.X > visible.X2 )
				Hadjustment.Value = Math.Round (point.X - visible.Width, 0);
			
			if (point.Y < visible.Y)
				Vadjustment.Value = Math.Round (point.Y, 0);
			else if (point.Y > visible.Y2)
				Vadjustment.Value = Math.Round (point.Y - visible.Height, 0);
		
		}
		
		public void ScrollToMakeVisible (RectangleD rect)
		{
			RectangleD visible = VisibleArea;
			
			if (visible.Contains (rect))
				return;
			
			Hadjustment.Lower = Math.Min (Hadjustment.Lower, rect.X);			
			Hadjustment.Upper = Math.Max (Hadjustment.Upper, rect.X2);
			Hadjustment.Change ();
			
			Vadjustment.Lower = Math.Min (Vadjustment.Lower, rect.Y);			
			Vadjustment.Upper = Math.Max (Vadjustment.Upper, rect.Y2);
			Vadjustment.Change ();
			
			if (rect.X < visible.X)
				Hadjustment.Value = Math.Round (rect.X, 0);

			if (rect.X2 > visible.X2 )
				Hadjustment.Value = Math.Round (rect.X2 - visible.Width, 0);
			
			if (rect.Y < visible.Y)
				Vadjustment.Value = Math.Round (rect.Y, 0);
			
			if (rect.Y2 > visible.Y2)
				Vadjustment.Value = Math.Round (rect.Y2 - visible.Height, 0);
		}
		#endregion
		
		#region Selection
		public int SelectionCount {
			get { return selection.Count; }
		}
		
		public IEnumerable<Figure> SelectionEnumerator {
			get { return selection; }
		}
		
		protected IEnumerable <IHandle> SelectionHandles {
			get {
				foreach (Figure figure in SelectionEnumerator) {
					foreach (IHandle handle in figure.Handles) {
						yield return handle;
					}
				}
			}
		}	

		public void AddToSelection (Figure figure)
		{
			if (!IsFigureSelected (figure) && Drawing.Includes (figure)) {
				selection.Add (figure);
				figure.SetAttribute (FigureAttribute.Selected, true);
				figure.Invalidate ();
			}
		}
		
		public void AddToSelection (FigureCollection collection)
		{
			foreach (Figure figure in collection)
				AddToSelection (figure);
		}

		public void ClearSelection ()
		{
			foreach (Figure figure in selection) {
				figure.SetAttribute (FigureAttribute.Selected, false);
				figure.Invalidate ();
			}
			selection.Clear ();

		}
		
		public bool IsFigureSelected (Figure figure)
		{
			return selection.Contains (figure);
		}
		
		public void RemoveFromSelection (Figure figure)
		{
			selection.Remove (figure);
			figure.SetAttribute (FigureAttribute.Selected, false);
			figure.Invalidate ();
		}

		public void ToggleSelection (Figure figure)
		{
			if (IsFigureSelected (figure))
				RemoveFromSelection (figure);
			else
				AddToSelection (figure);
			
		}
		#endregion
		
		#region Drag Events
		public static TargetEntry [] Targets {
			get {
				if (targets == null) {
					targets = new TargetList ();
					targets.AddUriTargets(1);
				}
				
				return (TargetEntry []) targets;
			}
		}

		protected override bool OnDragMotion (DragContext context, int x, int y, uint time_)
		{	
			return base.OnDragMotion (context, x, y, time_);
		}
		#endregion
		
		#region Drawing 
		protected override bool OnExposeEvent (EventExpose ev)
		{
			using (Cairo.Context context = CairoHelper.Create (ev.Window)) {
				context.Save();
				CairoHelper.Region (context, ev.Region);
				context.Clip ();
				
				PointD translation = DrawingToView (0.0, 0.0);
				context.Translate (translation.X, translation.Y);
				context.Scale (Scale, Scale);
				
				var drawVisitor = new DrawInRegionVisitor (ev.Region, context, this);

				foreach (Figure figure in Drawing.Figures)
					figure.AcceptVisitor (drawVisitor);
				
				context.ResetClip ();
				context.Restore ();
			}
			
			DebugUpdateFrame ();
			return base.OnExposeEvent (ev);
		}
		
		protected void OnDrawingInvalidated (object sender, DrawingEventArgs args)
		{
			RectangleD r = args.Rectangle;
			PointD p = DrawingToView (r.X, r.Y);
			r.X = p.X;
			r.Y = p.Y;
			r.Width = r.Width * Scale;
			r.Height = r.Height * Scale;
						
			QueueDrawArea ((int) r.X, (int) r.Y, (int) r.Width, (int) r.Height);
		}
				
		protected void OnDrawingSizeAllocated (object sender, DrawingEventArgs args)
		{
			UpdateAdjustments ();
			QueueDraw ();
		}
		
		protected void OnVisibleAreaChanged ()
		{
			var handler = VisibleAreaChanged;
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
		#endregion
				
		#region Mouse Events
		protected override bool OnMotionNotifyEvent (EventMotion gdk_event)
		{
			PointD point = ViewToDrawing(gdk_event.X, gdk_event.Y);
			var ev = new MouseEvent (this, gdk_event, point);

			if (_drag) {
				// TODO: Move this to a Tool
				ScrollToMakeVisible (point); 
				Editor.Tool.MouseDrag (ev);
			} else
				Editor.Tool.MouseMove (ev);

			return base.OnMotionNotifyEvent(gdk_event);
		}
		
		protected override bool OnButtonReleaseEvent (EventButton gdk_event)
		{
			Drawing.RecalculateDisplayBox ();
			PointD point = ViewToDrawing (gdk_event.X, gdk_event.Y);
			var ev = new MouseEvent (this, gdk_event, point);
			
			Editor.Tool.MouseUp (ev);
			_drag = false;
			
			return base.OnButtonReleaseEvent(gdk_event);
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton gdk_event) 
		{
			base.IsFocus = true;
			
			PointD point = ViewToDrawing (gdk_event.X, gdk_event.Y);
			var ev = new MouseEvent (this, gdk_event, point);

			Editor.Tool.MouseDown (ev);
			_drag = true;

			return base.OnButtonPressEvent(gdk_event);
		}

		protected override bool OnScrollEvent (EventScroll e)
		{
			if (e.Device.Source == InputSource.Mouse) {
				if (e.Direction == ScrollDirection.Up)
					Scale = Scale + _range.Step;
				else if (e.Direction == ScrollDirection.Down)
					Scale = Scale - _range.Step;
			}
			
			return base.OnScrollEvent (e);
		}
		#endregion
		
		#region Key Events
		protected override bool OnKeyPressEvent (Gdk.EventKey ev)
		{
			Editor.Tool.KeyDown (new KeyEvent (this, ev));
			return base.OnKeyPressEvent (ev);
		}
		
		protected override bool OnKeyReleaseEvent (Gdk.EventKey ev)
		{
			Editor.Tool.KeyUp (new KeyEvent (this, ev));
			return base.OnKeyReleaseEvent (ev);
		}
		#endregion

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			
			UpdateAdjustments ();
			OnVisibleAreaChanged ();
		}

		protected override void OnAdjustmentValueChanged (object sender, EventArgs args)
		{
			QueueDraw ();
			OnVisibleAreaChanged ();
		}		

		private void UpdateAdjustments ()
		{
			RectangleD drawing_box = Drawing.DisplayBox;
			drawing_box.Add (VisibleArea);
			
			Hadjustment.PageSize = Allocation.Width;
			Hadjustment.PageIncrement = Allocation.Width * 0.9;
			Hadjustment.StepIncrement = 1.0;
			Hadjustment.Lower = drawing_box.X;
			Hadjustment.Upper = drawing_box.X2;
			Hadjustment.Change ();
			
			Vadjustment.PageSize = Allocation.Height;
			Vadjustment.PageIncrement = Allocation.Height * 0.9;
			Vadjustment.StepIncrement = 1.0;
			Vadjustment.Lower = drawing_box.Y;
			Vadjustment.Upper = drawing_box.Y2;
			Vadjustment.Change ();
		}
	
		[ConditionalAttribute ("DEBUG_SHOW_FPS")]
		private void DebugCreateTimer ()
		{
			GLib.Timeout.Add (1000, delegate() {
				System.Console.WriteLine ("FPS: {0}", _frameCount.ToString());
				_frameCount = 0;
				return true;
			});
		}

		[ConditionalAttribute ("DEBUG_SHOW_FPS")]
		private void DebugUpdateFrame ()
		{
			_frameCount ++;
		}
		
		static TargetList targets;
		bool _drag;
		IDrawing _drawing;
		FigureCollection selection;
		ScaleRange _range;
		double _scale = 1.0;
		
		// used for debug purposes
		private int _frameCount = 0;
	}
}








