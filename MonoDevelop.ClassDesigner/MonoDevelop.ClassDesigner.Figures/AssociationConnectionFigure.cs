// 
// CollectionAssociationLine.cs
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

using Gtk;
using System;
using System.Linq;
using MonoDevelop.Ide;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoHotDraw.Figures;

namespace MonoDevelop.ClassDesigner.Figures
{
	public sealed class AssociationConnectionFigure : AbstractConnectionFigure
	{
		public AssociationConnectionFigure (ConnectionType connectionType)
		{
			ConnectionLine = new AssociationLine ();
			if (connectionType == ConnectionType.Inheritance)
				throw new ArgumentException ("Connection must be of type association or collection");
			
			Type = connectionType;
			
			Add (ConnectionLine);
			SetAttribute (FigureAttribute.Selectable, true);
		}
		
		public AssociationConnectionFigure (MemberFigure member,
		                                    ConnectionType connectionType,
		                                    IFigure startFigure,
		                                    IFigure endFigure) : this (connectionType)
		{
			if (!ConnectionLine.CanConnectStart (startFigure) && 
			    !ConnectionLine.CanConnectEnd (endFigure)) {
				return;
			}
			
			Member = member;
			
			ConnectionLine.ConnectStart (startFigure.ConnectorAt (0.0, 0.0));
			ConnectionLine.ConnectEnd (endFigure.ConnectorAt (0.0, 0.0));
			
			// FIXME: handle collection setup
			//
			//if (Type == ConnectionType.CollectionAssociation)
			//
			
			//FIXME: this isn't quite working yet - startFigure isn't the direct parent of Member...
			startFigure.Remove (Member);
			Add (Member);
			Member.MoveTo (ConnectionLine.EndPoint.X - 5.0, ConnectionLine.EndPoint.Y + 10.0);
			Member.SetAttribute (FigureAttribute.Draggable, true);
			Member.SetAttribute (FigureAttribute.Selectable, false);
		}
		
		public ConnectionType Type { get; private set; }
		public MemberFigure Member { get; private set; }
		
		public void Show ()
		{
			ConnectionLine.ConnectStart (ConnectionLine.StartConnector);
			ConnectionLine.ConnectEnd (ConnectionLine.EndConnector);
		}
		
		public void Hide ()
		{
			ConnectionLine.DisconnectStart ();
			ConnectionLine.DisconnectEnd ();
		}
	}
}

