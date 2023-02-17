using FluentAssertions;
using Jump_Bruteforcer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            cmap.Platforms.Should().BeInAscendingOrder(x => x.instanceNum);

        }
        [Fact]
        public void TestPlatformTeleport()
        {

            string path = @$"..\..\..\jmaps\platform_teleport.jmap";
            string Text = File.ReadAllText(path);
            Map Map = JMap.Parse(Text);
            CollisionMap cmap = Map.CollisionMap;
            cmap.Platforms.Should().HaveCountGreaterThan(0);
            cmap.Platforms.Should().BeInAscendingOrder(x => x.instanceNum);
            (int x, int y) = (384, 384);
            CollisionMap.GetTouchingPlatforms(x, y);

        }

    }
}
