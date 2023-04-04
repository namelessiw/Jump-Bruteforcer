using System;
using System.Collections.Immutable;
using System.Windows;

namespace Jump_Bruteforcer
{

    public class BoundingBox
    {
        public Rect rect;
        public BoundingBox(int x, int y, int width, int height)
        {
            rect = new(x, y, width, height);
        }

        public bool Contains(int x, int y)
        {
            return rect.Contains(x, y);
        }
    }


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
        Platform,
        Killer,
        Solid
    }

    public class Object
    {
        public int X, Y;
        public ObjectType ObjectType;
        public CollisionType CollisionType;
        public BoundingBox? bbox;
        public int instanceNum;
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

        public Object(int X, int Y, ObjectType objectType, BoundingBox? bbox = null, int instanceNum = 0)
        {
            this.X = X;
            this.Y = Y;
            ObjectType = objectType;
            CollisionType = toCollisionType[objectType];
            this.bbox = bbox;
            this.instanceNum = instanceNum;
        }

        public override string ToString()
        {
            return $"({ObjectType}, {X}, {Y})";
        }
    }
}
