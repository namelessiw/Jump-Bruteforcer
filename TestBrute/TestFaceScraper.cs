using FluentAssertions;
using Jump_Bruteforcer;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace TestBrute
{
    public class TestFaceScraper
    {
        private readonly ITestOutputHelper output;

        public TestFaceScraper(ITestOutputHelper output)
        {
            if (Application.Current == null) //https://stackoverflow.com/a/14224558
                new Application();
            this.output = output;
        }

        [Fact]
        public void TestNoInputs()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(49, 23, 0);

            // 36 frames until stable
            for (int i = 0; i < 36; i++)
            {
                v = v.NewState(Input.Neutral, cmap);
            }

            v.State.Should().BeEquivalentTo(new PlayerNode(49, 215.4, 0).State);
        }

        [Fact]
        public void TestNoInputsFaceScraper()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(49, 23, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);

            // 35 frames until stable

            for (int i = 0; i < 35; i++)
            {
                v = v.NewState(Input.Neutral, cmap);
            }

            v.State.Should().BeEquivalentTo(new PlayerNode(49, 218.4, 0, flags: Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);
        }
    }
}
