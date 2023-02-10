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
        //easier ones
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
        //harder ones
        //[InlineData(241, 119.4, 541, 231, "decession")] takes 5 minutes
        //[InlineData(753, 567.4, 743, 119, "1")] takes 5 minutes
        //[InlineData(379, 566, 115, 147, "needlesatan")] takes like half an hour
        [InlineData(47, 567.4, 47, 370, "uwu1")]
        [InlineData(490, 407.4, 490, 50, "just_for_fun")]
        [InlineData(401, 407.4, 687, 211, "gate")]
        [InlineData(127, 342.85055, 738, 247, "exhopetheendof")]
        [InlineData(127, 342.85055, 738, 247, "ex_2")]
        [InlineData(401, 407.4, 476, 343, "tomo_2")]
        

        //might not be solvable for the program right now
        //[InlineData(58, 535.4, 677, 567, "winter_king")] 
        //[InlineData(49, 567.4, 765, 567, "winter_king_2")]
        //[InlineData(377, 307.4, 599, 407, "needle_extremity_2_ex_2")]
        //[InlineData(344, 311.4, 485, 311, "needle_extremity_2_1")]
        //[InlineData(440, 407.4, 593, 407, "needle_extremity_2_2")]
        //[InlineData(434, 407.4, 509, 407, "needle_extremity_2_6")]
        //[InlineData(401, 407.4, 587, 407, "needle_extremity_2_7")]
        //[InlineData(113, 407.3, 753, 247, "Ascend_screen_1")]
        //[InlineData(49, 567.4, 702, 279, "32px_precision")]
        //[InlineData(49, 87.4, 752, 423, "quadruple_no_vfpi")]


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