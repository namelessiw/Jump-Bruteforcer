using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump_Bruteforcer
{
    public class CollisionMap
    {
        public Dictionary<(int X, int Y), CollisionType> Collision { get; init; }
        public List<Object> Platforms { get; init; }
        public CollisionMap(Dictionary<(int X, int Y), CollisionType>? Collision, List<Object>? Platforms)
        {
            this.Collision = Collision is not null? Collision : new Dictionary<(int X, int Y), CollisionType>();
            this.Platforms = Platforms is not null? Platforms : new List<Object>();
        }

        public CollisionType GetCollisionType(int x, int y)
        {
            Collision.TryGetValue((x, y), out CollisionType type);
            return type;
        }

        public static void GetTouchingPlatforms(int x, int y)
        {
            throw new NotImplementedException();
        }
    }
}
