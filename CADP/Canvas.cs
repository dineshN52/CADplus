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

namespace CADP;

public partial class Canvas : System.Windows.Controls.Canvas {

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
         mNewfile = false;
         InvalidateVisual ();
      }
   }

   public void Save () {
      if (mNewfile) {
         if (mFile.SaveAs (AllShapes, true, true))
            mNewfile = false;
         else return;
      } else
         mFile.Save (AllShapes);
      mModified = false;
      InvalidateVisual ();
   }

   public void SaveAs (bool IsText) {
      if (mFile.SaveAs (AllShapes, IsText, mNewfile)) { mNewfile = false; mModified = false; }
      InvalidateVisual ();
   }

   public void Zoom (bool IsZoomIn) {
      mScaleValue += IsZoomIn ? 0.1 : -0.1;
      mScaleValue = mScaleValue < 0.8 ? 0.8 : mScaleValue;
      mScaleValue = mScaleValue > 10.0 ? 10.0 : mScaleValue;
      ScaleTransform scaleTransform = new (mScaleValue, mScaleValue);
      this.LayoutTransform = scaleTransform;
   }
   #endregion

   #region Overrides-----------
   protected override void OnRender (DrawingContext dc) {
      base.OnRender (dc);
      MainWindow ParentWindow = (MainWindow)Window.GetWindow (VisualParent);
      ParentWindow.InvalidateVisual ();
      foreach (var shape in mShapes) {
         switch (shape) {
            case Scribble scr:
               Color sc = (Color)ColorConverter.ConvertFromString (scr.Color);
               Brush sb = new SolidColorBrush (sc);
               Pen sPen = new (sb, scr.Thickness);
               for (int i = 0; i < scr.Points.Count - 1; i++)
                  dc.DrawLine (sPen, new System.Windows.Point (scr.Points[i].X, scr.Points[i].Y),
                     new System.Windows.Point (scr.Points[i + 1].X, scr.Points[i + 1].Y));
               break;
            case Rectangle rectangle:
               Color rc = (Color)ColorConverter.ConvertFromString (rectangle.Color);
               Brush rb = new SolidColorBrush (rc);
               Pen rPen = new (rb, rectangle.Thickness);
               Point A = rectangle.Points[0]; Point B = rectangle.Points[^1];
               Rect r = new (new System.Windows.Point (A.X, A.Y), new System.Windows.Point (B.X, B.Y));
               dc.DrawRectangle (Background, rPen, r);
               break;
            case Line line:
               Color lc = (Color)ColorConverter.ConvertFromString (line.Color);
               Brush lb = new SolidColorBrush (lc);
               Pen lPen = new (lb, line.Thickness);
               dc.DrawLine (lPen, new System.Windows.Point (line.Points[0].X, line.Points[0].Y),
                  new System.Windows.Point (line.Points[^1].X, line.Points[^1].Y));
               break;
            case Circle circle:
               Color cc = (Color)ColorConverter.ConvertFromString (circle.Color);
               Brush cb = new SolidColorBrush (cc);
               Pen cPen = new (cb, circle.Thickness);
               Point C = circle.Points[0];
               dc.DrawEllipse (Background, cPen, new System.Windows.Point (C.X, C.Y), circle.Radius, circle.Radius);
               break;
         }
      }
   }

   protected override void OnMouseDown (MouseButtonEventArgs e) {
      base.OnMouseDown (e);
      if (e.LeftButton == MouseButtonState.Pressed) {
         switch (mCurrentShape) {
            case Scribble scr:
               scr.Points.Add (new Point (e.GetPosition (this).X, e.GetPosition (this).Y));
               break;
            case Rectangle rect:
               rect.Points.Add (new Point (e.GetPosition (this).X, e.GetPosition (this).Y));
               break;
            case Line line:
               line.Points.Add (new Point (e.GetPosition (this).X, e.GetPosition (this).Y));
               break;
            case Circle circle:
               circle.Points.Add (new Point (e.GetPosition (this).X, e.GetPosition (this).Y));
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
      if (e.LeftButton != MouseButtonState.Released || mCurrentShape.Points.Count < 1) return;
      mModified = true;
      switch (mCurrentShape) {
         case Scribble sr:
            sr.Points.Add (new Point (e.GetPosition (this).X, e.GetPosition (this).Y));
            InvalidateVisual ();
            break;
         case Rectangle rect:
            rect.Points.Add (new Point (e.GetPosition (this).X, e.GetPosition (this).Y));
            InvalidateVisual ();
            break;
         case Line line:
            line.Points.Add (new Point (e.GetPosition (this).X, e.GetPosition (this).Y));
            InvalidateVisual ();
            break;
         case Circle circle:
            double radius = Math.Sqrt ((e.GetPosition (this).X - circle.Points[0].X) * (e.GetPosition (this).X - circle.Points[0].X) +
               (e.GetPosition (this).Y - circle.Points[0].Y) * (e.GetPosition (this).Y - circle.Points[0].Y));
            double a = circle.Points[0].Y, b = circle.Points[0].X;
            if (radius <= a && radius <= this.ActualHeight - a && radius <= b && radius <= this.ActualWidth - a) circle.Radius = radius;
            InvalidateVisual ();
            break;
      }
   }

   protected override void OnMouseLeftButtonDown (MouseButtonEventArgs e) {
      base.OnMouseLeftButtonDown (e);
      if (e.LeftButton != MouseButtonState.Pressed || mCurrentShape.Points.Count <= 1) return;
      mShapes.RemoveAt (mShapes.Count - 1);
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
   }

   protected override void OnMouseWheel (MouseWheelEventArgs e) {
      base.OnMouseWheel (e);
      Zoom (e.Delta > 0);
   }
   #endregion

   #region Properties----------
   public List<Shapes> AllShapes { get => mShapes; set => mShapes = value; }

   public FileManager CanvasFileManager => mFile;

   public bool IsNewFile => mNewfile;

   public bool IsModified => mModified;

   public string CurrentShapeColor { set => mCurrentShapeColor = value; }

   public int CurrentShapeThickness { set => mCurrentThickness = value; }

   public int UndoShapeCount => mUndoShapes.Count;
   #endregion

   #region Private ------------
   private Shapes mCurrentShape = new ();
   private List<Shapes> mShapes = new ();
   private readonly Stack<Shapes> mUndoShapes = new ();
   private readonly FileManager mFile = new ();
   private int mCurrentThickness = 1;
   private double mScaleValue = 1.0;
   private bool mModified = false;
   private bool mNewfile = true;
   private bool mPressedUndo;
   private string mCurrentShapeColor = "#000000";
   #endregion
}