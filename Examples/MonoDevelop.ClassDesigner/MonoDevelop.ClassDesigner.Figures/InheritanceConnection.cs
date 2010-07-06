// MonoDevelop ClassDesigner
//
// Authors:
//  Evan Briones <erbriones@gmail.com>
//	Manuel Cerón <ceronman@gmail.com>
//
// Copyright (C) 2010 Evan Briones
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
using MonoHotDraw;
using MonoHotDraw.Figures;

namespace MonoDevelop.ClassDesigner.Figures
{
	public class InheritanceConnectionFigure : AbstractConnectionFigure
	{
		ConnectionType type;
		
		public InheritanceConnectionFigure (IFigure subClass, IFigure superClass)
		{
			type = ConnectionType.Inheritance;
			ConnectionLine = new InheritanceLine ();
			
			if (!ConnectionLine.CanConnectEnd (subClass) && 
			    !ConnectionLine.CanConnectEnd (superClass))
				
			
			ConnectionLine.DisconnectEnd ();
			ConnectionLine.DisconnectStart ();
			
			ConnectionLine.ConnectStart (subClass.ConnectorAt (0.0, 0.0));
			ConnectionLine.ConnectEnd (superClass.ConnectorAt (0.0, 0.0));
		}
		
		public ConnectionType Type {
			get { return type; }
		}
		
		internal class InheritanceLine : LineConnection
		{
			public InheritanceLine () : base ()
			{
				EndTerminal = new TriangleArrowLineTerminal ();
			}
			
			public InheritanceLine (IFigure fig1, IFigure fig2) : base (fig1, fig2)
			{
				EndTerminal = new TriangleArrowLineTerminal ();
			}
			
			public override bool CanConnectEnd (IFigure figure)
			{
				if (!(figure is ClassFigure))
					return false;
				
				if (figure.Includes (StartFigure))
					return false;
				
				return true;
			}
			
			public override bool CanConnectStart (IFigure figure)
			{
				if (!(figure is ClassFigure))
					return false;
				
				if (figure.Includes (EndFigure))
					return false;
				
				return true;
			}
		}
	}
}
