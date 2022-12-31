using Jump_Bruteforcer;
using Xunit.Abstractions;

namespace TestBrute
{
    public class PlayerTest
    {
        private readonly ITestOutputHelper output;

        public PlayerTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestGetInputString()
        {
            Player p = new(100);
            p = p.MoveLeft(2);
            p = p.MoveLeft(3);
            p = p.MoveRight(5);
            p.MoveNeutral(8);
            Assert.Equal("(2) Left\r\n(5) Right\r\n(8) Neutral\r\n", p.GetInputString());
        }

    }
}
