
namespace SagaNetwork.Models
{
    [GenerateApi(UClassType.Struct)]
    public class IntRange
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public IntRange () { }
        public IntRange (int min, int max)
        {
            MinValue = min;
            MaxValue = max;
        }
    }
}
