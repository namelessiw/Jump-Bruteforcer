using FluentAssertions;
using Jump_Bruteforcer;
using Xunit.Abstractions;

namespace TestBrute
{
    public class VStringTest
    {
        private readonly ITestOutputHelper output;

        public VStringTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData(407.4, true, 328, 15)]
        [InlineData(407.4, true, 321, 3)]
        [InlineData(407.4, true, 320, 0)]
        [InlineData(407.4, false, 340, 0)]
        [InlineData(407.4, false, 350, 5)]
        [InlineData(407.4, false, 400, 120)]
        [InlineData(407.4, false, 550, 127)]
        public void testVstrings(double start_y, bool single_jump, int lowest_goal, int expected_vs_count)
        {
            List<VPlayer> VStrings = VPlayer.GenerateVStrings(start_y, single_jump, lowest_goal);
            if (VStrings.Count > 0)
            {
                string vs = string.Join(";", VStrings[0].VString);
                output.WriteLine(vs);
                output.WriteLine(VStrings[0].LowestGoal.ToString());
            }
            VStrings.Count.Should().Be(expected_vs_count);
        }

        [Fact]
        public void TestVstringsIncludeInitialPosition()
        {
            (double start_y, bool single_jump, int lowest_goal) = (407.4, true, 321);
            List<VPlayer> VStrings = VPlayer.GenerateVStrings(start_y, single_jump, lowest_goal);
            VStrings[0].VString.Should().Contain(start_y);
            VStrings[1].VString.Should().Contain(start_y);
            VStrings[2].VString.Should().Contain(start_y);
        }
    }
}
