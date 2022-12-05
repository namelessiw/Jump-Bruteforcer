using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump_Bruteforcer
{
    internal class Search
    {
        private SortedSet<int> covered;
        private List<Player> players;
        private (int x, float y) start;
        private (int x, int y) goal;
        private int currentFrame;
        public Search((int, float) start, (int, int) goal) {
            covered= new SortedSet<int>();
            players = new List<Player>();
            this.start = start;
            this.goal = goal;
            currentFrame= 0;
        }

        /// <summary>
        /// This method explores the movement space of each player: left, right, and none
        /// </summary>
        private void move()
        {

        }
        public void search()
        {
            
            players.Add(new Player(start.x, start.y, Input.None));
            covered.Add(start.x);

            while (currentFrame < 30)
            {
                foreach (Player p in players)
                {
                    p.move();
                    currentFrame++;
                }
            }




        }

    }
}
