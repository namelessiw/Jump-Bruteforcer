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
        public void findsGoal(int start_x, double start_y, int goal_x, int goal_y)
        {
            Search s = new Search((start_x, start_y), (goal_x, goal_y));
            (bool success, string InputString) = s.Run();
            string vs = string.Join(";", s.v_string);

            output.WriteLine(InputString);
            Assert.True(success, vs);
        }

        [Fact]
        public void searchResets()
        {
            Search s = new Search((452, 407.4f), (578, 407));
            s.Run();
            Assert.Equal(0, s.currentFrame);
            s.Run();
        }

        [Fact]
        public void GoalUnreachable1()
        {
            Search s = new Search((452, 407.4f), (578, 407));
            (bool success, string InputString) = s.Run();

            Assert.False(success, InputString);
        }

        // probably overkill but could be a performance test of sorts for now ig
        [Fact]
        public void ExtremeGoalUnreachable1()
        {
            Search s = new Search((0, 0), (1, 10000));
            (bool success, string InputString) = s.Run();

            Assert.False(success, InputString);
        }

        // at this point i realized, you should probably be able to search for djump aswell instead of just sjump
        [Fact]
        public void InputTest()
        {
            // currently it finds the highest sjump with the correct vstring for shift inputs
            // may change in the future
            string Expected = "(0) Left, Jump\r\n(21) Release\r\n(26) Neutral\r\n";

            Search s = new Search((401, 407.4f), (323, 343));
            (bool success, string InputString) = s.Run();

            Assert.Equal(InputString, Expected);
        }
    }
}