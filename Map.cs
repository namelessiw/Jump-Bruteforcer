
using System.Collections.Generic;
using System.Text;

namespace Jump_Bruteforcer
{
    internal class Map
    {
        private readonly List<Object> Objects;

        public Map()
        {
            Objects = new();
        }


        public new string ToString()
        {
            StringBuilder sb = new("");

            sb.Append($"\nObjects ({Objects.Count}):");

            foreach (Object o in Objects)
            {
                sb.AppendLine(o.ToString());
            }

            return sb.ToString();
        }


        public void AddObject(Object Object)
        {
            Objects.Add(Object);
        }



        public Dictionary<(int X, int Y), CollisionType> GetCollisionMap()
        {

            Dictionary<(int X, int Y), CollisionType> CollisionMap = new();


            return CollisionMap;
        }

        private static readonly Dictionary<Color, CollisionType> cmap = new()
        {
            {Color.FromArgb(128, 24, 24), CollisionType.Killer},
            {Color.Black , CollisionType.Solid},
            {Color.LawnGreen , CollisionType.Warp},
            {Color.DarkBlue , CollisionType.Water1},
            {Color.LightBlue , CollisionType.Water2},
            {Color.Blue , CollisionType.Water3},
            {Color.SlateGray, CollisionType.Platform}
        };

        private static Dictionary<ObjectType, Image> toImage;
        static Map()
        {
            toImage = new Dictionary<ObjectType, Image>();
            foreach (string e in Enum.GetNames(typeof(ObjectType)))
            {
                toImage.Add((ObjectType)Enum.Parse(typeof(ObjectType), e), GetImage(e.ToLower()));
            }
        }

        private static Image GetImage(string fileName)
        {
            string path = @$"..\..\..\images\{fileName}.png";
            return File.Exists(path) ? Image.FromFile(path) : new Bitmap(1,1);
            
        }

        public Bitmap GenerateImage()
        {
            int width = 800, height = 608;
            Bitmap bmp = new(width, height);
            
            //bmp.SetResolution()
            Objects.Sort(Comparer<Object>.Create((Object o1, Object o2) => o1.CollisionType.CompareTo(o2.CollisionType)));

            using (Graphics g = Graphics.FromImage(bmp))
            {
                
                foreach (Object o in Objects)
                {
                    if (o.CollisionType != CollisionType.None)
                    {
                        Image img = toImage[o.ObjectType];
                        g.DrawImage(img, o.X - 5, o.Y - 8, img.Width, img.Height);

                    }
                    
                }
            }
            
            return bmp;
        }


    }
}
