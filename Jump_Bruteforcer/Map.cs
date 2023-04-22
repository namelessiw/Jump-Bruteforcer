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
    public enum VineDistance
    {
        FAR,
        CORNER,
        EDGE,
        INSIDE
    }
    public class Map
    {
        private readonly ImmutableArray<Object> Objects;
        public ImageSource Bmp { get; init; }
        public CollisionMap CollisionMap { get; init; }
        public const int WIDTH = 801, HEIGHT = 609;


        public Map(List<Object> objects)
        {
            List<Object> platforms = new(objects.FindAll(o => o.ObjectType == ObjectType.Platform));
            objects.Sort(Comparer<Object>.Create((o1, o2) => o1.CollisionType.CompareTo(o2.CollisionType)));
            Objects = ImmutableArray.CreateRange(objects);
            Bmp = GenerateCollisionImage();
            (var cmap, var leftvine, var rightvine) = GenerateCollisionMap();
            CollisionMap = new(cmap, platforms, leftvine, rightvine);
        }

        private ImageSource GenerateCollisionImage()
        {
            DrawingGroup drawingGroup = new DrawingGroup();
            
            var mapBounds = new RectangleGeometry(new Rect(0, 0, WIDTH, HEIGHT));
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
        
        private (ImmutableSortedSet<CollisionType>[,], VineDistance[,], VineDistance[,]) GenerateCollisionMap()
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

            //vine distance matrices
            var vineLeftDistances = new VineDistance[WIDTH, HEIGHT];
            var vineRightDistances = new VineDistance[WIDTH, HEIGHT];
            var query2 = from o in Objects
                         where o.ObjectType == ObjectType.VineLeft || o.ObjectType == ObjectType.VineRight
                         select o;
            foreach(Object vine in query2)
            {
                if (vine.ObjectType == ObjectType.VineRight)
                {
                    vineDistanceHelper(vineRightDistances, vine, 0);
                }
                else
                {
                    vineDistanceHelper(vineLeftDistances, vine, 18);
                }
            }

            return (collision, vineLeftDistances, vineRightDistances);

            static void vineDistanceArrayHelper(VineDistance[,] vineDistances, int x, int y, VineDistance distance)
            {
                if ((uint)x < WIDTH && (uint)y < HEIGHT)
                    vineDistances[x, y] = (VineDistance)Math.Max((byte)distance, (byte)vineDistances[x, y]);
            }

            static void vineDistanceHelper(VineDistance[,] vineDistance, Object vine, int offset)
            {
                vineDistanceArrayHelper(vineDistance, vine.X - 6 + offset, vine.Y - 11, VineDistance.CORNER);
                vineDistanceArrayHelper(vineDistance, vine.X - 6 + offset, vine.Y + 42, VineDistance.CORNER);
                vineDistanceArrayHelper(vineDistance, vine.X + 19 + offset, vine.Y - 11, VineDistance.CORNER);
                vineDistanceArrayHelper(vineDistance, vine.X + 19 + offset, vine.Y + 42, VineDistance.CORNER);
                foreach (int x in Enumerable.Range(-5 + offset, 24))
                {
                    vineDistanceArrayHelper(vineDistance, vine.X + x, vine.Y - 11, VineDistance.EDGE);
                    vineDistanceArrayHelper(vineDistance, vine.X + x, vine.Y + 42, VineDistance.EDGE);
                }
                foreach (int y in Enumerable.Range(-10, 52))
                {
                    vineDistanceArrayHelper(vineDistance, vine.X - 6 + offset, vine.Y + y, VineDistance.EDGE);
                    vineDistanceArrayHelper(vineDistance, vine.X + 19 + offset, vine.Y + y, VineDistance.EDGE);
                }
                var query3 = from x in Enumerable.Range(-5 + offset, 24)
                             from y in Enumerable.Range(-10, 52)
                             select (x, y);
                foreach ((int x, int y) in query3)
                {
                    vineDistanceArrayHelper(vineDistance, vine.X + x, vine.Y + y, VineDistance.INSIDE);
                }
            }
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
