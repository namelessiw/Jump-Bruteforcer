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

        public (int x, float y) position { get; set; }


        public Player(int x, float y)
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

        public void moveUp()
        {
            position = (position.x, position.y + 1);
        }

    }
}
