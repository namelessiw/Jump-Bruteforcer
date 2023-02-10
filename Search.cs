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
        public (int x, double y) start;
        private (int x, int y) goal;
        private string _strat = "";
        private Dictionary<(int, int), CollisionType> _collisionMap = new();
        private double _aStarWeight = 1.0;
        private PointCollection playerPath = new();
        public PointCollection PlayerPath { get { return playerPath; } set { playerPath = value; OnPropertyChanged(); } }
        public int StartX { get { return start.x; } set { start.x = value; OnPropertyChanged(); } }
        public double StartY { get { return start.y; } set { start.y = value; OnPropertyChanged(); } }
        public int GoalX { get { return goal.x; } set { goal.x = value; OnPropertyChanged(); } }
        public int GoalY { get { return goal.y; } set { goal.y = value; OnPropertyChanged(); } }
        public string Strat { get { return _strat; } set { _strat = value; OnPropertyChanged(); } }
        public double AStarWeight { get { return _aStarWeight; } set { _aStarWeight = value; OnPropertyChanged(); } }
        public Dictionary<(int, int), CollisionType> CollisionMap { get { return _collisionMap; } set { _collisionMap = value; } }
        public event PropertyChangedEventHandler PropertyChanged;


        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Search((int, double) start, (int, int) goal, Dictionary<(int, int), CollisionType> collision)
        {
            this.start = start;
            this.goal = goal;
            CollisionMap = collision;
        }
        //inadmissable heuristic because of y position rounding
        public float Distance(PlayerNode n, (int x, int y) goal)
        {
            return (float)(AStarWeight * Math.Ceiling((Math.Max(Math.Abs(n.State.X - goal.x) / 3, Math.Abs(n.State.Y - goal.y) / 9.4))));
        }
        public static float Distance(PlayerNode n1, PlayerNode n2)
        {
            return (float)Math.Ceiling(Math.Max(Math.Abs(n1.State.X - n2.State.X) / 3, Math.Abs(n1.State.Y - n2.State.Y) / 9.4));
        }

        // max per px
        // (x, y) -> (open, closed)

        public SearchResult RunAStar()
        {
            PlayerNode root = new PlayerNode(start.x, start.y, 0);
            root.PathCost = 0;
            var openSet = new SimplePriorityQueue<PlayerNode, float>();
            openSet.Enqueue(root, Distance(root, goal));
            var closedSet = new HashSet<PlayerNode>();

            while (openSet.Count > 0)
            {
                PlayerNode v = openSet.Dequeue();
                if (v.IsGoal(goal))
                {

                    (List<Input> inputs, PointCollection points) = v.GetPath();
                    Strat = PlayerNode.GetInputString(inputs);
                    PlayerPath = points;

                    // count states in open/closed set per pixel

                    VisualizeSearch.CountStates(openSet, closedSet);

                    return new SearchResult(Strat, true);
                }
                closedSet.Add(v);
                foreach (PlayerNode w in v.GetNeighbors(CollisionMap))
                {
                    if (closedSet.Contains(w))
                    {
                        continue;
                    }
                    float newCost = v.PathCost + Distance(v, w);
                    if (!openSet.Contains(w) || newCost < w.PathCost)
                    {
                        w.Parent = v;
                        w.PathCost = newCost;
                        if (openSet.Contains(w))
                        {
                            openSet.UpdatePriority(w, newCost + Distance(w, goal));
                        }
                        else
                        {
                            openSet.Enqueue(w, newCost + Distance(w, goal));
                        }
                    }

                }

            }
            Strat = "SEARCH FAILURE";
            return new SearchResult();
        }

        public SearchResult RunBFS()
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

                    return new SearchResult(Strat, true);
                }
                foreach (PlayerNode w in v.GetNeighbors(CollisionMap))
                {
                    if (!visited.Contains(w))
                    {
                        visited.Add(w);
                        Q.Enqueue(w);
                    }
                }

            }
            Strat = "SEARCH FAILURE";
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
