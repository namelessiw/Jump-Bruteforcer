
using System.Collections.Generic;
using System.Text;

namespace Jump_Bruteforcer
{
    internal class Map
    {
        private readonly SortedSet<Object> Objects;

        public Map()
        {
            Objects = new(Comparer<Object>.Create((Object o1, Object o2) => o1.CollisionType.CompareTo(o2.CollisionType)));
        }


        public new string ToString()
        {
            StringBuilder sb = new("");

            sb.Append($"\nObjects ({Objects.Count}):");

            foreach (Object o in Objects)
            {
                sb.AppendLine($"{o.ObjectType}:\n\tx: {o.X}\n\ty: {o.Y}\n");
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

            using (Graphics g = Graphics.FromImage(bmp))
            {
                foreach(Object o in Objects)
                {
                    if (o.CollisionType != CollisionType.None)
                    {
                        
                        //g.DrawImage(toImage[o], new Point(o.X, o.Y));

                    }
                    
                }
            }

            return bmp;
        }


    }
}
