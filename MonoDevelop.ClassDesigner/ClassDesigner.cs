// 
// ClassDesigner.cs
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

using Gdk;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using MonoDevelop.DesignerSupport.Toolbox;
using MonoDevelop.Diagram;
using MonoDevelop.Diagram.Components;
using MonoDevelop.ClassDesigner.Gui.Dialogs;
using MonoDevelop.ClassDesigner.Gui.Toolbox;
using MonoDevelop.ClassDesigner.Figures;
using MonoDevelop.ClassDesigner.Visitor;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui;

using MonoHotDraw;
using MonoHotDraw.Tools;
using MonoHotDraw.Connectors;
using MonoHotDraw.Figures;


namespace MonoDevelop.ClassDesigner
{
	public sealed class ClassDesigner : AbstractDesigner
	{
		static readonly string path = "/MonoDevelop/ClassDesigner/FigureCommandHandlers";
		static FigureCommandHandlerCollection handlers = new FigureCommandHandlerCollection (path);
		ToolboxList toolboxItems;

		public ClassDesigner (Project project) : this ()
		{
			this.UntitledName = "ClassDiagram.cd";
			this.Project = project;
		}
		
		public ClassDesigner (FilePath fileName, Project ownerProject) : this ()
		{
			if (String.IsNullOrEmpty (fileName)) 
				throw new ArgumentNullException ();
			
			this.ContentName = fileName.FileName;
			this.IsDirty = false;
			this.Project = IdeApp.Workspace.GetProjectContainingFile (fileName);
		}	
		
		protected ClassDesigner () : base (100)
		{
			View.Drawing = new ClassDiagram ();
			ProjectDomService.TypesUpdated += OnTypesUpdated;
			this.UntitledName = "ClassDiagram.cd";
			this.IsViewOnly = false;
			//this.View.VisibleAreaChanged
			IsDirty = true;
			SetupTools ();
		}
		
		public override void Dispose ()
		{
			ProjectDomService.TypesUpdated -= OnTypesUpdated;
		}
		
		public void Remove (IFigure figure)
		{
			View.Remove (figure);
		}
		
		void OnTypesUpdated (object sender, TypeUpdateInformationEventArgs e)
		{
			Gtk.Application.Invoke (delegate { Diagram.UpdateRange (e.TypeUpdateInformation.Modified); });
			Gtk.Application.Invoke (delegate { Diagram.RemoveRange (e.TypeUpdateInformation.Removed); });
		}

		
		#region Public API
		public ClassDiagram Diagram {
			get { return (ClassDiagram) View.Drawing; }
		}
		
		public void AddInheritanceLines ()
		{
			ClassFigure subclass;
			ClassFigure superclass;
			
			foreach (IType type in Dom.Types) {				
				if (type.ClassType == ClassType.Class) {		
					subclass = Diagram.GetTypeFigure (type.Name) as ClassFigure;
					
					if (subclass.HideInheritance)
						continue;
					
					if (type.BaseType == null)
						superclass = null;
					else
						superclass = Diagram.GetTypeFigure (type.BaseType.Name) as ClassFigure;
					
					if (subclass != null && superclass != null) {
						var connection = new InheritanceConnectionFigure (subclass, superclass);
						View.Add (connection);
					}
				}
			}	
		}
		
		public void AddFromDirectory (string directory)
		{
			AddRange (Project.Files.Where(pf => pf.FilePath.IsChildPathOf (directory)).Select (file => file.Name));
		}
		
		public override void AddFromFile (string fileName)
		{
			ParsedDocument doc = ProjectDomService.ParseFile (Dom, fileName);			
			var file = Project.Files.GetFile (fileName);
			
			if (file.FilePath.Extension == ".cd") {
				Load (fileName);
				return;
			}
		
			if (doc == null)
				return;
			
			var compilationUnit = doc.CompilationUnit;
			
			if (compilationUnit == null)
				return;
			
			Diagram.AddRange (compilationUnit.Types);
		}
		
		public void AddFromNamespace (string ns)
		{	
			IList<IMember> members = Dom.GetNamespaceContents (ns, false, true);
			
			if (members == null) {
				Console.WriteLine ("In namespace: {0} members were not found.", ns);
				return;
			}
			
			foreach (IMember item in members) {
				if (item.MemberType == MemberType.Namespace) {
					AddFromNamespace (item.FullName);
				}
				else if (item.MemberType == MemberType.Type) {
					AddFromType (Dom.GetType (item.FullName));
				}
			}
		}
		
		public override void AddFromProject (Project project)
		{
			Project = project;
			Diagram.AddRange (Dom.Types);
		}
 
		public void AddFromType (IType type)
		{
			Diagram.Add (type);
		}
		
		public void AutoName ()
		{
			if (String.IsNullOrEmpty (ContentName)) {
				string baseName = Path.GetFileNameWithoutExtension(UntitledName);
				string extension = Path.GetExtension(UntitledName);
				string current = UntitledName;
				
				for (int i = 1; Project.Files.GetFileWithVirtualPath (current) != null
						|| File.Exists (Project.BaseDirectory.Combine (current)); i++) {
					current = baseName + i + extension;
				}
				
				ContentName = Project.BaseDirectory.Combine (current);
			}
		}
		#endregion
		
		#region Commands
		
		[CommandHandler (MembersFormat.FullSignature)]
		protected void FormatByFullSignature ()
		{
			Diagram.Format = MembersFormat.FullSignature;	
		}
		
		[CommandHandler (MembersFormat.Name)]
		protected void FormatByName ()
		{
			Diagram.Format = MembersFormat.Name;
		}
		
		[CommandHandler (MembersFormat.NameAndType)]
		protected void FormatByNameAndType ()
		{
	
			Diagram.Format = MembersFormat.NameAndType;
		}
		
		[CommandHandler (GroupingSetting.Member)]
		protected void GroupByAccess ()
		{
			Diagram.Grouping = GroupingSetting.Member;
		}
		
		[CommandHandler (GroupingSetting.Alphabetical)]
		protected void GroupByAlphabetical ()
		{
			Diagram.Grouping = GroupingSetting.Alphabetical;
		}
		
		[CommandHandler (GroupingSetting.Kind)]
		protected void GroupByKind ()
		{
			Diagram.Grouping = GroupingSetting.Kind;		
		}
		
		[CommandUpdateHandler (MembersFormat.FullSignature)]
		protected void UpdateFormatByFullSignature (CommandInfo info)
		{
			info.Enabled = true;
			
			if (Diagram.Format == MembersFormat.FullSignature)
				info.Enabled = false;	
			
			if (View.SelectionCount == 0)
				info.Visible = true;
			else
				info.Visible = false;

		}
		
		[CommandUpdateHandler (MembersFormat.Name)]
		protected void UpdateFormatByName (CommandInfo info)
		{
			info.Enabled = true;
			
			if (Diagram.Format == MembersFormat.Name)
				info.Enabled = false;
			
			if (View.SelectionCount == 0)
				info.Visible = true;
			else
				info.Visible = false;
		}
		
		[CommandUpdateHandler (MembersFormat.NameAndType)]
		protected void UpdateFormatByNameAndType (CommandInfo info)
		{
			info.Enabled = true;
			
			if (Diagram.Format == MembersFormat.NameAndType)
				info.Enabled = false;
			
			if (View.SelectionCount == 0)
				info.Visible = true;
			else
				info.Visible = false;			
		}
		
		[CommandUpdateHandler (GroupingSetting.Member)]
		protected void UpdateGroupByAccess (CommandInfo info)
		{
			info.Enabled = true;
			
			if (Diagram.Grouping == GroupingSetting.Member)
				info.Enabled = false;

			if (View.SelectionCount == 0)
				info.Visible = true;
			else
				info.Visible = false;
		}
		
		[CommandUpdateHandler (GroupingSetting.Alphabetical)]
		protected void UpdateGroupByAlphabetical (CommandInfo info)
		{
			info.Enabled = true;
			
			if (Diagram.Grouping == GroupingSetting.Alphabetical)
				info.Enabled = false;
			
			if (View.SelectionCount == 0)
				info.Visible = true;
			else
				info.Visible = false;
		}

		[CommandUpdateHandler (GroupingSetting.Kind)]
		protected void UpdateGroupByKind (CommandInfo info)
		{
			info.Enabled = true;
						
			if (Diagram.Grouping == GroupingSetting.Kind)
				info.Enabled = false;
			
			if (View.SelectionCount == 0)
				info.Visible = true;
			else
				info.Visible = false;
		}

		[CommandHandler (DesignerCommands.AddClass)]
		protected void CreateClass ()
		{
			var dialog = new AddFigureDialog ("Class", ClassType.Class, PointerToDrawing, this);
		}
		
		[CommandHandler (DesignerCommands.AddComment)]
		protected void CreateComment ()
		{
			var figure = new CommentFigure (String.Empty);
			figure.MoveTo (PointerToDrawing.X, PointerToDrawing.Y);
			View.Add (figure);
		}
		
		[CommandHandler (DesignerCommands.AddDelegate)]
		protected void CreateDelegate ()
		{
			var dialog = new AddFigureDialog ("Delegate", ClassType.Class, PointerToDrawing, this);
		}
		
		[CommandHandler (DesignerCommands.AddEnum)]
		protected void CreateEnum ()
		{
			var dialog = new AddFigureDialog ("Enum", ClassType.Delegate, PointerToDrawing, this);			
		}
		
		[CommandHandler (DesignerCommands.AddInterface)]		
		protected void CreateInterface ()
		{
			var dialog = new AddFigureDialog ("Interface", ClassType.Interface, PointerToDrawing, this);
		}
		
		[CommandHandler (DesignerCommands.AddStruct)]
		protected void CreateStruct ()
		{
			var dialog = new AddFigureDialog ("Struct", ClassType.Struct, PointerToDrawing, this);
		}
		
		#endregion
				
		#region AbstractViewContent Members
		public override void Load (string fileName)
		{
			Diagram.Load (fileName, this.Dom);	
			IsDirty = false;
			Control.GrabFocus ();
		}
		
		public override void Save ()
		{
			XElement xml;
			lock (Diagram) {
				xml = Diagram.Serialize ();
			}
			xml.Save (ContentName);
			IsDirty = false;
		}
		
		public override void Save (string fileName)
		{
			ContentName = fileName;
			Save ();
		}

		public override bool IsFile {
			get {
				return true;
			}
		}
		
		#endregion

		#region AbstractDesigner Members
		protected override IEnumerable<FigureCommandHandler> CommandHandlers {
			get { return handlers; }
		}
		
		public override void AddCommands ()
		{
		}
		
		public override void DisplayMenu (IFigure figure, MouseEvent ev)
		{
			IdeApp.CommandService.ShowContextMenu ("/ClassDesigner/ContextMenu/Diagram");
		} 
		#endregion
		
		#region Toolbox support
		public override IEnumerable<ItemToolboxNode> GetDynamicItems (IToolboxConsumer consumer)
		{
			foreach (ItemToolboxNode item in toolboxItems) {
				item.Category = GettextCatalog.GetString ("Class Diagram");
				yield return item;
			}
		}
		
		public override void ConsumeItem (ItemToolboxNode item)
		{
			var figureItem = item as IToolboxFigure;
			var connectorItem = item as IToolboxConnector;
			
			if (connectorItem == null && figureItem == null)
				return;
			
			if (connectorItem != null) {
				AbstractConnectionFigure connector;
				
				if (connectorItem.ConnectorType == ConnectionType.Inheritance)
					connector = new InheritanceConnectionFigure ();
				else
					connector = new AssociationConnectionFigure (connectorItem.ConnectorType);
				
				Tool = new ConnectionCreationTool (this, connector.ConnectionLine);
				return;
			}
			
			int x, y;
			Control.GetPointer (out x, out y);
			var point = View.ViewToDrawing (x, y);
			
			if (figureItem.ClassType == ClassType.Unknown) {
				var comment = new CommentFigure (String.Empty);
				comment.MoveTo (point.X, point.Y);
				View.Add (comment);
				return;
			}
			
			var dialog = new AddFigureDialog (figureItem.Name, figureItem.ClassType, point, this);
			dialog.ShowAll ();
		}
		
		public override string DefaultItemDomain {
			get { return "Class Diagram"; }
		}
		#endregion
		
		
		#region Private Methods
			
		void AddRange (IEnumerable<string> files)
		{
			foreach (string file in files)
				AddFromFile (file);
		}
		
		void SetupTools ()
		{
	
			toolboxItems = new ToolboxList ();
			
			var icon = ImageService.GetPixbuf (Stock.TextFileIcon, Gtk.IconSize.SmallToolbar);
			toolboxItems.Add (new FigureToolboxItemNode ("Comment", ClassType.Unknown, true, icon));
			
			icon = ImageService.GetPixbuf (Stock.ProtectedClass, Gtk.IconSize.SmallToolbar);
			toolboxItems.Add (new FigureToolboxItemNode ("Abstract Class", ClassType.Class, true, icon));
			
			icon = ImageService.GetPixbuf (Stock.Class, Gtk.IconSize.SmallToolbar);
			toolboxItems.Add (new FigureToolboxItemNode ("Class", ClassType.Class, false, icon));
			
			icon = ImageService.GetPixbuf (Stock.Interface, Gtk.IconSize.SmallToolbar);
			toolboxItems.Add (new FigureToolboxItemNode ("Interface", ClassType.Interface, false, icon));
			
			icon = ImageService.GetPixbuf (Stock.Enum, Gtk.IconSize.SmallToolbar);
			toolboxItems.Add (new FigureToolboxItemNode ("Enum", ClassType.Enum, false, icon));
			
			icon = ImageService.GetPixbuf (Stock.Delegate, Gtk.IconSize.SmallToolbar);
			toolboxItems.Add (new FigureToolboxItemNode ("Delegate", ClassType.Delegate, false, icon));
			
			icon = ImageService.GetPixbuf (Stock.Struct, Gtk.IconSize.SmallToolbar);
			toolboxItems.Add (new FigureToolboxItemNode ("Struct", ClassType.Struct, false, icon));
			
			icon = ImageService.GetPixbuf (Stock.SplitWindow, Gtk.IconSize.SmallToolbar);
			toolboxItems.Add (new ConnectorToolboxItemNode ("Association", ConnectionType.Association, icon));
			
			icon = ImageService.GetPixbuf (Stock.MiscFiles, Gtk.IconSize.SmallToolbar);
			toolboxItems.Add (new ConnectorToolboxItemNode ("Inheritance", ConnectionType.Inheritance, icon));
		}
		
		#endregion
	}
}