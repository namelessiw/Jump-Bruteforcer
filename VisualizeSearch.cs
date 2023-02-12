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
        public static void CountStates(SimplePriorityQueue<PlayerNode, Priority> openSet, HashSet<PlayerNode> closedSet)
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

        public static Color HsvToRgb(double h, double s, double v)
        {
            int hi = (int)Math.Floor(h / 60.0) % 6;
            double f = (h / 60.0) - Math.Floor(h / 60.0);

            double p = v * (1.0 - s);
            double q = v * (1.0 - (f * s));
            double t = v * (1.0 - ((1.0 - f) * s));

            Color ret;

            static Color GetRgb(double r, double g, double b)
            {
                return Color.FromArgb(255, (byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0));
            }

            switch (hi)
            {
                case 0:
                    ret = GetRgb(v, t, p);
                    break;
                case 1:
                    ret = GetRgb(q, v, p);
                    break;
                case 2:
                    ret = GetRgb(p, v, t);
                    break;
                case 3:
                    ret = GetRgb(p, q, v);
                    break;
                case 4:
                    ret = GetRgb(t, p, v);
                    break;
                case 5:
                    ret = GetRgb(v, p, q);
                    break;
                default:
                    ret = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
                    break;
            }
            return ret;
        }

        /*public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }*/

        public static WriteableBitmap HeatMap()
        {
            WriteableBitmap bmp = new(800, 608, 96, 96, PixelFormats.Bgra32, null);


            Dictionary<(int X, int Y), (int Open, int Closed)>.Enumerator enumerator = VisualizeSearch.StatesPerPx.GetEnumerator();


            string[] Plasma = new string[] { "#0d0887", "#100788", "#130789", "#16078a", "#19068c", "#1b068d", "#1d068e", "#20068f", "#220690", "#240691", "#260591", "#280592", "#2a0593", "#2c0594", "#2e0595", "#2f0596", "#310597", "#330597", "#350498", "#370499", "#38049a", "#3a049a", "#3c049b", "#3e049c", "#3f049c", "#41049d", "#43039e", "#44039e", "#46039f", "#48039f", "#4903a0", "#4b03a1", "#4c02a1", "#4e02a2", "#5002a2", "#5102a3", "#5302a3", "#5502a4", "#5601a4", "#5801a4", "#5901a5", "#5b01a5", "#5c01a6", "#5e01a6", "#6001a6", "#6100a7", "#6300a7", "#6400a7", "#6600a7", "#6700a8", "#6900a8", "#6a00a8", "#6c00a8", "#6e00a8", "#6f00a8", "#7100a8", "#7201a8", "#7401a8", "#7501a8", "#7701a8", "#7801a8", "#7a02a8", "#7b02a8", "#7d03a8", "#7e03a8", "#8004a8", "#8104a7", "#8305a7", "#8405a7", "#8606a6", "#8707a6", "#8808a6", "#8a09a5", "#8b0aa5", "#8d0ba5", "#8e0ca4", "#8f0da4", "#910ea3", "#920fa3", "#9410a2", "#9511a1", "#9613a1", "#9814a0", "#99159f", "#9a169f", "#9c179e", "#9d189d", "#9e199d", "#a01a9c", "#a11b9b", "#a21d9a", "#a31e9a", "#a51f99", "#a62098", "#a72197", "#a82296", "#aa2395", "#ab2494", "#ac2694", "#ad2793", "#ae2892", "#b02991", "#b12a90", "#b22b8f", "#b32c8e", "#b42e8d", "#b52f8c", "#b6308b", "#b7318a", "#b83289", "#ba3388", "#bb3488", "#bc3587", "#bd3786", "#be3885", "#bf3984", "#c03a83", "#c13b82", "#c23c81", "#c33d80", "#c43e7f", "#c5407e", "#c6417d", "#c7427c", "#c8437b", "#c9447a", "#ca457a", "#cb4679", "#cc4778", "#cc4977", "#cd4a76", "#ce4b75", "#cf4c74", "#d04d73", "#d14e72", "#d24f71", "#d35171", "#d45270", "#d5536f", "#d5546e", "#d6556d", "#d7566c", "#d8576b", "#d9586a", "#da5a6a", "#da5b69", "#db5c68", "#dc5d67", "#dd5e66", "#de5f65", "#de6164", "#df6263", "#e06363", "#e16462", "#e26561", "#e26660", "#e3685f", "#e4695e", "#e56a5d", "#e56b5d", "#e66c5c", "#e76e5b", "#e76f5a", "#e87059", "#e97158", "#e97257", "#ea7457", "#eb7556", "#eb7655", "#ec7754", "#ed7953", "#ed7a52", "#ee7b51", "#ef7c51", "#ef7e50", "#f07f4f", "#f0804e", "#f1814d", "#f1834c", "#f2844b", "#f3854b", "#f3874a", "#f48849", "#f48948", "#f58b47", "#f58c46", "#f68d45", "#f68f44", "#f79044", "#f79143", "#f79342", "#f89441", "#f89540", "#f9973f", "#f9983e", "#f99a3e", "#fa9b3d", "#fa9c3c", "#fa9e3b", "#fb9f3a", "#fba139", "#fba238", "#fca338", "#fca537", "#fca636", "#fca835", "#fca934", "#fdab33", "#fdac33", "#fdae32", "#fdaf31", "#fdb130", "#fdb22f", "#fdb42f", "#fdb52e", "#feb72d", "#feb82c", "#feba2c", "#febb2b", "#febd2a", "#febe2a", "#fec029", "#fdc229", "#fdc328", "#fdc527", "#fdc627", "#fdc827", "#fdca26", "#fdcb26", "#fccd25", "#fcce25", "#fcd025", "#fcd225", "#fbd324", "#fbd524", "#fbd724", "#fad824", "#fada24", "#f9dc24", "#f9dd25", "#f8df25", "#f8e125", "#f7e225", "#f7e425", "#f6e626", "#f6e826", "#f5e926", "#f5eb27", "#f4ed27", "#f3ee27", "#f3f027", "#f2f227", "#f1f426", "#f1f525", "#f0f724", "#f0f921" };

            int MaxStatesPerPx = VisualizeSearch.MaxStatesPerPx;

            while (enumerator.MoveNext())
            {
                (int x, int y) = enumerator.Current.Key;
                (int Open, int Closed) = enumerator.Current.Value;

                int max = Math.Max(Open, Closed);
                int sum = Open + Closed;
                //double brightness = 255 - (double)sum / MaxStatesPerPx * 255;

                /*double h = (double)sum / MaxStatesPerPx * 330 + 15,
                    s = 0.5 + (double)Closed / max / 2,
                    v = 1;*/

                /*byte r = (byte)((double)Open / max * brightness);
                byte g = (byte)(brightness / 8 * 2);
                byte b = (byte)((double)Closed / max * brightness);*/

                int i = (int)((double)sum / MaxStatesPerPx * 255);
                Color c = (Color)ColorConverter.ConvertFromString(Plasma[i]);

                double Brightness = (double)Closed / max;
                c = Color.FromArgb(255, (byte)(c.R * Brightness), (byte)(c.G * Brightness), (byte)(c.B * Brightness));

                //Color c = HsvToRgb(h, s, v);

                byte[] C = { c.B, c.G, c.R, 255 };
                bmp.WritePixels(new Int32Rect(x, y, 1, 1), C, 4, 0);
            }

            return bmp;
        }
    }
}