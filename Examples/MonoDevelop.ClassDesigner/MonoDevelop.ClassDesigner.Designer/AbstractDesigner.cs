	// 
// AbstractDesigner.cs
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
using System.Xml;
using System.Collections.Generic;
using Gtk;
using MonoDevelop.Core.Logging;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoHotDraw;
using MonoHotDraw.Figures;


namespace MonoDevelop.ClassDesigner.Designer 
{
	public abstract class AbstractDesigner : IZoomable, IPrintable
	{
		static readonly short spacing = 50;
		Project project;
		IDrawingEditor editor;

		protected AbstractDesigner () : this (new SteticComponent ())
		{
		}
		
		protected AbstractDesigner (IDrawingEditor editor)
		{
			this.editor = editor;
		}
		
		public virtual Project Project {
			get {
				return project;
			} set {
				if (value == null)
					return;
				
				project = value;
			}
		}

		public virtual IDrawingEditor Editor {
			get { return editor; }
			set {
				if (value == null)
					return;
				
				editor = value;
			}
		}
		
		protected IList<IFigure> Figures {
			get {
				return Editor.View.Drawing.FigureCollection;
			}
		}
		
		protected virtual ProjectDom GetProjectDom ()
		{
			return ProjectDomService.GetProjectDom (project);
		}

		public virtual void AutoLayout ()
		{
			var rowHeight = 0.0;
			var x = 50.0;
			var y = 50.0;
			var length = Figures.Count;
			
			foreach (IFigure figure in Figures) {
				if (length != Figures.Count) {
					return;
				}
			
				if (x > 1000) {
					x = 50.0;
					y += (rowHeight + spacing);
					rowHeight = 0.0;
				}
				
				figure.MoveTo (x, y);
				rowHeight = Math.Max (rowHeight, figure.DisplayBox.Height);
				x += (figure.DisplayBox.Width + spacing);
			}
		}
									
		public abstract void Load (string file);
		public abstract void AddFromFile (string file);
		public abstract void AddFromProject (Project project);
		
		#region IPrintable implementation
		public void PrintDocument ()
		{
			throw new System.NotImplementedException();
		}
		
		
		public void PrintPreviewDocument ()
		{
			throw new System.NotImplementedException();
		}
		
		#endregion
		
		#region IZoomable implementation
		public void ZoomIn ()
		{
			Editor.View.Scale += Editor.View.ScaleRange.Step;
		}
		
		public void ZoomOut ()
		{
			Editor.View.Scale -= Editor.View.ScaleRange.Step;
		}
		
		
		public void ZoomReset ()
		{
			Editor.View.Scale = 1;
		}
		
		public bool EnableZoomIn {
			get {
				return true;
			}
		}
		
		public bool EnableZoomOut {
			get {
				return true;
			}
		}
		
		public bool EnableZoomReset {
			get {
				return true;
			}
		}
		
		#endregion
	}
}
