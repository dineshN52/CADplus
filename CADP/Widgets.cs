using System.Windows.Input;

namespace CADP;

public abstract class Widget {

   public Widget (Canvas canvas) {
      owner = canvas;
   }

   public void Attach () {
      owner.MouseDown += OnMouseDown;
      owner.MouseMove += OnMouseMove;
   }

   public void Detach () {
      owner.MouseDown -= OnMouseDown;
      owner.MouseMove -= OnMouseMove;
   }

   public void OnMouseDown (object sender, MouseButtonEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed && owner.CurrentShape != null) {
         if (owner.CurrentShape.Points.Count == 0) {
            owner.CurrentShape.Points.Add (UnitConverter.Transform (e.GetPosition (owner).X, e.GetPosition (owner).Y, owner.Height, owner.Width));
            owner.AllShapes.Add (owner.CurrentShape);
            if (owner.IsPressedUndo) owner.UndoShapes.Clear ();
            owner.IsDrawing = true;
            owner.RenderMainWindowTools (true);
         } else {
            owner.AddNewShapeObject ();
         }
      }
   }

   public void OnMouseMove (object sender, MouseEventArgs e) {
      if (e.LeftButton != MouseButtonState.Released || owner.CurrentShape == null || owner.CurrentShape.Points.Count < 1) return;
      owner.CurrentShape.Points.Add (UnitConverter.Transform (e.GetPosition (owner).X, e.GetPosition (owner).Y, owner.Height, owner.Width));
      owner.IsModified = true;
      owner.RenderMainWindowTools (true);
      owner.InvalidateVisual ();
   }


   Canvas owner;
}

public class RectangleWidget : Widget {
   public RectangleWidget (Canvas canvas) : base (canvas) { }
}

public class LineWidget : Widget {
   public LineWidget (Canvas canvas) : base (canvas) { }
}

public class CircleWidget : Widget {
   public CircleWidget (Canvas canvas) : base (canvas) { }
}

public class ScribbleWidget : Widget {
   public ScribbleWidget (Canvas canvas) : base (canvas) { }
}