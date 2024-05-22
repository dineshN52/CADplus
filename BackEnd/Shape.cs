using System.Collections.Generic;
using System.Text;
using System;
using Utils;

namespace BackEnd;
/// <summary>Class which creates custom shapes</summary>
public abstract class Shape {

   #region Properties------------------------------------------------
   public string? Color { get; set; }

   public int Thickness { get; set; }

   public abstract string prompt { get; }
   #endregion

   #region Abstract Methods------------------------------------------
   public abstract Dictionary<string, double> GetDimension ();

   public abstract void Draw (DrawingCommands cmd);
   #endregion

   #region Field-----------------------------------------------------
   public List<Point> Points = new ();
   #endregion
}

public class Line : Shape {

   #region Overrides-------------------------------------------------
   public override string ToString () {
      StringBuilder s = new ();
      s.AppendLine ("Line");
      s.AppendLine (Color);
      s.AppendLine (Thickness.ToString ());
      s.AppendLine (Points[0].ToString ());
      s.AppendLine (Points[^1].ToString ());
      return s.ToString ();
   }

   public override void Draw (DrawingCommands cmd) {
      if (Color != null)
         cmd.DrawLine (Points[0].X, Points[0].Y, Points[^1].X, Points[^1].Y, Color, Thickness);
   }

   public override Dictionary<string, double> GetDimension () => new () {
      { "StartX", Points[0].X},
      { "StartY", Points [0].Y },
      {"Length",Length}
   };

   public override string prompt => Points.Count > 0 ? "  Pick End point" : "  Pick Beginning point";
   #endregion

   #region Properties------------------------------------------------
   public double Length => Points.Count > 0 ? Math.Sqrt ((Points[^1].X - Points[0].X) * (Points[^1].X - Points[0].X) +
               (Points[^1].Y - Points[0].Y) * (Points[^1].Y - Points[0].Y)) : 0;
   #endregion
}
public class Rectangle : Shape {

   #region Overrides-------------------------------------------------
   public override string ToString () {
      StringBuilder s = new ();
      s.AppendLine ("Rectangle");
      s.AppendLine (Color);
      s.AppendLine (Thickness.ToString ());
      s.AppendLine (Points[0].ToString ());
      s.AppendLine (Points[^1].ToString ());
      return s.ToString ();
   }

   public override void Draw (DrawingCommands cmd) {
      if (Color != null)
         cmd.DrawRectangle (Points[0].X, Points[0].Y, Points[^1].X, Points[^1].Y, Color, Thickness);
   }

   public override Dictionary<string, double> GetDimension () => new () {
      { "StartX", Points[0].X },
      { "StartY", Points [0].Y },
      {"Width",Width},
      {"Height",Height }
   };

   public override string prompt => Points.Count > 0 ? "  Pick second corner of rectangle" : "  Pick first corner of rectangle";
   #endregion

   #region Properties------------------------------------------------
   public double Width => Points.Count > 0 ? (Points[^1].X - Points[0].X) : 0;

   public double Height => Points.Count > 0 ? (Points[^1].Y - Points[0].Y) : 0;
   #endregion
}

public class Circle : Shape {

   #region Method----------------------------------------------------
   private double GetRadius () => mRadius = Math.Sqrt ((Points[^1].X - Points[0].X) * (Points[^1].X - Points[0].X) +
               (Points[^1].Y - Points[0].Y) * (Points[^1].Y - Points[0].Y));
   #endregion

   #region Overrides-------------------------------------------------
   public override string ToString () {
      StringBuilder s = new ();
      s.AppendLine ("Circle");
      s.AppendLine (Color);
      s.AppendLine (Thickness.ToString ());
      s.AppendLine (Points[0].ToString ());
      s.AppendLine (Radius.ToString ());
      return s.ToString ();
   }

   public override void Draw (DrawingCommands cmd) {
      if (Color != null)
         cmd.DrawCircle (Points[0].X, Points[0].Y, Points[^1].X, Points[^1].Y, Color, Thickness);
   }

   public override Dictionary<string, double> GetDimension () => new () {
      { "StartX", Points[0].X },
      { "StartY", Points [0].Y },
      {"Radius", Radius}
   };

   public override string prompt => Points.Count > 0 ? "  Pick point on circle" : "  Pick center point";
   #endregion

   #region Property--------------------------------------------------
   public double Radius { get { return GetRadius (); } set { mRadius = value; } }
   #endregion

   private double mRadius;
}

public class ConnectedLine : Shape {

   #region Overrides-------------------------------------------------
   public override string ToString () {
      StringBuilder sb = new ();
      sb.AppendLine ("ConnectedLine");
      sb.AppendLine (Color);
      sb.AppendLine (Thickness.ToString ());
      sb.AppendLine (LinePoints.Count.ToString ());
      for (int i = 0; i < LinePoints.Count; i += 2)
         sb.AppendLine (new Point (LinePoints[i], LinePoints[i + 1]).ToString ());
      return sb.ToString ();
   }

   public override void Draw (DrawingCommands cmd) {
      if (Color != null) {
         cmd.DrawConnectedLine (LinePoints, HoverPoint.X, HoverPoint.Y, Color, Thickness);
      }
   }

   public override Dictionary<string, double> GetDimension () => new () {
      { "StartX", LinePoints[0] },
      { "StartY", LinePoints[1] },
      { "CurrentLineLength",CurrentLineLength}
   };

   public override string prompt => Points.Count > 0 ? "  Pick start point of connected Line" : "  Pick start position of next line";
   #endregion

   #region Property--------------------------------------------------
   public Point HoverPoint { get; set; }

   public double CurrentLineLength => LinePoints.Count > 1 ? Math.Sqrt ((LinePoints[^2] - HoverPoint.X) * (LinePoints[^2] - HoverPoint.X) +
               (LinePoints[^1] - HoverPoint.Y) * (LinePoints[^1] - HoverPoint.Y)) : 0;
   #endregion

   #region Field-----------------------------------------------------
   public List<double> LinePoints = new ();
   #endregion
}

public struct Point {

   public Point (double x, double y) {
      X = x; Y = y;
   }

   public double X { get; set; }

   public double Y { get; set; }

   public static Point Parse (string input) {
      Point C = new ();
      if (C.TryParse (input, out Point f)) return f;
      throw new ArgumentException ("Input is not in correct format");
   }

   private readonly bool TryParse (string input, out Point f) {
      f = new Point ();
      if (input.Length <= 0) return false;
      string[] points = input.Split (',');
      if (points.Length == 2) {
         f = new (double.Parse (points[0]), double.Parse (points[1]));
         return true;
      }
      return false;
   }

   public override readonly string ToString () => ($"{X},{Y}");
}