using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BackEnd;

/// <summary>Class to manage opening and saving of the files(Text and Binary files)</summary>
public class FileManager {

   #region Methods-------------
   public bool OpenFile (out List<Shapes> f) {
      OpenFileDialog open = new () {
         Filter = "Text Files(*.txt)|*.txt|BIN Files(*.bin)|*.bin",
      };
      f = new List<Shapes> ();
      if (open.ShowDialog () == true) {
         int i = open.FilterIndex;
         if (i == 1) {
            string[] allShapes = File.ReadAllLines (open.FileName);
            if (allShapes[0] == "Scribble" || allShapes[0] == "Rectangle" || allShapes[0] == "Line" || allShapes[0] == "Circle") f = Open (allShapes);
            else throw new FormatException ("Input is not in correct format");
         } else if (i == 2) {
            byte[] allBytes = File.ReadAllBytes (open.FileName);
            f = Open (allBytes);
         }
         mCurrentFileName = open.FileName;
         return true;
      }
      return false;
   }

   private static List<Shapes> Open (string[] allShapes) {
      List<Shapes> all = new ();
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

   private static List<Shapes> Open (byte[] allBytes) {
      List<Shapes> all = new ();
      int limits = 0;
      for (int i = 0; i < allBytes.Length; i = limits) {
         byte[] shape = new byte[4]; byte[] color = new byte[7]; byte[] thickness = new byte[4];
         Array.Copy (allBytes, i, shape, 0, 4);
         Array.Copy (allBytes, i + 5, color, 0, 7);
         Array.Copy (allBytes, i + 12, thickness, 0, 4);
         int s = BitConverter.ToInt32 (shape, 0);
         switch (s) {
            case 1:
               byte[] count = new byte[4];
               Array.Copy (allBytes, i + 16, count, 0, 4);
               Scribble scr = new () {
                  Color = Encoding.Default.GetString (color),
                  Thickness = BitConverter.ToInt32 (thickness, 0)
               };
               limits = (limits + 20) + (BitConverter.ToInt32 (count, 0) * 16);
               for (int j = i + 20; j < limits; j += 16) {
                  byte[] x = new byte[8]; byte[] y = new byte[8];
                  Array.Copy (allBytes, j, x, 0, 8);
                  Array.Copy (allBytes, j + 8, y, 0, 8);
                  Point p = new (BitConverter.ToDouble (x), BitConverter.ToDouble (y));
                  scr.Points.Add (p);
               }
               all.Add (scr);
               break;
            case 2:
               Line line = new () {
                  Color = Encoding.Default.GetString (color),
                  Thickness = BitConverter.ToInt32 (thickness, 0)
               };
               limits += 48;
               for (int j = i + 16; j < limits; j += 32) {
                  byte[] x1 = new byte[8]; byte[] y1 = new byte[8]; byte[] x2 = new byte[8]; byte[] y2 = new byte[8];
                  Array.Copy (allBytes, j, x1, 0, 8);
                  Array.Copy (allBytes, j + 8, y1, 0, 8);
                  Array.Copy (allBytes, j + 16, x2, 0, 8);
                  Array.Copy (allBytes, j + 24, y2, 0, 8);
                  Point X = new (BitConverter.ToDouble (x1), BitConverter.ToDouble (y1));
                  Point Y = new (BitConverter.ToDouble (x2), BitConverter.ToDouble (y2));
                  line.Points.Add (X); line.Points.Add (Y);
               }
               all.Add (line);
               break;
            case 3:
               Rectangle rect = new () {
                  Color = Encoding.Default.GetString (color),
                  Thickness = BitConverter.ToInt32 (thickness, 0)
               };
               limits += 48;
               for (int j = i + 16; j < limits; j += 32) {
                  byte[] startX = new byte[8]; byte[] startY = new byte[8]; byte[] endX = new byte[8]; byte[] endY = new byte[8];
                  Array.Copy (allBytes, j, startX, 0, 8);
                  Array.Copy (allBytes, j + 8, startY, 0, 8);
                  Array.Copy (allBytes, j + 16, endX, 0, 8);
                  Array.Copy (allBytes, j + 24, endY, 0, 8);
                  Point X = new (BitConverter.ToDouble (startX), BitConverter.ToDouble (startY));
                  Point Y = new (BitConverter.ToDouble (endX), BitConverter.ToDouble (endY));
                  rect.Points.Add (X); rect.Points.Add (Y);
               }
               all.Add (rect);
               break;
            case 4:
               Circle circle = new () {
                  Color = Encoding.Default.GetString (color),
                  Thickness = BitConverter.ToInt32 (thickness, 0)
               };
               limits += 40;
               for (int j = i + 16; j < limits; j += 24) {
                  byte[] centerX = new byte[8]; byte[] centerY = new byte[8]; byte[] radius = new byte[8];
                  Array.Copy (allBytes, j, centerX, 0, 8);
                  Array.Copy (allBytes, j + 8, centerY, 0, 8);
                  Array.Copy (allBytes, j + 16, radius, 0, 8);
                  Point center = new (BitConverter.ToDouble (centerX), BitConverter.ToDouble (centerY));
                  circle.Radius = BitConverter.ToDouble (radius, 0);
                  circle.Points.Add (center);
               }
               all.Add (circle);
               break;
            default:
               throw new FormatException ("Input is not in correct format");
         }
      }
      return all;
   }


   public bool SaveAs (List<Shapes> allShapes, bool IsText, bool IsNewFile) {
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

   public void Save (List<Shapes> allShapes) {
      if (mCurrentFileName != null) {
         string ext = string.Join ("", mCurrentFileName.TakeLast (3).ToArray ());
         bool IsText = ext == "txt";
         ActualSave (allShapes, IsText, mCurrentFileName);
      }
   }

   private static void ActualSave (List<Shapes> allShapes, bool IsText, string fileName) {
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
   private static BinaryWriter BinaryWrite (ref BinaryWriter bw, List<Shapes> allShapes) {
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

   #region Field---------------
   public string mCurrentFileName = string.Empty;
   #endregion
}