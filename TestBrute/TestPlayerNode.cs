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
            PlayerNode n2 = n1.NewState(Input.Left | Input.Jump, collision);
            PlayerNode n3 = n1.NewState(Input.Left | Input.Jump | Input.Release, collision);
            PlayerNode n4 = n3.NewState(Input.Right | Input.Jump | Input.Release, collision);
            PlayerNode n5 = n4.NewState(Input.Release, collision);
            PlayerNode n6 = n5.NewState(Input.Right | Input.Release, collision);


            n1.NewState(Input.Left, collision).State.Should().BeEquivalentTo(new PlayerNode(447, 567.254, 0).State);
            n1.NewState(Input.Right, collision).State.Should().BeEquivalentTo(new PlayerNode(453, 567.254, 0).State);
            
            n2.State.Should().BeEquivalentTo(new PlayerNode(447, 559.154, -8.1).State);
            n1.NewState(Input.Right | Input.Jump, collision).State.Should().BeEquivalentTo(new PlayerNode(453, 559.154, -8.1).State);

            n3.State.Should().BeEquivalentTo(new PlayerNode(447, 563.8290000000001, -3.4250000000000003).State);
            n1.NewState(Input.Right | Input.Jump | Input.Release, collision).State.Should().BeEquivalentTo(new PlayerNode(453, 563.8290000000001, -3.4250000000000003).State);

            n2.NewState(Input.Neutral, collision).State.Should().BeEquivalentTo(new PlayerNode(447, 551.454, -7.699999999999999).State);
            n3.NewState(Input.Neutral, collision).State.Should().BeEquivalentTo(new PlayerNode(447, 560.8040000000001, -3.0250000000000004).State);

            n4.State.Should().BeEquivalentTo(new PlayerNode(450, 561.0790000000001, -2.75, false).State);
            n3.NewState(Input.Right | Input.Jump, collision).State.Should().BeEquivalentTo(new PlayerNode(450, 557.229, -6.6, false).State);

            n4.NewState(Input.Neutral, collision).State.Should().BeEquivalentTo(new PlayerNode(450, 558.729, -2.35, false).State);
            n4.NewState(Input.Jump, collision).State.Should().BeEquivalentTo(new PlayerNode(450, 558.729, -2.35, false).State);

            n5.State.Should().BeEquivalentTo(new PlayerNode(450, 560.2415000000001, -0.8375, false).State);
            n4.NewState(Input.Jump | Input.Release, collision).State.Should().BeEquivalentTo(new PlayerNode(450, 560.2415000000001, -0.8375, false).State);

            n6.State.Should().BeEquivalentTo(new PlayerNode(453, 560.2646250000001, 0.023125000000000007, false).State);

            n6.NewState(Input.Neutral, collision).State.Should().BeEquivalentTo(new PlayerNode(453, 560.6877500000002, 0.42312500000000003, false).State);
            n6.NewState(Input.Release, collision).State.Should().BeEquivalentTo(new PlayerNode(453, 560.6877500000002, 0.42312500000000003, false).State);

            PlayerNode n7 = n6;
            for (int i = 0; i < 5; i++)
            {
                n7 = n7.NewState(Input.Neutral, collision);
            }
            n7.State.Should().BeEquivalentTo(new PlayerNode(453, 566.3802500000002, 2.023125, false).State);
            n7.NewState(Input.Jump, collision).State.Should().BeEquivalentTo(new PlayerNode(453, 567.3802500000002, 0, true).State);

        }


    }
}
