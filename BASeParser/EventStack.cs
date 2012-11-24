using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeParser
{
    public class EventStack<T> : Stack<T> 
    {
        public delegate bool ItemPrePushPopFunc(ref T itempushed);
        public event ItemPrePushPopFunc ItemPrePush;
        public event ItemPrePushPopFunc ItemPrePop;
        new public void Push(T itempush)
        {
            
            if ((ItemPrePush==null) || !ItemPrePush.Invoke(ref itempush))
            {
                base.Push(itempush);


            }

        
        }
        new public T Pop()
        {
            T returnvalue = base.Pop();
            if((ItemPrePop!=null) && ItemPrePop(ref returnvalue))
            {
                base.Push(returnvalue);

            }
            return returnvalue;
        }

        public EventStack() 
        {
            
            
        }
        public EventStack(IEnumerable<T> collection)
            : base(collection)
        {


        }
        public EventStack(int capacity)
            : base(capacity)
        {



        }

    }
}
