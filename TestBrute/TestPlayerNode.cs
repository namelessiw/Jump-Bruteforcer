using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Jump_Bruteforcer;
using Priority_Queue;

namespace TestBrute
{
    public class TestPlayerNode
    {
        [Theory]
        [InlineData(1,2,3, true, false)]
        [InlineData(0,0,0, true, true)]
        [InlineData(0,0,0,false, false)]
        public void TestNodeEquals(int x, double y, double vSpeed, bool canDJump, bool shouldEqual)
        {
            var n1 = new PlayerNode(0,0,0, true);
            var n2 = new PlayerNode(x, y, vSpeed, canDJump);
            (n1.Equals(n2)).Should().Be(shouldEqual);

        }

        [Fact]
        public void TestQueueContains()
        {
            var n1 = new PlayerNode(0, 0, 0);
            var n2 = new PlayerNode(0, 0, 0);
            var n3 = new PlayerNode(1, 0, 0);
            var q = new SimplePriorityQueue<PlayerNode, float>();
            q.Enqueue(n1, 1f);
            q.Contains(n2).Should().BeTrue();
            q.Contains(n3).Should().BeFalse();

        }
        
        [Fact]
        public void TestNewState()
        {
            int floor_thickness = 10;
            double playerY = 567.254;
            int playerYRounded = (int)Math.Round(playerY);
            var n1 = new PlayerNode(450, playerY, 0);
            Dictionary<(int, int), CollisionType> collision = Enumerable.Range(0, 800 * floor_thickness).ToDictionary(x => (x % 800, x / 800 + playerYRounded + 1), x => CollisionType.Solid);
            n1.NewState(Input.Left, collision).Equals(new PlayerNode(447, 567.254, 0)).Should().BeTrue();
            n1.NewState(Input.Right, collision).Equals(new PlayerNode(453, 567.254, 0)).Should().BeTrue();
            /*
            n1.NewState(Input.Left | Input.Jump, collision).Should().Equals(new PlayerNode(447, 559.154, -8.1));
            n1.NewState(Input.Right | Input.Jump, collision).Should().Equals(new PlayerNode(453, 559.154, -8.1));
            n1.NewState(Input.Left | Input.Jump | Input.Release, collision).Should().Equals(new PlayerNode(447, 563.829, -3.425));
            n1.NewState(Input.Right | Input.Jump | Input.Release, collision).Should().Equals(new PlayerNode(453, 563.829, -3.425));*/
        }


    }
}
