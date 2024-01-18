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
        Water2CurveRight,
        Platform,
        Water1,
        Water2,
        VineLeft,
        VineRight,
        KillerBlock,
        Water1CurveRight,
        PlayerStart,
        Warp,
        JumpRefresher,
        Water3,
        Water3CurveLeft,
        Water3CurveRight,
        Water2CurveLeft,
        MiniKillerBlock,
        Booster,
        BoosterMini,
        RefreshBlock,
        FruitRefresher,
        GravityBlockUp,
        GravityBlockDown,
        ElevatorUp,
        ElevatorDown,
        LineKillerV,
        LineKillerH,
        LineKillerDA,
        LineKillerDB,
        ShootRefresherL,
        ShootRefresherR,
        TripleAdd,
        DotField,
        NoDot,
        CatharsisWater,
        TripleRemove,
        WaterDisappear,
        WaterMini,
        Water2Mini,
        Water3Mini,
        GravityBlockUpMini,
        GravityBlockDownMini,
        ElevatorUpMini,
        ElevatorDownMini,
        TripleAddMini,
        DotFieldMini,
        NoDotMini,
        CatharsisWaterMini,
        WaterDisappearMini,
        SidewaysPlatform,
    }

    //Must be ordered by ascending collision priority
    public enum CollisionType
    {
        None,
        CatharsisWater,
        Water2,
        Water3,
        Water1,
        GravityArrowDown,
        GravityArrowUp,
        Warp,
        Platform,
        Killer,
        Solid
    }

    public class Object
    {
        public int X, Y;
        public double XScale, YScale;
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
            {ObjectType.Water2CurveRight,CollisionType.Water2},
            {ObjectType.Platform,CollisionType.Platform},
            {ObjectType.Water1,CollisionType.Water1 },
            {ObjectType.Water2,CollisionType.Water2 },
            {ObjectType.VineRight,CollisionType.None },
            {ObjectType.VineLeft,CollisionType.None },
            {ObjectType.KillerBlock,CollisionType.Killer },
            {ObjectType.Water1CurveRight,CollisionType.Water1},
            {ObjectType.PlayerStart,CollisionType.None },
            {ObjectType.Warp,CollisionType.Warp },
            {ObjectType.JumpRefresher,CollisionType.None },
            {ObjectType.Water3,CollisionType.CatharsisWater },
            {ObjectType.Water3CurveLeft,CollisionType.CatharsisWater },
            {ObjectType.Water3CurveRight,CollisionType.CatharsisWater },
            {ObjectType.Water2CurveLeft,CollisionType.Water2 },
            {ObjectType.MiniKillerBlock, CollisionType.None},
            {ObjectType.Booster, CollisionType.None },
            {ObjectType.BoosterMini, CollisionType.None },
            {ObjectType.RefreshBlock, CollisionType.None },
            {ObjectType.FruitRefresher, CollisionType.None },
            {ObjectType.GravityBlockUp, CollisionType.None },
            {ObjectType.GravityBlockDown, CollisionType.None },
            {ObjectType.ElevatorUp, CollisionType.None },
            {ObjectType.ElevatorDown, CollisionType.None },
            {ObjectType.LineKillerV, CollisionType.None },
            {ObjectType.LineKillerH, CollisionType.None },
            {ObjectType.LineKillerDA, CollisionType.None },
            {ObjectType.LineKillerDB, CollisionType.None },
            {ObjectType.ShootRefresherL, CollisionType.None },
            {ObjectType.ShootRefresherR, CollisionType.None },
            {ObjectType.TripleAdd, CollisionType.None },
            {ObjectType.DotField, CollisionType.None },
            {ObjectType.NoDot, CollisionType.None },
            {ObjectType.CatharsisWater, CollisionType.CatharsisWater },
            {ObjectType.TripleRemove, CollisionType.None },
            {ObjectType.WaterDisappear, CollisionType.None },
            {ObjectType.WaterMini, CollisionType.None },
            {ObjectType.Water2Mini, CollisionType.None },
            {ObjectType.Water3Mini, CollisionType.None },
            {ObjectType.GravityBlockUpMini, CollisionType.None },
            {ObjectType.GravityBlockDownMini, CollisionType.None },
            {ObjectType.ElevatorUpMini, CollisionType.None },
            {ObjectType.ElevatorDownMini, CollisionType.None },
            {ObjectType.TripleAddMini, CollisionType.None },
            {ObjectType.DotFieldMini, CollisionType.None },
            {ObjectType.NoDotMini, CollisionType.None },
            {ObjectType.CatharsisWaterMini, CollisionType.CatharsisWater },
            {ObjectType.WaterDisappearMini, CollisionType.None },
            {ObjectType.SidewaysPlatform, CollisionType.None },
        };

        public Object(int X, int Y, double XScale, double YScale, ObjectType objectType, int instanceNum = 0)
        {
            this.XScale = XScale;
            this.YScale = YScale;

            if (objectType == ObjectType.Apple)
            {
                X -= 10;
                Y -= 12;
            }
            if (objectType == ObjectType.Warp)
            {
                X -= 16;
                Y -= 16;
            }
            BoundingBox? bbox = null;
            if (objectType == ObjectType.Platform)
            {
                bbox = new BoundingBox(X - 5, Y - 10, 42, 36);
            }
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
