using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump_Bruteforcer
{
    internal class Player
    {
        public const double GRAVITY = 0.4, RELEASE_MULTIPLIER = 0.45, MAX_VSPEED = 9, SJUMP_VSPEED = -8.5, DJUMP_VSPEED = -7;
        public double Y, VSpeed;
        public int Frame;

        public Player(double Y)
        {
            this.Y = Y;
            VSpeed = 0;
            Frame = 0;
        }

        public void Jump(bool SJump)
        {
            VSpeed = SJump ? SJUMP_VSPEED : DJUMP_VSPEED;
        }

        public void Advance(bool Release)
        {
            if (Release)
            {
                if (VSpeed < 0)
                {
                    VSpeed *= RELEASE_MULTIPLIER;
                }
            }

            // potential optimization: else if?
            if (VSpeed > MAX_VSPEED)
            {
                VSpeed = MAX_VSPEED;
            }

            VSpeed += GRAVITY;

            // collision checks here
            Y += VSpeed;

            Frame++;
        }
    }
}
