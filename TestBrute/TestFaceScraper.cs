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
        public void TestNoInputsRegular()
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
        public void TestNoInputsFaceScraperFacingRight()
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

        [Fact]
        public void TestNoInputsFaceScraperFacingLeft()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(49, 23, 0, Bools.CanDJump | Bools.FaceScraper);

            // 36 frames until stable

            for (int i = 0; i < 36; i++)
            {
                v = v.NewState(Input.Neutral, cmap);
            }

            v.State.Should().BeEquivalentTo(new PlayerNode(49, 217.4, 0, flags: Bools.CanDJump | Bools.FaceScraper).State);
        }

        [Fact]
        public void TestFaceScraperRunIntoWallFacingRight()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(79, 218.4, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);

            double x_previous;
            do
            {
                x_previous = v.State.X;
                v = v.NewState(Input.Right, cmap);
            }
            while (v.State.X != x_previous);

            v.State.Should().BeEquivalentTo(new PlayerNode(115, 218.4, 0, flags: Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScraperRunIntoWallFacingLeft()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(79, 217.4, 0, Bools.CanDJump | Bools.FaceScraper);

            double x_previous;
            do
            {
                x_previous = v.State.X;
                v = v.NewState(Input.Left, cmap);
            }
            while (v.State.X != x_previous);

            v.State.Should().BeEquivalentTo(new PlayerNode(44, 217.4, 0, flags: Bools.CanDJump | Bools.FaceScraper).State);
        }

        [Fact]
        public void TestFaceScraperBonkCeilingFacingRight()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(115, 74.5, -0.2, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            v = v.NewState(Input.Jump, cmap);

            do
            {
                v = v.NewState(Input.Neutral, cmap);
            }
            while (v.State.VSpeed != 0);

            v.State.Should().BeEquivalentTo(new PlayerNode(115, 36.7, 0, flags: Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScraperBonkCeilingFacingLeft()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(115, 74.5, -0.2, Bools.CanDJump | Bools.FaceScraper);
            v = v.NewState(Input.Jump, cmap);

            do
            {
                v = v.NewState(Input.Neutral, cmap);
            }
            while (v.State.VSpeed != 0);

            v.State.Should().BeEquivalentTo(new PlayerNode(115, 36.7, 0, flags: Bools.FaceScraper).State);
        }

        [Fact]
        public void TestFaceScraperLeftEdgeHitSpikeFacingRight()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(199, 218.1, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            Player.IsAlive(cmap, v).Should().BeFalse();
        }

        [Fact]
        public void TestFaceScraperLeftEdgeHitSpikeFacingLeft()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(203, 218.1, 0, Bools.CanDJump | Bools.FaceScraper);
            Player.IsAlive(cmap, v).Should().BeFalse();
        }

        [Fact]
        public void TestFaceScraperLeftEdgeMissSpikeFacingRight()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(200, 218.1, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            Player.IsAlive(cmap, v).Should().BeTrue();
        }

        [Fact]
        public void TestFaceScraperLeftEdgeMissSpikeFacingLeft()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(204, 218.1, 0, Bools.CanDJump | Bools.FaceScraper);
            Player.IsAlive(cmap, v).Should().BeTrue();
        }
    }
}
