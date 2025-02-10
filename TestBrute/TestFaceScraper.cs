﻿using FluentAssertions;
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

            for (int i = 0; i < 34; i++)
            {
                v = v.NewState(Input.Neutral, cmap);
            }

            v.State.Should().BeEquivalentTo(new PlayerNode(49, 218.4000000000001, 0.4, flags: Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);

            v = v.NewState(Input.Neutral, cmap);

            v.State.Should().BeEquivalentTo(new PlayerNode(49, 218.4000000000001, 0, flags: Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScraperLeftRightInputs()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(88, 218.3, 0, Bools.CanDJump | Bools.FaceScraper);

            v = v.NewState(Input.Right, cmap);

            v.State.Should().BeEquivalentTo(new PlayerNode(91, 218.3, 0, flags: Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);

            v = v.NewState(Input.Left, cmap);

            v.State.Should().BeEquivalentTo(new PlayerNode(88, 218.3, 0, flags: Bools.CanDJump | Bools.FaceScraper).State);
        }

        [Fact]
        public void TestNoInputsFaceScraperFacingLeft()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(49, 23, 0, Bools.CanDJump | Bools.FaceScraper);

            // 35 frames until stable

            for (int i = 0; i < 36; i++)
            {
                v = v.NewState(Input.Neutral, cmap);
            }

            v.State.Should().BeEquivalentTo(new PlayerNode(49, 218.4000000000001, 0, flags: Bools.CanDJump | Bools.FaceScraper).State);
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

            v.State.Should().BeEquivalentTo(new PlayerNode(44, 218.2, 0, flags: Bools.CanDJump | Bools.FaceScraper).State);
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

        [Fact]
        public void TestFaceScrapeFacingLeftTurningRightHitSpike()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(279, 163, 9.4, Bools.CanDJump | Bools.FaceScraper);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v = v.NewState(Input.Right, cmap);
            Player.IsAlive(cmap, v).Should().BeFalse();

            v = new PlayerNode(276, 163, 9.4, Bools.CanDJump | Bools.FaceScraper);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v = v.NewState(Input.Right, cmap);
            Player.IsAlive(cmap, v).Should().BeFalse();
        }

        [Fact]
        public void TestFaceScrapeFacingLeftTurningRightMissSpike()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(279, 165, 9.4, Bools.CanDJump | Bools.FaceScraper);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v = v.NewState(Input.Right, cmap);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v.State.Should().BeEquivalentTo(new PlayerNode(279, 165, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);

            v = new PlayerNode(276, 165, 9.4, Bools.CanDJump | Bools.FaceScraper);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v = v.NewState(Input.Right, cmap);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v.State.Should().BeEquivalentTo(new PlayerNode(276, 165, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScrapeFacingRightHitboxEdge()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(280, 164, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            v.State.Should().BeEquivalentTo(new PlayerNode(280, 164, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);
            Player.IsAlive(cmap, v).Should().BeTrue();

            v = new PlayerNode(281, 164, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            v.State.Should().BeEquivalentTo(new PlayerNode(281, 164, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);
            Player.IsAlive(cmap, v).Should().BeFalse();
        }

        [Fact]
        public void TestFaceScrapeFacingRightTurningLeftHitSpike()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(328, 164, 9.4, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v = v.NewState(Input.Left, cmap);
            Player.IsAlive(cmap, v).Should().BeFalse();

            v = new PlayerNode(331, 164, 9.4, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v = v.NewState(Input.Left, cmap);
            Player.IsAlive(cmap, v).Should().BeFalse();
        }

        [Fact]
        public void TestFaceScrapeFacingRightTurningLeftMissSpike()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(328, 165, 9.4, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v = v.NewState(Input.Left, cmap);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v.State.Should().BeEquivalentTo(new PlayerNode(328, 165, 0, Bools.CanDJump | Bools.FaceScraper).State);

            v = new PlayerNode(331, 165, 9.4, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v = v.NewState(Input.Left, cmap);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v.State.Should().BeEquivalentTo(new PlayerNode(331, 165, 0, Bools.CanDJump | Bools.FaceScraper).State);
        }

        [Fact]
        public void TestFaceScrapeFacingLeftTurningRightNoWallClip()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(275, 172.4, 9.4, Bools.CanDJump | Bools.FaceScraper);
            v = v.NewState(Input.Right, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(275, 181.8, 9.4, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScrapeFacingRightTurningLeftNoWallClip()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(332, 163, 9.4, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            v = v.NewState(Input.Left, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(332, 172.4, 9.4, Bools.CanDJump | Bools.FaceScraper).State);
        }

        [Fact]
        public void TestFaceScraperFacingLeftOnGroundTurningRight()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(250, 217.2, 0, Bools.CanDJump | Bools.FaceScraper);
            v = v.NewState(Input.Right, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(253, 217.6, 0.4, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScraperFacingRightOnGroundTurningLeft()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(250, 218.2, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            v = v.NewState(Input.Left, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(247, 218.2, 0, Bools.CanDJump | Bools.FaceScraper).State);
        }

        [Fact]
        public void TestFaceScraperFacingLeftJumpRight()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(250, 217.4, 0, Bools.CanDJump | Bools.FaceScraper);
            v = v.NewState(Input.Right | Input.Jump, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(253, 210.8, -6.6, Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScraperFacingRightJumpLeft()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(253, 218.4, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            v = v.NewState(Input.Left | Input.Jump, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(250, 210.3, -8.1, Bools.CanDJump | Bools.FaceScraper).State);
        }

        [Fact]
        public void TestFaceScraperFacingRightJumpLeft1PxAboveGround()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(253, 217.4, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            v = v.NewState(Input.Left | Input.Jump, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(250, 210.8, -6.6, Bools.FaceScraper).State);
        }

        [Fact]
        public void TestFaceScraperFacingRightSwitchCostume()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(250, 218.4, 0, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(250, 218.4, 0, Bools.FaceScraper | Bools.FacingRight).State);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(250, 218.4, 0, Bools.CanDJump | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScraperFacingLeftSwitchCostume()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(250, 217.4, 0, Bools.CanDJump | Bools.FaceScraper);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(250, 217.8, 0.4, Bools.FaceScraper).State);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(250, 217.8, 0, Bools.CanDJump).State);
        }

        [Fact]
        public void TestRegularFacingLeftSwitchCostumeOnGround()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(390, 55.4, 0, Bools.CanDJump);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(390, 55.4, 0, Bools.None).State);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(390, 55.8, 0.4, Bools.FaceScraper).State);
        }

        [Fact]
        public void TestRegularFacingRightSwitchCostumeOnGround()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(390, 55.4, 0, Bools.CanDJump | Bools.FacingRight);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(390, 55.4, 0, Bools.FacingRight).State);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(390, 55.8, 0.4, Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestCostumeSwitchDuringMovement()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(390, 55.4, 0, Bools.CanDJump);
            v = v.NewState(Input.Right, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(393, 55.4, 0, Bools.CanDJump | Bools.FacingRight).State);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(396, 55.4, 0, Bools.FacingRight).State);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(396, 55.8, 0.4, Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScraperJumpBeforeCostumeSwitch()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(390, 55.4, 0, Bools.CanDJump | Bools.FacingRight);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(390, 55.4, 0, Bools.FacingRight).State);
            v = v.NewState(Input.Jump, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(390, 47.3, -8.1, Bools.CanDJump | Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScraperJumpAfterCostumeSwitch()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(390, 55.4, 0, Bools.CanDJump | Bools.FacingRight);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(390, 55.4, 0, Bools.FacingRight).State);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(390, 55.8, 0.4, Bools.FaceScraper | Bools.FacingRight).State);
            v = v.NewState(Input.Jump, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(390, 56.6, 0.8, Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScraperInputsDuringCostumeSwitch()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(369, 215.2, 0, Bools.CanDJump);
            v = v.NewState(Input.Jump, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(369, 207.1, -8.1, Bools.CanDJump).State);
            v = v.NewState(Input.Facescraper | Input.Release | Input.Right, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(369, 199.4, -7.7, Bools.None).State);
            v = v.NewState(Input.Release, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(369, 196.335, -3.065, Bools.FaceScraper).State);
        }

        //image_xscale and image_angle get desynced so we get a new state for the hitbox which is not accounted for so this fails
        [Fact]
        public void TestSwitchToFaceScraperSuccess()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(378, 56.6, 0.8, Bools.None);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(378, 57.8, 1.2, Bools.None).State);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(378, 57.8, 0, Bools.FaceScraper | Bools.CanDJump).State);
        }

        [Fact]
        public void TestSwitchToFaceScraperFail()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(378, 57.8, 1.2, Bools.None);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(378, 59.4, 1.6, Bools.None).State);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(378, 61.4, 2, Bools.None).State);
        }

        [Fact]
        public void TestSwitchToRegularFail()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(381, 45.6, 0, Bools.FaceScraper);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(381, 46, 0.4, Bools.FaceScraper).State);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(381, 46.8, 0.8, Bools.FaceScraper).State);
        }

        [Fact]
        public void TestSwitchToRegularSuccess()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(381, 47, 0.4, Bools.FaceScraper);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(381, 47.8, 0.8, Bools.FaceScraper).State);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(381, 49, 1.2, Bools.None).State);
        }

        [Fact]
        public void TestHitboxChangeAfterSwitch()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(378, 205, 1, Bools.None);
            v = v.NewState(Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(378, 206.4, 1.4, Bools.None).State);
            Player.IsAlive(cmap, v).Should().BeTrue();
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(378, 208.2, 1.8, Bools.FaceScraper).State);
            Player.IsAlive(cmap, v).Should().BeFalse();
        }

        // just gonna not worry about this for now
        /*
        [Fact]
        public void TestCorrectHitboxFacingLeft()
        {
            string path = @$"..\..\..\instance_maps\rHell1.txt";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".txt", Text);
            CollisionMap cmap = Map.CollisionMap;

            // bottom right pixel
            var v = new PlayerNode(476, 58.3, 0, Bools.FaceScraper | Bools.CanDJump);
            v = v.NewState(Input.Left, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(473, 58.3, 0, Bools.FaceScraper | Bools.CanDJump).State);
            v = v.NewState(Input.Left, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(472, 58.3, 0, Bools.FaceScraper | Bools.CanDJump).State);

            // top right missing pixels
            v = new PlayerNode(538, 262, -5.4, Bools.FaceScraper | Bools.CanDJump);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(538, 261, 0, Bools.FaceScraper | Bools.CanDJump).State);

            v = new PlayerNode(537, 262, -5.4, Bools.FaceScraper | Bools.CanDJump);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(537, 260, 0, Bools.FaceScraper | Bools.CanDJump).State);

            v = new PlayerNode(536, 262, -5.4, Bools.FaceScraper | Bools.CanDJump);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(536, 259, 0, Bools.FaceScraper | Bools.CanDJump).State);

            v = new PlayerNode(535, 262, -5.4, Bools.FaceScraper | Bools.CanDJump);
            v = v.NewState(Input.Neutral, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(535, 257, -5, Bools.FaceScraper | Bools.CanDJump).State);
        }
        */

        //broken testcase
        //[Fact]
        //public void TestCorrectHitboxFacingRight()
        //{
        //    string path = @$"..\..\..\instance_maps\rHell1.txt";
        //    string Text = File.ReadAllText(path);
        //    Map Map = Parser.Parse(".txt", Text);
        //    CollisionMap cmap = Map.CollisionMap;

        //    // bottom right pixel
        //    var v = new PlayerNode(533, 218.4, 0, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v.State.Should().BeEquivalentTo(new PlayerNode(533, 218.4, 0, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump).State);

        //    v = new PlayerNode(532, 218.4, 0, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v.State.Should().BeEquivalentTo(new PlayerNode(532, 218.8, 0, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump).State);

        //    v = new PlayerNode(531, 218.4, 0, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v.State.Should().BeEquivalentTo(new PlayerNode(531, 220.8, 1.2, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump).State);

        //    // top right missing pixels
        //    v = new PlayerNode(543, 262, -5.4, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v.State.Should().BeEquivalentTo(new PlayerNode(543, 261, 0, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump).State);

        //    v = new PlayerNode(542, 262, -5.4, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v.State.Should().BeEquivalentTo(new PlayerNode(542, 260, 0, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump).State);

        //    v = new PlayerNode(533, 262, -5.4, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v.State.Should().BeEquivalentTo(new PlayerNode(533, 260, 0, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump).State);

        //    v = new PlayerNode(532, 262, -5.4, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v.State.Should().BeEquivalentTo(new PlayerNode(532, 259, 0, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump).State);

        //    v = new PlayerNode(531, 262, -5.4, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump);
        //    v = v.NewState(Input.Neutral, cmap);
        //    v.State.Should().BeEquivalentTo(new PlayerNode(531, 257, -5, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump).State);
        //}

        [Fact]
        public void TestKale1Macro()
        {
            string path = @$"..\..\..\jmaps\4_07_rKale1.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".jmap", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(371, 529.57925, 3.86675, Bools.FacingRight);
            v = v.NewState(Input.Right, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(374, 533.846, 4.26675, Bools.FacingRight).State);
            v = v.NewState(Input.Right | Input.Facescraper, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(377, 538.51275, 4.66675, Bools.FacingRight).State);
            v = v.NewState(Input.Left, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(374, 543.5795, 5.06675, Bools.None).State);
        }

        // skipping this one for now as well
        /*
        [Fact]
        public void TestFaceScraperFacingLeftBottomRightExtraPixel()
        {
            string path = @$"..\..\..\jmaps\4_07_rKale1.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".jmap", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(254, 427.1, 0, Bools.FaceScraper);
            Player.IsAlive(cmap, v).Should().BeTrue();

            v = new PlayerNode(254, 428.1, 0, Bools.FaceScraper);
            Player.IsAlive(cmap, v).Should().BeFalse();
        }
        */

        [Fact]
        public void TestFaceScraperFacingLeftTurningRightJumpOutsideOfBlock()
        {
            string path = @$"..\..\..\jmaps\4_07_rKale1.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".jmap", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(372, 538.6566, 4.6144875, Bools.FaceScraper | Bools.CanDJump);
            v = v.NewState(Input.Jump | Input.Right, cmap);
            Player.IsAlive(cmap, v).Should().BeFalse();
            // v.State.Should().BeEquivalentTo(new PlayerNode(375, 532.0566, -6.6, Bools.FaceScraper | Bools.FacingRight).State);
        }

        [Fact]
        public void TestFaceScraperFacingLeftTurningRightJumpInsideOfBlock()
        {
            string path = @$"..\..\..\jmaps\4_05_rBlood1.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".jmap", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(698, 391.32, 0, Bools.FaceScraper);
            v = v.NewState(Input.Jump | Input.Right, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(701, 383.22, -8.1, Bools.FaceScraper | Bools.FacingRight | Bools.CanDJump).State);
        }

        [Fact]
        public void TestFaceScraperFacingRightTurningLeftJumpInsideOfBlock()
        {
            string path = @$"..\..\..\jmaps\4_05_rBlood1.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".jmap", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(630, 505.4, 0, Bools.FaceScraper | Bools.FacingRight);
            v = v.NewState(Input.Jump | Input.Left, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(627, 497.3, -8.1, Bools.FaceScraper | Bools.CanDJump).State);
        }

        [Fact]
        public void TestFaceScraperFacingLeftNotInWall()
        {
            string path = @$"..\..\..\jmaps\4_05_rBlood1.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".jmap", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(631, 570.3, 0, Bools.FaceScraper | Bools.CanDJump);
            v = v.NewState(Input.Jump, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(631, 562.2, -8.1, Bools.FaceScraper | Bools.CanDJump).State);
        }

        [Fact]
        public void TestFaceScraperFacingRightNotInWall()
        {
            string path = @$"..\..\..\jmaps\4_05_rBlood1.jmap";
            string Text = File.ReadAllText(path);
            Map Map = Parser.Parse(".jmap", Text);
            CollisionMap cmap = Map.CollisionMap;

            var v = new PlayerNode(680, 570.3, 0, Bools.FaceScraper | Bools.CanDJump | Bools.FacingRight);
            v = v.NewState(Input.Jump, cmap);
            v.State.Should().BeEquivalentTo(new PlayerNode(680, 562.2, -8.1, Bools.FaceScraper | Bools.CanDJump | Bools.FacingRight).State);
        }
    }
}
