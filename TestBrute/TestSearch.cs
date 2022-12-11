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

        [Fact]
        public void findsGoal()
        {
            Search s = new Search((0, 1f), (15, -20));
            Assert.True(s.Run());
       
        }

        [Fact]
        public void findsGoal2()
        {
            Search s = new Search((0, 0f), (15, -31));
            Assert.True(s.Run());

        }

        [Fact]
        public void findsGoal3()
        {
            Search s = new Search((0, 0f), (69, -20));
            Assert.True(s.Run());

        }

        [Fact]
        public void findsGoal4()
        {
            Search s = new Search((0, 0f), (69, -20));
            Assert.True(s.Run());
            

        }
        [Fact]
        public void findsGoal5()
        {
            Search s = new Search((0, -1f), (15, 20));
            Assert.True(s.Run());

        }

        [Fact]
        public void findsGoal6()
        {
            Search s = new Search((0, -407f), (15, -401));

        
            Assert.True(s.Run());

        }
    }
}