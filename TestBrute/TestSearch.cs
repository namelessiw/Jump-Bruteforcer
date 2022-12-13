using System.Diagnostics;
using Jump_Bruteforcer;
using Xunit;
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
            bool success = s.Run();
            String vs = string.Join(";", s.v_string);
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
            Assert.False(s.Run());
        }
    }
}