// 
// CircularLineTerminal.cs
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

using Cairo;
using MonoHotDraw.Util;
using System;
using System.Runtime.Serialization;

namespace MonoHotDraw.Figures
{
	[Serializable]
	public class CircularLineTerminal : LineTerminal
	{		
		double _scaleX;
		double _scaleY;
		double _radius;
		
		public CircularLineTerminal () : this (1.0, 1.0, 5.0)
		{
		}
		
		public CircularLineTerminal (double scaleX, double scaleY, double radius) : base ()
		{	
			_scaleX = scaleX;
			_scaleY = scaleY;
			_radius = radius;
		}
		
		protected CircularLineTerminal (SerializationInfo info, StreamingContext context) : base (info, context)
		{
			_scaleX = info.GetDouble ("Scale-X");
			_scaleY = info.GetDouble ("Scale-Y");
			_radius = info.GetDouble ("Radius");
		}		
		
		public override PointD Draw (Context context, PointD a, PointD b)
		{
			// Save context 
			context.Save ();
			
			if (_scaleX != _scaleY)
				context.Scale(_scaleX, _scaleY);
			
			var midpoint = new PointD(a.X + 2 * _radius, a.Y + 2 * _radius);
			
			context.Arc (midpoint.X, midpoint.Y, _radius, 0, (2 * Math.PI));
			context.Restore ();
			context.Stroke ();
	
			return Geometry.EdgePointOfCircle (midpoint, _radius, b);
		}
		
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("Scale-X", _scaleX);
			info.AddValue ("Scale-Y", _scaleY);
			info.AddValue ("Radius", _radius);
				
			base.GetObjectData (info, context);
		}
	}
}

