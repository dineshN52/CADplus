using System.Windows.Input;
using System.Windows.Media;
using BackEnd;

namespace CADP;

public interface IWidget {

   void Attach (object sender);

   void Detach (object sender);

   void OnMouseDown (object sender, MouseButtonEventArgs e);

   void OnMouseMove (object sender, MouseEventArgs e);
}

public class Widget : IWidget {

   #region Constructor-----------------------------------------------
   public Widget (Canvas m) => owner = m;
   #endregion

   #region Interface implementation----------------------------------
   public void Attach (object sender) {
      mInvProjXfm = owner.InvProjXfm;
      owner.MouseDown += OnMouseDown;
      owner.MouseMove += OnMouseMove;
   }

   public void Detach (object sender) {
      owner.MouseDown -= OnMouseDown;
      owner.MouseMove -= OnMouseMove;
   }

   public virtual void OnMouseDown (object sender, MouseButtonEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed && owner.CurrentShape != null) {
         if (owner.CurrentShape.Points.Count == 0) {
            System.Windows.Point p = mInvProjXfm.Transform (e.GetPosition (owner));
            owner.CurrentShape.Points.Add (new Point (p.X, p.Y));
            owner.AllShapes.Add (owner.CurrentShape);
            if (owner.IsPressedUndo) owner.UndoShapes.Clear ();
            owner.IsDrawing = true;
            owner.RenderMainWindowTools (true);
         } else
            owner.AddNewShapeObject ();
      }
   }

   public virtual void OnMouseMove (object sender, MouseEventArgs e) {
      if (e.LeftButton != MouseButtonState.Released || owner.CurrentShape == null || owner.CurrentShape.Points.Count < 1) return;
      System.Windows.Point p = mInvProjXfm.Transform (e.GetPosition (owner));
      owner.CurrentShape.Points.Add (new Point (p.X, p.Y));
      owner.IsModified = true;
      owner.RenderMainWindowTools (true);
      owner.InvalidateVisual ();
   }
   #endregion

   #region Properties------------------------------------------------
   public Matrix InvProjXfm { get => mInvProjXfm; set => mInvProjXfm = value; }
   #endregion

   #region Fields----------------------------------------------------
   protected Canvas owner = new ();
   protected Matrix mInvProjXfm;
   #endregion
}

public class RectangleWidget : Widget {

   #region Constructor-----------------------------------------------
   public RectangleWidget (Canvas m) : base (m) { }
   #endregion
}

public class LineWidget : Widget {

   #region Constructor-----------------------------------------------
   public LineWidget (Canvas m) : base (m) { }
   #endregion
}

public class CircleWidget : Widget {

   #region Constructor-----------------------------------------------
   public CircleWidget (Canvas m) : base (m) { }
   #endregion
}

public class ConnectedLineWidget : Widget {

   #region Constructor-----------------------------------------------
   public ConnectedLineWidget (Canvas m) : base (m) => owner = m;
   #endregion

   #region Overrides-------------------------------------------------
   public override void OnMouseDown (object sender, MouseButtonEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed && owner.CurrentShape != null) {
         System.Windows.Point p = mInvProjXfm.Transform (e.GetPosition (owner));
         ((ConnectedLine)owner.CurrentShape).LinePoints.Add (p.X);
         ((ConnectedLine)owner.CurrentShape).LinePoints.Add (p.Y);
         if (((ConnectedLine)owner.CurrentShape).LinePoints.Count == 2) {
            owner.AllShapes.Add (owner.CurrentShape);
            owner.IsDrawing = true;
            owner.RenderMainWindowTools (true);
         }
         if (owner.IsPressedUndo) owner.UndoShapes.Clear ();
         owner.RenderMainWindowTools (true);
      }
   }
   public override void OnMouseMove (object sender, MouseEventArgs e) {
      if (e.LeftButton == MouseButtonState.Released) {
         if (owner.CurrentShape != null && ((ConnectedLine)owner.CurrentShape).LinePoints.Count > 1) {
            System.Windows.Point p = mInvProjXfm.Transform (e.GetPosition (owner));
            ((ConnectedLine)owner.CurrentShape).HoverPoint = new Point (p.X, p.Y);
            owner.IsModified = true;
         }
      } else if (e.LeftButton == MouseButtonState.Pressed && owner.CurrentShape != null) {
         System.Windows.Point p = mInvProjXfm.Transform (e.GetPosition (owner));
         ((ConnectedLine)owner.CurrentShape).LinePoints.Add (p.X);
         ((ConnectedLine)owner.CurrentShape).LinePoints.Add (p.Y);
         owner.CurrentShape.Points.Clear ();
      }
      owner.RenderMainWindowTools (true);
      owner.InvalidateVisual ();
   }
   #endregion
}