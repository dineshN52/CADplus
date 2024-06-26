using System.Windows;
using System.Windows.Media;


namespace Utils;
public class DrawingCommands {

   #region Constructor-----------------------------------------------
   public DrawingCommands (DrawingContext dc, Matrix currentMatrix) => (mDc, mMatrix) = (dc, currentMatrix);
   #endregion

   #region Methods---------------------------------------------------
   public void DrawLine (double startPointX, double startPointY, double endPointX, double endPointY, string color, int thickness) {
      (Point p1, Point p2) = (new (startPointX, startPointY), new (endPointX, endPointY));
      Color lc = (Color)ColorConverter.ConvertFromString (color);
      Brush lb = new SolidColorBrush (lc);
      Pen lPen = new (lb, thickness);
      mDc.DrawLine (lPen, mMatrix.Transform (p1), mMatrix.Transform (p2));
   }

   public void DrawRectangle (double startPointX, double startPointY, double endPointX, double endPointY, string color, int thickness) {
      (Point cornerA, Point cornerB) = (new (startPointX, startPointY), new (endPointX, endPointY));
      Color rc = (Color)ColorConverter.ConvertFromString (color);
      Brush rb = new SolidColorBrush (rc);
      Pen rPen = new (rb, thickness);
      Rect r = new (mMatrix.Transform (cornerA), mMatrix.Transform (cornerB));
      mDc.DrawRectangle (Brushes.Transparent, rPen, r);
   }

   public void DrawCircle (double centerX, double centerY, double tangentX, double tangentY, string color, int thickness) {
      (Point center, Point tangentPt) = (new (centerX, centerY), new (tangentX, tangentY));
      Color cc = (Color)ColorConverter.ConvertFromString (color);
      Brush cb = new SolidColorBrush (cc);
      Pen cPen = new (cb, thickness);
      center = mMatrix.Transform (center); tangentPt = mMatrix.Transform (tangentPt);
      var Radius = Math.Sqrt (((tangentPt.X - center.X) * (tangentPt.X - center.X)) + ((tangentPt.Y - center.Y) * (tangentPt.Y - center.Y)));
      mDc.DrawEllipse (Brushes.Transparent, cPen, center, Radius, Radius);
   }

   public void DrawConnectedLine (List<double> CoOrdinates, double HoverX, double HoverY, string color, int thickness) {
      List<Point> points = new ();
      Point hoverPt = new (HoverX, HoverY); hoverPt = mMatrix.Transform (hoverPt);
      Color clc = (Color)ColorConverter.ConvertFromString (color);
      Brush clb = new SolidColorBrush (clc);
      Pen clPen = new (clb, thickness);
      for (int i = 0; i < CoOrdinates.Count; i += 2) {
         Point p = new (CoOrdinates[i], CoOrdinates[i + 1]);
         points.Add (mMatrix.Transform (p));
      }
      if (points.Count > 1) {
         for (int i = 0; i < points.Count - 1; i++)
            mDc.DrawLine (clPen, points[i], points[i + 1]);
      }
      if (hoverPt != null)
         mDc.DrawLine (clPen, points[^1], hoverPt);
   }
   #endregion

   #region Fields----------------------------------------------------
   readonly DrawingContext mDc;
   readonly Matrix mMatrix;
   #endregion

}