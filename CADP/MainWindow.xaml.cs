using System;
using System.Windows;
using System.Windows.Controls.Ribbon;

namespace CADP {

   /// <summary>Interaction logic for MainWindow.xaml</summary>
   public partial class MainWindow : RibbonWindow {

      #region Constructor-------------
      public MainWindow () {
         InitializeComponent ();
         Closing += MainWindow_Closing;

      }
      #endregion

      #region Properties----------
      public Canvas Canvas => paintCanvas;
      #endregion

      #region Methods------------------
      private void Newfile_Click (object sender, RoutedEventArgs e) {
         MainWindow w = new ();
         w.Show ();
      }

      private void OpenFile_Click (object sender, RoutedEventArgs e) {
         try {
            paintCanvas.Open ();
         } catch (Exception ex) {
            MessageBox.Show (ex.Message);
         }
      }

      private void Save_Click (object sender, RoutedEventArgs e) => paintCanvas.Save ();

      private void TextFile_Click (object sender, RoutedEventArgs e) => paintCanvas.SaveAs (true);

      private void BinFile_Click (object sender, RoutedEventArgs e) => paintCanvas.SaveAs (false);

      private void Exit_Click (object sender, RoutedEventArgs e) => Close ();

      private void Undo_Click (object sender, RoutedEventArgs e) => paintCanvas.Undo ();

      private void Redo_Click (object sender, RoutedEventArgs e) => paintCanvas.Redo ();

      private void Scribble_Click (object sender, RoutedEventArgs e) { paintCanvas.ScribbleOn (); Inputbar.InvalidateVisual (); }

      private void Rectangle_Click (object sender, RoutedEventArgs e) { paintCanvas.RectOn (); Inputbar.InvalidateVisual (); }

      private void Line_Click (object sender, RoutedEventArgs e) { paintCanvas.LineOn (); Inputbar.InvalidateVisual (); }

      private void Circle_Click (object sender, RoutedEventArgs e) { paintCanvas.CircleOn (); Inputbar.InvalidateVisual (); }

      private void MainWindow_Closing (object? sender, System.ComponentModel.CancelEventArgs e) {
         SaveMessagePop mPop = new (e, paintCanvas.CanvasFileManager, paintCanvas.IsNewFile) {
            Owner = this,
         };
         if (paintCanvas.IsModified) mPop.ShowDialog ();
         else SystemCommands.CloseWindow (this);
      }

      private void ThicknessVal_TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e) {
         if (paintCanvas != null)
            paintCanvas.CurrentShapeThickness = (int)double.Parse (ThicknessVal.Text);
      }

      private void Pick_Click (object sender, RoutedEventArgs e) => paintCanvas.Pick ();

      private void Red_Click (object sender, RoutedEventArgs e) => paintCanvas.CurrentShapeColor = "#FF0000";

      private void Green_Click (object sender, RoutedEventArgs e) => paintCanvas.CurrentShapeColor = "#00FF00";

      private void Blue_Click (object sender, RoutedEventArgs e) => paintCanvas.CurrentShapeColor = "#0000FF";

      private void Black_Click (object sender, RoutedEventArgs e) => paintCanvas.CurrentShapeColor = "#000000";

      private void ZoomIn_Click (object sender, RoutedEventArgs e) => paintCanvas.Zoom (true);

      private void ZoomOut_Click (object sender, RoutedEventArgs e) => paintCanvas.Zoom (false);
      #endregion
   }
}