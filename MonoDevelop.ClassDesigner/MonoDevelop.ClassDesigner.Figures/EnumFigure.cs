// MonoDevelop ClassDesigner
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//  Evan Briones <erbriones@gmail.com
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

using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MonoHotDraw.Figures;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Figures
{	
	public sealed class EnumFigure: TypeFigure
	{
		public EnumFigure (): base ()
		{
			FillColor = new Cairo.Color (0.6367, 0.9570, 0.6757);
		}
		public EnumFigure (IType domtype): base (domtype)
		{
			FillColor = new Cairo.Color (0.6367, 0.9570, 0.6757);
		}

		#region ISerializableFigure implementation
		public override XElement Serialize ()
		{
			var xml = base.Serialize ();
			xml.Name = "Enum";
			return xml;
		}
		#endregion
		
		public override ClassType ClassType {
			get { return ClassType.Enum; }
		}
		
		protected override void RebuildCompartments ()
		{
			MemberCompartments.Clear ();
			MemberCompartments.AddRange (Members.OrderBy (m => m.Name));
		}
	}
}
