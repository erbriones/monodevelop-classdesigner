// 
// TypeFigureToolboxItemNode.cs
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
using MonoDevelop.DesignerSupport.Toolbox;
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Gui.Toolbox
{
	[System.ComponentModel.ToolboxItem(true)]
	public class FigureToolboxItemNode : ItemToolboxNode, IToolboxFigure
	{
		ClassType _classType;
		bool is_abstract;
		string _namespace;
		
		public FigureToolboxItemNode (string name, ClassType classType, bool isAbstract, Gdk.Pixbuf icon) : base ()
		{
			is_abstract = isAbstract;
			_classType = classType;
			Name = name;
			Icon = icon;
		}
		
		public string Namespace {
			get { return _namespace; }
		}
		
		public ClassType ClassType {
			get { return _classType; }
		}
		
		public bool IsAbstract {
			get { return is_abstract; }
		}
	}
}

