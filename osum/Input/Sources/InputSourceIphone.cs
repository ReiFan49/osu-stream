using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using osum.Helpers;
namespace osum
{
	public class InputSourceIphone : InputSource
	{
		public InputSourceIphone() : base()
		{
		}

		public void HandleTouchesBegan(NSSet touches, UIEvent evt)
		{
            Clock.Update(true);
            TrackingPoint newPoint = null;

			foreach (UITouch u in touches.ToArray<UITouch>()) {
				newPoint = new TrackingPointIphone(u.LocationInView(EAGLView.Instance), u);
				trackingPoints.Add(newPoint);
				TriggerOnDown(newPoint);
			}
		}

		public void HandleTouchesMoved(NSSet touches, UIEvent evt)
		{
            Clock.Update(true);
            TrackingPoint point = null;

			foreach (UITouch u in touches.ToArray<UITouch>()) {
				point = trackingPoints.Find(t => t.Tag == u);
				if (point != null)
				{
					point.Location = u.LocationInView(EAGLView.Instance);
					TriggerOnMove(point);
				}
			}
		}

		public void HandleTouchesEnded(NSSet touches, UIEvent evt)
		{
            Clock.Update(true);
            TrackingPoint point = null;

            //todo: don't need to foreach where there's only one point (likely 99%)
			foreach (UITouch u in touches.ToArray<UITouch>()) {
				point = trackingPoints.Find(t => t.Tag == u);
				if (point != null)
				{
					trackingPoints.Remove(point);
					TriggerOnUp(point);
				}
			}
			
		}

		public void HandleTouchesCancelled(NSSet touches, UIEvent evt)
		{
			HandleTouchesEnded(touches, evt);
		}		
	}
}

