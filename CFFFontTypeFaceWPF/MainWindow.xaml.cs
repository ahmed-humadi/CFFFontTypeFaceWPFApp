using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CFFFont;
using CFFFont.CustomFontViewer;
using System.IO;
namespace CFFFontTypeFaceWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CustPanel.LayoutTransform = new ScaleTransform(1, -1);
            
        }
        private void DrawGlyph()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    CustPanel.ClearVisuals();
                    byte[] fontData = new byte[stream.Length];
                    stream.Read(fontData);
                    using (CFFFontTypeFace cFFFontTypeFace = new CFFFontTypeFace(fontData))
                    {
                        //Geometry glyph = cFFFontTypeFace.GetGlyphOutLine(25);

                        //glyph.Transform = new ScaleTransform(500 / 1000.0, 500 / 1000.0);
                        //using (DrawingContext dc = CustPanel.RenderOpen())
                        //{
                        //    dc.PushTransform(new TranslateTransform(350, 100));
                        //    dc.DrawGeometry(Brushes.Black, new Pen(Brushes.Black, 2), glyph);
                        //    dc.Pop();
                        //}
                        double x = 0; double y = 0;//CustPanel.ActualHeight - 30;
                        for (ushort i = 0; i < cFFFontTypeFace.NumberOfGlyphs; i++)
                        {

                            Geometry glyph = cFFFontTypeFace.GetGlyphOutLine(i);

                            x = x + 20;

                            if (x > 650)
                            { y -= 30; x = 20; }
                            Matrix matrix = new Matrix(20.0 / 1000.0, 0, 0, 20.0 / 1000.0, x, y);

                            using (DrawingContext dc = CustPanel.RenderOpen())
                            {
                                dc.PushTransform(new MatrixTransform(matrix));
                                dc.DrawGeometry(Brushes.Black, null, glyph);
                                dc.Pop();
                            }
                        }
                    }
                }
            }
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            DrawGlyph();
        }
    }
}
