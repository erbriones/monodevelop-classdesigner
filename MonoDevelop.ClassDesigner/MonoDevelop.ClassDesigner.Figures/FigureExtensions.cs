// 
// FigureExtensions.cs
//  
// Author:
//       Graham Lyon <graham.lyon@gmail.com>
// 
// Copyright (c) 2011 Graham Lyon
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
using System.Xml.Linq;

using MonoHotDraw.Figures;
using MonoHotDraw.Util;

using MonoDevelop.ClassDesigner;

namespace MonoDevelop.ClassDesigner.Figures
{
	public static class FigureExtensions
	{
		public static XElement SerializePosition (this Figure figure, bool includeHeight)
		{
			var xml = new XElement ("Position",
				new XAttribute ("X", ClassDiagram.PixelsToInches (figure.DisplayBox.X)),
				new XAttribute ("Y", ClassDiagram.PixelsToInches (figure.DisplayBox.Y)),
				new XAttribute ("Width", ClassDiagram.PixelsToInches (figure.DisplayBox.Width))
			);
			
			if (includeHeight) {
				xml.Add (new XAttribute ("Height", ClassDiagram.PixelsToInches (figure.DisplayBox.Height)));
			}
			
			return xml;
		}
		
		public static void DeserializePosition (this Figure figure, XElement position)
		{
			if (position == null)
				return;
			
			try {
				var xAttr = position.Attribute ("X");
				var x = (xAttr == null) ?
						figure.DisplayBox.X : ClassDiagram.InchesToPixels(Double.Parse (xAttr.Value));
				
				var yAttr = position.Attribute ("Y");
				var y = (yAttr == null) ?
						figure.DisplayBox.Y : ClassDiagram.InchesToPixels(Double.Parse (yAttr.Value));
				
				var widthAttr = position.Attribute ("Width");
				var width = (widthAttr == null) ?
						figure.DisplayBox.Width : ClassDiagram.InchesToPixels(Double.Parse (widthAttr.Value));
				
				var heightAttr = position.Attribute ("Height");
				var height = (heightAttr == null) ?
						figure.DisplayBox.Height : ClassDiagram.InchesToPixels(Double.Parse (heightAttr.Value));
				
				figure.MoveTo (x, y);
				figure.DisplayBox = new RectangleD (x, y, width, height);
			} catch (Exception e) {
				throw new DeserializationException ("Unable to deserialize position data", e);	
			}
		}
	}
}

