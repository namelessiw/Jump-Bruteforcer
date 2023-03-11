using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FluentAssertions;
using Jump_Bruteforcer;
using Priority_Queue;

namespace TestBrute
{
    public class TestPlayerNode
    {
        [Theory]
        [InlineData(1,2,3, 3, false)]
        [InlineData(0,0,0, 0, true)]
        [InlineData(0,0,0,1, false)]
        public void TestNodeEquals(int x, double y, double vSpeed, double hSpeed, bool shouldEqual)
        {
            var n1 = new PlayerNode(0,0,0, 0);
            var n2 = new PlayerNode(x, y, vSpeed, hSpeed);
            (n1.Equals(n2)).Should().Be(shouldEqual);

        }

       

    }
}
