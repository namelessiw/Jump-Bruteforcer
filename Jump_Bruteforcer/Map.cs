using System.Collections.Generic;
using System.Collections.Immutable;
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
            List<Object> platforms = [];
            foreach (Object obj in objects) {
                if (obj.ObjectType == ObjectType.PlayerStart) {
                    PlayerStart = (obj.X + 17, obj.Y + 23);
                    hasPlayerStart = true;
                }
                if (obj.ObjectType == ObjectType.Warp)
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
            Objects = [.. objects];
            (var cmap, var vineDistance) = GenerateCollisionMap();
            CollisionMap = new(cmap, platforms, vineDistance);
            Bmp = GenerateCollisionImage();
        }

        static readonly List<(CollisionType, Color)> CollisionTypeColors = new()
        {
            {(CollisionType.Solid, Color.FromRgb(0, 0, 0))},
            {(CollisionType.Killer, Color.FromRgb(255, 0, 0))},
            {(CollisionType.Warp, Color.FromRgb(0, 255, 0))},
            {(CollisionType.Platform, Color.FromRgb(81, 43, 0))},
            {(CollisionType.Water1, Color.FromRgb(0, 0, 255))},
            {(CollisionType.Water2, Color.FromRgb(0, 127, 255))},
            {(CollisionType.Water3, Color.FromRgb(0, 255, 255))},
            {(CollisionType.CatharsisWater, Color.FromRgb(195, 195, 195))},
            {(CollisionType.GravityArrowUp, Color.FromRgb(255, 63, 63))},
            {(CollisionType.GravityArrowDown, Color.FromRgb(63, 63, 255))},
        };
        private BitmapImage GenerateCollisionImage()
        {
            // using a bitmap for now just because idk how id do this otherwise
            System.Drawing.Bitmap bitmap = new(WIDTH, HEIGHT);

            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    CollisionType type = CollisionMap.Collision[x, y];

                    int colors = 0;
                    int r = 0, g = 0, b = 0;

                    // add colors for each collision type present
                    foreach ((CollisionType t, Color c) in CollisionTypeColors)
                    {
                        if (type.HasFlag(t))
                        {
                            colors++;
                            r += c.R;
                            g += c.G;
                            b += c.B;
                        }
                    }

                    // need to draw vines separately as they are handled separately
                    if (CollisionMap.GetVineDistance(x, y, ObjectType.VineLeft, true) == VineDistance.INSIDE)
                    {
                        colors++;
                        r += 0;
                        g += 195;
                        b += 0;
                    }

                    if (CollisionMap.GetVineDistance(x, y, ObjectType.VineRight, true) == VineDistance.INSIDE)
                    {
                        colors++;
                        r += 0;
                        g += 195;
                        b += 0;
                    }

                    // average over all colors or use white if no collision on this pixel
                    if (colors == 0)
                    {
                        r = g = b = 255;
                    }
                    else
                    {
                        r /= colors;
                        g /= colors;
                        b /= colors;
                    }

                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(255, r, g, b));
                }
            }

            // need to convert to a BitmapImage instead and idk so
            // taken from https://stackoverflow.com/questions/26260654/wpf-converting-bitmap-to-imagesource
            MemoryStream ms = new();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private (CollisionType[,], VineDistance[,,]) GenerateCollisionMap()
        {

            var query = from o in Objects
                         where o.CollisionType != CollisionType.None
                         let hitbox = toHitbox[o.ObjectType]
                         from coords in hitbox
                         let x = o.X + coords.x
                         let y = o.Y + coords.y
                         where 0 <= x && x < WIDTH && 0 <= y && y < HEIGHT
                         group new { x, y, o } by (x, y) into pixel
                         select pixel;
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
            StringBuilder sb = new();

            sb.Append($"\nObjects ({Objects.Length}):");

            foreach (Object o in Objects)
            {
                sb.AppendLine(o.ToString());
            }

            return sb.ToString();
        }

        private static readonly Dictionary<ObjectType, HashSet<(int x, int y)>> toHitbox;
        static Map()
        {
            HashSet<(int x, int y)> PlayerHitboxDilationOffsets = GetPlayerHitbox(new System.Drawing.Bitmap($@"../../../hitboxes/player.png"));

            toHitbox = new Dictionary<ObjectType, HashSet<(int x, int y)>>();
            foreach (string e in Enum.GetNames(typeof(ObjectType)))
            {
                string path = $@"../../../hitboxes/{e.ToLower()}.png";

                if (!File.Exists(path))
                {
                    continue;
                }

                ObjectType o = (ObjectType)Enum.Parse(typeof(ObjectType), e);

                System.Drawing.Bitmap img = new System.Drawing.Bitmap(path);
                toHitbox.Add(o, GetDilatedHitbox(PlayerHitboxDilationOffsets, img));
            }
        }

        private static HashSet<(int x, int y)> GetPlayerHitbox(System.Drawing.Bitmap img)
        {
            HashSet<(int x, int y)> hitbox = [];

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    if (img.GetPixel(x, y).A == 0)
                    {
                        continue;
                    }

                    (int x, int y) dilationOffset = (PLAYER_ORIGIN_X - x, PLAYER_ORIGIN_Y - y);
                    hitbox.Add(dilationOffset);
                }
            }

            return hitbox;
        }

        const int PLAYER_ORIGIN_X = 17, PLAYER_ORIGIN_Y = 23; // TODO: make this and other object origins a setting
        private static HashSet<(int x, int y)> GetDilatedHitbox(HashSet<(int x, int y)> playerHitbox, System.Drawing.Bitmap otherHitbox)
        {
            HashSet<(int, int)> DilatedObjectHitbox = [];

            for (int x = 0; x < otherHitbox.Width; x++)
            {
                for (int y = 0; y < otherHitbox.Height; y++)
                {
                    if (otherHitbox.GetPixel(x, y).A == 0)
                    {
                        continue;
                    }

                    foreach ((int xOffset, int yOffset) in playerHitbox)
                    {
                        DilatedObjectHitbox.Add((x + xOffset, y + yOffset));
                    }
                }
            }

            return DilatedObjectHitbox;
        }
    }
}
