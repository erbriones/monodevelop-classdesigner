//
// FigureCommandHandler.cs
//
// Author:
//   Lluis Sanchez Gual
//	 Evan Briones
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Addins;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Components.Commands;
using MonoHotDraw.Figures;

namespace MonoDevelop.Diagram.Components
{
	//[MultiFigureSelectionHandlerAttribute]
	//[FigureCommandHandler.TransactedNodeHandler]
	public class FigureCommandHandler: ICommandRouter
	{
		object nextTarget;
//		CanDeleteFlags canDeleteFlags;
				
		[Flags]
		enum CanDeleteFlags {
			NotChecked = 0,
			Checked = 1,
			Single = 2,
			Multiple = 4
		}

		internal void SetNextTarget (object nextTarget)
		{
			this.nextTarget = nextTarget;
		}
		
		object ICommandRouter.GetNextCommandTarget ()
		{
			return nextTarget;
		}
		
		internal protected IEnumerable<IFigure> SelectedFigures {
			get { return Designer.View.SelectionEnumerator; }
		}
		
		internal protected AbstractDesigner Designer {
			get;
			set;
		}
		
		public virtual void RenameItem (string newName)
		{
		}
		
		public virtual void ActivateItem ()
		{
		}
		
		public virtual void ActivateMultipleItems ()
		{
			/*
			if (currentNodes.Length == 1)
				ActivateItem ();
			else {
				ITreeNavigator[] nodes = currentNodes;
				
				
				try {
					currentNodes = new ITreeNavigator [1];
					foreach (ITreeNavigator nod in nodes) {
						currentNodes [0] = nod;
						ActivateItem ();
					}
				} finally {
					currentNodes = nodes;
				}
			}
			*/
		}
		
		public virtual void OnItemSelected ()
		{
		}

		internal protected virtual bool MultipleFiguresSelected {
			get { return SelectedFigures.Count () > 1; }
		}
		
		public virtual bool CanHandle (IEnumerable<IFigure> figures)
		{
			return true;
		}
		
		public virtual bool CanHandle (IFigure figure)
		{
			return true;
		}
		/*
		[CommandUpdateHandler (EditCommands.Delete)]
		internal void CanDeleteCurrentItem (CommandInfo info)
		{
			info.Bypass = !CanDeleteMultipleItems ();
		}
		
		[CommandHandler (EditCommands.Delete)]
		[AllowMultiSelection]
		internal void DeleteCurrentItem ()
		{
			DeleteMultipleItems ();
		}
		
		bool CheckCanDeleteFlags (CanDeleteFlags flag)
		{
			/*
			// if DeleteItem or DeleteMultipleItems, we can assume that the delete
			// operation is supported. If it is not supported for a specific context,
			// then the CanDelete* method will be overriden, in which case this
			// CheckCanDeleteFlags won't be used.
			
			if (canDeleteFlags == CanDeleteFlags.NotChecked) {
				canDeleteFlags |= CanDeleteFlags.Checked;
				if (GetType().GetMethod ("DeleteItem").DeclaringType != typeof(NodeCommandHandler))
					canDeleteFlags |= CanDeleteFlags.Single;
				if (GetType().GetMethod ("DeleteMultipleItems").DeclaringType != typeof(NodeCommandHandler) &&
				    GetType().GetMethod ("CanDeleteItem").DeclaringType == typeof(NodeCommandHandler))
					canDeleteFlags |= CanDeleteFlags.Multiple;
			
			}
			return (canDeleteFlags & flag) != 0;
			return true;
		}
		
		public virtual bool CanDeleteItem ()
		{
			return CheckCanDeleteFlags (CanDeleteFlags.Single);
		}
		
		public virtual bool CanDeleteMultipleItems ()
		{
			/*
			if (CheckCanDeleteFlags (CanDeleteFlags.Multiple))
				return true;
			
			if (currentNodes.Length == 1)
				return CanDeleteItem ();
			else {
				ITreeNavigator[] nodes = currentNodes;
				try {
					currentNodes = new ITreeNavigator [1];
					foreach (ITreeNavigator nod in nodes) {
						currentNodes [0] = nod;
						if (!CanDeleteItem ())
							return false;
					}
				} finally {
					currentNodes = nodes;
				}
				return true;
			}
			return true;
		}
		
		public virtual void DeleteItem ()
		{
		}
		
		public virtual void DeleteMultipleItems ()
		{
			if (currentNodes.Length == 1)
				DeleteItem ();
			else {
				ITreeNavigator[] nodes = currentNodes;
				try {
					currentNodes = new ITreeNavigator [1];
					foreach (ITreeNavigator nod in nodes) {
						currentNodes [0] = nod;
						DeleteItem ();
					}
				} finally {
					currentNodes = nodes;
				}
			}
		}
		*/
		
		// FIXME: Should this be removed
		internal class TransactedNodeHandlerAttribute: CustomCommandTargetAttribute
		{
			protected override void Run (object target, Command cmd)
			{
				FigureCommandHandler handler = (FigureCommandHandler) target;
				if (handler.Designer == null) {
					base.Run (target, cmd);
					return;
				}
				try {
				//	nch.tree.LockUpdates ();
					base.Run (target, cmd);
				} finally {
				//	nch.tree.UnlockUpdates ();
				}
			}
	
			protected override void Run (object target, Command cmd, object data)
			{
				base.Run (target, cmd, data);
			}
		}
	}


	internal class MultiFigureSelectionHandlerAttribute: CustomCommandUpdaterAttribute
	{
		// If multiple nodes are selected and the method does not have the AllowMultiSelectionAttribute
		// attribute, disable the command.
		
		protected override void CommandUpdate (object target, CommandArrayInfo cinfo)
		{
			FigureCommandHandler nc = (FigureCommandHandler) target;
			base.CommandUpdate (target, cinfo);
			
			if (nc.MultipleFiguresSelected) {
				bool allowMultiArray = false;
				ICommandArrayUpdateHandler h = ((ICommandArrayUpdateHandler)this).Next;
				while (h != null) {
					if (h is AllowMultiSelectionAttribute) {
						allowMultiArray = true;
						break;
					}
					h = h.Next;
				}
				
				if (!allowMultiArray)
					cinfo.Clear ();
			}
		}
		
		protected override void CommandUpdate (object target, CommandInfo cinfo)
		{
			FigureCommandHandler nc = (FigureCommandHandler) target;
			
			base.CommandUpdate (target, cinfo);
			
			if (nc.MultipleFiguresSelected) {
				bool allowMulti = false;
				bool canSelect = true;
				ICommandUpdateHandler h = ((ICommandUpdateHandler)this).Next;
				
				//FIXME: How do I properly make sure that multi selection works.
				// 
				//canSelect = nc.CurrentFigureSelection.All (f => nc.CanHandle (f));
				while (h != null && canSelect) {
					if (h is AllowMultiSelectionAttribute) {
						allowMulti = true;
						break;
					}
					h = h.Next;
				}
				if (!allowMulti)
					cinfo.Enabled = false;				
			}
		}
	}

	public class AllowMultiSelectionAttribute: CustomCommandUpdaterAttribute
	{
	}
	
	public class AllowNoSelectionAttribute: CustomCommandUpdaterAttribute
	{
	}
}
