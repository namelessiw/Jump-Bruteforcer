using Jump_Bruteforcer;
using Xunit.Abstractions;

namespace TestBrute
{
    public class TestPlayer
    {
        private readonly ITestOutputHelper output;

        public TestPlayer(ITestOutputHelper output)
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

        [Theory]
        [InlineData(10, 10, 7, 5, 10, 6, true)] //upwards
        [InlineData(10, 10, 3, 5, 10, 4, true)] //downwards
        [InlineData(12, 10, 5, 5, 11, 5, false)] //leftwards
        [InlineData(8, 10, 5, 5, 9, 5, false)] //rightwards
        [InlineData(10, 10, 7, 4, 10, 4, false)] //upwards pass through
        [InlineData(10, 10, 3, 6, 10, 6, false)] //downwards pass through
        [InlineData(12, 9, 5, 5, 9, 5, false)] //leftwards pass through
        [InlineData(8, 11, 5, 5, 11, 5, false)] //rightwards pass through
        public void TestSolidCollision(int startX, int targetX, double startY, double targetY, int endX, int endY, bool vSpeedReset)
        {
            Player p = new(10);
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (10, 5), CollisionType.Solid }
            };

            
            Assert.Equal((endX, endY, vSpeedReset), Player.SolidCollision(collision, startX, targetX, startY, targetY));

        }
    }
}
