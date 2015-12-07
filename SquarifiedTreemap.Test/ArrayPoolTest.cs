using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SquarifiedTreemap.Test
{
    [TestClass]
    public class ArrayPoolTest
    {
        private readonly ArrayPool<int> _pool = new ArrayPool<int>();

        [TestMethod]
        public void AssertThat_AllocatedArray_IsLargeEnough_WhenAllocatingNew()
        {
            Assert.IsTrue(_pool.Allocate(10).Length >= 10);
        }

        [TestMethod]
        public void AssertThat_AllocatedArray_IsLargeEnough_WhenRecycling()
        {
            //Allocate a large enough array
            var a = _pool.Allocate(10);

            //Free it
            _pool.Free(a);

            //Allocate again, let's see if we get a large enough array
            Assert.IsTrue(_pool.Allocate(5).Length >= 5);
        }

        [TestMethod]
        public void AssertThat_FreeingArrayIntoFullPool_DoesNotThrow()
        {
            //Free items into pool, exceeding it's size and thus freeing into a full pool
            Random r = new Random(345);
            for (var i = 0; i < 100; i++)
                _pool.Free(new int[r.Next(i) + 1]);
        }
    }
}
