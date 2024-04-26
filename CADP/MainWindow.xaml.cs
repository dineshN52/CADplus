using System;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CADP {

   #region MainWindow------------------------------------------------------------------------------
   /// <summary>Interaction logic for MainWindow.xaml</summary>
   public partial class MainWindow : RibbonWindow {

      #region Constructor--------------------------------------------
      public MainWindow () {
         InitializeComponent ();
         Closing += MainWindow_Closing;
      }
      #endregion

      #region Properties---------------------------------------------
      public Canvas Canvas => paintCanvas;
      #endregion

      #region Methods------------------------------------------------
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

      private void ThicknessVal_TextChanged (object sender, TextChangedEventArgs e) {
         if (paintCanvas != null && paintCanvas.CurrentShape != null) {
            paintCanvas.CurrentShapeThickness = ThicknessVal.Text != "" ? (int)double.Parse (ThicknessVal.Text) : 1;
            paintCanvas.CurrentShape.Thickness = paintCanvas.CurrentShapeThickness;
         }
      }

      private void Pick_Click (object sender, RoutedEventArgs e) => paintCanvas.Pick ();

      private void ZoomIn_Click (object sender, RoutedEventArgs e) => paintCanvas.Zoom (true);

      private void ZoomOut_Click (object sender, RoutedEventArgs e) => paintCanvas.Zoom (false);

      private void ClrPicker_SelectedColorChanged (object sender, RoutedPropertyChangedEventArgs<Color?> e) {
         Color? col = ((ColorPicker)sender).SelectedColor;
         if (paintCanvas.CurrentShape != null) {
            paintCanvas.CurrentShape.Color = col != null ? ColorToHexString (col.Value) : "#000000";
            paintCanvas.CurrentShapeColor = paintCanvas.CurrentShape.Color;
         }
      }

      public static string ColorToHexString (Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

      private void ThicknessVal_PreviewTextInput (object sender, System.Windows.Input.TextCompositionEventArgs e) {
         var combinedText = ((TextBox)sender).Text + e.Text;
         if (!Regex.IsMatch (combinedText, "^[123]$")) {
            e.Handled = true;
         }
      }
      #endregion
   }
   #endregion
}