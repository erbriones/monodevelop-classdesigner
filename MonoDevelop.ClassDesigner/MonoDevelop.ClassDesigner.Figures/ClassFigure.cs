// MonoDevelop ClassDesigner
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//  Evan Briones <erbriones@gmail.com>
//
// Copyright (C) 2009 Manuel Cerón
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
using System.Linq;
using MonoHotDraw.Commands;
using MonoHotDraw.Figures;
using MonoDevelop.Core;
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.ClassDesigner.Figures
{	
	public sealed class ClassFigure: TypeFigure, IAssociation, INestedTypeSupport
	{
		List<IFigure> nestedFigures;
		List<AssociationConnectionFigure> associations;
		bool hideInheritance;
		bool hideAssociations;
		bool hideCollection;
		
		public ClassFigure (IType domType) : base (domType)
		{
			hideInheritance = false;
			HideAssociations = false;
			nestedFigures = new List<IFigure> ();
			FillColor = new Cairo.Color (0.1, 0.1, 0.9, 0.4);
		}
		
		public bool HideInheritance {
			get { return hideInheritance; }
			set { hideInheritance = value; }
		}
		
		// FIXME need to probably add to compartment
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
		
		#region IAssociation
		public bool HideAssociations {
			get { return hideAssociations; }
			set {
				if (hideAssociations == value)
					return;
				
				hideAssociations = value;
				
				if (hideAssociations) {
					associations.ForEach ((a) => {
						if (a.Type == ConnectionType.Association)
							a.Hide ();
					});
				} else {
					associations.ForEach ((a) => {
						if (a.Type == ConnectionType.Association)
							a.Show ();
					});
				}
			}
		}

		public bool HideCollectionAssocations {
			get { return hideCollection; }
			set {
				if (hideCollection == value)
					return;
				
				hideCollection = value;
				
				if (hideCollection) {
					associations.ForEach ((a) => {
						if (a.Type == ConnectionType.CollectionAssociation)
							a.Hide ();
					});
				} else {
					associations.ForEach ((a) => {
						if (a.Type == ConnectionType.CollectionAssociation)
							a.Show ();
					});
				}
			}
		}

		public IEnumerable<IFigure> AssociationFigures {
			get {
				throw new NotImplementedException ();
			}
		}

		public void AddAssociation (IBaseMember member, IFigure associatedFigure, bool AsCollection)
		{
			AssociationConnectionFigure association; 
			
			if (AsCollection)
				association = new AssociationConnectionFigure (member, ConnectionType.CollectionAssociation,
				                                               this, associatedFigure);
			else
				association = new AssociationConnectionFigure (member, ConnectionType.Association,
				                                               this, associatedFigure);
				associations.Add (association);
		}

		public void RemoveAssociation (IBaseMember member)
		{
			
		}
		#endregion
		
		protected override ClassType ClassType {
			get {
				return ClassType.Class;
			}
		}	
	}
}
