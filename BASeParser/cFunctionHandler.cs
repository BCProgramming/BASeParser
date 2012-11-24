using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeParser
{
   
    public class cFunctionHandler : IEvalPlugin
    {

        #region IEvalPlugin Members
        private readonly string mfunctionmanagementfuncs = " FUNCTION REMFUNCTION ";
        CParser mParser=null;
        public cFunctionHandler(PluginEvent eventdelegate,CParser withparser)
        {
            mParser = withparser;
            EventDelegates = new List<PluginEvent>();
            EventDelegates.Add(eventdelegate);
        }
        public List<PluginEvent> EventDelegates { get; set; }

        public void PluginEventFunc(PluginEventTypeConstants eventtype)
        {
            foreach (PluginEvent loopevent in EventDelegates)
            {
                loopevent.Invoke(this, eventtype);

            }



        }

        public object HandleSubscript(object onvalue, object[] parameters)
        {
            return null;
        }

        public object HandleOperator(string Operator, object OpA, object OpB)
        {
            return null;
        }

        public object HandleUnaryOperation(string Operator, object Operand, operatortypeconstants optype)
        {
            return null;
        }

        public object HandleFunction(string FuncName, List<object> parameters)
        {
            switch (FuncName.ToUpper())
            {
                case "FUNCTION":
                    //parameters: function name, function expression
                    if (parameters.Count() < 2) throw new ArgumentException("Not enough arguments supplied to Function \"Function\"");
                    String funcnameadd=null,funcexpressionadd=null;
                    funcnameadd = (String)parameters[0];
                    funcexpressionadd = (String)parameters[1];
                    //we add a "stub" function name, so that when we create the "actual" function with that name, a recursive call
                    //will be parsed out properly.
                    CFunction TempFunction = new CFunction(mParser, funcnameadd, "0");
                    mParser.Functions.Add(TempFunction);

                    CFunction newfunction = new CFunction(mParser, funcnameadd, funcexpressionadd);
                    //now, we can remove TempFunction and add newfunction.
                    mParser.Functions.Remove(TempFunction);
                    mParser.Functions.Add(newfunction);
                    //PluginEvent(this,PluginEventTypeConstants.PET_FUNCTIONSCHANGED)
                    PluginEventFunc(PluginEventTypeConstants.PET_FUNCTIONSCHANGED);
                    return newfunction;
                case "REMFUNCTION":
                    //parameters: function name to remove.
                    if (parameters.Count() == 0) throw new ArgumentException("Not enough arguments supplied to Function \"RemFunction\"");
                    mParser.Functions.RemoveAll((w) => w.Name.Equals((String)parameters[0], StringComparison.OrdinalIgnoreCase));
                    PluginEventFunc(PluginEventTypeConstants.PET_FUNCTIONSCHANGED);
                    return 0;
                default:
                    foreach (CFunction loopfunction in mParser.Functions)
                    {
                        if (loopfunction.Name.Equals(FuncName, StringComparison.OrdinalIgnoreCase))
                        {
                            return loopfunction.Invoke(parameters.ToArray());


                        }



                    }



                    break;



            }


            return null;
        }

        public object HandleAssignmentOperator(CParser withparser,string Operator, string LValue, CParser RValue)
        {
            return null;
        }

        public object HandleInvocation(object OnObject, string routinename, object[] parameters)
        {
            return null;
        }

        public bool CanHandleFunction(string FuncName, ref bool[] noparsedargs)
        {

            if (FuncName.Equals("FUNCTION", StringComparison.OrdinalIgnoreCase))
            {
                noparsedargs[1] = true;

            }

            return (getHandledFunctions().ToLower().IndexOf(" " + FuncName.ToLower() + " ") != -1);

        }

        public bool IsAssignmentOperator(CParser Withparser, String Operator)
        {
            return false;
        }

        public int CanHandleOperator(string Operator, out operatortypeconstants opconstants)
        {
            opconstants = operatortypeconstants.operator_binary;
            return 0;
        }

        public string getHandledFunctions()
        {
            StringBuilder sbuild = new StringBuilder(mfunctionmanagementfuncs);
            foreach (CFunction funcloop in mParser.Functions)
            {
                sbuild.Append(" " + funcloop.Name + " ");


            }
            return sbuild.ToString();
        }

       
        #endregion

        #region IEvalPlugin Members


        public string getHandledOperators(operatortypeconstants requesttype)
        {
            return "";
        }

        #endregion
    }
}
