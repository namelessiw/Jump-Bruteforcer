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
        public void findsGoal(int start_x, double start_y, int goal_x, int goal_y)
        {
            Search s = new Search((start_x, start_y), (goal_x, goal_y));
            bool success = s.Run();
            String vs = string.Join(";", s.v_string);
            Assert.True(success, vs);


        }
    }
}