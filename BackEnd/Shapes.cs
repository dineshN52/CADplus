using System.Collections.Generic;
using System.Text;
using System;

namespace BackEnd;
/// <summary>Class which creates custom shapes</summary>
public class Shapes {

    public List<Point> Points = new ();

    public virtual string? Color { get; set; }

    public virtual int Thickness { get; set; }

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