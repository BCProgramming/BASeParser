using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeParser.Expandos
{
    //an "expando" object is really just a dynamic object whose methods and properties can be added to dynamically.
    //Just like normal objects, it has a set of properties, and a set of methods.
    public class expandoProperty
    {
        public class PropertyEventArgs : EventArgs
        {
            public Object newValue { get; set; }
            public Object oldValue { get; set; }
            public String Name { get; private set; }
            public PropertyEventArgs(String EventName, Object pnewValue, Object poldValue)
            {
                Name = EventName;
                newValue = pnewValue;
                oldValue = poldValue;


            }
        }
        public delegate void PropertyEvent(expandoProperty sender, PropertyEventArgs args);
        //public delegate void PropertyReturnEvent(expandoProperty sender,PropertyEventArgs args,out object returnvalue);
        public event PropertyEvent PropertyBeforeAssign;
        public event PropertyEvent PropertyAfterAssign;
        public event PropertyEvent PropertyGet;
        private object mValue;
        public String Name { get; set; }
        public Object Value
        {
            get
            {
                PropertyEventArgs eventobj = new PropertyEventArgs("PropertyGet", mValue, mValue);
                PropertyGet.Invoke(this, eventobj);
                mValue = eventobj.newValue;
                return mValue;
            }
            set
            {
                Object oldvalue = mValue;
                PropertyEventArgs sendargs = new PropertyEventArgs("PropertyBeforeAssign", value, mValue);
                PropertyBeforeAssign.Invoke(this, sendargs);
                mValue = sendargs.newValue;
                PropertyAfterAssign.Invoke(this, new PropertyEventArgs("PropertyAfterAssign", sendargs.newValue, sendargs.oldValue));

            }
        }
        public Type PropertyType
        {
            get
            {
                if (this.Value == null)
                    return null;
                else
                    return this.Value.GetType();
            }

        }
        interface iExpandoObject
        {

        }
    }
}
