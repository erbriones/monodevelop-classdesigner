// MonoDevelop ClassDesigner
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//
// Copyright (C) 2009 Manuel Cerón
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
using System.Collections.Generic;
using System.Xml;
using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.ClassDesigner.Figures;
using MonoHotDraw;
using MonoHotDraw.Figures;
using MonoHotDraw.Tools;

namespace MonoDevelop.ClassDesigner {
	
	public class ClassDesignerView: AbstractViewContent {
		SteticComponent mhdEditor;
		ClassDiagram diagram;
		
		public override string StockIconId {
			get {
				return Stock.Convert;
			}
		}
		
		public override void Load (string fileName)
		{
			Console.WriteLine ("Loading {0}", fileName);
			
			diagram.Load (XmlReader.Create (fileName));			
		}
		
		public override void Save ()
		{
			Save (ContentName);
		}

		public override void Save (string fileName)
		{	
			XmlWriter writer;
			
			lock (writer = XmlWriter.Create (fileName)) {
				diagram.Write (writer);
				
				writer.Flush ();
				writer.Close ();
			}
			
			ContentName = fileName;
			IsDirty = false;
		}

		public override bool IsFile {
			get {
				return true;
			}
		}
		
		public override Widget Control {
			get {
				return mhdEditor;
			}
		}

		public ClassDesignerView ()
		{
			IsViewOnly = false;
			mhdEditor = new SteticComponent();
			mhdEditor.ShowAll();

			Project project = IdeApp.ProjectOperations.CurrentSelectedProject;
			ProjectDom dom = ProjectDomService.GetProjectDom(project);
			
			diagram = new ClassDiagram (mhdEditor, dom);
			diagram.DiagramChanged += OnDiagramChanged;
			UntitledName = "Untitled1.cd";
		}
		
		public ClassDesignerView (string fileName) : this ()
		{
			ContentName = fileName;
		}
		
		public void Create ()
		{
			diagram.Create ();
		}
		
		
		void OnDiagramChanged (object sender, EventArgs e)
		{
			IsDirty = true;
		}
	}
}
