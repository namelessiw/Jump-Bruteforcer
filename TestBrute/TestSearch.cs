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
        public void FindsGoal(int start_x, double start_y, int goal_x, int goal_y)
        {
            Search s = new((start_x, start_y), (goal_x, goal_y));
            string InputString = s.Run();
            string vs = string.Join(";", s.v_string);

            output.WriteLine(InputString);
            Assert.False(string.IsNullOrEmpty(InputString), vs);
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
            string InputString = s.Run();

            Assert.True(string.IsNullOrEmpty(InputString));
        }

        // probably overkill but could be a performance test of sorts for now ig
        [Fact]
        public void ExtremeGoalUnreachable1()
        {
            Search s = new((0, 0), (1, 1000));
            string InputString = s.Run();

            Assert.True(string.IsNullOrEmpty(InputString));
        }

        // at this point i realized, you should probably be able to search for djump aswell instead of just sjump
        [Fact]
        public void InputTest()
        {
            // currently it finds the highest sjump with the correct vstring for shift inputs
            // may change in the future
            string Expected = "(0) Left, Jump\r\n(21) Release\r\n(26) Neutral\r\n";

            Search s = new((401, 407.4f), (323, 343));
            string InputString = s.Run();

            Assert.Equal(Expected, InputString);
        }
    }
}