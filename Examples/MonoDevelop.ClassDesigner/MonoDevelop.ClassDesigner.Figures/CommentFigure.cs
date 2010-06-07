// 
// CommentFigure.cs
//  
// Author:
//       Evan <erbriones@gmail.com>
// 
// Copyright (c) 2010 Evan
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
using Gdk;
using MonoHotDraw.Figures;
using MonoHotDraw.Util;

namespace MonoDevelop.ClassDesigner.Figures
{

	public class CommentFigure : MultiLineTextFigure
	{

		public CommentFigure (string comment) : base (comment)
		{
		}
		
		public override void BasicDraw (Cairo.Context context)
		{
			RectangleD rect = DisplayBox;
			
			CairoFigures.RoundedRectangle (context, rect, 7.5);

			context.Color = new Cairo.Color (1.0, 1.0, 0.7, 0.8);
			context.FillPreserve ();
			context.LineWidth = 1.0;
			context.Color = new Cairo.Color(0.0, 0.0, 0.0, 1.0);
			context.Stroke ();
			base.BasicDraw (context);
		}
		
		public override void BasicDrawSelected (Cairo.Context context)
		{
			RectangleD rect = DisplayBox;
			rect.OffsetDot5 ();
			
			CairoFigures.RoundedRectangle (context, rect, 7.5);
			
			context.LineWidth = 3.0;
			context.Color = new Cairo.Color(0.0, 0.0, 0.0, 1.0);
			context.Stroke ();
		}

	}
}