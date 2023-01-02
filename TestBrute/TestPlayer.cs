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
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (10, 5), CollisionType.Solid }
            };

            
            Assert.Equal((endX, endY, vSpeedReset), Player.SolidCollision(collision, startX, targetX, startY, targetY));

        }

        [Theory]
        [InlineData(0, 3, 10, 5, false)] // up right
        [InlineData(6, 3, 10, 5, false)] // up left
        [InlineData(0, 3, 0, 5, false)] // down right
        [InlineData(6, 3, 0, 5, false)] // down left
        public void TestCornerCollision(int startX, int targetX, double startY, double targetY, bool vSpeedReset)
        {
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (targetX, (int)Math.Round(targetY)), CollisionType.Solid }
            };

            Assert.Equal((startX, targetY, vSpeedReset), Player.SolidCollision(collision, startX, targetX, startY, targetY));
        }

        [Theory]
        [InlineData(10, 20, 15, 10, 12, 13, true)] // up right
        [InlineData(10, 0, 15, 10, 12, 13, true)] // up left
        [InlineData(10, 20, 15, 20, 18, 17, true)] // down right
        [InlineData(10, 0, 15, 20, 18, 17, true)] // down left
        [InlineData(419, 422, 406.9, 416.3, 408, 406.9, true)] // down right
        [InlineData(419, 422, 406.5, 414.25, 408, 406.5, true)] // down right vfpi
        [InlineData(419, 422, 407.5, 415.25, 410, 408.5, true)] // down right vfpi
        [InlineData(452, 455, 406.95, 410.325, 408, 406.95, true)] // down right
        [InlineData(452, 455, 406.5, 409.875, 408, 406.5, true)] // down right vfpi
        [InlineData(452, 455, 407.5, 410.875, 410, 408.5, true)] // down right vfpi
        [InlineData(452, 455, 408.5, 411.875, 410, 408.5, true)] // down right vfpi
        public void TestDualCollision(int startX, int targetX, double startY, double targetY, int solidY, double endY, bool vSpeedReset)
        {
            int tY = (int)Math.Round(targetY);
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (targetX, tY), CollisionType.Solid },
                { (startX, solidY), CollisionType.Solid } // snap to this solid
            };

            if (solidY != tY)
            {
                collision.Add((startX, tY), CollisionType.Solid);
            }

            Assert.Equal((targetX, endY, vSpeedReset), Player.SolidCollision(collision, startX, targetX, startY, targetY));
        }
    }
}
