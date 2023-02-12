using Priority_Queue;
using System.ComponentModel;
using System.Text.Json;
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
        public int Distance(PlayerNode n, (int x, int y) goal)
        {
            return (int)(AStarWeight * Math.Ceiling((Math.Max(Math.Abs(n.State.X - goal.x) / 3, Math.Abs(n.State.Y - goal.y) / 9.4))));
        }


        public SearchResult RunAStar()
        {
            Dictionary<PlayerNode, long> nodeTime = new();
            PlayerNode root = new PlayerNode(start.x, start.y, 0);
            root.PathCost = 0;

            var openSet = new SimplePriorityQueue<PlayerNode, int>();
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
                    VisualizeSearch.CountStates(openSet, closedSet);

                    return new SearchResult(Strat, true, closedSet.Count);
                }
                closedSet.Add(v);
                nodeTime.Remove(v);
                foreach (PlayerNode w in v.GetNeighbors(CollisionMap))
                {
                    if (closedSet.Contains(w))
                    {
                        continue;
                    }
                    int newCost = v.PathCost + 1;
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
            VisualizeSearch.CountStates(openSet, closedSet);
            return new SearchResult(Strat, false, closedSet.Count);
        }
    }
    public class SearchResult
    {
        public string InputString { get; } = string.Empty;
        public bool Success { get; }
        public int Visited { get; }

        public SearchResult(string inputString, bool success, int visited) => (InputString, Success, Visited) = (inputString, success, visited);
        public override string ToString() => JsonSerializer.Serialize(this);     
    }
}
