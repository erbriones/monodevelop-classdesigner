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
using Gdk;
using Gtk;
using System;
using Pango;
using MonoHotDraw.Figures;
using MonoHotDraw.Util;

namespace MonoHotDraw.Tools
{
	public class MultiLineTextTool : TextTool
	{
		public MultiLineTextTool (IDrawingEditor editor, MultiLineTextFigure fig, ITool dt) : base (editor, fig, dt)
		{	
			textview = new Gtk.TextView ();
			textview.Buffer.Changed += new System.EventHandler (ChangedHandler);
			textview.ModifyFont (fig.PangoLayout.FontDescription.Copy ());
			textview.RightMargin = 5;
			textview.Justification = ConvertJustificaton ();
		}
		
		#region Tool Activation
		public override void Deactivate ()
		{
			if (!showingWidget) {
				View.RemoveWidget (textview);
				UpdateUndoActivity ();
				PushUndoActivity ();
			}
			
			base.Deactivate ();
		}
		#endregion
		
		#region Mouse Events
		public override void MouseDown (MouseEvent ev)
		{
			var view = ev.View;
			SetAnchorCoords (ev.X, ev.Y);
			View = view;
			
			Gdk.EventType type = ev.GdkEvent.Type;
			
			if (type == EventType.TwoButtonPress) {
				CreateUndoActivity ();
				showingWidget = true;
				textview.Buffer.Text = ((MultiLineTextFigure) Figure).Text;
				
				View.AddWidget (textview, 0, 0);
				CalculateTextViewSize ();
				
				textview.Show ();
				textview.GrabFocus ();
				
				//selects all
				textview.Buffer.SelectRange (textview.Buffer.StartIter, textview.Buffer.EndIter);
				
				return;
			}
			
			DefaultTool.MouseDown (ev);
		}
		#endregion
		
		#region MultiLineTextTool Members
		private Gtk.Justification ConvertJustificaton ()
		{
			Pango.Alignment alignment = ((MultiLineTextFigure) Figure).FontAlignment;
			
			switch (alignment) {
				case Pango.Alignment.Center: 
					return Gtk.Justification.Center;
				case Pango.Alignment.Left: 
					return Gtk.Justification.Left;
				case Pango.Alignment.Right: 
					return Gtk.Justification.Right;
				default: 
					return Gtk.Justification.Left;
			}
		}

		private void ChangedHandler (object sender, EventArgs args)
		{
			((MultiLineTextFigure) Figure).Text = textview.Buffer.Text;
			CalculateTextViewSize ();
		}
		
		private void CalculateTextViewSize ()
		{
			var padding = (int) ((MultiLineTextFigure) Figure).Padding;
			RectangleD r = Figure.DisplayBox;
			r.Inflate (-padding, -padding);
			
			// Drawing Coordinates must be translated to View's coordinates in order to 
			// Correctly put the widget in the DrawingView
			PointD point = View.DrawingToView (r.X, r.Y);

			var x = (int) point.X;
			var y = (int) point.Y;
			var w = (int) Math.Max (r.Width, 10.0) + textview.RightMargin * 2;
			var h = (int) Math.Max (r.Height, 10.0);
			
			textview.SetSizeRequest (w, h);
			View.MoveWidget (textview, x, y);
		}

		private Gtk.TextView textview;
		#endregion
	}
}
