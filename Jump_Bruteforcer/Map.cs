using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Jump_Bruteforcer
{
    public class Map
    {
        private readonly ImmutableArray<Object> Objects;
        public Bitmap Bmp { get; init; }
        public CollisionMap CollisionMap { get; init; }


        public Map(List<Object> objects)
        {
            List<Object> platforms = new(objects.FindAll(o => o.ObjectType == ObjectType.Platform));
            objects.Sort(Comparer<Object>.Create((o1, o2) => o1.CollisionType.CompareTo(o2.CollisionType)));
            Objects = ImmutableArray.CreateRange(objects);
            Bmp = GenerateCollisionImage();
            CollisionMap = new(GenerateCollisionMap(), platforms);
        }

        private Bitmap GenerateCollisionImage()
        {
            int width = 800, height = 608;
            Bitmap bmp = new(width, height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                var query = from o in Objects
                            where o.CollisionType != CollisionType.None
                            select o;
                foreach (Object o in query)
                {
                    Image img = toImage[o.ObjectType];
                    g.DrawImage(img, o.X - 5, o.Y - 8, img.Width, img.Height);
                }
            }

            return bmp;
        }

        private Dictionary<(int X, int Y), CollisionType> GenerateCollisionMap()
        {
            Dictionary<(int X, int Y), CollisionType> CollisionMap = new();
            var query = from o in Objects
                        where o.CollisionType != CollisionType.None
                        select o;
            foreach (Object o in query)
            {
                Bitmap img = (Bitmap) toImage[o.ObjectType];
                (int objX, int objY) = (o.X - 5, o.Y - 8);
                for (int x = 0; x < img.Width; x++)
                {
                    for (int y = 0; y < img.Height; y++)
                    {
                        if (img.GetPixel(x, y).A != 0)
                        {
                            CollisionMap[(objX + x, objY + y)] = o.CollisionType;
                        }
                        
                    }
                }
                
            }
            return CollisionMap;
        }

        public override string ToString()
        {
            StringBuilder sb = new("");

            sb.Append($"\nObjects ({Objects.Length}):");

            foreach (Object o in Objects)
            {
                sb.AppendLine(o.ToString());
            }

            return sb.ToString();
        }

        private static readonly Dictionary<ObjectType, Image> toImage;
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
            try
            {
                Uri path = new Uri(@$"pack://application:,,,/Jump Bruteforcer;component/images/{fileName}.png", UriKind.Absolute);
                return new Bitmap(Application.GetResourceStream(path).Stream);
            }
            catch (IOException e)
            {
                return new Bitmap(1, 1);
            }

        }
    }
}
