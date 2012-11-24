using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeParser
{
    public class Variable
    {
        public Variable(String pName, Object pValue)
        {
            Name = pName;
            Value=pValue;


        }
        private String _Name;

        public String Name
        {
            get { return _Name; }
            private set { _Name = value; }
        }

        public Object Value;
    }
}
