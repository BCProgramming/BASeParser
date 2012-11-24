using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
namespace BASeParser
{
    
     /// <summary>represents a single Function object</summary>
     

    //with any luck, this new architecture whereby CFunction derives from MulticastDelegate can make it possible to allow, later on, the use of Functions  in a capacity to handle actual Framework events.
    public class CFunction 
    {

        

        private String mExpression = "";
        private String mName = "";
        
         /// <summary>the expression that comprises this function. references to parameters should be made using the passed "p" array.</summary>
        
        public String Expression { get { return mExpression; } 
            set { 
            if(eventfunction!=null)
                eventfunction(this,Functionchangeeventconstants.expression_changed);
                
                mExpression=value;
            
            
            }

        }

        /// <summary>the name used in expressions to refer to this function.</summary>


        public String Name
        {
            get { return mName; }
            set {
                if (eventfunction != null)
                    eventfunction(this, Functionchangeeventconstants.name_changed);
                
                mName=value;
            
            
            }
        }
        private readonly CParser mParser=null;
        /// <summary>
        /// enumeration of reasons a function can change.
        /// </summary>
        public enum Functionchangeeventconstants
        {
            name_changed=1,
            expression_changed=2


        }
        public delegate void FunctionChangeEvent(CFunction functionchanged,Functionchangeeventconstants eventtype);
        private FunctionChangeEvent eventfunction=null;
        public CFunction(CParser ParentParser,String Functionname):this(ParentParser,Functionname,"")
        {
            
            //mParser = ParentParser.SpawnParser("");
            

        }
        public CFunction(CParser ParentParser,String Functionname, String expression)
        {
            eventfunction = ParentParser.FunctionEvent;
            Name=Functionname;
            VariableList newvariables = ParentParser.Variables;
            newvariables.Add("P", new object[] { });
            mParser = new CParser(expression, ParentParser, newvariables);

        }
        //expression should refer to p[0], p[1], etc to access the parameters passed to it.
        /// <summary>
        /// invokes this function using the given set of parameters.
        /// </summary>
        /// <param name="parameters">the parameters to pass too the functions expression.</param>
        /// <returns>the executed result from the function, or null of an error occurs.</returns>
        public Object Invoke(Object[] parameters)
        {
            //first, assign the parameters variable....
            //mParser.Variables.Find((w)=>w.Key.Equals("P")).Value = parameters;
            //mParser.Variables.RemoveAll((w) => w.Name == "P");
            //mParser.Variables.Add("P",parameters);
            mParser.Variables["P"].Value = parameters;
            return mParser.Execute();
        }
        /// <summary>
        /// invokes the function with an empty set of parameters.
        /// </summary>
        /// <returns>the executed result of the function, or null of an error occurs.</returns>
        public Object Invoke()
        {
            //will only work if the function doesn't take arguments...
            return mParser.Execute();


        }



    }
}
