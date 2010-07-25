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

using Gdk;
using System;
using Cairo;
using MonoHotDraw.Figures;
using MonoHotDraw.Handles;
using MonoHotDraw.Commands;

namespace MonoHotDraw.Tools
{
	// TODO: Should be this inside PolyLineFigure ???
	public class PolyLineFigureTool : FigureTool
	{
		public PolyLineFigureTool (IDrawingEditor editor, IFigure fig, ITool dt) : base (editor, fig, dt)
		{
		}
		
		#region Mouse Events
		public override void MouseDown (MouseEvent ev)
		{
			var view = ev.View;
			View = view;
			Gdk.EventType type = ev.GdkEvent.Type;
			
			SetAnchorCoords (ev.X, ev.Y);
			
			if (type != EventType.TwoButtonPress) {
				DefaultTool.MouseDown (ev);
				return;
			}
		
			// Split the line the mouse position
			var connection = (PolyLineFigure) Figure;
			connection.SplitSegment (ev.X, ev.Y);
			
			// Re-add the connector to the selection
			view.ClearSelection ();
			view.AddToSelection (Figure);
			
			// Change cursor for dragging
			handle = (PolyLineHandle) view.FindHandle (ev.X, ev.Y);
			((Gtk.Widget) view).GdkWindow.Cursor = handle.CreateCursor ();
			handle.InvokeStart (ev.X, ev.Y, ev.View);
		
			// add new undo activity
			CreateUndoActivity();
		}
		
		public override void MouseDrag (MouseEvent ev)
		{
			if (handle != null)
				handle.InvokeStep (ev.X, ev.Y, ev.View);
		}
		
		public override void MouseUp (MouseEvent ev)
		{
			if (handle == null)
				return;
		
			handle.InvokeEnd (ev.X, ev.Y, ev.View);
			UpdateUndoActivity ();
			PushUndoActivity ();
		}
		#endregion
		
		#region UndoActivity
		public class PolyLineFigureToolUndoActivity : AbstractUndoActivity
		{
			public PolyLineFigureToolUndoActivity(IDrawingView view) : base (view)
			{
				Undoable = true;
				Redoable = true;
			}
			
			public override bool Undo ()
			{
				if (!base.Undo ())
					return false;
				
				Figure.RemovePointAt (Index);
				return true;
			}
			
			public override bool Redo ()
			{
				if (!base.Redo ())
					return false;
				
				Figure.InsertPointAt (Index, NewPoint.X, NewPoint.Y);
				return true;
			}
			
			public PolyLineFigure Figure { get; set; }
			public int Index { get; set; }
			public PointD NewPoint { get; set; }
		}
		#endregion
		
		#region PolyLineFigureTool Members
		protected void CreateUndoActivity ()
		{
			var activity = new PolyLineFigureToolUndoActivity (View);
			activity.Figure = (PolyLineFigure) Figure;
			activity.Index = handle.Index;
			UndoActivity = activity;
		}
		
		protected void UpdateUndoActivity ()
		{
			var activity = (PolyLineFigureToolUndoActivity) UndoActivity;
			activity.NewPoint = activity.Figure.PointAt (activity.Index);
		}
		
		private PolyLineHandle handle;
		#endregion
	}
}
