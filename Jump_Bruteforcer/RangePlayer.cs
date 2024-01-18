using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump_Bruteforcer.Jump_Bruteforcer
{
    internal class RangePlayer
    {
        int X, Frame;
        double YUpper, YLower, VSpeed;
        bool DoubleJump;

        public RangePlayer(int X, int Y)
        {
            this.X = X;
            (YUpper, YLower) = GetRoundingBounds(Y);
            VSpeed = 0;
            Frame = 0;
            DoubleJump = true;
        }

        public RangePlayer(int X, int Y, bool DoubleJump) : this(X, Y)
        {
            this.DoubleJump = DoubleJump;
        }

        public RangePlayer(RangePlayer p)
        {
            X = p.X;
            YUpper = p.YUpper;
            YLower = p.YLower;
            VSpeed = p.VSpeed;
            Frame = p.Frame;
            DoubleJump = p.DoubleJump;
        }

        // y 407 => [406.5, 407.5)
        // y 406 => (405.5, 406.5]
        (double YUpper, double YLower) GetRoundingBounds(int Y)
        {
            // naming logic:
            // lower y value => higher position on screen
            double YUpper = Y - 0.5;
            double YLower = Y + 0.5;

            if (Y % 2 == 0)
            {
                YUpper = double.BitIncrement(YUpper);
            }
            else
            {
                YLower = double.BitDecrement(YLower);
            }

            return (YUpper, YLower);
        }


    }
}
