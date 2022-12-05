using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump_Bruteforcer
{
    enum Input
    { 
        None,
        Left,
        Right
    }
    class Player
    {
            VSpeed = SJump ? SJUMP_VSPEED : DJUMP_VSPEED;
        }

        public (int x, float y) position { get; }
        private Input lastInput;

        public Player(int x, float y, Input lastInput) {
            position = (x, y);
            this.lastInput = lastInput;
        }

            VSpeed += GRAVITY;

            // collision checks here
            Y += VSpeed;

            Frame++;
        }
    }
}
