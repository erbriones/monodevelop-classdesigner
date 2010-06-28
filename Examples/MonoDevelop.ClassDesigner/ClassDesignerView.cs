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
using System.ComponentModel;
using System.Xml;
using Gdk;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.ClassDesigner;
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.ClassDesigner.Gui.Toolbox;
using MonoDevelop.DesignerSupport.Toolbox;
using MonoHotDraw;
using MonoHotDraw.Figures;
using MonoHotDraw.Tools;

namespace MonoDevelop.ClassDesigner
{	
	public class ClassDesignerView: AbstractViewContent, IToolboxDynamicProvider, IToolboxConsumer
	{
		Designer designer;
		ItemToolboxNode drag_item;
		Gtk.Widget _source;
		ToolboxList tools;
		
		public event EventHandler ItemsChanged;
		
		public ClassDesignerView () : this ((Project) null)
		{
		}

		public ClassDesignerView (Project project)
		{
			this.UntitledName = "ClassDiagram.cd";
			this.IsViewOnly = false;	
				
			designer = new Designer (project, new SteticComponent ());
			designer.Editor.View.VisibleAreaChanged += OnDiagramChanged;
			SetupTools ();
			Control.ShowAll ();
		}
		
		public ClassDesignerView (string fileName)
		{
			if (String.IsNullOrEmpty (fileName)) 
				throw new ArgumentNullException ();
			
			this.ContentName = fileName;
			
			designer = new Designer (IdeApp.Workspace.GetProjectContainingFile (fileName));
			designer.Project = this.Project;
			designer.Editor.View.VisibleAreaChanged += OnDiagramChanged;
			
			SetupTools ();
			Control.ShowAll ();
		}
		
		public override string StockIconId {
			get {
				return Gtk.Stock.Convert;
			}
		}
		
		public Designer Designer {
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
		
		public override Gtk.Widget Control {
			get {
				return (Gtk.Widget) designer.Editor;
			}
		}
		
		void OnDiagramChanged (object sender, EventArgs e)
		{
			IsDirty = true;
		}	
		
		#region Toolbox and DND support
		public IEnumerable<ItemToolboxNode> GetDynamicItems (IToolboxConsumer consumer)
		{
			foreach (ItemToolboxNode item in tools) {
				item.Category = GettextCatalog.GetString ("Class Diagram");
				yield return item;
			}
		}
		
		public void ConsumeItem (ItemToolboxNode item)
		{
			var figure = item as IToolboxFigure;
			
			if (figure == null)
				return;
			
			if (figure.ClassType == ClassType.Unknown) {
				//Designer.AddComment (String.Empty);
				return;
			}
			
			var type = new DomType (figure.Name);
			type.ClassType = figure.ClassType;
			
			if (figure.IsAbstract)
				type.Modifiers = Modifiers.Abstract;
			
			Designer.AddFromType (type);
		}
				
		public bool CustomFilterSupports (ItemToolboxNode item)
		{
			return false;
		}

		public Gtk.TargetEntry[] DragTargets {
			get {
				return StandardDrawingView.Targets;
			}
		}

		public ToolboxItemFilterAttribute[] ToolboxFilterAttributes {
			get { return new ToolboxItemFilterAttribute [] {}; }
		}

		public string DefaultItemDomain {
			get { return "Class Diagram"; }
		}

		#endregion
		
		#region Drag and Drop Support
		public void DragItem (ItemToolboxNode item, Gtk.Widget source, DragContext ctx)
		{
			//var connector = item as IToolboxConnector;
			
			_source = source;
			_source.DragDataGet += OnDragDataGet;
			_source.DragEnd += OnDragEnd;
		}

		void OnDragEnd (object o, Gtk.DragEndArgs args)
		{
			if (_source != null) {
				_source.DragDataGet -= OnDragDataGet;
				_source.DragEnd -= OnDragEnd;
				_source = null;
			}
		}
		
		void OnDragDataGet (object o, Gtk.DragDataGetArgs args)
		{
			if (drag_item == null)
				return;
			
			ConsumeItem (drag_item);
			drag_item = null;
		}
		#endregion
		
		void SetupTools ()
		{
			tools = new ToolboxList ();
			var icon = ImageService.GetPixbuf (Stock.TextFileIcon, Gtk.IconSize.SmallToolbar);
			tools.Add (new FigureToolboxItemNode ("Comment", ClassType.Unknown, true, icon));
			
			icon = ImageService.GetPixbuf (Stock.ProtectedClass, Gtk.IconSize.SmallToolbar);
			tools.Add (new FigureToolboxItemNode ("Abstract Class", ClassType.Class, true, icon));
			
			icon = ImageService.GetPixbuf (Stock.Class, Gtk.IconSize.SmallToolbar);
			tools.Add (new FigureToolboxItemNode ("Class", ClassType.Class, false, icon));
			
			icon = ImageService.GetPixbuf (Stock.Interface, Gtk.IconSize.SmallToolbar);
			tools.Add (new FigureToolboxItemNode ("Interface", ClassType.Interface, false, icon));
			
			icon = ImageService.GetPixbuf (Stock.Enum, Gtk.IconSize.SmallToolbar);
			tools.Add (new FigureToolboxItemNode ("Enum", ClassType.Enum, false, icon));
			
			icon = ImageService.GetPixbuf (Stock.Delegate, Gtk.IconSize.SmallToolbar);
			tools.Add (new FigureToolboxItemNode ("Delegate", ClassType.Delegate, false, icon));
			
			icon = ImageService.GetPixbuf (Stock.Struct, Gtk.IconSize.SmallToolbar);
			tools.Add (new FigureToolboxItemNode ("Struct", ClassType.Struct, false, icon));
			
			icon = ImageService.GetPixbuf (Stock.SplitWindow, Gtk.IconSize.SmallToolbar);
			tools.Add (new ConnectorToolboxItemNode ("Association", ConnectorType.Association, icon));
			
			icon = ImageService.GetPixbuf (Stock.MiscFiles, Gtk.IconSize.SmallToolbar);
			tools.Add (new ConnectorToolboxItemNode ("Inheritance", ConnectorType.Inheritance, icon));
		}
	}
}