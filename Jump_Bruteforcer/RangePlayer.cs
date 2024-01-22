using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jump_Bruteforcer
{
    internal class RangePlayer
    {
        // eventually account for borders

        int x, frame, hSpeed;

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

        public int HSpeed
        {
            get => hSpeed;
            set => hSpeed = value;
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

        public RangePlayer(int X, double YUpper, double YLower, bool CanSingleJump, bool CanDoubleJump, double VSpeed)
        {
            x = X;
            yUpper = YUpper;
            yLower = YLower;
            canDoubleJump = CanDoubleJump;
            canSingleJump = CanSingleJump;
            vSpeed = VSpeed;
            frame = 0;
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

        public bool IsAlive() => !PlaceMeeting(x, yUpper, false, CollisionType.Killer);

        public bool CanRelease() => vSpeed < 0;

        private static bool PlaceMeeting(int x, double y, bool kidUpsidedown, CollisionType type)
        {
            return collisionMap.GetCollisionTypes(x, y, kidUpsidedown).Contains(type);
        }

        private static bool PlaceFree(int x, double y, bool kidUpsidedown)
        {
            return collisionMap.GetHighestPriorityCollisionType(x, y, kidUpsidedown) != CollisionType.Solid;
        }

        public RangePlayer Advance(Input input)
        {
            int h = (input & Input.Left) == Input.Left ? -1 : 0;
            h = (input & Input.Right) == Input.Right ? 1 : h;

            if (h != 0)
            {
                hSpeed = h * PhysicsParams.WALKING_SPEED;
            }

            // playerJump
            if ((input & Input.Jump) == Input.Jump)
            {
                vSpeed = canSingleJump ? PhysicsParams.SJUMP_VSPEED : PhysicsParams.DJUMP_VSPEED;
                canDoubleJump = canSingleJump;
            }

            canSingleJump = false;

            // playerVJump
            if ((input & Input.Release) == Input.Release)
            {
                vSpeed *= PhysicsParams.RELEASE_MULTIPLIER;
            }

            if (vSpeed > PhysicsParams.MAX_VSPEED)
            {
                vSpeed = PhysicsParams.MAX_VSPEED;
            }

            frame += 1;

            // movement
            vSpeed += PhysicsParams.GRAVITY;

            int xPrevious = x;
            double yUpperPrevious = yUpper, yLowerPrevious = yLower;

            x += hSpeed;
            yUpper += vSpeed;
            yLower += vSpeed;

            // collision
            var collisionTypesUpper = collisionMap.GetCollisionTypes(x, yUpper, false);
            var collisionTypesLower = collisionMap.GetCollisionTypes(x, yLower, false);

            RangePlayer UpperPlayer = null;

            // TODO: this assumes vertical collision, should be rewritten to work regardless of horizontal/vertical collision and preferrably also actually accurate
            bool upperSolid = collisionTypesUpper.Contains(CollisionType.Solid);
            bool lowerSolid = collisionTypesLower.Contains(CollisionType.Solid);
            if (upperSolid || lowerSolid)
            {
                (x, yUpper, yLower) = (xPrevious, yUpperPrevious, yLowerPrevious);
                if (Math.Round(yUpper) != Math.Round(yLower))
                {
                    UpperPlayer = SplitOnPixelBoundary();

                    if (lowerSolid)
                    {
                        SolidCollision(this);
                    }
                    else
                    {
                        x += hSpeed;
                        yUpper += vSpeed;
                        yLower += vSpeed;
                    }
                    if (upperSolid)
                    {
                        SolidCollision(UpperPlayer);
                    }
                    else
                    {
                        UpperPlayer.x += UpperPlayer.hSpeed;
                        UpperPlayer.yUpper += UpperPlayer.vSpeed;
                        UpperPlayer.yLower += UpperPlayer.vSpeed;
                    }
                }
                else
                {
                    SolidCollision(this);
                }
            }

            bool lowerKiller = PlaceMeeting(x, yLower, false, CollisionType.Killer);
            bool upperKiller = PlaceMeeting(x, yUpper, false, CollisionType.Killer);
            if (lowerKiller != upperKiller)
            {
                UpperPlayer = SplitOnPixelBoundary();
            }

            static void SolidCollision(RangePlayer p)
            {
                if (!PlaceFree(p.x + p.hSpeed, p.yUpper, false))
                {
                    int sign = Math.Sign(p.hSpeed);
                    if (sign != 0)
                    {
                        while (PlaceFree(p.x + sign, p.yUpper, false))
                        {
                            p.x += sign;
                        }
                        p.hSpeed = 0;
                    }
                }

                if (!PlaceFree(p.x, p.yUpper + p.vSpeed, false))
                {
                    int sign = Math.Sign(p.vSpeed);
                    if (sign != 0)
                    {
                        p.canDoubleJump |= sign == 1;
                        while (Math.Abs(p.vSpeed) >= 1 && PlaceFree(p.x, p.yUpper + sign, false))
                        {
                            p.yUpper += sign;
                            p.yLower += sign;
                            p.vSpeed -= sign;
                        }
                        p.vSpeed = 0;
                    }
                }

                if (!PlaceFree(p.x + p.hSpeed, p.yUpper + p.vSpeed, false))
                {
                    p.hSpeed = 0;
                }

                p.x += p.hSpeed;
                p.yUpper += p.vSpeed;
                p.yLower += p.vSpeed;

                (double pixelBoundUpper, double pixelBoundLower) = GetPixelBounds((int)Math.Round((p.yUpper + p.yLower) / 2));
                p.YUpper = Math.Max(p.YUpper, pixelBoundUpper);
                p.YLower = Math.Min(p.YLower, pixelBoundLower);

                /*if (!PlaceFree(x, yUpper, false))
                {
                    (x, yUpper, yLower) = (xPrevious, yUpperPrevious, yLowerPrevious);
                }*/
            }

            return UpperPlayer;
        }

        // assumes range never spans more than 2px
        // this becomes lower, new becomes upper range
        public RangePlayer SplitOnPixelBoundary()
        {
            double newYLower = GetLowerPixelBound((int)Math.Round(YUpper));
            double newYUpper = GetUpperPixelBound((int)Math.Round(YLower));

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
            (double YUpper, double YLower) = (GetUpperPixelBound(Y), GetLowerPixelBound(Y + 1));

            YUpper -= 1;
            YLower -= 1;

            // remove positions that would be inside of the floor
            double OnFloorBound = GetLowerPixelBound(Y - 1);
            YLower = Math.Min(YLower, OnFloorBound);

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

        static double GetUpperPixelBound(int Y)
        {
            double YUpper = Y - 0.5;

            if (Y % 2 != 0)
            {
                YUpper = double.BitIncrement(YUpper);
            }

            return YUpper;
        }

        static double GetLowerPixelBound(int Y)
        {
            double YLower = Y + 0.5;

            if (Y % 2 != 0)
            {
                YLower = double.BitDecrement(YLower);
            }

            return YLower;
        }

        public override string ToString()
        {
            return $"({x}, [{yUpper}, {yLower}]), Frame {frame}";
        }
    }
}
