// 
// Collapsable.cs
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

using System.Linq;
using System.Collections.Generic;

using MonoDevelop.ClassDesigner;
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.Components.Commands;
using MonoDevelop.Diagram.Components;
using MonoHotDraw.Figures;

namespace MonoDevelop.ClassDesigner.Commands
{
	internal sealed class CollapsableCommand : FigureCommandHandler
	{
		#region Commands
		[CommandHandler (DesignerCommands.Collapse)]
		protected void CollapseItem ()
		{
			var designer = (ClassDesigner) Designer;
			
			foreach (IFigure figure in designer.View.SelectionEnumerator) {
				var c = figure as ICollapsable;
					
				if (c != null)		
					c.Collapsed = true;
			}
		}
		
		[CommandHandler (DesignerCommands.Expand)]
		protected void ExpandItem ()
		{
			var designer = (ClassDesigner) Designer;

			foreach (IFigure figure in designer.View.SelectionEnumerator) {
				var c = figure as ICollapsable;
					
				if (c != null)
					c.Collapsed = false;
			}
		}
		
		[CommandUpdateHandler (DesignerCommands.Collapse)]
		protected void UpdateCollapseItem (CommandInfo info)
		{
			var designer = (ClassDesigner) Designer;
			info.Enabled = false;
			info.Visible = true;
			
			if (designer.View.SelectionCount == 0) {
				info.Visible = false;	
				return;
			}
			
			foreach (IFigure figure in designer.View.SelectionEnumerator) {
				var c = figure as ICollapsable;
				if (c != null) {
					if (!c.Collapsed) {
						info.Visible = info.Enabled = true;
						return;
					}
				}
			}
		}
		
		[CommandUpdateHandler (DesignerCommands.Expand)]
		protected void UpdateExpandItem (CommandInfo info)
		{
			var designer = (ClassDesigner) Designer;
			info.Enabled = false;
			info.Visible = true;
			
			if (designer.View.SelectionCount == 0) {
				info.Visible = false;
				return;
			}
			
			foreach (IFigure figure in designer.View.SelectionEnumerator) {
				var c = figure as ICollapsable;
				if (c != null) {
					if (c.Collapsed) {
						info.Visible = info.Enabled = true;
						return;
					}
				}
			}
		}
		#endregion
	}
}

