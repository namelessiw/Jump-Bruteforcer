namespace Jump_Bruteforcer
{
    public enum ObjectType
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

    //Must be ordered by collision priority
    public enum CollisionType
    {
        Solid,
        Killer,
        Warp,
        Water1,
        Water3,
        Water2,
        None
    }

    class Object
    {
        public double X, Y;
        public ObjectType Type;
        public Dictionary<string, string> Properties = new();

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


        public static CollisionType GetHigherCollisionPriority(CollisionType ct1, CollisionType ct2)
        {
            return ct1 < ct2? ct1: ct2;
        }
    }
}
