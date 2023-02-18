using FluentAssertions;
using Jump_Bruteforcer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xunit.Abstractions;

namespace TestBrute
{
    public class TestCollisionMap
    {
        private readonly ITestOutputHelper output;
        public TestCollisionMap(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestPlatforms()
        {

            string path = @$"..\..\..\jmaps\platform.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            CollisionMap cmap = Map.CollisionMap;
            cmap.Platforms.Should().HaveCountGreaterThan(0);


        }
        [Fact]
        public void TestPlatformTeleport()
        {

            string path = @$"..\..\..\jmaps\platform_teleport.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            CollisionMap cmap = Map.CollisionMap;
            cmap.Platforms.Should().HaveCountGreaterThan(0);
            cmap.Platforms.Should().BeInDescendingOrder(x => x.X);
            (int x, int y) = (384, 350);
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
            Map Map = JMap.Parse(Text);
            CollisionMap cmap = Map.CollisionMap;
            cmap.Platforms.Should().HaveCountGreaterThan(0);
            cmap.Platforms.Should().BeInAscendingOrder(x => x.X);
            (int x, int y) = (384, 350);
            int minInstanceNum = 0;
            Jump_Bruteforcer.Object? platform = cmap.GetCollidingPlatform(x, y, minInstanceNum);
            platform.Should().NotBeNull();
            platform.Y.Should().Be(328);
            platform = cmap.GetCollidingPlatform(x, 354, minInstanceNum);
            platform.Should().NotBeNull();
            platform.Y.Should().Be(328);
            platform = cmap.GetCollidingPlatform(x, 355, minInstanceNum);
            platform.Should().NotBeNull();
            platform.Y.Should().Be(336);

        }


    }
}
