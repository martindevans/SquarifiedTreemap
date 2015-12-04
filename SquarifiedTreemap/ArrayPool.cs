namespace SquarifiedTreemap
{
    internal class ArrayPool<T>
    {
        public T[] Allocate(int count)
        {
            return new T[count];
        }

        public void Free(T[] arr)
        {
        }
    }
}
