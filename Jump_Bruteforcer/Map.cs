﻿using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jump_Bruteforcer
{
    public enum VineDistance
    {
        FAR,
        CORNER,
        EDGE,
        INSIDE
    }
    public enum VineArrayIdx
    {
        VINELEFTFACINGRIGHT,
        VINELEFTFACINGLEFT,
        VINERIGHTFACINGRIGHT,
        VINERIGHTFACINGLEFT,
    }
    public class Map
    {
        private readonly ImmutableArray<Object> Objects;
        public ImageSource Bmp { get; init; }
        public CollisionMap CollisionMap { get; init; }
        public const int WIDTH = 801, HEIGHT = 609;
        public bool hasPlayerStart { get; init; } = false;
        public (int, int) PlayerStart { get; init; }
        public bool hasWarp { get; init; } = false;
        public (int, int) Warp { get; init; }
        


        public Map(List<Object> objects)
        {
            List<Object> platforms = new();
            foreach (Object obj in objects) {
                if (obj.ObjectType == ObjectType.PlayerStart) {
                    PlayerStart = (obj.X + 17, obj.Y + 23);
                    hasPlayerStart = true;
                }
                if(obj.ObjectType == ObjectType.Warp)
                {
                    Warp = (obj.X + 16, obj.Y + 16);
                    hasWarp = true;
                }
                if (obj.ObjectType == ObjectType.Platform)
                {
                    platforms.Add(obj);
                }
            }
            objects.Sort(Comparer<Object>.Create((o1, o2) => o1.CollisionType.CompareTo(o2.CollisionType)));
            Objects = ImmutableArray.CreateRange(objects);
            Bmp = GenerateCollisionImage();
            (var cmap, var vineDistance) = GenerateCollisionMap();
            CollisionMap = new(cmap, platforms, vineDistance);
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

        private (CollisionType[,], VineDistance[,,]) GenerateCollisionMap()
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
            var collision = new CollisionType[WIDTH, HEIGHT];
            for (int i = 0; i < WIDTH; i++)
            {
                for (int j = 0; j < HEIGHT; j++)
                {
                    collision[i, j] = CollisionType.None;
                }
            }
            var comparer = Comparer<CollisionType>.Create((a, b) => b.CompareTo(a));
            foreach (var pixel in query)
            {
                collision[pixel.Key.x, pixel.Key.y] = (from o in pixel select o.o.CollisionType).Aggregate((a, b) => a | b);
            }

            //vine distance matrix
            var vineDistances = new VineDistance[WIDTH, HEIGHT, Enum.GetNames(typeof(VineArrayIdx)).Length];
            var query2 = from o in Objects
                         where o.ObjectType == ObjectType.VineLeft || o.ObjectType == ObjectType.VineRight
                         select o;
            foreach (Object vine in query2)
            {
                if (vine.ObjectType == ObjectType.VineRight)
                {
                    vineDistanceHelper(vineDistances, VineArrayIdx.VINERIGHTFACINGRIGHT, vine, 0, 0);
                    vineDistanceHelper(vineDistances, VineArrayIdx.VINERIGHTFACINGLEFT, vine, 0, 2);
                }
                else
                {
                    vineDistanceHelper(vineDistances, VineArrayIdx.VINELEFTFACINGRIGHT, vine, 18, 0);
                    vineDistanceHelper(vineDistances, VineArrayIdx.VINELEFTFACINGLEFT, vine, 18, 2);
                }
            }

            return (collision, vineDistances);

            static void vineDistanceArrayHelper(VineDistance[,,] vineDistances, VineArrayIdx idx, int x, int y, VineDistance distance)
            {
                if ((uint)x < WIDTH && (uint)y < HEIGHT)
                    vineDistances[x, y, (int)idx] = (VineDistance)Math.Max((byte)distance, (byte)vineDistances[x, y, (int)idx]);
            }

            static void vineDistanceHelper(VineDistance[,,] vineDistance, VineArrayIdx idx, Object vine, int offset, int bbox_extension)
            {
                vineDistanceArrayHelper(vineDistance, idx, vine.X - 6 + offset, vine.Y - 9, VineDistance.CORNER);
                vineDistanceArrayHelper(vineDistance, idx, vine.X - 6 + offset, vine.Y + 44, VineDistance.CORNER);
                vineDistanceArrayHelper(vineDistance, idx, vine.X + 19 + offset + bbox_extension, vine.Y - 9, VineDistance.CORNER);
                vineDistanceArrayHelper(vineDistance, idx, vine.X + 19 + offset + bbox_extension, vine.Y + 44, VineDistance.CORNER);
                foreach (int x in Enumerable.Range(-5 + offset, 24 + bbox_extension))
                {
                    vineDistanceArrayHelper(vineDistance, idx, vine.X + x, vine.Y - 9, VineDistance.EDGE);
                    vineDistanceArrayHelper(vineDistance, idx, vine.X + x, vine.Y + 44, VineDistance.EDGE);
                }
                foreach (int y in Enumerable.Range(-8, 52))
                {
                    vineDistanceArrayHelper(vineDistance, idx, vine.X - 6 + offset, vine.Y + y, VineDistance.EDGE);
                    vineDistanceArrayHelper(vineDistance, idx, vine.X + 19 + offset + bbox_extension, vine.Y + y, VineDistance.EDGE);
                }
                var query3 = from x in Enumerable.Range(-5 + offset, 24 + bbox_extension)
                             from y in Enumerable.Range(-8, 52)
                             select (x, y);
                foreach ((int x, int y) in query3)
                {
                    vineDistanceArrayHelper(vineDistance, idx, vine.X + x, vine.Y + y, VineDistance.INSIDE);
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
