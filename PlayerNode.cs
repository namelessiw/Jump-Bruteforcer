using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Jump_Bruteforcer
{

    public class PlayerNode : IEquatable<PlayerNode>
    {
        public int X { get; init; }
        public double Y { get; init; } 
        public double VSpeed { get; init; } 
        public PlayerNode? Parent { get; set; }
        public double PathCost {get; set; }
        public Input? Action { get; set; }

        public PlayerNode(int x, double y, double vSpeed) {
            X = x;
            Y = y;
            VSpeed = vSpeed;
            Parent = null;
            PathCost= 0;
            Action= null;
        }
        
        public (List<Input>, PointCollection) GetPath()
        {
            throw new NotImplementedException();
        } 

        public HashSet<PlayerNode> GetNeighbors() 
        { 
            throw new NotImplementedException(); 
        }



        public bool Equals(PlayerNode? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.VSpeed == other.VSpeed && this.X == other.X && this.Y == other.Y;
        }

        public override int GetHashCode()
        {
            return (X, Y, VSpeed).GetHashCode();
        }
    }
}
