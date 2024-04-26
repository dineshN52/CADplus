using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System;
using BackEnd;
using Line = BackEnd.Line;
using Rectangle = BackEnd.Rectangle;
using System.Windows.Controls;

namespace CADP;

public partial class Canvas : System.Windows.Controls.Canvas {

   #region Constructor-----------------------------------------------
   public Canvas () {
      IsNewFile = true;
      IsModified = false;
      CurrentShapeColor = "#000000";
      CurrentShapeThickness = 1;
      mScaleValue = 1;
   }
   #endregion 

   #region Methods---------------------------------------------------
   public void ScribbleOn () {
      widget?.Detach ();
      mCurrentShape = new Scribble () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness };
      widget = new ScribbleWidget (this);
      widget.Attach ();
   }

   public void LineOn () {
      widget?.Detach ();
      mCurrentShape = new Line () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness };
      widget = new LineWidget (this);
      widget.Attach ();
   }

   public void RectOn () {
      widget?.Detach ();
      mCurrentShape = new Rectangle () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness };
      widget = new RectangleWidget (this);
      widget.Attach ();
   }

   public void CircleOn () {
      widget?.Detach ();
      mCurrentShape = new Circle () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness };
      widget = new CircleWidget (this);
      widget.Attach ();
   }

   public void Pick () => mCurrentShape = null;

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
      if (mFile.OpenFile (out List<Shape> f)) {
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

   private StackPanel AddStack () {
      StackPanel st = new () { Orientation = Orientation.Horizontal };
      if (mCurrentShape != null && mCurrentShape.Dimension != null) {
         foreach (var items in mCurrentShape.Dimension) {
            st.Children.Add (new TextBlock () { Text = items.Key.ToString (), TextAlignment = TextAlignment.Center, Margin = new Thickness (5) });
            st.Children.Add (new TextBox () { Width = 50, Margin = new Thickness (5), Text = Math.Round (items.Value, 3).ToString () });
         }
      } else {
         st.Children.Add (new TextBlock () { Text = "DX", TextAlignment = TextAlignment.Center, Margin = new Thickness (5) });
         st.Children.Add (new TextBox () { Width = 50, Margin = new Thickness (5) });
         st.Children.Add (new TextBlock () { Text = "DY", TextAlignment = TextAlignment.Center, Margin = new Thickness (5) });
         st.Children.Add (new TextBox () { Width = 50, Margin = new Thickness (5) });
      }
      return st;
   }

   public void AddNewShapeObject () {
      switch (mCurrentShape) {
         case Scribble:
            mCurrentShape = new Scribble () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness }; break;
         case Rectangle:
            mCurrentShape = new Rectangle () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness }; break;
         case Line:
            mCurrentShape = new Line () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness }; break;
         case Circle:
            mCurrentShape = new Circle () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness }; break;
      }
      IsDrawing = false;
      RenderMainWindowTools (true);
   }
   #endregion

   #region Overrides-------------------------------------------------
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
                  dc.DrawLine (sPen, UnitConverter.InverseTransform (scr.Points[i], this.Height, this.Width),
                     UnitConverter.InverseTransform (scr.Points[i + 1], this.Height, this.Width));
               }
               break;
            case Rectangle rectangle:
               Color rc = (Color)ColorConverter.ConvertFromString (rectangle.Color);
               Brush rb = new SolidColorBrush (rc);
               Pen rPen = new (rb, rectangle.Thickness);
               Rect r = new (UnitConverter.InverseTransform (rectangle.Points[0], this.Height, this.Width),
                  UnitConverter.InverseTransform (rectangle.Points[^1], this.Height, this.Width));
               dc.DrawRectangle (Background, rPen, r);
               break;
            case Line line:
               Color lc = (Color)ColorConverter.ConvertFromString (line.Color);
               Brush lb = new SolidColorBrush (lc);
               Pen lPen = new (lb, line.Thickness);
               dc.DrawLine (lPen, UnitConverter.InverseTransform (line.Points[0], this.Height, this.Width),
                 UnitConverter.InverseTransform (line.Points[^1], this.Height, this.Width));
               break;
            case Circle circle:
               Color cc = (Color)ColorConverter.ConvertFromString (circle.Color);
               Brush cb = new SolidColorBrush (cc);
               Pen cPen = new (cb, circle.Thickness);
               dc.DrawEllipse (Background, cPen, UnitConverter.InverseTransform (circle.Points[0], this.Height, this.Width), circle.Radius, circle.Radius);
               break;
         }
      }
   }

   protected override void OnMouseWheel (MouseWheelEventArgs e) {
      base.OnMouseWheel (e);
      Zoom (e.Delta > 0);
   }
   #endregion

   #region Properties------------------------------------------------
   public List<Shape> AllShapes { get => mShapes; set => mShapes = value; }

   public FileManager CanvasFileManager => mFile;

   public bool IsNewFile { get; set; }

   public bool IsModified { get; set; }

   public bool IsDrawing { get; set; }

   public string CurrentShapeColor { get; set; }

   public int CurrentShapeThickness { get; set; }

   public int UndoShapeCount => mUndoShapes.Count;

   public bool IsPressedUndo => mPressedUndo;

   public string CurrentShapePrompt {
      get {
         if (mCurrentShape != null)
            return mCurrentShape.prompt;
         return "Pick any shapes to draw";
      }
   }

   public StackPanel CurrentShapeStack => AddStack ();

   public Shape? CurrentShape => mCurrentShape ?? null;

   public Stack<Shape> UndoShapes => mUndoShapes;
   #endregion

   #region Private --------------------------------------------------
   private Shape? mCurrentShape;
   private List<Shape> mShapes = new ();
   private readonly Stack<Shape> mUndoShapes = new ();
   private readonly FileManager mFile = new ();
   private double mScaleValue;
   private bool mPressedUndo;
   private Widget widget;
   #endregion
}