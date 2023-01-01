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
        //[InlineData(10, 10, 6, 4)] //upwards
        //[InlineData(10, 10, 4, 6)] //downwards
        //[InlineData(11, 10, 5, 5)] //leftwards
        //[InlineData(9, 10, 5, 5)] //rightwards
        [InlineData(10, 10, 6, 5)]
        public void TestSolidCollision(int startX, int endX, double startY, double endY)
        {
            Player p = new(10);
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (10, 5), CollisionType.Solid }
            };

            
            Assert.Equal((p.X_position, 5, true), Player.SolidCollision(collision, startX, endX, startY, endY));

        }
    }
}
