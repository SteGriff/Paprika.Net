using System;

namespace Paprika.Net.Exceptions
{
    public class PaprikaException : Exception
    {
        public PaprikaException(string message) : base(message)
        { }
    }
}
