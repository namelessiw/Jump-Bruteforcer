using Jump_Bruteforcer;
using Xunit;

namespace TestBrute
{
    public class SearchTests
    {
        [Fact]
        public void findsGoal()
        {
            Search s = new Search((0, 0f), (15, 20));
            Assert.True(s.Run());
            Assert.Equal(20, s.currentFrame);
        }

        [Fact]
        public void findsGoal2()
        {
            Search s = new Search((0, 0f), (15, 31));
            Assert.False(s.Run());

        }

        [Fact]
        public void findsGoal3()
        {
            Search s = new Search((0, 0f), (69, 20));
            Assert.False(s.Run());

        }

        [Fact]
        public void findsGoal4()
        {
            Search s = new Search((0, 0f), (69, 20));
            s.Run();
            Assert.Equal(30, s.currentFrame);

        }
    }
}