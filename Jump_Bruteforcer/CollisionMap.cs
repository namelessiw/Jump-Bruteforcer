using System.Collections.Immutable;
using System.Numerics;

namespace Jump_Bruteforcer
{
    internal readonly record struct VineDistances(
        VineDistance LeftFacingRight,
        VineDistance LeftFacingLeft,
        VineDistance RightFacingRight,
        VineDistance RightFacingLeft)
    {
        public VineDistance Left(bool facingRight) => facingRight ? LeftFacingRight : LeftFacingLeft;
        public VineDistance Right(bool facingRight) => facingRight ? RightFacingRight : RightFacingLeft;
    }

    public class CollisionMap
    {
        private const int VineVariantCount = 4;
        private const int PixelCount = Map.WIDTH * Map.HEIGHT;

        public CollisionType[,] Collision { get; init; }
        public List<Object> Platforms { get; init; }

        private readonly CollisionType[] collisionCells;
        private readonly VineDistance[] vineDistanceCells;
        public readonly HashSet<(int x, int y)> goalPixels;

        public CollisionMap(CollisionType[,]? Collision, List<Object>? Platforms, VineDistance[,,] vineDistances)
        {
            this.Collision = Collision ?? new CollisionType[Map.WIDTH, Map.HEIGHT];
            this.Platforms = Platforms ?? new List<Object>();
            collisionCells = new CollisionType[PixelCount];
            vineDistanceCells = new VineDistance[PixelCount * VineVariantCount];
            this.goalPixels = new();
            for (int y = 0; y < Map.HEIGHT; y++)
            {
                for (int x = 0; x < Map.WIDTH; x++)
                {
                    int pixelIndex = PixelIndex(x, y);
                    CollisionType collision = this.Collision[x, y];
                    collisionCells[pixelIndex] = collision;
                    if ((collision & CollisionType.Warp) != CollisionType.None)
                    {
                        goalPixels.Add((x, y));
                    }

                    int vineIndex = pixelIndex * VineVariantCount;
                    for (int variant = 0; variant < VineVariantCount; variant++)
                    {
                        vineDistanceCells[vineIndex + variant] = vineDistances[x, y, variant];
                    }

                }
            }

        }
        public bool onWarp(int x, double y)
        {
            int yRounded = (int)Math.Round(y);
            return (uint)x < Map.WIDTH && (uint)yRounded < Map.HEIGHT &&
                (collisionCells[PixelIndex(x, yRounded)] & CollisionType.Warp) != CollisionType.None;
        }
        public VineDistance GetVineDistance(int x, double y, ObjectType vine, bool facingRight)
        {
            int yRounded = (int)Math.Round(y);
            if (!((uint)x < Map.WIDTH & (uint)yRounded < Map.HEIGHT))
            {
                return VineDistance.FAR;
            }
            int variant = vine == ObjectType.VineRight
                ? facingRight ? (int)VineArrayIdx.VINERIGHTFACINGRIGHT : (int)VineArrayIdx.VINERIGHTFACINGLEFT
                : facingRight ? (int)VineArrayIdx.VINELEFTFACINGRIGHT : (int)VineArrayIdx.VINELEFTFACINGLEFT;
            return vineDistanceCells[PixelIndex(x, yRounded) * VineVariantCount + variant];
        }

        internal VineDistances GetVineDistances(int x, double y)
        {
            int yRounded = (int)Math.Round(y);
            if ((uint)x >= Map.WIDTH || (uint)yRounded >= Map.HEIGHT)
            {
                return default;
            }

            int index = PixelIndex(x, yRounded) * VineVariantCount;
            return new VineDistances(
                vineDistanceCells[index + (int)VineArrayIdx.VINELEFTFACINGRIGHT],
                vineDistanceCells[index + (int)VineArrayIdx.VINELEFTFACINGLEFT],
                vineDistanceCells[index + (int)VineArrayIdx.VINERIGHTFACINGRIGHT],
                vineDistanceCells[index + (int)VineArrayIdx.VINERIGHTFACINGLEFT]);
        }
        public CollisionMap(Dictionary<(int, int), CollisionType>? Collision, List<Object>? Platforms)
        {
            this.Collision = new CollisionType[Map.WIDTH, Map.HEIGHT];
            collisionCells = new CollisionType[PixelCount];
            if (Collision != null)
            {
                foreach (var kvp in Collision)
                {
                    (int x, int y) = kvp.Key;
                    this.Collision[x, y] = kvp.Value;
                    collisionCells[PixelIndex(x, y)] = kvp.Value;
                }
            }
            vineDistanceCells = new VineDistance[PixelCount * VineVariantCount];
            this.Platforms = Platforms ?? new List<Object>();
            this.goalPixels = new();
            if (Collision != null)
            {
                foreach (var kvp in Collision)
                {
                    if ((kvp.Value & CollisionType.Warp) != CollisionType.None)
                    {
                        goalPixels.Add(kvp.Key);
                    }
                }
            }
        }
        public static int UnsetAllBitsExceptMSB(int x)
        {
            x |= x >> 16;
            x |= x >> 8;
            x |= x >> 4;
            x |= x >> 2;
            x |= x >> 1;
            x ^= x >> 1;
            return x;
        }

        public CollisionType GetHighestPriorityCollisionType(int x, double y, bool invertedGrav)
        {
            return (CollisionType)UnsetAllBitsExceptMSB((int)GetCollisionTypes(x, y, invertedGrav));
        }

        /// <summary>
        /// returns the set of CollisionTypes at pixel (x, y) in order of descending priority
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public CollisionType GetCollisionTypes(int x, double y, bool invertedGrav)
        {
            int yRounded = (int)Math.Round(y + (invertedGrav ? 3 : 0));
            return (uint)x < Map.WIDTH && (uint)yRounded < Map.HEIGHT
                ? collisionCells[PixelIndex(x, yRounded)]
                : CollisionType.None;
        }

        private static int PixelIndex(int x, int y) => y * Map.WIDTH + x;

        /// <summary>
        /// gets the lowest instance number platform at coordinate (x, y) with an instance number greater than or equal to minInstanceNum
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="minInstanceNum"></param>
        /// <returns></returns>
        public Object? GetCollidingPlatform(int x, int y, int minInstanceNum)
        {
            return (from Object platform in Platforms
                    where platform.instanceNum >= minInstanceNum & platform.bbox.Contains(x, y)
                    select platform).MinBy(x => x.instanceNum);


        }
        public Object? GetCollidingPlatform(int x, double y, int minInstanceNum)
        {
            return (from Object platform in Platforms
                    where platform.instanceNum >= minInstanceNum & platform.bbox.Contains(x, (int)Math.Round(y))
                    select platform).MinBy(x => x.instanceNum);
        }
    }
}
