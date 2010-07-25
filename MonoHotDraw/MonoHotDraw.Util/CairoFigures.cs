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
using Cairo;

namespace MonoHotDraw.Util {

	public sealed class CairoFigures {
	
		private CairoFigures ()
		{
		}

		public static void CurvedRectangle (Cairo.Context c, RectangleD rect, double radius) {
		
			if (rect.Width < (radius * 2.0) ) {
				radius = rect.Width/2.0;
			}

			if (rect.Height < (radius * 2.0) ) {
				radius = rect.Height/2.0;
			}
			
			c.MoveTo (rect.X, rect.Y+radius);
			c.LineTo (rect.X, rect.Y2-radius);
			c.CurveTo (rect.X, rect.Y2-radius, rect.X, rect.Y2, rect.X+radius, rect.Y2);
			c.LineTo (rect.X2-radius, rect.Y2);
			c.CurveTo (rect.X2-radius, rect.Y2, rect.X2, rect.Y2, rect.X2, rect.Y2-radius);
			c.LineTo (rect.X2, rect.Y+radius);
			c.CurveTo (rect.X2, rect.Y+radius, rect.X2, rect.Y, rect.X2-radius, rect.Y);
			c.LineTo (rect.X+radius, rect.Y);
			c.CurveTo (rect.X+radius, rect.Y, rect.X, rect.Y, rect.X, rect.Y+radius);
		}
		
		public static void RoundedRectangle (Cairo.Context c, RectangleD rect, double radius)
		{
			if (radius > (rect.Width /2) || radius > (rect.Height / 2)) {
				radius = Math.Min ((rect.Width /2), (rect.Height / 2));
			}
			
			c.Save ();
			
			/* Bottom Left */
			c.MoveTo(rect.X, rect.Y + radius);
			c.Arc (rect.X + radius, rect.Y + radius, radius, Math.PI, -Math.PI/2);
			c.LineTo (rect.X2 - radius, rect.Y);
			
			/* Bottom Right */
			c.Arc (rect.X2 - radius, rect.Y + radius, radius, -Math.PI/2, 0);
			c.LineTo (rect.X2, rect.Y2 - radius);
				
			/* Top Right */
			c.Arc (rect.X2 - radius, rect.Y2 - radius, radius, 0, Math.PI/2);
			c.LineTo (rect.X + radius, rect.Y2);
					
			/* Top Left */
			c.Arc(rect.X + radius, rect.Y2 - radius, radius, Math.PI/2, Math.PI);
			c.ClosePath ();
			
			c.Restore ();
		}
		
	}
}
