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
            Player left = p.moveLeft(new_y);
            Player right = p.moveRight(new_y);
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
            return p.position.x == goal.x && ((int)Math.Round(p.position.y)) == goal.y;
        }

        public bool Run()
        {
            List<VPlayer> vstrings = VPlayer.GenerateVStrings(start.y, true, goal.y);

            // clean up in case this gets run multiple times
            // too lazy to do this properly rn
            players.Clear();
            covered.Clear();
            currentFrame = 0;

            return vstrings.Exists(RunVString);
        }

        bool RunVString(VPlayer vs)
        {
            players.Add(new Player(start.x, start.y));
            covered.Add(start.x);

            // doing it like this storing the y position in the player is kinda redundant
            // might aswell only check using player x pos and vstring y
            while (currentFrame < vs.VString.Count)
            {
                int numPlayers = players.Count;
                for (int i = 0; i < numPlayers; i++)
                {
                    Move(players[i], vs.VString[currentFrame]);
                    if (reachedGoal(players[i]))
                    {
                        v_string = vs.VString;
                        return true;
                    }
                }
                currentFrame++;
            }
            players.Clear();
            covered.Clear();
            currentFrame = 0;

            return false;
        }




    }
}
