// 
// TypeUpdateFigureVisitor.cs
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


using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.Ide;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;

using MonoHotDraw;
using MonoHotDraw.Figures;
using MonoHotDraw.Handles;
using MonoHotDraw.Visitor;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.ClassDesigner.Visitor
{
	public class TypeUpdateFigureVisitor : IFigureVisitor
	{
		IDrawing drawing;
		ProjectDom dom;
		TypeUpdateInformation updateInfo;

		public TypeUpdateFigureVisitor (IDrawing drawing, ProjectDom dom, TypeUpdateInformation updateInfo)
		{
			this.drawing = drawing;
			this.dom = dom;
			this.updateInfo = updateInfo;
		}
		
		#region IFigureVisitor implementation
		
		public void VisitFigure (IFigure hostFigure)
		{
			var tf = hostFigure as TypeFigure;
			
			if (tf == null)
				return;
	
			if (updateInfo.Removed.Any (t => t.FullName == tf.Name.FullName)) {
				drawing.Remove (hostFigure);
				return;
			}
			
			var updatedMembers = new Dictionary<string, IFigure> ();		
			foreach (IMember member in tf.Name.Members) {
				var key = member.FullName;
				IFigure figure;
				tf.Members.TryGetValue (key, out figure);
				
				if (figure == null) {
					var icon = ImageService.GetPixbuf (member.StockIcon, Gtk.IconSize.Menu);		
					figure = new MemberFigure (icon, member, false);
				}
				
				updatedMembers.Add (key, figure);
			}
			
			tf.Members.Clear ();
			tf.Members.Concat (updatedMembers);
			
			var groupVistor = new GroupFormatVisitor (drawing, tf);
			tf.AcceptVisitor (groupVistor);
		}

		public void VisitHandle (IHandle hostHandle)
		{
		}
		#endregion
	}
}
