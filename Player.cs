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
        Up = 4,
        Down = 8
    }

    public static class Player
    {


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
            return CollisionMap.GetCollisionType(x, y) == type;
        }

        


        
    

        private static (int NewX, double NewY, bool VSpeedReset, bool DJumpRefresh, bool OnPlatform) SolidCollision(CollisionMap CollisionMap, int CurrentX, int NewX, double CurrentY, double NewY)
        {
            int CurrentYRounded = (int)Math.Round(CurrentY);
            int NewYRounded = (int)Math.Round(NewY);
            bool VSpeedReset = false;
            bool DJumpRefresh = false;
            double VSpeed = NewY - CurrentY;

            if (CollisionMap.GetCollisionType(NewX, CurrentYRounded) == CollisionType.Solid)
            {
                int sign = Math.Sign(NewX - CurrentX);
                if (sign != 0)
                {
                    while (CollisionMap.GetCollisionType(CurrentX + sign, CurrentYRounded) != CollisionType.Solid)
                    {
                        CurrentX += sign;
                    }
                }
                NewX = CurrentX;

            }
            if (CollisionMap.GetCollisionType(CurrentX, NewYRounded) == CollisionType.Solid)
            {
                // (re)rounding everytime because otherwise vfpi would lose its parity
                
                int sign = Math.Sign(VSpeed);
                if (sign != 0)
                {
                    if (VSpeed > 0)
                    {
                        DJumpRefresh= true;
                    }
                    int yRounded = (int)Math.Round(CurrentY + sign);
                    while (Math.Abs(VSpeed) >= 1 && (CollisionMap.GetCollisionType(CurrentX, yRounded) != CollisionType.Solid))
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

            if (CollisionMap.GetCollisionType(NewX, NewYRounded) == CollisionType.Solid)
            {
                NewX = CurrentX;
            }

            return (NewX, NewY, VSpeedReset, DJumpRefresh, false);
        }

        public static bool IsAlive(CollisionMap CollisionMap, PlayerNode node)
        {
            int yRounded = node.State.RoundedY;
            CollisionType ctype = CollisionMap.GetCollisionType(node.State.RoundedX, yRounded);
            bool inbounds =  node.State.X is >= 0 and <= 799 & yRounded is >= 0 and <= 607;
            return ctype != CollisionType.Killer & inbounds;
        }
    }
}
