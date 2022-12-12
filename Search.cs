using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace Jump_Bruteforcer
{
    public class Search
    {
        private SortedSet<int> covered;
        private List<Player> players;
        private (int x, double y) start;
        private (int x, int y) goal;
        public int currentFrame { get; set; }
        public List<double> v_string = new();
        public Search((int, double) start, (int, int) goal) {
            covered= new SortedSet<int>();
            players = new List<Player>();
            this.start = start;
            this.goal = goal;
            currentFrame= 0;
        }

        /// <summary>
        /// This method explores the movement space of each player: left, right, and none
        /// </summary>
        private void Move(Player p, double new_y)
        {
            Player left = p.moveLeft();
            Player right = p.moveRight();
            if (!covered.Contains(left.position.x))
            {
                covered.Add(left.position.x);
                players.Add(left);
            }
            if (!covered.Contains(right.position.x))
            {
                covered.Add(right.position.x);
                players.Add(right);
            }
            p.position = (p.position.x, new_y);
        }

        /// <summary>
        /// determines if the player 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool reachedGoal(Player p)
        {
            return p.position.x == goal.x && ((int)p.position.y) == goal.y;
        }
        public bool Run()
        {
            List<VPlayer> vstrings = VPlayer.GenerateVStrings(start.y, true, goal.y);

            foreach (VPlayer vs in vstrings)
            {
                
                players.Add(new Player(start.x, start.y));
                covered.Add(start.x);

                while (currentFrame < 30)
                {
                    currentFrame++;
                    int numPlayers = players.Count;
                    for (int i = 0; i < numPlayers; i++)
                    {
                        if (currentFrame < vs.VString.Count())
                        {
                            Move(players[i], vs.VString[currentFrame]);
                            if (reachedGoal(players[i]))
                            {
                                v_string = vs.VString;
                                return true;
                            }
                        }

                    }
                }
                players.Clear(); covered.Clear();
                currentFrame = 0;
                /*
                if (vs.VString.Count > 0) { 
                    v_string = vs.VString;
                }*/
                v_string = vs.VString;
            }
            return false;
        }




    }
}
