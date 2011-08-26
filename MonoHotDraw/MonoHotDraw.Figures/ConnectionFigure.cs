// 
// ConnectionFigure.cs
//  
// Author:
//       Graham Lyon <graham.lyon@gmail.com>
// 
// Copyright (c) 2011 Graham Lyon
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

namespace MonoHotDraw.Figures
{
	[Serializable]
	public abstract class ConnectionFigure : AttributeFigure
	{
		private IConnector startConnector;
		private IConnector endConnector;

		protected ConnectionFigure (SerializationInfo info, StreamingContext context) : base (info, context)
		{
			EndConnector = (IConnector) info.GetValue ("EndConnector", typeof (IConnector));
			StartConnector = (IConnector) info.GetValue ("StartConnector", typeof (IConnector));
		}
		
		public ConnectionFigure (Figure startFigure, Figure endFigure)
		{
			if (startFigure != null)
				StartConnector = startFigure.ConnectorAt (0.0, 0.0);

			if (endFigure != null)
				EndConnector  = endFigure.ConnectorAt (0.0, 0.0);
		}
		
		public ConnectionFigure ()
		{
		}
		
		public event EventHandler ConnectionChanged;
		
		public IConnector StartConnector {
			get { return startConnector; }
			set {
				if (startConnector == value) {
					return;
				}
				
				if (startConnector != null) {
					startConnector.Owner.FigureChanged -= FigureChangedHandler;
				}
				
				startConnector = value;
				if (startConnector != null) {
					startConnector.Owner.FigureChanged += FigureChangedHandler;
				}
				UpdateConnection ();
			}
		}
		
		public IConnector EndConnector {
			get { return endConnector; }
			set {
				if (endConnector == value) {
					return;
				}
				
				if (endConnector != null) {
					endConnector.Owner.FigureChanged -= FigureChangedHandler;
				}
				
				endConnector = value;
				if (endConnector != null) {
					endConnector.Owner.FigureChanged += FigureChangedHandler;
				}
				UpdateConnection ();
			}
		}
		
		public Figure StartFigure { get { return startConnector == null ? null : startConnector.Owner; } }
		public Figure EndFigure { get { return endConnector == null ? null : endConnector.Owner; } }
		
		public abstract IHandle StartHandle { get; }
		public abstract IHandle EndHandle { get; }
		
		public abstract PointD StartPoint { get; set; }
		public abstract PointD EndPoint { get; set; }
		
		public abstract PointD AfterStart { get; }
		public abstract PointD BeforeEnd { get; }
		
		public override IEnumerable<IHandle> Handles {
			get {
				foreach (var handle in base.Handles) {
					yield return handle;
				}
				
				yield return StartHandle;
				yield return EndHandle;
			}
		}

		private void FigureChangedHandler (object sender, FigureEventArgs args)
		{
			UpdateConnection ();
		}
		
		protected virtual void OnConnectionChanged ()
		{
			// FIXME: Some event args here would be lovely...
			if (ConnectionChanged != null)
				ConnectionChanged (this, EventArgs.Empty);
		}

		public virtual bool CanConnectEnd (Figure figure)
		{
			return true;
		}

		public virtual bool CanConnectStart (Figure figure)
		{
			return true;
		}
		
		public abstract void UpdateConnection ();
		
		#region Serialization
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("EndConnector",   EndConnector);
			info.AddValue ("StartConnector", StartConnector);
			
			base.GetObjectData (info, context);
		}
		#endregion
	}
}
