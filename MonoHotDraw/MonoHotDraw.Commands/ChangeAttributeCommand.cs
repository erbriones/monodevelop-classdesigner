// MonoHotDraw. Diagramming Framework
//
// Authors:
//	Mario Carrión <mario@monouml.org>
//
// Copyright (C) 2006, 2007, 2008, 2009 MonoUML Team (http://www.monouml.org)
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
using MonoHotDraw.Figures;

namespace MonoHotDraw.Commands
{
	public class ChangeAttributeCommand : AbstractCommand
	{
		public ChangeAttributeCommand (string name, FigureAttribute attribute, object value, IDrawingEditor editor)
			: base (name, editor)
		{
			this.attribute = attribute;
			this.value = value;
		}
		
		#region Public Members
		public override bool IsExecutable {
			get { return DrawingView.SelectionCount > 0; }
		}

		public override void Execute ()
		{
			base.Execute ();

			UndoActivity = CreateUndoActivity ();
			UndoActivity.AffectedFigures = new FigureCollection (DrawingView.SelectionEnumerator);

			foreach (Figure figure in UndoActivity.AffectedFigures)
				figure.SetAttribute (attribute, value);
		}
		#endregion
		
		#region ChangeAttributeCommand Members
		protected override IUndoActivity CreateUndoActivity ()
		{
			return new ChangeAttributeUndoActivity (DrawingView, attribute, value);
		}
		
		private FigureAttribute attribute;
		private object value;
		#endregion
		
		#region UndoActivity
		class ChangeAttributeUndoActivity : AbstractUndoActivity
		{
			public ChangeAttributeUndoActivity (IDrawingView drawingView, FigureAttribute attribute, object value)
				: base (drawingView)
			{
				originalValues = new Dictionary<Figure, object> ();
				Undoable = true;
				Redoable = true;
				Attribute = attribute;
				Value = value;
			}
			
			public override IEnumerable<Figure> AffectedFigures {
				get { return base.AffectedFigures; }
				set { 
					base.AffectedFigures = value;
			
					foreach (Figure figure in AffectedFigures)
						SetOriginalValue (figure, figure.GetAttribute (Attribute));
				}
			}
			
			public FigureAttribute Attribute { get; set; }
			public object Value { get; set; }

			public override bool Undo ()
			{
				if (base.Undo () == false)
					return false;

				foreach (KeyValuePair<Figure, object> value in originalValues)
					value.Key.SetAttribute (Attribute, value.Value);

				return true;
			}

			public override bool Redo ()
			{
				if (Redoable == false)
					return false;

				foreach (KeyValuePair<Figure, object> value in originalValues)
					value.Key.SetAttribute (Attribute, Value);

				return true;
			}
			
			private void SetOriginalValue (Figure figure, object value)
			{
				if (value != null)
					originalValues [figure] = value;
			}
			
			private Dictionary<Figure, object> originalValues;
		}
		#endregion
	}
}