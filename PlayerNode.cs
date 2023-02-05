using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Jump_Bruteforcer;

namespace Jump_Bruteforcer
{
    public class State :IEquatable<State>
    {
        public int X { get; init; }
        public double Y { get; init; }
        public double VSpeed { get; init; }
        public bool CanDJump { get; init; }

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
            return X == other.X && Y == other.Y && VSpeed == other.VSpeed && CanDJump == other.CanDJump;
        }

        public override int GetHashCode()
        {
            return (X, Y, VSpeed).GetHashCode();
        }
    }
    public class PlayerNode : IEquatable<PlayerNode>
    {
        public State State { get; set; }
        public PlayerNode? Parent { get; set; }
        public double PathCost {get; set; }
        public Input? Action { get; set; }

        public PlayerNode(int x, double y, double vSpeed, bool canDJump = true, Input? action = null, int pathCost = 0, PlayerNode? parent = null) {
            State = new State()
            {
                X = x,
                Y = y,
                VSpeed = vSpeed,
                CanDJump = canDJump
            };
            Parent = parent;
            PathCost= pathCost;
            Action= action;
        }


        public (List<Input>, PointCollection) GetPath()
        {
            throw new NotImplementedException();
        } 

        public HashSet<PlayerNode> GetNeighbors() 
        { 
            throw new NotImplementedException(); 
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

            (int finalX, double finalY, bool reset) = Player.SolidCollision(CollisionMap, State.X, targetX, State.Y, targetY);
            finalVSpeed = reset ? 0 : finalVSpeed;

            bool canDJump = OnGround(targetX, targetY, CollisionMap) ||  OnGround(State.X, State.Y, CollisionMap) || (State.CanDJump && !input.HasFlag(Input.Jump));


            return new PlayerNode(finalX, finalY, finalVSpeed, canDJump, input);
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
                }else if (State.CanDJump)
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
            return (State.X, State.Y, State.VSpeed).GetHashCode();
        }
    }
}
