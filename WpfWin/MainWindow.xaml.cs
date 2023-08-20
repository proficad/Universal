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
using DxfNet;
using SkiaSharp.Views.Desktop;
using System.IO;
using Microsoft.Win32;

namespace WpfWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CollPages m_doc;
        private System.String m_path;
        public MainWindow()
        {
            InitializeComponent();

            
            m_path = @"C:\Users\Public\Documents\ProfiCAD Samples\indoors\floor plan.sxe";
            FileStream l_input_stream = File.Open(m_path, FileMode.Open);
            m_doc = Loader.Loader.Load(l_input_stream);
            
         
        }

        private void OnPainting(object sender, SKPaintSurfaceEventArgs e)
        {
            //SharedPage.OnPainting(sender, e);

            SkiaSharp.SKCanvas l_canvas = e.Surface.Canvas;
            l_canvas.Clear();
            l_canvas.Scale(0.3f);

            if (m_doc != null)
            {
                Drawer.Drawer.DrawPCadDoc(l_canvas, e.Info.Rect, m_doc.GetCurrentPage());
            }


        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                m_path = openFileDialog.FileName;

                Refresh_Drawing();
            }

        }
        private void MenuItem_Refresh(object sender, RoutedEventArgs e)
        {
            Refresh_Drawing();
        }


        private void Refresh_Drawing()
        {
            FileStream l_input_stream = File.Open(m_path, FileMode.Open);
            m_doc = Loader.Loader.Load(l_input_stream);

            /*
            haha.Width = m_doc.Get_Size_Current_Page().Width;
            haha.Height = m_doc.Get_Size_Current_Page().Height;
            
   
            haha.InvalidateVisual();
            */
            
        }


        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            int i = 66;
            i++;
        }
    }
}
