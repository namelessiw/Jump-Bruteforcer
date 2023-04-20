using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FluentAssertions;
using Jump_Bruteforcer;
using Priority_Queue;
using Xunit.Abstractions;

namespace TestBrute
{
    public class TestPlayerNode
    {


        private readonly ITestOutputHelper output;

        public TestPlayerNode(ITestOutputHelper output)
        {
            if (Application.Current == null) //https://stackoverflow.com/a/14224558
                new Application();
            this.output = output;
        }


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

            Dictionary<(int, int), ImmutableSortedSet<CollisionType>> collision = Enumerable.Range(0, 100 * floor_thickness).ToDictionary(x => (x % 100 + 400, x / 100 + playerYRounded + 1), x => ImmutableSortedSet.Create(CollisionType.Solid));
            CollisionMap cmap = new(
                collision, null);

            PlayerNode n2 = n1.NewState(Input.Left | Input.Jump, cmap);
            PlayerNode n3 = n1.NewState(Input.Left | Input.Jump | Input.Release, cmap);
            PlayerNode n4 = n3.NewState(Input.Right | Input.Jump | Input.Release, cmap);
            PlayerNode n5 = n4.NewState(Input.Release, cmap);
            PlayerNode n6 = n5.NewState(Input.Right | Input.Release, cmap);


            n1.NewState(Input.Left, cmap).State.Should().BeEquivalentTo(new PlayerNode(447, 567.254, 0).State);
            n1.NewState(Input.Right, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 567.254, 0).State);
            
            n2.State.Should().BeEquivalentTo(new PlayerNode(447, 559.154, -8.1).State);
            n1.NewState(Input.Right | Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 559.154, -8.1).State);

            n3.State.Should().BeEquivalentTo(new PlayerNode(447, 563.8290000000001, -3.4250000000000003).State);
            n1.NewState(Input.Right | Input.Jump | Input.Release, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 563.8290000000001, -3.4250000000000003).State);

            n2.NewState(Input.Neutral, cmap).State.Should().BeEquivalentTo(new PlayerNode(447, 551.454, -7.699999999999999).State);
            n3.NewState(Input.Neutral, cmap).State.Should().BeEquivalentTo(new PlayerNode(447, 560.8040000000001, -3.0250000000000004).State);

            n4.State.Should().BeEquivalentTo(new PlayerNode(450, 561.0790000000001, -2.75, false).State);
            n3.NewState(Input.Right | Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(450, 557.229, -6.6, false).State);

            n4.NewState(Input.Neutral, cmap).State.Should().BeEquivalentTo(new PlayerNode(450, 558.729, -2.35, false).State);
            n4.NewState(Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(450, 558.729, -2.35, false).State);

            n5.State.Should().BeEquivalentTo(new PlayerNode(450, 560.2415000000001, -0.8375, false).State);
            n4.NewState(Input.Jump | Input.Release, cmap).State.Should().BeEquivalentTo(new PlayerNode(450, 560.2415000000001, -0.8375, false).State);

            n6.State.Should().BeEquivalentTo(new PlayerNode(453, 560.2646250000001, 0.023125000000000007, false).State);

            n6.NewState(Input.Neutral, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 560.6877500000002, 0.42312500000000003, false).State);
            n6.NewState(Input.Release, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 560.6877500000002, 0.42312500000000003, false).State);

            PlayerNode n7 = n6;
            for (int i = 0; i < 5; i++)
            {
                n7 = n7.NewState(Input.Neutral, cmap);
            }
            n7.State.Should().BeEquivalentTo(new PlayerNode(453, 566.3802500000002, 2.023125, false).State);
            n7.NewState(Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 567.3802500000002, 0, true).State);
            n7 = n7.NewState(Input.Neutral, cmap);
            n7.NewState(Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 559.2802500000001, -8.1, true).State);
        }

        [Fact]
        public void TestNewStateCorner()
        {
            
            var n1 = new PlayerNode(0, 0, 2, false);
            Dictionary<(int, int), ImmutableSortedSet<CollisionType>> collision = new()
            {
                { (3, 2), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (1, 2), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (3, 0), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (2, 2), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (3, 1), ImmutableSortedSet.Create(CollisionType.Solid) },
            };
            CollisionMap cmap = new(collision, null);

            n1.NewState(Input.Right | Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(2, 1, 0, true).State);
            PlayerNode n2 = n1.NewState(Input.Right, cmap);
            n2.State.Should().BeEquivalentTo(new PlayerNode(2, 1, 0, true).State);
            n2.NewState(Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(2, -7.1, -8.1, true).State);
            var n3 = new PlayerNode(5, 4, -2, false);
            n3.NewState(Input.Left, cmap).State.Should().BeEquivalentTo(new PlayerNode(5, 2.4, -1.6, false).State);
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
            Dictionary<(int, int), ImmutableSortedSet<CollisionType>> collision = new()
            {
                { (targetX, tY), ImmutableSortedSet.Create( CollisionType.Solid) },
                { (startX, solidY), ImmutableSortedSet.Create(CollisionType.Solid) } // snap to this solid
            };

            if (solidY != tY)
            {
                collision.Add((startX, tY), ImmutableSortedSet.Create( CollisionType.Solid));
            }
            CollisionMap cmap = new(collision, null);

            double vspeed = targetY - startY - PhysicsParams.GRAVITY;
            PlayerNode n1 = new(startX, startY, vspeed, canDJump:false);
            Input input = targetX - startX < 0? Input.Left : Input.Right;
            n1.NewState(input, cmap).State.Should().BeEquivalentTo(new PlayerNode(targetX, endY, 0, canJump).State);
        }

        [Fact]
        public void TestHorizontalDualCollision()
        {
            string path = @$"..\..\..\jmaps\1_green_5.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(342, 376.845, 0);
            n1.NewState(Input.Left | Input.Jump, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(341, 371.845, 0, false, false).State);
        }

        [Fact]
        public void TestNewStateBHop()
        {
            Dictionary<(int, int), ImmutableSortedSet<CollisionType>> collision = new()
            {
                { (0, 568), ImmutableSortedSet.Create(CollisionType.Solid) }
            };
            CollisionMap cmap = new(collision, null);

            PlayerNode n1 = new(0, 567.1, 0);
            n1 = n1.NewState(Input.Jump | Input.Release, cmap);
            for (int i = 0; i < 17; i++)
            {
                n1 = n1.NewState(Input.Neutral, cmap);
            }
            n1.State.Should().BeEquivalentTo(new PlayerNode(0, 566.6500000000001, 3.374999999999999).State);
            n1.NewState(Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(0, 558.5500000000001, -8.1).State);
        }

        [Fact]
        public void TestGetNeighborsNoCollisionNegativeVSpeed()
        {
            Dictionary<(int, int), ImmutableSortedSet<CollisionType>> collision = new();
            CollisionMap cmap = new(collision, null);

            PlayerNode n1 = new(400, 400, -1);

            PlayerNode[] players = new PlayerNode[PlayerNode.inputs.Length];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = n1.NewState(PlayerNode.inputs[i], cmap);
            }

            n1.GetNeighbors(cmap).Should().BeEquivalentTo(new HashSet<PlayerNode>(players));

        }
        [Fact]
        public void TestGetNeighborsNoCollisionPositiveVSpeedNoJump()
        {
            Dictionary<(int, int), ImmutableSortedSet<CollisionType>> collision = new();
            CollisionMap cmap = new(collision, null);
            PlayerNode n1 = new(400, 400, 1, false);

            Input[] inputs =  {Input.Neutral, Input.Left, Input.Right};
            PlayerNode[] players = new PlayerNode[inputs.Length];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = n1.NewState(inputs[i], cmap);
            }

            n1.GetNeighbors(cmap).Should().BeEquivalentTo(new HashSet<PlayerNode>(players));

        }

        [Fact]
        public void TestGetNeighborsNoCollisionPositiveVSpeed()
        {
            Dictionary<(int, int), ImmutableSortedSet<CollisionType>> collision = new();
            CollisionMap cmap = new(collision, null);
            PlayerNode n1 = new(400, 400, 1);

            Input[] inputs =  {Input.Neutral, Input.Left, Input.Right, Input.Jump, Input.Jump | Input.Release, Input.Left | Input.Jump,
                Input.Right | Input.Jump, Input.Left | Input.Jump | Input.Release, Input.Right | Input.Jump | Input.Release };
            PlayerNode[] players = new PlayerNode[inputs.Length];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = n1.NewState(inputs[i], cmap);
            }

            n1.GetNeighbors(cmap).Should().BeEquivalentTo(new HashSet<PlayerNode>(players));

        }

     

        [Fact]
        public void TestGetPath()
        {
            Dictionary<(int, int), ImmutableSortedSet<CollisionType>> collision = new() {
                { (400, 568), ImmutableSortedSet.Create(CollisionType.Solid) },
            };
            CollisionMap cmap = new(collision, null);

            PlayerNode n1 = new(400, 567.1, 0);
            List<Input> inputs  = new List<Input>() {Input.Left | Input.Jump, Input.Left, Input.Left, Input.Left | Input.Release, Input.Left, Input.Neutral, Input.Neutral,
            Input.Right | Input.Jump | Input.Release, Input.Right | Input.Release, Input.Right | Input.Release, Input.Right, Input.Neutral};
            PointCollection points = new PointCollection() { new Point(400, 567), new Point(397, 559), new Point(394, 551), new Point(391, 544), new Point(388, 541),
            new Point(385, 539), new Point(385, 537), new Point(385, 535), new Point(388, 532), new Point(391, 531), new Point(394, 531), new Point(397, 532), new Point(397, 533)};

            foreach(Input input in inputs)
            {
                n1 = n1.NewState(input, cmap);
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

        [Fact]
        public void TestNewStateBonk()
        {
            Dictionary<(int, int), ImmutableSortedSet<CollisionType>> collision = new() {
                { (0, 568), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 560), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 559), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 558), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 557), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 556), ImmutableSortedSet.Create(CollisionType.Solid)},
                { (0, 555), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 554), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 553), ImmutableSortedSet.Create(CollisionType.Solid) },
            };
            CollisionMap cmap = new(collision, null);

            PlayerNode n1 = new(0, 567.1, 0);
            n1 = n1.NewState(Input.Jump, cmap);
            n1.State.CanDJump.Should().BeTrue();
            n1 = n1.NewState(Input.Jump, cmap);
            n1.State.CanDJump.Should().BeFalse();

        }

        [Fact]
        public void TestCorner()
        {
            Dictionary<(int, int), ImmutableSortedSet<CollisionType>> collision = new() {
                { (0, 568), ImmutableSortedSet.Create( CollisionType.Solid )},
                { (0, 560), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 559), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 558), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 557), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 556), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 555), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 554), ImmutableSortedSet.Create(CollisionType.Solid) },
                { (0, 553), ImmutableSortedSet.Create(CollisionType.Solid) },
            };
            CollisionMap cmap = new(collision, null);

            PlayerNode n1 = new(0, 567.1, 0);
            n1 = n1.NewState(Input.Jump, cmap);
            n1.State.CanDJump.Should().BeTrue();
            n1 = n1.NewState(Input.Jump, cmap);
            n1.State.CanDJump.Should().BeFalse();

        }
        [Fact]
        public void TestDJumpDoesNotRefresh()
        {

            string path = @$"..\..\..\jmaps\dj.jmap";
            string Text = File.ReadAllText(path);
            Input[] inputs = new Input[] {Input.Right, Input.Left, Input.Left, Input.Left, Input.Left, Input.Left | Input.Jump, Input.Neutral};
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(452, 407.4, 0);

            foreach (Input input in inputs)
            {
                n1 = n1.NewState(input, Map.CollisionMap);
            }
            n1.State.CanDJump.Should().BeFalse();
        }

        [Fact]
        public void TestMoveOnPlatform()
        {

            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(394, 375, 0, true, true);
            n1.State.CanDJump.Should().BeTrue();

            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 375.4, 0.4, true, true).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 375, 0, true, true).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 375.4, 0.4, true, true).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 375, 0, true, true).State);
            n1 = n1.NewState(Input.Left, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(391, 375.4, 0.4, true, true).State);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 375, 0, true, true).State);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(397, 375.4, 0.4, true, true).State);

        }

        [Fact]
        public void TestFallInsidePlatform()
        {

            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(394, 396, 0, true, false);

            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 396.4, 0.4, true, false).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 397.2, 0.8, true, false).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 398.4, 1.2000000000000002, true, false).State);

        }
        [Fact]
        public void TestMoveOutOfPlatform()
        {

            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(418, 380, 6, true, false);

            n1.NewState(Input.Neutral, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(418, 375, 0, true, true).State);

            n1.NewState(Input.Right, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(421, 386.4, 6.4, true, false).State);
            

        }

        [Fact]
        public void TestMoveIntoPlatform()
        {

            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(421, 380, 6, true, false);

            n1.NewState(Input.Neutral, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(421, 386.4, 6.4, true, false).State);

            n1.NewState(Input.Left, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(418, 375, 0, true, true).State);


        }

        [Fact]
        public void TestPlatformTripleJump()
        {
            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(394, 375, 0, true, true);
            n1.State.CanDJump.Should().BeTrue();

            n1 = n1.NewState(Input.Jump | Input.Release, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 371.575, -3.4250000000000003, true, true).State);
            n1 = n1.NewState(Input.Jump | Input.Release, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 368.15, -3.4250000000000003, true, true).State);
            n1 = n1.NewState(Input.Jump | Input.Release, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 365.4, -2.75, false, false).State);

        }

        [Fact]
        public void TestFallOntoPlatform()
        {
            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(394, 375, 9.4, true, true);
            n1.NewState(Input.Neutral, Map.CollisionMap).State.VSpeed.Should().Be(0);
            

        }

        [Fact]
        public void TestCannotFallThrough()
        {
            string path = @$"..\..\..\jmaps\platform_triple_jump.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(410, 408, 0, true, true);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(410, 407.4, 0.4, true, true).State);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(410, 407, 0, true, true).State);
        }
        [Fact]
        public void TestSnapCausesOnPlatform()
        {
            string path = @$"..\..\..\jmaps\platform_triple_jump.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(401, 407, 0, true, false);
            n1.NewState(Input.Jump | Input.Release, Map.CollisionMap).NewState(Input.Jump, Map.CollisionMap).State.VSpeed.Should().NotBe(PhysicsParams.SJUMP_VSPEED + PhysicsParams.GRAVITY);
        }

        [Fact]
        public void TestCeilingPlatform()
        {
            string path = @$"..\..\..\jmaps\platform_ceiling.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(466, 499, 0, false, false);
            n1.NewState(Input.Neutral, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(466, 499.4, 0.4, false, false).State);
            n1 = n1.NewState(Input.Jump, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(466, 492, 0, true, false).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(466, 492.4, 0.4, true, false).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(466, 493.2, 0.8, true, false).State);
        }

        // requires multiple object types per pixel
        [Fact]
        public void TestNabla2()
        {
            string path = @$"..\..\..\jmaps\nabla_2.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            var n1 = new PlayerNode(479, 592.8957, 8.8, false, false);

            Input[] inputs = new Input[]
            {
                Input.Left | Input.Jump,
                Input.Left,
                Input.Left,
                Input.Left,
                Input.Left,
                Input.Left,
                Input.Left,
                Input.Left,
                Input.Left,
                Input.Left | Input.Jump,
                Input.Left | Input.Jump | Input.Release,
                Input.Left | Input.Release,
                Input.Left,
                Input.Left,
                Input.Left,
                Input.Left,
            };

            for (int i = 0; i < inputs.Length; i++)
            {
                n1 = n1.NewState(inputs[i], Map.CollisionMap);
                //output.WriteLine($"frame {i}\tstate: {n1.State}");
            }

            n1.State.Should().BeEquivalentTo(new PlayerNode(431, 585.89575, 2, true).State);
        }

    }
}
