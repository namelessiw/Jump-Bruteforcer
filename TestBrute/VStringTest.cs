using Jump_Bruteforcer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBrute
{
    public class VStringTest
    {
        [Fact]
        public void VStringTest1()
        {
            List<VPlayer> VStrings = VPlayer.GenerateVStrings(407.4, true, 327.5);
            Assert.Equal(15, VStrings.Count);
        }

        [Fact]
        public void VStringTest2()
        {
            List<VPlayer> VStrings = VPlayer.GenerateVStrings(407.4, true, 321.3);
            Assert.Equal(2, VStrings.Count);
        }

        [Fact]
        public void VStringTest3()
        {
            List<VPlayer> VStrings = VPlayer.GenerateVStrings(407.4, true, 320);
            Assert.Empty(VStrings);
        }

        [Fact]
        public void VStringTest4()
        {
            List<VPlayer> VStrings = VPlayer.GenerateVStrings(407.4, false, 340);
            Assert.Empty(VStrings);
        }

        [Fact]
        public void VStringTest5()
        {
            List<VPlayer> VStrings = VPlayer.GenerateVStrings(407.4, false, 350);
            Assert.Equal(3, VStrings.Count);
        }

        [Fact]
        public void VStringTest6()
        {
            List<VPlayer> VStrings = VPlayer.GenerateVStrings(407.4, false, 400);
            Assert.Equal(120, VStrings.Count);
        }

        [Fact]
        public void VStringTest7()
        {
            List<VPlayer> VStrings = VPlayer.GenerateVStrings(407.4, false, 550);
            Assert.Equal(127, VStrings.Count);
        }
    }
}
