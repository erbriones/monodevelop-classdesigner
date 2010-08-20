// 
// GroupFormatVisitor.cs
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

using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.Projects.Dom;

using MonoHotDraw;
using MonoHotDraw.Figures;
using MonoHotDraw.Handles;
using MonoHotDraw.Visitor;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.ClassDesigner.Visitor
{
	internal sealed class GroupFormatVisitor : IFigureVisitor
	{
		IDrawing drawing;
		TypeFigure typeFigure;
		
		public GroupFormatVisitor (IDrawing drawing, TypeFigure typeFigure)
		{
			this.drawing = drawing;
			this.typeFigure = typeFigure;
		}
		
		public TypeFigure TypeFigure {
			get {return typeFigure;}
			set {
				typeFigure = value;
				typeFigure.Clear ();
			}
		}
		
		#region IFigureVisitor implementation
		public void VisitFigure (IFigure figure)
		{			
			var diagram = (ClassDiagram) drawing;
			var compartment = figure as CompartmentFigure;
			
			if (compartment == null)
				return;
			
			if (diagram.Grouping == GroupingSetting.Alphabetical)
				GroupByAlphabetical (compartment);
			else if (diagram.Grouping == GroupingSetting.Member)
				GroupByAccess (compartment);
			else if (diagram.Grouping == GroupingSetting.Kind)
				GroupByKind (compartment);
		}

		public void VisitHandle (IHandle hostHandle)
		{
		}
		#endregion
		
		void GroupByAlphabetical (CompartmentFigure figure)
		{
			if (figure.Name != "Members") {
				TypeFigure.Remove (figure);
				return;
			}
			
			IEnumerable<IFigure> members = TypeFigure.Members.Values
				.OrderBy (m => (((MemberFigure) m).Name));
			
			Rebuild (figure, members);
		}
				
		void GroupByAccess (CompartmentFigure figure)
		{
			IEnumerable<MemberFigure> members = TypeFigure.Members.Values.OfType<MemberFigure> ();
			
			if (figure.Name == "Public" &&
 				(TypeFigure.Name.ClassType == ClassType.Delegate || 
				TypeFigure.Name.ClassType == ClassType.Interface ||
				TypeFigure.Name.ClassType == ClassType.Enum)) {
				
				Rebuild (figure, members.OfType<IFigure> ());
				return;
			}
			
			if (figure.Name == "Public")
				members = members.Where (m => m.MemberInfo.IsPublic);
			else if (figure.Name == "Private")
				members = members
					.Where (m => m.MemberInfo.IsPrivate || m.MemberInfo.IsDefault);
			else if (figure.Name == "Protected")
				members = members
					.Where (m => m.MemberInfo.IsProtected);
			else if (figure.Name == "Protected Internal")
				members = members.Where (m => m.MemberInfo.IsProtectedAndInternal);
			else if (figure.Name == "Internal")
				members = members.Where (m => m.MemberInfo.IsInternal);
			else {
				TypeFigure.Remove (figure);
				return;
			}
			
			Rebuild (figure, members.OfType<IFigure> ());
		}

		void GroupByKind (CompartmentFigure figure)
		{
			IEnumerable<MemberFigure> members = TypeFigure.Members.Values.OfType<MemberFigure> ();
			
			if (figure.Name == "Parameters")
				members = members.Where (m => (m.MemberInfo.MemberType == MemberType.Parameter));
			else if (figure.Name == "Fields")
				members = members.Where (m => (m.MemberInfo.MemberType == MemberType.Field));
			else if (figure.Name == "Properties")
				members = members.Where (m => (m.MemberInfo.MemberType == MemberType.Property));
			else if (figure.Name == "Methods")
				members = members.Where (m => (m.MemberInfo.MemberType == MemberType.Method));
			else if (figure.Name == "Events")
				members = members.Where (m => (m.MemberInfo.MemberType == MemberType.Event));
			else {
				TypeFigure.Remove (figure);
				return;
			}
				
			Rebuild (figure, members.OfType<IFigure> ());
		}
		
		void Rebuild (CompartmentFigure figure, IEnumerable<IFigure> members)
		{
			var removableMembers = figure.Figures.Where (f => members.Contains (f));
			figure.AddRange (members);
			figure.RemoveRange (removableMembers);
			TypeFigure.AddCompartment (figure);
		}
	}
}