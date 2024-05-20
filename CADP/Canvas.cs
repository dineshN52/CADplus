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
using Utils;
using Point = BackEnd.Point;

namespace CADP;

public partial class Canvas : System.Windows.Controls.Canvas {

   #region Constructor-----------------------------------------------
   public Canvas () {
      IsNewFile = true;
      IsModified = false;
      CurrentShapeColor = "#000000";
      CurrentShapeThickness = 1;
      MouseWheel += Zoom;
      MouseMove += ShowMousePt;
      Loaded += delegate {
         var bound = new Bound (new Point (-10, -10), new Point (1000, 500));
         mProjXfm = Transform.ComputeZoomExtentsProjXfm (ActualWidth, ActualHeight, bound);
         mInvProjXfm = mProjXfm; mInvProjXfm.Invert ();
         InvalidateVisual ();
      };
   }
   #endregion

   #region Methods---------------------------------------------------
   public void LineOn () {
      mWidget?.Detach (this);
      mCurrentShape = new Line () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness };
      mWidget = new LineWidget (this);
      mWidget.Attach (this);
   }

   public void RectOn () {
      mWidget?.Detach (this);
      mCurrentShape = new Rectangle () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness };
      mWidget = new RectangleWidget (this);
      mWidget.Attach (this);
   }

   public void CircleOn () {
      mWidget?.Detach (this);
      mCurrentShape = new Circle () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness };
      mWidget = new CircleWidget (this);
      mWidget.Attach (this);
   }

   public void Pick () {
      mWidget?.Detach (this);
      mCurrentShape = null;
   }

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

   public void Zoom (object sender, MouseWheelEventArgs e) {
      mCurrentMousePosition = e.GetPosition (this);
      if (e.Delta > 0) ZoomImplements (true);
      else ZoomImplements (false);
   }

   public void ZoomImplements (bool IsZoomIn) {
      double zoomFactor = 1.05;
      if (IsZoomIn) zoomFactor = 1 / zoomFactor;
      var ptDraw = mInvProjXfm.Transform (mCurrentMousePosition); // mouse point in drawing space
                                                                  // Actual visible drawing area
      System.Windows.Point cornerA = mInvProjXfm.Transform (new System.Windows.Point ()),
         cornerB = mInvProjXfm.Transform (new System.Windows.Point (ActualWidth, ActualHeight));
      var b = new Bound (new Point (cornerA.X, cornerA.Y), new Point (cornerB.X, cornerB.Y));
      b = b.Inflated (new Point (ptDraw.X, ptDraw.Y), zoomFactor);
      mProjXfm = Transform.ComputeZoomExtentsProjXfm (ActualWidth, ActualHeight, b);
      mInvProjXfm = mProjXfm; mInvProjXfm.Invert ();
      mWidget.InvProjXfm = mInvProjXfm;
      InvalidateVisual ();
   }

   public void RenderMainWindowTools (bool IsInputbar) {
      MainWindow ParentWindow = (MainWindow)Window.GetWindow (VisualParent);
      if (IsInputbar) ParentWindow.Inputbar.InvalidateVisual ();
      else ParentWindow.undoredo.InvalidateVisual ();
   }

   StackPanel AddStack () {
      StackPanel st = new () { Orientation = Orientation.Horizontal };
      if (mShapes.Count > 0 && mCurrentShape != null) {
         Dictionary<string, double> DimensionValues = mCurrentShape.Points.Count == 0 ? mShapes[^1].GetDimension () : mCurrentShape.GetDimension ();
         foreach (var items in DimensionValues) {
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
         case Line:
            mCurrentShape = new Line () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness }; break;
         case Rectangle:
            mCurrentShape = new Rectangle () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness }; break;
         case Circle:
            mCurrentShape = new Circle () { Color = CurrentShapeColor, Thickness = CurrentShapeThickness }; break;
      }
      IsDrawing = false;
      RenderMainWindowTools (true);
   }

   void ShowMousePt (object sender, MouseEventArgs e) {
      MainWindow ParentWindow = (MainWindow)Window.GetWindow (VisualParent);
      var ptCanvas = e.GetPosition (this);
      var ptDrawing = mInvProjXfm.Transform (ptCanvas);
      ParentWindow.MousePointpresenter.Text = $"Mouse: {ptCanvas.X:F2}, {ptCanvas.Y:F2} => {ptDrawing.X:F2}, {ptDrawing.Y:F2}";
   }
   #endregion

   #region Overrides-------------------------------------------------
   protected override void OnRender (DrawingContext dc) {
      base.OnRender (dc);
      RenderMainWindowTools (false);
      foreach (var shape in mShapes)
         shape.Draw (new DrawingCommands (dc, mProjXfm));
   }
   #endregion

   #region Properties------------------------------------------------
   public List<Shape> AllShapes { get => mShapes; set => mShapes = value; }

   public FileManager CanvasFileManager => mFile;

   public Shape? CurrentShape => mCurrentShape ?? null;

   public bool IsNewFile { get; set; }

   public bool IsModified { get; set; }

   public bool IsDrawing { get; set; }

   public string CurrentShapeColor { get; set; }

   public int CurrentShapeThickness { get; set; }

   public int UndoShapeCount => mUndoShapes.Count;

   public bool IsPressedUndo => mPressedUndo;

   public string CurrentShapePrompt => mCurrentShape != null ? mCurrentShape.prompt : "Pick any shapes to draw";

   public StackPanel CurrentShapeStack => AddStack ();

   public Stack<Shape> UndoShapes => mUndoShapes;

   public System.Windows.Point CurrentMousePosition { get => mCurrentMousePosition; set => mCurrentMousePosition = value; }

   public Matrix InvProjXfm => mInvProjXfm;
   #endregion

   #region Fields ---------------------------------------------------
   private Shape? mCurrentShape;
   private List<Shape> mShapes = new ();
   private readonly Stack<Shape> mUndoShapes = new ();
   private readonly FileManager mFile = new ();
   private bool mPressedUndo;
   private Widget mWidget;
   private Matrix mProjXfm = Matrix.Identity, mInvProjXfm = Matrix.Identity;
   private System.Windows.Point mCurrentMousePosition;
   #endregion
}