
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace Jump_Bruteforcer
{
    internal class Map
    {
        private readonly List<Object> Objects;
        private Bitmap? image;

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

        private static readonly Dictionary<Color, CollisionType> toCollision = new()
        {
            {Color.FromArgb(128, 24, 24), CollisionType.Killer},
            {Color.FromArgb(0, 0, 0) , CollisionType.Solid},
            {Color.FromArgb(124, 252, 0) , CollisionType.Warp},
            {Color.FromArgb(0, 0, 139) , CollisionType.Water1},
            {Color.FromArgb(173, 216,230) , CollisionType.Water2},
            {Color.FromArgb(0, 0, 255) , CollisionType.Water3},
            {Color.FromArgb(112, 128, 144), CollisionType.Platform}
        };

        public Dictionary<(int X, int Y), CollisionType> GetCollisionMap()
        {

            Dictionary<(int X, int Y), CollisionType> CollisionMap = new();
            Bitmap bmp = GetCollisionImage();
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            IntPtr firstline = bmpData.Scan0;
            int[] argbValues = new int[Math.Abs(bmpData.Stride / 4 * bmp.Height)];
            Marshal.Copy(firstline, argbValues, 0, argbValues.Length);
            
            for(int i = 0; i < argbValues.Length; i++)
            {
                Color c = Color.FromArgb(argbValues[i]);
                if (toCollision.ContainsKey(c))
                {
                    CollisionMap.Add((i % bmp.Width, i / bmp.Width), toCollision[c]);
                }
                
            }
            bmp.UnlockBits(bmpData);

            return CollisionMap;
        }

       

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

        public Bitmap GetCollisionImage()
        {
            image ??= GenerateCollisionImage();
            return image;
        }

        private Bitmap GenerateCollisionImage()
        {
            int width = 800, height = 608;
            Bitmap bmp = new(width, height);
            Objects.Sort(Comparer<Object>.Create((Object o1, Object o2) => o1.CollisionType.CompareTo(o2.CollisionType)));

            using (Graphics g = Graphics.FromImage(bmp))
            {
                
                foreach (Object o in Objects.FindAll(o=>o.CollisionType != CollisionType.None))
                {
                    Image img = toImage[o.ObjectType];
                    g.DrawImage(img, o.X - 5, o.Y - 8, img.Width, img.Height);
                }
            }
            
            return bmp;
        }
    }
}
