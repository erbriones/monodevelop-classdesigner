// 
// ClassDesigner.cs
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
using System.Xml.Linq;
using System.Collections.Generic;
using System.Xml;
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Ide.Gui;
using MonoHotDraw;
using MonoHotDraw.Tools;
using MonoHotDraw.Connectors;
using MonoHotDraw.Figures;


namespace MonoDevelop.ClassDesigner
{
	public class ClassDesigner : AbstractDesigner
	{
		ClassDiagram diagram;
		
		public ClassDesigner () : base ()
		{
			diagram = new ClassDiagram ();
		}
		
		public ClassDesigner (Project project) : this ()
		{
			this.Project = project;
		}
		
		public ClassDesigner (Project project, IDrawingEditor editor) : base (editor)
		{
			diagram = new ClassDiagram ();
			this.Project = project;
			AutoLayout ();
		}
		
		public ClassDiagram Diagram {
			get { return diagram; }
			set {
				if (value == null)
					return;
				
				diagram = value;
			}
		}
		
		public void AddInheritanceLines ()
		{
			ClassFigure subclass;
			ClassFigure superclass;
			var dom = GetProjectDom ();
			
			foreach (IType type in dom.Types) {				
				if (type.ClassType == ClassType.Class) {		
					subclass = Diagram.GetFigure (type.Name) as ClassFigure;
					
					if (subclass.HideInheritance)
						continue;
					
					if (type.BaseType == null)
						superclass = null;
					else
						superclass = Diagram.GetFigure (type.BaseType.Name) as ClassFigure;
					
					if (subclass != null && superclass != null) {
						var connection = new InheritanceConnectionFigure (subclass, superclass);
						Editor.View.Add (connection);
					}
				}
			}	
		}
		
		public void AddFromDirectory (string directory)
		{
			AddRange (Project.Files.Where(pf => pf.FilePath.IsChildPathOf (directory)).Select (file => file.Name));
		}
		
		public override void AddFromFile (string fileName)
		{
			var dom = GetProjectDom ();
			ParsedDocument doc = ProjectDomService.ParseFile (dom, fileName);			
			var file = Project.Files.GetFile (fileName);
			
			if (file.FilePath.Extension == ".cd") {
				Load (fileName);
				return;
			}
		
			if (doc == null)
				return;
			
			var compilationUnit = doc.CompilationUnit;
			
			if (compilationUnit == null)
				return;
			
			var figures = compilationUnit.Types.Select (t => Diagram.CreateFigure (t)).Where (t => t != null);
			Editor.View.AddRange (figures);
			
			AutoLayout ();	
		}
		
		public void AddFromNamespace (string ns)
		{	
			var dom = GetProjectDom ();
			IList<IMember> members = dom.GetNamespaceContents (ns, false, true);
			
			if (members == null) {
				Console.WriteLine ("In namespace: {0} members were not found.", ns);
				return;
			}
			
			foreach (IMember item in members) {	
				if (item.MemberType == MemberType.Namespace) {
					AddFromNamespace (item.FullName);
					continue;
				} else if (item.MemberType != MemberType.Type)
					continue; 
				
				AddFromType (dom.GetType (item.FullName));
			}
			
			AutoLayout ();	
		}
		
		public override void AddFromProject (Project project)
		{
			Project = project;
			var dom = GetProjectDom ();
		
			Editor.View.AddRange (dom.Types.Select (t => Diagram.CreateFigure (t)).Where (f => f != null));
			
			AutoLayout ();	
		}
 
		public void AddFromType (IType type)
		{
			var figure = Diagram.CreateFigure (type);
			
			if (figure == null)
				return;
			
			Editor.View.Add (figure);
		}
		
		void AddRange (IEnumerable<string> files)
		{
			foreach (string file in files)
				AddFromFile (file);
		}
	
		public override void Load (string fileName)
		{
			diagram.Load (fileName, GetProjectDom ());
		
			Editor.View.AddRange (diagram.Figures);
		}
		
		
		protected virtual void OnDiagramChanged (DiagramEventArgs e)
		{
			var handler = DiagramChanged;
				
			if (handler != null)
				handler (this, e);
		}
		
		public event EventHandler<DiagramEventArgs> DiagramChanged;
	}
}
