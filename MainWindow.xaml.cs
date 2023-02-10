using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Jump_Bruteforcer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Search s;
        public MainWindow()
        {
            InitializeComponent();

            s = new Search((127, 342.85055), (738, 247), new Dictionary<(int X, int Y), CollisionType>()); 
            DataContext = s;
        }

        private void ButtonSelectJMap_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog o = new()
            {
                DefaultExt = ".jmap"
            };

            bool? result = o.ShowDialog();

            if (result == true)
            {
                string FileName = o.FileName;
                LabelFileName.Content = FileName;

                string Text = File.ReadAllText(FileName);

                Map Map = JMap.Parse(Text);
                BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(Map.Bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); //https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
                ImageJMap.Source = source;
                s.CollisionMap = Map.CollisionMap;
                
            }


        }
        
        private void ButtonStartSearch_Click(object sender, RoutedEventArgs e)
        {
            s.RunAStar();

            if (s.StatesPerPx.Count == 0)
                return;
            
            Dictionary<(int X, int Y), (int Open, int Closed)>.Enumerator enumerator = s.StatesPerPx.GetEnumerator();

            string FileName = (string)LabelFileName.Content;
            string Text = File.ReadAllText(FileName);
            Map Map = JMap.Parse(Text);
            Bitmap Bmp = Map.Bmp;
            int MaxStatesPerPx = s.MaxStatesPerPx;

            do
            {
                (int X, int Y) = enumerator.Current.Key;
                (int Open, int Closed) = enumerator.Current.Value;

                int intensity = (int)Math.Round((Open + Closed) / (double)MaxStatesPerPx * 255); 

                System.Drawing.Color c = System.Drawing.Color.FromArgb(255, Open == 0 ? 0 : 255, Closed == 0 ? 0 : 255, intensity);

                Bmp.SetPixel(X, Y, c);
            }
            while (enumerator.MoveNext());

            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(Bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); //https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
            ImageJMap.Source = source;

        }
    }
}
