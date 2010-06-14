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
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.ClassDesigner.Figures;
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
		bool is_file;

		GroupingSetting groupSetting;		
		MembersFormat membersFormat;
		List<IFigure> figures;
		
		public ClassDiagram () : this (GroupingSetting.Access, MembersFormat.FullSignature)
		{
		}
		
		protected ClassDiagram (GroupingSetting grouping, MembersFormat format)
		{
			majorVersion = 1;
			minorVersion = 1;
			groupSetting = grouping;
			membersFormat = format;
			is_file = false;
			
			this.figures = new List<IFigure> ();
		}
		
		public ClassDiagram (GroupingSetting grouping, MembersFormat format, IEnumerable<IFigure> figures)
		{
			majorVersion = 1;
			minorVersion = 1;
			groupSetting = grouping;
			membersFormat = format;
			
			this.figures = new List<IFigure> (figures);
		}
		
		public MembersFormat Format {
			get { return membersFormat; }
			set {
				if (membersFormat == value)
					return;
				
				membersFormat = value;
				//OnMembersFormatChanged (new MemberFormatArgs (membersFormat));
			}	
		}
		
		public GroupingSetting Grouping {
			get { return groupSetting; }
			set {
				if (groupSetting == value)
					return;
				
				groupSetting = value;
				//OnGroupSettingChanged (new GroupingArgs (groupSetting));
			}
		}
		
		public IEnumerable<IFigure> Figures {
			get { return figures; }
		}
		
		bool HasFigure (string fullName)
		{
			foreach (IFigure figure in figures) {
				TypeFigure f = figure as TypeFigure;
				
				if (f == null)
					continue;
				
				if (f.Name.FullName == fullName)
					return true;
			}
			
			return false;
		}
		
		public IFigure CreateFigure (IType type, bool hideInheritanceLine)
		{
			IFigure figure;
			
			if (type == null)
				return null;
			
			if (HasFigure (type.FullName))
				return null;
			
			if (type.ClassType == ClassType.Class) {
				Console.WriteLine ("Adding Class");
				figure = new ClassFigure (type, hideInheritanceLine);
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
			
			
			figures.Add (figure);
			//if (figure!= null)
			//	((TypeFigure) figure).Toggle ();
			
			return figure;
		}
		
		public TypeFigure GetFigure (string name, Type type)
		{			
			if (name == null || type == null)
				return null;
			
			foreach (IFigure figure in figures) {
				if (type.IsAssignableFrom (figure.GetType ())) {
					var tf = (TypeFigure) figure;
					
					if (tf.Name.Name == name)
						return tf; 
				}
			}
			
			return null;
		}
		
		public void Load (string fileName, ProjectDom dom)
		{
			var reader = XmlReader.Create (fileName);
			IFigure figure = null;
			XElement element;
			reader.MoveToContent ();
			
			while (!reader.EOF) {
				Console.WriteLine ("node: {0} type {1}", reader.Name, reader.NodeType);
				if (reader.NodeType != XmlNodeType.Element) {
					reader.Read ();
					continue;
				}
					
				switch (reader.Name) {
				case "ClassDiagram": {
					element = XElement.Load (reader);
					LoadSettings (element);
					reader = element.CreateReader ();
					reader.ReadStartElement ();
					reader.Read ();
					break;
				} case "Font": {
					element = XElement.Load (reader);
					
					var attribute = ClassDiagram.SetAttribute (element.Attribute ("Name"), null);
					
					if (attribute != null)
						AttributeFigure.SetDefaultAttribute (FigureAttribute.FontFamily, attribute);
					
					attribute = ClassDiagram.SetAttribute (element.Attribute ("Size"), null);
					
					if (attribute != null)
						AttributeFigure.SetDefaultAttribute (FigureAttribute.FontSize, Int32.Parse(attribute));
					
					break;
				}
				case "Class":
				case "Struct":
				case "Enum":
				case "Interface":
				case "Delegate":
					figure = LoadType (XElement.Load (reader), dom);
					break;
				case "Comment": {
					figure = LoadComment (XElement.Load (reader));
					break;
				} default:
					reader.Read ();
					break;
				}
		
				if (figure != null)
					figures.Add (figure); 
			}
			
			is_file = true;
		}
		
		
		
		IFigure LoadComment (XElement element)
		{
			string text;
			IFigure figure;
			XAttribute attribute = element.Attribute ("CommentText");
			double x, y;
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
					
					x = ClassDiagram.SetAttribute (child.Attribute("X"), 2.5);
					x = ClassDiagram.InchesToPixels (x);
				
					y = ClassDiagram.SetAttribute (child.Attribute("Y"), 2.5);
					y = ClassDiagram.InchesToPixels (y);
				
					var width = ClassDiagram.SetAttribute (child.Attribute("Width"), -1.0);						
					var height = ClassDiagram.SetAttribute (child.Attribute("Height"), -1.0);
					
					if (width > 0 && height > 0 && x > 0 && y > 0) {
						var	rect = new RectangleD (x, y, width, height);
						figure.DisplayBox = rect;
					}
				}
			}			
					
			figures.Add (figure);
			figure.MoveTo (55.0, 75.0);
			
			return figure;
		}
		
		void LoadSettings (XElement element)
		{
			
			var attribute = ClassDiagram.SetAttribute (element.Attribute ("GroupingSetting"), null);
						
			if (attribute == null) {
				if (attribute == GroupingSetting.Alphabetical.ToString ())
					groupSetting = GroupingSetting.Alphabetical;
				else if (attribute == GroupingSetting.Member.ToString ())
					groupSetting = GroupingSetting.Member;
			}
			
			attribute = ClassDiagram.SetAttribute (element.Attribute ("MembersFormat"), null);
			
			if (attribute != null) {
				if (attribute == MembersFormat.NameAndType.ToString ())
					membersFormat = MembersFormat.NameAndType;
				else if (attribute == MembersFormat.Name.ToString ())
					membersFormat = MembersFormat.Name;
			}
		}
		
		IFigure LoadType (XElement element, ProjectDom dom)
		{	
			string typeName;
			bool collapsed;
			bool hideInheritance;
			double x, y, width, height;
			x = y = width = height = 0.0;
			
			typeName = ClassDiagram.SetAttribute (element.Attribute ("Name"), "Unknown");	
			collapsed = ClassDiagram.SetAttribute (element.Attribute ("Collapsed"), false);
			hideInheritance = ClassDiagram.SetAttribute (element.Attribute ("HideInheritanceLine"), false);
			
			foreach (XElement child in element.Elements ()) {
				if (child.Name == "Position") {	
					x = ClassDiagram.SetAttribute (child.Attribute ("X"), 2.0);
					x = InchesToPixels (x);
					
					y = ClassDiagram.SetAttribute (child.Attribute ("Y"), 2.0);
					y = InchesToPixels (y);
					
					width = ClassDiagram.SetAttribute (child.Attribute ("Width"), 2.5);
					width = InchesToPixels (width);
					
					height = ClassDiagram.SetAttribute (child.Attribute ("Height"), 1.5);
					height = InchesToPixels (height);
				}
				
				if (child.Name == "ShowAsAssociation" || child.Name == "ShowAsCollectionAssociation") {
					continue;
				}
			}
			
			Console.WriteLine ("Name: {0} X:{1} Y:{2} width:{3}", typeName, x, y, width);			
			
			var type = dom.GetType (typeName);
				
			if (type == null)
				return null;
			
			var figure = (TypeFigure) CreateFigure (type, hideInheritance);
			
			if (figure == null)
				return null;
			
			figures.Add (figure);
			figure.MoveTo (x, y);
			
			if (!collapsed)
				figure.Toggle ();
			
			return figure;
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

				if (figure is ClassFigure) {
					var cls = (ClassFigure) figure;
					writer.WriteStartElement ("Class");
					writer.WriteAttributeString ("Name", cls.Name.FullName);
					
					if (cls.Expanded) 
						writer.WriteAttributeString ("Collapsed", "true");
					
					if (cls.HideInheritance)
						writer.WriteAttributeString ("HideInheritanceLine", "true");
					
				} else if (figure is StructFigure) {
					var tf = (TypeFigure) figure;
					writer.WriteStartElement ("Struct");
					writer.WriteAttributeString ("Name", tf.Name.FullName);
		
					if (!tf.Expanded)
						writer.WriteAttributeString ("Collapsed", "true");
					
				} else if (figure is EnumFigure) {
					var tf = (TypeFigure) figure;
					writer.WriteStartElement ("Enum");
					writer.WriteAttributeString ("Name", tf.Name.FullName);
					
					if(!tf.Expanded)
						writer.WriteAttributeString ("Collapsed", "true");
					
				} else if (figure is InterfaceFigure) {
					var tf = (TypeFigure) figure;
					writer.WriteStartElement ("Interface");
					writer.WriteAttributeString ("Name", tf.Name.FullName);
					
					if (!tf.Expanded)
						writer.WriteAttributeString ("Collapsed", "true");
		
				} else if (figure is DelegateFigure) {
					var tf = (TypeFigure) figure;
					writer.WriteStartElement ("Delegate");
					writer.WriteAttributeString ("Name", tf.Name.FullName);
					writer.WriteAttributeString ("Collapsed", (!tf.Expanded).ToString ());
		
				} else if (figure is CommentFigure) {
					var comment = (CommentFigure) figure;
					writer.WriteAttributeString ("CommentText", comment.Text);
		
				} else 
					continue;
				
				writer.WriteWhitespace("\n\t\t");
				writer.WriteStartElement ("Position");
				
				double x, y, width, height;
				x = PixelsToInches (figure.DisplayBox.X);
				y = PixelsToInches (figure.DisplayBox.Y);
				width = PixelsToInches (figure.DisplayBox.Width);
				height = PixelsToInches (figure.DisplayBox.Height);
				
				writer.WriteAttributeString ("X", x.ToString ());
				writer.WriteAttributeString ("Y", y.ToString ());
				writer.WriteAttributeString ("Width", width.ToString ());
				writer.WriteAttributeString ("Height", height.ToString ());
				writer.WriteEndElement (); // End Position
				writer.WriteWhitespace("\n\t");
				writer.WriteEndElement (); // End Type				
			}
			writer.WriteWhitespace("\n");
			writer.WriteEndElement (); // End ClassDiagram	
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

		public static string SetAttribute (XAttribute attribute, string fallback)
		{
			if (attribute == null)
				return fallback;
			
			return attribute.Value;
		}
		
		public static double SetAttribute (XAttribute attribute, double fallback)
		{
			if (attribute == null)
				return fallback;
			
			return Double.Parse (attribute.Value);
		}
		
		public static bool SetAttribute (XAttribute attribute, bool fallback)
		{
			if (attribute == null)
				return fallback;
			
			return Boolean.Parse (attribute.Value);
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
