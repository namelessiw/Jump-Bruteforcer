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
        public int X { get; init; }
        public double Y { get; init; }
        public double VSpeed { get; init; }
        public bool CanDJump { get; init; }
        public int RoundedY { get { return (int)Math.Round(Y); } }
        const int roundAmount = 1;

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ((State)obj).Equals(this);
        }

        public bool Equals(State? other)=>
            X == other.X & Math.Round(Y, roundAmount) == Math.Round(other.Y, roundAmount) &
            Math.Round(VSpeed, roundAmount) == Math.Round(other.VSpeed, roundAmount) & CanDJump == other.CanDJump;
        public override int GetHashCode() => (X, Math.Round(Y, roundAmount),Math.Round(VSpeed, roundAmount), CanDJump).GetHashCode();
        public override string ToString() => JsonSerializer.Serialize(this);
        
    }
    public class PlayerNode : IEquatable<PlayerNode>
    {
        public State State { get; set; }
        public PlayerNode? Parent { get; set; }
        public uint PathCost { get; set; }
        public Input? Action { get; set; }
        public static readonly ImmutableArray<Input> inputs = ImmutableArray.Create(Input.Neutral, Input.Left, Input.Right, Input.Jump, Input.Release, Input.Jump | Input.Release, Input.Left | Input.Jump,
                Input.Right | Input.Jump, Input.Left | Input.Release, Input.Right | Input.Release, Input.Left | Input.Jump | Input.Release, Input.Right | Input.Jump | Input.Release);

        public PlayerNode(int x, double y, double vSpeed, bool canDJump = true, Input? action = null, uint pathCost = uint.MaxValue, PlayerNode? parent = null) =>
            (State, Parent, PathCost, Action) = (new State() { X = x, Y = y, VSpeed = vSpeed, CanDJump = canDJump }, parent, pathCost, action);

        public static string GetMacro(List<Input> inputs)
        {
            if (inputs.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();

            int Direction = 0, NextDirection;

            foreach (Input input in inputs)
            {
                bool InputChanged = false;

                NextDirection = 0;
                if ((input & Input.Left) == Input.Left)
                {
                    NextDirection = -1;
                }
                else if ((input & Input.Right) == Input.Right)
                {
                    NextDirection = 1;
                }

                if (Direction != NextDirection)
                {
                    if (Direction != 0)
                    {
                        sb.Append((Direction == 1 ? "RightArrow" : "LeftArrow") + "(R)");
                        InputChanged = true;
                    }

                    if (NextDirection != 0)
                    {
                        sb.Append((InputChanged ? "," : "") + (NextDirection == 1 ? "RightArrow" : "LeftArrow") + "(P)");
                    }

                    InputChanged = true;
                }

                if ((input & Input.Jump) == Input.Jump)
                {
                    sb.Append((InputChanged ? "," : "") + "J(PR)");

                    InputChanged = true;
                }
                if ((input & Input.Release) == Input.Release)
                {
                    sb.Append((InputChanged ? "," : "") + "K(PR)");
                }

                Direction = NextDirection;

                sb.Append('>');
            }

            return sb.ToString();
        }

        public static string GetInputString(List<Input> inputs)
        {
            if (inputs.Count == 0)
                return "Frames: 0";
            
            StringBuilder sb = new();

            sb.AppendLine($"Frames: {inputs.Count}");

            Input PreviousInput = inputs[0];
            int Count = 1;

            for (int i = 1; i < inputs.Count; i++)
            {
                if (inputs[i] == PreviousInput)
                {
                    Count++;
                }
                else
                {
                    sb.AppendLine($"{PreviousInput}{(Count > 1 ? $" x{Count}" : "")}");
                    PreviousInput = inputs[i];
                    Count = 1;
                }
            }

            sb.AppendLine($"{PreviousInput}{(Count > 1 ? $" x{Count}" : "")}");

            return sb.ToString();
        }

        public bool IsGoal((int x, int y) goal)=> State.X == goal.x & State.RoundedY == goal.y;
        

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
                points.Add(new Point(currentNode.State.X, (currentNode.State.RoundedY)));
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
        public HashSet<PlayerNode> GetNeighbors(Dictionary<(int X, int Y), CollisionType> CollisionMap) 
        { 
            var neighbors =  new HashSet<PlayerNode>();
            foreach (Input input in inputs)
            {
                PlayerNode neighbor = NewState(input, CollisionMap);
                if (Player.IsAlive(CollisionMap, neighbor))
                {
                    neighbors.Add(neighbor);
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
        public PlayerNode NewState(Input input, Dictionary<(int X, int Y), CollisionType> CollisionMap)
        {
            
            (int targetX, double targetY) = (State.X, State.Y);
            if (input.HasFlag(Input.Left))
            {
                targetX -= 3;
            }
            if (input.HasFlag(Input.Right))
            {
                targetX += 3;
            }
            (double finalVSpeed, bool DJumpRefresh) = Player.CalculateVSpeed(this, input, CollisionMap);
            targetY += finalVSpeed;

            (_, int finalX, double finalY, bool reset, bool DJumpRefresh2) = Player.CollisionCheck(CollisionMap, State.X, targetX, State.Y, targetY);
            finalVSpeed = reset ? 0 : finalVSpeed;

            DJumpRefresh |= DJumpRefresh2 || (State.CanDJump && !input.HasFlag(Input.Jump));

            return new PlayerNode(finalX, finalY, finalVSpeed, DJumpRefresh, action: input, pathCost:PathCost + 1, parent:this);
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
