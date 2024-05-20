using System.Collections.Generic;
using System.Text;
using System;
using Utils;

namespace BackEnd;
/// <summary>Class which creates custom shapes</summary>
public abstract class Shape {

   public List<Point> Points = new ();

   public string? Color { get; set; }

   public int Thickness { get; set; }

   public abstract string prompt { get; }

   public abstract Dictionary<string, double> GetDimension ();

   public abstract void Draw (DrawingCommands cmd);

}

public class Line : Shape {
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

   public override string prompt => Points.Count > 0 ? "Pick End point" : "Pick Beginning point";

   public double Length => Points.Count > 0 ? Math.Sqrt ((Points[^1].X - Points[0].X) * (Points[^1].X - Points[0].X) +
               (Points[^1].Y - Points[0].Y) * (Points[^1].Y - Points[0].Y)) : 0;
}
public class Rectangle : Shape {

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

   public override string prompt => Points.Count > 0 ? "Pick second corner of rectangle" : "Pick first corner of rectangle";

   public double Width => Points.Count > 0 ? (Points[^1].X - Points[0].X) : 0;

   public double Height => Points.Count > 0 ? (Points[^1].Y - Points[0].Y) : 0;
}

public class Circle : Shape {

   public double Radius { get { return GetRadius (); } set { mRadius = value; } }

   private double GetRadius () => mRadius = Math.Sqrt ((Points[^1].X - Points[0].X) * (Points[^1].X - Points[0].X) +
               (Points[^1].Y - Points[0].Y) * (Points[^1].Y - Points[0].Y));

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
      {"Radius",Radius}
   };

   public override string prompt => Points.Count > 0 ? "Pick point on circle" : "Pick center point";

   private double mRadius;
}

public class ConnectedLine : Shape {

   public double CurrentLineLength { get; set; }

   public List<(Point, Point)> LinePoints = new ();

   public override string ToString () {
      StringBuilder sb = new ();
      sb.AppendLine (Color);
      sb.AppendLine (Thickness.ToString ());
      sb.AppendLine (LinePoints.Count.ToString ());
      foreach (var item in LinePoints) {
         sb.AppendLine (item.Item1.ToString ());
         sb.AppendLine (item.Item2.ToString ());
      }
      return sb.ToString ();
   }

   public override void Draw (DrawingCommands cmd) {
      cmd.DrawConnectedLine ();
   }

   public override Dictionary<string, double> GetDimension () {
      throw new NotImplementedException ();
   }

   public override string prompt => Points.Count > 0 ? "Pick start point of connected Line" : "Pick start position of next line";
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