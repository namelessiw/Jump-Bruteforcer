﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump_Bruteforcer
{
    internal class RangePlayer
    {
        int X, Frame;
        double YUpper, YLower, VSpeed;
        bool CanSingleJump, CanDoubleJump;

        public RangePlayer(int X, int Y)
        {
            this.X = X;
            (YUpper, YLower) = GetPixelBounds(Y);
            VSpeed = 0;
            Frame = 0;
            CanSingleJump = false;
            CanDoubleJump = true;
        }

        public RangePlayer(int X, double YUpper, double YLower)
        {
            this.X = X;
            this.YUpper = YUpper;
            this.YLower = YLower;
            VSpeed = 0;
            Frame = 0;
            CanSingleJump = false;
            CanDoubleJump = true;
        }

        public RangePlayer(int X, int Y, bool CanSingleJump, bool CanDoubleJump, double VSpeed) : this(X, Y)
        {
            this.CanDoubleJump = CanDoubleJump;
            this.CanSingleJump = CanSingleJump;
            this.VSpeed = VSpeed;
        }

        public RangePlayer(RangePlayer p)
        {
            X = p.X;
            YUpper = p.YUpper;
            YLower = p.YLower;
            VSpeed = p.VSpeed;
            Frame = p.Frame;
            CanSingleJump = p.CanSingleJump;
            CanDoubleJump = p.CanDoubleJump;
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
            return $"({X}, [{YUpper}, {YLower}]), Frame {Frame}";
        }
    }
}
