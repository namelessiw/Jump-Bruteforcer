using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            //s = new Search((401, 407.4f), (323, 343));
            //s = new Search((401, 407.4f), (419, 380));
            //s = new Search((408, 406.5), (459, 263));
            //s = new Search((410, 407.4), (476, 343));
            s = new Search((410, 407.4), (476, 343));
            DataContext = s;
        }

        private void ButtonSelectJMap_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog o = new()
            {
                DefaultExt = ".jmap"
            };

            Nullable<bool> result = o.ShowDialog();

            if (result == true)
            {
                string FileName = o.FileName;
                LabelFileName.Content = FileName;

                string Text = File.ReadAllText(FileName);

                Map Map = JMap.Parse(Text);
                BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(Map.Bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); //https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
                ImageJMap.Source = source;
                Clipboard.SetImage(source);
                s.CollisionMap = Map.CollisionMap;
                
            }


        }

        private void ButtonStartSearch_Click(object sender, RoutedEventArgs e)
        {
            s.RunAStar();
        }
    }
}
