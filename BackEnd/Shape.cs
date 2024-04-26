using System.Collections.Generic;
using System.Text;
using System;

namespace BackEnd;
/// <summary>Class which creates custom shapes</summary>
public class Shape {

   public List<Point> Points = new ();

   public virtual string? Color { get; set; }

   public virtual int Thickness { get; set; }

   public virtual string prompt => string.Empty;

   public virtual Dictionary<string, double>? Dimension { get; set; }
}
public class Scribble : Shape {

   public override string ToString () {
      StringBuilder s = new ();
      s.AppendLine ("Scribble");
      s.AppendLine (Color);
      s.AppendLine (Thickness.ToString ());
      s.AppendLine (Points.Count.ToString ());
      foreach (var p in Points)
         s.AppendLine (p.ToString ());
      return s.ToString ();
   }

   public override string prompt { get => Points.Count > 0 ? "Pick endpoint for scribble" : "Start scribbling"; }

   public override Dictionary<string, double>? Dimension {
      get {
         Dictionary<string, double> st = new () {
            { "StartX", Points[0].X }
            ,{ "StartY", Points [0].Y },
         };
         return st;
      }
   }
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

   public override string prompt => Points.Count > 0 ? "Pick End point" : "Pick Beginning point";

   public double Length => Points.Count > 0 ? Math.Sqrt ((Points[^1].X - Points[0].X) * (Points[^1].X - Points[0].X) +
               (Points[^1].Y - Points[0].Y) * (Points[^1].Y - Points[0].Y)) : 0;

   public override Dictionary<string, double>? Dimension {
      get {
         Dictionary<string, double> st = new () {
            { "StartX", Points[0].X }
            ,{ "StartY", Points [0].Y }, {"Length",Length}
         };
         return st;
      }
   }
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

   public override string prompt => Points.Count > 0 ? "Pick second corner of rectangle" : "Pick first corner of rectangle";

   public double Width => Points.Count > 0 ? Math.Abs (Points[^1].X - Points[0].X) : 0;

   public double Height => Points.Count > 0 ? Math.Abs (Points[^1].Y - Points[0].Y) : 0;

   public override Dictionary<string, double>? Dimension {
      get {
         Dictionary<string, double> st = new () {
            { "StartX", Points[0].X } ,{ "StartY", Points [0].Y }
            , {"Width",Width}, {"Height",Height }
         };
         return st;
      }
   }
}

public class Circle : Shape {

   public double Radius { get { return GetRadius (); } set { mRadius = value; } }

   private double GetRadius () {
      mRadius = Math.Sqrt ((Points[^1].X - Points[0].X) * (Points[^1].X - Points[0].X) +
               (Points[^1].Y - Points[0].Y) * (Points[^1].Y - Points[0].Y));
      double a = 500 - Math.Abs (Points[0].Y), b = 1000 - Math.Abs (Points[0].X);
      if (mRadius <= a && mRadius <= b)
         return mRadius;
      return mRadius - 1;
   }

   public override string ToString () {
      StringBuilder s = new ();
      s.AppendLine ("Circle");
      s.AppendLine (Color);
      s.AppendLine (Thickness.ToString ());
      s.AppendLine (Points[0].ToString ());
      s.AppendLine (Radius.ToString ());
      return s.ToString ();
   }

   public override string prompt => Points.Count > 0 ? "Pick point on circle" : "Pick center point";

   public override Dictionary<string, double>? Dimension {
      get {
         Dictionary<string, double> st = new () {
            { "StartX", Points[0].X }
            ,{ "StartY", Points [0].Y }, {"Radius",Radius}
         };
         return st;
      }
   }

   private double mRadius;
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