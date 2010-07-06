// 
// StructFigure.cs
//  
// Author:
//       Evan Briones <erbriones@gmail.com>
// 
// Copyright (C) 2010 Evan Briones
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
using MonoDevelop.Core;
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Figures
{

	public sealed class StructFigure : TypeFigure, IAssociation, INestedTypeSupport
	{
		bool hideAssociations;
		List<IFigure> nestedFigures;
		
		public StructFigure (IType domType) : base(domType)
		{
			FigureColor = new Cairo.Color (0.0, 0.2, 0.9, 0.4);
			nestedFigures = new List<IFigure> ();
			HideAssociations = false;
		}
		
		protected override ClassType ClassType {
			get { return ClassType.Struct; }
		}
	
		#region IAssociation
		public bool HideAssociations {
			get { return hideAssociations; }
			set {
				if (hideAssociations == value)
					return;
				
				hideAssociations = value;
				
			}
		}

		public bool HideCollectionAssocations {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public IEnumerable<IFigure> AssociationFigures {
			get {
				throw new NotImplementedException ();
			}
		}

		public void AddAssociation (IBaseMember memberInfo, IFigure associatedFigure, bool AsCollection)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAssociation (IBaseMember memberInfo)
		{
			throw new NotImplementedException ();
		}

		#endregion

		//FIXME: Add to compartment correctly
		#region INestedTypeSupport implementation
		public void AddNestedType (IFigure figure)
		{
			nestedFigures.Add (figure);
		}

		public void RemoveNestedType (IFigure figure)
		{
			nestedFigures.Remove (figure);
		}

		public IEnumerable<IFigure> NestedTypes {
			get { return nestedFigures; }
		}
		#endregion
	}
}
