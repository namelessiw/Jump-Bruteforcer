using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

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

            s = new Search((200, 200), (300, 300), new CollisionMap(new Dictionary<(int, int), ImmutableSortedSet<CollisionType>>(), null));
            DataContext = s;
        }
        private void BruteforceProjectButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = true,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string[] roomFolder = dialog.FileNames.ToArray();
                foreach (string room in roomFolder)
                {
                    LabelFileName.Content = Path.Join(Path.GetFileName(room), "instances.txt");
                    string roomDataFile = Path.Join(room, "instances.txt");
                    string Text = File.ReadAllText(roomDataFile);
                    string Extension = Path.GetExtension(roomDataFile);
                    Map Map;
                    try
                    {
                        Map = Parser.Parse(Extension, Text);

                        ImageJMap.Source = Map.Bmp;
                        s.CollisionMap = Map.CollisionMap;
                        ImageHeatMap.Source = new WriteableBitmap(Map.WIDTH, Map.HEIGHT, 96, 96, PixelFormats.Bgra32, null);
                        s.PlayerPath = new();
                        s.Strat = "";
                        if(Map.hasPlayerStart)
                        {
                            (s.StartX, s.StartY) = Map.PlayerStart;
                        }
                        else
                        {
                            continue;
                        }

                        if (Map.hasWarp)
                        {
                            (s.GoalX, s.GoalY) = Map.Warp;
                        }
                        else
                        {
                            continue;
                        }
                        SearchResult sr = s.RunAStar();
                        ImageHeatMap.Source = VisualizeSearch.HeatMap();
                        Macro = sr.Macro;

                        if (sr.Success)
                        {
                            string outputPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jump Bruteforcer macros");
                            Directory.CreateDirectory(outputPath);
                            File.WriteAllText(Path.Join(outputPath, $"{Path.GetFileName(room)}.txt"), sr.Macro);

                            CanvasWindow.UpdateLayout();
                            DrawingVisual drawingVisual = new DrawingVisual();
                            Rect renderBounds = new(CanvasWindow.RenderSize);
                            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                            {
                                drawingContext.DrawRectangle(new VisualBrush(CanvasWindow), null, renderBounds);
                            }
                            RenderTargetBitmap target = new RenderTargetBitmap((int)renderBounds.Width, (int)renderBounds.Height, 96, 96, PixelFormats.Pbgra32);
                            target.Render(drawingVisual);
                            FileStream stream = new FileStream(Path.Join(outputPath, $"{Path.GetFileName(room)}.png"), FileMode.Create);
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(target));
                            encoder.Save(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to import map with error:\n" + ex.Message);
                    }
                }
               

            }
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

                    ImageJMap.Source = Map.Bmp;
                    s.CollisionMap = Map.CollisionMap;
                    ImageHeatMap.Source = new WriteableBitmap(Map.WIDTH, Map.HEIGHT, 96, 96, PixelFormats.Bgra32, null);
                    s.PlayerPath = new();
                    s.Strat = "";
                    if (Map.hasPlayerStart)
                    {
                        (s.StartX, s.StartY) = Map.PlayerStart;
                    }

                    if (Map.hasWarp)
                    {
                        (s.GoalX, s.GoalY) = Map.Warp;
                    }

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
            Topmost = true;
            Topmost = false;
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

