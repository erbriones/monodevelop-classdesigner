// 
// InterfaceFigure.cs
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
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Ide;
using MonoDevelop.Projects.Dom;
using MonoHotDraw.Figures;

namespace MonoDevelop.ClassDesigner.Figures
{
	public sealed class InterfaceFigure : TypeFigure, IAssociation
	{
		List<AssociationConnectionFigure> associations;
		bool hideAssociations;		
		bool hideCollection;
		
		public InterfaceFigure (IType domType) : base (domType)
		{
			hideCollection = false;
			hideAssociations = false;
			FigureColor = new Cairo.Color (0.8, 0.8, 0.8, 0.4);
		}

		protected override ClassType ClassType {
			get { return ClassType.Interface; }
		}
		
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

		public void AddAssociation (IBaseMember memberInfo, IFigure associatedFigure, bool AsCollection)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAssociation (IBaseMember memberInfo)
		{
			throw new NotImplementedException ();
		}

		#endregion
		
		public override void UpdateGroups ()
		{
			if (grouping != GroupingSetting.Access) {
				base.UpdateGroups ();
				return;
			}
			
			string name;
			
			if (Name.IsPublic)
				name = "Public";
			else if (Name.IsPrivate)
				name = "Private";
			else if (Name.IsPrivate)
				name = "Protected";
			else if (Name.IsInternal)
				name = "Internal";
			else
				name = "Protected Internal";
			
			var compartment = Compartments.Where (c => c.Name == name).SingleOrDefault ();
			var members = new List<IMemberFigure> ();
				
			
			members.AddRange (Compartments
			                  .Select (c => c.FiguresEnumerator.Where (m => m != null))
			                  .OfType<IMemberFigure> ());
			                  
			foreach (var c in Compartments) {
				RemoveMemberGroup (c);
				compartment.Clear ();
			}
			
			if (members.Count () == 0) {
				foreach (var m in Name.Members) {
					var icon = ImageService.GetPixbuf (m.StockIcon, IconSize.Menu);
					members.Add (new TypeMemberFigure (icon, m, false));
				}
			}
			
			compartment.AddMembers (members);
			AddMemberGroup (compartment);
		}

	}
}
