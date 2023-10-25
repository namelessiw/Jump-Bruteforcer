using System.Collections.Immutable;

namespace Jump_Bruteforcer
{
    public class CollisionMap
    {
        public ImmutableSortedSet<CollisionType>[,] Collision { get; init; }
        public ImmutableSortedSet<CollisionType>[,] ScraperCollision { get; init; }
        public List<Object> Platforms { get; init; }

        private readonly VineDistance[,,] vineDistance;
        private readonly HashSet<(int x, int y)> goalPixels;
        private readonly HashSet<(int x, int y)> scraperGoalPixels;

        public CollisionMap(ImmutableSortedSet<CollisionType>[,]? Collision, ImmutableSortedSet<CollisionType>[,]? ScraperCollision,  List<Object>? Platforms, VineDistance[,,] vineDistances)
        {
            this.Collision = Collision ?? new ImmutableSortedSet<CollisionType>[Map.WIDTH, Map.HEIGHT];
            this.ScraperCollision = ScraperCollision ?? new ImmutableSortedSet<CollisionType>[Map.WIDTH, Map.HEIGHT];
            this.Platforms = Platforms ?? new List<Object>();
            this.vineDistance = vineDistances;
            this.goalPixels = new();
            this.scraperGoalPixels = new();
            for (int x = 0; x < Map.WIDTH; x++)
            {
                for (int y = 0; y < Map.HEIGHT; y++)
                {
                    if (this.Collision[x, y].Contains(CollisionType.Warp))
                    {
                        goalPixels.Add((x, y));
                    }
                    if (this.ScraperCollision[x, y].Contains(CollisionType.Warp))
                    {
                        scraperGoalPixels.Add((x, y));
                    }
                }
            }

        }
        public CollisionMap(ImmutableSortedSet<CollisionType>[,]? Collision, List<Object>? Platforms, VineDistance[,,] vineDistances)
        {
            this.Collision = Collision ?? new ImmutableSortedSet<CollisionType>[Map.WIDTH, Map.HEIGHT];

            this.Platforms = Platforms ?? new List<Object>();
            this.vineDistance = vineDistances;
            this.goalPixels = new();

            for (int x = 0; x < Map.WIDTH; x++)
            {
                for (int y = 0; y < Map.HEIGHT; y++)
                {
                    if (this.Collision[x, y].Contains(CollisionType.Warp))
                    {
                        goalPixels.Add((x, y));
                    }
 
                }
            }

        }
        public bool onWarp(int x, double y) => goalPixels.Contains((x, (int)Math.Round(y)));
        public bool ScraperOnWarp(int x, double y) => scraperGoalPixels.Contains((x, (int)Math.Round(y)));
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
        public CollisionMap(Dictionary<(int, int), ImmutableSortedSet<CollisionType>>? Collision, List<Object>? Platforms)
        {
            this.Collision = new ImmutableSortedSet<CollisionType>[Map.WIDTH, Map.HEIGHT];
            for (int i = 0; i < Map.WIDTH; i++)
            {
                for (int j = 0; j < Map.HEIGHT; j++)
                {
                    this.Collision[i, j] = ImmutableSortedSet<CollisionType>.Empty;
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
        public CollisionType GetHighestPriorityCollisionType(int x, int y)
        {
            return (uint)x < Map.WIDTH & (uint)y < Map.HEIGHT ? Collision[x, y].FirstOrDefault() : CollisionType.None;
        }
        //TODO account for the coordinates offset caused by scraperFacingRight
        public CollisionType GetHighestPriorityCollisionType(int x, int y, bool scraperFacingRight)
        {
            x += (scraperFacingRight ? 7 : 12);
            y -= 3;
            return (uint)x < Map.WIDTH & (uint)y < Map.HEIGHT ? ScraperCollision[x, y].FirstOrDefault() : CollisionType.None;
        }

        /// <summary>
        /// returns the set of CollisionTypes at pixel (x, y) in order of descending priority
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ImmutableSortedSet<CollisionType> GetCollisionTypes(int x, int y)
        {
            return (uint)x < Map.WIDTH & (uint)y < Map.HEIGHT ? Collision[x, y] : ImmutableSortedSet<CollisionType>.Empty;

        }
        public ImmutableSortedSet<CollisionType> GetCollisionTypes(int x, double y)
        {
            return (uint)x < Map.WIDTH & (uint)Math.Round(y) < Map.HEIGHT ? Collision[x, (int)Math.Round(y)] : ImmutableSortedSet<CollisionType>.Empty;
        }

        //TODO account for the coordinates offset caused by scraperFacingRight
        public ImmutableSortedSet<CollisionType> GetCollisionTypes(int x, int y, bool scraperFacingRight)
        {
            x += (scraperFacingRight ? 7 : 12);
            y -= 3;
            return (uint)x < Map.WIDTH & (uint)y < Map.HEIGHT ? ScraperCollision[x, y] : ImmutableSortedSet<CollisionType>.Empty;

        }
        public ImmutableSortedSet<CollisionType> GetCollisionTypes(int x, double y, bool scraperFacingRight)
        {
            x += (scraperFacingRight ? 7 : 12);
            y -= 3;
            return (uint)x < Map.WIDTH & (uint)Math.Round(y) < Map.HEIGHT ? ScraperCollision[x, (int)Math.Round(y)] : ImmutableSortedSet<CollisionType>.Empty;
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
