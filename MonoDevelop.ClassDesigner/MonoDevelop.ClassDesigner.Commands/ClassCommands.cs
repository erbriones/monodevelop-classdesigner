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
		
		//TODO: Make these two commands play nicely with the new automatic inheritance line adding...
		[CommandHandler (DesignerCommands.ShowBase)]
		protected void ShowBase ()
		{
			var designer = (ClassDesigner) Designer;
			foreach (var derivedFigure in Designer.View.SelectionEnumerator.OfType<ClassFigure> ()) {
				if (!String.IsNullOrEmpty (derivedFigure.BaseTypeFullName)) {
					var baseType = designer.Dom.GetType(derivedFigure.BaseTypeFullName);
					
					var baseFigure = designer.Diagram.GetTypeFigure (baseType.FullName)
							?? designer.Diagram.CreateTypeFigure (baseType);
					
					designer.View.Add (new InheritanceConnectionFigure (derivedFigure, baseFigure));
				}
			}
		}
		
		[CommandHandler (DesignerCommands.ShowDerived)]
		protected void ShowDerived ()
		{
			var designer = (ClassDesigner) Designer;
			var baseFigures = designer.View.SelectionEnumerator.OfType<ClassFigure> ();
			
			foreach (var type in designer.Dom.Types.Where (t => t.BaseType != null && t.ClassType == ClassType.Class)) {
				var baseFigure = baseFigures.SingleOrDefault (bf => bf.TypeFullName == type.BaseType.FullName);
				
				if (baseFigure != null) {
					var derivedFigure = designer.Diagram.GetTypeFigure (type.FullName)
							?? designer.Diagram.CreateTypeFigure (type);
					
					designer.Diagram.Add (new InheritanceConnectionFigure (derivedFigure, baseFigure));
				}
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

