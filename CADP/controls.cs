using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Media;
using Point = BackEnd.Point;
namespace CADP;

public static class UnitConverter {
   public static Point Transform (double X, double Y, double Height, double Width) => new (X - Width / 2, Height / 2 - Y);

   public static System.Windows.Point InverseTransform (Point P, double Height, double Width) => new (P.X + Width / 2, Height / 2 - P.Y);
}

public class Toolsbar : RibbonQuickAccessToolBar {

   public Toolsbar () {
      DefaultStyleKey = typeof (RibbonQuickAccessToolBar);
   }

   protected override void OnRender (DrawingContext drawingContext) {
      base.OnRender (drawingContext);
      MainWindow ParentWindow = (MainWindow)Window.GetWindow (VisualParent);
      ParentWindow.undo.IsEnabled = ParentWindow.Canvas.IsModified && ParentWindow.Canvas.AllShapes.Count > 0;
      ParentWindow.redo.IsEnabled = ParentWindow.Canvas.UndoShapeCount > 0 && ParentWindow.Canvas.IsModified;
   }
}
public class cStackPanel : StackPanel {

   public cStackPanel () {
      DefaultStyleKey = typeof (StackPanel);
   }

   protected override void OnRender (DrawingContext dc) {
      base.OnRender (dc);
      MainWindow ParentWindow = (MainWindow)Window.GetWindow (VisualParent);
      Binding promptBind = new () {
         Source = ParentWindow.Canvas.CurrentShapePrompt
      };
      ((TextBlock)Children[0]).SetBinding (TextBlock.TextProperty, promptBind);
      if (ParentWindow.Canvas.IsDrawing) {
         ((Grid)Children[1]).Children.Clear ();
         ((Grid)Children[1]).Children.Add (ParentWindow.Canvas.CurrentShapeStack);
      }
   }
}