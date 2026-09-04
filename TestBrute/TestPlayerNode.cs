using System.Collections.Immutable;
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
        [InlineData(1, 2, 3, true, false)]
        [InlineData(0, 0, 0, true, true)]
        [InlineData(0, 0, 0, false, false)]
        public void TestNodeEquals(int x, float y, float vSpeed, bool canDJump, bool shouldEqual)
        {
            var n1 = new PlayerNode(0, 0, 0);
            Bools flags = canDJump ? Bools.CanDJump | Bools.FacingRight : Bools.FacingRight;
            var n2 = new PlayerNode(x, y, vSpeed, flags);
            (n1.Equals(n2)).Should().Be(shouldEqual);

        }

        [Fact]
        public void TestQueueContains()
        {
            var n1 = new PlayerNode(0, 0, 0);
            var n2 = new PlayerNode(0, 0, 0);
            var n3 = new PlayerNode(1, 0, 0);
            var n4 = new PlayerNode(0, 0, 0, Bools.FacingRight);
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
            float playerY = 567.254f;
            int playerYRounded = (int)Math.Round(playerY);
            var n1 = new PlayerNode(450, playerY, 0);

            Dictionary<(int, int), CollisionType> collision = Enumerable.Range(0, 100 * floor_thickness).ToDictionary(x => (x % 100 + 400, x / 100 + playerYRounded + 1), x => CollisionType.Solid);
            CollisionMap cmap = new(
                collision, null);

            PlayerNode n2 = n1.NewState(Input.Left | Input.Jump, cmap);
            PlayerNode n3 = n1.NewState(Input.Left | Input.Jump | Input.Release, cmap);
            PlayerNode n4 = n3.NewState(Input.Right | Input.Jump | Input.Release, cmap);
            PlayerNode n5 = n4.NewState(Input.Release, cmap);
            PlayerNode n6 = n5.NewState(Input.Right | Input.Release, cmap);


            n1.NewState(Input.Left, cmap).State.Should().BeEquivalentTo(new PlayerNode(447, 567.254f, 0, Bools.CanDJump).State);
            n1.NewState(Input.Right, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 567.254f, 0).State);
            
            n2.State.Should().BeEquivalentTo(new PlayerNode(447, 559.154f, -8.1f, Bools.CanDJump).State);
            n1.NewState(Input.Right | Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 559.154f, -8.1f).State);

            n3.State.Should().BeEquivalentTo(new PlayerNode(447, 563.8290000000001f, -3.4250000000000003f, Bools.CanDJump).State);
            n1.NewState(Input.Right | Input.Jump | Input.Release, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 563.8290000000001f, -3.4250000000000003f).State);

            n2.NewState(Input.Neutral, cmap).State.Should().BeEquivalentTo(new PlayerNode(447, 551.454f, -7.699999999999999f, Bools.CanDJump).State);
            n3.NewState(Input.Neutral, cmap).State.Should().BeEquivalentTo(new PlayerNode(447, 560.8040000000001f, -3.0250000000000004f, Bools.CanDJump).State);

            n4.State.Should().BeEquivalentTo(new PlayerNode(450, 561.0790000000001f, -2.75f, Bools.FacingRight).State);
            n3.NewState(Input.Right | Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(450, 557.229f, -6.6f, Bools.FacingRight).State);

            n4.NewState(Input.Neutral, cmap).State.Should().BeEquivalentTo(new PlayerNode(450, 558.729f, -2.35f, Bools.FacingRight).State);
            n4.NewState(Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(450, 558.729f, -2.35f, Bools.FacingRight).State);

            n5.State.Should().BeEquivalentTo(new PlayerNode(450, 560.2415000000001f, -0.8375f, Bools.FacingRight).State);
            n4.NewState(Input.Jump | Input.Release, cmap).State.Should().BeEquivalentTo(new PlayerNode(450, 560.2415000000001f, -0.8375f, Bools.FacingRight).State);

            n6.State.Should().BeEquivalentTo(new PlayerNode(453, 560.2646250000001f, 0.023125000000000007f, Bools.FacingRight).State);

            n6.NewState(Input.Neutral, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 560.6877500000002f, 0.42312500000000003f, Bools.FacingRight).State);
            n6.NewState(Input.Release, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 560.6877500000002f, 0.42312500000000003f, Bools.FacingRight).State);

            PlayerNode n7 = n6;
            for (int i = 0; i < 5; i++)
            {
                n7 = n7.NewState(Input.Neutral, cmap);
            }
            n7.State.Should().BeEquivalentTo(new PlayerNode(453, 566.3802500000002f, 2.023125f, Bools.FacingRight).State);
            n7.NewState(Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 567.3802500000002f, 0).State);
            n7 = n7.NewState(Input.Neutral, cmap);
            n7.NewState(Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(453, 559.2802500000001f, -8.1f).State);
        }

        [Fact]
        public void TestNewStateCorner()
        {
            
            var n1 = new PlayerNode(0, 0, 2, Bools.FacingRight);
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (3, 2), CollisionType.Solid },
                { (1, 2), CollisionType.Solid },
                { (3, 0), CollisionType.Solid },
                { (2, 2), CollisionType.Solid },
                { (3, 1), CollisionType.Solid },
            };
            CollisionMap cmap = new(collision, null);

            n1.NewState(Input.Right | Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(2, 1, 0).State);
            PlayerNode n2 = n1.NewState(Input.Right, cmap);
            n2.State.Should().BeEquivalentTo(new PlayerNode(2, 1, 0).State);
            n2.NewState(Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(2, -7.1f, -8.1f).State);
            var n3 = new PlayerNode(5, 4, -2, Bools.FacingRight);
            n3.NewState(Input.Left, cmap).State.Should().BeEquivalentTo(new PlayerNode(5, 2.4f, -1.6f, Bools.None).State);
        }

        [Theory]
        [InlineData(10, 13, 15, 10, 12, 13, false, true)] // up right
        [InlineData(10, 7, 15, 10, 12, 13, false, false)] // up left
        [InlineData(10, 13, 15, 20, 18, 17, true, true)] // down right
        [InlineData(10, 7, 15, 20, 18, 17, true, false)] // down left
        [InlineData(419, 422, 406.9, 416.3, 408, 406.9, true, true)] // down right
        [InlineData(419, 422, 406.5, 414.25, 408, 406.5, true, true)] // down right vfpi
        [InlineData(419, 422, 407.5, 415.25, 410, 408.5, true, true)] // down right vfpi
        [InlineData(452, 455, 406.95, 410.325, 408, 406.95, true, true)] // down right
        [InlineData(452, 455, 406.5, 409.875, 408, 406.5, true, true)] // down right vfpi
        [InlineData(452, 455, 407.5, 410.875, 410, 408.5, true, true)] // down right vfpi
        [InlineData(452, 455, 407.5, 410.875, 409, 410.5, true, true)] // down right vfpi
        [InlineData(452, 455, 408.5, 411.875, 410, 408.5, true, true)] // down right vfpi
        public void TestNewStateDualCollision(int startX, int targetX, float startY, float targetY, int solidY, float endY, bool canJump, bool facingRight)
        {
            int tY = (int)Math.Round(targetY);
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (targetX, tY),  CollisionType.Solid },
                { (startX, solidY), CollisionType.Solid } // snap to this solid
            };

            if (solidY != tY)
            {
                collision.Add((startX, tY), CollisionType.Solid);
            }
            CollisionMap cmap = new(collision, null);

            float vspeed = targetY - startY - PhysicsParams.GRAVITY;
            PlayerNode n1 = new(startX, startY, vspeed, Bools.FacingRight);
            Input input = targetX - startX < 0? Input.Left : Input.Right;
            Bools flags = Bools.None;
            flags |= canJump ? Bools.CanDJump : Bools.None; 
            flags |= facingRight ?  Bools.FacingRight : Bools.None;
            n1.NewState(input, cmap).State.Should().BeEquivalentTo(new PlayerNode(targetX, endY, 0, flags).State);
        }

        [Fact]
        public void TestHorizontalDualCollision()
        {
            string path = @$"..\..\..\jmaps\1_green_5.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(342, 376.845f, 0);
            n1.NewState(Input.Left | Input.Jump, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(341, 371.845f, 0, Bools.None).State);
        }

        [Fact]
        public void TestFallAgainstSolidAndWater()
        {
            string path = @$"..\..\..\jmaps\silent_1.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(440, 192.4081374999986f, 9.4f, Bools.FacingRight);
            n1.NewState(Input.Right, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(442, 201.8081374999986f, 9.4f, Bools.FacingRight).State);
        }

        [Fact]
        public void TestNewStateBHop()
        {
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (0, 568), CollisionType.Solid }
            };
            CollisionMap cmap = new(collision, null);

            PlayerNode n1 = new(0, 567.1f, 0);
            n1 = n1.NewState(Input.Jump | Input.Release, cmap);
            for (int i = 0; i < 17; i++)
            {
                n1 = n1.NewState(Input.Neutral, cmap);
            }
            n1.State.Should().BeEquivalentTo(new PlayerNode(0, 566.6500000000001f, 3.374999999999999f).State);
            n1.NewState(Input.Jump, cmap).State.Should().BeEquivalentTo(new PlayerNode(0, 558.5500000000001f, -8.1f).State);
        }

        [Fact]
        public void TestGetNeighborsNoCollisionNegativeVSpeed()
        {
            Dictionary<(int, int), CollisionType> collision = new();
            CollisionMap cmap = new(collision, null);
                ImmutableArray<Input> inputs = ImmutableArray.Create(Input.Neutral, Input.Left, Input.Right, Input.Jump, Input.Release, Input.Jump | Input.Release, Input.Left | Input.Jump,
                Input.Right | Input.Jump, Input.Left | Input.Release, Input.Right | Input.Release, Input.Left | Input.Jump | Input.Release, Input.Right | Input.Jump | Input.Release);

        PlayerNode n1 = new(400, 400, -1);

            PlayerNode[] players = new PlayerNode[inputs.Length];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = n1.NewState(inputs[i], cmap);
            }

            (from n in n1.GetNeighbors(cmap) select n.Item1).Should().BeEquivalentTo(new HashSet<PlayerNode>(players));

        }
        [Fact]
        public void TestGetNeighborsNoCollisionPositiveVSpeedNoJump()
        {
            Dictionary<(int, int), CollisionType> collision = new();
            CollisionMap cmap = new(collision, null);
            PlayerNode n1 = new(400, 400, 1, Bools.FacingRight);

            PlayerNode[] players = new PlayerNode[PlayerNode.inputs.Length];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = n1.NewState(PlayerNode.inputs[i], cmap);
            }

            (from n in n1.GetNeighbors(cmap) select n.Item1).Should().BeEquivalentTo(new HashSet<PlayerNode>(players));

        }

        [Fact]
        public void TestGetNeighborsNoCollisionPositiveVSpeed()
        {
            Dictionary<(int, int), CollisionType> collision = new();
            CollisionMap cmap = new(collision, null);
            PlayerNode n1 = new(400, 400, 1);

            Input[] inputs =  {Input.Neutral, Input.Left, Input.Right, Input.Jump, Input.Jump | Input.Release, Input.Left | Input.Jump,
                Input.Right | Input.Jump, Input.Left | Input.Jump | Input.Release, Input.Right | Input.Jump | Input.Release };
            PlayerNode[] players = new PlayerNode[inputs.Length];
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = n1.NewState(inputs[i], cmap);
            }

            (from n in n1.GetNeighbors(cmap) select n.Item1).Should().BeEquivalentTo(new HashSet<PlayerNode>(players));

        }

     

        [Fact]
        public void TestGetPath()
        {
            Dictionary<(int, int), CollisionType> collision = new() {
                { (400, 568), CollisionType.Solid },
            };
            CollisionMap cmap = new(collision, null);

            PlayerNode n1 = new(400, 567.1f, 0);
            List<Input> inputs  = new List<Input>() {Input.Left | Input.Jump, Input.Left, Input.Left, Input.Left | Input.Release, Input.Left, Input.Neutral, Input.Neutral,
            Input.Right | Input.Jump | Input.Release, Input.Right | Input.Release, Input.Right | Input.Release, Input.Right, Input.Neutral};
            PointCollection points = new PointCollection() { new Point(400, 567), new Point(397, 559), new Point(394, 551), new Point(391, 544), new Point(388, 541),
            new Point(385, 539), new Point(385, 537), new Point(385, 535), new Point(388, 532), new Point(391, 531), new Point(394, 531), new Point(397, 532), new Point(397, 533)};

            foreach(Input input in inputs)
            {
                n1 = n1.NewState(input, cmap);
            }

            /*(List<Input> Inputs, PointCollection Points)  path = n1.GetPath();
            path.Inputs.Should().Equal(inputs);
            path.Points.Should().Equal(points);*/

        }

        [Fact]
        public void TestIsGoal()
        {
            (int x, int y) goal = (42, 99);
            new PlayerNode(42, 99.3f, 0).IsGoal(goal).Should().BeTrue();
            new PlayerNode(42, 98.9f, 0).IsGoal(goal).Should().BeTrue();

            new PlayerNode(42, 99.6f, 0).IsGoal(goal).Should().BeFalse();

        }

        [Fact]
        public void TestNewStateBonk()
        {
            Dictionary<(int, int), CollisionType> collision = new() {
                { (0, 568), CollisionType.Solid },
                { (0, 560), CollisionType.Solid },
                { (0, 559), CollisionType.Solid },
                { (0, 558), CollisionType.Solid },
                { (0, 557), CollisionType.Solid },
                { (0, 556), CollisionType.Solid},
                { (0, 555), CollisionType.Solid },
                { (0, 554), CollisionType.Solid },
                { (0, 553), CollisionType.Solid },
            };
            CollisionMap cmap = new(collision, null);

            PlayerNode n1 = new(0, 567.1f, 0);
            n1 = n1.NewState(Input.Jump, cmap);
            n1.State.Flags.HasFlag(Bools.CanDJump).Should().BeTrue();
            n1 = n1.NewState(Input.Jump, cmap);
            n1.State.Flags.HasFlag(Bools.CanDJump).Should().BeFalse();

        }

        [Fact]
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
            CollisionMap cmap = new(collision, null);

            PlayerNode n1 = new(0, 567.1f, 0);
            n1 = n1.NewState(Input.Jump, cmap);
            n1.State.Flags.HasFlag(Bools.CanDJump).Should().BeTrue();
            n1 = n1.NewState(Input.Jump, cmap);
            n1.State.Flags.HasFlag(Bools.CanDJump).Should().BeFalse();

        }
        [Fact]
        public void TestDJumpDoesNotRefresh()
        {

            string path = @$"..\..\..\jmaps\dj.jmap";
            string Text = File.ReadAllText(path);
            Input[] inputs = new Input[] {Input.Right, Input.Left, Input.Left, Input.Left, Input.Left, Input.Left | Input.Jump, Input.Neutral};
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(452, 407.4f, 0);

            foreach (Input input in inputs)
            {
                n1 = n1.NewState(input, Map.CollisionMap);
            }
            n1.State.Flags.HasFlag(Bools.CanDJump).Should().BeFalse();
        }

        [Fact]
        public void TestMoveOnPlatform()
        {

            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(394, 375, 0, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight);
            n1.State.Flags.HasFlag(Bools.CanDJump).Should().BeTrue();

            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 375.4f, 0.4f, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 375, 0, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 375.4f, 0.4f, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 375, 0, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);
            n1 = n1.NewState(Input.Left, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(391, 375.4f, 0.4f, Bools.CanDJump | Bools.OnPlatform).State);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 375, 0, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(397, 375.4f, 0.4f, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);

        }

        [Fact]
        public void TestFallInsidePlatform()
        {

            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(394, 396, 0);

            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 396.4f, 0.4f).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 397.2f, 0.8f).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 398.4f, 1.2000000000000002f).State);

        }
        [Fact]
        public void TestMoveOutOfPlatform()
        {

            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(418, 380, 6);

            n1.NewState(Input.Neutral, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(418, 375, 0, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);

            n1.NewState(Input.Right, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(421, 386.4f, 6.4f).State);
            

        }

        [Fact]
        public void TestMoveIntoPlatform()
        {

            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(421, 380, 6);

            n1.NewState(Input.Neutral, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(421, 386.4f, 6.4f).State);

            n1.NewState(Input.Left, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(418, 375, 0, Bools.CanDJump | Bools.OnPlatform).State);


        }

        [Fact]
        public void TestPlatformTripleJump()
        {
            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(394, 375, 0, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight);
            n1.State.Flags.HasFlag(Bools.CanDJump).Should().BeTrue();

            n1 = n1.NewState(Input.Jump | Input.Release, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 371.575f, -3.4250000000000003f, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);
            n1 = n1.NewState(Input.Jump | Input.Release, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 368.15f, -3.4250000000000003f, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);
            n1 = n1.NewState(Input.Jump | Input.Release, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(394, 365.4f, -2.75f, Bools.FacingRight).State);

        }

        [Fact]
        public void TestFallOntoPlatform()
        {
            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(394, 375, 9.4f, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight);
            n1.NewState(Input.Neutral, Map.CollisionMap).State.VSpeed.Should().Be(0);
            

        }

        [Fact]
        public void TestCannotFallThrough()
        {
            string path = @$"..\..\..\jmaps\platform_triple_jump.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(410, 408, 0, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(410, 407.4f, 0.4f, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(410, 407, 0, Bools.CanDJump | Bools.OnPlatform | Bools.FacingRight).State);
        }
        [Fact]
        public void TestSnapCausesOnPlatform()
        {
            string path = @$"..\..\..\jmaps\platform_triple_jump.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(401, 407, 0);
            n1.NewState(Input.Jump | Input.Release, Map.CollisionMap).NewState(Input.Jump, Map.CollisionMap).State.VSpeed.Should().NotBe(PhysicsParams.SJUMP_VSPEED + PhysicsParams.GRAVITY);
        }

        [Fact]
        public void TestCeilingPlatform()
        {
            string path = @$"..\..\..\jmaps\platform_ceiling.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(466, 499, 0, Bools.FacingRight);
            n1.NewState(Input.Neutral, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(466, 499.4f, 0.4f, Bools.FacingRight).State);
            n1 = n1.NewState(Input.Jump, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(466, 492, 0).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(466, 492.4f, 0.4f).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(466, 493.2f, 0.8f).State);
        }

        [Fact]
        public void TestUpsidedownGravity()
        {
            string path = @$"..\..\..\jmaps\gravitytest.txt";
            string Text = File.ReadAllText(path);
            Input[] inputs = new Input[] { Input.Right | Input.Jump, Input.Right, Input.Right, Input.Right, Input.Right, Input.Right, Input.Right, Input.Right, Input.Right, Input.Right, Input.Right, Input.Right | Input.Jump, Input.Right, Input.Right };
            Map Map = Parser.Parse(".txt", Text);
            var n1 = new PlayerNode(49, 567, 0);

            foreach (Input input in inputs)
            {
                n1 = n1.NewState(input, Map.CollisionMap);
            }
            n1.State.VSpeed.Should().Be(-0.4f);
            for (int i = 0; i < 50; i++)
            {
                n1 = n1.NewState(Input.Right, Map.CollisionMap);
            }
            //n1.State.Should().BeEquivalentTo(new PlayerNode(125, 109.50000000000017, -9.4, Bools.CanDJump | Bools.FacingRight | Bools.InvertedGravity).State);
            for (int i = 0; i < 8; i++)
            {
                n1 = n1.NewState(Input.Right, Map.CollisionMap);
            }
            n1.State.Should().BeEquivalentTo(new PlayerNode(149, 40.70000000000015f, 0, Bools.CanDJump | Bools.FacingRight | Bools.InvertedGravity | Bools.ParentInvertedGravity).State);
        }
        [Fact]
        public void TestRecoverDjumpUpsideDown()
        {
            string path = @$"..\..\..\jmaps\gravitytest.txt";
            string Text = File.ReadAllText(path);
         
            Map Map = Parser.Parse(".txt", Text);
            var n1 = new PlayerNode(149, 40.70000000000015f, 0, Bools.FacingRight | Bools.InvertedGravity | Bools.ParentInvertedGravity);

            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(149, 40.70000000000015f, 0, Bools.CanDJump | Bools.FacingRight | Bools.InvertedGravity | Bools.ParentInvertedGravity).State);
        }

        [Fact]
        public void TestVineJumpWhileTurningUpsideDown()
        {
            string path = @$"..\..\..\jmaps\gravflippervineplatform.txt";
            string Text = File.ReadAllText(path);
            Input[] inputs = new Input[] { Input.Jump, Input.Neutral, Input.Neutral, Input.Right, Input.Right };
            Map Map = Parser.Parse(".txt", Text);
            var n1 = new PlayerNode(373, 567, 0);

            foreach (Input input in inputs)
            {
                n1 = n1.NewState(input, Map.CollisionMap);
            }
            n1 = n1.NewState(Input.Left, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(363, 535.1f, 8.6f, Bools.CanDJump | Bools.InvertedGravity | Bools.ParentInvertedGravity).State);
            
        }
        [Fact]
        public void TestVineJumpWhileTurningRightsideUp()
        {
            string path = @$"..\..\..\jmaps\gravflippervineplatform.txt";
            string Text = File.ReadAllText(path);
            
            Map Map = Parser.Parse(".txt", Text);
            var n1 = new PlayerNode(352, 508.69999999999993f, -2, Bools.CanDJump | Bools.InvertedGravity | Bools.ParentInvertedGravity);
            for (int i = 0; i < 9; i++)
            {
                n1 = n1.NewState(Input.Right, Map.CollisionMap);
            }
            n1 = n1.NewState(Input.Left, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(378, 477.0999999999999f, 0.4f, Bools.CanDJump).State);

        }

        [Fact]
        public void TestTurningRightsideUpTouchingPlatform()
        {
            string path = @$"..\..\..\jmaps\gravflippervineplatform.txt";
            string Text = File.ReadAllText(path);

            Map Map = Parser.Parse(".txt", Text);
            var n1 = new PlayerNode(373, 567, 0);
            n1 = n1.NewState(Input.Jump | Input.Left, Map.CollisionMap);
            for (int i = 0; i < 6; i++)
            {
                n1 = n1.NewState(Input.Left, Map.CollisionMap);
            }
            n1.State.Should().BeEquivalentTo(new PlayerNode(352, 518.6999999999999f, -5.6999999999999975f, Bools.CanDJump | Bools.InvertedGravity).State);
            for (int i = 0; i < 5; i++)
            {
                n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            }
            n1.State.Should().BeEquivalentTo(new PlayerNode(352, 508.69999999999993f, -2, Bools.CanDJump | Bools.InvertedGravity | Bools.ParentInvertedGravity).State);

        }
        [Fact]
        public void TestTurningUpsideDownOnPlatform()
        {
            string path = @$"..\..\..\jmaps\gravflippervineplatform.txt";
            string Text = File.ReadAllText(path);
            
            Map Map = Parser.Parse(".txt", Text);
            var n1 = new PlayerNode(373, 567, 0);
            n1 = n1.NewState(Input.Jump | Input.Left, Map.CollisionMap);
            for (int i = 0; i < 3; i++)
            {
                n1 = n1.NewState(Input.Left, Map.CollisionMap);
            }
            n1.State.Should().BeEquivalentTo(new PlayerNode(361, 537, -6.899999999999999f, Bools.CanDJump).State);
            n1 = n1.NewState(Input.Release | Input.Left, Map.CollisionMap);
            for (int i = 0; i < 2; i++)
            {
                n1 = n1.NewState(Input.Left, Map.CollisionMap);
            }
            n1.State.Should().BeEquivalentTo(new PlayerNode(352, 530.085f, -1.9049999999999998f, Bools.CanDJump | Bools.InvertedGravity).State);
            n1 = n1.NewState(Input.Jump, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(352, 535, 0, Bools.CanDJump | Bools.OnPlatform | Bools.InvertedGravity | Bools.ParentInvertedGravity).State);

        }

        [Fact]
        public void TestTurningRightsideUpOnPlatform()
        {
            string path = @$"..\..\..\jmaps\gravflippervineplatform.txt";
            string Text = File.ReadAllText(path);

            Map Map = Parser.Parse(".txt", Text);
            var n1 = new PlayerNode(355, 478.0850000000001f, -6.000000000000001f, Bools.CanDJump | Bools.InvertedGravity | Bools.ParentInvertedGravity);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Left, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(352, 457.6850000000001f, -7.200000000000002f, Bools.CanDJump | Bools.ParentInvertedGravity).State);
            n1 = n1.NewState(Input.Jump, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(352, 462.0850000000001f, 0.4f, Bools.CanDJump).State);
            PlayerNode n2 = n1.NewState(Input.Jump, Map.CollisionMap);
            n2.State.Should().BeEquivalentTo(new PlayerNode(352, 453.98500000000007f, -8.1f, Bools.CanDJump).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Jump, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(352, 447.0850000000001f, -7.699999999999999f, Bools.CanDJump).State);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(352, 439, 0, Bools.CanDJump | Bools.OnPlatform).State);
        }

        [Fact]
        public void TestTurningUpsidedownInWater()
        {
            string path = @$"..\..\..\jmaps\gravflippervineplatform.txt";
            string Text = File.ReadAllText(path);

            Map Map = Parser.Parse(".txt", Text);
            var n1 = new PlayerNode(445, 535.16f, -1.685f, Bools.CanDJump | Bools.FacingRight);
            for (int i = 0; i < 2; i++)
            {
                n1 = n1.NewState(Input.Right, Map.CollisionMap);
            }
            n1.State.Should().BeEquivalentTo(new PlayerNode(451, 532.99f, -0.8850000000000001f, Bools.CanDJump | Bools.FacingRight | Bools.InvertedGravity).State);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(454, 528.59f, -0.4f, Bools.CanDJump | Bools.FacingRight | Bools.InvertedGravity | Bools.ParentInvertedGravity).State);
            for (int i = 0; i < 6; i++)
            {
                n1 = n1.NewState(Input.Right, Map.CollisionMap);
            }
            n1.State.Should().BeEquivalentTo(new PlayerNode(472, 518.19f, -2, Bools.CanDJump | Bools.FacingRight | Bools.InvertedGravity | Bools.ParentInvertedGravity).State);

        }
        [Fact]
        public void TestTurningRightsideUpWhileCollidingWithSolid()
        {
            string path = @$"..\..\..\jmaps\room773.txt";
            string Text = File.ReadAllText(path);

            Map Map = Parser.Parse(".txt", Text);
            var n1 = new PlayerNode(634, 152.16981250000063f, 7.448687500000003f, Bools.CanDJump );
            
            
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(634, 160.01850000000064f, 7.848687500000003f, Bools.CanDJump | Bools.InvertedGravity).State);
            n1 = n1.NewState(Input.Jump | Input.Right, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(634, 162.61850000000064f, 6.6f, Bools.FacingRight | Bools.InvertedGravity | Bools.ParentInvertedGravity).State);

        }

        [Fact]
        public void TestUpsidedownHitSpike()
        {
            string path = @$"..\..\..\jmaps\upsidedownhitspike.txt";
            string Text = File.ReadAllText(path);

            Map Map = Parser.Parse(".txt", Text);
            var n1 = new PlayerNode(485, 187.81249999999996f, -1.6485000000000003f, Bools.FacingRight | Bools.InvertedGravity | Bools.ParentInvertedGravity);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            Player.IsAlive(n1).Should().BeFalse();

        }
        // requires multiple object types per pixel
        [Fact]
        public void TestNabla2()
        {
            string path = @$"..\..\..\jmaps\nabla_2.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(479, 592.8957f, 8.8f, Bools.FacingRight);

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

            n1.State.Should().BeEquivalentTo(new PlayerNode(431, 585.8956999999999f, 2, Bools.CanDJump).State);
        }

        [Fact]
        public void TestNabla2PlatformSpikeDeath()
        {
            string path = @$"..\..\..\jmaps\nabla_2.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(741, 105.66000000000003f, -6.199999999999999f);

            Input[] inputs = new Input[]
            {
                Input.Jump | Input.Release | Input.Left,
            };

            for (int i = 0; i < inputs.Length; i++)
            {
                n1 = n1.NewState(inputs[i], Map.CollisionMap);
                //output.WriteLine($"frame {i}\tstate: {n1.State}");
            }
            n1.Should().BeNull();
        }

        [Fact]
        public void TestNabla2NoPlatformSnap()
        {
            string path = @$"..\..\..\jmaps\nabla_2.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(744, 111.86000000000003f, -6.6f, Bools.None);

            n1 = n1.NewState(Input.Left, Map.CollisionMap);

            n1.State.Should().BeEquivalentTo(new PlayerNode(741, 105.66000000000003f, -6.199999999999999f, Bools.None).State);
        }


        [Fact]
        public void TestVineClip()
        {

            string path = @$"..\..\..\jmaps\vineclip.jmap";
            string Text = File.ReadAllText(path);

            Map Map = Parser.Parse(Text);
            var n1 = new PlayerNode(165, 247.4f, 0);
            n1 = n1.NewState(Input.Jump, Map.CollisionMap);
            for (int i = 0; i < 6; i++)
                 n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1.State.Should().BeEquivalentTo(new PlayerNode(165, 203.8f, 0).State);
            n1.NewState(Input.Left, Map.CollisionMap).State.Should().BeEquivalentTo(new PlayerNode(150, 195.20000000000002f, -8.6f, Bools.CanDJump).State);


        }



    }
}
