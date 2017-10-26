using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paprika.Net
{
    public class PaprikaException : Exception
    {
        public PaprikaException(string message) : base(message)
        { }
    }
}
