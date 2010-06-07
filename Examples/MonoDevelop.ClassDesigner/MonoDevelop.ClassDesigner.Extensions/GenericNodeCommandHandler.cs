// 
// GenericNodeBuilderExtension.cs
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

using System;
using System.Linq;
using MonoDevelop.ClassDesigner;
using MonoDevelop.ClassDesigner.Diagram;
using MonoDevelop.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Ide.Gui.Pads.ClassPad;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.ClassDesigner.Extensions
{
	class GenericNodeCommandHandler : NodeCommandHandler
	{
		[CommandUpdateHandler (Commands.ShowClassDesigner)]
		[AllowMultiSelection]
		public void UpdateHandler (CommandInfo item)
		{
			var project = GetProject (CurrentNode);
			item.Enabled = IdeApp.Workbench.ActiveDocument != null;
			
			if (!item.Enabled)
				return;
			
			var designer = IdeApp.Workbench.ActiveDocument.GetContent<ClassDesignerView> ()
				?? IdeApp.Workbench.Documents.Select (d => d.GetContent<ClassDesignerView> ()).FirstOrDefault (v => v != null);
			
			if (designer != null)
				project = designer.Project;
					
			item.Enabled = CurrentNodes.Any (i => (GetProject (i) == project));
		}

		[CommandHandler (Commands.ShowClassDesigner)]
		[AllowMultiSelection]
		public void Handler ()
		{
			var designer = IdeApp.Workbench.ActiveDocument.GetContent<ClassDesignerView> ()
				?? IdeApp.Workbench.Documents.Select (d => d.GetContent<ClassDesignerView> ()).FirstOrDefault (v => v != null);
					
			if (designer == null) {
				designer = new ClassDesignerView (GetProject (CurrentNode));
				IdeApp.Workbench.OpenDocument(designer, true);
			}
			
			foreach (var node in CurrentNodes) {				
				if (node.DataItem is Project) {
					designer.Diagram.AddFromProject ((Project) node.DataItem);
				} else if (node.DataItem is ProjectFolder) {
					var folder = (ProjectFolder) node.DataItem;
					designer.Diagram.AddFromDirectory (folder.Path);
				} else if (node.DataItem is ProjectFile) {
					var file = (ProjectFile) node.DataItem;
					designer.Diagram.AddFromFile (file.FilePath);
				} else if (node.DataItem is NamespaceData) {
					var nsdata = (NamespaceData) node.DataItem;
					designer.Diagram.AddFromNamespace (nsdata.FullName);
				} else if (node.DataItem is ClassData) {
					var cls = (ClassData) node.DataItem;
					designer.Diagram.AddFromType (cls.Class);
				}
			}
		}
		
		static Project GetProject (ITreeNavigator node)
		{
			Project project = null;
			
			if (node.DataItem is Project)
				project = (Project) node.DataItem;
			else if (node.DataItem is ProjectFolder)
				project = ((ProjectFolder) node.DataItem).Project;
			else if (node.DataItem is ProjectFile)
				project = ((ProjectFile) node.DataItem).Project;
			else if (node.DataItem is NamespaceData)
				project = (Project) node.GetParentDataItem (typeof (Project), false);
			else if (node.DataItem is ClassData)
				project = (Project) node.GetParentDataItem (typeof (Project), false);
			
			return project;
		}
	}
}