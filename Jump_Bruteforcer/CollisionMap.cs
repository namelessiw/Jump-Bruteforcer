using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jump_Bruteforcer
{
    public class CollisionMap
    {
        public Dictionary<(int X, int Y), ImmutableSortedSet<CollisionType>> Collision { get; init; }
        public List<Object> Platforms { get; init; }
        public CollisionMap(Dictionary<(int X, int Y), ImmutableSortedSet<CollisionType>>? Collision, List<Object>? Platforms)
        {
            this.Collision = Collision ?? new Dictionary<(int X, int Y), ImmutableSortedSet<CollisionType>>();
            this.Platforms = Platforms ?? new List<Object>();
        }

        public CollisionType GetHighestPriorityCollisionType(int x, int y)
        {
            Collision.TryGetValue((x, y), out ImmutableSortedSet<CollisionType>? type);
            return type?.Last() ?? CollisionType.None;
        }

        /// <summary>
        /// returns the set of CollisionTypes at pixel (x, y) in order of descending priority
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ImmutableSortedSet<CollisionType> GetCollisionTypes(int x, int y)
        {
            Collision.TryGetValue((x, y), out ImmutableSortedSet<CollisionType>? type);
            return type ?? ImmutableSortedSet<CollisionType>.Empty;
        }
        public ImmutableSortedSet<CollisionType> GetCollisionTypes(int x, double y)
        {
            Collision.TryGetValue((x, (int)Math.Round(y)), out ImmutableSortedSet<CollisionType>? type);
            return type ?? ImmutableSortedSet<CollisionType>.Empty;
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
    }
}
