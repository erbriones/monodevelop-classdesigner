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
using System.Collections.Generic;

using Cairo;

using MonoHotDraw.Figures;
using MonoHotDraw.Handles;
using MonoHotDraw.Util;

using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Figures
{	
	public class MemberFigure : HStackFigure
	{
		TextFigure retval;
		TextFigure name;
		ImageFigure icon;
		
		// For testing purposes
		public MemberFigure (Gdk.Pixbuf icon, string name, string retval, bool hidden)
		{
			this.icon = new ImageFigure (icon);
			this.name = new TextFigure (name);
			this.retval = new TextFigure (retval);
			
			this.name.Padding = 2.0;
			this.name.FontSize = 10;
			this.retval.Padding = 0.0;
			this.retval.FontSize = 10;
			this.retval.FontColor = new Cairo.Color(0, 0, 1.0);
			
			Hidden = hidden;
			AllowFormatting = true;
			Alignment = HStackAlignment.Bottom;
			
			Add (this.icon);
			Add (this.retval);
			Add (this.name);
		}
		
		public MemberFigure (Gdk.Pixbuf icon, IMember memberInfo, bool hidden) : base ()
		{
			this.icon = new ImageFigure (icon);
			
			if (memberInfo.ReturnType != null)
				retval = new TextFigure (memberInfo.ReturnType.Name);
			else
				retval = new TextFigure (String.Empty);
					
			name = new TextFigure (memberInfo.Name);
			
			MemberInfo = memberInfo;
			Hidden = hidden;
			
			name.Padding = 1.0;
			name.FontSize = 10;
			retval.Padding = 0;
			retval.FontSize = 10;
			retval.FontColor = new Cairo.Color(0, 0, 1.0);
			
			AllowFormatting = true;
			Alignment = HStackAlignment.Bottom;
			
			Add (this.icon);
			Add (retval);
			Add (name);
		}

		protected override void BasicDraw (Context context)
		{
			if (Hidden)
				return;
			
			base.BasicDraw (context);
		}
		
		protected override void BasicDrawSelected (Context context)
		{
			if (Hidden)
				return;
			
			base.BasicDrawSelected (context);
		}
				
		public void Show ()
		{
			Hidden = false;
			Invalidate ();
		}
		
		public void Hide ()
		{
			Hidden = true;
			Invalidate ();
		}
		
		internal bool AllowFormatting { get; set; }
		public IMember MemberInfo { get; private set; }
		public bool Hidden { get; private set; }
		
		public string Name {
			get { return name.Text; }
		}
		
		public void UpdateFormat (MembersFormat format)
		{
			if (!AllowFormatting)
				return;
			
			Clear ();
			
			if (format == MembersFormat.Name) {
				Add (icon);
				Add (name);
			} else if (format == MembersFormat.FullSignature) {
				Add (icon);
				Add (retval);
				Add (name);
			} else if (format == MembersFormat.NameAndType) {
				Add (icon);
				Add (retval);
				Add (name);				
			}
		}
	}
}
