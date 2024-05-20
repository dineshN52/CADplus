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

   public Widget (Canvas m) => owner = m;

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

   public Matrix InvProjXfm { get => mInvProjXfm; set => mInvProjXfm = value; }

   Canvas owner = new ();
   Matrix mInvProjXfm;
}

public class RectangleWidget : Widget {
   public RectangleWidget (Canvas m) : base (m) {
   }
}

public class LineWidget : Widget {
   public LineWidget (Canvas m) : base (m) {
   }
}

public class CircleWidget : Widget {
   public CircleWidget (Canvas m) : base (m) {
   }
}

public class ConnectedLineWidget : Widget {
   public ConnectedLineWidget (Canvas m) : base (m) {
   }
}