// MonoDevelop ClassDesigner
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//  Evan Briones <erbriones@gmail.com>
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

using MonoDevelop.ClassDesigner;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.ClassDesigner {

	public class ShowClassDesignerHandler: CommandHandler {
		protected override void Run() 
		{
			object item = IdeApp.ProjectOperations.CurrentSelectedItem;
			Project project;
			
			if (item is ProjectFile)
				project = ((ProjectFile)item).Project;
			else if (item is Project)
				project = (Project) item;
			else if (item is ProjectFolder)
				project = ((ProjectFolder) item).Project;
			else
				project = IdeApp.ProjectOperations.CurrentSelectedProject;
	
			var view = new ClassDesignerView (project);
			view.Designer.AddFromProject (project);
			
			IdeApp.Workbench.OpenDocument(view, true);
			var dom = ProjectDomService.GetProjectDom (project);
			
			foreach (IType type in dom.Types) {
					System.Console.WriteLine("-----------");
					System.Console.WriteLine(type.FullName);
					System.Console.WriteLine(type.Namespace);
					System.Console.WriteLine(type.HasParts);
					System.Console.WriteLine(type.ClassType.ToString());
					System.Console.WriteLine(type.FieldCount);
					System.Console.WriteLine("-----------");
			}
		
			return;
		}
	}
}