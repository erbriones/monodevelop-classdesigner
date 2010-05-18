// MonoDevelop ClassDesigner
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//
// Copyright (C) 2009 Manuel Cerón
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
using Gdk;
using MonoHotDraw.Figures;
using MonoHotDraw.Util;

namespace MonoDevelop.ClassDesigner.Figures {
	
	public class TypeMemberFigure: HStackFigure {
		
		public TypeMemberFigure(Pixbuf icon, string retvalue, string name): base()
		{
			_icon = new PixbufFigure(icon);
			_retvalue = new SimpleTextFigure(retvalue);
			_name = new SimpleTextFigure(name);
			
			_name.Padding = 0.0;
			_name.FontSize = 10;
			_retvalue.Padding = 0.0;
			_retvalue.FontSize = 10;
			_retvalue.FontColor = new Cairo.Color(0, 0, 1.0);
			
			Alignment = HStackAlignment.Bottom;
			
			Add(_icon);
			Add(_retvalue);
			Add(_name);
		}
		
		private SimpleTextFigure _retvalue;
		private SimpleTextFigure _name;
		private PixbufFigure _icon;
	}
}
