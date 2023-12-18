﻿using Priority_Queue;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Media;

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
        public uint Distance(PlayerNode n, (int x, int y) goal)
        {
            return (uint)(AStarWeight * Math.Ceiling(Math.Max(Math.Abs(n.State.X - goal.x) / PhysicsParams.WALKING_SPEED, Math.Abs(n.State.Y - goal.y) / (PhysicsParams.MAX_VSPEED + PhysicsParams.GRAVITY))));
        }


        public SearchResult RunAStar()
        {
            PlayerNode root = new PlayerNode(start.x, start.y, startingVSpeed);
            root.PathCost = 0;
            int nodesVisited = 0;

            var openSet = new SimplePriorityQueue<PlayerNode, uint>();
            openSet.Enqueue(root, Distance(root, goal));

            var closedSet = new HashSet<PlayerNode>();

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
                        uint distance = Distance(w, goal);
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
