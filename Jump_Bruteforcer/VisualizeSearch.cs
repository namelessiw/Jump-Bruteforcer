﻿using Priority_Queue;
using System.ComponentModel;

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace Jump_Bruteforcer
{
    public class CoordinatePointConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //Keeps the VS Designer UI from crashing
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                for (int i = 0; i < values.Length; i++)
                {

                    values[i] = 0;

                }

            (int x, int y) = (System.Convert.ToInt32(values[0]), (int)Math.Round(System.Convert.ToDouble(values[1])));
            return new Point(x, y);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class VisualizeSearch
    {
        public static WriteableBitmap heuristicMap = new (Map.WIDTH, Map.HEIGHT, 96, 96, PixelFormats.Bgra32, null);
        public static WriteableBitmap stateMap = new(Map.WIDTH, Map.HEIGHT, 96, 96, PixelFormats.Bgra32, null);
        private static int maxStatesPerPx = 0;
        private static string[] cmap = new string[] { "#000000", "#000000", "#000000", "#000001", "#010101", "#010102", "#020102", "#020203", "#030204", "#030305", "#040306", "#050407", "#060509", "#07050A", "#08060C", "#09070D", "#0A080F", "#0B0911", "#0C0A12", "#0D0B14", "#0F0C16", "#100D17", "#110D19", "#120E1B", "#130F1C", "#14101E", "#15111F", "#161221", "#171323", "#181425", "#191526", "#1A1528", "#1B162A", "#1C172B", "#1D182D", "#1E192F", "#1F1A31", "#201A33", "#211B34", "#221C36", "#231D38", "#241E3A", "#251E3C", "#261F3E", "#27203F", "#282141", "#292143", "#292245", "#2A2347", "#2B2449", "#2C244B", "#2D254D", "#2E264F", "#2F2751", "#302753", "#302855", "#312957", "#32295A", "#332A5C", "#342B5E", "#352C60", "#352C62", "#362D64", "#372E67", "#382E69", "#382F6B", "#39306D", "#3A3070", "#3B3172", "#3B3274", "#3C3277", "#3D3379", "#3D347C", "#3E347E", "#3F3580", "#3F3683", "#403785", "#413788", "#41388A", "#42398D", "#42398F", "#433A92", "#433B95", "#443B97", "#443C9A", "#453D9C", "#453E9F", "#453EA2", "#463FA4", "#4640A7", "#4641AA", "#4641AC", "#4642AF", "#4743B1", "#4744B4", "#4745B7", "#4746B9", "#4647BC", "#4648BE", "#4649C0", "#464AC3", "#454BC5", "#454CC7", "#444DCA", "#444ECC", "#434FCE", "#4251D0", "#4152D1", "#4153D3", "#4055D5", "#3F56D6", "#3D58D7", "#3C59D9", "#3B5BDA", "#3A5CDB", "#385EDC", "#375FDC", "#3561DD", "#3463DE", "#3364DE", "#3166DF", "#3067DF", "#2E69DF", "#2C6BE0", "#2B6CE0", "#296EE0", "#286FE0", "#2771E0", "#2572E0", "#2474E0", "#2275E0", "#2177E0", "#2078E0", "#1F7AE0", "#1E7BE0", "#1D7DDF", "#1C7EDF", "#1B80DF", "#1A81DF", "#1A82DF", "#1984DE", "#1985DE", "#1987DE", "#1988DE", "#1A89DD", "#1A8BDD", "#1B8CDD", "#1B8DDD", "#1C8FDC", "#1D90DC", "#1E91DC", "#1F93DC", "#2194DB", "#2295DB", "#2396DB", "#2598DB", "#2699DA", "#289ADA", "#2A9BDA", "#2B9DDA", "#2D9ED9", "#2F9FD9", "#31A0D9", "#32A1D9", "#34A3D9", "#36A4D8", "#38A5D8", "#3AA6D8", "#3CA7D8", "#3EA8D8", "#40AAD8", "#42ABD7", "#44ACD7", "#46ADD7", "#48AED7", "#4AAFD7", "#4CB0D7", "#4EB2D7", "#50B3D7", "#52B4D6", "#54B5D6", "#56B6D6", "#58B7D6", "#5BB8D6", "#5DB9D6", "#5FBAD6", "#61BBD6", "#63BCD6", "#65BDD6", "#68BED6", "#6AC0D6", "#6CC1D6", "#6FC2D6", "#71C3D6", "#73C4D6", "#75C5D6", "#78C6D6", "#7AC7D6", "#7DC8D6", "#7FC9D6", "#81CAD7", "#84CBD7", "#86CBD7", "#89CCD7", "#8BCDD7", "#8DCED8", "#90CFD8", "#92D0D8", "#95D1D8", "#97D2D9", "#9AD3D9", "#9CD4DA", "#9FD5DA", "#A1D6DA", "#A3D7DB", "#A6D8DB", "#A8D8DC", "#ABD9DC", "#ADDADD", "#AFDBDE", "#B2DCDE", "#B4DDDF", "#B7DEDF", "#B9DFE0", "#BBE0E1", "#BEE1E2", "#C0E2E2", "#C2E3E3", "#C5E3E4", "#C7E4E5", "#C9E5E6", "#CBE6E6", "#CEE7E7", "#D0E8E8", "#D2E9E9", "#D4EAEA", "#D7EBEB", "#D9ECEC", "#DBEDED", "#DDEEEE", "#DFEFEF", "#E2F0F0", "#E4F1F1", "#E6F2F2", "#E8F3F3", "#EAF4F4", "#ECF5F5", "#EEF6F6", "#F1F7F7", "#F3F8F8", "#F5F9F9", "#F7FBFA", "#F9FCFC", "#FBFDFD", "#FDFEFE", "#FFFFFF" };

        public static void CountStates(SimplePriorityQueue<PlayerNode, (uint,uint)> openSet, int[,] closedSet)
        {
            int[,] openStates = new int[Map.WIDTH, Map.HEIGHT];
            foreach (PlayerNode node in openSet)
            {
                openStates[node.State.X, node.State.RoundedY] += 1;
            }

            StateMap(closedSet, openStates);
        }



        public static void StateMap(int[,] closedStates, int[,] openStates)
        {
            stateMap = new(Map.WIDTH, Map.HEIGHT, 96, 96, PixelFormats.Bgra32, null);

            int maxStatesPerPx = (from x in Enumerable.Range(0, Map.WIDTH)
                                  from y in Enumerable.Range(0, Map.HEIGHT)
                                  select openStates[x, y] + closedStates[x, y]).Max();
            for (int x = 0; x < Map.WIDTH; x++) {
                for (int y = 0; y < Map.HEIGHT; y++)
                {

                    int sum = openStates[x,y] + closedStates[x,y];
                    if (sum == 0)
                    {
                        continue;
                    }
                    int i = (int)((double)sum / maxStatesPerPx * 255);
                    Color c = (Color)ColorConverter.ConvertFromString(cmap[i]);
                    double Brightness = (double)closedStates[x, y] / Math.Max(openStates[x, y], closedStates[x, y]);
                    byte[] C = { (byte)(c.B * Brightness), (byte)(c.G * Brightness), (byte)(c.R * Brightness), 255 };
                    stateMap.WritePixels(new Int32Rect(x, y, 1, 1), C, 4, 0);
                }
            }
        }
        public static void HeuristicMap(uint[,] heuristic)
        {
            heuristicMap = new(Map.WIDTH, Map.HEIGHT, 96, 96, PixelFormats.Bgra32, null);
            uint maxHeuristic = heuristic.Cast<uint>().MaxBy(x => x == uint.MaxValue? 0 : x);
            for (int x = 0; x < Map.WIDTH; x++)
            {
                for(int y = 0; y < Map.HEIGHT; y++)
                {
                    int i = heuristic[x, y] == uint.MaxValue ? 255: (int)((double)heuristic[x, y] / maxHeuristic * 255);
                    Color c = (Color)ColorConverter.ConvertFromString(cmap[i]);
                    byte transparency = (byte)(heuristic[x, y] == uint.MaxValue ? 0 : 255);
                    byte[] C = { (byte)(c.B), (byte)(c.G), (byte)(c.R), transparency };
                    heuristicMap.WritePixels(new Int32Rect(x, y, 1, 1), C, 4, 0);
                }
            }

            
        }
    }
}
