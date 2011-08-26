// 
// MemberCommadns.cs
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

using MonoHotDraw.Figures;

using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.Components.Commands;
using MonoDevelop.Diagram.Components;
using MonoDevelop.Ide;
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Commands
{
	internal sealed class MemberCommands : FigureCommandHandler
	{
		public override bool CanHandle (IEnumerable<Figure> figures)
		{
			return figures != null && figures.Count () > 0 && figures.All (f => f is MemberFigure);
		}
		
		[CommandHandler (DesignerCommands.GoToDeclaration)]
		protected void GoToDeclaration ()
		{
			var type = SelectedFigures.OfType<MemberFigure> ().SingleOrDefault ().MemberInfo;
			if (type != null) {
				IdeApp.ProjectOperations.JumpToDeclaration (type);
			}
		}
		
		[CommandUpdateHandler (DesignerCommands.GoToDeclaration)]
		protected void GoToDeclarationUpdate (CommandInfo info)
		{
			if (SelectedFigures.Count () == 1) {
				var type = SelectedFigures.OfType<MemberFigure> ().SingleOrDefault ().MemberInfo;
				info.Enabled = info.Visible = IdeApp.ProjectOperations.CanJumpToDeclaration (type);
			} else {
				info.Enabled = info.Visible = false;
			}
		}
		
		[CommandHandler (DesignerCommands.Hide)]
		protected void Hide ()
		{
			foreach (var figure in SelectedFigures) {
				figure.Visible = false;
			}
		}
		
		[CommandUpdateHandler (DesignerCommands.Hide)]
		protected void HideUpdate (CommandInfo info)
		{
			info.Enabled = info.Visible = true;
		}
		
		[CommandHandler (DesignerCommands.ShowAssociation)]
		protected void ShowAsAssociation ()
		{
			var designer = (ClassDesigner) Designer;
			foreach (var m in SelectedFigures.OfType<MemberFigure> ()) {
				var type = designer.Dom.GetType (m.MemberInfo.ReturnType.DecoratedFullName);
				var typeFigure = designer.Diagram.GetTypeFigure (type.DecoratedFullName)
						?? designer.Diagram.CreateTypeFigure (type);
				var classFigure = designer.Diagram.GetTypeFigure (m.MemberInfo.DeclaringType.DecoratedFullName);
				
				designer.Diagram.Add (new AssociationLine (m, classFigure, typeFigure));
			}
		}
		
		[CommandUpdateHandler (DesignerCommands.ShowAssociation)]
		protected void ShowAsAssociationUpdate (CommandInfo info)
		{
			if (SelectedFigures.OfType<MemberFigure> ().All (f => f.MemberInfo.MemberType == MemberType.Property)) {
				info.Visible = info.Enabled = true;
			} else {
				info.Visible = info.Enabled = false;
			}
		}
		
		[CommandHandler (DesignerCommands.ShowAssociationCollection)]
		protected void ShowAsAssociationCollection ()
		{
			
		}
		
		[CommandUpdateHandler (DesignerCommands.ShowAssociationCollection)]
		protected void ShowAsAssociationCollectionUpdate (CommandInfo info)
		{
			info.Visible = info.Enabled = false;
		}
	}
}

