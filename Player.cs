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

        


        
    

        private static (double NewX, double NewY, bool VSpeedReset, bool HSpeedReset) SolidCollision(CollisionMap CollisionMap, double CurrentX, double NewX, double CurrentY, double NewY)
        {
            int CurrentYRounded = (int)Math.Round(CurrentY);
            int CurrentXRounded = (int)Math.Round(CurrentX);
            int NewYRounded = (int)Math.Round(NewY);
            int NewXRounded = (int)Math.Round(NewX);
            bool VSpeedReset = false;
            bool HSpeedReset = false;
            double VSpeed = NewY - CurrentY;
            double HSpeed = NewX - CurrentX;



            if (CollisionMap.GetCollisionType(NewXRounded, CurrentYRounded) == CollisionType.Solid)
            {
                int sign = Math.Sign(HSpeed);
                if (sign != 0)
                {
                    int xRounded = (int)Math.Round(CurrentX + sign);
                    while (Math.Abs(HSpeed) >= 1 && CollisionMap.GetCollisionType(xRounded, CurrentYRounded) != CollisionType.Solid)
                    {
                        CurrentX += sign;
                        HSpeed -= sign;
                        xRounded = (int)Math.Round(CurrentX + sign);
                    }
                }
                NewXRounded = (int)Math.Round(CurrentX);
                NewX = CurrentX;
                HSpeedReset = true;

            }
            if (CollisionMap.GetCollisionType(CurrentXRounded, NewYRounded) == CollisionType.Solid)
            {
                // (re)rounding everytime because otherwise vfpi would lose its parity
                
                int sign = Math.Sign(VSpeed);
                if (sign != 0)
                {
                    int yRounded = (int)Math.Round(CurrentY + sign);
                    while (Math.Abs(VSpeed) >= 1 && (CollisionMap.GetCollisionType(CurrentXRounded, yRounded) != CollisionType.Solid))
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

            if (CollisionMap.GetCollisionType(NewXRounded, NewYRounded) == CollisionType.Solid)
            {
                NewX = CurrentX;
                HSpeedReset = true;
            }

            return (NewX, NewY, VSpeedReset, HSpeedReset);
        }

        public static bool IsAlive(CollisionMap CollisionMap, PlayerNode node)
        {
            int yRounded = node.State.RoundedY;
            CollisionType ctype = CollisionMap.GetCollisionType(node.State.RoundedX, yRounded);
            bool inbounds =  node.State.RoundedX is >= 0 and <= 799 & yRounded is >= 0 and <= 607;
            return ctype != CollisionType.Killer & inbounds;
        }

        public static (double x, double y, double finalHspeed, double finalVSpeed) BubbleStep(Input input, double x, double y, double hSpeed, double vSpeed)
        {
            if (input.HasFlag(Input.Up))
            {
                vSpeed -= 0.1;
            }
            if (input.HasFlag(Input.Down))
            {
                vSpeed += 0.1;
            }
            if (input.HasFlag(Input.Left))
            {
                hSpeed -= 0.1;
            }
            if (input.HasFlag(Input.Right))
            {
                hSpeed += 0.1;
            }
            x += hSpeed;
            y += vSpeed;

            double angle = Math.Atan2(-vSpeed, hSpeed);
            double speed = Math.Sqrt(Math.Pow(vSpeed, 2) + Math.Pow(hSpeed, 2));
            if (speed > 0)
            {
                if (input == Input.Neutral)
                {
                    hSpeed -= Math.Sign(hSpeed) * Math.Cos(angle) * 0.05;
                    vSpeed -= Math.Sign(vSpeed) * Math.Sin(angle) * 0.05;
                }

            }
            else
            {
                hSpeed = vSpeed = 0;
            }

            return (x, y, hSpeed, vSpeed);
        }

        public static (double NewX, double NewY, bool vSpeedReset, bool hSpeedReset) CollisionCheck(CollisionMap collisionMap, double CurrentX, double NewX, double CurrentY, double NewY)
        {
            int RoundedNewY = (int)Math.Round(NewY);
            int RoundedNewX = (int)Math.Round(NewX);
            CollisionType ctype = collisionMap.GetCollisionType(RoundedNewX, RoundedNewY);
            if (ctype == CollisionType.Solid) 
            {
                return SolidCollision(collisionMap, CurrentX, NewX, CurrentY, NewY);
            }
            return (NewX, NewY, false, false);
        }
    }
}
