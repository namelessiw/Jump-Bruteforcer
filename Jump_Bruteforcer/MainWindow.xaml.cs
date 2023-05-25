using Microsoft.Win32;
using System.Collections.Immutable;
using System.IO;
using System.Windows;
using System.Windows.Input;

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

            s = new Search((127, 342.85055), (738, 247), new CollisionMap(new Dictionary<(int, int), ImmutableSortedSet<CollisionType>>(), null));
            DataContext = s;
        }

        private void ButtonSelectJMap_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog o = new()
            {
                DefaultExt = ".jmap",
                Filter = "map files|*.jmap;*.cmap;*.txt",
            };

            bool? result = o.ShowDialog();

            if (result == true)
            {
                string FileName = o.FileName;
                LabelFileName.Content = o.SafeFileName;

                string Text = File.ReadAllText(FileName);
                string Extension = System.IO.Path.GetExtension(FileName);
                Map Map;
                try
                {
                    Map = Parser.Parse(Extension, Text);


                    s.PlayerPath = new();
                    ImageJMap.Source = Map.Bmp;
                    s.CollisionMap = Map.CollisionMap;
                }
                catch (UnknownExtensionException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to import map with error:\n" + ex.Message);
                }

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
