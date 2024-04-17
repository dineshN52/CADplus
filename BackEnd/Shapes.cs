using System.Collections.Generic;
using System.Text;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace BackEnd;
/// <summary>Class which creates custom shapes</summary>
public class Shapes {

   public List<Point> Points = new ();

   public virtual string? Color { get; set; }

   public virtual int Thickness { get; set; }

   public virtual string prompt => string.Empty;

   public virtual StackPanel AddStack () {
      StackPanel st = new () {
         Orientation = Orientation.Horizontal
      };
      return st;
   }
}
public class Scribble : Shapes {

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

   public override StackPanel AddStack () {
      StackPanel st = new () {
         Orientation = Orientation.Horizontal,
      };
      st.Children.Add (new TextBlock () { Text = "X", Height = 15, Width = 20, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Points.Count > 0 ? Points[0].X.ToString () : "0", Height = 15, Width = 45 });
      st.Children.Add (new TextBlock () { Text = "Y", Height = 15, Width = 20, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Points.Count > 0 ? Points[0].Y.ToString () : "0", Height = 15, Width = 45 });
      return st;
   }

}
public class Line : Shapes {
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

   public override StackPanel AddStack () {
      StackPanel st = new () {
         Orientation = Orientation.Horizontal
      };
      st.Children.Add (new TextBlock () { Text = "X", Height = 15, Width = 20, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Points.Count > 0 ? Math.Round (Points[0].X, 3).ToString () : "0", Height = 15, Width = 45 });
      st.Children.Add (new TextBlock () { Text = "Y", Height = 15, Width = 20, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Points.Count > 0 ? Math.Round (Points[0].Y, 3).ToString () : "0", Height = 15, Width = 45 });
      st.Children.Add (new TextBlock () { Text = "Length", Height = 15, Width = 40, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Math.Round (Length, 3).ToString (), Height = 15, Width = 45 });
      return st;
   }
}
public class Rectangle : Shapes {

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

   public override StackPanel AddStack () {
      StackPanel st = new () {
         Orientation = Orientation.Horizontal
      };
      st.Children.Add (new TextBlock () { Text = "X", Height = 15, Width = 20, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Points.Count > 0 ? Math.Round (Points[0].X, 3).ToString () : "0", Height = 15, Width = 45 });
      st.Children.Add (new TextBlock () { Text = "Y", Height = 15, Width = 15, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Points.Count > 0 ? Math.Round (Points[0].Y, 3).ToString () : "0", Height = 15, Width = 45 });
      st.Children.Add (new TextBlock () { Text = "Width", Height = 15, Width = 40, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Math.Round (Width, 3).ToString (), Height = 15, Width = 45 });
      st.Children.Add (new TextBlock () { Text = "Height", Height = 15, Width = 40, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Math.Round (Height, 3).ToString (), Height = 15, Width = 45 });
      return st;
   }
}

public class Circle : Shapes {
   public double Radius { get; set; }

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

   public override StackPanel AddStack () {
      StackPanel st = new () {
         Orientation = Orientation.Horizontal
      };
      st.Children.Add (new TextBlock () { Text = "X", Height = 15, Width = 20, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Points.Count > 0 ? Math.Round (Points[0].X, 3).ToString () : "0", Height = 15, Width = 45 });
      st.Children.Add (new TextBlock () { Text = "Y", Height = 15, Width = 20, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Points.Count > 0 ? Math.Round (Points[0].Y, 3).ToString () : "0", Height = 15, Width = 45 });
      st.Children.Add (new TextBlock () { Text = "Radius", Height = 15, Width = 40, Background = Brushes.Transparent });
      st.Children.Add (new TextBox () { Text = Math.Round (Radius, 3).ToString (), Height = 15, Width = 45 });
      return st;
   }
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

   private bool TryParse (string input, out Point f) {
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