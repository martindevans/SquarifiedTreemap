namespace SquarifiedTreemap
{
    /// <summary>
    /// A small pool of arrays, this save constantly allocating and disposing arrays for scratch space.
    /// A future implementation of this could allocate an array up front and return slices
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ArrayPool<T>
    {
        private int _count;
        private readonly T[][] _pool = new T[16][];

        public T[] Allocate(int count)
        {
            if (count > 0)
            {
                //Find the first large enough non-null item and return it
                for (var i = 0; i < _pool.Length; i++)
                {
                    if (_pool[i] != null && _pool[i].Length >= count)
                    {
                        var arr = _pool[i];

                        //Remove item from pool
                        _pool[i] = null;
                        _count--;

                        return arr;
                    }
                }
            }

            return new T[count];
        }

        public void Free(T[] arr)
        {
            if (_count == _pool.Length)
            {
                //pool is full, try to replace a smaller item in the pool
                for (var i = 0; i < _pool.Length; i++)
                {
                    if (_pool[i].Length < arr.Length)
                    {
                        _pool[i] = arr;
                        return;
                    }
                }
            }
            else
            {
                //Pool is not full, replace first null item
                for (var i = 0; i < _pool.Length; i++)
                {
                    if (_pool[i]  == null)
                    {
                        _pool[i] = arr;
                        _count++;
                        return;
                    }
                }
            }
        }
    }
}
