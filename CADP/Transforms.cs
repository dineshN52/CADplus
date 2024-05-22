using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using Point = BackEnd.Point;

namespace CADP;

static class Transform {

   #region Methods---------------------------------------------------
   public static Matrix ComputeZoomExtentsProjXfm (double viewWidth, double viewHeight, Bound b) {
      var viewMargin = 4;
      // Compute the scaling, to fit specified drawing extents into the view space
      double scaleX = (viewWidth - 2 * viewMargin) / b.Width, scaleY = (viewHeight - 2 * viewMargin) / b.Height;
      //double scale = Math.Min (scaleX, scaleY);
      var scaleMatrix = Matrix.Identity; scaleMatrix.Scale (scaleX, -scaleY);
      // translation...
      System.Windows.Point pMid = scaleMatrix.Transform (new System.Windows.Point (b.Mid.X, b.Mid.Y));
      Point projectedMidPt = new (pMid.X, pMid.Y);
      System.Windows.Point viewMidPt = new (viewWidth / 2, viewHeight / 2);
      var translateMatrix = Matrix.Identity; translateMatrix.Translate (viewMidPt.X - projectedMidPt.X, viewMidPt.Y - projectedMidPt.Y);
      // Final zoom extents matrix, is a product of scale and translate matrices
      scaleMatrix.Append (translateMatrix);
      return scaleMatrix;
   }
   #endregion
}

//class PanWidget { // Works in screen space
//   #region Constructors
//   public PanWidget (UIElement eventSource, Action<Vector> onPan) {
//      mOnPan = onPan;
//      eventSource.MouseDown += (sender, e) => {
//         if (e.ChangedButton == MouseButton.Middle) PanStart (e.GetPosition (eventSource));
//      };
//      eventSource.MouseUp += (sender, e) => {
//         if (IsPanning) PanEnd (e.GetPosition (eventSource));
//      };
//      eventSource.MouseMove += (sender, e) => {
//         if (IsPanning) PanMove (e.GetPosition (eventSource));
//      };
//      eventSource.MouseLeave += (sender, e) => {
//         if (IsPanning) PanCancel ();
//      };
//   }
//   #endregion

//   #region Implementation
//   bool IsPanning => mPrevPt != null;

//   void PanStart (Point pt) {
//      mPrevPt = pt;
//   }

//   void PanMove (Point pt) {
//      mOnPan.Invoke (pt - mPrevPt!.Value);
//      mPrevPt = pt;
//   }

//   void PanEnd (Point? pt) {
//      if (pt.HasValue)
//         PanMove (pt.Value);
//      mPrevPt = null;
//   }

//   void PanCancel () => PanEnd (null);
//   #endregion

//   #region Private
//   Point? mPrevPt;
//   readonly Action<Vector> mOnPan;
//   #endregion
//}

public readonly struct Bound { // Bound in drawing space
   #region Constructors
   public Bound (Point cornerA, Point cornerB) {
      MinX = Math.Min (cornerA.X, cornerB.X);
      MaxX = Math.Max (cornerA.X, cornerB.X);
      MinY = Math.Min (cornerA.Y, cornerB.Y);
      MaxY = Math.Max (cornerA.Y, cornerB.Y);
   }

   public Bound (IEnumerable<Point> pts) {
      MinX = pts.Min (p => p.X);
      MaxX = pts.Max (p => p.X);
      MinY = pts.Min (p => p.Y);
      MaxY = pts.Max (p => p.Y);
   }

   public Bound (IEnumerable<Bound> bounds) {
      MinX = bounds.Min (b => b.MinX);
      MaxX = bounds.Max (b => b.MaxX);
      MinY = bounds.Min (b => b.MinY);
      MaxY = bounds.Max (b => b.MaxY);
   }

   public Bound () {
      this = Empty;
   }

   public static readonly Bound Empty = new () { MinX = double.MaxValue, MinY = double.MaxValue, MaxX = double.MinValue, MaxY = double.MinValue };
   #endregion

   #region Properties
   public double MinX { get; init; }
   public double MaxX { get; init; }
   public double MinY { get; init; }
   public double MaxY { get; init; }
   public double Width => MaxX - MinX;
   public double Height => MaxY - MinY;
   public Point Mid => new ((MaxX + MinX) / 2, (MaxY + MinY) / 2);
   public bool IsEmpty => MinX > MaxX || MinY > MaxY;
   #endregion

   #region Methods
   public Bound Inflated (Point ptAt, double factor) {
      if (IsEmpty) return this;
      var minX = ptAt.X - (ptAt.X - MinX) * factor;
      var maxX = ptAt.X + (MaxX - ptAt.X) * factor;
      var minY = ptAt.Y - (ptAt.Y - MinY) * factor;
      var maxY = ptAt.Y + (MaxY - ptAt.Y) * factor;
      return new () { MinX = minX, MaxX = maxX, MinY = minY, MaxY = maxY };
   }
   #endregion
}