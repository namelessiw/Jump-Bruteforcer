using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump_Bruteforcer
{
    internal class RangePlayer
    {
        int x, frame;

        double yUpper, yLower, vSpeed;
        bool canSingleJump, canDoubleJump;

        static CollisionMap collisionMap = new(new Dictionary<(int, int), ImmutableSortedSet<CollisionType>>(), null);

        public int X
        {
            get => x;
            set => x = value;
        }

        public int Frame
        {
            get => frame;
            set => frame = value;
        }

        public double YUpper
        {
            get => yUpper;
            set => yUpper = value;
        }

        public double YLower
        {
            get => yLower;
            set => yLower = value;
        }

        public double VSpeed
        {
            get => vSpeed;
            set => vSpeed = value;
        }

        public bool CanSingleJump
        {
            get => canSingleJump;
            set => canSingleJump = value;
        }

        public bool CanDoubleJump
        {
            get => canDoubleJump;
            set => canDoubleJump = value;
        }

        public static void SetCollisionMap(CollisionMap CollisionMap)
        {
            collisionMap = CollisionMap;
        }

        public RangePlayer(int X, int Y)
        {
            x = X;
            (yUpper, yLower) = GetPixelBounds(Y);
            vSpeed = 0;
            frame = 0;
            canSingleJump = false;
            canDoubleJump = true;
        }

        public RangePlayer(int X, double YUpper, double YLower)
        {
            x = X;
            yUpper = YUpper;
            yLower = YLower;
            vSpeed = 0;
            frame = 0;
            canSingleJump = false;
            canDoubleJump = true;
        }

        public RangePlayer(int X, int Y, bool CanSingleJump, bool CanDoubleJump, double VSpeed) : this(X, Y)
        {
            canDoubleJump = CanDoubleJump;
            canSingleJump = CanSingleJump;
            vSpeed = VSpeed;
        }

        public RangePlayer(RangePlayer p)
        {
            x = p.x;
            yUpper = p.yUpper;
            yLower = p.yLower;
            vSpeed = p.vSpeed;
            frame = p.frame;
            canSingleJump = p.canSingleJump;
            canDoubleJump = p.canDoubleJump;
        }

        // assumes range never spans more than 2px
        public RangePlayer SplitOnPixelBoundary()
        {
            (double _, double newYLower) = GetPixelBounds((int)Math.Round(YUpper));
            (double newYUpper, double _) = GetPixelBounds((int)Math.Round(YLower));

            RangePlayer newPlayer = new RangePlayer(this);
            newPlayer.YLower = newYLower;
            YUpper = newYUpper;

            return newPlayer;
        }

        // returns player on floor (full sjump range)
        // y 407 => (405.5 - 406.5]
        // y 408 => [406.5 - 407.5)
        public static RangePlayer FromFloorPixel(int X, int Y)
        {
            // assume always at least two pixels of floor
            // in case of 1x1 dotkid this could be different
            (double YUpper, double YLower) = (GetPixelBounds(Y).YUpper, GetPixelBounds(Y + 1).YLower);

            YUpper -= 1;
            YLower -= 1;

            // remove positions that would be inside of the floor
            (double _, double OverFloor) = GetPixelBounds(Y - 1);
            YLower = Math.Min(YLower, OverFloor);

            return new RangePlayer(X, YUpper, YLower);
        }

        // y 407 => (406.5, 407.5)
        // y 406 => [405.5, 406.5]
        static (double YUpper, double YLower) GetPixelBounds(int Y)
        {
            // naming logic:
            // lower y value => higher position on screen
            double YUpper = Y - 0.5;
            double YLower = Y + 0.5;

            if (Y % 2 != 0)
            {
                YUpper = double.BitIncrement(YUpper);
                YLower = double.BitDecrement(YLower);
            }

            return (YUpper, YLower);
        }

        public override string ToString()
        {
            return $"({x}, [{yUpper}, {yLower}]), Frame {frame}";
        }
    }
}
