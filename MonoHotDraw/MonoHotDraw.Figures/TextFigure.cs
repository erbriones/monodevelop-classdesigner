// MonoHotDraw. Diagramming Framework
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//	Mario Carrión <mario@monouml.org>
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
using Gtk;
using Pango;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MonoHotDraw.Tools;
using MonoHotDraw.Util;

namespace MonoHotDraw.Figures
{
	[Serializable]
	public class TextFigure : AttributeFigure
	{
		public TextFigure (string text) : base ()
		{
			
			TextEditable  = true;
			Padding       = 2.0;
			FontColor     = (Cairo.Color) AttributeFigure.GetDefaultAttribute (FigureAttribute.FontColor);
			FontAlignment = (Pango.Alignment) AttributeFigure.GetDefaultAttribute (FigureAttribute.FontAlignment);
			FontFamily    = (string) AttributeFigure.GetDefaultAttribute (FigureAttribute.FontFamily);
			FontSize      = Convert.ToDouble (AttributeFigure.GetDefaultAttribute (FigureAttribute.FontSize));
			FontStyle     = (Pango.Style) AttributeFigure.GetDefaultAttribute (FigureAttribute.FontStyle);
			this.text     = text;
			
			GenerateDummyContext ();
		}

		protected TextFigure (SerializationInfo info, StreamingContext context) : base (info, context)
		{
			FontColor     = (Cairo.Color) info.GetValue ("FontColor", typeof (Cairo.Color));
			FontAlignment = (Pango.Alignment) info.GetValue ("FontAlignment", typeof (Pango.Alignment));
			FontFamily    = (string) info.GetValue ("FontFamily", typeof (string));
			FontSize      = (double) info.GetDouble ("FontSize");
			FontStyle     = (Pango.Style) info.GetValue ("FontStyle", typeof (Pango.Style));
			displaybox    = (RectangleD) info.GetValue ("DisplayBox", typeof (RectangleD));
			text          = (string) info.GetValue ("Text", typeof (string));
			textEditable  = info.GetBoolean ("TextEditable");
			padding       = info.GetDouble ("Padding");
		}
		
		public event EventHandler TextChanged;
		
		#region Text Members
		public Pango.Alignment FontAlignment { get; set; }
		public Cairo.Color FontColor { get; set; }
		public string FontFamily { get; set; }
		public double FontSize { get; set; }
		public Pango.Style FontStyle { get; set; }
		
		public virtual string Text {
			get { return text; }
			set {
				if (text == value)
					return;
				
				text = value;
				WillChange ();
				
				if (!String.IsNullOrEmpty (text))
					PangoLayout.SetText (value);
				
				RecalculateDisplayBox ();
				Changed ();					
				OnTextChanged ();
			}
		}

		public bool TextEditable { get; set; }
		public virtual Pango.Layout PangoLayout	{ get; protected set; }
		
		public virtual double Padding {
			get { return padding; }
			set {
				if (value < 0)
					return;
			
				WillChange ();
				padding = value;
				RecalculateDisplayBox ();
				Changed ();
			}
		}		

		public override ITool CreateFigureTool (IDrawingEditor editor, ITool dt)
		{
			return TextEditable ? new SimpleTextTool (editor, this, dt) : dt;
		}
				
		protected virtual void OnTextChanged ()
		{
			var handler = TextChanged;
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
		
		protected virtual void SetupLayout (Cairo.Context context)
		{
			PangoLayout = Pango.CairoHelper.CreateLayout (context);
			PangoLayout.FontDescription = FontFactory
				.GetFontFromDescription (String.Format ("{0} {1}", FontFamily, FontSize));
			
			if (Text != null && Text.Length > 0)
				PangoLayout.SetText (Text);
			
			PangoLayout.Alignment = FontAlignment;
			PangoLayout.ContextChanged ();
		}

		#endregion

		#region Attribute Figure Members
		public override object GetAttribute (FigureAttribute attribute)
		{
			switch (attribute) {
				case FigureAttribute.FillColor:
					return FillColor;
				case FigureAttribute.FontAlignment:
					return FontAlignment;
				case FigureAttribute.FontColor:
					return FontColor;
				case FigureAttribute.FontSize:
					return FontSize;
				case FigureAttribute.FontStyle:
					return FontStyle;
				case FigureAttribute.LineColor:
					return LineColor;
			}
			return base.GetAttribute (attribute); 
		}

		public override void SetAttribute (FigureAttribute attribute, object value)
		{
			//FIXME: Improve this logic, because doesn't make any sense
			//invalidating when isn't needed (current value = new value)
			
			WillChange ();
			switch (attribute) {
				case FigureAttribute.FillColor:
					FillColor = (Cairo.Color) value;
					break;
				case FigureAttribute.FontAlignment:
					FontAlignment = (Pango.Alignment) value; 
					break;
				case FigureAttribute.FontColor:
					FontColor = (Cairo.Color) value;
					break;
				case FigureAttribute.FontSize:
					FontSize = (double) value;
					break;
				case FigureAttribute.FontStyle:
					FontStyle = (Pango.Style) value;
					break;
				case FigureAttribute.LineColor:
					LineColor = (Cairo.Color) value;
					break;
				default:
					base.SetAttribute (attribute, value);
					break;
			}
			GenerateDummyContext (); 
			Changed ();
		}
		#endregion
		
		#region ISerializable implementation
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("DisplayBox", displaybox);
			info.AddValue ("FontAlignment", FontAlignment);
			info.AddValue ("FontColor", FontColor);
			info.AddValue ("FontFamily", FontFamily);
			info.AddValue ("FontSize", FontSize);
			info.AddValue ("FontStyle", FontStyle);
			info.AddValue ("Padding", padding);
			info.AddValue ("Text", text);
			info.AddValue ("TextEditable", textEditable);

			base.GetObjectData (info, context);
		}
		#endregion
		
		#region Drawing Members
		protected override RectangleD BasicDisplayBox {
			get { return displaybox; }
			set {
				WillChange ();
				displaybox = value; 
				RecalculateDisplayBox ();
			}
		}

		protected override void BasicDraw (Cairo.Context context)
		{
			SetupLayout (context);
			DrawText (context);
			
			if (!usingDummy)
				return;
			
			RecalculateDisplayBox ();
			Changed ();
			usingDummy = false;
		}

		protected override void BasicDrawSelected (Cairo.Context context)
		{
			RectangleD rect = DisplayBox;
			context.LineWidth = 1.0;
			
			rect.OffsetDot5 ();
			context.Rectangle (GdkCairoHelper.CairoRectangle(rect));
			context.Stroke ();
		}

		protected virtual void DrawText (Cairo.Context context)
		{
			context.Color = FontColor;
			context.MoveTo (DisplayBox.X + Padding, DisplayBox.Y + Padding);
			Pango.CairoHelper.ShowLayout (context, PangoLayout);
			context.Stroke ();
		}
		
		protected void RecalculateDisplayBox ()
		{
			int w = 0;
			int h = 0;
			
			if (PangoLayout != null)
				PangoLayout.GetPixelSize (out w, out h);
			
			var r = new RectangleD (DisplayBox.X + Padding, DisplayBox.Y + Padding, 
									(double) w, (double) h);

			r.Inflate (Padding, Padding);
			displaybox = r; 
		}

		#endregion
		
		#region Private Members
		private void GenerateDummyContext ()
		{
			// Generates a dummy Cairo.Context. This trick is necesary in order to get
			// a Pango.Layout before obtaining a valid Cairo Context, otherwise, we should
			// wait until Draw method is called. The Pango.Layout is neccesary for
			// RecalculateDisplayBox.
			var surface = new ImageSurface (Cairo.Format.ARGB32, 100, 100);			
			using (Cairo.Context dummycontext =  new Cairo.Context (surface)) {
				SetupLayout (dummycontext);
				RecalculateDisplayBox ();
			}
		}
		
		private RectangleD      displaybox;
		private double          padding;
		private string          text;
		private bool            textEditable;
		private bool            usingDummy = true;
		#endregion
	}
}
