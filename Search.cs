using Priority_Queue;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Jump_Bruteforcer
{
    public class Search : INotifyPropertyChanged
    {
        private readonly SortedSet<int> covered;
        private readonly List<Player> players;
        public (int x, double y) start;
        private (int x, int y) goal;
        private string _strat = "";
        private Dictionary<(int, int), CollisionType> _collisionMap = new();
        public int CurrentFrame { get; set; }
        public List<double> v_string = new();
        private PointCollection playerPath = new();
        public PointCollection PlayerPath { get { return playerPath; } set { playerPath = value; OnPropertyChanged(); } }
        public int StartX { get { return start.x; } set { start.x = value; OnPropertyChanged(); } }
        public double StartY { get { return start.y; } set { start.y = value; OnPropertyChanged(); } }
        public int GoalX { get { return goal.x; } set { goal.x = value; OnPropertyChanged(); } }
        public int GoalY { get { return goal.y; } set { goal.y = value; OnPropertyChanged(); } }
        public string Strat { get { return _strat; } set { _strat = value; OnPropertyChanged(); } }
        public Dictionary<(int, int), CollisionType> CollisionMap { get { return _collisionMap; } set { _collisionMap = value; } }
        public event PropertyChangedEventHandler PropertyChanged;


        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")  
        {  
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }  


        public Search((int, double) start, (int, int) goal)
        {
            covered = new SortedSet<int>();
            players = new List<Player>();
            this.start = start;
            this.goal = goal;
            CurrentFrame = 0;
        }
        public static float Distance(PlayerNode n, (int x, int y) goal)
        {
            return (float)Math.Max(Math.Abs(n.State.X - goal.x) / 3, Math.Abs(n.State.Y - goal.y) / 8.1);
        }



        public void RunBFS()
        {
            PlayerNode root = new PlayerNode(start.x, start.y, 0);

            Queue<PlayerNode> Q = new Queue<PlayerNode>();
            HashSet<PlayerNode> visited = new HashSet<PlayerNode>() { root };
            Q.Enqueue(root);
            while (Q.Count > 0)
            {
                PlayerNode v = Q.Dequeue();
                if (v.IsGoal(goal))
                {

                    (List<Input> inputs, PointCollection points) = v.GetPath();
                    Strat = PlayerNode.GetInputString(inputs);
                    PlayerPath = points;
                    
                    return;
                }
                foreach(PlayerNode w in v.GetNeighbors(CollisionMap))
                {
                    if (!visited.Contains(w))
                    {
                        visited.Add(w);
                        Q.Enqueue(w);
                    }
                }
                
            }



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

        public SearchResult Run()
        {
            List<VPlayer> vstrings = VPlayer.GenerateVStrings(start.y, true, goal.y);

            var solution = (from vs in vstrings
                         select RunVString(vs) into s
                         where s.Success
                         select s).FirstOrDefault() ?? new SearchResult();

                        
            Reset();
            return solution;
        }

        private SearchResult RunVString(VPlayer vs)
        {
 
            players.Add(new Player(start.x));
            covered.Add(start.x);
            

            while (CurrentFrame < vs.VString.Count)
            {
                int numPlayers = players.Count;

                // check if any of the resulting positions are the same as the goal
                if (Math.Round(vs.VString[CurrentFrame]) == goal.y)
                {
                    Player? p = players.Find(p => p.X_position == goal.x);
                    if (p != null)
                    {
                        p.MergeVStringInputs(vs.InputHistory, CurrentFrame);
                        v_string = vs.VString;
                        PlayerPath = p.GetTrajectory(v_string);
                        Strat = p.GetInputString();
                        return new SearchResult(p.GetInputString(), true);
                    }
                    
                }

                
                // perform horizontal movement
                for (int i = 0; i < numPlayers; i++)
                {
                    Move(players[i]);
                }
                CurrentFrame++;


            }
            Reset();
            return new SearchResult();
        }
    }

    public class SearchResult
    {
        public string InputString { get; } = string.Empty;
        public bool Success { get; }
        

        public SearchResult(string inputString, bool success)
        {
            InputString = inputString;
            Success = success;
        }

        public SearchResult()
        {
        }
    }
}
