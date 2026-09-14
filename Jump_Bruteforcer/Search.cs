using Priority_Queue;
using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace Jump_Bruteforcer
{
    public readonly record struct SearchNode(State State, int NodeIndex, uint PathCost)
    {
        public bool IsGoal((int x, int y) goal) => Math.Abs(State.X - goal.x) <= 1 & State.RoundedY == goal.y;
    }

    public class Search : INotifyPropertyChanged
    {
        private (int x, double y) start;
        private (int x, int y) goal;
        private string _strat = "";
        private CollisionMap _collisionMap = new(new Dictionary<(int, int), CollisionType>(), null);
        private PointCollection playerPath = new();
        private double startingVSpeed = 0;
        private String nodesVisited = "";
        private String timeTaken = "";
        private String macro = "";
        public PointCollection PlayerPath { get { return playerPath; } set { playerPath = value; OnPropertyChanged(); } }
        public int StartX { get { return start.x; } set { start.x = value; OnPropertyChanged(); } }
        public double StartY { get { return start.y; } set { start.y = value; OnPropertyChanged(); } }
        public int GoalX { get { return goal.x; } set { goal.x = Math.Clamp(value, 0, Map.WIDTH - 1); OnPropertyChanged(); } }
        public int GoalY { get { return goal.y; } set { goal.y = Math.Clamp(value, 0, Map.HEIGHT - 1); OnPropertyChanged(); } }
        public string Strat { get { return _strat; } set { _strat = value; OnPropertyChanged(); } }
        public String NodesVisited { get { return nodesVisited; } set { nodesVisited = value; OnPropertyChanged(); } }
        public CollisionMap CollisionMap { get { return _collisionMap; } set { _collisionMap = value; } }
        public double StartingVSpeed { get { return startingVSpeed; } set { startingVSpeed = value; OnPropertyChanged(); } }
        public String TimeTaken { get { return timeTaken; } set { timeTaken = value; OnPropertyChanged(); } }
        public String Macro { get { return macro; } set { macro = value; } }
        // Numeric timings exclude map loading and result rendering/export.
        public TimeSpan FloodFillElapsed { get; private set; }
        public TimeSpan SearchElapsed { get; private set; }
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
        public uint Distance(PlayerNode n)
        {
            return GoalDistance[n.State.X, (int)Math.Round(n.State.Y)];
        }

        public readonly uint[,] GoalDistance = new uint[Map.WIDTH, Map.HEIGHT];

        public void FloodFill()
        {
            HashSet<(int, int)> CurrentGoalPixels;
            if (goal.x + 1 == Map.WIDTH)
            {
                CurrentGoalPixels = new() { { goal }, { (goal.x - 2, goal.y) }, { (goal.x - 1, goal.y) } };
            }
            else if (goal.x - 1 < 0)
            {
                CurrentGoalPixels = new() { { goal }, { (goal.x + 2, goal.y) }, { (goal.x + 1, goal.y) } };
            }
            else
            {
                CurrentGoalPixels = new() { { goal }, { (goal.x + 1, goal.y) }, { (goal.x - 1, goal.y) } };
            }

            for (int X = 0; X < Map.WIDTH; X++)
            {
                for (int Y = 0; Y < Map.HEIGHT; Y++)
                {
                    GoalDistance[X, Y] = uint.MaxValue;
                }
            }

            HashSet<(int X, int Y)> NewPositions = new(), Temp;

            foreach ((int X, int Y) GoalPos in CollisionMap.goalPixels.Union(CurrentGoalPixels))
            {
                GoalDistance[GoalPos.X, GoalPos.Y] = 0;
                NewPositions.Add(GoalPos);
            }

            int MaxHSpeed = PhysicsParams.WALKING_SPEED, MaxVSpeedDown = (int)Math.Ceiling(PhysicsParams.MAX_VSPEED + PhysicsParams.GRAVITY), MaxVSpeedUp = (int)Math.Abs(Math.Ceiling(PhysicsParams.SJUMP_VSPEED + PhysicsParams.GRAVITY));
            uint Distance = 1;

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
                            if (GoalDistance[X, Y] == uint.MaxValue && !(CollisionMap.Collision[X, Y].HasFlag(CollisionType.Killer) || CollisionMap.Collision[X, Y].HasFlag(CollisionType.Solid)))
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
            var startTime = Stopwatch.GetTimestamp();
            FloodFillElapsed = TimeSpan.Zero;
            SearchElapsed = TimeSpan.Zero;
            FloodFill();
            FloodFillElapsed = Stopwatch.GetElapsedTime(startTime);
            var searchStartTime = Stopwatch.GetTimestamp();

            PlayerNode root = new PlayerNode(start.x, start.y, startingVSpeed);

            root.PathCost = 0;
            int nodesVisited;
            uint timestamp = uint.MaxValue;

            uint rootDistance = Distance(root);
            var openSet = new PriorityQueue<SearchNode, ulong>();
            openSet.Enqueue(new SearchNode(root.State, root.NodeIndex, root.PathCost), Priority(rootDistance, timestamp));

            var nodeParentIndices = new List<int>();
            var nodeInputs = new List<Input>();
            var visitedStateKeys = new HashSet<ulong>();
            var neighborCandidates = new NeighborCandidate[PlayerNode.MaxNeighborCount];
            int[,] closedStates = new int[Map.WIDTH, Map.HEIGHT];
            if (rootDistance != uint.MaxValue)
            {
                bool rootVisited = false;
                while (openSet.Count > 0)
                {
                    SearchNode v = openSet.Dequeue();
                    if (v.IsGoal(goal) || CollisionMap.onWarp(v.State.X, v.State.Y))
                    {
                        SearchElapsed = Stopwatch.GetElapsedTime(searchStartTime);
                        (List<Input> inputs, PointCollection points) = SearchOutput.GetPath(root ,v.NodeIndex, nodeParentIndices, nodeInputs, CollisionMap);
                        TimeTaken = Stopwatch.GetElapsedTime(startTime).ToString(@"dd\:hh\:mm\:ss\.ff");
                        Macro = SearchOutput.GetMacro(inputs);
                        Strat = $"Frames: {inputs.Count}\n\nVertical inputs:\n{SearchOutput.GetVerticalInputString(inputs, true)}\n\nHorizontal inputs:\n{SearchOutput.GetHorizontalInputString(inputs)}\n\nInputs per frame:\n{SearchOutput.GetInputString(inputs)}";
                        PlayerPath = points;

                        var optimalGoal = points.Last();
                        (GoalX, GoalY) = ((int)Math.Round(optimalGoal.X), (int)Math.Round(optimalGoal.Y));
                        VisualizeSearch.CountStates(openSet, closedStates);
                        VisualizeSearch.HeuristicMap(GoalDistance);
                        nodesVisited = visitedStateKeys.Count;
                        NodesVisited = nodesVisited.ToString();

                        return new SearchResult(Strat, macro, true, nodesVisited);
                    }
                    // Keep the original goal-at-root visited count while ensuring
                    // the root is present before any of its neighbors are checked.
                    if (!rootVisited)
                    {
                        visitedStateKeys.Add(PlayerNode.StateKey(v.State));
                        rootVisited = true;
                    }

                    int neighborCount = PlayerNode.GetNeighborCandidates(v.State, CollisionMap, neighborCandidates);
                    for (int i = 0; i < neighborCount; i++)
                    {
                        NeighborCandidate candidate = neighborCandidates[i];
                        // A state is marked discovered when it is first enqueued.
                        // Consequently the old openSet.Contains/UpdatePriority
                        // branch could never be reached for an equal state.
                        if (!visitedStateKeys.Add(candidate.Key))
                        {
                            continue;
                        }

                        uint newCost = v.PathCost + 1;
                        int roundedY = candidate.State.RoundedY;
                        closedStates[candidate.State.X, roundedY] += 1;
                        uint distance = GoalDistance[candidate.State.X, roundedY];
                        int nodeIndex = nodeInputs.Count;
                        nodeInputs.Add(candidate.Input);
                        nodeParentIndices.Add(v.NodeIndex);
                        SearchNode w = new(candidate.State, nodeIndex, newCost);
                        openSet.Enqueue(w, Priority(newCost + distance, --timestamp));
                    }

                }
            }

            
            SearchElapsed = Stopwatch.GetElapsedTime(searchStartTime);
            Strat = "SEARCH FAILURE";
            VisualizeSearch.CountStates(openSet, closedStates);
            VisualizeSearch.HeuristicMap(GoalDistance);
            nodesVisited = visitedStateKeys.Count;
            NodesVisited = nodesVisited.ToString();
            TimeTaken = Stopwatch.GetElapsedTime(startTime).ToString(@"hh\:mm\:ss\.ff");
            return new SearchResult(Strat, "", false, nodesVisited);
        }

        private static ulong Priority(uint cost, uint timestamp) => ((ulong)cost << 32) | timestamp;
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
