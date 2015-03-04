using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MYOCR.Exceptions
{
    public class BorderException : ApplicationException
    {
        public BorderException(string str) : base(str) { }
        public override string ToString()
        {
            return this.Message;
        }
    }
}
