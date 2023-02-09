using System.Collections.Immutable;
using System.Windows;
using System.Windows.Media;
using FluentAssertions;
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
        public void TestGetTrajectory()
        {
            Player p = new(5);
            List<double> VString = new() { 50.5, 47.3, 43.2, 39.7, 36.3, 32.4, 33.6, 37.3, 41.4 };
            p.MoveNeutral(1);
            p = p.MoveLeft(2);
            p = p.MoveLeft(3);
            Player left = p.MoveLeft(4);
            Player right = p.MoveRight(4);
            p = left.MoveRight(5);
            p = p.MoveRight(6);
            p = p.MoveRight(7);
            p.MoveNeutral(8);

            PointCollection trajectory = new PointCollection(){new Point(5, (int)Math.Round(50.5)),
                new Point(5, (int)Math.Round(47.3)), new Point(2, (int)Math.Round(43.2)), new Point(-1, (int)Math.Round(39.7)),
                new Point(-4, (int)Math.Round(36.3)), new Point(-1, (int)Math.Round(32.4)), new Point(2, (int)Math.Round(33.6)),
                new Point(5, (int)Math.Round(37.3)), new Point(5, (int)Math.Round(41.4)) };

            output.WriteLine(string.Join(";", p.GetTrajectory(VString)));
            output.WriteLine(string.Join(";", trajectory));
            trajectory.Should().Equal(p.GetTrajectory(VString));
        }


        [Fact]
        public void TestGetInputString()
        {
            Player p = new(100);
            p = p.MoveLeft(2);
            p = p.MoveLeft(3);
            p = p.MoveRight(5);
            p.MoveNeutral(8);
            p.GetInputString().Should().Be("(2) Left\r\n(5) Right\r\n(8) Neutral\r\n");
        }

        [Fact]
        public void TestMergeVStringInputs()
        {
            Player p = new(100);
            p = p.MoveLeft(2);
            p = p.MoveLeft(3);
            p = p.MoveRight(5);
            p.MoveNeutral(8);
            SortedDictionary<int, Input> vstringInputs = new() { { 2, Input.Jump }, { 4, Input.Release }, { 5, Input.Release }, { 11, Input.Release } };
            p.MergeVStringInputs(vstringInputs, 10);
            p.GetInputString().Should().Be("(2) Left, Jump\r\n(4) Release\r\n(5) Right, Release\r\n(8) Neutral\r\n");
        }



        [Theory]
        [InlineData(CollisionType.Solid, 10, 10, 7, 5, 10, 6, true)] //upwards
        [InlineData(CollisionType.Solid, 10, 10, 3, 5, 10, 4, true)] //downwards
        [InlineData(CollisionType.Solid, 12, 10, 5, 5, 11, 5, false)] //leftwards
        [InlineData(CollisionType.Solid, 8, 10, 5, 5, 9, 5, false)] //rightwards
        [InlineData(CollisionType.None, 10, 10, 7, 4, 10, 4, false)] //upwards pass through
        [InlineData(CollisionType.None, 10, 10, 3, 6, 10, 6, false)] //downwards pass through
        [InlineData(CollisionType.None, 12, 9, 5, 5, 9, 5, false)] //leftwards pass through
        [InlineData(CollisionType.None, 8, 11, 5, 5, 11, 5, false)] //rightwards pass through
        public void TestSolidCollision(CollisionType ctype, int startX, int targetX, double startY, double targetY, int endX, int endY, bool vSpeedReset)
        {
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (10, 5), CollisionType.Solid }
            };


            Player.CollisionCheck(collision, startX, targetX, startY, targetY).Should().BeEquivalentTo((ctype, endX, endY, vSpeedReset));

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

            Player.CollisionCheck(collision, startX, targetX, startY, targetY).Should().BeEquivalentTo((CollisionType.Solid, startX, targetY, vSpeedReset));
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
        [InlineData(452, 455, 407.5, 410.875, 409, 410.5, true)] // down right vfpi
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

            Player.CollisionCheck(collision, startX, targetX, startY, targetY).Should().BeEquivalentTo((CollisionType.Solid, targetX, endY, vSpeedReset));
        }

        [Fact]
        public void TestFallIntoCorner()
        {
            int startX = 0, targetX = 2, endX = 1;
            double startY = 0, targetY = 2, endY = 1;
            bool vSpeedReset = true;
            Dictionary<(int, int), CollisionType> collision = new()
            {
                { (2, 2), CollisionType.Solid },
                { (0, 2), CollisionType.Solid },
                { (2, 0), CollisionType.Solid },
                { (1, 2), CollisionType.Solid },
                { (2, 1), CollisionType.Solid },
            };

            Player.CollisionCheck(collision, startX, targetX, startY, targetY).Should().BeEquivalentTo((CollisionType.Solid, endX, endY, vSpeedReset));
        }
    }
}
