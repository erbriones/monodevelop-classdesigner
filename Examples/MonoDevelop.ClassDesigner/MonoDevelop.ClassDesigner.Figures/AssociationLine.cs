// 
// AssociationLine.cs
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

using System;
using MonoDevelop.ClassDesigner.Figures;
using MonoHotDraw.Figures;

namespace MonoDevelop.ClassDesigner
{
	internal sealed class AssociationLine : LineConnection
	{
		bool route_manually;	
				
		internal AssociationLine () : base ()
		{
			route_manually = false;
			EndTerminal = new TriangleArrowLineTerminal (5.0, 10.0);
		}
		
		internal AssociationLine (IFigure fig1, IFigure fig2) : base (fig1, fig2)
		{
			route_manually = false;
			EndTerminal = new TriangleArrowLineTerminal (5.0, 10.0);
		}
		
		public bool RouteManually {
			get { return route_manually; }
			set {
				if (route_manually == value)
					return;
				
				route_manually = value;
			}
		}
		
		public override bool CanConnectStart (IFigure figure)
		{
			if (figure is CommentFigure)
				return false;
			else if (figure.Includes (EndFigure))
				return false;
			else if (figure is TypeFigure)
				return true;
			
			return false;
		}
		
		public override bool CanConnectEnd (IFigure figure)
		{
			if (figure is CommentFigure)
				return false;
			else if (figure is DelegateFigure)
				return false;
			else if (figure.Includes (StartFigure))
				return false;
			else if (figure is TypeFigure)
				return true;
			
			return false;
		}
	}
}

