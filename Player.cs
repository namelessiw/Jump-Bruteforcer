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

        public (int x, double y) position { get; set; }


        public Player(int x, double y)
        {
            position = (x, y);

        }
        

        public Player moveLeft()
        {
            return new Player(position.x - 3, position.y + 1);
        }

        public Player moveRight()
        {
            return new Player(position.x + 3, position.y + 1);
        }


    }
}
