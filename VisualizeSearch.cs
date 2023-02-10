using Priority_Queue;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jump_Bruteforcer
{
    public static class VisualizeSearch
    {
        private static Dictionary<(int X, int Y), (int Open, int Closed)> statesPerPx = new();
        private static int maxStatesPerPx = 0;
        public static int MaxStatesPerPx { get { return maxStatesPerPx; } set { maxStatesPerPx = value; } }
        public static Dictionary<(int X, int Y), (int Open, int Closed)> StatesPerPx { get { return statesPerPx; } set { statesPerPx = value;} }
        public static void CountStates(SimplePriorityQueue<PlayerNode, float> openSet, HashSet<PlayerNode> closedSet)
        {
            statesPerPx.Clear();
            foreach (PlayerNode p in openSet)
            {
                int X = p.State.X;
                int Y = p.State.RoundedY;

                if (!StatesPerPx.ContainsKey((X, Y)))
                {
                    StatesPerPx.Add((X, Y), (1, 0));
                }
                else
                {
                    StatesPerPx[(X, Y)] = (StatesPerPx[(X, Y)].Open + 1, StatesPerPx[(X, Y)].Closed);
                }

                MaxStatesPerPx = Math.Max(MaxStatesPerPx, StatesPerPx[(X, Y)].Open + StatesPerPx[(X, Y)].Closed);
            }

            foreach (PlayerNode p in closedSet)
            {
                int X = p.State.X;
                int Y = (int)Math.Round(p.State.Y);

                if (!StatesPerPx.ContainsKey((X, Y)))
                {
                    StatesPerPx.Add((X, Y), (0, 1));
                }
                else
                {
                    StatesPerPx[(X, Y)] = (StatesPerPx[(X, Y)].Open, StatesPerPx[(X, Y)].Closed + 1);
                }

                MaxStatesPerPx = Math.Max(MaxStatesPerPx, StatesPerPx[(X, Y)].Open + StatesPerPx[(X, Y)].Closed);
            }
        }

        public static WriteableBitmap HeatMap()
        {
            WriteableBitmap bmp = new(800, 608, 96, 96, PixelFormats.Bgra32, null);


            Dictionary<(int X, int Y), (int Open, int Closed)>.Enumerator enumerator = VisualizeSearch.StatesPerPx.GetEnumerator();


            int MaxStatesPerPx = VisualizeSearch.MaxStatesPerPx;

            while (enumerator.MoveNext())
            {
                (int x, int y) = enumerator.Current.Key;
                (int Open, int Closed) = enumerator.Current.Value;

                int max = Math.Max(Open, Closed);
                int sum = Open + Closed;
                double brightness = 255 - (double)sum / MaxStatesPerPx * 255;
                byte r = (byte)((double)Open / max * brightness);
                byte g = (byte)(brightness / 4);
                byte b = (byte)((double)Closed / max * brightness);

                byte[] C = { b, g, r, 255 };
                bmp.WritePixels(new Int32Rect(x, y, 1, 1), C, 4, 0);
            }

            return bmp;
        }
    }
}