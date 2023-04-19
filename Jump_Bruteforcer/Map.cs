using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Jump_Bruteforcer
{
    public class Map
    {
        private readonly ImmutableArray<Object> Objects;
        public ImageSource Bmp { get; init; }
        public CollisionMap CollisionMap { get; init; }
        public const int WIDTH = 800, HEIGHT = 608;


        public Map(List<Object> objects)
        {
            List<Object> platforms = new(objects.FindAll(o => o.ObjectType == ObjectType.Platform));
            objects.Sort(Comparer<Object>.Create((o1, o2) => o1.CollisionType.CompareTo(o2.CollisionType)));
            Objects = ImmutableArray.CreateRange(objects);
            Bmp = GenerateCollisionImage();
            CollisionMap = new(GenerateCollisionMap(), platforms);
        }

        private ImageSource GenerateCollisionImage()
        {
            DrawingGroup drawingGroup = new DrawingGroup();
            
            int width = 800, height = 608;
            var mapBounds = new RectangleGeometry(new Rect(0, 0, width, height));
            drawingGroup.Children.Add(new GeometryDrawing(Brushes.Transparent, new Pen(), mapBounds));

            var query = from o in Objects
                        where o.CollisionType != CollisionType.None
                        select o;
            foreach (Object o in query)
            {
                BitmapSource img = toImage[o.ObjectType];
                Rect rect = new Rect(o.X - 5, o.Y - 8, img.Width, img.Height);
                drawingGroup.Children.Add(new ImageDrawing(img, rect));
                
            }
            drawingGroup.ClipGeometry = mapBounds;
            DrawingImage drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();

            return drawingImage;
        }
        
        private ImmutableSortedSet<CollisionType>[,] GenerateCollisionMap()
        {

            var query = (from o in Objects
                    where o.CollisionType != CollisionType.None
                    let hitbox = toHitbox[o.ObjectType]
                    from spriteX in Enumerable.Range(0, hitbox.GetLength(0))
                    from spriteY in Enumerable.Range(0, hitbox.GetLength(1))
                    where hitbox[spriteX, spriteY]
                    let x = o.X + spriteX - 5
                    let y = o.Y + spriteY - 8
                    where 0 <= x && x < WIDTH && 0 <= y && y < HEIGHT
                    group new { x, y, o } by (x, y) into pixel
                    select pixel);
            var collision = new ImmutableSortedSet<CollisionType>[WIDTH, HEIGHT];
            for (int i = 0; i < WIDTH; i++)
            {
                for (int j = 0; j < HEIGHT; j++)
                {
                    collision[i, j] = ImmutableSortedSet<CollisionType>.Empty;
                }
            }
            foreach (var pixel in query)
            {
                collision[pixel.Key.x, pixel.Key.y] = (from o in pixel select o.o.CollisionType)
                    .ToImmutableSortedSet(Comparer<CollisionType>.Create((a, b) => b.CompareTo(a)));
            }
            return collision;
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

        private static readonly Dictionary<ObjectType, BitmapSource> toImage;
        private static readonly Dictionary<ObjectType, bool[,]> toHitbox;
        static Map()
        {
            toImage = new Dictionary<ObjectType, BitmapSource>();
            toHitbox = new Dictionary<ObjectType, bool[,]>();
            foreach (string e in Enum.GetNames(typeof(ObjectType)))
            {
                ObjectType o = (ObjectType)Enum.Parse(typeof(ObjectType), e);
                BitmapSource img = GetImage(e.ToLower());

                toImage.Add(o, img);
                toHitbox.Add(o, GetHitbox(img));
            }
        }

        private static bool[,] GetHitbox(BitmapSource img)
        {
            var hitbox = new bool[img.PixelWidth, img.PixelHeight];

            int[] argbValues = new int[img.PixelHeight * img.PixelWidth];
            var stride = img.PixelWidth * img.Format.BitsPerPixel / 8;
            img.CopyPixels(argbValues, stride, 0);

            var query = from x in Enumerable.Range(0, img.PixelWidth)
                        from y in Enumerable.Range(0, img.PixelHeight)
                        where (argbValues[y * img.PixelWidth + x] & 0xff000000) != 0
                        select (x, y);
            foreach ((int x, int y) in query)
            {
                hitbox[x, y] = true;
            }

            return hitbox;
        }

        private static BitmapSource GetImage(string fileName)
        {
            try
            {

                Uri path = new Uri(@$"pack://application:,,,/Jump Bruteforcer;component/images/{fileName}.png", UriKind.Absolute);
                return new BitmapImage(path);
            }
            catch (IOException e)
            {

                return BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
            }

        }
    }
}
