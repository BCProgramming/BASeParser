using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeParser
{
    [Flags]
    public enum PluginEventTypeConstants
        {
            PET_OPERATORSCHANGED=2,
            PET_FUNCTIONSCHANGED=4,
            PET_REQUESTPARSER=5
        }
    public delegate Object PluginEvent(IEvalPlugin source,PluginEventTypeConstants eventtype);
    [Flags]
    public enum operatortypeconstants
    {
        operator_none=0,
        operator_binary=2,
        operator_unary_prefix=4,
        operator_unary_postfix=8,
        operator_all=14
    }
    public interface IEvalPlugin
    {

       
        List<PluginEvent> EventDelegates { get; set; }
        Object HandleSubscript(Object onvalue, Object[] parameters);
        Object HandleOperator(String Operator, Object OpA, Object OpB);
        Object HandleUnaryOperation(String Operator, Object Operand,operatortypeconstants optype);
        Object HandleFunction(String FuncName, List<Object> parameters);
        object HandleAssignmentOperator(CParser withparser,string Operator, string LValue, CParser RValue);
        Object HandleInvocation(Object OnObject, String routinename, Object[] parameters);
        bool CanHandleFunction(String FuncName,ref bool[] noparsedargs);
        bool IsAssignmentOperator(CParser Withparser, String Operator);
        int CanHandleOperator(String Operator, out operatortypeconstants opconstants);
        String getHandledFunctions();
        String getHandledOperators(operatortypeconstants requesttype);
               
    }
}
