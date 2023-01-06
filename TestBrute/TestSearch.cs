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
            Assert.True(r.Success, vs);
        }

        [Fact]
        public void SearchResets()
        {
            Search s = new((452, 407.4f), (578, 407));
            s.Run();
            Assert.Equal(0, s.CurrentFrame);
        }

        [Fact]
        public void GoalUnreachable1()
        {
            Search s = new((452, 407.4f), (578, 407));
            SearchResult r = s.Run();

            Assert.True(string.IsNullOrEmpty(r.InputString));
            Assert.False(r.Success);
        }

        // probably overkill but could be a performance test of sorts for now ig
        [Fact]
        public void ExtremeGoalUnreachable1()
        {
            Search s = new((0, 0), (1, 1000));
            SearchResult r = s.Run();

            Assert.True(string.IsNullOrEmpty(r.InputString));
            Assert.False(r.Success);
        }

        [Fact]
        public void InputTest()
        {
            string Expected = "(1) Left, Jump\r\n(17) Release\r\n(19) Release\r\n(27) Neutral\r\n";

            Search s = new((401, 407.4f), (323, 343));
            string InputString = s.Run().InputString;

            Assert.Equal(Expected, InputString);
        }
    }
}