using System;
using Gdk;
using Gtk;
using MonoDevelop.ClassDesigner.Figures;
using MonoHotDraw.Figures;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected virtual void OnAddClassFigureActionActivated (object sender, System.EventArgs e)
	{
		mhdcanvas.AddWithDragging(new HeaderFigure());
	}

	protected virtual void OnAddStackFigureActionActivated (object sender, System.EventArgs e)
	{
		Pixbuf pb = RenderIcon("gtk-info", IconSize.Button, "");
		mhdcanvas.AddWithDragging((IFigure) new MemberFigure (pb, "Hello", "World", true));
	}

	protected virtual void OnAddMemberGroupActionActivated (object sender, System.EventArgs e)
	{
		CompartmentFigure group = new CompartmentFigure("Methods");
		Pixbuf icon = RenderIcon("gtk-info", IconSize.Menu, "");
		
		for (int i=0; i<5; i++) {
			group.AddMember(new MemberFigure (icon, "int", string.Format("method{0}", i), true));
		}
		
		mhdcanvas.AddWithDragging(group);
	}

	protected virtual void OnAddSimpleTextFigureActionActivated (object sender, System.EventArgs e)
	{
		TextFigure figure = new TextFigure("Hello World");
		figure.Padding = 0;
		mhdcanvas.AddWithDragging(figure);
	}
}