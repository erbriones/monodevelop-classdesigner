// 
// ZoomWidget.cs
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

using System;
using MonoHotDraw;
using Gtk;

namespace MonoDevelop.ClassDesigner.Components
{
	public class ZoomWidget : Bin
	{	
		double _scale;
		ScaleRange _range;
		SeparatorMenuItem seperator;
		Entry zoomEntry;
		HScale zoomSlider;
		
		public ZoomWidget (ScaleRange range, double startScale)
		{
			_range = range;
			_scale = startScale;
			
			zoomSlider = new HScale (range.Minimum, range.Maximum, range.Step);
			zoomEntry = new Entry (startScale.ToString ());
			seperator = new SeparatorMenuItem ();
			zoomSlider.Value = startScale;
			
			zoomSlider.ValueChanged += OnSliderChangeValue;;
			zoomEntry.TextInserted += OnEntryTextInserted;
			
			Add (zoomSlider);
			Add (seperator);
			Add (zoomEntry);
		}
		
		void OnSliderChangeValue (object o, EventArgs e)
		{
			Scale = zoomSlider.Value;
		}

		void OnEntryTextInserted (object o, TextInsertedArgs e)
		{
			double percent;
			
			if (String.IsNullOrEmpty(e.Text))
				return;
			
			Double.TryParse (e.Text, out percent);
			Scale = percent / 100;
		}
		
		public double Scale {
			get {
				return _scale;
			} set {
				if (value > _range.Maximum)
					_scale = _range.Maximum;
				else if (value < _range.Minimum)
					_scale = _range.Minimum;
				else
					_scale = value;
			}
		}
		
		public ScaleRange ScaleRange {
			get { 
					if (_range == null)
						_range = new ScaleRange (10, 0.25, 0.1);
					
					return _range;
			}
			set {
				if (value == null)
					return;
				
				_range = value;	
			}
		}
	}
}
