using System;
using System.Collections.Immutable;

namespace Jump_Bruteforcer
{
    //must be ordered by ID
    //names need to correspond to filenames of the images of the objects
    public enum ObjectType
    {
        Unknown,
        Block,
        MiniBlock,
        SpikeUp,
        SpikeRight,
        SpikeLeft,
        SpikeDown,
        MiniSpikeUp,
        MiniSpikeRight,
        MiniSpikeLeft,
        MiniSpikeDown,
        Apple,
        Save,
        Platform,
        Water1,
        Water2,
        VineRight,
        VineLeft,
        KillerBlock,
        BulletBlocker,
        PlayerStart,
        Warp,
        JumpRefresher,
        Water3,
        GravityArrowUp,
        GravityArrowDown,
        SaveUpsideDown
    }

    //Must be ordered by ascending collision priority
    public enum CollisionType
    {
        None,
        Water2,
        Water3,
        Water1,
        Warp,
        Killer,
        Platform,
        Solid
    }

    public class Object
    {
        public int X, Y;
        public ObjectType ObjectType;
        public CollisionType CollisionType;
        private static readonly Dictionary<ObjectType, CollisionType> toCollisionType = new()
        {
            {ObjectType.Unknown, CollisionType.None},
            {ObjectType.Block, CollisionType.Solid },
            {ObjectType.MiniBlock,CollisionType.Solid },
            {ObjectType.SpikeUp, CollisionType.Killer },
            {ObjectType.SpikeRight,CollisionType.Killer },
            {ObjectType.SpikeLeft,CollisionType.Killer },
            {ObjectType.SpikeDown,CollisionType.Killer },
            {ObjectType.MiniSpikeUp,CollisionType.Killer },
            {ObjectType.MiniSpikeRight,CollisionType.Killer },
            {ObjectType.MiniSpikeLeft,CollisionType.Killer },
            {ObjectType.MiniSpikeDown,CollisionType.Killer },
            {ObjectType.Apple,CollisionType.Killer },
            {ObjectType.Save,CollisionType.None},
            {ObjectType.Platform,CollisionType.Platform},
            {ObjectType.Water1,CollisionType.Water1 },
            {ObjectType.Water2,CollisionType.Water2 },
            {ObjectType.VineRight,CollisionType.None },
            {ObjectType.VineLeft,CollisionType.None },
            {ObjectType.KillerBlock,CollisionType.Killer },
            {ObjectType.BulletBlocker,CollisionType.None},
            {ObjectType.PlayerStart,CollisionType.None },
            {ObjectType.Warp,CollisionType.Warp },
            {ObjectType.JumpRefresher,CollisionType.None },
            {ObjectType.Water3,CollisionType.Water3 },
            {ObjectType.GravityArrowUp,CollisionType.None },
            {ObjectType.GravityArrowDown,CollisionType.None },
            {ObjectType.SaveUpsideDown,CollisionType.None }
        };

        public Object(int X, int Y, ObjectType objectType)
        {
            this.X = X;
            this.Y = Y;
            this.ObjectType = objectType;
            this.CollisionType = toCollisionType[objectType];
        }

        public override string ToString()
        {
            return $"({ObjectType}, {X}, {Y})";
        }
    }
}
