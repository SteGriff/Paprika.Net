namespace Paprika.Net.Exceptions
{
    public class BracketResolutionException : PaprikaException
    {
        public string Category { get; set; }

        public BracketResolutionException(string message, string category)
            : base(message)
        {
            Category = category;
        }
    }
}
