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
		throw new NotImplementedException ();
	}

	protected virtual void OnAddMemberGroupActionActivated (object sender, System.EventArgs e)
	{
		throw new NotImplementedException ();
	}

	protected virtual void OnAddSimpleTextFigureActionActivated (object sender, System.EventArgs e)
	{
		mhdcanvas.AddWithDragging(new TextFigure("Hello World"));
	}
}