using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Jump_Bruteforcer;
using System.Text.Json;
using System.Collections.Immutable;
using System.Windows;

namespace Jump_Bruteforcer
{
    public class State :IEquatable<State>
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double VSpeed { get; init; }
        public double HSpeed { get; init; }
        public int RoundedY { get { return (int)Math.Round(Y); } }
        public int RoundedX { get { return (int)Math.Round(X); } }
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
        public bool Equals(State? other)=>
            RoundedX == other.RoundedX & RoundedY == other.RoundedY &
            ApproximatelyEquals(VSpeed, other.VSpeed) & ApproximatelyEquals(HSpeed, other.HSpeed);
        public override int GetHashCode() => (Quantize(X), Quantize(Y), Quantize(VSpeed), Quantize(HSpeed)).GetHashCode();
        public override string ToString() => JsonSerializer.Serialize(this);
        
    }
    public class PlayerNode : IEquatable<PlayerNode>
    {
        public State State { get; set; }
        public PlayerNode? Parent { get; set; }
        public uint PathCost { get; set; }
        public Input? Action { get; set; }
        public static readonly ImmutableArray<Input> inputs = ImmutableArray.Create(Input.Neutral, Input.Left, Input.Right, Input.Up, Input.Down, Input.Up | Input.Left, Input.Up |
                    Input.Right, Input.Down | Input.Left, Input.Down | Input.Right, Input.Left | Input.Right  );

        public PlayerNode(double x, double y, double vSpeed, double hSpeed, Input? action = null, uint pathCost = uint.MaxValue, PlayerNode? parent = null) =>
            (State, Parent, PathCost, Action) = (new State() { X = x, Y = y, VSpeed = vSpeed, HSpeed = hSpeed }, parent, pathCost, action);

        public bool IsGoal((int x, int y) goal)=> State.RoundedX == goal.x & State.RoundedY == goal.y;
        

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
                points.Add(new Point(currentNode.State.RoundedX, (currentNode.State.RoundedY)));
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
            var neighbors =  new HashSet<PlayerNode>();
            
            foreach (Input input in inputs)
            {
                PlayerNode neighbor = this;
                int i = 0;
                while (i < 5)
                {
                    neighbor = neighbor.NewState(input, CollisionMap);
                    if (Player.IsAlive(CollisionMap, neighbor))
                    {
                        neighbors.Add(neighbor);
                    }

                    i++;
                }


            }

            return neighbors; 
        }

        /// <summary>
        /// Creates the next state in the tree after applying inputs to the game starting from the current state
        /// </summary>
        /// <param name="input"></param> the inputs for the next frame
        /// <param name="CollisionMap"></param> the game field
        /// <returns>A new PlayerNode that results from running inputs on the collision map</returns>
        public PlayerNode NewState(Input input, CollisionMap CollisionMap)
        {
            
            (double X, double Y, double hSpeed, double vSpeed) = Player.BubbleStep(input, State.X, State.Y, State.HSpeed, State.VSpeed);
            (double finalX, double finalY, bool vSpeedReset, bool hSpeedReset) = Player.CollisionCheck(CollisionMap, State.X, X, State.Y, Y);
            hSpeed = hSpeedReset ? 0 : hSpeed;
            vSpeed = vSpeedReset ? 0 : vSpeed;



            return new PlayerNode(finalX, finalY, vSpeed, hSpeed, action: input, pathCost:PathCost + 1, parent:this);
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
        public override string ToString() => State.ToString();
    }
}
