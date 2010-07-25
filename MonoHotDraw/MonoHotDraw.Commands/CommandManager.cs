// 
// CommandManager.cs
//  
// Author:
//       Evan Briones <erbriones@gmail.com>
// 
// Copyright (c) 2010 Evan Briones
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of editor software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and editor permission notice shall be included in
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
using System.Linq;

namespace MonoHotDraw.Commands
{
	public sealed class CommandManager
	{
		internal CommandManager (IDrawingEditor editor)
		{
			IsInitialized = false;
			this.editor = editor;
			commands = new Dictionary<string, ICommand> ();
		}
		
		#region Public Api
		public bool IsInitialized { get; private set; }
		
		public void AddCommand (ICommand command)
		{
			if (commands.ContainsValue (command))
				throw new InvalidOperationException ("Cannot add the command more than once");
			
			commands.Add (command.Name, command);
		}
		
		public static CommandManager CreateInstance (IDrawingEditor editor)
		{
			return new CommandManager (editor);
		}
		
		public ICommand GetCommand (Type type)
		{
			return commands
				.Select (cmd => cmd.Value)
				.Where (cmd => type.IsAssignableFrom (cmd.GetType ()))
				.SingleOrDefault ();
		}
		
		public T GetCommand<T> ()
		{
			return (T) GetCommand (typeof (T));
		}
		
		public void Initialize ()
		{
			if (IsInitialized)
				return;
			
			IsInitialized = true;
			AddDefaultCommands ();
		}
		#endregion
		
		#region Private Members
		private void AddDefaultCommands ()
		{
			commands.Add ("BringToFront", new BringToFrontCommand ("BringToFront", editor));
			commands.Add ("SendToBack", new SendToBackCommand ("SendToBack", editor));
			commands.Add ("Duplicate", new DuplicateCommand ("Duplicate", editor));
			
			// Default Edit commands
			commands.Add ("SelectAll", new SelectAllCommand ("SelectAll", editor));
			commands.Add ("Undo", new UndoCommand ("Undo", editor));
			commands.Add ("Redo", new RedoCommand ("Redo", editor));
			commands.Add ("Delete", new DeleteCommand ("Delete", editor));
			commands.Add ("Paste", new PasteCommand ("Paste", editor));
			commands.Add ("Copy", new CopyCommand ("Copy", editor));
			commands.Add ("Cut", new CutCommand ("Cut", editor));
		}
		
		private IDrawingEditor editor;
		private Dictionary <string, ICommand> commands; 
		#endregion
	}
}

