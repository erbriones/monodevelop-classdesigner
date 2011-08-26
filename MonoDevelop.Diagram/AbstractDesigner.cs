// 
// Diagram.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mono.Addins;

using MonoHotDraw;
using MonoHotDraw.Commands;
using MonoHotDraw.Figures;
using MonoHotDraw.Tools;

using MonoDevelop.Components.Commands;
using MonoDevelop.DesignerSupport;
using MonoDevelop.DesignerSupport.Toolbox; 
using MonoDevelop.Diagram.Components;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;

namespace MonoDevelop.Diagram
{
	public abstract class AbstractDesigner : AbstractViewContent, IDrawingEditor,
		IToolboxDynamicProvider, IToolboxConsumer, IZoomable, IPrintable, ICommandDelegatorRouter,
		IPropertyPadProvider
	{
		ScrolledWindow window;
		ITool tool;

		protected AbstractDesigner (int undoBufferSize)
		{
			tool = new SelectionTool (this);
			View = new StandardDrawingView (this);
			UndoBufferSize = undoBufferSize;
					
			UndoManager = new UndoManager (UndoBufferSize);
			UndoManager.StackChanged += delegate {
				OnStackChanged ();
			};

			CommandList = new List<ICommand> ();
			RebuildCommandList ();
			
			window = new ScrolledWindow ();
			window.Add ((Widget) View);
			window.ShowAll ();
		}
		
		#region Public Api
		public event EventHandler UndoStackChanged;
		
		public ProjectDom Dom {
			get { return ProjectDomService.GetProjectDom (this.Project); }
		}
		
		public PointD PointerToDrawing {
			get { 
				int x, y;
				Control.GetPointer (out x, out y);
				return View.ViewToDrawing (x, y);
			}
		}
		
		public abstract void AddFromFile (string file);
		public abstract void AddFromProject (Project project);
		
		public virtual void AutoLayout ()
		{	
			var spacing = 50.0;
			var rowHeight = 0.0;
			var x = 50.0;
			var y = 50.0;
			
			foreach (Figure figure in View.Drawing.Children) {
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
		#endregion
		
		#region AbstractViewContent implemented members
		public override Widget Control {
			get { return window; }
		}

		public override string StockIconId {
			get { return Gtk.Stock.Convert; }
		}
		#endregion
		
		#region Inheritable Members
		protected int UndoBufferSize { get; set; }
		
		protected virtual void OnStackChanged ()
		{
			var handler = UndoStackChanged;
			
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
		
		protected void OnItemsChanged ()
		{
			var handler = ItemsChanged;
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
		#endregion
		
		#region IDrawingEditor implementation
		public abstract void DisplayMenu (Figure figure, MouseEvent ev);
		
		public ITool Tool {
			get { return tool; }
			set {
				if (value == null)
					return;
				
				if (tool.Activated)
					tool.Deactivate ();
				
				tool = value;
				tool.Activate ();
			}
		}
		
		public IDrawingView View { get; set; }
		public UndoManager UndoManager { get; private set; }
		public MonoHotDraw.Commands.CommandManager CommandManager { get; private set; }
		#endregion
		
		#region IPrintable implementation
		public void PrintDocument (PrintingSettings settings)
		{
			throw new NotImplementedException ();
		}

		public void PrintPreviewDocument (PrintingSettings settings)
		{
			throw new NotImplementedException ();
		}

		public bool CanPrint {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
		
		#region IZoomable implementation
		public bool EnableZoomIn {
			get { return View.Scale < View.ScaleRange.Maximum; }
		}

		public bool EnableZoomOut {
			get { return View.Scale > View.ScaleRange.Minimum; }
		}

		public bool EnableZoomReset {
			get { return View.Scale != 1; }
		}
		
		public void ZoomIn ()
		{
			View.Scale += View.ScaleRange.Step;
		}
		
		public void ZoomOut ()
		{
			View.Scale -= View.ScaleRange.Step;
		}
		
		public void ZoomReset ()
		{
			View.Scale = 1;
		}		
		#endregion

		#region IPropertyPadProvider implementation
		public virtual object GetActiveComponent ()
		{
			if (View.SelectionCount == 0)
				return null;
			
			return View.SelectionEnumerator.Where (f => f is IPropertyProvider);
		}
		
		public virtual object GetProvider ()
		{
			return null;
		}
		
		public virtual void OnEndEditing (object obj)
		{
			return;
		}
		
		public virtual void OnChanged (object obj)
		{
			return;
		}
		
		#endregion
		
		#region Toolbox support
		public event EventHandler ItemsChanged;
		
		public abstract IEnumerable<ItemToolboxNode> GetDynamicItems (IToolboxConsumer consumer);
		public abstract void ConsumeItem (ItemToolboxNode item);				

		public bool CustomFilterSupports (ItemToolboxNode item)
		{
			return false;
		}

		public virtual System.ComponentModel.ToolboxItemFilterAttribute[] ToolboxFilterAttributes {
			get { return new System.ComponentModel.ToolboxItemFilterAttribute [] {}; }
		}

		public virtual string DefaultItemDomain {
			get { return "Diagram"; }
		}
		#endregion
		
		#region Drag and Drop Support
		ItemToolboxNode drag_item;
		Widget _source;
		
		public TargetEntry[] DragTargets {
			get { return StandardDrawingView.Targets; }
		}
		
		public void DragItem (ItemToolboxNode item, Widget source, Gdk.DragContext ctx)
		{
			drag_item = item;
			_source = source;
			_source.DragDataGet += OnDragDataGet;
			_source.DragEnd += OnDragEnd;
		}

		void OnDragEnd (object sender, DragEndArgs args)
		{
			if (_source != null) {
				_source.DragDataGet -= OnDragDataGet;
				_source.DragEnd -= OnDragEnd;
				_source = null;
			}
		}
		
		void OnDragDataGet (object sender, DragDataGetArgs args)
		{
			if (drag_item == null)
				return;
			
			ConsumeItem (drag_item);
			drag_item = null;
		}
		#endregion
		
		#region Default Commands		
		[CommandHandler (DiagramCommands.BringToFront)]
		protected void BringToFront ()
		{
			var command = CommandList.Where (c => c.Name == "BringToFront").SingleOrDefault ();
			
			if (command.IsExecutable)
				command.Execute ();
		}
		
		[CommandHandler (DiagramCommands.Duplicate)]
		protected void Duplicate ()
		{
			var command = CommandList.Where (c => c.Name == "Duplicate").SingleOrDefault ();
			
			if (command.IsExecutable)
				command.Execute ();
		}
						
				
		[CommandHandler (DiagramCommands.SendToBack)]
		protected void SendToBack ()
		{
			var command = CommandList.Where (c => c.Name == "SendToBack").SingleOrDefault ();
			
			if (command.IsExecutable)
				command.Execute ();
		}

		[CommandUpdateHandler (DiagramCommands.BringToFront)]
		protected void UpdateBringToFront (CommandInfo info)
		{
			var command = CommandList.Where (c => c.Name == "BringToFront").SingleOrDefault ();
			info.Enabled = command.IsExecutable;
		}	

		[CommandUpdateHandler (DiagramCommands.Duplicate)]
		protected void UpdateDuplicate (CommandInfo info)
		{
			var command = CommandList.Where (c => c.Name == "Duplicate").SingleOrDefault ();
			info.Enabled = command.IsExecutable;
		}	

		
		[CommandUpdateHandler (DiagramCommands.SendToBack)]
		protected void UpdateSendToBack (CommandInfo info)
		{
			var command = CommandList.Where (c => c.Name == "SendToBack").SingleOrDefault ();
			info.Enabled = command.IsExecutable;
		}	
		#endregion
		
		#region Command System
		protected abstract string CommandHandlersPath { get; }
		
		protected IEnumerable<FigureCommandHandler> CommandHandlers {
			get { return AddinManager.GetExtensionObjects<FigureCommandHandler> (CommandHandlersPath); }
		}
		
		public List<ICommand> CommandList { get; private set; }
		
		public object GetNextCommandTarget ()
		{
			return null;
		}
		
		public object GetDelegatedCommandTarget ()
		{
			var targets = new ArrayList ();
			var figures = View.SelectionEnumerator.ToFigures ();
			FigureCommandTargetChain targetChain = null;
			FigureCommandTargetChain lastNode = null;
			
			//TODO: Fix this reference leak
			foreach (var c in CommandHandlers) {
				c.Designer = this;
			}
			
			var commands = CommandHandlers.Where (c => c.CanHandle (View.SelectionEnumerator));
			
			foreach (var c in commands) {
				var newNode = new  FigureCommandTargetChain (c, figures);
				if (lastNode == null) {
					targetChain = lastNode = newNode;
					continue;
				}
				
				lastNode.Next = newNode;
				lastNode = newNode;
			}
			
			if (targetChain != null)
				targets.Add (targetChain);
			
			if (targets.Count == 1)
				return targets [0];
			else if (targets.Count < 1)
				return new MulticastNodeRouter (targets);
			return null;
		}
		
		public virtual void RebuildCommandList ()
		{
			CommandList.Clear ();
			
			// Diagram specific commands
			CommandList.Add (new BringToFrontCommand ("BringToFront", this));
			CommandList.Add (new SendToBackCommand ("SendToBack", this));
			CommandList.Add (new DuplicateCommand ("Duplicate", this));
			
			// Default Edit commands
			CommandList.Add (new SelectAllCommand ("SelectAll", this));
			CommandList.Add (new UndoCommand ("Undo", this));
			CommandList.Add (new RedoCommand ("Redo", this));
			CommandList.Add (new DeleteCommand ("Delete", this));
			CommandList.Add (new PasteCommand ("Paste", this));
			CommandList.Add (new CopyCommand ("Copy", this));
			CommandList.Add (new CutCommand ("Cut", this));
		}
		
		class MulticastNodeRouter : IMultiCastCommandRouter
		{
			ArrayList targets;
			
			public MulticastNodeRouter (ArrayList targets)
			{
				this.targets = targets;
			}
			
			public IEnumerable GetCommandTargets ()
			{
				return targets;
			}
		}
		
		class FigureCommandTargetChain : ICommandDelegatorRouter
		{
			FigureCommandHandler target;
			FigureCollection selection;
			internal FigureCommandTargetChain Next;
		
			public FigureCommandTargetChain (FigureCommandHandler target,
											 FigureCollection selection)
			{
				this.target = target;
				this.selection = selection;
			}
			
			public object GetNextCommandTarget ()
			{
				return Next;
			}

			public object GetDelegatedCommandTarget ()
			{
				//target.SetSelection (selection);
				return target;
			}
		}
		#endregion
	}
}
