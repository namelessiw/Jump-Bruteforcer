using FluentAssertions;
using Jump_Bruteforcer;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using Xunit.Abstractions;
using Xunit.Sdk;

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
            SearchResult result = s.RunAStar();
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void TestHeuristicIsAdmissable()
        {
            (int x, int y) evenGoal = (30, 20);
            (int x, int y) oddGoal = (30, 21);
            throw new NotImplementedException();

        }
    }
}