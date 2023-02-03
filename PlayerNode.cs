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

    public class PlayerNode : IEquatable<PlayerNode>
    {
        public int X { get; init; }
        public double Y { get; init; } 
        public double VSpeed { get; init; } 
        public bool CanDJump { get; init; }
        public PlayerNode? Parent { get; set; }
        public double PathCost {get; set; }
        public Input? Action { get; set; }

        public PlayerNode(int x, double y, double vSpeed, bool canDJump = true, Input? action = null) {
            X = x;
            Y = y;
            VSpeed = vSpeed;
            CanDJump = canDJump;
            Parent = null;
            PathCost= 0;
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
            (int targetX, double targetY) = (X, Y);
            if (input.HasFlag(Input.Left))
            {
                targetX -= 3;
            }
            if (input.HasFlag(Input.Right))
            {
                targetX += 3;
            }
            double finalVSpeed = CalculateVSpeed(input, VSpeed);
            targetY += finalVSpeed;

            (int finalX, double finalY, bool reset) = Player.SolidCollision(CollisionMap, X, targetX, Y, targetY);
            finalVSpeed = reset ? 0 : CalculateVSpeed(input, VSpeed);
            bool canDJump = CollisionMap.TryGetValue((finalX, (int)Math.Round(finalY) + 1), out CollisionType ctype) && ctype.Equals(CollisionType.Solid);
            return new PlayerNode(finalX, finalY, finalVSpeed, canDJump, input);


        }

        public double CalculateVSpeed(Input input, double vspeed)
        {
            double finalVSpeed = vspeed;
            if (vspeed > PhysicsParams.MAX_VSPEED)
            {
                vspeed = PhysicsParams.MAX_VSPEED;
            }
            if (input.HasFlag(Input.Release))
            {
                vspeed *= PhysicsParams.RELEASE_MULTIPLIER;
            }


            return vspeed + PhysicsParams.GRAVITY;
        }


        public bool Equals(PlayerNode? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.VSpeed == other.VSpeed && this.X == other.X && this.Y == other.Y && this.CanDJump == other.CanDJump;
        }

        public override int GetHashCode()
        {
            return (X, Y, VSpeed).GetHashCode();
        }
    }
}
