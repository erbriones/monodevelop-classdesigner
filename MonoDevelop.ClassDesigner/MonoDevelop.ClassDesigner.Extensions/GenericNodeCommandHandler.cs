// 
// GenericNodeBuilderExtension.cs
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
using MonoDevelop.ClassDesigner;
using MonoDevelop.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Projects;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Ide.Gui.Pads.ClassPad;

namespace MonoDevelop.ClassDesigner.Extensions
{
	internal class GenericNodeCommandHandler : NodeCommandHandler
	{
		[CommandUpdateHandler (DesignerCommands.ShowDesigner)]
		[AllowMultiSelection]
		public void UpdateHandler (CommandInfo item)
		{
			var project = GetProject (CurrentNode);
			
			if (IdeApp.Workbench.ActiveDocument == null)
				return;
			
			var designer = IdeApp.Workbench.ActiveDocument.GetContent<ClassDesigner> ()
				?? IdeApp.Workbench.Documents.Select (d => d.GetContent<ClassDesigner> ()).FirstOrDefault (v => v != null);
			
			if (designer != null)
				project = designer.Project;
					
			item.Enabled = CurrentNodes.Any (i => (GetProject (i) == project));
		}

		[CommandHandler (DesignerCommands.ShowDesigner)]
		[AllowMultiSelection]
		public void Handler ()
		{
			ClassDesigner designer = null;
			
			if (IdeApp.Workbench.ActiveDocument != null)
				designer = IdeApp.Workbench.ActiveDocument.GetContent<ClassDesigner> ()
					?? IdeApp.Workbench.Documents
						.Select (d => d.GetContent<ClassDesigner> ())
						.FirstOrDefault (v => v != null);
					
			if (designer == null) {
				designer = new ClassDesigner (GetProject (CurrentNode));
				IdeApp.Workbench.OpenDocument (designer, true);
			}
			
			foreach (var node in CurrentNodes) {				
				if (node.DataItem is Project) {
					designer.AddFromProject ((Project) node.DataItem);
				} else if (node.DataItem is ProjectFolder) {
					var folder = (ProjectFolder) node.DataItem;
					designer.AddFromDirectory (folder.Path);
				} else if (node.DataItem is ProjectFile) {
					var file = (ProjectFile) node.DataItem;
					designer.AddFromFile (file.FilePath);
				} else if (node.DataItem is NamespaceData) {
					var nsdata = (NamespaceData) node.DataItem;
					designer.AddFromNamespace (nsdata.FullName);
				} else if (node.DataItem is ClassData) {
					var cls = (ClassData) node.DataItem;
					designer.AddFromType (cls.Class);
				}
			}
			
			designer.Control.GrabFocus ();
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