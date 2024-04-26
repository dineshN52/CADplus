using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Shapes;

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
            if (allShapes[0] is "Scribble" or "Rectangle" or "Line" or "Circle") f = Open (allShapes);
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
            case "Scribble":
               Scribble sr = new () {
                  Color = allShapes[i + 1], Thickness = int.Parse (allShapes[i + 2])
               };
               limits = limits + 4 + int.Parse (allShapes[i + 3]);
               for (int j = i + 4; j < limits; j++)
                  sr.Points.Add (Point.Parse (allShapes[j]));
               all.Add (sr);
               break;
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
               Scribble scr = new () {
                  Color = Encoding.Default.GetString (br.ReadBytes (7)),
                  Thickness = BitConverter.ToInt32 (br.ReadBytes (4), 0)
               };
               int count = BitConverter.ToInt32 (br.ReadBytes (4), 0);
               for (int j = scr.Points.Count; j < count; j++) {
                  Point p = new (BitConverter.ToDouble (br.ReadBytes (8)), BitConverter.ToDouble (br.ReadBytes (8)));
                  scr.Points.Add (p);
               }
               all.Add (scr);
               counts -= (count * 16) + 20;
               break;
            case 2:
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
            case 3:
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
            case 4:
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
            case Scribble scr:
               bw.Write (1);
               if (scr.Color != null) bw.Write (scr.Color);
               bw.Write (scr.Thickness);
               bw.Write (scr.Points.Count);
               foreach (var point in scr.Points) {
                  bw.Write (point.X);
                  bw.Write (point.Y);
               }
               break;
            case Line line:
               bw.Write (2);
               if (line.Color != null) bw.Write (line.Color);
               bw.Write (line.Thickness);
               bw.Write (line.Points[0].X);
               bw.Write (line.Points[0].Y);
               bw.Write (line.Points[^1].X);
               bw.Write (line.Points[^1].Y);
               break;
            case Rectangle rect:
               bw.Write (3);
               if (rect.Color != null) bw.Write (rect.Color);
               bw.Write (rect.Thickness);
               bw.Write (rect.Points[0].X);
               bw.Write (rect.Points[0].Y);
               bw.Write (rect.Points[^1].X);
               bw.Write (rect.Points[^1].Y);
               break;
            case Circle circle:
               bw.Write (4);
               if (circle.Color != null) bw.Write (circle.Color);
               bw.Write (circle.Thickness);
               bw.Write (circle.Points[0].X);
               bw.Write (circle.Points[0].Y);
               bw.Write (circle.Radius);
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