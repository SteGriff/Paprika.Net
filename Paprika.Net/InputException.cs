using System;

namespace Paprika.Net
{
    public class InputException : Exception
    {
        public InputException(string message) : base(message)
        { }
    }
}
