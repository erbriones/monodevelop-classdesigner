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
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using Gdk;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.Ide.Gui;
using MonoHotDraw;
using MonoHotDraw.Figures;
using MonoHotDraw.Util;

namespace MonoDevelop.ClassDesigner.Diagram
{
	public class ClassDiagram 
	{	
		int majorVersion;
		int minorVersion;
		MembersFormat membersFormat;
		GroupingSetting groupSetting;
		ProjectDom dom;
		SteticComponent editor;
		List<IFigure> figures;
		
		public ClassDiagram (SteticComponent mhdEditor, ProjectDom dom)
		{
			majorVersion = 1;
			minorVersion = 1;
			membersFormat = MembersFormat.FullSignature;
			groupSetting = GroupingSetting.Member;
			
			figures = new List<IFigure> ();
			this.dom = dom;
			editor = mhdEditor;
			
			mhdEditor.View.Drawing.FigureAdded += HandleFigureAdded;
			mhdEditor.View.Drawing.FigureRemoved += HandleFigureRemoved;
		}
				
		void AddConnections ()
		{
			ClassFigure subclass;
			ClassFigure superclass;
			
			foreach (IType type in dom.Types) {				
				if (type.ClassType == ClassType.Class) {
					
					subclass = GetClassFigure (type.Name);
					
					if (type.BaseType == null)
						superclass = null;
					else
						superclass = GetClassFigure (type.BaseType.Name);
					
					if (subclass != null && superclass != null) {
						InheritanceConnectionFigure connection = new InheritanceConnectionFigure (subclass, superclass);
						editor.View.Drawing.Add (connection);
					}
				}
			}
		}
		
		public void AddFromType (IType type)
		{
			TypeFigure figure = CreateFigure (type);
			Console.WriteLine ("AddFromType");
			
			if (figure == null) {
				Console.WriteLine ("Null Figure");
				return;
			}
			
			figures.Add (figure);					
			figure.Toggle ();
			editor.View.Drawing.Add (figure);
		}
		
		public void AddFromNamespace (string ns)
		{
			Console.WriteLine ("Namespace: {0}", ns);
			IList<IMember> members = dom.GetNamespaceContents (ns, false, true);
		
			if (members == null)
				return;
			
			foreach (IMember item in members) {	
				if (item.MemberType == MemberType.Namespace) {
					AddFromNamespace (item.FullName);
					continue;
				} else if (item.MemberType != MemberType.Type)
					continue; 
				
				AddFromType (dom.GetType (item.FullName));
			}
			
			((StandardDrawing) editor.View.Drawing).AutoLayout ();
		}
		
		public void AddFromFile (string fileName)
		{
			if (dom == null)
				Console.WriteLine ("Null Dom");
			
			ParsedDocument doc = ProjectDomService.ParseFile (dom, fileName);
			
			if (doc == null)
				return;
			
			foreach (IType type in doc.CompilationUnit.Types) {
				AddFromType (type);
				Console.WriteLine ("type {0}", type);
			}
			
			((StandardDrawing) editor.View.Drawing).AutoLayout ();
		}
		
		public void AddFromDirectory (string directory)
		{
			if (dom == null)
				Console.WriteLine ("null dom");
			
			foreach (ProjectFile file in dom.Project.Files) {
				if (file.FilePath.ParentDirectory == directory)
					AddFromFile (file.Name);
			}
			
			((StandardDrawing) editor.View.Drawing).AutoLayout ();
		}
		
		public void AddFromProject (Project project)
		{
			ProjectDom dom = ProjectDomService.GetProjectDom (project);
			
			foreach (IType type in dom.Types) {
				AddFromType (type);	
			}
			
			((StandardDrawing) editor.View.Drawing).AutoLayout ();
		}
		
		TypeFigure CreateFigure (IType type)
		{
			TypeFigure figure;
			
			if (type == null)
				return null;
			
			if (HasFigure (type.Name))
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
		
			return figure;
		}

		ClassFigure GetClassFigure (string name)
		{
			ClassFigure f;
			
			if (name == null)
				return null;
			
			foreach (IFigure figure in figures) {
				f = figure as ClassFigure;
				if (f == null)
					continue;
				
				if (f.Name.Name == name)
					return f;
			}
			
			return null;
		}
		
		bool HasFigure (string name)
		{
			foreach (IFigure figure in figures) {
				TypeFigure f = figure as TypeFigure;
				
				if (f == null)
					continue;
				
				if (f.Name.Name == name)
					return true;
			}
			
			return false;
		}

		public void Load (XmlReader reader)
		{
			XElement element;
			XAttribute attribute;
			reader.MoveToContent ();
			
			while (!reader.EOF) {
				Console.WriteLine ("node: {0} type {1}", reader.Name, reader.NodeType);
				if (reader.NodeType != XmlNodeType.Element) {
					reader.Read ();
					continue;
				}
					
				switch (reader.Name) {
				case "ClassDiagram": {
					reader.Read ();
					break;
				} case "Font": {
					element = XElement.Load (reader);
					attribute = element.Attribute ("Name");
					AttributeFigure.SetDefaultAttribute (FigureAttribute.FontFamily, attribute.Value);
					
					attribute = element.Attribute ("Size");
					AttributeFigure.SetDefaultAttribute (FigureAttribute.FontSize, Int32.Parse(attribute.Value));
					break;
				}
				case "Class":
				case "Struct":
				case "Enum":
				case "Interface":
				case "Delegate":
					LoadType (XElement.Load (reader));
					break;
				case "Comment": {
					LoadComment (XElement.Load (reader));
					break;
				} default:
					reader.Read ();
					break;
				}
			}
			
			AddConnections ();
		}
		
		void LoadComment (XElement element)
		{
			string text;
			IFigure figure;
			XAttribute attribute = element.Attribute ("CommentText");
			double x, y, width, height;
			x = 50.0;
			y = 75.0;
			
			if (attribute == null) {
				figure = new CommentFigure (String.Empty);
			} else {
				text = attribute.Value;
				figure = new CommentFigure (text);
			}
			
			foreach (XElement child in element.Elements ()) {
				if (child.Name == "Position") {
						x = SetAttribute (child.Attribute("X"), 2.0);
						x = InchesToPixels (x);
					
						y = SetAttribute (child.Attribute("Y"), 2.25);
						y = InchesToPixels (y);
					
						width = SetAttribute (child.Attribute("Width"), 0.0);						
						height = SetAttribute (child.Attribute("Height"), 0.0);
				}
			}			

			figures.Add (figure);
			editor.View.Drawing.Add (figure);
			figure.MoveTo (x, y);
		}
		
		void LoadType (XElement element)
		{
			string typeName;
			bool collapsed;
			bool hideInheritance;
			double x, y, width, height;
			x = y = width = height = 0.0;
			
			typeName = SetAttribute (element.Attribute ("Name"), "Unknown");	
			collapsed = SetAttribute (element.Attribute ("Collapsed"), false);
			hideInheritance = SetAttribute (element.Attribute ("HideInheritanceLine"), false);
			
			foreach (XElement child in element.Elements ()) {
				if (child.Name == "Position") {	
					x = SetAttribute (child.Attribute ("X"), 2.0);
					x = InchesToPixels (x);
					
					y = SetAttribute (child.Attribute ("Y"), 2.0);
					y = InchesToPixels (y);
					
					width = SetAttribute (child.Attribute ("Width"), 2.5);
					width = InchesToPixels (x);
					
					height = SetAttribute (child.Attribute ("Height"), 1.5);
					height = InchesToPixels (x);
				}
				
				if (child.Name == "ShowAsAssociation" || child.Name == "ShowAsCollectionAssociation") {
					continue;
				}
			}
			
			Console.WriteLine ("Name: {0} X:{1} Y:{2} width:{3}", typeName, x, y, width);
			
			IType type;
			TypeFigure figure = null;
			type = dom.GetType (typeName);
				
			if (type != null) {
				figure = CreateFigure (type);
			}
			
			if (figure == null)
				return;
			
			figures.Add (figure);
			
			if (!collapsed)
				figure.Toggle ();
			
			editor.View.Drawing.Add (figure);
			figure.MoveTo (x, y);
		}
		
		public void Write (XmlWriter writer)
		{
			writer.WriteStartDocument ();
			writer.WriteWhitespace("\n");
			writer.WriteStartElement ("ClassDiagram");
			writer.WriteAttributeString ("MajorVersion", majorVersion.ToString ());
			writer.WriteAttributeString ("MinorVersion", minorVersion.ToString ());
			writer.WriteAttributeString ("MembersFormat", membersFormat.ToString ());
			
			writer.WriteWhitespace("\n\t");
			writer.WriteStartElement ("Font");
			writer.WriteAttributeString ("Name", TypeFigure.GetDefaultAttribute(FigureAttribute.FontFamily).ToString ());
			writer.WriteAttributeString ("Size", TypeFigure.GetDefaultAttribute(FigureAttribute.FontSize).ToString ());
			writer.WriteEndElement (); // End Font
			
			foreach (IFigure figure in figures) {				
				writer.WriteWhitespace("\n\t");
				TypeFigure tf;
				
				if (figure is ClassFigure) {
					tf = figure as TypeFigure;
					writer.WriteStartElement ("Class");
				} else if (figure is StructFigure) {
					tf = figure as TypeFigure;
					writer.WriteStartElement ("Struct");
				} else if (figure is EnumFigure) {
					tf = figure as TypeFigure;
					writer.WriteStartElement ("Enum");
				} else if (figure is InterfaceFigure) {
					tf = figure as TypeFigure;
					writer.WriteStartElement ("Interface");
				} else if (figure is DelegateFigure) {
					tf = figure as TypeFigure;
					writer.WriteStartElement ("Delegate");
				} else {
					continue;
				}
				
				writer.WriteAttributeString ("Name", tf.Name.FullName);
				
				if (!tf.Expanded)
					writer.WriteAttributeString ("Collapsed", "true");
				
				// Add hiding inheritance
				//if (tf.Inheritance)
				//	writer.WriteAttributeString ("HideInheritanceLine", true);
				
				writer.WriteWhitespace("\n\t\t");
				writer.WriteStartElement ("Position");
				
				double x, y, width;
				x = PixelsToInches (figure.DisplayBox.X);
				y = PixelsToInches (figure.DisplayBox.Y);
				width = PixelsToInches (figure.DisplayBox.Width);
				
				writer.WriteAttributeString ("X", x.ToString ());
				writer.WriteAttributeString ("Y", y.ToString ());
				writer.WriteAttributeString ("Width", width.ToString ());
				writer.WriteEndElement (); // End Position
				writer.WriteWhitespace("\n\t");
				writer.WriteEndElement (); // End Type				
			}
			writer.WriteWhitespace("\n");
			writer.WriteEndElement (); // End ClassDiagram	
		}
		
		void HandleFigureAdded (object sender, FigureEventArgs e)
		{			
			Console.WriteLine ("Figure Added");
			
			if (!figures.Contains (e.Figure))
				figures.Add (e.Figure);	
		
			if (DiagramChanged == null)
				return;
			
			DiagramChanged (this, EventArgs.Empty);
		}
		
		void HandleFigureRemoved (object sender, FigureEventArgs e)
		{
			TypeFigure figure = e.Figure as TypeFigure;
			
			figures.Remove (figure);
			Console.WriteLine ("Figure Removed");

			if (DiagramChanged == null)
				return;
			
			DiagramChanged (this, EventArgs.Empty);
		}
		
		static double InchesToPixels (double inches)
		{
			if (Screen.Default.Resolution == -1)
				return inches * 60.0;
			
			return inches * Gdk.Screen.Default.Resolution;
		}

		static double PixelsToInches (double pixels)
		{
			if (Screen.Default.Resolution == -1)
				return pixels / 60.0;
			
			return pixels / Gdk.Screen.Default.Resolution;
		}
		
		static double SetAttribute (XAttribute attribute, double fallback)
		{
			if (attribute == null)
				return fallback;
			
			return Double.Parse (attribute.Value);
		}
		
		static bool SetAttribute (XAttribute attribute, bool fallback)
		{
			if (attribute == null)
				return fallback;
			
			return Boolean.Parse (attribute.Value);
		}
		
		static string SetAttribute (XAttribute attribute, string fallback)
		{
			if (attribute == null)
				return fallback;
			
			return attribute.Value;
		}
		
		public EventHandler DiagramChanged;
	}
}
