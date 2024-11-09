using System.Collections.Immutable;
using System.Numerics;

namespace Jump_Bruteforcer
{
    public class CollisionMap
    {
        public CollisionType[,] Collision { get; init; }
        public List<Object> Platforms { get; init; }

        private readonly VineDistance[,,] vineDistance;
        public readonly HashSet<(int x, int y)> goalPixels;

        public CollisionMap(CollisionType[,]? Collision, List<Object>? Platforms, VineDistance[,,] vineDistances)
        {
            this.Collision = Collision ?? new CollisionType[Map.WIDTH, Map.HEIGHT];
            this.Platforms = Platforms ?? new List<Object>();
            this.vineDistance = vineDistances;
            this.goalPixels = new();
            for (int x = 0; x < Map.WIDTH; x++)
            {
                for (int y = 0; y < Map.HEIGHT; y++)
                {
                    if (this.Collision[x, y].HasFlag(CollisionType.Warp))
                    {
                        goalPixels.Add((x, y));
                    }

                }
            }

        }
        public bool onWarp(int x, double y) => goalPixels.Contains((x, (int)Math.Round(y)));
        public VineDistance GetVineDistance(int x, double y, ObjectType vine, bool facingRight)
        {
            int yRounded = (int)Math.Round(y);
            if (!((uint)x < Map.WIDTH & (uint)yRounded < Map.HEIGHT))
            {
                return VineDistance.FAR;
            }
            if (vine == ObjectType.VineRight)
            {
                if (facingRight)
                    return vineDistance[x, yRounded, (int)VineArrayIdx.VINERIGHTFACINGRIGHT];
                else
                    return vineDistance[x, yRounded, (int)VineArrayIdx.VINERIGHTFACINGLEFT];
            }
            else
            {
                if (facingRight)
                    return vineDistance[x, yRounded, (int)VineArrayIdx.VINELEFTFACINGRIGHT];
                else
                    return vineDistance[x, yRounded, (int)VineArrayIdx.VINELEFTFACINGLEFT];
            }
        }
        public CollisionMap(Dictionary<(int, int), CollisionType>? Collision, List<Object>? Platforms)
        {
            this.Collision = new CollisionType[Map.WIDTH, Map.HEIGHT];
            for (int i = 0; i < Map.WIDTH; i++)
            {
                for (int j = 0; j < Map.HEIGHT; j++)
                {
                    this.Collision[i, j] = CollisionType.None;
                }
            }
            if (Collision != null)
            {
                foreach (var kvp in Collision)
                {
                    (int x, int y) = kvp.Key;
                    this.Collision[x, y] = kvp.Value;
                }
            }
            this.vineDistance = new VineDistance[Map.WIDTH, Map.HEIGHT, Enum.GetNames(typeof(VineArrayIdx)).Length];
            this.Platforms = Platforms ?? new List<Object>();
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

        public CollisionType GetHighestPriorityCollisionType(int x, int y, bool invertedGrav)
        {
            if (invertedGrav)
            {
                return (uint)x < Map.WIDTH & (uint)y + 3 < Map.HEIGHT ? (CollisionType)UnsetAllBitsExceptMSB( (int)Collision[x, y + 3]) : CollisionType.None;
            }
            return (uint)x < Map.WIDTH & (uint)y < Map.HEIGHT ? (CollisionType)UnsetAllBitsExceptMSB((int)Collision[x, y]) : CollisionType.None;
        }


        public CollisionType GetHighestPriorityCollisionType(int x, double y, bool invertedGrav)
        {
            if (invertedGrav)
            {
                return (uint)x < Map.WIDTH & (uint)Math.Round(y + 3) < Map.HEIGHT ? (CollisionType)UnsetAllBitsExceptMSB((int)Collision[x, (int)Math.Round(y + 3)]) : CollisionType.None;
            }
            return (uint)x < Map.WIDTH & (uint)Math.Round(y) < Map.HEIGHT ? (CollisionType)UnsetAllBitsExceptMSB((int)Collision[x, (int)Math.Round(y)]) : CollisionType.None;
        }

        /// <summary>
        /// returns the set of CollisionTypes at pixel (x, y) in order of descending priority
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public CollisionType GetCollisionTypes(int x, int y, bool invertedGrav)
        {
            if (invertedGrav)
            {
                return (uint)x < Map.WIDTH & (uint)y + 3 < Map.HEIGHT ? Collision[x, y + 3] : CollisionType.None;
            }
            return (uint)x < Map.WIDTH & (uint)y < Map.HEIGHT ? Collision[x, y] : CollisionType.None;

        }
        public CollisionType GetCollisionTypes(int x, double y, bool invertedGrav)
        {
            if (invertedGrav)
            {
                return (uint)x < Map.WIDTH & (uint)Math.Round(y + 3) < Map.HEIGHT ? Collision[x, (int)Math.Round(y + 3)] : CollisionType.None;
            }
            return (uint)x < Map.WIDTH & (uint)Math.Round(y) < Map.HEIGHT ? Collision[x, (int)Math.Round(y)] : CollisionType.None;
        }

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
