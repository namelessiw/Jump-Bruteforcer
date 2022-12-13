using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump_Bruteforcer
{
    [Flags]
    enum Input
    { 
        Neutral = 1,
        Left = 2,
        Right = 4,
        Jump = 8,
        Release = 16
    }
    class Player
    {

        public int x_position { get; set; }


        public Player(int x)
        {
            x_position = x;

        }
        

        public Player moveLeft()
        {
            return new Player(x_position - 3);
        }

        public Player moveRight()
        {
            return new Player(x_position + 3);
        }


    }
}
