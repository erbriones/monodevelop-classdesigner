// 
// ClassCommands.cs
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

using System;
using System.Linq;
using System.Collections.Generic;

using MonoDevelop.ClassDesigner;
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.Components.Commands;
using MonoDevelop.Diagram.Components;
using MonoDevelop.Projects.Dom;
using MonoHotDraw.Figures;

namespace MonoDevelop.ClassDesigner.Commands
{
	internal sealed class ClassCommands : FigureCommandHandler
	{
		public override bool CanHandle (IEnumerable<IFigure> figures)
		{
			if (figures == null && figures.Count () == 0)
				return false;
			
			return figures.All (f => f is ClassFigure);
		}
		
		#region Commands
		
				
		[CommandHandler (DesignerCommands.ShowBase)]
		protected void ShowBase ()
		{
			var designer = (ClassDesigner) Designer;
			
			foreach (ClassFigure superFigure in designer.View.SelectionEnumerator.OfType<ClassFigure> ()) {
				IType superType = designer.Dom.GetType (superFigure.Name.FullName);
	
				if (superType.BaseType == null) 
					continue;
				
				string baseTypeName = superType.BaseType.FullName;
				ClassFigure baseFigure = (ClassFigure) designer.Diagram.GetFigure (baseTypeName);
				
				if (baseFigure == null) {
					IType baseType = designer.Dom.GetType (baseTypeName);
					baseFigure = (ClassFigure) designer.Diagram.CreateFigure (baseType);
				}
				
				designer.View.Add (new InheritanceConnectionFigure (baseFigure, superFigure));
			}
		}
		
		[CommandHandler (DesignerCommands.ShowDerived)]
		protected void ShowDerived ()
		{
			var designer = (ClassDesigner) Designer;
			
			IEnumerable<IType> baseTypes = designer.View.SelectionEnumerator
				.OfType<ClassFigure> ()
				.Select (figure => designer.Dom.GetType (figure.Name.FullName));
			
			foreach (IType type in designer.Dom.Types) {
				if (type.BaseType == null || type.ClassType != ClassType.Class)
					continue;
				
				var baseType = baseTypes
					.Where (bt => bt.FullName == type.BaseType.FullName)
					.SingleOrDefault ();
				
				if (baseType == null)
					continue;
				
				var baseFigure = (ClassFigure) designer.Diagram.GetFigure (baseType.FullName);
				var superFigure = (ClassFigure) designer.Diagram.GetFigure (type.FullName);
				
				if (baseFigure == null)
					baseFigure = (ClassFigure) designer.Diagram.CreateFigure (baseType);
				
				if (superFigure == null)
					superFigure = (ClassFigure) designer.Diagram.CreateFigure (type);
				
				designer.Diagram.Add (new InheritanceConnectionFigure (baseFigure, superFigure));
			}
		}
		
		[CommandUpdateHandler (DesignerCommands.ShowBase)]
		protected void UpdateShowBase (CommandInfo info)
		{
			info.Enabled = true;
		}
		
		[CommandUpdateHandler (DesignerCommands.ShowDerived)]
		protected void UpdateShowDerived (CommandInfo info)
		{
			info.Enabled = true;
		}
		#endregion
	}
}

