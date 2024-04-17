using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System;
using BackEnd;
using Point = BackEnd.Point;
using Line = BackEnd.Line;
using Rectangle = BackEnd.Rectangle;
using System.Windows.Controls;

namespace CADP;

public partial class Canvas : System.Windows.Controls.Canvas {

   #region Constructor---------
   public Canvas () {
      IsNewFile = true;
      IsModified = false;
      mCurrentShapeColor = "#000000";
      mCurrentThickness = 1;
      mScaleValue = 1;
   }
   #endregion 

   #region Methods-------------
   public void ScribbleOn () => mCurrentShape = new Scribble ();

   public void LineOn () => mCurrentShape = new Line ();

   public void RectOn () => mCurrentShape = new Rectangle ();

   public void CircleOn () => mCurrentShape = new Circle ();

   public void Pick () => mCurrentShape = new Shapes ();

   public void Undo () {
      if (mShapes.Count == 0) return;
      mUndoShapes.Push (mShapes.Last ());
      mShapes.RemoveAt (mShapes.Count - 1);
      mPressedUndo = true;
      InvalidateVisual ();
   }

   public void Redo () {
      if (mUndoShapes.Count == 0) return;
      mShapes.Add (mUndoShapes.Pop ());
      InvalidateVisual ();
   }

   public void Open () {
      if (mFile.OpenFile (out List<Shapes> f)) {
         AllShapes = f;
         IsNewFile = false;
         InvalidateVisual ();
      }
   }

   public void Save () {
      if (IsNewFile) {
         if (mFile.SaveAs (AllShapes, true, true))
            IsNewFile = false;
         else return;
      } else
         mFile.Save (AllShapes);
      IsModified = false;
      RenderMainWindowTools (false);
   }

   public void SaveAs (bool IsText) {
      if (mFile.SaveAs (AllShapes, IsText, IsNewFile)) { IsNewFile = false; IsModified = false; }
      RenderMainWindowTools (false);
   }

   public void Zoom (bool IsZoomIn) {
      mScaleValue += IsZoomIn ? 0.1 : -0.1;
      mScaleValue = mScaleValue < 0.8 ? 0.8 : mScaleValue;
      mScaleValue = mScaleValue > 10.0 ? 10.0 : mScaleValue;
      ScaleTransform scaleTransform = new (mScaleValue, mScaleValue);
      LayoutTransform = scaleTransform;
   }

   public void RenderMainWindowTools (bool IsInputbar) {
      MainWindow ParentWindow = (MainWindow)Window.GetWindow (VisualParent);
      if (IsInputbar) ParentWindow.Inputbar.InvalidateVisual ();
      else ParentWindow.undoredo.InvalidateVisual ();
   }
   #endregion

   #region Overrides-----------
   protected override void OnRender (DrawingContext dc) {
      base.OnRender (dc);
      RenderMainWindowTools (false);
      foreach (var shape in mShapes) {
         switch (shape) {
            case Scribble scr:
               Color sc = (Color)ColorConverter.ConvertFromString (scr.Color);
               Brush sb = new SolidColorBrush (sc);
               Pen sPen = new (sb, scr.Thickness);
               for (int i = 0; i < scr.Points.Count - 1; i++) {
                  double[] A = UnitConverter.InverseTransform (scr.Points[i]);
                  double[] B = UnitConverter.InverseTransform (scr.Points[i + 1]);
                  dc.DrawLine (sPen, new System.Windows.Point (A[0], A[1]),
                     new System.Windows.Point (B[0], B[1]));
               }
               break;
            case Rectangle rectangle:
               Color rc = (Color)ColorConverter.ConvertFromString (rectangle.Color);
               Brush rb = new SolidColorBrush (rc);
               Pen rPen = new (rb, rectangle.Thickness);
               double[] C = UnitConverter.InverseTransform (rectangle.Points[0]);
               double[] D = UnitConverter.InverseTransform (rectangle.Points[^1]);
               Rect r = new (new System.Windows.Point (C[0], C[1]), new System.Windows.Point (D[0], D[1]));
               dc.DrawRectangle (Background, rPen, r);
               break;
            case Line line:
               Color lc = (Color)ColorConverter.ConvertFromString (line.Color);
               Brush lb = new SolidColorBrush (lc);
               Pen lPen = new (lb, line.Thickness);
               double[] E = UnitConverter.InverseTransform (line.Points[0]);
               double[] F = UnitConverter.InverseTransform (line.Points[^1]);
               dc.DrawLine (lPen, new System.Windows.Point (E[0], E[1]),
                  new System.Windows.Point (F[0], F[1]));
               break;
            case Circle circle:
               Color cc = (Color)ColorConverter.ConvertFromString (circle.Color);
               Brush cb = new SolidColorBrush (cc);
               Pen cPen = new (cb, circle.Thickness);
               double[] G = UnitConverter.InverseTransform (circle.Points[0]);
               dc.DrawEllipse (Background, cPen, new System.Windows.Point (G[0], G[1]), circle.Radius, circle.Radius);
               break;
         }
      }
   }

   protected override void OnMouseDown (MouseButtonEventArgs e) {
      base.OnMouseDown (e);
      if (mCurrentShape == null || mCurrentShape.Points.Count > 1) return;
      if (e.LeftButton == MouseButtonState.Pressed) {
         switch (mCurrentShape) {
            case Scribble scr:
               scr.Points.Add (UnitConverter.Transform (e.GetPosition (this).X, e.GetPosition (this).Y));
               break;
            case Rectangle rect:
               rect.Points.Add (UnitConverter.Transform (e.GetPosition (this).X, e.GetPosition (this).Y));
               break;
            case Line line:
               line.Points.Add (UnitConverter.Transform (e.GetPosition (this).X, e.GetPosition (this).Y));
               break;
            case Circle circle:
               circle.Points.Add (UnitConverter.Transform (e.GetPosition (this).X, e.GetPosition (this).Y));
               break;
         }
         mCurrentShape.Color = mCurrentShapeColor;
         mCurrentShape.Thickness = mCurrentThickness;
         mShapes.Add (mCurrentShape);
         if (mPressedUndo) mUndoShapes.Clear ();
      }
   }

   protected override void OnMouseMove (MouseEventArgs e) {
      base.OnMouseMove (e);
      if (mCurrentShape == null || e.LeftButton != MouseButtonState.Released || mCurrentShape.Points.Count < 1) return;
      IsModified = true;
      switch (mCurrentShape) {
         case Scribble sr:
            sr.Points.Add (UnitConverter.Transform (e.GetPosition (this).X, e.GetPosition (this).Y));
            break;
         case Rectangle rect:
            rect.Points.Add (UnitConverter.Transform (e.GetPosition (this).X, e.GetPosition (this).Y));
            break;
         case Line line:
            line.Points.Add (UnitConverter.Transform (e.GetPosition (this).X, e.GetPosition (this).Y));
            break;
         case Circle circle:
            Point p = UnitConverter.Transform (e.GetPosition (this).X, e.GetPosition (this).Y);
            double radius = Math.Sqrt ((p.X - circle.Points[0].X) * (p.X - circle.Points[0].X) +
               (p.Y - circle.Points[0].Y) * (p.Y - circle.Points[0].Y));
            double a = 500 - Math.Abs (circle.Points[0].Y), b = 1000 - Math.Abs (circle.Points[0].X);
            if (radius <= a && radius <= b)
               circle.Radius = radius;
            break;
      }
      IsDrawing = true;
      RenderMainWindowTools (true);
      InvalidateVisual ();
   }

   protected override void OnMouseLeftButtonDown (MouseButtonEventArgs e) {
      base.OnMouseLeftButtonDown (e);
      if (mCurrentShape == null || e.LeftButton != MouseButtonState.Pressed || mCurrentShape.Points.Count <= 1) return;
      switch (mCurrentShape) {
         case Scribble:
            mCurrentShape = new Scribble (); break;
         case Rectangle:
            mCurrentShape = new Rectangle (); break;
         case Line:
            mCurrentShape = new Line (); break;
         case Circle:
            mCurrentShape = new Circle (); break;
      }
      IsDrawing = false;
      RenderMainWindowTools (true);
   }

   protected override void OnMouseWheel (MouseWheelEventArgs e) {
      base.OnMouseWheel (e);
      Zoom (e.Delta > 0);
   }
   #endregion

   #region Properties----------
   public List<Shapes> AllShapes { get => mShapes; set => mShapes = value; }

   public FileManager CanvasFileManager => mFile;

   public bool IsNewFile { get; set; }

   public bool IsModified { get; set; }

   public bool IsDrawing { get; set; }

   public string CurrentShapeColor { set => mCurrentShapeColor = value; }

   public int CurrentShapeThickness { set => mCurrentThickness = value; }

   public int UndoShapeCount => mUndoShapes.Count;

   public string CurrentShapePrompt {
      get {
         if (mCurrentShape != null)
            return mCurrentShape.prompt;
         return "Pick any shapes to draw";
      }
   }

   public StackPanel CurrentShapeStack {
      get {
         if (mCurrentShape != null)
            return mCurrentShape.AddStack ();
         return new StackPanel () { Background = Brushes.AliceBlue };
      }
   }
   #endregion

   #region Private ------------
   private Shapes? mCurrentShape;
   private List<Shapes> mShapes = new ();
   private readonly Stack<Shapes> mUndoShapes = new ();
   private readonly FileManager mFile = new ();
   private int mCurrentThickness;
   private double mScaleValue;
   private bool mPressedUndo;
   private string mCurrentShapeColor;
   #endregion
}