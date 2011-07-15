// 
// DelegateFigure.cs
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

using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
//using MonoDevelop.Core;

using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;

using MonoHotDraw.Figures;

namespace MonoDevelop.ClassDesigner.Figures
{
	public sealed class DelegateFigure : TypeFigure
	{
		public DelegateFigure (IType domType) : base (domType)
		{
			FillColor = new Cairo.Color (0.8, 0.1, 0.8, 0.4);
		}

		#region ISerializableFigure implementation
		public override XElement Serialize ()
		{
			var xml = base.Serialize ();
			xml.Name = "Delegate";
			return xml;
		}
		#endregion
		
		protected override ClassType ClassType {
			get { return ClassType.Delegate; }
		}
	}
}
