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
using MonoDevelop.Core;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.ClassDesigner.Figures;
using MonoHotDraw;
using MonoHotDraw.Figures;

namespace MonoDevelop.ClassDesigner
{
	public class ClassDiagram 
	{	
		int majorVersion;
		int minorVersion;
		MembersFormat membersFormat;
		ProjectDom dom;
		SteticComponent mhdEditor;
		List<TypeFigure> figures;

		enum MembersFormat
		{
			Name,
			NameAndType,
			FullSignature
		}
		
		public ClassDiagram (SteticComponent mhdEditor, ProjectDom dom)
		{
			majorVersion = 1;
			minorVersion = 1;
			this.mhdEditor = mhdEditor;
			this.dom = dom;
			figures = new List<TypeFigure> ();
			membersFormat = MembersFormat.FullSignature;
			
			mhdEditor.View.Drawing.FigureAdded += HandleFigureAdded;
			mhdEditor.View.Drawing.FigureRemoved += HandleFigureRemoved;
		}

		void AddConnections ()
		{
			foreach (IType type in dom.Types) {
				if (type.ClassType == ClassType.Class) {
					ClassFigure subclass = GetFigure (type.Name);
					ClassFigure superclass = GetFigure (type.BaseType.Name);
					
					if (subclass != null && superclass != null) {
						InheritanceConnectionFigure connection = new InheritanceConnectionFigure (subclass, superclass);
						mhdEditor.View.Drawing.Add (connection);
					}
				}
			}
		}

		void AddConnections (List<IType> connections)
		{
			foreach (IType type in connections) {
				if (type.ClassType == ClassType.Class) {
					ClassFigure subclass = GetFigure (type.Name);
					ClassFigure superclass = GetFigure (type.BaseType.Name);
					
					if (subclass != null && superclass != null) {
						InheritanceConnectionFigure connection = new InheritanceConnectionFigure (subclass, superclass);
						mhdEditor.View.Drawing.Add (connection);
					}
				}
			}
		}

		void AddTypeFigure (XElement element, List<IType> connections)
		{
			string typeName;
			bool collapsed;
			bool hideInheritance;
			double x, y, width;
			x = y = width = 0.0;
			
			
			XAttribute attribute = element.Attribute ("Name");
			typeName = attribute.Value;
			
			// Type does not exist
			if (typeName == null)
				return;
			
			attribute = element.Attribute ("Collapsed");
			
			if (attribute == null)
				collapsed = false;
			else
				collapsed = Boolean.Parse (attribute.Value);
			
			
			attribute = element.Attribute("HideInheritanceLine");
			
			if (attribute == null)
				hideInheritance = false;
			else
				hideInheritance = Boolean.Parse (attribute.Value);	
			
			foreach (XElement child in element.Elements ()) {
				if (child.Name == "Position") {
					attribute = child.Attribute ("X");
					x = InchesToPixels (Double.Parse (attribute.Value));
					
					attribute = child.Attribute ("Y");
					y = InchesToPixels (Double.Parse (attribute.Value));
					
					attribute = child.Attribute ("Width");
					width = Double.Parse (attribute.Value);
				}
				
				if (child.Name == "ShowAsAssociation" || child.Name == "ShowAsCollectionAssociation") {
					connections.Add (dom.GetType (typeName));
				}
			}
			
			Console.WriteLine ("Name: {0} X:{1} Y:{2} width:{3}", typeName, x, y, width);
			
			IType type = dom.GetType (typeName);
			TypeFigure figure;
			
			figure = CreateFigure (type);
			
			if (figure == null)
				return;
			
			figures.Add (figure);
			
			if (!collapsed)
				figure.Toggle ();
			
			mhdEditor.View.Drawing.Add (figure);
			figure.MoveTo (x, y);
			figure.FigureChanged += HandleFigureChanged;
		}
		
		TypeFigure CreateFigure (IType type)
		{
			TypeFigure figure;
			
			if (type == null)
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

		public void Create ()
		{
			Create (true);
		}

		public void Create (bool collapse)
		{	
			double x = 50.0;
			double y = 50.0;
			
			foreach (IType type in dom.Types) {
				TypeFigure figure;
				
				figure = CreateFigure (type);
				
				if (figure == null)
					continue;
				
				figures.Add (figure);
												
			
				if (figure.Expanded && collapse)
					figure.Toggle ();
				
				mhdEditor.View.Drawing.Add (figure);
				figure.MoveTo (x, y);
				figure.FigureChanged += HandleFigureChanged;
				
				x += figure.DisplayBox.Width + 50.0;
				
				if (x > 1000.0) {
					x = 50.0;
					y += figure.DisplayBox.Height + 100.0;
				}
				
			}
			
			AddConnections ();
		}

		ClassFigure GetFigure (string name)
		{
			if (name == null)
				return null;
			
			foreach (TypeFigure figure in figures) {
				
				if (figure.Name.Name == name)
					return figure as ClassFigure;
			}
			return null;
		}

		public void Load (XmlReader reader)
		{
			List<IType> connections = new List<IType> ();
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
					AddTypeFigure (XElement.Load (reader), connections);
					break;
				default:
					reader.Read ();
					break;
				}
			}
			
			AddConnections (connections);
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
			
			foreach (TypeFigure tf in figures) {				
				writer.WriteWhitespace("\n\t");
				
				if (tf is ClassFigure) {
					writer.WriteStartElement ("Class");
				} else if (tf is StructFigure) {
					writer.WriteStartElement ("Struct");
				} else if (tf is EnumFigure) {
					writer.WriteStartElement ("Enum");
				} else if (tf is InterfaceFigure) {
					writer.WriteStartElement ("Interface");
				} else if (tf is DelegateFigure) {
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
				x = PixelsToInches (tf.DisplayBox.X);
				y = PixelsToInches (tf.DisplayBox.Y);
				width = PixelsToInches (tf.DisplayBox.Width);
				
				writer.WriteAttributeString ("X", x.ToString ());
				writer.WriteAttributeString ("Y", y.ToString ());
				writer.WriteAttributeString ("Width", width.ToString ());
				writer.WriteEndElement (); // End Position
				writer.WriteWhitespace("\n\t");
				writer.WriteEndElement (); // End Type				
				writer.WriteWhitespace("\n");
			}
			
			writer.WriteEndElement (); // End ClassDiagram	
		}
		
		void HandleFigureAdded (object sender, FigureEventArgs e)
		{			
			Console.WriteLine ("Figure Added");
			TypeFigure figure = e.Figure as TypeFigure;
			
			if (!figures.Contains (figure))
				figures.Add (figure);	
		
			if (DiagramChanged == null)
				return;
			
			DiagramChanged (this, EventArgs.Empty);
		}
		
		void HandleFigureChanged (object sender, FigureEventArgs e)
		{
			Console.WriteLine ("Figure Changed");
			if (DiagramChanged == null)
				return;
			
			DiagramChanged (this, EventArgs.Empty);
		}
		
		void HandleFigureRemoved (object sender, FigureEventArgs e)
		{
			e.Figure.FigureChanged -= HandleFigureChanged;
			TypeFigure figure = e.Figure as TypeFigure;
			
			figures.Remove (figure);
			Console.WriteLine ("Figure Removed");

			if (DiagramChanged == null)
				return;
			
			DiagramChanged (this, EventArgs.Empty);
		}
		
		double InchesToPixels (double inches)
		{
			if (mhdEditor.Screen.Resolution == -1)
				return inches * 60.0;
			
			return inches * mhdEditor.Screen.Resolution;
		}

		double PixelsToInches (double pixels)
		{
			if (mhdEditor.Screen.Resolution == -1)
				return pixels / 60.0;
			
			return pixels / mhdEditor.Screen.Resolution;
		}
		
		public EventHandler DiagramChanged;
	}
}
