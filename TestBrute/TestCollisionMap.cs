using FluentAssertions;
using Jump_Bruteforcer;
using System.Windows;
using Xunit.Abstractions;

namespace TestBrute
{
    public class TestCollisionMap
    {
        private readonly ITestOutputHelper output;
        public TestCollisionMap(ITestOutputHelper output)
        {
            if (Application.Current == null) //https://stackoverflow.com/a/14224558
                new Application();
            this.output = output;
        }

        [Fact]
        public void TestPlatforms()
        {

            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            CollisionMap cmap = Map.CollisionMap;
            cmap.Platforms.Should().HaveCountGreaterThan(0);


        }
        [Fact]
        public void TestPlatformTeleport()
        {

            string path = @$"..\..\..\jmaps\platform_teleport.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            CollisionMap cmap = Map.CollisionMap;
            cmap.Platforms.Should().HaveCountGreaterThan(0);
            cmap.Platforms.Should().BeInDescendingOrder(x => x.X);
            (int x, int y) = (384, 352);
            int minInstanceNum = 0;
            Jump_Bruteforcer.Object? platform =  cmap.GetCollidingPlatform(x, y, minInstanceNum);
            platform.Should().NotBeNull();
            platform.Y.Should().Be(360);
            platform = cmap.GetCollidingPlatform(x, 349, minInstanceNum);
            platform.Should().NotBeNull();
            platform.Y.Should().Be(352);

        }
        [Fact]
        public void TestPlatformElevator()
        {

            string path = @$"..\..\..\jmaps\platform_elevator.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            CollisionMap cmap = Map.CollisionMap;
            cmap.Platforms.Should().HaveCountGreaterThan(0);
            cmap.Platforms.Should().BeInAscendingOrder(x => x.X);
            (int x, int y) = (384, 350);
            int minInstanceNum = 0;
            Jump_Bruteforcer.Object? platform = cmap.GetCollidingPlatform(x, y, minInstanceNum);
            platform.Should().NotBeNull();
            platform.Y.Should().Be(328);
            platform = cmap.GetCollidingPlatform(x, 355, minInstanceNum);
            platform.Should().NotBeNull();
            platform.Y.Should().Be(328);
            platform = cmap.GetCollidingPlatform(x, 356, minInstanceNum);
            platform.Should().NotBeNull();
            platform.Y.Should().Be(336);

        }

        [Fact]
        public void TestVineDistance()
        {
            string path = @$"..\..\..\jmaps\vineclip.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(Text);
            CollisionMap cmap = Map.CollisionMap;
            cmap.GetVineDistance(172, 247.4, ObjectType.VineLeft, true).Should().Be(VineDistance.CORNER) ;
            cmap.GetVineDistance(172, 248.4, ObjectType.VineLeft, true).Should().Be(VineDistance.EDGE);
            cmap.GetVineDistance(173, 247.4, ObjectType.VineLeft, true).Should().Be(VineDistance.EDGE);
            cmap.GetVineDistance(197, 247.4, ObjectType.VineLeft, true).Should().Be(VineDistance.CORNER);
            cmap.GetVineDistance(197, 248.4, ObjectType.VineLeft, true).Should().Be(VineDistance.EDGE);
            cmap.GetVineDistance(172, 247.4, ObjectType.VineLeft, false).Should().Be(VineDistance.CORNER);
            cmap.GetVineDistance(173, 247.4, ObjectType.VineLeft, false).Should().Be(VineDistance.EDGE);
            cmap.GetVineDistance(199, 247.4, ObjectType.VineLeft, false).Should().Be(VineDistance.CORNER);
            cmap.GetVineDistance(198, 248.4, ObjectType.VineLeft, true).Should().Be(VineDistance.FAR);
            cmap.GetVineDistance(198, 248.4, ObjectType.VineLeft, false).Should().Be(VineDistance.INSIDE);

            cmap.GetVineDistance(154, 151.4, ObjectType.VineRight, true).Should().Be(VineDistance.CORNER);
            cmap.GetVineDistance(155, 151.4, ObjectType.VineRight, true).Should().Be(VineDistance.EDGE);
            cmap.GetVineDistance(179, 151.4, ObjectType.VineRight, true).Should().Be(VineDistance.CORNER);
            cmap.GetVineDistance(154, 151.4, ObjectType.VineRight, false).Should().Be(VineDistance.CORNER);
            cmap.GetVineDistance(155, 151.4, ObjectType.VineRight, false).Should().Be(VineDistance.EDGE);
            cmap.GetVineDistance(181, 151.4, ObjectType.VineRight, false).Should().Be(VineDistance.CORNER);
            cmap.GetVineDistance(180, 152.4, ObjectType.VineRight, true).Should().Be(VineDistance.FAR);
            cmap.GetVineDistance(180, 152.4, ObjectType.VineRight, false).Should().Be(VineDistance.INSIDE);

        }


    }
}
