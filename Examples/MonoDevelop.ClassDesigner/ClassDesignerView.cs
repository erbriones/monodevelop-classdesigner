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
using Gdk;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.ClassDesigner;
using Cls = MonoDevelop.ClassDesigner.Designer;
using MonoDevelop.ClassDesigner.Designer;
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.DesignerSupport.Toolbox;
using MonoHotDraw;
using MonoHotDraw.Figures;
using MonoHotDraw.Tools;

namespace MonoDevelop.ClassDesigner {
	
	public class ClassDesignerView: AbstractViewContent {
		Cls.Designer designer;
		
		public ClassDesignerView () : this ((Project) null)
		{
		}

		public ClassDesignerView (Project project)
		{
			this.UntitledName = "ClassDiagram.cd";
			this.IsViewOnly = false;
			
			designer = new Cls.Designer (project, new SteticComponent ());
			designer.Editor.View.VisibleAreaChanged += OnDiagramChanged;
			Control.ShowAll ();
		}
		
		public ClassDesignerView (string fileName)
		{
			if (String.IsNullOrEmpty (fileName)) 
				throw new ArgumentNullException ();
			
			this.ContentName = fileName;
			
			designer = new Cls.Designer (IdeApp.Workspace.GetProjectContainingFile (fileName));
			designer.Project = this.Project;
			designer.Editor.View.VisibleAreaChanged += OnDiagramChanged;
			Control.ShowAll ();
		}
		
		public override string StockIconId {
			get {
				return Stock.Convert;
			}
		}
		
		public Cls.Designer Designer {
			get { return designer; }
		}
		
		public override Project Project {
			get {
				return designer.Project;
			}
			set {
				designer.Project = value;
			}
		}
		
		public override void Load (string fileName)
		{
			Designer.Load (fileName);
			
			IsDirty = false;
		}
		
		public override void Save ()
		{
			Save (ContentName);
		}

		public override void Save (string fileName)
		{	
			lock (Designer.Diagram)
				Designer.Diagram.Write (fileName);
			
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
				return (Widget) designer.Editor;
			}
		}
		
		void OnDiagramChanged (object sender, EventArgs e)
		{
			IsDirty = true;
		}		
	}
}
