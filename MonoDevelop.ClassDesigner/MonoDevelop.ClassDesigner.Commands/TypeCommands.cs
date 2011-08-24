// 
// TypeCommands.cs
//  
// Author:
//       Graham Lyon <graham.lyon@gmail.com>
// 
// Copyright (c) 2011 Graham Lyon
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

using System.Collections.Generic;
using System.Linq;

using MonoDevelop.ClassDesigner;
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Diagram.Components;
using MonoDevelop.Projects.Dom;
using MonoHotDraw.Figures;

namespace MonoDevelop.ClassDesigner.Commands
{
	public class TypeCommands : FigureCommandHandler
	{
		public override bool CanHandle (IEnumerable<IFigure> figures)
		{
			return figures != null && figures.Count () > 0 && figures.All (f => f is TypeFigure);
		}
		
		[CommandHandler (DesignerCommands.GoToDeclaration)]
		protected void GoToDeclaration ()
		{
			var type = Designer.Dom.GetType (SelectedFigures.OfType<TypeFigure> ().SingleOrDefault ().DecoratedFullName);
			if (type != null) {
				IdeApp.ProjectOperations.JumpToDeclaration (type);
			}
		}
		
		[CommandUpdateHandler (DesignerCommands.GoToDeclaration)]
		protected void GoToDeclarationUpdate (CommandInfo info)
		{
			if (SelectedFigures.Count () == 1) {
				var type = Designer.Dom.GetType (SelectedFigures.OfType<TypeFigure> ().SingleOrDefault ().DecoratedFullName);
				info.Enabled = info.Visible = IdeApp.ProjectOperations.CanJumpToDeclaration (type);
			} else {
				info.Enabled = info.Visible = false;
			}
		}
		
		[CommandHandler (DesignerCommands.ShowAllMembers)]
		protected void ShowAll ()
		{
			var figure = SelectedFigures.OfType<TypeFigure> ().SingleOrDefault ();
			if (figure != null) {
				figure.ShowAll ();
			}
		}
		
		[CommandUpdateHandler (DesignerCommands.ShowAllMembers)]
		protected void ShowAllUpdate (CommandInfo info)
		{
			if (SelectedFigures.Count () == 1) {
				var figure = SelectedFigures.OfType<TypeFigure> ().SingleOrDefault ();
				info.Enabled = info.Visible = figure.HasHiddenMembers;
			} else {
				info.Enabled = info.Visible = false;
			}
		}
	}
}

