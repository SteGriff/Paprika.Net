using System;

namespace Paprika.Net
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
