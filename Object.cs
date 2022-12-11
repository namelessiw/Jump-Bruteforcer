using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump_Bruteforcer
{
    enum ObjectType
    {
        Block,
        MiniBlock,
        SpikeUp,
        SpikeRight,
        SpikeDown,
        SpikeLeft,
        MiniSpikeUp,
        MiniSpikeRight,
        MiniSpikeDown,
        MiniSpikeLeft,
        Apple,
        Save,
        Platform,
        Water1,
        Water2,
        Water3,
        Warp,
        PlayerStart,
        KillerBlock,
        JumpRefresher,
        GravityArrowUp,
        GravityArrowDown,
        SaveUpsideDown,
        VineLeft,
        VineRight,
        BulletBlocker,
        Unknown
    }

    enum CollisionType
    {
        None,
        Solid,
        Killer,
        Warp,
        Water1,
        Water2,
        Water3
    }

    class Object
    {
        public double X, Y;
        public ObjectType Type;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        public Object(double X, double Y, ObjectType Type)
        {
            this.X = X;
            this.Y = Y;
            this.Type = Type;
        }

        public Object(double X, double Y, ObjectType Type, Dictionary<string, string> Properties) : this(X, Y, Type)
        {
            this.Properties = Properties;
        }

        static readonly CollisionType[] CollisionOrder = new CollisionType[]
        {
            CollisionType.Solid,
            CollisionType.Killer,
            CollisionType.Warp,
            CollisionType.Water1,
            CollisionType.Water3,
            CollisionType.Water2,
        };

        public static CollisionType GetHigherCollisionPriority(CollisionType ct1, CollisionType ct2)
        {
            if (ct1 == ct2)
                return ct1;

            foreach (CollisionType ct in CollisionOrder)
            {
                if (ct1 == ct || ct2 == ct)
                    return ct;
            }

            throw new Exception($"No collision order definitions for collision types {ct1} and {ct2}");
        }
    }
}
