// TODO: Check this.

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
using Cairo;
using MonoHotDraw.Figures;
using MonoHotDraw.Util;
using MonoHotDraw.Visitor;

namespace MonoHotDraw.Commands
{
	public class PasteCommand : FigureTransferCommand
	{

		public PasteCommand (string name, IDrawingEditor editor) : base (name, editor)
		{
		}
		
		protected override IUndoActivity CreateUndoActivity ()
		{
			return new PasteUndoActivity (DrawingView);
		}		

		#region Public Api
		public override bool IsExecutable {
			get { return Clipboard.GetInstance ().Contents != null; }
		}

		// FIXME: LastClick was removed
		// TODO: Check that clipboard contents are FigureCollection 
		public override void Execute ()
		{
//			base.Execute ();
//			FigureCollection figures = Clipboard.GetInstance ().Contents as FigureCollection;
//			if (figures == null)
//				return;
//				
//			UndoActivity = CreateUndoActivity ();
//			UndoActivity.AffectedFigures = figures;
//
//			PointD lastClick = DrawingView.LastClick;
//			RectangleD r     =  figures.GetBounds ();
//			
//			DrawingView.ClearSelection ();
//			
//			UndoActivity.AffectedFigures = InsertFigures (UndoActivity.AffectedFigures, lastClick.X - r.X, lastClick.Y - r.Y);
		}
		
		#endregion

		#region UndoActivity
		
		internal class PasteUndoActivity : AbstractUndoActivity
		{
			public PasteUndoActivity (IDrawingView drawingView) : base (drawingView)
			{
				Undoable = true;
				Redoable = true;
			}
			
			public override bool Redo ()
			{
				var visitor = new InsertIntoDrawingVisitor (DrawingView.Drawing);
				
				DrawingView.ClearSelection ();
				
				foreach (IFigure figure in AffectedFigures)
					figure.AcceptVisitor (visitor);
				
				DrawingView.AddToSelection (visitor.AddedFigures); // Create new selection with pasted figure

				return true;
			}

			public override bool Undo ()
			{	
				var visitor = new DeleteFromDrawingVisitor (DrawingView.Drawing);

				foreach (IFigure figure in AffectedFigures)
					figure.AcceptVisitor (visitor);
				
				// Remove pasted figures from selection 
				foreach (IFigure fig in visitor.DeletedFigures)
					DrawingView.RemoveFromSelection (fig);
						
				return true;
			}			
		}
		#endregion
	}
}
