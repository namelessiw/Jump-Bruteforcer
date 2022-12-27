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

        public Object(double X, double Y, ObjectType Type)
        {
            this.X = X;
            this.Y = Y;
            this.Type = Type;
        }



        public static CollisionType GetHigherCollisionPriority(CollisionType ct1, CollisionType ct2)
        {
            return ct1 < ct2? ct1: ct2;
        }
    }
}
