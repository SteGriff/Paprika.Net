namespace Paprika.Net
{
    public class IntRange
    {
        public int LowerBound { get; set; }
        public int UpperBound { get; set; }

        public IntRange (int lower, int upper)
        {
            LowerBound = lower;
            UpperBound = upper;
        }
    }
}
