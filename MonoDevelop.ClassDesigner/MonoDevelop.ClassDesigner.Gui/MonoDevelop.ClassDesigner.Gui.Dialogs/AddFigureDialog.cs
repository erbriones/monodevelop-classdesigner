// 
// AddFigureDialog.cs
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

using Cairo;
using Gtk;

using System;

using MonoHotDraw;
using MonoHotDraw.Figures;
using MonoDevelop.Diagram;
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.ClassDesigner.Gui.Toolbox;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Gui.Dialogs
{
	public partial class AddFigureDialog : Gtk.Dialog
	{
		ClassType type;
		ClassDesigner designer;
		PointD point;
		bool isAbstract;
		
		public AddFigureDialog (string typeName, ClassType type, PointD point, ClassDesigner designer)
		{
			this.Build ();
			this.designer = designer;
			this.point = point;
			this.type = type;

			Title = String.Format ("Add new {0}", typeName);
			typeLabel.Text = String.Format ("{0} name:", typeName);
				
			accessModifier.InsertText (0, "Default");
			accessModifier.InsertText (1, "public");
			accessModifier.InsertText (2, "internal");
		}
		
		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			Destroy ();
		}
		
		protected virtual void OnCreateNewToggled (object sender, System.EventArgs e)
		{
			if (existing.Active) {
				newFileName.Sensitive = false;
				existingFileName.Sensitive = true;
				fileChooser.Sensitive = true;
			} else {
				newFileName.Sensitive = true;
				existingFileName.Sensitive = false;
				fileChooser.Sensitive = false;
			}
		}
		
		protected virtual void OnFileChooserSelectionChanged (object sender, EventArgs e)
		{
			existingFileName.Text = fileChooser.Filename;
		}
		
		protected virtual void OnAccessModifierChanged (object sender, EventArgs e)
		{
		}
		
		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			if (String.IsNullOrEmpty (typeName.Text)) {
				typeName.GrabFocus ();
				return;
			}
			
			if (newFileName.Sensitive && String.IsNullOrEmpty (newFileName.Text)) {
				newFileName.GrabFocus ();
				return;
			} else if (existingFileName.Sensitive && String.IsNullOrEmpty (existingFileName.Text)) {
				existingFileName.GrabFocus ();
				return;
			}
	
			var item = new DomType (typeName.Text);
			item.ClassType = type;
			
			if (accessModifier.ActiveText == "public")
				item.Modifiers = Modifiers.Public; 
		
			//if (isAbstract)
			//	type.Modifiers &= Modifiers.Abstract;
			
			var figure = designer.Diagram.CreateFigure (item);
	
			if (figure == null) {
				typeName.GrabFocus ();
				return;
			}		
		
			figure.MoveTo (point.X, point.Y);
			designer.View.Add (figure);
			Destroy ();
		}
	}
}

