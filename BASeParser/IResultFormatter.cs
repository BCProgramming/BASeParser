using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeParser
{
    //ResultFormatter interface; a collection of these is hosted by the CParser object and are used with the "resultToString" method.
    public interface IResultFormatter
    {
        string Formatresult(Object resultvalue);
    }
}
