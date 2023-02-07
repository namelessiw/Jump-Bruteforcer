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

namespace Jump_Bruteforcer
{
    public class State :IEquatable<State>
    {
        public int X { get; init; }
        public double Y { get; init; }
        public double VSpeed { get; init; }
        public bool CanJump { get; init; }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ((State)obj).Equals(this);
        }

        public bool Equals(State? other)
        {
            return JsonSerializer.Serialize(this) == JsonSerializer.Serialize(other);
        }

        public override int GetHashCode()
        {
            return JsonSerializer.Serialize(this).GetHashCode();
        }
    }
    public class PlayerNode : IEquatable<PlayerNode>
    {
        public State State { get; set; }
        public PlayerNode? Parent { get; set; }
        public int PathCost {get; set; }
        public Input? Action { get; set; }
        public static readonly ImmutableArray<Input> inputs = ImmutableArray.Create(Input.Neutral, Input.Left, Input.Right, Input.Jump, Input.Release, Input.Jump | Input.Release, Input.Left | Input.Jump,
                Input.Right | Input.Jump, Input.Left | Input.Release, Input.Right | Input.Release, Input.Left | Input.Jump | Input.Release, Input.Right | Input.Jump | Input.Release);

        public PlayerNode(int x, double y, double vSpeed, bool canJump = true, Input? action = null, int pathCost = 0, PlayerNode? parent = null) {
            State = new State()
            {
                X = x,
                Y = y,
                VSpeed = vSpeed,
                CanJump = canJump
            };
            Parent = parent;
            PathCost= pathCost;
            Action= action;
        }


        public (List<Input>, PointCollection) GetPath()
        {
            throw new NotImplementedException();
        } 

        /// <summary>
        /// creates the set of all unique states that can be reached in one frame from the current state with arbitrary inputs.
        /// states with fewer inputs are favored if two states are the same. States inside playerkillers are excluded.
        /// </summary>
        /// <returns>a Hashset of playerNodes</returns>
        /// <exception cref="NotImplementedException"></exception>
        public HashSet<PlayerNode> GetNeighbors(Dictionary<(int X, int Y), CollisionType> CollisionMap) 
        { 
            var neighbors =  new HashSet<PlayerNode>();
            foreach (Input input in inputs)
            {
                PlayerNode neighbor = NewState(input, CollisionMap);
                int yRounded = (int)Math.Round(neighbor.State.Y);
                CollisionMap.TryGetValue((neighbor.State.X, yRounded), out CollisionType ctype);
                if (ctype != CollisionType.Killer)
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
        /// <returns>A new <c>PlayerNode</c> that results from running inputs on the collision map</returns>
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
            double finalVSpeed = CalculateVSpeed(input, CollisionMap);
            targetY += finalVSpeed;

            (int finalX, double finalY, bool reset, bool DJumpRefresh) = Player.SolidCollision(CollisionMap, State.X, targetX, State.Y, targetY);
            finalVSpeed = reset ? 0 : finalVSpeed;

            bool canDJump = DJumpRefresh || OnGround(targetX, targetY, CollisionMap) ||  OnGround(State.X, State.Y, CollisionMap) || (State.CanJump && !input.HasFlag(Input.Jump));


            return new PlayerNode(finalX, finalY, finalVSpeed, canDJump, action: input, pathCost:PathCost + 1, parent:this);
        }

        private bool OnGround(int x, double y, Dictionary<(int X, int Y), CollisionType> CollisionMap)
        {
            int yRounded = (int)Math.Round(y);

            return CollisionMap.TryGetValue((x, yRounded + 1), out CollisionType ctype) && ctype == CollisionType.Solid;
        }

        private double CalculateVSpeed(Input input, Dictionary<(int X, int Y), CollisionType> CollisionMap)
        {
            double finalVSpeed = State.VSpeed;

            finalVSpeed = Math.Clamp(finalVSpeed, -PhysicsParams.MAX_VSPEED, PhysicsParams.MAX_VSPEED);


            if (input.HasFlag(Input.Jump)) 
            {
                if(OnGround(State.X, State.Y, CollisionMap))
                {
                    finalVSpeed = PhysicsParams.SJUMP_VSPEED;
                }else if (State.CanJump)
                {
                    finalVSpeed = PhysicsParams.DJUMP_VSPEED;
                }
            }
            if (input.HasFlag(Input.Release) && finalVSpeed < 0)
            {
                finalVSpeed *= PhysicsParams.RELEASE_MULTIPLIER;
            }
            finalVSpeed += PhysicsParams.GRAVITY;

            return finalVSpeed;
        }


        public bool Equals(PlayerNode? other)
        {
            if (other is null)
            {
                return false;
            }

            return State.Equals(other.State);
        }

        public override int GetHashCode()
        {
            return JsonSerializer.Serialize(State).GetHashCode();
        }
    }
}
