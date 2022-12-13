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
        private void Move(Player p)
        {
            Player left = p.moveLeft();
            Player right = p.moveRight();
            if (!covered.Contains(left.x_position))
            {
                covered.Add(left.x_position);
                players.Add(left);
            }
            if (!covered.Contains(right.x_position))
            {
                covered.Add(right.x_position);
                players.Add(right);
            }
        }

        /// <summary>
        /// resets visited locations and players so that search can be run again.
        /// </summary>
        private void reset()
        {
            players.Clear();
            covered.Clear();
            currentFrame = 0;
        }

        public bool Run()
        {
            List<VPlayer> vstrings = VPlayer.GenerateVStrings(start.y, true, goal.y);

            bool reachedGoal = vstrings.Exists(RunVString);
            reset();
            return reachedGoal;
        }

        bool RunVString(VPlayer vs)
        {
            players.Add(new Player(start.x));
            covered.Add(start.x);

            while (currentFrame < vs.VString.Count)
            {
                int numPlayers = players.Count;
                
                // perform horizontal movement
                for (int i = 0; i < numPlayers; i++)
                {
                    Move(players[i]);
                }

                // check if any of the resulting positions are the same as the goal
                if (Math.Round(vs.VString[currentFrame]) == goal.y)
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        // can be simplified to only check matching x
                        // since y has been checked outside of the loop
                        if (players[i].x_position == goal.x)
                        {
                            v_string = vs.VString;
                            return true;
                        }
                    }
                }
                currentFrame++;
            }
            return false;
        }
    }
}
