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
		bool manual_label_position;
		bool manual_label_size;
		HStackFigure member_label;
		ImageFigure image;
		TextFigure name;
		
		public AssociationConnectionFigure (IBaseMember memberName,
		                                    ConnectionType connectionType,
		                                    IFigure startFigure,
		                                    IFigure endFigure)
		{
			if (connectionType == ConnectionType.Inheritance)
				throw new ArgumentException ("Connection must be of type association or collection");
					
			manual_label_size = false;
			manual_label_position = false;
			
			ConnectionLine = new AssociationLine ();
			//if (ConnectionLine.CanConnectStart (startFigure) && ConnectionLine.CanConnectEnd (endFigure))
			//	throw new ArgumentException ("One or more of the figures is not valid");
			
			var pixbuf = ImageService.GetPixbuf (memberName.StockIcon, IconSize.Menu);
			
			image = new ImageFigure (pixbuf);
			name = new TextFigure (memberName.Name);
			member_label = new HStackFigure ();
			member_label.Add (image);
			member_label.Add (name);
			
			ConnectionLine.ConnectStart (startFigure.ConnectorAt (0.0, 0.0));
			ConnectionLine.ConnectEnd (endFigure.ConnectorAt (0.0, 0.0));
			
			Type = connectionType;
			
			if (Type == ConnectionType.CollectionAssociation)
				throw new NotImplementedException ();
			
			Add (MemberLabel);
			Add (ConnectionLine);
			
			MemberLabel.MoveTo (ConnectionLine.EndPoint.X - 10.0, ConnectionLine.EndPoint.Y + 10.0);
		}
		
		HStackFigure MemberLabel {
			get { return member_label; }
		}
	}
}

