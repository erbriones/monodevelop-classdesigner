// MonoDevelop ClassDesigner
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//  Evan Briones <erbriones@gmail.com>
//
// Copyright (C) 2009 Manuel Cerón
// Copyright (C) 2010 Evan Briones
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
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Figures
{	
	public class TypeMemberFigure: HStackFigure, IMemberFigure
	{
		bool allow_formatting;
		bool _hidden;
		IBaseMember _memberInfo;
		TextFigure _retvalue;
		TextFigure _name;
		ImageFigure _icon;
		
		// For testing purposes
		public TypeMemberFigure (Pixbuf icon, string name, string retval, bool hidden)
		{
			_icon = new ImageFigure (icon);
			_name = new TextFigure (name);
			_retvalue = new TextFigure (retval);
			
			_name.Padding = 0.0;
			_name.FontSize = 10;
			_retvalue.Padding = 0.0;
			_retvalue.FontSize = 10;
			_retvalue.FontColor = new Cairo.Color(0, 0, 1.0);
			
			Hidden = hidden;
			AllowFormatting = true;
			Alignment = HStackAlignment.Bottom;
			
			Add (_icon);
			Add (_retvalue);
			Add (_name);
		}
		
		public TypeMemberFigure (Pixbuf icon, IBaseMember memberInfo, bool hidden) : base ()
		{
			_icon = new ImageFigure (icon);
			_memberInfo = memberInfo;
			
			if (memberInfo.ReturnType != null)
				_retvalue = new TextFigure (memberInfo.ReturnType.Name);
			else
				_retvalue = new TextFigure (String.Empty);
					
			_name = new TextFigure (memberInfo.Name);
			_hidden = hidden;
			
			_name.Padding = 0.0;
			_name.FontSize = 10;
			_retvalue.Padding = 0.0;
			_retvalue.FontSize = 10;
			_retvalue.FontColor = new Cairo.Color(0, 0, 1.0);
			
			AllowFormatting = true;
			Alignment = HStackAlignment.Bottom;
			
			Add(_icon);
			Add(_retvalue);
			Add(_name);
		}

		internal bool AllowFormatting {
			get { return allow_formatting; }
			set { allow_formatting = value; }
		}
		public bool Hidden {
			get { return _hidden; }
			set { _hidden = value; }
		}
		
		public string Name {
			get { return _name.Text; }
		}
		
		public IBaseMember MemberInfo {
			get { return _memberInfo; }
		}
		
		public void UpdateFormat (MembersFormat format)
		{
			if (!AllowFormatting)
				return;
			
			Remove (_retvalue);
			
			if (format == MembersFormat.Name)
				return;
			
			Remove (_name);
			Add (_retvalue);
			Add (_name);
		}

	}
}
