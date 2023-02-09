using FluentAssertions;
using Jump_Bruteforcer;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using Xunit.Abstractions;

namespace TestBrute
{
    public class SearchTests
    {

        private readonly ITestOutputHelper output;

        public SearchTests(ITestOutputHelper output)
        {
            this.output = output;
        }




        [Theory]
        [InlineData(0, -1f, 15, 10)]
        [InlineData(0, -1f, 15, -10)]
        [InlineData(452, 407.4f, 575, 403)]
        [InlineData(452, 407, 452, 407)]
        public void FindsGoal(int start_x, double start_y, int goal_x, int goal_y)
        {
            Search s = new((start_x, start_y), (goal_x, goal_y));
            SearchResult r = s.Run();
            string vs = string.Join(";", s.v_string);

            output.WriteLine(r.InputString);
            r.Success.Should().BeTrue(vs);
        }

        [Fact]
        public void SearchResets()
        {
            Search s = new((452, 407.4f), (578, 407));
            s.Run();
            s.CurrentFrame.Should().Be(0);
        }

        [Fact]
        public void GoalUnreachable1()
        {
            Search s = new((452, 407.4f), (578, 407));
            SearchResult r = s.Run();

            r.InputString.Should().BeNullOrEmpty();
            r.Success.Should().BeFalse();
        }

        // probably overkill but could be a performance test of sorts for now ig
        [Fact]
        public void ExtremeGoalUnreachable1()
        {
            Search s = new((0, 0), (1, 1000));
            SearchResult r = s.Run();

            r.InputString.Should().BeNullOrEmpty();
            r.Success.Should().BeFalse();
        }

        [Fact]
        public void InputTest()
        {
            string Expected = "(0) Left, Jump\r\n(16) Release\r\n(18) Release\r\n(26) Neutral\r\n";

            Search s = new((401, 407.4f), (323, 343));
            string InputString = s.Run().InputString;

            InputString.Should().Be(Expected);
        }

        [Fact]
        public void TestMiniF()
        {
            PlayerNode n1 = new PlayerNode(452, 407.4, 0);
            
            string path = @$"..\..\..\minif.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            Input[] inputs = new Input[] { Input.Right | Input.Jump | Input.Release,Input.Right,Input.Right,Input.Right | Input.Release,
                Input.Right,Input.Right,Input.Right,Input.Right,Input.Right, Input.Right, Input.Right | Input.Jump,
                Input.Left,Input.Left,Input.Left,Input.Left,Input.Left,Input.Left,Input.Left,Input.Left,Input.Left,
                Input.Left,Input.Left,Input.Left,Input.Left,Input.Left,Input.Left,Input.Left,
            };
            List<PlayerNode> needToVisit= new List<PlayerNode>() { n1};
            foreach (Input input in inputs)
            {
                n1 = n1.NewState(input, Map.CollisionMap);
                needToVisit.Add(n1);
            }
            n1.State.X.Should().Be(482);
            ((int)Math.Round(n1.State.Y)).Should().Be(343);

            var s = new Search((452, 407.4), (482, 343));
            s.CollisionMap = Map.CollisionMap;
            HashSet<PlayerNode> visited = s.RunAStar();
            foreach(PlayerNode n in needToVisit)
            {
                visited.Should().Contain(n);
            }

        }

        [Theory]
        [InlineData(410, 407.4, 476, 343, "tomo")]
        [InlineData(410, 407.4, 518, 503, "sjump")] 
        [InlineData(419, 407.4, 476, 407, "floor")] 
        [InlineData(452, 407.4, 482, 343, "minif")] 
        [InlineData(410, 407.4, 491, 407, "double")] 
        [InlineData(401, 407.4, 380, 343, "double_plane")] 
        [InlineData(401, 407.4, 413, 263, "45")] 
        [InlineData(410, 407.4, 485, 407, "co")] 
        [InlineData(410, 407.4, 443, 311, "leehe")] 
        [InlineData(410, 407.1, 338, 351, "squished")] 
        [InlineData(410, 407.4, 551, 407, "65")] 
        [InlineData(410, 407.4, 485, 407, "groundex15")]
        [InlineData(389, 407.4, 356, 311, "badl")]
        [InlineData(420, 407.4, 477, 375, "ground_dplane")]
        [InlineData(410, 407.4, 452, 279, "32px")] 
        [InlineData(410, 407.4, 450, 311, "the_stupid")] 
        public void TestJMaps(int startX, double startY, int goalX, int goalY, string jmapName)
        {
            
            string path = @$"..\..\..\jmaps\{jmapName}.jmap";
            string Text = File.ReadAllText(path);

            Map Map = JMap.Parse(Text);
            Search s = new Search((startX, startY), (goalX, goalY), Map.CollisionMap);
            s.CollisionMap.Count.Should().BeGreaterThan(0);
            s.RunAStar();
            s.Strat.Should().Contain("Frames");
        }
        [Fact]
        public void TestGroundDPlane()
        {
            PlayerNode n1 = new PlayerNode(420, 407.4, 0);
            string path = @$"..\..\..\ground_dplane.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            n1 = n1.NewState(Input.Right | Input.Jump | Input.Release, Map.CollisionMap);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1 = n1.NewState(Input.Right | Input.Release, Map.CollisionMap);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1 = n1.NewState(Input.Right, Map.CollisionMap);
            n1 = n1.NewState(Input.Left | Input.Jump, Map.CollisionMap);
            n1 = n1.NewState(Input.Left, Map.CollisionMap);
            n1 = n1.NewState(Input.Left, Map.CollisionMap);


            for (int i = 0; i < 13; i++)
            {
                n1 = n1.NewState(Input.Right, Map.CollisionMap);
            }
            for (int i = 0; i < 10; i++)
            {
                n1 = n1.NewState(Input.Neutral, Map.CollisionMap);
            }
            n1.State.X.Should().Be(477);
            ((int)Math.Round(n1.State.Y)).Should().Be(375);
        }
    }
}