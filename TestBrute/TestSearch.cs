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
        [InlineData(388, 407.4, 541, 407, "platforminvert")] //Frames 51
        [InlineData(399, 487.4, 399, 295, "platform_teleport")] //Frames 17
        [InlineData(399, 487.4, 399, 295, "platform_elevator")] //Frames 27
        //harder ones
        [InlineData(241, 119.4, 541, 231, "decession")] //Frames 229
        [InlineData(753, 567.4, 743, 119, "nameless")] //Frames 577
        [InlineData(379, 566, 115, 147, "needlesatan")] //Frames 921
        [InlineData(113, 407.3, 753, 247, "Ascend screen 1")] //Frames 331
        [InlineData(27, 240, 527, 52, "Ascend screen 2")] //Frames 300
        [InlineData(600, 537, 20, 370, "Ascend screen 3")] //Frames 316
        [InlineData(782, 375.4, 305, 17, "Ascend screen 4")] //Frames 332
        [InlineData(305, 503.4, 785, 148, "Ascend screen 5")] //Frames 355
        [InlineData(17, 151.4, 785, 311, "Ascend screen 6")] //Frames 316
        [InlineData(753, 567.4, 98, 119, "ftfa-1")] //Frames 784
        [InlineData(183, 567.4, 578, 87, "ftfa-2")] //Frames 414
        [InlineData(573, 567.4, 484, 87, "ftfa-3")] //Frames 825
        [InlineData(75, 308, 33, 476, "ctw_ex_inspired")] //Frames 899
        [InlineData(47, 567.4, 47, 370, "uwu1")] //Frames 592
        [InlineData(490, 407.4, 490, 50, "just_for_fun")] //Frames 582
        [InlineData(401, 407.4, 687, 211, "gate")] //Frames 110
        [InlineData(127, 342.85055, 738, 247, "exhopetheendof")] //Frames 221
        [InlineData(401, 407.4, 476, 343, "tomo_2")]  //Frames 37
        [InlineData(58, 535.4, 677, 567, "winter_king")] //Frames 416
        [InlineData(49, 567.4, 765, 567, "winter_king_2")] //Frames 297
        [InlineData(17, 343, 179, 471, "ex_rz")] //Frames 107
        [InlineData(17, 119, 259, 535, "ex_hades")] //Frames 603
        [InlineData(49, 87.4, 762, 567, "e_2")]  //Frames 1195
        [InlineData(125, 119.4, 680, 46, "subset")] //Frames 991
        [InlineData(49, 567, 771, 231, "i_wanna_x")] //Frames 1103
        [InlineData(114, 119, 389, 606, "anticlimax_second_screen")] //Frames 624
        [InlineData(17, 311, 733, 119, "drown_drones")] //Frames 746
        [InlineData(49, 567, 775, 567, "aod")] //Frames 867
        [InlineData(441, 209, 790, 530, "delphi1")] //Frames 804
        [InlineData(20, 567.4, 692, 182, "delphi2")] //Frames 848
        [InlineData(20, 567.4, 692, 182, "delphi2_modified")] //Frames 848
        [InlineData(50, 567.4, 51, 60, "forties")] //Frames 609
        [InlineData(49, 503, 52, 320, "I_wanna_D_L")] //Frames 980
        [InlineData(96, 450, 770, 435, "kae1325")] //Frames 1002
        [InlineData(686, 562, 599, 485, "hard2timarc")] //Frames 1020
        [InlineData(61, 565, 593, 278, "Doner_Goner_Game")] //Frames 867
        [InlineData(393, 406, 780, 182, "i_wanna_lxz")] //Frames 906
        [InlineData(45, 118, 745, 603, "Zeus_recreation")] //Frames 906
        [InlineData(748, 4, 51, 573, "Zeus_recreation_2")] //Frames 717
        [InlineData(793, 84, 768, 566, "rapeechscreen1")] //Frames 986
        [InlineData(6, 563, 746, 31, "rapeechscreen2")] //Frames 1172

        //might not be solvable for the program right now
        /*
        [InlineData(376, 407.4, 598, 407, "needle_extremity_2_ex_2")] //works with epsilon=50
        [InlineData(344, 311.4, 485, 315, "needle_extremity_2_1")] //works with epsilon=50
        [InlineData(440, 407.4, 551, 412, "needle_extremity_2_2")]  //works with epsilon=50
        [InlineData(434, 407.4, 509, 407, "needle_extremity_2_6")] //works with epsilon=50
        [InlineData(401, 407.4, 587, 407, "needle_extremity_2_7")] //works with epsilon=50
        [InlineData(49, 567.4, 702, 279, "32px_precision")]
        [InlineData(49, 87.4, 752, 423, "quadruple_no_vfpi")]
        */

        public void TestJMaps(int startX, double startY, int goalX, int goalY, string jmapName)
        {
            
            string path = @$"..\..\..\jmaps\{jmapName}.jmap";
            string Text = File.ReadAllText(path);

            Map Map = JMap.Parse(Text);
            Search s = new Search((startX, startY), (goalX, goalY), Map.CollisionMap);
            SearchResult result = s.RunAStar();
            result.Success.Should().BeTrue();
            output.WriteLine(result.ToString());
        }

        [Fact]
        public void TestHeuristicIsAdmissable()
        {
            (int x, int y) evenGoal = (30, 20);
            (int x, int y) oddGoal = (30, 21);
            Search evenS = new Search((0, 0), evenGoal, new CollisionMap(null, null));
            Search oddS = new Search((0, 0), oddGoal, new CollisionMap(null, null));
            evenS.Distance(new PlayerNode(30, 20, 0), evenGoal).Should().Be(0);
            evenS.Distance(new PlayerNode(30, 29.5, 0), evenGoal).Should().Be(1);
            evenS.Distance(new PlayerNode(30, 29, 0), evenGoal).Should().Be(1);
            evenS.Distance(new PlayerNode(30, 30, 0), evenGoal).Should().Be(2);

            evenS.Distance(new PlayerNode(30, 10.5, 0), evenGoal).Should().Be(1);
            evenS.Distance(new PlayerNode(30, 11, 0), evenGoal).Should().Be(1);
            evenS.Distance(new PlayerNode(30, 10, 0), evenGoal).Should().Be(2);

            throw new NotImplementedException();

        }
    }
}