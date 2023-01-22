using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Jump_Bruteforcer;
using Priority_Queue;

namespace TestBrute
{
    public class TestPlayerNode
    {
        [Theory]
        [InlineData(1,2,3, false)]
        [InlineData(0,0,0, true)]
        public void TestNodeEquals(int x, double y, double vSpeed, bool shouldEqual)
        {
            var n1 = new PlayerNode(0,0,0);
            var n2 = new PlayerNode(x, y, vSpeed);
            (n1.Equals(n2)).Should().Be(shouldEqual);

        }

        [Fact]
        public void TestQueueContains()
        {
            var n1 = new PlayerNode(0, 0, 0);
            var n2 = new PlayerNode(0, 0, 0);
            var n3 = new PlayerNode(1, 0, 0);
            var q = new SimplePriorityQueue<PlayerNode, float>();
            q.Enqueue(n1, 1f);
            q.Contains(n2).Should().BeTrue();
            q.Contains(n3).Should().BeFalse();

        }


    }
}
