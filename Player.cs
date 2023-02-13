using System.Collections.Immutable;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Jump_Bruteforcer
{
    [Flags]
    public enum Input
    {
        Neutral = 0,
        Left = 1,
        Right = 2,
        Jump = 4,
        Release = 8
    }

    public static class Player
    {
        public static bool OnGround(int x, double y, Dictionary<(int X, int Y), CollisionType> CollisionMap)
        {
            return CollisionMap.TryGetValue((x, (int)Math.Round(y + 1)), out CollisionType ctype) && ctype == CollisionType.Solid;
        }

        public static (double, bool) CalculateVSpeed(PlayerNode n, Input input, Dictionary<(int X, int Y), CollisionType> CollisionMap)
        {
            double finalVSpeed = n.State.VSpeed;
            bool DJumpRefresh = false;

            finalVSpeed = Math.Clamp(finalVSpeed, -PhysicsParams.MAX_VSPEED, PhysicsParams.MAX_VSPEED);


            if (input.HasFlag(Input.Jump))
            {
                if (OnGround(n.State.X, n.State.Y, CollisionMap))
                {
                    finalVSpeed = PhysicsParams.SJUMP_VSPEED;       
                    DJumpRefresh= true;
                }
                else if (n.State.CanDJump)
                {
                    finalVSpeed = PhysicsParams.DJUMP_VSPEED;
                }
            }
            if (input.HasFlag(Input.Release) && finalVSpeed < 0)
            {
                finalVSpeed *= PhysicsParams.RELEASE_MULTIPLIER;
            }
            finalVSpeed += PhysicsParams.GRAVITY;

            return (finalVSpeed, DJumpRefresh);
        }

        public static (CollisionType Type, int NewX, double NewY, bool VSpeedReset, bool DJumpRefresh) CollisionCheck(Dictionary<(int X, int Y), CollisionType> CollisionMap, int CurrentX, int NewX, double CurrentY, double NewY)
        {
            int RoundedNewY = (int)Math.Round(NewY);

            if (CollisionMap.TryGetValue((NewX, RoundedNewY), out CollisionType Type))
            {
                bool DJumpRefresh;
                switch (Type)
                {
                    case CollisionType.Killer:
                    case CollisionType.Warp:
                        return (Type, NewX, NewY, false, false);
                    case CollisionType.Solid:
                        bool VSpeedReset;
                        (NewX, NewY, VSpeedReset, DJumpRefresh) = SolidCollision(CollisionMap, CurrentX, NewX, CurrentY, NewY);

                        if (CollisionMap.TryGetValue((NewX, (int)Math.Round(NewY)), out Type))
                        {
                            return (Type, NewX, NewY, VSpeedReset, DJumpRefresh);
                        }
                        return (CollisionType.Solid, NewX, NewY, VSpeedReset, DJumpRefresh);

                    default:
                        //throw new NotImplementedException($"Collision with type {Type} not implemented. collision at x={NewX}, y={RoundedNewY}");
                        return (Type, NewX, NewY, false, false);
                }
            }
            return (CollisionType.None, NewX, NewY, false, false);
        }

        private static (int NewX, double NewY, bool VSpeedReset, bool DJumpRefresh) SolidCollision(Dictionary<(int X, int Y), CollisionType> CollisionMap, int CurrentX, int NewX, double CurrentY, double NewY)
        {
            int CurrentYRounded = (int)Math.Round(CurrentY);
            int NewYRounded = (int)Math.Round(NewY);
            bool VSpeedReset = false;
            bool DJumpRefresh = false;
            CollisionType Type;

            if (CollisionMap.TryGetValue((NewX, CurrentYRounded), out Type) && Type == CollisionType.Solid)
            {
                int sign = Math.Sign(NewX - CurrentX);
                if (sign != 0)
                {
                    while (!CollisionMap.TryGetValue((CurrentX + sign, CurrentYRounded), out Type) || Type != CollisionType.Solid)
                    {
                        CurrentX += sign;
                    }
                }

            }
            if (CollisionMap.TryGetValue((CurrentX, NewYRounded), out Type) && Type == CollisionType.Solid)
            {
                // (re)rounding everytime because otherwise vfpi would lose its parity
                double VSpeed = NewY - CurrentY;
                int sign = Math.Sign(VSpeed);
                if (sign != 0)
                {
                    if (VSpeed > 0)
                    {
                        DJumpRefresh= true;
                    }
                    int yRounded = (int)Math.Round(CurrentY + sign);
                    while (Math.Abs(VSpeed) >= 1 && (!CollisionMap.TryGetValue((CurrentX, yRounded), out Type) || Type != CollisionType.Solid))
                    {
                        CurrentY += sign;
                        VSpeed -= sign;
                        yRounded = (int)Math.Round(CurrentY + sign);
                    }
                }


                NewYRounded = (int)Math.Round(CurrentY);
                NewY = CurrentY;
                VSpeedReset = true;
            }

            if (CollisionMap.TryGetValue((NewX, NewYRounded), out Type) && Type == CollisionType.Solid)
            {
                NewX = CurrentX;
            }

            return (NewX, NewY, VSpeedReset, DJumpRefresh);
        }

        public static bool IsAlive(Dictionary<(int X, int Y), CollisionType> CollisionMap, PlayerNode node)
        {
            int yRounded = node.State.RoundedY;
            CollisionMap.TryGetValue((node.State.X, yRounded), out CollisionType ctype);
            bool inbounds =  node.State.X is >= 0 and <= 799 & yRounded is >= 0 and <= 607;
            return ctype != CollisionType.Killer & inbounds;
        }
    }
}
