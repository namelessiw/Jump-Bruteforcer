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
    public class Search : INotifyPropertyChanged
    {
        private (int x, double y) start;
        private (int x, int y) goal;
        private bool startScraper;
        private bool startfacingRight;
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
        public bool StartScraper { get { return startScraper; } set { startScraper = value; OnPropertyChanged(); } }
        public bool StartFacingRight { get { return startfacingRight; } set { startfacingRight = value; OnPropertyChanged(); } }
        public string Strat { get { return _strat; } set { _strat = value; OnPropertyChanged(); } }
        public String NodesVisited { get { return nodesVisited; } set { nodesVisited = value; OnPropertyChanged(); } }
        public CollisionMap CollisionMap { get { return _collisionMap; } set { _collisionMap = value; } }
        public double StartingVSpeed { get { return startingVSpeed; } set { startingVSpeed = value; OnPropertyChanged(); } }
        public String TimeTaken { get { return timeTaken; } set { timeTaken = value; OnPropertyChanged(); } }
        public String Macro { get { return macro; } set { macro = value; } }
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
            if (goal.x + 1 == Map.WIDTH)
            {
                var CurrentGoalPixels = new HashSet<(int, int)>() { { goal }, { (goal.x - 2, goal.y) }, { (goal.x - 1, goal.y) } };
            }else if (goal.x - 1 < 0)
            {
                var CurrentGoalPixels = new HashSet<(int, int)>() { { goal }, { (goal.x + 2, goal.y) }, { (goal.x + 1, goal.y) } };
            }else
            {
                var CurrentGoalPixels = new HashSet<(int, int)>() { { goal }, { (goal.x + 1, goal.y) }, { (goal.x - 1, goal.y) } };
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
            FloodFill();
            Bools flags = Bools.None/* | Bools.CanDJump*/;
            flags |= StartFacingRight ? Bools.FacingRight : Bools.None;
            flags |= StartScraper ? Bools.FaceScraper : Bools.None;
            PlayerNode root = new PlayerNode(start.x, start.y, startingVSpeed, flags);

            root.PathCost = 0;
            int nodesVisited;
            uint timestamp = uint.MaxValue;

            var openSet = new SimplePriorityQueue<PlayerNode, (uint, uint)>();
            openSet.Enqueue(root, (Distance(root), timestamp));

            var nodeParentIndices = new List<int>();
            var nodeInputs = new List<Input>();
            var visitedNodeHashes = new HashSet<ulong>();
            int[,] closedStates = new int[Map.WIDTH, Map.HEIGHT];
            if (Distance(root) != uint.MaxValue)
            {
                while (openSet.Count > 0)
                {
                    PlayerNode v = openSet.Dequeue();
                    if (v.IsGoal(goal) || CollisionMap.onWarp(v.State.X, v.State.Y))
                    {
                        (List<Input> inputs, PointCollection points) = SearchOutput.GetPath(root ,v.NodeIndex, nodeParentIndices, nodeInputs, CollisionMap);
                        TimeTaken = Stopwatch.GetElapsedTime(startTime).ToString(@"dd\:hh\:mm\:ss\.ff");
                        Macro = SearchOutput.GetMacro(inputs, (root.State.Flags & Bools.FaceScraper) == Bools.FaceScraper);
                        Strat = SearchOutput.GetInputString(inputs);
                        PlayerPath = points;

                        var optimalGoal = points.Last();
                        (GoalX, GoalY) = ((int)Math.Round(optimalGoal.X), (int)Math.Round(optimalGoal.Y));
                        VisualizeSearch.CountStates(openSet, closedStates);
                        VisualizeSearch.HeuristicMap(GoalDistance);
                        nodesVisited = visitedNodeHashes.Count;
                        NodesVisited = nodesVisited.ToString();

                        return new SearchResult(Strat, macro, true, nodesVisited);
                    }
                    visitedNodeHashes.Add(v.Hash());
                    foreach ((PlayerNode w, Input input) in v.GetNeighbors(CollisionMap))
                    {
                        if (visitedNodeHashes.Contains(w.Hash()))
                        {
                            continue;
                        }

                        uint newCost = v.PathCost + (uint)((input & Input.Facescraper) == Input.Facescraper ? 48 : 1);
                        if (!openSet.Contains(w) || newCost < w.PathCost)
                        {
                            closedStates[w.State.X, w.State.RoundedY] += 1;
                            visitedNodeHashes.Add(w.Hash());
                            w.PathCost = newCost;
                            uint distance = (uint)Distance(w);
                            w.NodeIndex = nodeInputs.Count;
                            nodeInputs.Add(input);
                            nodeParentIndices.Add(v.NodeIndex);
                            if (openSet.Contains(w))
                            {
                                openSet.UpdatePriority(w, (newCost + distance, timestamp));
                            }
                            else
                            {
                                openSet.Enqueue(w, (newCost + distance, --timestamp));
                            }
                        }

                    }

                }
            }

            
            Strat = "SEARCH FAILURE";
            VisualizeSearch.CountStates(openSet, closedStates);
            VisualizeSearch.HeuristicMap(GoalDistance);
            nodesVisited = visitedNodeHashes.Count;
            NodesVisited = nodesVisited.ToString();
            TimeTaken = Stopwatch.GetElapsedTime(startTime).ToString(@"hh\:mm\:ss\.ff");
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
