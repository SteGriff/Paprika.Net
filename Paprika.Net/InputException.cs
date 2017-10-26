using System;

namespace Paprika.Net
{
    public class InputException : PaprikaException
    {
        public InputException(string message) : base(message)
        { }
    }
}
