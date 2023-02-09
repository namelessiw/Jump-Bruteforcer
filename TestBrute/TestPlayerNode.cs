using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
            var n4 = new PlayerNode(0, 0, 0, false);
            var q = new SimplePriorityQueue<PlayerNode, float>();
            q.Enqueue(n1, 1f);
            q.Contains(n2).Should().BeTrue();
            q.Contains(n3).Should().BeFalse();
            q.Contains(n4).Should().BeFalse();

        }
        
        [Fact]
        public void TestNewStateBasic()
        {
            int floor_thickness = 10;
            double playerY = 567.254;
            int playerYRounded = (int)Math.Round(playerY);
            var n1 = new PlayerNode(450, playerY, 0);
            
            Dictionary<(int, int), CollisionType> collision = Enumerable.Range(0, 100 * floor_thickness).ToDictionary(x => (x % 100 + 400, x / 100 + playerYRounded + 1), x => CollisionType.Solid);
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
            n7 = n7.NewState(Input.Neutral, collision);
            n7.NewState(Input.Jump, collision).State.Should().BeEquivalentTo(new PlayerNode(453, 559.2802500000001, -8.1, true).State);
        }

        [Fact]
        public void TestNewStateCorner()
        {
            
            var n1 = new PlayerNode(0, 0, 2, false);
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (3, 2), CollisionType.Solid },
                { (1, 2), CollisionType.Solid },
                { (3, 0), CollisionType.Solid },
                { (2, 2), CollisionType.Solid },
                { (3, 1), CollisionType.Solid },
            };
            n1.NewState(Input.Right | Input.Jump, collision).State.Should().BeEquivalentTo(new PlayerNode(2, 1, 0, true).State);
            PlayerNode n2 = n1.NewState(Input.Right, collision);
            n2.State.Should().BeEquivalentTo(new PlayerNode(2, 1, 0, true).State);
            n2.NewState(Input.Jump, collision).State.Should().BeEquivalentTo(new PlayerNode(2, -7.1, -8.1, true).State);
            var n3 = new PlayerNode(5, 4, -2, false);
            n3.NewState(Input.Left, collision).State.Should().BeEquivalentTo(new PlayerNode(5, 2.4, -1.6, false).State);
        }

        [Theory]
        [InlineData(10, 13, 15, 10, 12, 13, false)] // up right
        [InlineData(10, 7, 15, 10, 12, 13, false)] // up left
        [InlineData(10, 13, 15, 20, 18, 17, true)] // down right
        [InlineData(10, 7, 15, 20, 18, 17, true)] // down left
        [InlineData(419, 422, 406.9, 416.3, 408, 406.9, true)] // down right
        [InlineData(419, 422, 406.5, 414.25, 408, 406.5, true)] // down right vfpi
        [InlineData(419, 422, 407.5, 415.25, 410, 408.5, true)] // down right vfpi
        [InlineData(452, 455, 406.95, 410.325, 408, 406.95, true)] // down right
        [InlineData(452, 455, 406.5, 409.875, 408, 406.5, true)] // down right vfpi
        [InlineData(452, 455, 407.5, 410.875, 410, 408.5, true)] // down right vfpi
        [InlineData(452, 455, 407.5, 410.875, 409, 410.5, true)] // down right vfpi
        [InlineData(452, 455, 408.5, 411.875, 410, 408.5, true)] // down right vfpi
        public void TestNewStateDualCollision(int startX, int targetX, double startY, double targetY, int solidY, double endY, bool canJump)
        {
            int tY = (int)Math.Round(targetY);
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (targetX, tY), CollisionType.Solid },
                { (startX, solidY), CollisionType.Solid } // snap to this solid
            };

            if (solidY != tY)
            {
                collision.Add((startX, tY), CollisionType.Solid);
            }

            double vspeed = targetY - startY - PhysicsParams.GRAVITY;
            PlayerNode n1 = new(startX, startY, vspeed, canJump:false);
            Input input = targetX - startX < 0? Input.Left : Input.Right;
            n1.NewState(input, collision).State.Should().BeEquivalentTo(new PlayerNode(targetX, endY, 0, canJump).State);
        }

        [Fact]
        public void TestNewStateBHop()
        {
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (0, 568), CollisionType.Solid }
            };
            PlayerNode n1 = new(0, 567.1, 0);
            n1 = n1.NewState(Input.Jump | Input.Release, collision);
            for (int i = 0; i < 17; i++)
            {
                n1 = n1.NewState(Input.Neutral, collision);
            }
            n1.State.Should().BeEquivalentTo(new PlayerNode(0, 566.6500000000001, 3.374999999999999).State);
            n1.NewState(Input.Jump, collision).State.Should().BeEquivalentTo(new PlayerNode(0, 558.5500000000001, -8.1).State);
        }

        [Fact]
        public void TestGetNeighborsNoCollisionNegativeVSpeed()
        {
            Dictionary<(int, int), CollisionType> collision = new();
            PlayerNode n1 = new(400, 400, -1);

            PlayerNode[] players = new PlayerNode[PlayerNode.inputs.Length];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = n1.NewState(PlayerNode.inputs[i], collision);
            }

            n1.GetNeighbors(collision).Should().BeEquivalentTo(new HashSet<PlayerNode>(players));

        }
        [Fact]
        public void TestGetNeighborsNoCollisionPositiveVSpeedNoJump()
        {
            Dictionary<(int, int), CollisionType> collision = new();
            PlayerNode n1 = new(400, 400, 1, false);

            Input[] inputs =  {Input.Neutral, Input.Left, Input.Right};
            PlayerNode[] players = new PlayerNode[inputs.Length];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = n1.NewState(inputs[i], collision);
            }

            n1.GetNeighbors(collision).Should().BeEquivalentTo(new HashSet<PlayerNode>(players));

        }

        [Fact]
        public void TestGetNeighborsNoCollisionPositiveVSpeed()
        {
            Dictionary<(int, int), CollisionType> collision = new();
            PlayerNode n1 = new(400, 400, 1);

            Input[] inputs =  {Input.Neutral, Input.Left, Input.Right, Input.Jump, Input.Jump | Input.Release, Input.Left | Input.Jump,
                Input.Right | Input.Jump, Input.Left | Input.Jump | Input.Release, Input.Right | Input.Jump | Input.Release };
            PlayerNode[] players = new PlayerNode[inputs.Length];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = n1.NewState(inputs[i], collision);
            }

            n1.GetNeighbors(collision).Should().BeEquivalentTo(new HashSet<PlayerNode>(players));

        }

        [Fact]
        public void TestGetNeighborsCollisionAndKiller()
        {
            Dictionary<(int, int), CollisionType> collision = new() { 
                { (0, 568), CollisionType.Solid },         
                { (0, 559), CollisionType.Killer },
            };
            for(int x = -10; x < 10; x++)
            {
                collision[(x, 568)] = CollisionType.Solid;
                collision[(x, 559)] = CollisionType.Killer;
            }
            for (int y = 559; y < 568; y++)
            {
                for(int x = -3; x < 0; x++)
                {
                    collision[(x, y)] = CollisionType.Solid;
                }
            }

            PlayerNode n1 = new(0, 567.1, 0);
            Input[] inputs = { Input.Neutral, Input.Right, Input.Jump | Input.Release, Input.Right | Input.Jump | Input.Release };

            PlayerNode[] players = new PlayerNode[inputs.Length];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = n1.NewState(inputs[i], collision);
            }

            n1.GetNeighbors(collision).Should().BeEquivalentTo(new HashSet<PlayerNode>(players));

        }

        [Fact]
        public void TestGetPath()
        {
            Dictionary<(int, int), CollisionType> collision = new() {
                { (400, 568), CollisionType.Solid },
            };
            PlayerNode n1 = new(400, 567.1, 0);
            List<Input> inputs  = new List<Input>() {Input.Left | Input.Jump, Input.Left, Input.Left, Input.Left | Input.Release, Input.Left, Input.Neutral, Input.Neutral,
            Input.Right | Input.Jump | Input.Release, Input.Right | Input.Release, Input.Right | Input.Release, Input.Right, Input.Neutral};
            PointCollection points = new PointCollection() { new Point(400, 567), new Point(397, 559), new Point(394, 551), new Point(391, 544), new Point(388, 541),
            new Point(385, 539), new Point(385, 537), new Point(385, 535), new Point(388, 532), new Point(391, 531), new Point(394, 531), new Point(397, 532), new Point(397, 533)};

            foreach(Input input in inputs)
            {
                n1 = n1.NewState(input, collision);
            }

            (List<Input> Inputs, PointCollection Points)  path = n1.GetPath();
            path.Inputs.Should().Equal(inputs);
            path.Points.Should().Equal(points);

        }

        [Fact]
        public void TestIsGoal()
        {
            (int x, int y) goal = (42, 99);
            new PlayerNode(42, 99.3, 0).IsGoal(goal).Should().BeTrue();
            new PlayerNode(42, 98.9, 0).IsGoal(goal).Should().BeTrue();

            new PlayerNode(42, 99.6, 0).IsGoal(goal).Should().BeFalse();

        }

        public void TestNewStateBonk()
        {
            Dictionary<(int, int), CollisionType> collision = new() {
                { (0, 568), CollisionType.Solid },
                { (0, 560), CollisionType.Solid },
                { (0, 559), CollisionType.Solid },
                { (0, 558), CollisionType.Solid },
                { (0, 557), CollisionType.Solid },
                { (0, 556), CollisionType.Solid },
                { (0, 555), CollisionType.Solid },
                { (0, 554), CollisionType.Solid },
                { (0, 553), CollisionType.Solid },
            };
            PlayerNode n1 = new(0, 567.1, 0);
            n1 = n1.NewState(Input.Jump, collision);
            n1.State.CanJump.Should().BeTrue();
            n1 = n1.NewState(Input.Jump, collision);
            n1.State.CanJump.Should().BeFalse();

        }

        public void TestCorner()
        {
            Dictionary<(int, int), CollisionType> collision = new() {
                { (0, 568), CollisionType.Solid },
                { (0, 560), CollisionType.Solid },
                { (0, 559), CollisionType.Solid },
                { (0, 558), CollisionType.Solid },
                { (0, 557), CollisionType.Solid },
                { (0, 556), CollisionType.Solid },
                { (0, 555), CollisionType.Solid },
                { (0, 554), CollisionType.Solid },
                { (0, 553), CollisionType.Solid },
            };
            PlayerNode n1 = new(0, 567.1, 0);
            n1 = n1.NewState(Input.Jump, collision);
            n1.State.CanJump.Should().BeTrue();
            n1 = n1.NewState(Input.Jump, collision);
            n1.State.CanJump.Should().BeFalse();

        }


    }
}
