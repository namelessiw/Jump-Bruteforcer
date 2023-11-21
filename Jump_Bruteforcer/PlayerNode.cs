﻿using System.Windows.Media;
using System.Text.Json;
using System.Collections.Immutable;
using System.Windows;
using System.IO;

namespace Jump_Bruteforcer
{
    [Flags]
    public enum Bools
    {
        None = 0,
        CanDJump = 1,
        OnPlatform = 2,
        FacingRight = 4,
        FaceScraper = 8,
    }
    public class State : IEquatable<State>
    {

        public int X { get; init; }
        public double Y { get; init; }
        public double VSpeed { get; init; }
        public Bools Flags { get; init; }
        public int RoundedY { get { return (int)Math.Round(Y); } }
        const int epsilon = 10;

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ((State)obj).Equals(this);
        }

        private static double Quantize(double a)
        {
            return Math.Round(a * epsilon);
        }
        private static bool ApproximatelyEquals(double a, double b)
        {
            return Quantize(a) == Quantize(b);
        }
        public bool Equals(State? other) =>
            X == other.X & ApproximatelyEquals(Y, other.Y) &
            ApproximatelyEquals(VSpeed, other.VSpeed) & (Flags | Bools.FacingRight) == (other.Flags | Bools.FacingRight) & (Flags | Bools.FaceScraper) == (other.Flags | Bools.FaceScraper);
        public override int GetHashCode() => (X, Quantize(Y), Quantize(VSpeed), Flags | Bools.FacingRight, Bools.FaceScraper).GetHashCode();
        public override string ToString() => JsonSerializer.Serialize(this);

    }
    public class PlayerNode : IEquatable<PlayerNode>
    {
        public State State { get; set; }
        public PlayerNode? Parent { get; set; }
        public uint PathCost { get; set; }
        public Input? Action { get; set; }
        public static readonly ImmutableArray<Input> inputs = ImmutableArray.Create(Input.Neutral, Input.Left, Input.Right, Input.Facescraper);
        public static readonly ImmutableArray<Input> inputsJump = ImmutableArray.Create(Input.Jump, Input.Left | Input.Jump, Input.Right | Input.Jump, Input.Jump | Input.Release, Input.Left | Input.Jump | Input.Release, Input.Right | Input.Jump | Input.Release);
        public static readonly ImmutableArray<Input> inputsRelease = ImmutableArray.Create(Input.Release, Input.Left | Input.Release, Input.Right | Input.Release);
        private static readonly ImmutableArray<CollisionType> jumpables = ImmutableArray.Create(CollisionType.Solid, CollisionType.Platform, CollisionType.Water1, CollisionType.Water2, CollisionType.Water3);
        public PlayerNode(int x, double y, double vSpeed, Bools flags = Bools.CanDJump | Bools.FacingRight, Input? action = null, uint pathCost = uint.MaxValue, PlayerNode? parent = null) =>
            (State, Parent, PathCost, Action) = (new State() { X = x, Y = y, VSpeed = vSpeed, Flags = flags }, parent, pathCost, action);

        public PlayerNode(State state, PlayerNode? parent, uint pathCost, Input? action)
        {
            State = state;
            Parent = parent;
            PathCost = pathCost;
            Action = action;
        }

        public bool IsGoal((int x, int y) goal) => Math.Abs(State.X - goal.x) <= 1 & State.RoundedY == goal.y;






        /// <summary>
        /// For a given PlayerNode, returns the inputs to get there and the path taken through the game space
        /// </summary>
        /// <returns>a tuple containing the list of inputs and a PointCollection representing the path</returns>
        public (List<Input> Inputs, PointCollection Points) GetPath()
        {
            List<Input> inputs = new List<Input>();
            List<Point> points = new List<Point>();
            PlayerNode? currentNode = this;

            while (currentNode != null)
            {
                if (currentNode.Action != null)
                {
                    inputs.Add((Input)currentNode.Action);
                }
                points.Add(new Point(currentNode.State.X, currentNode.State.RoundedY));
                currentNode = currentNode.Parent;
            }
            inputs.Reverse();
            points.Reverse();

            return (inputs, new PointCollection(points));
        }

        /// <summary>
        /// creates the set of all unique states that can be reached in one frame from the current state with arbitrary inputs.
        /// states with fewer inputs are favored if two states are the same. States inside playerkillers are excluded.
        /// </summary>
        /// <returns>a Hashset of playerNodes</returns>
        public HashSet<PlayerNode> GetNeighbors(CollisionMap CollisionMap)
        {
            var neighbors = new HashSet<PlayerNode>();
            fillNeighbors(CollisionMap, neighbors, inputs);

            if (State.VSpeed < 0)
            {
                fillNeighbors(CollisionMap, neighbors, inputsRelease);
            }
            if (CollisionMap.GetCollisionTypes(State.X, (int)Math.Round(State.Y + 4)).Contains(CollisionType.Platform) || ((State.Flags & Bools.CanDJump) == Bools.CanDJump) || CollisionMap.GetCollisionTypes(State.X, (int)Math.Round(State.Y + 1)).Overlaps(jumpables))
            {
                fillNeighbors(CollisionMap, neighbors, inputsJump);
            }

            return neighbors;

            void fillNeighbors(CollisionMap CollisionMap, HashSet<PlayerNode> neighbors, ImmutableArray<Input> inputs)
            {
                foreach (var neighbor in from Input input in inputs
                                         let neighbor = NewState(input, CollisionMap)
                                         where Player.IsAlive(CollisionMap, neighbor)
                                         select neighbor)
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        /// <summary>
        /// Creates the next state in the tree after applying inputs to the game starting from the current state
        /// </summary>
        /// <param name="input"></param> the inputs for the next frame
        /// <param name="CollisionMap"></param> the game field
        /// <returns>A new PlayerNode that results from running inputs on the collision map</returns>
        public PlayerNode NewState(Input input, CollisionMap CollisionMap)
        {

            State newState = Player.Update(this, input, CollisionMap);
            uint newStateCost = (uint)((input & Input.Facescraper) == Input.Facescraper ? 48 : 1); 

            return new PlayerNode(newState, action: input, pathCost: PathCost + newStateCost, parent: this);
        }

        public bool Equals(PlayerNode? other)
        {
            if (other is null)
            {
                return false;
            }

            return State.Equals(other.State);
        }

        public override int GetHashCode() => State.GetHashCode();
        public override string ToString() => $"{{State: {State.ToString()}, Action: {Action.ToString()} }}";
    }
}