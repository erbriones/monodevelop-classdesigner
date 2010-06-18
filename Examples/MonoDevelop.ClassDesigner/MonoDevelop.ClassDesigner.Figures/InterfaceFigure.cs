// 
// InterfaceFigure.cs
//  
// Author:
//       Evan <erbriones@gmail.com>
// 
// Copyright (c) 2010 Evan
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
using MonoDevelop.Core;
using MonoDevelop.Core.Gui;
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Figures
{

	public class InterfaceFigure : TypeFigure
	{
		public InterfaceFigure (IType domType) : base(domType)
		{
			FigureColor = new Cairo.Color (0.8, 0.8, 0.8, 0.4);
		}

		protected override ClassType ClassType {
			get { return ClassType.Interface; }
		}
		
		
		// FIXME: Figure out how this works for access
		public override void Update ()
		{
			if (grouping != GroupingSetting.Access) {
				base.Update ();
				return;
			}
			
			var compartment = Compartments.Where (c => c.Name == "Members").SingleOrDefault ();
			var members = new List<TypeMemberFigure> ();
				
			
			members.AddRange (Compartments
			                  .Select (c => c.FiguresEnumerator.Where (m => m != null))
			                  .OfType<TypeMemberFigure> ());
			                  
			foreach (var c in Compartments) {
				RemoveMemberGroup (c);
				compartment.Clear ();
			}
			
			if (members.Count () == 0) {
				foreach (var m in Name.Members) {
					var icon = ImageService.GetPixbuf (m.StockIcon, IconSize.Menu);
					members.Add (new TypeMemberFigure (icon, m, false));
				}
			}
			
			compartment.AddMembers (members);
			AddMemberGroup (compartment);
		}

	}
}
