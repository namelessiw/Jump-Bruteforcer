using Priority_Queue;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Jump_Bruteforcer
{
    public class Search : INotifyPropertyChanged
    {
        private (int x, double y) start;
        private (int x, int y) goal;
        private string _strat = "";
        private CollisionMap _collisionMap = new(new Dictionary<(int, int), ImmutableSortedSet<CollisionType>>(), null);
        private double _aStarWeight = 1.0;
        private PointCollection playerPath = new();
        private double startingVSpeed = 0;
        public PointCollection PlayerPath { get { return playerPath; } set { playerPath = value; OnPropertyChanged(); } }
        public int StartX { get { return start.x; } set { start.x = value; OnPropertyChanged(); } }
        public double StartY { get { return start.y; } set { start.y = value; OnPropertyChanged(); } }
        public int GoalX { get { return goal.x; } set { goal.x = value; OnPropertyChanged(); } }
        public int GoalY { get { return goal.y; } set { goal.y = value; OnPropertyChanged(); } }
        public string Strat { get { return _strat; } set { _strat = value; OnPropertyChanged(); } }
        public double AStarWeight { get { return _aStarWeight; } set { _aStarWeight = value; OnPropertyChanged(); } }
        public CollisionMap CollisionMap { get { return _collisionMap; } set { _collisionMap = value; } }
        public double StartingVSpeed { get { return startingVSpeed; } set { startingVSpeed = value; OnPropertyChanged(); } }
        public event PropertyChangedEventHandler? PropertyChanged;


        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Search((int, double) start, (int, int) goal, CollisionMap collision)
        {
            this.start = start;
            this.goal = goal;
            CollisionMap = collision;
        }


        //inadmissable heuristic because of y position rounding
        public int Distance(PlayerNode n)
        {
            int Distance = GoalDistance[n.State.X, (int)Math.Round(n.State.Y)];
            if (Distance < -1)
            {
                throw new Exception($"no known floodfill distance for ({n.State.X} ,{(int)Math.Round(n.State.Y)})");
            }
            return Distance;
        }

        public readonly int[,] GoalDistance = new int[Map.WIDTH, Map.HEIGHT];

        public void FloodFill()
        {
            CollisionMap.goalPixels.Add(goal);
            CollisionMap.goalPixels.Add((goal.x - 1, goal.y));
            CollisionMap.goalPixels.Add((goal.x + 1, goal.y));


            for (int X = 0; X < Map.WIDTH; X++)
            {
                for (int Y = 0; Y < Map.HEIGHT; Y++)
                {
                    GoalDistance[X, Y] = -1;
                }
            }

            HashSet<(int X, int Y)> NewPositions = new(), Temp;

            foreach ((int X, int Y) GoalPos in CollisionMap.goalPixels)
            {
                GoalDistance[GoalPos.X, GoalPos.Y] = 0;
                NewPositions.Add(GoalPos);
            }

            int MaxHSpeed = PhysicsParams.WALKING_SPEED, MaxVSpeedDown = (int)Math.Ceiling(PhysicsParams.MAX_VSPEED + PhysicsParams.GRAVITY), MaxVSpeedUp = (int)Math.Abs(Math.Ceiling(PhysicsParams.SJUMP_VSPEED + PhysicsParams.GRAVITY));
            int Distance = 1;

            while (NewPositions.Count > 0)
            {
                Temp = new HashSet<(int X, int Y)>(NewPositions);
                NewPositions.Clear();

                // floodfill
                foreach ((int X, int Y) Pos in Temp)
                {
                    int MinX = Math.Max(Pos.X - MaxHSpeed, 0),
                        MaxX = Math.Min(Pos.X + MaxHSpeed, Map.WIDTH - 1),
                        MinY = Math.Max(Pos.Y - MaxVSpeedDown, 0),
                        MaxY = Math.Min(Pos.Y + MaxVSpeedUp, Map.HEIGHT - 1);

                    for (int X = MinX; X <= MaxX; X++)
                    {
                        for (int Y = MinY; Y <= MaxY; Y++)
                        {
                            if (GoalDistance[X, Y] == -1 && !(CollisionMap.Collision[X, Y].Contains(CollisionType.Killer) || CollisionMap.Collision[X, Y].Contains(CollisionType.Solid)))
                            {
                                GoalDistance[X, Y] = Distance;
                                NewPositions.Add((X, Y));
                            }
                        }
                    }
                }

                Distance++;
            }
        }

        public SearchResult RunAStar()
        {
            FloodFill();

            PlayerNode root = new PlayerNode(start.x, start.y, startingVSpeed);

            root.PathCost = 0;
            int nodesVisited = 0;

            var openSet = new SimplePriorityQueue<PlayerNode, uint>();
            openSet.Enqueue(root, (uint)Distance(root));

            var closedSet = new HashSet<PlayerNode>();

            if (Distance(root) == -1)
            {
                Strat = "SEARCH FAILURE";
                VisualizeSearch.CountStates(openSet, closedSet);
                nodesVisited = closedSet.Count;
                return new SearchResult(Strat, "", false, nodesVisited);
            }

            while (openSet.Count > 0)
            {
                PlayerNode v = openSet.Dequeue();
                if (v.IsGoal(goal) || CollisionMap.onWarp(v.State.X, v.State.Y))
                {

                    (List<Input> inputs, PointCollection points) = v.GetPath();
                    Strat = SearchOutput.GetInputString(inputs);
                    PlayerPath = points;
                    SearchOutput.DumpPath(v);
                    var optimalGoal = points.Last();
                    (GoalX, GoalY) = ((int)Math.Round(optimalGoal.X), (int)Math.Round(optimalGoal.Y)); 
                    VisualizeSearch.CountStates(openSet, closedSet);
                    nodesVisited = closedSet.Count;

                    string Macro = SearchOutput.GetMacro(inputs);

                    return new SearchResult(Strat, Macro, true, nodesVisited);
                }
                closedSet.Add(v);
                foreach (PlayerNode w in v.GetNeighbors(CollisionMap))
                {
                    if (closedSet.Contains(w))
                    {
                        continue;
                    }
                    uint newCost = v.PathCost + 1;
                    if (!openSet.Contains(w) || newCost < w.PathCost)
                    {
                        w.Parent = v;
                        w.PathCost = newCost;
                        uint distance = (uint)Distance(w);
                        if (openSet.Contains(w))
                        {
                            openSet.UpdatePriority(w, newCost + distance);
                        }
                        else
                        {
                            openSet.Enqueue(w, newCost + distance);
                        }
                    }

                }

            }
            Strat = "SEARCH FAILURE";
            VisualizeSearch.CountStates(openSet, closedSet);
            nodesVisited = closedSet.Count;
            return new SearchResult(Strat, "", false, nodesVisited);
        }
    }
    public class SearchResult
    {
        public string InputString { get; } = string.Empty;
        public string Macro { get; } = string.Empty;
        public bool Success { get; }
        public int Visited { get; }

        public SearchResult(string inputString, string macro, bool success, int visited) => (InputString, Macro, Success, Visited) = (inputString, macro, success, visited);
        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
