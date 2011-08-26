// MonoHotDraw. Diagramming Framework
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//
// Copyright (C) 2006, 2007, 2008, 2009 MonoUML Team (http://www.monouml.org)
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
using System.Runtime.Serialization;
using Cairo;
using MonoHotDraw.Commands;
using MonoHotDraw.Connectors;
using MonoHotDraw.Handles;
using MonoHotDraw.Tools;
using MonoHotDraw.Util;
using MonoHotDraw.Visitor;

namespace MonoHotDraw.Figures
{
	[Serializable]
	public abstract class Figure : ICloneable, ISerializable
	{
		bool visible;
		
		protected Figure ()
		{
			FillColor = new Color (1.0, 1.0, 0.2, 0.8);
			LineColor = (Color) AttributeFigure.GetDefaultAttribute (FigureAttribute.LineColor);
			visible = true;
		}

		protected Figure (SerializationInfo info, StreamingContext context)
		{
			FillColor = (Color) info.GetValue ("FillColor", typeof (Color));
			LineColor = (Color) info.GetValue ("LineColor", typeof (Color));
		}

		public event FigureEventHandler FigureChanged;
		public event FigureEventHandler FigureInvalidated;
		
		
		#region Public Api
		public virtual void Add (Figure figure)
		{
			throw new NotSupportedException ("Does not support adding child figures.");
		}

		public virtual void Remove (Figure figure)
		{
			throw new NotSupportedException ("Does not support removing child figures.");
		}
		
		public virtual Figure Container {
			get { return null; }
		}
		
		public virtual bool CanConnect {
			get { return true; }
		}

		public virtual RectangleD DisplayBox {
			get {
				if (Visible) {
					return BasicDisplayBox;
				} else {
					return new RectangleD (0, 0);
				}
			}
			set {
				if (value != DisplayBox) {
					WillChange ();
					BasicDisplayBox = value;
					Changed ();
				}
			}
		}
		
		public virtual IEnumerable <Figure> Figures {
			get { yield break; }
		}
		
		public Color FillColor { get; set; }

		public virtual IEnumerable <IHandle> Handles {
			get { yield break; }
		}
		
		public virtual RectangleD InvalidateDisplayBox {
			get {
				RectangleD rect = DisplayBox;
				rect.Inflate (AbstractHandle.Size + 1.0 , AbstractHandle.Size + 1.0);
				return rect;
			}
		}

		public virtual Color LineColor { get; set; }

		public virtual double LineWidth {
			get { return (double) GetAttribute (FigureAttribute.LineWidth); }
			set {
				if (value >= 0)
					SetAttribute (FigureAttribute.LineWidth, value);
			}
		}
		
		public virtual bool Visible {
			get { return visible; }
			set {
				if (visible != value) {
					WillChange ();
					visible = value;
					Changed ();
				}
			}
		}
		
		public virtual void AcceptVisitor (IFigureVisitor visitor)
		{
			visitor.VisitFigure (this);
			
			foreach (Figure figure in Figures)
				figure.AcceptVisitor (visitor);
			
			foreach (IHandle handle in Handles)
				visitor.VisitHandle (handle);
		}
		
		public virtual IConnector ConnectorAt (double x, double y)
		{
			return new ChopBoxConnector (this);
		}


		public virtual bool ContainsPoint (double x, double y)
		{
			return DisplayBox.Contains (x, y);
		}
		
		public virtual ITool CreateFigureTool (IDrawingEditor editor, ITool defaultTool)
		{
			return defaultTool;
		}

		public void Draw (Context context)
		{
			context.Save ();
			BasicDraw (context);
			context.Restore ();
		}
		
		public void DrawSelected (Context context)
		{
			context.Save ();
			BasicDrawSelected (context);
			context.Restore ();
		}

		public virtual object GetAttribute (FigureAttribute attribute)
		{
			switch (attribute) {
			case FigureAttribute.FillColor:
				return FillColor;
			case FigureAttribute.LineColor:
				return LineColor;
			default:
				return null;
			}
		}

		public virtual bool Includes (Figure figure)
		{
			return (this == figure);
		}
		
		public void Invalidate ()
		{
			OnFigureInvalidated (new FigureEventArgs (this, InvalidateDisplayBox));
		}

		public void MoveBy (double x, double y)
		{
			WillChange ();
			BasicMoveBy (x, y);
			Changed ();
		}
		
		public void MoveTo (double x, double y)
		{
			RectangleD r = DisplayBox;
			r.X = x;
			r.Y = y;
			DisplayBox = r;
		}
		
		public virtual Figure SelectableAt (double x, double y)
		{
			var selectable = GetAttribute (FigureAttribute.Selectable);
			return selectable != null && (bool) selectable == true ? this : null;
		}
		
		public virtual void SetAttribute (FigureAttribute attribute, object value)
		{
			switch (attribute) {
				case FigureAttribute.FillColor:
					FillColor = (Color) value;
					break;
				case FigureAttribute.LineColor:
					LineColor = (Color) value;
					break;
			}
		}
		
		#endregion
		
		#region ICloneable implementation
		public virtual object Clone ()
		{
			return GenericCloner.Clone <Figure> (this);
		}

		#endregion
		
		#region ISerializable implementation
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("FillColor", FillColor);
			info.AddValue ("LineColor", LineColor);
		}
		#endregion
		
		protected abstract RectangleD BasicDisplayBox { get; set; }

		protected virtual void BasicMoveBy (double x, double y)
		{
			RectangleD r = BasicDisplayBox;
			r.X += x;
			r.Y += y;
			BasicDisplayBox = r;
		}
		
		protected virtual void BasicDraw (Context context)
		{
		}

		protected virtual void BasicDrawSelected (Context context)
		{
		}
		
		internal void InternalMoveBy (double x, double y)
		{
			BasicMoveBy (x, y);
		}
		
		protected void Changed ()
		{
			Invalidate ();
			OnFigureChanged (new FigureEventArgs (this, DisplayBox));
		}

		protected virtual void OnFigureChanged (FigureEventArgs e)
		{
			var handler = FigureChanged;
			if (handler != null)
				handler (this, e);
		}
		
		protected virtual void OnFigureInvalidated (FigureEventArgs e)
		{
			var handler = FigureInvalidated;
			if (handler != null)
				handler (this, e);
		}
		
		protected void WillChange ()
		{
			Invalidate ();
		}
	}
}
