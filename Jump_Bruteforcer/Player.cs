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
        public static bool OnGround(int x, double y, CollisionMap CollisionMap)
        {
            return CollisionMap.GetHighestPriorityCollisionType(x, (int)Math.Round(y + 1)) == CollisionType.Solid;
        }

        /// <summary>
        /// Checks the CollisionMap for collision of "type" at (x, y). If there is, return true, else false.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type"></param>
        /// <param name="CollisionMap"></param>
        /// <returns></returns>
        private static bool PlaceMeeting(int x, int y, CollisionType type, CollisionMap CollisionMap)
        {
            return CollisionMap.GetHighestPriorityCollisionType(x, y) == type;
        }

        public static (double, bool, bool) CalculateVSpeed(PlayerNode n, Input input, CollisionMap CollisionMap)
        {
            double finalVSpeed = n.State.VSpeed;
            bool DJumpRefresh = false;
            bool onPlatform = n.State.OnPlatform && PlaceMeeting(n.State.X, n.State.RoundedY + 4, CollisionType.Platform, CollisionMap);

            finalVSpeed = Math.Clamp(finalVSpeed, -PhysicsParams.MAX_VSPEED, PhysicsParams.MAX_VSPEED);


            if (input.HasFlag(Input.Jump))
            {
                if (OnGround(n.State.X, n.State.Y, CollisionMap) || onPlatform || CollisionMap.GetHighestPriorityCollisionType(n.State.X, (int)Math.Round(n.State.Y + 1)) == CollisionType.Platform)
                {
                    finalVSpeed = PhysicsParams.SJUMP_VSPEED;
                    DJumpRefresh = true;
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

            return (finalVSpeed, DJumpRefresh, onPlatform);
        }

        public static (CollisionType Type, int NewX, double NewY, bool VSpeedReset, bool DJumpRefresh, bool OnPlatform) CollisionCheck(CollisionMap CollisionMap, int CurrentX, int NewX, double CurrentY, double NewY, double CurrentVSpeed = 0)
        {
            int RoundedNewY = (int)Math.Round(NewY);
            CollisionType ctype = CollisionMap.GetHighestPriorityCollisionType(NewX, RoundedNewY);

            if (ctype != CollisionType.None)
            {
                bool DJumpRefresh;
                bool OnPlatform;
                switch (ctype)
                {
                    case CollisionType.Killer:
                    case CollisionType.Warp:
                        return (ctype, NewX, NewY, false, false, false);
                    case CollisionType.Solid:
                        bool VSpeedReset;
                        (NewX, NewY, VSpeedReset, DJumpRefresh, OnPlatform) = SolidCollision(CollisionMap, CurrentX, NewX, CurrentY, NewY);
                        return (ctype, NewX, NewY, VSpeedReset, DJumpRefresh, OnPlatform);
                    case CollisionType.Platform:
                        (NewX, NewY, VSpeedReset, DJumpRefresh, OnPlatform) = PlatformCollision(CollisionMap, NewX, NewY, CurrentVSpeed, false);
                        return (ctype, NewX, NewY, VSpeedReset, DJumpRefresh, OnPlatform);

                    default:
                        //throw new NotImplementedException($"Collision with type {Type} not implemented. collision at x={NewX}, y={RoundedNewY}");
                        return (ctype, NewX, NewY, false, false, false);
                }
            }
            return (CollisionType.None, NewX, NewY, false, false, false);
        }


        private static (int NewX, double NewY, bool VSpeedReset, bool DJumpRefresh, bool OnPlatform) PlatformCollision(CollisionMap collisionMap, int newX, double newY, double currentVSpeed, bool VSpeedReset)
        {

            bool dJumpRefresh = false;
            bool onPlatform = false;
            bool vSpeedReset = VSpeedReset;
            int minInstanceNum = 0;


            Object? platform = collisionMap.GetCollidingPlatform(newX, (int)Math.Round(newY), minInstanceNum);

            while (platform is not null)
            {
                if (newY - currentVSpeed / 2 <= platform.Y)
                {
                    newY = platform.Y - 9;
                    vSpeedReset = true;
                    dJumpRefresh = true;
                    onPlatform = true;
                }
                minInstanceNum = platform.instanceNum + 1;
                platform = collisionMap.GetCollidingPlatform(newX, (int)Math.Round(newY), minInstanceNum);
            }

            return (newX, newY, vSpeedReset, dJumpRefresh, onPlatform);

        }

        private static (int NewX, double NewY, bool VSpeedReset, bool DJumpRefresh, bool OnPlatform) SolidCollision(CollisionMap CollisionMap, int CurrentX, int NewX, double CurrentY, double NewY)
        {
            int CurrentYRounded = (int)Math.Round(CurrentY);
            int NewYRounded = (int)Math.Round(NewY);
            bool VSpeedReset = false;
            bool DJumpRefresh = false;
            double VSpeed = NewY - CurrentY;

            if (CollisionMap.GetHighestPriorityCollisionType(NewX, CurrentYRounded) == CollisionType.Solid)
            {
                int sign = Math.Sign(NewX - CurrentX);
                if (sign != 0)
                {
                    while (CollisionMap.GetHighestPriorityCollisionType(CurrentX + sign, CurrentYRounded) != CollisionType.Solid)
                    {
                        CurrentX += sign;
                    }
                }
                NewX = CurrentX;

            }
            if (CollisionMap.GetHighestPriorityCollisionType(CurrentX, NewYRounded) == CollisionType.Solid)
            {
                // (re)rounding everytime because otherwise vfpi would lose its parity

                int sign = Math.Sign(VSpeed);
                if (sign != 0)
                {
                    if (VSpeed > 0)
                    {
                        DJumpRefresh = true;
                    }
                    int yRounded = (int)Math.Round(CurrentY + sign);
                    while (Math.Abs(VSpeed) >= 1 && CollisionMap.GetHighestPriorityCollisionType(CurrentX, yRounded) != CollisionType.Solid)
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

            if (CollisionMap.GetHighestPriorityCollisionType(NewX, NewYRounded) == CollisionType.Solid)
            {
                NewX = CurrentX;
            }

            if (CollisionMap.GetCollidingPlatform(NewX, (int)Math.Round(NewY), 0) is not null)
            {
                return PlatformCollision(CollisionMap, NewX, NewY, VSpeed, VSpeedReset);
            }


            return (NewX, NewY, VSpeedReset, DJumpRefresh, false);
        }

        public static bool IsAlive(CollisionMap CollisionMap, PlayerNode node)
        {
            int yRounded = node.State.RoundedY;
            CollisionType ctype = CollisionMap.GetHighestPriorityCollisionType(node.State.X, yRounded);
            bool inbounds = node.State.X is >= 0 and <= 799 & yRounded is >= 0 and <= 607;
            return ctype != CollisionType.Killer & inbounds;
        }
    }
}
