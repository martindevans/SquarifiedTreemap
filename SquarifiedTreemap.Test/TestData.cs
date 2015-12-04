using SquarifiedTreemap.Model;

namespace SquarifiedTreemap.Test
{
    public class TestData
        : ITreemapNode
    {
        public readonly string Value;

        public float? Area { get; }

        public TestData(string name, float? area = null)
        {
            Value = name;
            Area = area;
        }
    }
}
