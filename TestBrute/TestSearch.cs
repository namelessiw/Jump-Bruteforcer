using FluentAssertions;
using Jump_Bruteforcer;
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
    }
}