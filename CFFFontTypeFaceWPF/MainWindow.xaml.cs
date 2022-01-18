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
            DrawGlyph();
        }
        private void DrawGlyph()
        {
            using(FileStream stream = new FileStream(@"F:\EGEDFC+MyriadMM_565_600_.cff", FileMode.Open))
            {
                byte[] fontData = new byte[stream.Length];
                stream.Read(fontData);
                using (CFFFontTypeFace cFFFontTypeFace = new CFFFontTypeFace(fontData))

                {
                    Geometry glyph = cFFFontTypeFace.GetGlyphOutLine(2);

                    glyph.Transform = new ScaleTransform(40.0 / 1000.0, 40.0 / 1000.0);
                    //glyph.Transform = new TranslateTransform(50, 50);
                    using (DrawingContext dc = CustPanel.RenderOpen())
                    {
                        dc.DrawGeometry(Brushes.Black, new Pen(Brushes.Black, 2), glyph);
                    }
                }
            }
        }
    }
}
