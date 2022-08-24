using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mitten
{
    [Serializable]
    class InvalidIdException : Exception
    {
        public InvalidIdException()
        {
        }

        public InvalidIdException(string message)
            :base(message)
        {
        }

        public InvalidIdException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
