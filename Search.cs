namespace Jump_Bruteforcer
{
    public class Search
    {
        private readonly SortedSet<int> covered;
        private readonly List<Player> players;
        private (int x, double y) start;
        private (int x, int y) goal;
        public int CurrentFrame { get; set; }
        public List<double> v_string = new();
        public Search((int, double) start, (int, int) goal)
        {
            covered = new SortedSet<int>();
            players = new List<Player>();
            this.start = start;
            this.goal = goal;
            CurrentFrame = 0;
        }

        /// <summary>
        /// This method explores the movement space of each player: left, right, and none
        /// </summary>
        private void Move(Player p)
        {
            Player left = p.MoveLeft(CurrentFrame);
            Player right = p.MoveRight(CurrentFrame);
            if (!covered.Contains(left.X_position))
            {
                covered.Add(left.X_position);
                players.Add(left);
            }
            if (!covered.Contains(right.X_position))
            {
                covered.Add(right.X_position);
                players.Add(right);
            }

            p.MoveNeutral(CurrentFrame);
        }

        /// <summary>
        /// resets visited locations and players so that search can be run again.
        /// </summary>
        private void Reset()
        {
            players.Clear();
            covered.Clear();
            CurrentFrame = 0;
        }

        public string Run()
        {
            List<VPlayer> vstrings = VPlayer.GenerateVStrings(start.y, true, goal.y);

            var solution = (from vs in vstrings
                         select RunVString(vs) into s
                         where !string.IsNullOrEmpty(s)
                         select s).FirstOrDefault();

                        
            Reset();
            return solution;
        }

        string RunVString(VPlayer vs)
        {
 
            players.Add(new Player(start.x));
            covered.Add(start.x);
            

            while (CurrentFrame < vs.VString.Count)
            {
                int numPlayers = players.Count;

                // perform horizontal movement
                for (int i = 0; i < numPlayers; i++)
                {
                    Move(players[i]);
                }


                // check if any of the resulting positions are the same as the goal
                if (Math.Round(vs.VString[CurrentFrame]) == goal.y)
                {
                    Player? p = players.Find(p => p.X_position == goal.x);
                    if (p != null)
                    {
                        p.MergeVStringInputs(vs.InputHistory, CurrentFrame);
                        v_string = vs.VString;
                        return p.GetInputString();
                    }
                    
                }
                CurrentFrame++;
            }
            Reset();
            return string.Empty;
        }
    }
}
