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
        private string Macro = "";

        public MainWindow()
        {
            InitializeComponent();

            s = new Search((127, 342.85055), (738, 247), new CollisionMap(null, null));
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
                LabelFileName.Content = o.SafeFileName;

                string Text = File.ReadAllText(FileName);

                Map Map = JMap.Parse(Text);
                ImageJMap.Source = Map.Bmp;
                s.CollisionMap = Map.CollisionMap;

            }


        }

        private void ButtonStartSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchResult sr = s.RunAStar();
            ImageHeatMap.Source = VisualizeSearch.HeatMap();

            Macro = sr.Macro;
        }

        private void ButtonToggleHeatmap_Click(object sender, RoutedEventArgs e)
        {
            ImageHeatMap.Visibility = ImageHeatMap.Visibility is Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }


        private void ImageJMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (s.StartX, s.StartY) = ((int)e.GetPosition(ImageJMap).X, e.GetPosition(ImageJMap).Y);
        }

        private void ImageJMap_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            (s.GoalX, s.GoalY) = ((int)e.GetPosition(ImageJMap).X, (int)e.GetPosition(ImageJMap).Y);
        }

        private void CopyMacroButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(Macro);
        }
    }
}
