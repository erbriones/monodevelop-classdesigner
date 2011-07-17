// 
// ClassDiagramDisplayBinding.cs
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

using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

using System.IO;
using System;

namespace MonoDevelop.ClassDesigner
{

	public class ClassDiagramDisplayBinding : IViewDisplayBinding
	{
		#region IViewDisplayBinding implementation
		IViewContent IViewDisplayBinding.CreateContent (FilePath fileName, string mimeType, Project ownerProject)
		{
			return new ClassDesigner(fileName, ownerProject);
		}

		string IViewDisplayBinding.Name {
			get { return "Class Diagram"; }
		}
		#endregion

		#region IDisplayBinding implementation
		bool IDisplayBinding.CanHandle (FilePath fileName, string mimeType, Project ownerProject)
		{	
			return ownerProject != null && ((fileName.IsNotNull && fileName.HasExtension (".cd"))
					|| (mimeType != null && mimeType == "class-diagram"));
		}

		bool IDisplayBinding.CanUseAsDefault {
			get { return true; }
		}
		#endregion
	}
}
