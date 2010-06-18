// 
// ClassDiagram.cs
//  
// Author:
//       Evan <erbriones@gmail.com>
// 
// Copyright (c) 2010 Evan
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
using System.Xml;
using System.Xml.Linq;
using Gdk;
using MonoDevelop.Core;
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Ide.Gui;
using MonoHotDraw;
using MonoHotDraw.Figures;
using MonoHotDraw.Util;

namespace MonoDevelop.ClassDesigner.Designer
{
	public class ClassDiagram 
	{	
		readonly int majorVersion;
		readonly int minorVersion;

		GroupingSetting groupSetting;		
		MembersFormat membersFormat;
		List<IFigure> figures;
		
		public ClassDiagram () : this (GroupingSetting.Member, MembersFormat.FullSignature)
		{
		}
		
		public ClassDiagram (GroupingSetting grouping, MembersFormat format): this (grouping, format, null)
		{
		}
		
		public ClassDiagram (GroupingSetting grouping, MembersFormat format, IEnumerable<IFigure> figures)
		{
			majorVersion = 1;
			minorVersion = 1;
			groupSetting = grouping;
			membersFormat = format;
			
			if (figures == null)
				this.figures = new List<IFigure> ();
			else
				this.figures = new List<IFigure> (figures);			
		}
		
		public MembersFormat Format {
			get { return membersFormat; }
			set {
				TypeFigure.format = membersFormat;
				
				if (membersFormat == value)
					return;
				
				membersFormat = value;
				figures.ForEach (f => ((TypeFigure) f).Update ());
			}	
		}
		
		public GroupingSetting Grouping {
			get { return groupSetting; }
			set {
				TypeFigure.grouping = value;

				if (groupSetting == value)
					return;
				
				groupSetting = value;
				figures.ForEach (f => ((TypeFigure) f).Update ());
			}
		}
		
		public IEnumerable<IFigure> Figures {
			get { return figures; }
		}
		
		bool HasFigure (string fullName)
		{		
			return figures.Where (f => f is TypeFigure).Any (tf => ((TypeFigure) tf).Name.FullName == fullName);
		}
		
		public IFigure CreateFigure (IType type)
		{
			TypeFigure figure;
			
			if (type == null)
				return null;
			
			if (HasFigure (type.FullName))
				return null;
			
			if (type.ClassType == ClassType.Class) {
				Console.WriteLine ("Adding Class");
				figure = new ClassFigure (type);
			} else if (type.ClassType == ClassType.Enum) {
				Console.WriteLine ("Adding Enum");
				figure = new EnumFigure (type);
			} else if (type.ClassType == ClassType.Interface) {
				Console.WriteLine ("Adding Interface");
				figure = new InterfaceFigure (type);
			} else if (type.ClassType == ClassType.Struct) {
				Console.WriteLine ("Adding Struct");
				figure = new StructFigure (type);
			} else if (type.ClassType == ClassType.Delegate) {
				Console.WriteLine ("Adding Delegate");
				figure = new DelegateFigure (type);
			} else {
				return null;
			}

			
			figure.Update ();
			figures.Add (figure);
			
			return figure;
		}
		
		public TypeFigure GetFigure (string fullName, Type type)
		{			
			if (fullName == null || type == null)
				return null;
			
			var figure = figures
				.Where (f => type.IsAssignableFrom (f.GetType ()))
				.Select (tf => ((TypeFigure) tf).Name.FullName == fullName);
			
			return (TypeFigure) figure;
		}
		
		public void Load (string fileName, ProjectDom dom)
		{
			var root = XElement.Load(fileName);
			IFigure figure = null;

			var grouping = root.Attributes ()
				.Where (a => a.Name == "GroupingSetting")
				.SingleOrDefault ();
			
			if (grouping != null) {
				Console.WriteLine ("Set Group {0}: ", grouping.Value);
				
				if (grouping.Value == GroupingSetting.Alphabetical.ToString ())
					Grouping = GroupingSetting.Alphabetical;
				else if (grouping.Value == GroupingSetting.Access.ToString ())
					Grouping = GroupingSetting.Access;
				else
					Grouping = GroupingSetting.Member;
			} else  {
				Grouping = GroupingSetting.Member;
			}
			
			var format = root.Attributes ()
				.Where (a => a.Name == "MembersFormat")
				.SingleOrDefault ();
			
			if (format != null) {
				if (format.Value == MembersFormat.NameAndType.ToString ())
					Format = MembersFormat.NameAndType;
				else if (format.Value == MembersFormat.Name.ToString ())
					Format = MembersFormat.Name;
				else
					Format = MembersFormat.FullSignature;
			}
			
			var font = root.Elements ()
				.Where (e => e.Name == "Font")
				.SingleOrDefault ();
			
			if (font != null) {
				var attribute = font.Attributes ()
					.Where (a => a.Name == "Name")
					.SingleOrDefault ();
			
				if (attribute != null)
					AttributeFigure.SetDefaultAttribute (FigureAttribute.FontFamily, attribute.Value);
				
				attribute = font.Attributes ()
					.Where (a => a.Name == "Size")
					.SingleOrDefault ();
				
				if (attribute != null)
					AttributeFigure.SetDefaultAttribute (FigureAttribute.FontSize, Double.Parse(attribute.Value));	
			}
			
			foreach (var element in root.Elements ()) {				
				switch (element.Name.LocalName) {
					case "Class":
					case "Struct":
					case "Enum":
					case "Interface":
					case "Delegate":
						LoadType (element, dom);
						break;
					case "Comment":
						LoadComment (element);
						break;
					default:
						break;
				}
			}			
		}
		
		
		void LoadComment (XElement element)
		{
			IFigure figure;
			
			//
			// Figure Attributes
			//
			
			var attribute = element.Attribute ("CommentText");
			
			if (attribute == null)
				figure = new CommentFigure (String.Empty);
			else
				figure = new CommentFigure (attribute.Value);
			
			figures.Add (figure);
			
			//
			// Position Element
			//
			
			var position = element.Elements ()
				.Where (e => e.Name == "Position")
				.SingleOrDefault ();
			
			PositionFigure (position, figure, true);
		}
				
		void LoadType (XElement element, ProjectDom dom)
		{	
			TypeFigure figure = null;
			
			//
			// Figure Attributes
			//
			
			var typeName = element.Attributes ()
				.Where (a => a.Name == "Name")
				.SingleOrDefault ();
			
			figure = (TypeFigure) CreateFigure (dom.GetType(typeName.Value));
			
			if (figure == null)
				return;
			
			if (figure is ClassFigure) {
				var cls = (ClassFigure) figure;
				var hideInheritance = element.Attributes ()
					.Where (a => a.Name == "HideInheritanceLine")
					.SingleOrDefault ();
				
				if (hideInheritance != null)
					cls.HideInheritance = Boolean.Parse (hideInheritance.Value);
			}
					
			var collapsed = element.Attributes ()
				.Where (a => a.Name == "Collapsed")
				.SingleOrDefault ();
			
			if (collapsed != null)
				figure.Collapsed = !Boolean.Parse (collapsed.Value);
	
			//
			// Position Element
			//
			
			var position = element.Elements ()
				.Where (e => e.Name == "Position")
				.SingleOrDefault ();
						
			PositionFigure (position, figure, false);
			
			//
			// Members Element
			//
			
			var members = element.Elements ()
				.Where (e => e.Name == "Members")
				.SingleOrDefault ();
			
			//
			// Associations Element
			//
			
			var associations = element.Elements ()
				.Where (e => e.Name == "ShowAsAssociation")
				.SingleOrDefault ();
			
			//
			// Association Collections Element
			//
			
			var collection = element.Elements ()
				.Where (e => e.Name == "ShowAsAssociationCollection")
				.SingleOrDefault ();
			
			//
			// Compartments Element
			//
			
			var compartments = element.Elements ()
				.Where (e => e.Name == "Compartments")
				.SingleOrDefault ();
			
			if (compartments != null) {							
				Console.WriteLine ("Compartments");
				foreach (var e in compartments.Elements ("Compartment")) {
					var name = e.Attribute ("Name");
					
					if (name == null)
						continue;
					
					var compartment = figure.Compartments
						.Where (c => c.Name == name.Value)
						.SingleOrDefault ();
					
					if (compartment == null)
						continue;
					
					collapsed = e.Attribute ("Collapsed");
					
					if (collapsed == null)
						continue;
					
					compartment.Collapsed = Boolean.Parse (collapsed.Value);
				}
			}
			//
			// NestedTypes Element
			//
			
			var nestedTypes = element.Elements ()
				.Where (e => e.Name == "NestedTypes")
				.SingleOrDefault ();
			
			}
		
		// A position must have an x, y and width attribute.
		static void PositionFigure (XElement position, IFigure figure, bool hasHeightAttribute)
		{
			if (position == null)
				return;
			
			try {
				var x = Double.Parse (position.Attributes ()
					.Where (a => a.Name == "X")
				    .SingleOrDefault ().Value
				);
				
				var y = Double.Parse (position.Attributes ()
					.Where (a => a.Name == "Y")
				    .SingleOrDefault ().Value
				);
				
				var width = Double.Parse (position.Attributes ()
					.Where (a => a.Name == "Width")
				    .SingleOrDefault ().Value
				);
				
				figure.MoveTo (x, y);
				var height = figure.DisplayBox.Height;
				
				if (hasHeightAttribute) {
					height = InchesToPixels (
						Double.Parse (position.Attributes ()
							.Where (a => a.Name == "Height")
						    .SingleOrDefault ().Value
					    )
					);
				}
					
				figure.DisplayBox = new RectangleD (InchesToPixels (x),
				                                    InchesToPixels (y),
				                                    InchesToPixels (width),
				                                    height);
			} catch {
				Console.WriteLine ("The position element is malformed.");	
			}
		}
		
		public void Write (string fileName)
		{
			var root = new XElement ("ClassDiagram",
				new XAttribute ("MajorVersion", majorVersion),
			    new XAttribute ("MinorVersion", minorVersion),
			   	new XAttribute ("MembersFormat", membersFormat.ToString ()),
			    new XElement ("Font",
			    	new XAttribute ("Name", AttributeFigure.GetDefaultAttribute(FigureAttribute.FontFamily)),
			        new XAttribute ("Size", AttributeFigure.GetDefaultAttribute(FigureAttribute.FontSize))
			    )
			);

			foreach (IFigure figure in figures) {
				XElement element;
				var tf = figure as TypeFigure;
				
				if (figure is ClassFigure)
					element = new XElement ("Class");
				else if (figure is StructFigure)
					element = new XElement ("Struct");
				else if (figure is EnumFigure)
					element = new XElement ("Enum");
				else if (figure is InterfaceFigure)
					element = new XElement ("Interface");
				else if (figure is DelegateFigure)
					element = new XElement ("Class");
				else if (figure is CommentFigure) {
					var comment = (CommentFigure) figure;
					
					element = new XElement ("Comment",
						new XAttribute ("CommentText", comment.Text)
					);
				} else 
					continue;
				
				if (tf != null) {
					element.Add (new XAttribute ("Name", ((TypeFigure) figure).Name.FullName));
						
					if (!tf.Expanded)
						element.Add (new XAttribute ("Collapsed", "true"));
				}
				
				if (figure is ClassFigure) {
					var cls = figure as ClassFigure;
					
					if (cls.HideInheritance)
						element.Add (new XAttribute ("HideInheritanceLine", "true"));
				}
				
				var position = new XElement ("Position",
						new XAttribute ("X", PixelsToInches (figure.DisplayBox.X)),
					    new XAttribute ("Y", PixelsToInches (figure.DisplayBox.Y)),
					    new XAttribute ("Width", PixelsToInches (figure.DisplayBox.Width))
				);
				
				if (figure is CommentFigure)
					position.Add (new XAttribute ("Height", PixelsToInches (figure.DisplayBox.Height)));
				
				element.Add (position);
				
				if (tf != null) {
					string name = null;
					var project = IdeApp.Workspace.GetProjectContainingFile (fileName);
					var dom = ProjectDomService.GetProjectDom (project);
					
					foreach (var pf in project.Files) {
						var parseDoc = ProjectDomService.GetParsedDocument (dom, pf.Name);
						
						if (!String.IsNullOrEmpty (name))
							break;
						
						if (parseDoc.CompilationUnit != null) {
							var exists = parseDoc.CompilationUnit.Types
								.Any (t => t.FullName == tf.Name.FullName);
							
							if (exists)
								name = pf.FilePath.FileName;
						}
					}
					
					var identifier = new XElement ("TypeIdentifier",
						new XElement ("HashCode", String.Format ("{0:X}", tf.GetHashCode ())),
					    new XElement ("FileName", name)
					);
					element.Add (identifier);
				}
				
				root.FirstNode.AddBeforeSelf (element);
			}

			root.Save (fileName);
		}
				
		public static double InchesToPixels (double inches)
		{
			if (Screen.Default.Resolution == -1)
				return inches * 60.0;
			
			return inches * Gdk.Screen.Default.Resolution;
		}

		public static double PixelsToInches (double pixels)
		{
			if (Screen.Default.Resolution == -1)
				return pixels / 60.0;
			
			return pixels / Gdk.Screen.Default.Resolution;
		}
				
		protected void OnMembersFormatChanged (DiagramEventArgs e)
		{
			var handler = MembersFormatChanged;
			
				if (handler != null)
					handler (this, e);
		}
		
		protected void OnGroupSettingChanged (DiagramEventArgs e)
		{
			var handler = GroupSettingChanged;
			
				if (handler != null)
					handler (this, e);
		}		
		
		public event EventHandler<DiagramEventArgs> MembersFormatChanged;
		public event EventHandler<DiagramEventArgs> GroupSettingChanged;
	}
}
