using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
namespace BASeParser
{
    internal class CoreOpFunc : IEvalPlugin

    {
        private const String HandledFunctionString = " sin cos tan sqr array createobject store log exp round range seq";
        private const String ArrayAwareFunctions = " store "; //array aware functions are functions that can accept an array. otherwise, the array is flattened and the function called
                                                                //with each value as a parameter.
        private const String HandledOperatorString = " <= >= < > == - + * / ^ $$ \\ DIV IN ";
        private const String HandledPostfixOps = " ! ";
        private const String HandledPrefixOps = " - ";
        private const String ArrayOperators = " IN ";
        private List<PluginEvent> mPluginEvents;

        public CoreOpFunc(PluginEvent eventdelegate)
        {
            mPluginEvents = new List<PluginEvent>();
            mPluginEvents.Add(eventdelegate);
        }


        private double factorial(int N)
        {
            if (N < 0) throw new InvalidOperationException("cannot take the factorial of a negative number.");
            if (N == 0) return 1;
            return N * factorial(N - 1);


        }
        public MethodInfo[] FindMethodOverload(object[] Arguments, String name,BindingFlags bf)
        {
            Type[] paramtypes = (from n in Arguments select n.GetType()).ToArray();
            Dictionary<Type, MethodInfo> objectmethods = new Dictionary<Type, MethodInfo>();
            List<MethodInfo> result=new List<MethodInfo>();

            foreach (Type iteratetype in paramtypes)
            {
                //objectmethods.Add(iteratetype, iteratetype.GetMethod(name, paramtypes));
                MethodInfo gotmethod = iteratetype.GetMethod(name,paramtypes);
                if (gotmethod != null) result.Add(gotmethod);


            }

            return result.ToArray();

        }
       

        public bool useOverloadedUnaryOperator(object Operand, String Operator, out Object result)
        {
            if (unaryoperatorlookup == null)
            {
                unaryoperatorlookup = new Dictionary<string, string>();
                //- (Unary), op_UnaryNegation
//+(unary), op_UnaryPlus
//~, op_OnesComplement
                unaryoperatorlookup.Add("-", "op_UnaryNegation");
                unaryoperatorlookup.Add("+", "op_UnaryPlus");
                unaryoperatorlookup.Add("~", "op_OnesComplement");
            }
            if (!unaryoperatorlookup.ContainsKey(Operator))
            {
                result = null;
                return false;

            }
            string lookformethod = unaryoperatorlookup[Operator];

            MethodInfo Searchresult = null;
            MethodInfo[] Searchresults = FindMethodOverload(new object[] { Operand }, lookformethod, BindingFlags.Static);
            if (Searchresults.Length > 0)
                Searchresult = Searchresults[0];


            if (Searchresult != null)
            {

                result = Searchresult.Invoke(null, BindingFlags.Static, null, new object[] { Operand },
                                 System.Threading.Thread.CurrentThread.CurrentCulture);
                return true;
            }


            result = null;
            return false;
        }

        Dictionary<String, String> suffixoperatorlookup = null;
        Dictionary<String, String> unaryoperatorlookup = null; 
        public bool useOverloadedOperator(object OpA, object OpB,String Operator,out object result)
        {

            if (suffixoperatorlookup == null)
            {
                suffixoperatorlookup = new Dictionary<string, string>();
                suffixoperatorlookup.Add("*", "op_Multiply");
                suffixoperatorlookup.Add("-", "op_Subtraction");
                suffixoperatorlookup.Add("+", "op_Addition");
                suffixoperatorlookup.Add("/", "op_Division");
                suffixoperatorlookup.Add("%", "op_Modulus");
                suffixoperatorlookup.Add("^", "op_ExclusiveOr");
                suffixoperatorlookup.Add("&", "op_BitwiseAnd");
                suffixoperatorlookup.Add("|", "op_BitwiseOr");
                suffixoperatorlookup.Add("&&", "op_LogicalAnd");
                suffixoperatorlookup.Add("||", "op_LogicalOr");
                suffixoperatorlookup.Add("<", "op_GreaterThan");
                suffixoperatorlookup.Add(">", "op_LessThan");
                suffixoperatorlookup.Add(">=", "op_GreaterThanOrEqual");
                suffixoperatorlookup.Add("<=", "op_LessThanOrEqual");
                suffixoperatorlookup.Add("==", "op_Equality");
                /*
            

=, op_Assign
<<, op_LeftShift
>>, op_RightShift
*=, op_MultiplicationAssignment
-=, op_SubtractionAssignment
^=, op_ExclusiveOrAssignment
<<=, op_LeftShiftAssignment
%=,op_ModulusAssignment
+=, op_AdditionAssignment
&= op_BitwiseAndAssignment
|= op_BItwiseOrAssignment
,  op_Comma
/=, op_DivisionAssignment
--, op_Decrement
++, op_Increment

                 * */
            }
         
            if (!suffixoperatorlookup.ContainsKey(Operator))
            {
                result = null;
                return false;


            }
            
            String lookformethod = suffixoperatorlookup[Operator];
            /*
            MethodInfo[] OpAMembers = OpAType.GetMethods();
            MethodInfo[] basemembers = OpAType.BaseType.GetMethods();
            MethodInfo[] OpAOperators = OpAType.GetMethods().Where((a) => a.Name.StartsWith(lookformethod)).ToArray();
            MethodInfo[] OpBOperators = OpBType.GetMethods().Where((a) => a.Name.StartsWith(lookformethod)).ToArray();
            */
            MethodInfo Searchresult=null;
            MethodInfo[] Searchresults = FindMethodOverload(new object[] { OpA, OpB }, lookformethod,BindingFlags.Static);
            if (Searchresults.Length > 0)
                Searchresult = Searchresults[0];


            if (Searchresult != null)
            {

                result = Searchresult.Invoke(null, BindingFlags.Static, null, new object[] {OpA, OpB},
                                 System.Threading.Thread.CurrentThread.CurrentCulture);
                return true;
            }


            result = null;
            return false;
        }

        #region IEvalPlugin Members

        public object HandleAssignmentOperator(CParser withparser,string Operator, string LValue, CParser RValue)
        {

            //Three steps:
            
            //if the Operator is not the standard Assignment Operator, =...

            //first, we need to figure out what the assign the value to.
            //for now, we either find or create a Variable.
            Variable usevariable = withparser.Variables[LValue];

            if (Operator == ":=")
            {
                usevariable.Value = RValue.Execute();
            }
            else
            {
                //it must be something like += or -=. Get the Binary operator...
                String binop = Operator.Substring(0, Operator.Length - 1);
                Object OperandA = usevariable.Value;
                Object OperandB = RValue.Execute();
                //perform the binary operation.
                Object result = withparser.HandleOperation(binop, OperandA, OperandB);
                //store it back into the variable.
                usevariable.Value = result;






            }

            return usevariable.Value;



            return null;

        }

        public object HandleOperator(string Operator, object OpA, object OpB)
        {
            Debug.Print("HandleOperator:" + Operator + " OpA:" + OpA + " OpB:" + OpB);

            Type OpAType = OpA.GetType();
            Type OpBType = OpB.GetType();

            MethodInfo[] OpAMembers = OpAType.GetMethods();
            MethodInfo[] basemembers = OpAType.BaseType.GetMethods();
            MethodInfo[] OpAOperators = OpAType.GetMethods().Where((a) => a.Name.StartsWith("op_")).ToArray();
            MethodInfo[] OpBOperators = OpBType.GetMethods().Where((a) => a.Name.StartsWith("op_")).ToArray();

            Object useoverload = null;
            if (useOverloadedOperator(OpA, OpB,Operator, out useoverload))
                return useoverload;


            Object[] ret=null;
            Object loopvalue=null; 
            if (ArrayOperators.IndexOf(Operator,StringComparison.OrdinalIgnoreCase) < 0)
            {
                if (OpA is Object[]) OpA = ((Object[])OpA).ToList();
                if (OpB is Object[]) OpB = ((Object[])OpB).ToList();
                if (OpA is List<Object>)
                {
                    var Enumcast = (IEnumerable<Object>)OpA;
                    var earray = Enumcast.ToArray();
                    ret = new object[Enumcast.Count()];
                    for (int i = 0; i < ret.Count() ; i++)
                    {
                        ret[i] = HandleOperator(Operator, earray[i], OpB);



                    }


                    return ret;

                }
                else if (OpB is List<Object>)
                {

                    var Enumcast = (IEnumerable<Object>)OpB;
                    var earray = Enumcast.ToArray();
                    ret = new object[Enumcast.Count()];
                    for (int i = 0; i < ret.Count(); i++)
                    {
                        ret[i] = HandleOperator(Operator, OpA,earray[i]);



                    }

                    return ret;
                }

            }
            //if either operand is still an array, we know that we are not dealing with
            //an array operator, so we will proceed by recursively calling this routine for each element, and creating a new array as a result.
            if (OpA is Object[] || OpB is Object[])
            {
                
                if (OpA is Object[]) //handles when both are object[] arrays, too.
                {
                    Object[] Castarray = (Object[])OpA;
                    Object[] returnvalues = new object[Castarray.Length];
                    //call for each item, and construct an array to return.
                    for (int i = 0; i < Castarray.Length; i++)
                    {
                        returnvalues[i] = HandleOperator(Operator, Castarray[i], OpB);


                    }
                    return returnvalues;
                }
                else if (OpB is Object[])
                {
                    Object[] Castarray = (Object[])OpB;
                    Object[] returnvalues = new object[Castarray.Length];
                    //call for each item, construct an array and return it.
                    for (int i = 0; i < Castarray.Length; i++)
                    {
                        returnvalues[i] = HandleOperator(Operator, OpA, Castarray[i]);

                    }
                    return returnvalues;
                }


            }
            else
            {



                switch (Operator.ToLower())
                {
                    case "^":
                        return Math.Pow((double)OpA, (double)OpB);

                    case "/":
                        return (double)OpA / (double)OpB;
                    case "\\":
                    case "div":
                        return Math.Floor((double)OpA / (double)OpB);
                    case "in":
                        return ((Object[])OpB).Contains(OpA);
                    case "*":
                        return (double)OpA * (double)OpB;
                    case "==":
                        return OpA.Equals(OpB);
                    case ">=":
                    case "<=":
                    case "<":
                    case ">":
                        IComparable ica = (IComparable)OpA;
                        IComparable icb = (IComparable)OpB;
                        int result = ica.CompareTo(icb);
                        switch (Operator.ToLower())
                        {
                            case ">=": return result != -1;
                            case "<=": return result != 1;
                            case ">": return result == 1;
                            case "<": return result==-1;
                           
                                


                        }
                        return null;
                    case "+":
                        if ((OpA.GetType().Name == "String") || (OpB.GetType().Name == "String"))
                        {
                            return OpA + OpB.ToString();
                        }
                        else
                        {
                            double parseA, parseB;
                            if (double.TryParse(OpA.ToString(), out parseA))
                                if (double.TryParse(OpB.ToString(), out parseB))
                                    return parseA + parseB;
                        }
                        return null;

                    case "-":
                        return (double)OpA - (double)OpB;
                    case "$$":
                        //throw new NotImplementedException();
                        var rx = new Regex((string)OpB);
                        return rx.Match((String)OpA);




                    default:
                        return null;
                }
            }
            return null;
        }
        private CParser GetParserObject()
        {

            foreach (var itemloop in mPluginEvents)
            {
                Object returnvalue = itemloop.Invoke(this, PluginEventTypeConstants.PET_REQUESTPARSER);
                if (returnvalue != null)
                    return (CParser)returnvalue;
            }
            //(CParser)mPluginEvents(this, PluginEventTypeConstants.PET_REQUESTPARSER);
            return null;

        }

        public object HandleFunction(string FuncName, List<object> pparameters)
        {
            Debug.Print("HandleFunction:" + FuncName + "parameters:" + pparameters);
            //convert any Array's within tthis pparameters list into a List...
            List<object> parameters = (from ix in pparameters select ((ix.GetType().IsArray)?((Object[])ix).ToList():ix)).ToList();
            

            if (ArrayAwareFunctions.IndexOf(FuncName, StringComparison.OrdinalIgnoreCase) == -1 &&
                parameters.Any((w)=>w is List<Object>))
            {
                List<Object> parms = parameters;
                List<Object> firstlist=null;
                int firstarrayindex = 0;
                //iterate through the parameters, saving a reference to the first (and ONLY the first) array argument.
                //we only do the first, because the intention is that the function call will recurse on this method and thus
                //have the same logic, so if there are further array arguments, they will be processed in the same manner (since they will
                //eventually be the "first" array in the list of parameters)

                for (int i = 0; i < parms.Count; i++)
                {

                    if (firstlist==null && parms[i] is List<Object> )
                    {

                        firstlist = ((List<Object>)parms[i]);
                        firstarrayindex=i;

                    }


                }
                //Now, using the parms array, create a new list of parameters for each element in that array.

                //iterate through each value in firstarray.
                Object[] Buildreturn = new object[firstlist.Count];
                int a = 0;
                foreach (Object loopparameter in firstlist)
                {
                    //create a new array...
                    List<Object> useparameters = parms;
                    //set the proper index to be the current scalar...
                    useparameters[firstarrayindex] = loopparameter;
                    //call the function.
                    Buildreturn[a] = HandleFunction(FuncName, useparameters);
                    a++;
                }
                return Buildreturn;






            }
            double castdouble;


            switch (FuncName.ToLower())
            {
                case "sin":
                    
                    return Math.Sin((double) Convert.ChangeType(parameters[0],TypeCode.Double));
                    

                case "cos":
                    return Math.Cos((double)Convert.ChangeType(parameters[0], TypeCode.Double));

                    
                case "tan":
                    return Math.Tan((double)Convert.ChangeType(parameters[0], TypeCode.Double));

                case "sqr":
                    return Math.Sqrt((double)Convert.ChangeType(parameters[0], TypeCode.Double));
                case "log":
                    return Math.Log((double)Convert.ChangeType(parameters[0], TypeCode.Double));
                case "exp":
                    return Math.Exp((double)Convert.ChangeType(parameters[0], TypeCode.Double));
                case "round":
                    return Math.Round(
                        (double)Convert.ChangeType(parameters[0],TypeCode.Double), 
                        (int)(Convert.ChangeType(parameters[1],TypeCode.Int32)));
                case "array":
                    return parameters;
                case "range":
                    //start,end,step
                    if (parameters.Count < 2) throw new CParser.ParserSyntaxError(0,"Insufficient arguments to Range function");
                    //type conversion to doubles...
                    double startval = (double)Convert.ChangeType(parameters[0], TypeCode.Double);
                    double endval = (double)Convert.ChangeType(parameters[1], TypeCode.Double);
                    double stepval = parameters.Count > 2?(double)Convert.ChangeType(parameters[2],TypeCode.Double):1;




                    return Range(endval, startval, stepval);

                case "store":
                    CParser parserobject = GetParserObject();
                    

                    //store(variablename,variablevalue)
                    //Variable founditem = parserobject.Variables.Find((w)=>w.Name==parameters[0].ToString());
                    Variable founditem = parserobject.Variables[parameters[0].ToString()];
                    if(founditem!=null)
                    {
                        parserobject.Variables.Remove(founditem);
                       

                    }
                    parserobject.Variables.Add((String)parameters[0], parameters[1]);
                    return parameters[1];
                case "seq":
                    //seq(var,initexpr,incrementexpression,terminateexpression)
                    List<Object> buildresult = new List<object>();
                    CParser parserobj = GetParserObject();
                    Variable foundvar = parserobj.Variables[parameters[0].ToString()];
                    if (foundvar == null) foundvar = parserobj.Variables.Add(parameters[0].ToString(), null);
                    
                    //get the expressions.
                    String initexpression = (String)parameters[1];
                    String incrementExpression = (String)parameters[2];
                    String terminateExpression = (String)parameters[3];

                    CParser Initializer = GetParserObject();
                    Initializer.Expression = initexpression;
                    Initializer.Execute();
                    CParser incrementor = GetParserObject();
                    incrementor.Expression = incrementExpression;
                    CParser terminator = GetParserObject();
                    terminator.Expression = terminateExpression;

                    while (!((bool)terminator.Execute()))
                    {
                        buildresult.Add(foundvar.Value);
                        incrementor.Execute();



                    }

                    return buildresult;

                    break;
                case "createobject":
                    if (parameters.Count() == 0)
                        throw new ArgumentException("CreateObject requires at least 1 argument");
                    
                    object returnobject = CParser.CreateCOMObject((string)parameters[0]);
                    if(returnobject==null)
                    {
                        //returnobject=Activator.CreateInstance(AssemblyName,typeName,Binder,args[],Cultureinfo culture)

                        String lAsmName = (String)parameters[0];
                        String mTypename=(String)parameters[1];
                        object[] constructorargs=parameters.GetRange(2,parameters.Count()-2).ToArray();

                        ObjectHandle objhandle = Activator.CreateInstance(lAsmName, mTypename, true, BindingFlags.CreateInstance, null, constructorargs, CultureInfo.CurrentCulture, null, null);
                        return objhandle.Unwrap();
                    }
                    return null;
                
                default:
                    return null;
            }
        }

        private static object Range(double endval, double startval, double stepval)
        {
            stepval = Math.Abs(stepval)*(Math.Sign(endval - startval));
            //change stepval to go in the correct direction...
            Func<double, double, bool> testfunction;
            if (Math.Sign(stepval) == -1)
                testfunction = ((v, e) => (v <= e));
            else
                testfunction = ((v, e) => (v >= e));

            List<Object> buildlist = new List<object>();
            for (double currvalue = startval; !testfunction(currvalue, endval); currvalue += stepval)
            {
                buildlist.Add(currvalue);
            }
            return buildlist.ToArray();
        }

        public bool CanHandleFunction(string FuncName, ref bool[] noparsedargs)
        {
            //noparsedargs=new bool[
            if(FuncName.Equals("store",StringComparison.OrdinalIgnoreCase))
                noparsedargs[0]=true;
            else if(FuncName.Equals("seq",StringComparison.OrdinalIgnoreCase))
            {
                //seq(var,initexpr,incrementexpression,terminateexpression)
                noparsedargs[0] = true;
                noparsedargs[1] = true;
                noparsedargs[2] = true;
                noparsedargs[3] = true;
               

            }
            return (HandledFunctionString.IndexOf(" " + FuncName.ToLower() + " ") != -1);

        }
        public bool IsAssignmentOperator(CParser Withparser,String Operator)
        {
            if (Operator == ":=") return true;

            if (Operator.EndsWith(":="))
            {
                operatortypeconstants gettype;
                String parseout = Operator.Substring(0, Operator.Length-2);
                if (CanHandleOperator(parseout, out gettype) > 0 && gettype==operatortypeconstants.operator_binary)
                {
                    return true;


                }



            }

            return false;


        }

        public int CanHandleOperator(string Operator, out operatortypeconstants optype)
        {
            bool isassignment = false;

            //determine if it is a assignment expression.
            if (Operator == "=")
            {
                isassignment = true;
                optype = operatortypeconstants.operator_binary; //all assignments are binary
                return int.MaxValue / 2; //really low precedence.
            }
            int binaryops = HandledOperatorString.IndexOf(" " + Operator + " ") + 1;
            int prefixops = HandledPrefixOps.IndexOf(" " + Operator + " ") + 1;
            int postfixops = HandledPostfixOps.IndexOf(" " + Operator + " ") + 1;
            int ret = 0;
            optype = operatortypeconstants.operator_binary;
            if (binaryops != 0)
            {
                optype = operatortypeconstants.operator_binary;
                ret = binaryops;
            }
            if (prefixops != 0)
            {
                optype = (operatortypeconstants) (((int) optype) & (int) (operatortypeconstants.operator_unary_prefix));

                ret += prefixops;
            }
            if (postfixops != 0)
            {
                optype = (operatortypeconstants) (((int) optype) & (int) (operatortypeconstants.operator_unary_postfix));
                ret += postfixops;
            }
            //return +1; so that -1 (not found) becomes 0 and everything else becomes positive. 0 is not found, anything else represents a priority 
            //of the operator. (lower means higher priority) 
            return ret;
        }

        public string getHandledFunctions()
        {
            return HandledFunctionString;
        }

        public string getHandledOperators(operatortypeconstants requesttype)
        {
            StringBuilder buildreturn=new StringBuilder();
            if ((requesttype & operatortypeconstants.operator_binary)==operatortypeconstants.operator_binary )
            {
                buildreturn.Append(HandledOperatorString);
            }
            if ((requesttype & operatortypeconstants.operator_unary_prefix) == operatortypeconstants.operator_unary_prefix)
            {
                buildreturn.Append(" " + HandledPrefixOps);
            }
            if ((requesttype & operatortypeconstants.operator_unary_postfix) == operatortypeconstants.operator_unary_postfix)
            {
                buildreturn.Append(" " + HandledPostfixOps);
            }
            return buildreturn.ToString();
        }

        public List<PluginEvent> EventDelegates
        {
            get { return mPluginEvents; }
            set { mPluginEvents = value; }
        }

        public object HandleUnaryOperation(string Operator, object Operand, operatortypeconstants optype)
        {
            //throw new NotImplementedException();
            Object resultvalue;
            if (useOverloadedUnaryOperator(Operand, Operator, out resultvalue))
                return resultvalue;


            switch (Operator.ToUpper())
            {
                case "-":
                    if(typeof(System.Double) == Operand.GetType())
                    {
                        return -(double)Operand;

                    }
                    else if (typeof(System.String)==Operand.GetType())
                    {
                        return ((String)Operand).Reverse();

                    }

                break;
                case "!":
                return factorial(Convert.ToInt32(Operand));
            }
            return null;
        }


        public object HandleSubscript(object onvalue, object[] parameters)
        {
            //throw new NotImplementedException();
            //Debug.Print("CoreOpFunc 'HandleSubscript' called.");
            //return null;
            if ((typeof (String)).Equals(onvalue.GetType()))
            {
                var onstring = (String) onvalue;
                int index = int.Parse(parameters[0].ToString());
                if (parameters.Length == 1)
                    return onstring[index];
                else
                {
                    int lengthuse = int.Parse(parameters[1].ToString());
                    return onstring.Substring(index, lengthuse);
                }
            }
            else if(onvalue.GetType()== typeof(List<Object>))
            {
                return (Object)(((List<Object>)onvalue)[(int)(double)parameters[0]]);


            }
        else 
            {
                //assume your standard array...
                //object[] test;
                //test.GetValue(int indices[])
                object[] objuse = (object[])onvalue;
                var  indices= new int[parameters.Count()];
                //parameters.Select((w, y) => indices[y] = (int)w );
                for (int i = 0; i < indices.Length; i++)
                    indices[i] = Convert.ToInt32(parameters[i]);
                    

                return objuse.GetValue(indices);



            }
            
        }


        public object HandleInvocation(object OnObject, string routinename, object[] parameters)
        {
            //IEnumerable<MethodInfo> invokethis = from p in (OnObject.GetType().GetMethods())
            //                                     where p.Name.Equals(routinename, StringComparison.OrdinalIgnoreCase)
            //                                     select p;



            Type[] parametertypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parametertypes[i] = parameters[i].GetType();

            }


            MethodInfo match = OnObject.GetType().GetMethod(routinename, parametertypes);

            if (match != null)
            {
                object returnvalue = null;
                returnvalue = match.Invoke(OnObject, parameters);
                if (returnvalue != null)
                    return returnvalue;
                //methodcalled = true;
            }
            if (parameters.Length > 0)
            {
                Object newvalue = parameters[0];
                Object[] indexes;
                Type[] indextypes;
                if (parameters.Length > 1)
                {
                    indexes = new object[parameters.Length - 1];
                    indextypes = new Type[parameters.Length - 1];
                    for (int i = 1; i < parameters.Length; i++)
                    {
                        indexes[i - 1] = parameters[i];
                        indextypes[i - 1] = parameters[i].GetType();
                    }
                }
                else
                {
                    indexes = new object[0];
                    indextypes = new Type[0];
                }
            
                
                PropertyInfo matchprop = OnObject.GetType().GetProperty(routinename,newvalue.GetType(),indextypes);
                if (matchprop != null)
                {
                    Object returnvalue;
                    try
                    {
                        returnvalue = matchprop.GetValue(OnObject, parameters);
                        if (returnvalue != null)
                            return returnvalue;

                    }
                    catch (TargetParameterCountException exx)
                    {
                    }
                    catch (System.Reflection.TargetInvocationException exp)
                    {

                    }



                    matchprop.SetValue(OnObject, newvalue, indexes);
                    return newvalue;
                    
                }

            }

            else if (parameters.Count() == 0)
            {
                IEnumerable<PropertyInfo> objprops = from p in (OnObject.GetType().GetProperties())
                                                     where
                                                         p.Name.Equals(routinename, StringComparison.OrdinalIgnoreCase)
                                                     select p;
                if (objprops.Count() > 0)
                {
                    foreach (PropertyInfo propinfo in objprops)
                    {
                        object returnproperty = propinfo.GetGetMethod().Invoke(OnObject, parameters.ToArray());
                        if (returnproperty != null)
                            return returnproperty;

                    }


                }



            }
            else
            {
                return null;
            }



            return null;
        }

        #endregion
    }
}