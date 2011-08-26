// MonoHotDraw. Diagramming Framework
//
// Authors:
//	Manuel Cerón <ceronman@gmail.com>
//	Mario Carrión <mario@monouml.org>
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

using Cairo;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MonoHotDraw.Connectors;
using MonoHotDraw.Handles;
using MonoHotDraw.Locators;
using MonoHotDraw.Util;

namespace MonoHotDraw.Figures
{
	[Serializable]
	public class LineConnectionFigure : ConnectionFigure
	{
		protected LineConnectionFigure (SerializationInfo info, StreamingContext context)
		{
			Line = (PolyLineFigure) info.GetValue ("Line", typeof (PolyLineFigure));
		}
		
		public LineConnectionFigure () : this (null, null)
		{
			Line.AddPoint (0.0, 0.0);
			Line.AddPoint (0.0, 0.0);
		}

		public LineConnectionFigure (IFigure startFigure, IFigure endFigure) : base (startFigure, endFigure)
		{
			Line = new PolyLineFigure ();
			UpdateConnection ();
		}
		
		public override IHandle StartHandle {
			get { return new ChangeConnectionStartHandle (this); }
		}
		
		public override IHandle EndHandle {
			get { return new ChangeConnectionEndHandle (this); }
		}
		
		public override PointD StartPoint {
			get { return Line.StartPoint; }
			set { Line.StartPoint = value; }
		}
		
		public override PointD EndPoint {
			get { return Line.EndPoint; }
			set { Line.EndPoint = value; }
		}
		
		public override PointD AfterStart {
			get { return Line.PointAt (1); }
		}
		
		public override PointD BeforeEnd {
			get { return Line.PointAt (Line.PointCount - 2); }
		}
		
		public PolyLineFigure Line { get; protected set; }

		protected override RectangleD BasicDisplayBox {
			get { return Line.DisplayBox; }
			set { Line.DisplayBox = value; }
		}
		
		public override bool CanConnect {
			get { return false; }
		}
		
		public override IEnumerable<IFigure> Figures {
			get {
				foreach (var figure in base.Figures) {
					yield return figure;
				}
				yield return Line;
			}
		}

		public override IEnumerable<IHandle> Handles {
			get {
				if (Line.PointCount < 2)
					yield break;

				foreach (var handle in base.Handles) {
					yield return handle;
				}
				
				for (int i = 1; i < Line.PointCount - 1; i++)
					yield return new LineConnectionHandle (Line, new PolyLineLocator (i), i);
			}
		}
		
		protected override void BasicMoveBy (double x, double y)
		{
			Line.MoveBy (x, y);
			UpdateConnection ();
		}
		
		public override void UpdateConnection ()
		{
			if (Line != null) {
				WillChange ();
				if (StartConnector != null) {
					Line.StartPoint = StartConnector.FindStart (this);
				}
				if (EndConnector != null) {
					Line.EndPoint = EndConnector.FindEnd (this);
				}
				Changed ();
			}
		}
		
		#region Serialization
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("Line", Line);
		}
		#endregion
	}
}
