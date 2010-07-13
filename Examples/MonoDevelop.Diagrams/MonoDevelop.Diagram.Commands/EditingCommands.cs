// 
// EditingCommands.cs
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

using System.Linq;
using System.Collections.Generic;

using MonoDevelop.Components.Commands;
using MonoDevelop.Diagram.Components;
using MonoDevelop.Ide.Commands;
using MonoHotDraw.Figures;

namespace MonoDevelop.Diagram.Commands
{
	public class EditingCommands : FigureCommandHandler
	{	
		#region Commands
		[CommandHandler (EditCommands.Copy)]
		protected void Copy ()
		{
			var command = Designer.CommandList.Where (c => c.Name == "Copy").SingleOrDefault ();
			
			if (command.IsExecutable)
				command.Execute ();
		}

		[CommandHandler (EditCommands.Cut)]
		protected void Cut ()
		{
			var command = Designer.CommandList.Where (c => c.Name == "Cut").SingleOrDefault ();
			
			if (command.IsExecutable)
				command.Execute ();
		}
		
		[CommandHandler (EditCommands.DeleteKey)]
		[CommandHandler (EditCommands.Delete)]
		protected void Delete ()
		{
			var command = Designer.CommandList.Where (c => c.Name == "Delete").SingleOrDefault ();
			
			if (command.IsExecutable)
				command.Execute ();
		}
		
		[CommandHandler (EditCommands.Paste)]
		protected void Paste ()
		{
			var command = Designer.CommandList.Where (c => c.Name == "Paste").SingleOrDefault ();
			
			if (command.IsExecutable)
				command.Execute ();
		}
		
				[CommandHandler (EditCommands.Redo)]
		protected void Redo ()
		{
			var command = Designer.CommandList.Where (c => c.Name == "Redo").SingleOrDefault ();
			
			if (command.IsExecutable)
				command.Execute ();
		}
		
		[CommandHandler (EditCommands.SelectAll)]
		protected void SelectAll ()
		{
			var command = Designer.CommandList.Where (c => c.Name == "SelectAll").SingleOrDefault ();
			
			if (command.IsExecutable)
				command.Execute ();			
		}
		[CommandHandler (EditCommands.Undo)]
		protected void Undo ()
		{
			var command = Designer.CommandList.Where (c => c.Name == "Undo").SingleOrDefault ();
		
			if (command.IsExecutable)
				command.Execute ();
		}

		[CommandUpdateHandler (EditCommands.Copy)]
		protected void UpdateCopy (CommandInfo info)
		{
			var command = Designer.CommandList.Where (c => c.Name == "Copy").SingleOrDefault ();
			info.Enabled = command.IsExecutable;
		}
		
		[CommandUpdateHandler (EditCommands.Cut)]
		protected void UpdateCut (CommandInfo info)
		{
			var command = Designer.CommandList.Where (c => c.Name == "Cut").SingleOrDefault ();			
			info.Enabled = command.IsExecutable;
		}
		
		[CommandUpdateHandler (EditCommands.DeleteKey)]
		[CommandUpdateHandler (EditCommands.Delete)]
		protected void UpdateDelete (CommandInfo info)
		{
			var command = Designer.CommandList.Where (c => c.Name == "Delete").SingleOrDefault ();
			info.Enabled = command.IsExecutable;
		}
		
		[CommandUpdateHandler (EditCommands.Paste)]
		protected void UpdatePaste (CommandInfo info)
		{
			var command = Designer.CommandList.Where (c => c.Name == "Paste").SingleOrDefault ();
			info.Enabled = command.IsExecutable;
		}
						
		[CommandUpdateHandler (EditCommands.Redo)]
		protected void UpdateRedo (CommandInfo info)
		{		
			var command = Designer.CommandList.Where (c => c.Name == "Redo").SingleOrDefault ();
			info.Enabled = command.IsExecutable;
		}

		[CommandUpdateHandler (EditCommands.SelectAll)]
		protected void UpdateSelectAll (CommandInfo info)
		{		
			info.Enabled = true;
			
			if (Designer.View.SelectionCount == 0)
				info.Visible = true;
			else
				info.Visible = false;
		}
		
		[CommandUpdateHandler (EditCommands.Undo)]
		protected void UpdateUndo (CommandInfo info)
		{
			var command = Designer.CommandList.Where (c => c.Name == "Undo").SingleOrDefault ();
			info.Enabled = command.IsExecutable;
		}
		#endregion
	}
}

