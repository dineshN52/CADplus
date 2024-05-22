using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BackEnd;

/// <summary>Class to manage opening and saving of the files(Text and Binary files)</summary>
public class FileManager {

   #region Methods---------------------------------------------------
   public bool OpenFile (out List<Shape> f) {
      OpenFileDialog open = new () {
         Filter = "Text Files(*.txt)|*.txt|BIN Files(*.bin)|*.bin",
      };
      f = new List<Shape> ();
      if (open.ShowDialog () == true) {
         int i = open.FilterIndex;
         if (i == 1) {
            string[] allShapes = File.ReadAllLines (open.FileName);
            if (allShapes[0] is "Scribble" or "Rectangle" or "Line" or "Circle" or "ConnectedLine") f = Open (allShapes);
            else throw new FormatException ("Input is not in correct format");
         } else if (i == 2) {
            using FileStream fs = new (open.FileName, FileMode.Open);
            BinaryReader Br = new (fs);
            long numBytes = new FileInfo (open.FileName).Length;
            f = Open (Br, (int)numBytes);
         }
         mCurrentFileName = open.FileName;
         return true;
      }
      return false;
   }

   private static List<Shape> Open (string[] allShapes) {
      List<Shape> all = new ();
      int limits = 0;
      for (int i = 0; i < allShapes.Length; i = limits) {
         string s = allShapes[i];
         switch (s) {
            case "Line":
               Line line = new () {
                  Color = allShapes[i + 1], Thickness = int.Parse (allShapes[i + 2])
               };
               limits += 5;
               line.Points.Add (Point.Parse (allShapes[i + 3]));
               line.Points.Add (Point.Parse (allShapes[i + 4]));
               all.Add (line);
               break;
            case "Rectangle":
               Rectangle rect = new () {
                  Color = allShapes[i + 1], Thickness = int.Parse (allShapes[i + 2])
               };
               limits += 5;
               rect.Points.Add (Point.Parse (allShapes[i + 3]));
               rect.Points.Add (Point.Parse (allShapes[i + 4]));
               all.Add (rect);
               break;
            case "Circle":
               Circle circle = new () {
                  Color = allShapes[i + 1], Thickness = int.Parse (allShapes[i + 2])
               };
               limits += 5;
               circle.Points.Add (Point.Parse (allShapes[i + 3]));
               circle.Radius = double.Parse (allShapes[i + 4]);
               all.Add (circle);
               break;
            case "ConnectedLine":
               ConnectedLine cLine = new () {
                  Color = allShapes[i + 1], Thickness = int.Parse (allShapes[i + 2])
               };
               int limit = (int.Parse (allShapes[i + 3]) - 2) / 2;
               for (int j = 0; j < limit; j++) {
                  Point p = Point.Parse (allShapes[i + j + 4]);
                  cLine.LinePoints.Add (p.X);
                  cLine.LinePoints.Add (p.Y);
               }
               cLine.HoverPoint = Point.Parse (allShapes[i + limit + 4]);
               limits = limits + limit + 5;
               all.Add (cLine);
               break;
         }
      }
      return all;
   }

   public List<Shape> Open (BinaryReader br, int counts) {
      List<Shape> all = new ();
      while (counts > 0) {
         var s = br.ReadBytes (4);
         int sr = BitConverter.ToInt32 (s, 0);
         br.Read ();
         switch (sr) {
            case 1:
               Line line = new () {
                  Color = Encoding.Default.GetString (br.ReadBytes (7)),
                  Thickness = BitConverter.ToInt32 (br.ReadBytes (4), 0)
               };
               Point X = new (BitConverter.ToDouble (br.ReadBytes (8)), BitConverter.ToDouble (br.ReadBytes (8)));
               Point Y = new (BitConverter.ToDouble (br.ReadBytes (8)), BitConverter.ToDouble (br.ReadBytes (8)));
               line.Points.Add (X); line.Points.Add (Y);
               all.Add (line);
               counts -= 48;
               break;
            case 2:
               Rectangle rect = new () {
                  Color = Encoding.Default.GetString (br.ReadBytes (7)),
                  Thickness = BitConverter.ToInt32 (br.ReadBytes (4), 0)
               };
               Point P = new (BitConverter.ToDouble (br.ReadBytes (8)), BitConverter.ToDouble (br.ReadBytes (8)));
               Point Q = new (BitConverter.ToDouble (br.ReadBytes (8)), BitConverter.ToDouble (br.ReadBytes (8)));
               rect.Points.Add (P); rect.Points.Add (Q);
               all.Add (rect);
               counts -= 48;
               break;
            case 3:
               Circle circle = new () {
                  Color = Encoding.Default.GetString (br.ReadBytes (7)),
                  Thickness = BitConverter.ToInt32 (br.ReadBytes (4), 0)
               };
               Point center = new (BitConverter.ToDouble (br.ReadBytes (8)), BitConverter.ToDouble (br.ReadBytes (8)));
               circle.Radius = BitConverter.ToDouble (br.ReadBytes (8), 0);
               circle.Points.Add (center);
               all.Add (circle);
               counts -= 40;
               break;
            case 4:
               ConnectedLine cline = new () {
                  Color = Encoding.Default.GetString (br.ReadBytes (7)),
                  Thickness = BitConverter.ToInt32 (br.ReadBytes (4), 0)
               };
               int limit = BitConverter.ToInt32 (br.ReadBytes (4));
               List<double> Coordinates = new ();
               for (int i = 0; i < limit - 16; i += 8)
                  Coordinates.Add (BitConverter.ToDouble (br.ReadBytes (8)));
               cline.HoverPoint = new Point (BitConverter.ToDouble (br.ReadBytes (8)), BitConverter.ToDouble (br.ReadBytes (8)));
               cline.LinePoints = Coordinates;
               all.Add (cline);
               counts -= limit + 20;
               break;
            default:
               throw new FormatException ("Input is not in correct format");
         }
      }
      return all;
   }

   public bool SaveAs (List<Shape> allShapes, bool IsText, bool IsNewFile) {
      SaveFileDialog dialog = new () {
         FileName = "Untitled",
         Filter = "Text Files(*.txt)|*.txt|BIN Files(*.bin)|*.bin|All(*.*)|*",
         DefaultExt = IsText ? "*.txt" : "*.bin",
         FilterIndex = IsText ? 1 : 2
      };
      if (dialog.ShowDialog () == true) {
         string s = string.Join ("", dialog.FileName.TakeLast (3).ToArray ());
         IsText = s == "txt";
         ActualSave (allShapes, IsText, dialog.FileName);
         if (IsNewFile) { mCurrentFileName = dialog.FileName; return true; }
      }
      return false;
   }

   public void Save (List<Shape> allShapes) {
      if (mCurrentFileName != null) {
         string ext = string.Join ("", mCurrentFileName.TakeLast (3).ToArray ());
         bool IsText = ext == "txt";
         ActualSave (allShapes, IsText, mCurrentFileName);
      }
   }

   private static void ActualSave (List<Shape> allShapes, bool IsText, string fileName) {
      if (IsText) {
         StringBuilder newFile = new ();
         foreach (var file in allShapes)
            newFile.Append (file.ToString ());
         File.WriteAllText (fileName, newFile.ToString ());
      } else {
         using FileStream fs = new (fileName, FileMode.Create);
         BinaryWriter bw = new (fs);
         bw = BinaryWrite (ref bw, allShapes);
      }
   }

   private static BinaryWriter BinaryWrite (ref BinaryWriter bw, List<Shape> allShapes) {
      foreach (var file in allShapes) {
         switch (file) {
            case Line line:
               bw.Write (1);
               if (line.Color != null) bw.Write (line.Color);
               bw.Write (line.Thickness);
               bw.Write (line.Points[0].X);
               bw.Write (line.Points[0].Y);
               bw.Write (line.Points[^1].X);
               bw.Write (line.Points[^1].Y);
               break;
            case Rectangle rect:
               bw.Write (2);
               if (rect.Color != null) bw.Write (rect.Color);
               bw.Write (rect.Thickness);
               bw.Write (rect.Points[0].X);
               bw.Write (rect.Points[0].Y);
               bw.Write (rect.Points[^1].X);
               bw.Write (rect.Points[^1].Y);
               break;
            case Circle circle:
               bw.Write (3);
               if (circle.Color != null) bw.Write (circle.Color);
               bw.Write (circle.Thickness);
               bw.Write (circle.Points[0].X);
               bw.Write (circle.Points[0].Y);
               bw.Write (circle.Radius);
               break;
            case ConnectedLine cLine:
               bw.Write (4);
               if (cLine.Color != null) bw.Write (cLine.Color);
               bw.Write (cLine.Thickness);
               bw.Write (cLine.LinePoints.Count * 8);
               foreach (var item in cLine.LinePoints)
                  bw.Write (item);
               break;
         }
      }
      return bw;
   }
   #endregion

   #region Field-----------------------------------------------------
   public string mCurrentFileName = string.Empty;
   #endregion
}