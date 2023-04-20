using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Jump_Bruteforcer
{
    public class CollisionMap
    {
        public ImmutableSortedSet<CollisionType>[,] Collision { get; init; }
        public List<Object> Platforms { get; init; }
        public CollisionMap(ImmutableSortedSet<CollisionType>[,]? Collision, List<Object>? Platforms)
        {
            this.Collision = Collision ?? new ImmutableSortedSet<CollisionType>[Map.WIDTH, Map.HEIGHT];
            this.Platforms = Platforms ?? new List<Object>();
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

            this.Platforms = Platforms ?? new List<Object>();
        }

        public CollisionType GetHighestPriorityCollisionType(int x, int y)
        {
            return (uint)x < Map.WIDTH & (uint)y < Map.HEIGHT?  Collision[x, y].FirstOrDefault() : CollisionType.None;
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
