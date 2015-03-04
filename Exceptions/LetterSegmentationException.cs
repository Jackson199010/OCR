using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MYOCR.Exceptions
{
    public class LetterSegmentationException : ApplicationException
    {
        public LetterSegmentationException(string str) : base(str) { }

        public override string ToString()
        {
            return this.Message;
        }
    }
}
