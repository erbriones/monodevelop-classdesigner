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

using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using MonoHotDraw.Figures;
using MonoDevelop.Core;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Core.Gui;

namespace MonoDevelop.ClassDesigner.Figures {
	
	public class EnumFigure: TypeFigure {

		public EnumFigure(IType domtype): base(domtype) {
			FigureColor = new Cairo.Color (0.1, 0.9, 0.2, 0.4);
		}
		
		protected override ClassType ClassType {
			get {
				return ClassType.Enum;
			}
		}
		
		// FIXME: Set up correct compartments
		public override void Update ()
		{
			List<TypeMemberFigure> members = new List<TypeMemberFigure> ();
			TypeMemberGroupFigure compartment = Compartments
				.Where (c => c.Name == "Fields")
				.SingleOrDefault ();
			
			members.AddRange (Compartments.Select(c => c.FiguresEnumerator).OfType<TypeMemberFigure> ());
			
			if (members.Count () != Name.FieldCount) {
				foreach (var f in Name.Fields) {
					var icon = ImageService.GetPixbuf (f.StockIcon, IconSize.Menu);
					members.Add (new TypeMemberFigure (icon, f, false));
				}
			}
			
			if (grouping == GroupingSetting.Alphabetical)
				compartment.AddMembers (members.OrderBy (m => m.Name));
			else
				compartment.AddMembers (members);
			
			AddMemberGroup (compartment);
		}
		// FIXME: Set up correct compartments
		protected override void CreateCompartments ()
		{
			var fields = new TypeMemberGroupFigure (GettextCatalog.GetString ("Fields"));
			AddCompartment (fields);
		}		
	}
}
