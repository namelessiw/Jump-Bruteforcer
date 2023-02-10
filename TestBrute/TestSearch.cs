using FluentAssertions;
using Jump_Bruteforcer;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TestBrute
{
    public class SearchTests
    {

        private readonly ITestOutputHelper output;

        public SearchTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData(410, 407.4, 476, 343, "tomo")]
        [InlineData(410, 407.4, 518, 503, "sjump")] 
        [InlineData(419, 407.4, 476, 407, "floor")] 
        [InlineData(452, 407.4, 482, 343, "minif")] 
        [InlineData(410, 407.4, 491, 407, "double")] 
        [InlineData(401, 407.4, 380, 343, "double_plane")] 
        [InlineData(401, 407.4, 413, 263, "45")] 
        [InlineData(410, 407.4, 485, 407, "co")] 
        [InlineData(410, 407.4, 443, 311, "leehe")] 
        [InlineData(409, 407.1, 388, 351, "squished")] 
        [InlineData(410, 407.4, 551, 407, "65")] 
        [InlineData(410, 407.4, 485, 407, "groundex15")]
        [InlineData(389, 407.4, 356, 311, "badl")]
        [InlineData(420, 407.4, 477, 375, "ground_dplane")]
        [InlineData(410, 407.4, 452, 279, "32px")] 
        [InlineData(410, 407.4, 450, 311, "the_stupid")]
        //[InlineData(241, 119.4, 541, 231, "decession")] takes 5 minutes
        public void TestJMaps(int startX, double startY, int goalX, int goalY, string jmapName)
        {
            
            string path = @$"..\..\..\jmaps\{jmapName}.jmap";
            string Text = File.ReadAllText(path);

            Map Map = JMap.Parse(Text);
            Search s = new Search((startX, startY), (goalX, goalY), Map.CollisionMap);
            s.CollisionMap.Count.Should().BeGreaterThan(0);
            SearchResult result = s.RunAStar();
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void TestHeuristicIsAdmissable()
        {
            (int x, int y) evenGoal = (30, 20);
            (int x, int y) oddGoal = (30, 21);
            throw new NotImplementedException();

        }
    }
}