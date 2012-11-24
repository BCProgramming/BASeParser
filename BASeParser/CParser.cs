using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;
namespace BASeParser
{
    

    public class CParser : ICorePlugin,IResultFormatter
    {
        public enum ParseStateConstants
        {
            ParseState_Idle,
            ParseState_Parsing,
            ParseState_IdleParsed,
            ParseState_Executing,
            ParseState_ExecuteComplete



        }
            
        public class EvaluationError : ApplicationException
        {
            public CFormItem formitem { get; set; }
            public String Description { get; set; }
            
            public override string Message
            {
                get
                {
                    
                      return Description;
                }
            }
            public EvaluationError(CFormItem withitem, String pDescription)
            {
                formitem = withitem;
                Description = pDescription;

            }


        }
        public class ParserSyntaxError : ApplicationException
        {
            
            public int Position { get; set; }
            public String Description { get; set; }
            private String mmessage=null;
            public override String Message
            {
                get
                {
                    //return base.Message;
                    return mmessage;
                }
            

            }
            public ParserSyntaxError(int pPosition, String pDescription)
            {
                Position = pPosition;
                Description = pDescription;
                mmessage = Description + " at position " + Position;
            }





        }
        const Char delimiter = ',';
        //static const String StdOperators = " + - * / ";
        const String starting_brackets = "({[<";
        const String ending_brackets = ")}]>";
        public ParseStateConstants ParseState{get;set;}
        public Object LastResult { get; private set; }
        public List<IResultFormatter> Resultformatters { get; private set; }
        protected LinkedList<CFormItem> mFormulaStack=null;
        //public VariableList Variables { get; set; }
        public VariableList Variables { get; set; }
        private ObservableCollection<ICorePlugin> CorePlugins = null;
        private ObservableCollection<IEvalPlugin> EvalPlugins = null;
        //public delegate void EvaluationStackPushFunc(Object newitem);
        //public event EvaluationStackPushFunc EvaluationStackPush;
        protected Stack<CFormItem> unaryprefixstack = new Stack<CFormItem>();
        public List<CFunction> Functions{get;set;}
        private CoreOpFunc coreplugin{get { 
        foreach(IEvalPlugin loopplugin in EvalPlugins)
        {
            if( loopplugin.GetType() == typeof(CoreOpFunc))
            {
                return (CoreOpFunc)loopplugin; 

            }



        }
            return null;


        } } 
          private cFunctionHandler corefuncplugin{get { 
        foreach(IEvalPlugin loopplugin in EvalPlugins)
        {
            if( loopplugin.GetType() == typeof(cFunctionHandler))
            {
                return (cFunctionHandler)loopplugin; 

            }



        }
            return null;


        } } 
        private String mAllFunctions = null;
        private String mBinOperators = null,mPrefixOperators=null,mPostfixOperators=null;
        private String[] mSplitOperators = null, mSplitFunctions = null;
        private string[] mSplitPrefixOperators=null,mSplitPostfixOperators=null;
        private String[] mlensortoperators = null, mlensortfunctions = null;
        private String mExpression=null;
        public CParser ParentParser { get; protected set; }
        public String Expression 
        { get{
            return mExpression; 
        } set
        {
            mExpression = value;
            mpreviousitem=null;
         mFormulaStack = RipFormula(mExpression);
         buildstack_infix(mFormulaStack);
            
        }  }




        public static object CreateCOMObject(string sProgID)
        {

            return CreateCOMObject(sProgID, "");
        }

        /// <summary>
        /// Creates a COM object given it's ProgID.
        /// </summary>
        /// <param name="sProgID">The ProgID to create</param>
        /// <returns>The newly created object, or null on failure.</returns>
        public static object CreateCOMObject(string sProgID,string servername)
        {
            // We get the type using just the ProgID
            Type oType=null;
            if(servername.Length==0)
                oType = Type.GetTypeFromProgID(sProgID);
            else
            {
                oType = Type.GetTypeFromProgID(sProgID, servername);
            }

            if (oType != null)
                return Activator.CreateInstance(oType);


            return null;

        }



        public Object PluginEvent(IEvalPlugin source, PluginEventTypeConstants eventtype)
        {
            //rebuild the mBinOperators and mAllFunctions strings...
            mBinOperators="";mAllFunctions="";
            if(eventtype==PluginEventTypeConstants.PET_REQUESTPARSER) return this;
            StringBuilder buildops=new StringBuilder(),buildfuncs=new StringBuilder();
            StringBuilder buildprefixops = new StringBuilder(), buildpostfixops = new StringBuilder();
            foreach (IEvalPlugin loopplugin in EvalPlugins)
            {
                buildops.Append(" ");
                buildfuncs.Append(" ");
                buildops.Append(loopplugin.getHandledOperators(operatortypeconstants.operator_binary));
                buildprefixops.Append(loopplugin.getHandledOperators(operatortypeconstants.operator_unary_prefix));
                buildpostfixops.Append(loopplugin.getHandledOperators(operatortypeconstants.operator_unary_postfix));
                buildfuncs.Append(loopplugin.getHandledFunctions());
                buildfuncs.Append(" ");
                buildops.Append(" ");
                buildprefixops.Append(" ");
                buildpostfixops.Append(" ");
            }
            
            
            
            mAllFunctions = buildfuncs.ToString();
            mBinOperators = buildops.ToString();
            mPrefixOperators = buildprefixops.ToString();
            mPostfixOperators = buildpostfixops.ToString();
            //TODO: move the replacement code so that it works on the stringbuilders before converting them to the immutable String type
            while (mAllFunctions.IndexOf("  ") != -1)
                mAllFunctions = mAllFunctions.Replace("  ", " ");
            while (mBinOperators.IndexOf("  ") != -1)
                mBinOperators = mBinOperators.Replace("  ", " ");
            while (mPrefixOperators.IndexOf("  ") != -1)
                mPrefixOperators = mPrefixOperators.Replace("  ", " ");
            while (mPostfixOperators.IndexOf("  ") != -1)
                mPostfixOperators = mPostfixOperators.Replace("  ", " ");
            
            mSplitFunctions = mAllFunctions.Trim().Split(' ');
            mSplitOperators = (" " + mBinOperators.Trim() + " " + mPrefixOperators.Trim() + " " + mPostfixOperators.Trim()+ " ").Trim().Split(' ');
            mSplitBinOperators = mBinOperators.Trim().Split(' ');
            mSplitPrefixOperators = mPrefixOperators.Trim().Split(' ');
            mSplitPostfixOperators = mPostfixOperators.Trim().Split(' ');
            List<String> templist;
            templist = mSplitFunctions.ToList();
            
            templist.Sort((a,b) => b.Length.CompareTo(a.Length));
            mlensortfunctions=templist.ToArray();

            templist = mSplitOperators.ToList();
            templist.Sort((a,b) => b.Length.CompareTo(a.Length));

            mlensortoperators = templist.ToArray();

            return null;


        }
        protected void MakeParsable(ref String expression)
        {
            foreach (String Op in mSplitOperators)
            {

                expression = expression.Replace(Op, " " + Op + " ");

            }

            while (expression.IndexOf("  ") > -1)
                expression = expression.Replace("  ", " ");



        }
        private LinkedList<CFormItem> RipFormulaRPN(String expression)
        {
            var returnvalue = new LinkedList<CFormItem>();
            int currentposition = 0;
            CFormItem newitem;
            return null;


        }
        public IEnumerable<CFormItem> RipFormula_Enumerable(String expression)
        {
            //LinkedList<CFormItem> returnvalue = new LinkedList<CFormItem>();
            int currentposition = 0;
            CFormItem newitem;

            String mexpression = expression;

            if (CStackCache.instance().Exists(expression))
            {
                foreach (var valloop in CStackCache.instance()[expression].AsEnumerable())
                    yield return valloop;
                yield break;
            }
            while (currentposition < mexpression.Length)
            {


                foreach (ICorePlugin currplug in CorePlugins)
                {
                    newitem = new CFormItem();
                    if (currplug.ParseLocation(this, ref mexpression, ref currentposition, ref newitem))
                    {
                        //the plugin successfully parsed the item, so add it to the "stack"....


                        //returnvalue.AddLast(newitem);
                        newitem.handler = currplug;
                        yield return newitem; 
                    }

                    if (currentposition > mexpression.Length) break;
                }

            }
            


        }
        private LinkedList<CFormItem> RipFormula(String expression)
        {
            LinkedList<CFormItem> returnme = new LinkedList<CFormItem>();
            foreach (var loopvalue in RipFormulaEnum(expression))
            {

                returnme.AddLast(loopvalue);

            }
            CStackCache.instance().CacheStack(expression, returnme);
            return returnme;

        }

        private IEnumerable<CFormItem> RipFormulaEnum(String expression)
        {
           
            int currentposition=0;
            CFormItem newitem;
            
            String mexpression = expression;


            if (CStackCache.instance().Exists(expression))
            {
                foreach(var loopitem in CStackCache.instance()[expression])
                       yield return loopitem;
                
            }
                        MakeParsable(ref mexpression);
            ParseState = ParseStateConstants.ParseState_Parsing;
            try
            {
                //check for Assignments.
                bool indoublequote = false;
                bool insinglequote = false;
                int bracketcount = 0;
                for (int i = 0; i < expression.Length; i++)
                {
                    if (expression[i] == '\'') insinglequote = !insinglequote;
                    else if (expression[i] == '"') indoublequote = !indoublequote;
                    else if (starting_brackets.Contains(expression[i])) bracketcount++;
                    else if (ending_brackets.Contains(expression[i])) bracketcount--;
                    if (bracketcount <= 0 && indoublequote == false && insinglequote == false)
                    {
                        IEvalPlugin EvalPlug = null;
                        String assignmentop = null;
                        if ((assignmentop = AssignmentOperatorAtPos(expression, ref i, ref EvalPlug)) != null)
                        {
                            newitem = new CFormItem();
                            newitem.ItemType=CFormItem.FormItemTypeConstants.IT_ASSIGNMENT;
                            newitem.Operation=assignmentop;
                            newitem.Position=i;
                            newitem.Priority=32768;
                            newitem.handler=this;
                            if (assignmentop == "+=")
                            {
                                Debug.Print("was +=");

                            }
                            String LValue = expression.Substring(0,i);
                            string RValue = expression.Substring(i+assignmentop.Length);
                            newitem.Value = new object[] { LValue,new CParser(RValue,this)};
                            yield return newitem;
                            yield break;


                        }


                    }


                }



                
                while (currentposition < mexpression.Length)
                {


                    foreach (ICorePlugin currplug in CorePlugins)
                    {
                        newitem = new CFormItem();
                        if (currplug.ParseLocation(this, ref mexpression, ref currentposition, ref newitem))
                        {
                            //the plugin successfully parsed the item, so add it to the "stack"....

                            newitem.handler = currplug;
                            //returnvalue.AddLast(newitem);
                            yield return newitem;


                        }

                        if (currentposition >= mexpression.Length) yield break;
                    }

                }
            }
            finally
            {
                ParseState = ParseStateConstants.ParseState_IdleParsed;
            }
            // return returnvalue;


        }
        protected bool buildstack_infix(LinkedList<CFormItem> fromlist)
        {
            /*
             rearrangement schema:
             - always push literals
             
             
             * */
            
            //loop through the List....

            Stack<CFormItem> operatorstack= new Stack<CFormItem>();
            Stack<CFormItem> parsestack = new Stack<CFormItem>();
            foreach (CFormItem formulaitem in fromlist)
            {
                switch(formulaitem.ItemType)
                {

                    case CFormItem.FormItemTypeConstants.IT_VALUE:
                    case CFormItem.FormItemTypeConstants.IT_FUNCTION:
                    case CFormItem.FormItemTypeConstants.IT_SUBEXPRESSION:
                    case CFormItem.FormItemTypeConstants.IT_ARRAYACCESS:
                    case CFormItem.FormItemTypeConstants.IT_UNARY:
                    parsestack.Push(formulaitem);
                        break;
                    
                    case CFormItem.FormItemTypeConstants.IT_OPERATOR:
                        /* do NOT push operators but put them on a separate stack
             - if an operator//s priority is inferior to the one of the
             last operator on the operator stack then push all
             operators on the operator stack onto the parse stack
             and put the current operator on the operator stack*/
                        //if nothing is on the operator stack then push this operator onto it...
                        if (operatorstack.Count == 0)
                            operatorstack.Push(formulaitem);
                        else if (operatorstack.Peek().Priority > formulaitem.Priority)
                        {
                            //if an operator's priority is inferior to the one of the last 
                            //operator on the operator stack then push all operators on the operator stack
                            //onto the parse stack ...
                            while (operatorstack.Count > 0)
                                parsestack.Push(operatorstack.Pop());
                            //and put the current operator on the operator stack.
                            operatorstack.Push(formulaitem);

                        }
                        else
                        {
                            operatorstack.Push(formulaitem);

                        }
                        break;
                    default:
                        parsestack.Push(formulaitem);
                        break;
                }




            }

            //and lastly, if we have anything on the operator stack, pop everything off and push them onto the parse stack.
            while (operatorstack.Count>0)
                parsestack.Push(operatorstack.Pop());



            //in order for the stack to be properly collapsed, we need to reverse the order here.
            Stack<CFormItem> reversed=new Stack<CFormItem>();
            while(parsestack.Count>0)
                reversed.Push(parsestack.Pop());

            parsestack=reversed;
            mFormulaStack = new LinkedList<CFormItem>((IEnumerable<CFormItem>)parsestack);
            return true; 


        }
        protected bool isVariable(String vartest)
        {
            return Variables.Exists(vartest);

        }
        /// <summary>
        /// determines if the given operator is a Assignment Operator.
        /// </summary>
        /// <param name="testvalue">operator to test</param>
        /// <returns>IEvalPlugin interface handle to the Evaluation Plugin that understands the assignment. null otherwise.</returns>
        protected IEvalPlugin isAssignmentOperator(String testvalue)
        {

            

            foreach (var loopeval in EvalPlugins)
            {
                if (loopeval.IsAssignmentOperator(this,testvalue))
                    return loopeval;


            }

            return null;

        }
        protected String AssignmentOperatorAtPos(String Expression, ref int Position,ref IEvalPlugin pluginobj)
        {
            //start from end of string, moving backward, and 
            //take the longest that is a valid Assignment according to the plugins.
            for (int i = Expression.Length - Position; i > 0; i--)
            {
                //loop invariant: i is the current length to test.
                String testval=Expression.Substring(Position, i );
                IEvalPlugin grabplug = isAssignmentOperator(testval);
                if (grabplug != null)
                {
                    pluginobj = grabplug;
                    //Position = i;
                    return testval;

                }



            }

            return null;



        }
        protected bool isOperator(String optest, out operatortypeconstants optypeout)
        {
            operatortypeconstants returnvalue = operatortypeconstants.operator_none;
           var retbinoperator= mSplitBinOperators.SingleOrDefault((w)=>(w.Trim().Equals(optest.Trim())));
           var retprefixoperator = mSplitPrefixOperators.SingleOrDefault((w) => w.Trim().Equals(optest.Trim()));
           var retpostfixoperator = mSplitPostfixOperators.SingleOrDefault((w) => w.Trim().Equals(optest.Trim()));
           if (retbinoperator != null)
               returnvalue =returnvalue|operatortypeconstants.operator_binary;
           if (retprefixoperator != null)
               returnvalue = returnvalue| operatortypeconstants.operator_unary_prefix;
           if (retpostfixoperator != null)
               returnvalue = returnvalue | operatortypeconstants.operator_unary_postfix;
           optypeout = returnvalue;
           return(returnvalue!=operatortypeconstants.operator_none);
            
        }

        protected bool isOperator(String optest)
        {
            var ret = mSplitOperators.SingleOrDefault(w => (w.Trim().Equals(optest.Trim())));
            return (ret != null);


        }
        /*
         * <summary>
         * Determines if a Variable name is present at position <c>position</c> in the String <c>inExpression</c>
         * </summary>
         * <returns>
         * The name of the variable, or null if no variable is present.
         * </returns>
         * */
        protected String VariableAtPos(String inExpression, int position,CParser withparser)
        {
            //simple- iterate through all our variables...

            String tempgrab = null;
            foreach (Variable loopvalue in withparser.Variables)
            {
                if (inExpression.Length > loopvalue.Name.Length + position)
                {
                    tempgrab = inExpression.Substring(position, loopvalue.Name.Length);
                    if (loopvalue.Name.Equals(tempgrab, StringComparison.OrdinalIgnoreCase))
                        return loopvalue.Name;


                }

            }
            return null; 

        }
        protected String FunctionAtPos(String inExpression, int position)
        {
            //if there is a function at the given position, it is returned. otherwise, we return null.
            foreach (String ssfunc in mSplitFunctions)
            {
                String sfunc= ssfunc+"(";
                if ((inExpression.Length >= position + sfunc.Length) &&
                    sfunc.Equals(inExpression.Substring(position, sfunc.Length), StringComparison.OrdinalIgnoreCase))
                {

                    int matchedbracket = MatchBracket(inExpression, position + ssfunc.Length);
                    return inExpression.Substring(position, matchedbracket - position+1);


                    //return sfunc;

                }


            }
            return null;


        }

        protected String OperatorAtPos(String inExpression, int position)
        {
         //if there is an operator at the given position, it is returned. otherwise, we return null.
            foreach (String sOp in mSplitOperators)
            {
                if((inExpression.Length>= position+sOp.Length) && sOp.Equals(inExpression.Substring(position,sOp.Length),StringComparison.OrdinalIgnoreCase))
                    return sOp;



            }
            //none found, return null...
            return null;




        }
        internal static String ExtractBrackets(String inExpression, int StartPos)
        {
            return ExtractBrackets(inExpression, StartPos, starting_brackets, ending_brackets);

        }
        internal static String ExtractBrackets(String inExpression, int StartPos, string startbrackets, string endbrackets)
        {
            return ExtractBrackets(inExpression, StartPos, startbrackets, endbrackets, "\"'");

        }
        internal static String ExtractBrackets(String inExpression, int StartPos, string startbrackets, string endbrackets, string quotechars)
        {
            //PRE: inExpression[startpos] must be one of the characters in startbrackets
            //extract the contents of a set of brackets. for example:
            //ExtractBrackets("14+{\"set\",\"of\",\"array\",\"values\"}:,3)
            //should return the entire contents of  the curly brackets. note that the function overload that doesn't accept parameters for start brackets, endbrackets, etc defaults to 
            //a sane set of values. also, the bracket matched is the one found at the given position.

            
            int matchedpos = MatchBracket(inExpression, StartPos, startbrackets, endbrackets, quotechars);
            if (matchedpos == -1) throw new ParserSyntaxError(StartPos, "unmatched parenthesis");
            return inExpression.Substring(StartPos+1, matchedpos - StartPos - 1);

        }
        internal static int MatchBracket(String inExpression, int StartbracketPos)
        {
            return MatchBracket(inExpression, StartbracketPos, starting_brackets, ending_brackets);

        }
        internal static int MatchBracket(String inExpression, int StartbracketPos, string startbrackets, string endbrackets)
        {
            return MatchBracket(inExpression,StartbracketPos,startbrackets,endbrackets,"\"'");
            
        }
        internal static int MatchBracket(String inExpression, int StartbracketPos, String startbrackets, string endbrackets, String quotechars)
        {
            char endbracket;
            char startbracket = inExpression[StartbracketPos];

            //const string startbrackets="({[<";
            //const string endbrackets = ")}]>";
            int[] bracketcounts = new int[startbrackets.Length];  // 4 items
            bool[] inquotes;
            inquotes = new bool[quotechars.Length];
            //map out the "important" brackets- the one found at the specified position.
            while (inExpression[StartbracketPos] ==' ')
                StartbracketPos++;
            switch (inExpression[StartbracketPos])
            {
                case '(':
                    endbracket = ')';
                    break;
                case '{':
                    endbracket = '}';
                    break;
                case '[':
                    endbracket = ']';
                    break;
                case '<':
                    endbracket = '>';
                    break;
                default:
                    throw new Exception("MatchBracket passed a position that doesn't contain a bracket");
            }

            //now, iterate through the string... counting brackets and quotes...
            for (int i = StartbracketPos; i < inExpression.Length; i++)
            {
                char currchar = inExpression[i];
                if (startbrackets.Contains(currchar))
                    bracketcounts[startbrackets.IndexOf(currchar)]++;
                else if (endbrackets.Contains(currchar))
                {
                    bracketcounts[endbrackets.IndexOf(currchar)]--;
                    if (currchar.Equals(endbracket) && bracketcounts.All((a) => a <= 0) &&
                                            inquotes.All((a) => a == false))
                    {
                        //we found it... end of the bracket set...
                        //return the position of t he ending bracket.
                        return i; 


                    }
                    else if (quotechars.Contains(currchar))
                        inquotes[quotechars.IndexOf(currchar)] = !inquotes[quotechars.IndexOf(currchar)];





                }



            }
            return -1;

        }
        protected String ParseValue(String ExpressionParse, int startposition,CParser withparser)
        {
            //loop through until we find a space, operator, bracket, or comma.
            int foundstart = -1;
            int bracketcount = 0;
            bool inquote = false;
            bool returnnow = false;
            String foundop = null;
            String foundvar = null;
            String foundfunc=null;
            if (foundop != null)
                return foundop;
           // while (ExpressionParse[startposition] == ' ')
           //     startposition++;
            IEvalPlugin evalplug=null;
            if (ExpressionParse[startposition] == ' ') return "";
            for (int i = startposition; i < ExpressionParse.Length; i++)
            {
                bool outercontext = !inquote && bracketcount == 0;
                if (outercontext && ExpressionParse[i] == '@')
                {
                    //Form@Text("string")
                    //Form@Text()
                    //merely find first open bracket.
                    int FirstBracket = ExpressionParse.IndexOf('(', i);
                    return ExpressionParse.Substring(i, FirstBracket + 1-i);


                }
                if (ExpressionParse[i] != ' ' && foundstart == -1)
                    foundstart = i;
                //if (ExpressionParse[i] == ' ' && foundstart != -1 && bracketcount==0)
                if ((ExpressionParse[i] == ' ' && foundstart != -1 && inquote == false) || returnnow)
                    return ExpressionParse.Substring(foundstart, i - foundstart);
                else if (starting_brackets.IndexOf(ExpressionParse[i])>=0 && !inquote)
                {
                    //return ExtractBrackets(ExpressionParse, i);
                    int matched = MatchBracket(ExpressionParse, i);
                    if (matched == -1) throw new ParserSyntaxError(startposition, "unmatched parenthesis");
                    return ExpressionParse.Substring(startposition, matched - startposition+1); 

                }
                
                else if (ExpressionParse[i] == '\"' && bracketcount == 0)
                {
                    inquote = !inquote;
                    //if inquote is false, we just finished a string literal... but only if bracketcount is 0!
                    returnnow = !inquote;
                }
                else if (outercontext && (foundvar = AssignmentOperatorAtPos(ExpressionParse, ref i, ref evalplug))!=null)
                {
                    if (startposition != i) return ExpressionParse.Substring(foundstart, i - foundstart);
                    return foundvar;
                }
                else if (outercontext && ((foundvar = VariableAtPos(ExpressionParse, i, withparser)) != null))
                {
                    if (startposition != i) return ExpressionParse.Substring(foundstart, i - foundstart);
                    return foundvar;


                }
                else if (((foundop = OperatorAtPos(ExpressionParse, i)) != null && foundop.Length > 0) && outercontext)
                {
                    if (startposition != i) return ExpressionParse.Substring(foundstart, i - foundstart);
                    return foundop;


                }
                else if (((foundfunc = FunctionAtPos(ExpressionParse, i)) != null && foundfunc.Length > 0 && outercontext))
                {


                    if (startposition != i) return ExpressionParse.Substring(foundstart, i - foundstart);
                    return foundfunc;

                }


            }

            return ExpressionParse.Substring(foundstart);






        }
        public Object Self()
        {
            return this;
        }
        public Object Execute(String Expression)
        {
            this.Expression=Expression;
            return Execute();


        }
        public Object[] Execute(String[] Expressions)
        {
            Object[] returnvalues = new object[Expressions.Length];
            for (int i = 0; i < Expressions.Length; i++)
            {
                returnvalues[i] = Execute(Expressions[i]);


            }
            return returnvalues;

        }

        public Object Execute()
        {
            return CollapseStack(mFormulaStack);

        }
        public Object CollapseStack(LinkedList<CFormItem> stackCollapse)
        {
            EventStack<Object> evalStack= new EventStack<Object>();
            LastResult=null;
            ParseState = ParseStateConstants.ParseState_Executing;
            foreach (CFormItem loopitem in stackCollapse)
            {
                foreach(ICorePlugin loopcore in CorePlugins)
                {
                    if (loopcore.CanHandleItem(loopitem))
                        loopcore.HandleItem(this, loopitem,ref evalStack,ref stackCollapse);

                }
            }
            //Debug.Print("Evalstack.Count:" + evalStack.Count(); 
            //Debug.Assert(evalStack.Count > 0);
            ParseState = ParseStateConstants.ParseState_ExecuteComplete;
            LastResult = evalStack.Pop();
            return LastResult;
        }


        

        public CParser SpawnParser(String expression)
        {
            //spawnparser: creates a "child" parser, which references the same Lists of plugins, variables, and so forth.
            //also, while we are here we add the Spawned parsers pluginevent too..
            return new CParser(expression, this);
            





        }
        protected CParser[] ParseArgumentsToParserArray(String pExpression, int Startposition)
        {
            int tempend = 0;
            return ParseArgumentsToParserArray(pExpression,Startposition,out tempend);
        }
        protected CParser[] ParseArgumentsToParserArray(String pExpression, int Startposition,out int endlocation)
        {
            String[] parsedargs = ParseArguments(pExpression, Startposition,out endlocation);
            CParser[] retarray = new CParser[parsedargs.Length];
            for (int i = 0; i < parsedargs.Length; i++)
                retarray[i] = SpawnParser(parsedargs[i]);


            return retarray;


        }
        protected String[] ParseArguments(String pExpression, int startposition)
        {
            int tempend=0;
            return ParseArguments(pExpression, startposition, out tempend);
        }
        protected String[] ParseArguments(String epExpression, int startposition,out int endposition)
        {
            /*
             * ParseArguments: given a comma separated list of expressions, splits the expressions into an array of strings and returns it.
             * Note that it is, for reasons that should be obvious, not as simple as a or .Split() call
             * */

            String pExpression;

            pExpression = epExpression;
            /*
            if (starting_brackets.Any((w) => pExpression.StartsWith(w.ToString())))
                pExpression = pExpression.Substring(1);
            {
                if (ending_brackets.Any((w) => pExpression.EndsWith(w.ToString())))
                    pExpression = Expression.Substring(0, pExpression.Length - 1);
            }
             * */
            List<String> retval = new List<String>();
            int argstart = 0;
            bool inquote = false;
            int i = 0;
            endposition = startposition + 1;
            if(starting_brackets.Any((w)=>pExpression[startposition]=='w'))
                argstart = startposition + 1;
            else
                argstart = startposition;
            
            for (i = startposition; i < pExpression.Length; i++)
            {
                if (starting_brackets.IndexOf(pExpression[i]) > -1)
                {
                    //if we find a starting bracket, skip over the entire block....
                    int p = MatchBracket(pExpression, i) - 1;
                    //Note: -1 because the i++ in the for will add one, and we don't want to skip any characters.
                    if (p == i)
                        return new String[0];

                    i = p;



                }
                else if (pExpression[i] == '\"')
                {
                    inquote = !inquote;
                    if ((i + 1 == pExpression.Length))
                        retval.Add(pExpression.Substring(argstart));


                }
                else if ((pExpression[i] == ',' && !inquote))
                {
                    retval.Add(pExpression.Substring(argstart, i - argstart));
                    argstart = i + 1;

                }
                else if ((i + 1 == pExpression.Length))
                {
                    string stringresult = pExpression.Substring(argstart);
                    i += stringresult.Length;
                    while (ending_brackets.Any((e) => stringresult.EndsWith(e.ToString())))
                    {
                        stringresult = stringresult.Substring(0, stringresult.Length - 1);


                    }

                    retval.Add(stringresult);
                    

                }


            }
            endposition = i;
            if (retval.Count() == 0 && pExpression.Length > 0)
                retval.Add(pExpression);
            return retval.ToArray();



        }
        public String ResultToString(Object Result)
        {
            String runresult=null;
            foreach (IResultFormatter resformat in Resultformatters)
            {
                runresult = null;
                runresult = resformat.Formatresult(Result);
                if (runresult != null)
                    return runresult;
            }

            return null;

        }

        private String ResultToString_private(Object Result)
        {
            StringBuilder resultbuild=new StringBuilder();
            if(Result is IList<Object>)
            {

                int countrunner = 0;
                IList<Object> resultlist = (IList<Object>)Result;
                resultbuild.Append("{"); 
                foreach (Object loopitem in resultlist)
                {
                    resultbuild.Append(ResultToString(loopitem));
                    if(countrunner<resultlist.Count-1) resultbuild.Append(",");
                    
                    countrunner++;
                }
                resultbuild.Append("}");
            }
            else if (typeof(System.Text.RegularExpressions.Match).Equals(Result.GetType()))
            {
                Match smatch=(Match)Result;
                resultbuild.Append("Position " + smatch.Index.ToString() + " length= " + smatch.Length);


            }
            else if (typeof(System.Text.RegularExpressions.MatchCollection).Equals(Result.GetType()))
            {
                Debug.Print("Regular Expression matchcollection detected...");
                System.Text.RegularExpressions.MatchCollection matchcast;
                matchcast = (System.Text.RegularExpressions.MatchCollection)Result;
                resultbuild.Append(matchcast.Count.ToString() + " matches." + Environment.NewLine);
                resultbuild.Append("{" + Environment.NewLine);

                foreach (System.Text.RegularExpressions.Match loopmatch in matchcast)
                {

                    resultbuild.Append(ResultToString(loopmatch));
                    resultbuild.Append(Environment.NewLine);


                }
            }
            else
            {
                resultbuild.Append(Result.ToString());


            }
            return resultbuild.ToString();

         }






        private static String matchquote(String inExpression, int startpos)
        {
            const string quotecharacters = "'\"`";
            bool[] inquote = new bool[quotecharacters.Length];
            char matchquote = inExpression[startpos];


            for (int i = startpos+1; i < inExpression.Length;i++ )
            {
                if (inExpression[i] == matchquote||i==inExpression.Length)
                {
                    if (inquote.All((w) => w == false))
                        return inExpression.Substring(startpos+1, i - startpos-1);


                }

                int foundedpos = quotecharacters.IndexOf(inExpression[i]);
                if (foundedpos > 0)
                    inquote[foundedpos] = !inquote[foundedpos];

            }



            return ""; 


        }


        #region ICorePlugin Members
        private CFormItem mpreviousitem;
        private string[] mSplitBinOperators;

        bool ICorePlugin.ParseLocation(CParser withparser, ref string Expression, ref int position, ref CFormItem CurrentItem)
        {

            String[] handledfuncs, handledops;
            operatortypeconstants operatortypefound;
            handledfuncs = mSplitFunctions;
            handledops = mSplitOperators;
            IEvalPlugin gotplug = null;
            //VariableList Variables = withparser.Variables;
            VariableList Variables = withparser.Variables;
            //var previousitem = mFormulaStack.Find(CurrentItem).Previous; 
            //look at our current position to see if we have a function or op. a function must have a ( at the end, too.
            String parsedval = "";
            
            if (Expression[position] == '`') 
            {
                String backtickitem = matchquote(Expression, position);
                CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_BACKTICK;
                CurrentItem.Operation = backtickitem;
                CurrentItem.handler=this;
                CurrentItem.Position = position;
                position+=backtickitem.Length+2;
                return true;
            }

            while (parsedval == "")
            {
                parsedval = ParseValue(Expression, position, withparser);
                if (parsedval == "") position++;
            }
            //if the value has a Object access operator, only parse up to it. the next parsevalue will pick it up.
            if (parsedval.IndexOf("@") > 0)
            {
                parsedval = parsedval.Substring(0, parsedval.IndexOf("@"));


            }
            Debug.Print("Parsedvalue returned:" + parsedval);
            if (parsedval.StartsWith("@"))
            {
                Debug.Print(parsedval);
                //we cannot rely on ParseValue to understand the semantics here- it probably won't pick up on the brackets.
                //therefore, we will do the effort ourself.
                //the object access operator @ is used in the form:
                //value@methodname([param1],[param2]) etc.
                //first step: get the method name:
                CurrentItem.Position = position;
                String objectmethodname = Expression.Substring(position + 1, Expression.IndexOf("(", position) - position - 1);
                //now, parse arguments...
                int bracketposition = position + parsedval.Length-1;
                CParser[] methodarguments = ParseArgumentsToParserArray(Expression, bracketposition, out position);
                position++;
                CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_OBJACCESS;
                CurrentItem.Operation = objectmethodname;

                CurrentItem.Value = methodarguments;



            }
            else if (parsedval.Trim().StartsWith("{") && parsedval.Trim().EndsWith("}"))
            {
                CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_FUNCTION;
                CurrentItem.Operation = "array";
                CurrentItem.Position = position;
                CurrentItem.Value = ParseArgumentsToParserArray(ExtractBrackets(parsedval.Trim(), 0), 0);
                position += parsedval.Length;

            }

            else if (parsedval.StartsWith("(") && parsedval.EndsWith(")"))
            {
                //a "subexpression"...
                CurrentItem.Operation = "";
                CurrentItem.Position = position;
                String extracted = ExtractBrackets(parsedval, 0);
                CurrentItem.Value = (Object)new CParser(extracted);
                CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_SUBEXPRESSION;
                position += parsedval.Length;


            }
            //IT_FUNCTION- ends with a bracket, but starts with a function name.
            else if (parsedval.EndsWith(")") && !parsedval.StartsWith("(") && parsedval.Length > 1)
            {
                //add a new "function" item...
                CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_FUNCTION;
                //operation will be the function name...
                String functionname = parsedval.Substring(0, parsedval.IndexOf("("));
                //String functionarguments = parsedval.Substring(parsedval.IndexOf("(")+1, parsedval.IndexOf(")") - parsedval.IndexOf("(")-1);
                String functionarguments = ExtractBrackets(parsedval, parsedval.IndexOf("("));
                CurrentItem.Operation = functionname;
                CurrentItem.Position = position;
                //now, the tricky part...
                //use ParseArguments to parse out the arguments to this function.
                String[] funcarguments = ParseArguments(functionarguments, 0);
                //now create an "object" array, of CParser objects...
                Object[] paramobjects = new Object[funcarguments.Length];
                bool[] noparsedargs = new bool[funcarguments.Length];
                var handler = EvalPlugins.FirstOrDefault((w) => w.CanHandleFunction(functionname, ref noparsedargs));

                for (int i = 0; i <= funcarguments.Length - 1; i++)
                {

                    // paramobjects[i] = new CParser(funcarguments[i]);
                    if (noparsedargs[i])
                    {
                        paramobjects[i] = funcarguments[i];

                    }
                    else
                    {

                        //paramobjects[i] = this.SpawnParser(funcarguments[i]);
                        paramobjects[i] = new CParser(funcarguments[i], this);
                    }
                }
                CurrentItem.Value = paramobjects;
                position += parsedval.Length;
                //CurrentItem.Value 


            }
            else if (parsedval.StartsWith("["))
            {
                //Array access: pretty similar to the "function" type, but instead of an op we are accessing the previous item on the stack
                //using a set of subscripts (parameters)
                CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_ARRAYACCESS;
                //extract the subscripts, use the super magic extractbrackets routine, which has superpowers.
                String subscriptcontents = ExtractBrackets(parsedval, parsedval.IndexOf("["));
                Object[] subscripts = (Object[])ParseArgumentsToParserArray(subscriptcontents, 0);

                CurrentItem.Value = subscripts;
                position += parsedval.Length;

            }
                /*
            else if (parsedval.StartsWith("\"") && parsedval.EndsWith("\""))
            {



            }
            */


            else if (isVariable(parsedval))
            {
                CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_VARIABLE;
                CurrentItem.Operation = parsedval;
                CurrentItem.Position = position;
                CurrentItem.Value = Variables[parsedval];
                position += parsedval.Length;

            }

            else if (isOperator(parsedval, out operatortypefound))
            {
                //switch (previousitem.Value.ItemType)
                bool mcacheprefix = (operatortypefound & operatortypeconstants.operator_unary_prefix) == operatortypeconstants.operator_unary_prefix;
                bool mcachepostfix = (operatortypefound & operatortypeconstants.operator_unary_postfix) == operatortypeconstants.operator_unary_postfix;
                bool mcachebinfix = (operatortypefound & operatortypeconstants.operator_binary) == operatortypeconstants.operator_binary;
                //case CFormItem.FormItemTypeConstants.
                bool mallowed = false;
                if (mcachepostfix && !mcacheprefix)
                {
                    //cant be the first item...
                    if (mpreviousitem == null)
                        throw new ParserSyntaxError(position, "Unary Postfix cannot be the first token in an expression.");
                    //alright, otherwise, it's cool.
                    CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_UNARY;
                    CurrentItem.Operation = parsedval.Trim().ToLower();
                    CurrentItem.Priority = 1; //1=postfix, 0 =prefix.
                    CurrentItem.Position = position;
                    position += parsedval.Length;
                    mallowed = true;
                }
                else if (mcacheprefix && !mcachepostfix)
                {

                    if (mpreviousitem == null)
                        mallowed = true;
                    else
                    {
                        switch (mpreviousitem.ItemType)
                        {
                            case CFormItem.FormItemTypeConstants.IT_ARRAYACCESS:
                            case CFormItem.FormItemTypeConstants.IT_FUNCTION:
                            case CFormItem.FormItemTypeConstants.IT_OBJACCESS:
                            case CFormItem.FormItemTypeConstants.IT_SUBEXPRESSION:
                            case CFormItem.FormItemTypeConstants.IT_VALUE:
                            case CFormItem.FormItemTypeConstants.IT_VARIABLE:
                                mallowed = !mcachebinfix;
                                break;

                            default:
                                mallowed = true;
                                break;

                        }


                    }
                    if (mallowed)
                    {
                        CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_UNARY;
                        CurrentItem.Operation = parsedval.Trim().ToLower();
                        CurrentItem.Priority = 0; //1=postfix, 0 =prefix.
                        CurrentItem.Position = position;
                        position += parsedval.Length;

                    }
                }

                if (mcachebinfix && !mallowed)
                {
                    CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_OPERATOR;
                    CurrentItem.Operation = parsedval.Trim().ToLower();
                    CurrentItem.Priority = mBinOperators.IndexOf(" " + parsedval.Trim() + " ");
                    CurrentItem.Position = position;
                    position += parsedval.Length;
                }

            }
            else if (parsedval.StartsWith("(") && parsedval.EndsWith(")"))
            {

                CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_SUBEXPRESSION;
                CurrentItem.Value = new CParser(parsedval.Substring(1, parsedval.Length - 2));
                CurrentItem.Position = position;
                position += parsedval.Length;
            }
            else if (parsedval.Trim() == "")
            {
                position++;
            }
            else
            {
                //assume a literal.
                CurrentItem.ItemType = CFormItem.FormItemTypeConstants.IT_VALUE;
                if (parsedval.StartsWith("\""))
                    CurrentItem.Value = parsedval.Substring(1, parsedval.Length - 2);
                else
                    CurrentItem.Value = Double.Parse(parsedval);
                position += parsedval.Length;


            }


            mpreviousitem = CurrentItem;
            return true;


        }


        bool ICorePlugin.CanHandleItem(CFormItem itemtest)
        {
            return itemtest.handler == this; 
        }
        public CParser(String Expression) : this()
        {
            this.Expression = Expression;


        }
        public CParser(String pExpression, CParser pparent,VariableList pVariables,
            ObservableCollection<ICorePlugin> pCorePlugins,ObservableCollection<IEvalPlugin> pEvalPlugins,List<CFunction> pFunctions)
        {
            ParentParser = pparent;
            Variables = pVariables;
            CorePlugins = pCorePlugins;
            EvalPlugins = pEvalPlugins;
            Functions = pFunctions;
            foreach (IEvalPlugin evalplug in EvalPlugins)
            {
                //emulate the raising of the "pluginEvent"...
                PluginEvent(evalplug, (PluginEventTypeConstants)((int)PluginEventTypeConstants.PET_FUNCTIONSCHANGED + (int)PluginEventTypeConstants.PET_OPERATORSCHANGED));
                //add outselves to the interfaces List of eventdelegates...

                evalplug.EventDelegates.Add(PluginEvent);
            }




            //lastly, assign our expression, which starts the "fun" process of parsing, too.
            this.Expression = pExpression;


        }
        public CParser(String pExpression, CParser pparent, VariableList pVariables)
            : this(pExpression, pparent, pVariables, pparent.CorePlugins, pparent.EvalPlugins, pparent.Functions)
        {
            //
        }

        public CParser(String pExpression, CParser parent) : this(pExpression,parent,parent.Variables,parent.CorePlugins,parent.EvalPlugins,parent.Functions) 
        {
            //
        

        }
        public CParser()
        {
            //create the "list" of CorePlugins... initially, we are the only plugin.
            CorePlugins = new ObservableCollection<ICorePlugin>();
            EvalPlugins = new ObservableCollection<IEvalPlugin>();
            Variables = new VariableList();
            Resultformatters = new List<IResultFormatter> {this};
            Functions = new List<CFunction>();
            Variables.Add("Self", (Object)(this));
            CorePlugins.Add(this);
            IEvalPlugin mcoreplugin  = new CoreOpFunc(PluginEvent);
            IEvalPlugin mcorefuncplugin = new cFunctionHandler(PluginEvent,this);
            EvalPlugins.Add(mcoreplugin);
            EvalPlugins.Add(mcorefuncplugin);

            PluginEvent(mcoreplugin, (PluginEventTypeConstants)((int)PluginEventTypeConstants.PET_FUNCTIONSCHANGED + (int)PluginEventTypeConstants.PET_OPERATORSCHANGED));
            PluginEvent(mcorefuncplugin, (PluginEventTypeConstants)((int)PluginEventTypeConstants.PET_FUNCTIONSCHANGED + (int)PluginEventTypeConstants.PET_OPERATORSCHANGED));
        }
        #endregion

        #region ICorePlugin Members

        internal Object HandleFunction(String funcname, Object[] parameters)
        {
            //step one: convert parameters into their executed results; pretty simple with lambda expressions.
            Object[] paramresults = new Object[parameters.Length];
 
                
            //now, we look  through all our evaluation plugins, and find one that implements this function:
            bool[] noparsed = new bool[parameters.Length];
            IEvalPlugin foundPlug = EvalPlugins.FirstOrDefault((a) => a.CanHandleFunction(funcname,ref noparsed));

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!noparsed[i])
                    paramresults[i] = ((CParser)parameters[i]).Execute();
                else
                    paramresults[i] = parameters[i];
                    
                

            }
            
            if (foundPlug != null)
                return foundPlug.HandleFunction(funcname, new List<object>(paramresults));


            return null;

        }
        internal object HandleSubscript(object onvalue, object[] parameters)
        {
            foreach (IEvalPlugin currplug in EvalPlugins)
            {
                object retval = currplug.HandleSubscript(onvalue, parameters);
                if (retval != null)
                    return retval;
                //first one to return a non-null result wins.

            }
            return null;


        }
        private object HandleOperation(String pOperator, Object Operand,operatortypeconstants optype)
        {
            //handle a unary operation.
            
            foreach (IEvalPlugin foundplug in EvalPlugins)
            {
             
                    if(
                        (
                        (
                        (int)optype & (int)operatortypeconstants.operator_unary_postfix)==(int)operatortypeconstants.operator_unary_postfix 
                        ) || (
                        (
                        (int)optype & (int)operatortypeconstants.operator_unary_prefix)==(int)operatortypeconstants.operator_unary_prefix))
                    {
                        if (foundplug != null)
                        {
                            return foundplug.HandleUnaryOperation(pOperator, Operand, optype);

                        }

                    }



                

                

            }
            return null;

        }
        internal bool CanHandleFunction(String Functionname, ref bool[] noparsedargs)
        {

            foreach (IEvalPlugin loopplugin in EvalPlugins)
            {
                if (loopplugin.CanHandleFunction(Functionname, ref noparsedargs))
                    return true;


            }
            return false;

        }

        internal int CanHandleOperator(String Operator,out operatortypeconstants optype)
        {
            foreach (IEvalPlugin loopplugin in EvalPlugins)
            {
                int returnvalue = loopplugin.CanHandleOperator(Operator, out optype);
                if (returnvalue > 0)
                    return returnvalue;


            }
            optype = operatortypeconstants.operator_none; 
            return 0;

        }

        internal Object HandleOperation(String pOperator,Object OperandA,object OperandB)
    {
        BASeParser.operatortypeconstants optype;
        IEvalPlugin foundPlug = EvalPlugins.FirstOrDefault((a) => (a.CanHandleOperator(pOperator,out optype)>-1));
        if (foundPlug != null)
            return foundPlug.HandleOperator(pOperator, OperandA, OperandB);
        else
            return null;

    }
        public bool HandleInvocation(Object objcall,String methodcall,Object[] parameters,out Object returnvalue)
        {
            
            
            foreach (IEvalPlugin pluginloop in EvalPlugins)
            {
                returnvalue = pluginloop.HandleInvocation(objcall, methodcall, parameters);
                if (null != returnvalue)
                    return true;

            }
            returnvalue = null;
            return false;

        }
        private static String getcommandoutput(String scommand)
        {
            scommand = System.Environment.ExpandEnvironmentVariables(scommand);
            ProcessStartInfo psinfo = new ProcessStartInfo(scommand);
            StringBuilder buildresult = new StringBuilder();
            Process cmdprocess = Process.Start(psinfo);

            

            StreamReader processoutput = cmdprocess.StandardOutput;
            while (!cmdprocess.HasExited)
            {
                buildresult.Append(processoutput.ReadToEnd());

            }
            return buildresult.ToString();
        }

        private bool Stackpushevent(ref Object itempush)
        {
            if (itempush == null) return true ;
            Debug.Print("StackPushEvent:" + itempush.ToString());
            //todo: fix exception error when using new "DIV" operator...
            if (unaryprefixstack.Count() > 0)
            {
                //steps:
                //until the stack is empty...
                Object currvalue=itempush;
                while (unaryprefixstack.Count() > 0)
                {
                    CFormItem curritem = unaryprefixstack.Pop();
                    currvalue = HandleOperation(curritem.Operation, currvalue, operatortypeconstants.operator_unary_prefix);



                }
                if(currvalue!=null) itempush = currvalue;
                return true;


            }


            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="withparser">Parent parser object</param>
        /// <param name="AssignmentOp">Operator used for assignment- ie +=, *=, =</param>
        /// <param name="LValue">String- everything to the left of the assignment.</param>
        /// <param name="RValue">CParser object initialized to everything on the right of the assignment.</param>
        private Object HandleAssignment(CParser withparser, String AssignmentOp, String LValue, CParser RValue)
        {

            //first, find the plugin that understands this assignment...
            IEvalPlugin findhandler = EvalPlugins.FirstOrDefault((w) => w.IsAssignmentOperator(withparser, AssignmentOp));

            return findhandler.HandleAssignmentOperator(withparser,AssignmentOp, LValue, RValue);



        }

        public void HandleItem(CParser withparser, CFormItem itemhandle, ref EventStack<object> EvalStack, ref LinkedList<CFormItem> execlist)
        {
            object[] castobject;
            VariableList Variables = withparser.Variables;
            //if it's the first item, add our event handler...
            EvalStack.ItemPrePush+=Stackpushevent;
            switch (itemhandle.ItemType)
            {
                    case CFormItem.FormItemTypeConstants.IT_ASSIGNMENT:

                        //    CFormItem newitem = new CFormItem();
                        //    newitem.ItemType=CFormItem.FormItemTypeConstants.IT_ASSIGNMENT;
                        //    newitem.Operation=assignmentop;
                        //    newitem.Position=i;
                        //    newitem.Priority=32768;
                        //    

                        //    String LValue = expression.Substring(0,i);
                        //    string RValue = expression.Substring(i+assignmentop.Length);
                        //    newitem.Value = new object[] { LValue,new CParser(RValue)};

                    Debug.Print("IT_ASSIGNMENT: Assignment To:" + ((Object[])itemhandle.Value)[0].ToString());
                    EvalStack.Push(HandleAssignment(this, itemhandle.Operation, (String)((Object[])itemhandle.Value)[0], ((Object[])itemhandle.Value)[1] as CParser));



                    break;
                case CFormItem.FormItemTypeConstants.IT_BACKTICK:
                    EvalStack.Push(getcommandoutput(itemhandle.Operation));
                    break;
                case CFormItem.FormItemTypeConstants.IT_VALUE:
                    EvalStack.Push(itemhandle.Value);
                    break;
                case CFormItem.FormItemTypeConstants.IT_SUBEXPRESSION:
                    EvalStack.Push(((CParser)(itemhandle.Value)).Execute());
                    break;
                case CFormItem.FormItemTypeConstants.IT_FUNCTION:
                    //create a CParser[] array by casting each element of the itemhandle.value array.
                    castobject = (object[])itemhandle.Value;
                    Object[] funcparams;
                    funcparams = new Object[castobject.Length];
                    for (int i = 0; i < castobject.Length; i++)
                    {
                        
                        funcparams[i] = castobject[i];




                    }
                    //foreach(object loopobject in castobject)
                            
                            

                    EvalStack.Push(HandleFunction(itemhandle.Operation,funcparams));
                    break;
                

                case CFormItem.FormItemTypeConstants.IT_OBJACCESS:
                    Object objcall = EvalStack.Pop();
                    var parameters = from x in ((CParser[])itemhandle.Value) select (Object)x.Execute();
                    //MethodInfo invokethis = objcall.GetType().GetMethod((String)itemhandle.Operation);
                    
                    //invokethis.All((x) => x.Invoke(objcall, parameters.ToArray()));
                    bool methodcalled = false;
                    Object returnvalue = null;
                    methodcalled=HandleInvocation(objcall, itemhandle.Operation, parameters.ToArray(),out returnvalue);
                    if (methodcalled == false)
                    {
                        throw new EvaluationError(itemhandle, "Evaluation Error: Object of type " + objcall.GetType().Name + " does not have a method or property named " + itemhandle.Operation + " That accepts " + parameters.Count() + "Arguments.");

                    }
                    else
                    {

                        EvalStack.Push(returnvalue);
                    }
                    
                    

                    break;
                case CFormItem.FormItemTypeConstants.IT_OPERATOR:
                    Object OpB = EvalStack.Pop();
                    Object OpA = EvalStack.Pop(); 
                    EvalStack.Push(HandleOperation(itemhandle.Operation,OpA,OpB));
                    break;
                case CFormItem.FormItemTypeConstants.IT_UNARY:
                    if (itemhandle.Priority == 0)
                    {
                        //prefix
                        //push it into the operators stack...
                        unaryprefixstack.Push(itemhandle);

                    }
                    else
                    {
                        
                        //postfix
                        object itempop = EvalStack.Pop();
                        EvalStack.Push(HandleOperation(itemhandle.Operation,itempop,operatortypeconstants.operator_unary_postfix));

                    }

                    break;
                case CFormItem.FormItemTypeConstants.IT_VARIABLE:
                    EvalStack.Push(Variables[itemhandle.Operation].Value);
                    break;
                case CFormItem.FormItemTypeConstants.IT_ARRAYACCESS:
                    Object itemuse = EvalStack.Pop();
                    CParser[] parameval = (CParser[])itemhandle.Value;
                    //object[] subscripts = (object[]) ((a) => a.Execute);
                    object[] subscripts = new object[parameval.Length];
                    for (int loopsubscript = 0; loopsubscript < parameval.Length; loopsubscript++)
                        subscripts[loopsubscript] = parameval[loopsubscript].Execute();
                        
                    EvalStack.Push(HandleSubscript(itemuse,subscripts));
                    //Object[] parameters = ((CParser[])itemhandle.Value)
                    break; 
            }


        }

        #endregion
        internal void FunctionEvent(CFunction functionchanged, CFunction.Functionchangeeventconstants eventtype)
        {
            PluginEvent(corefuncplugin, PluginEventTypeConstants.PET_FUNCTIONSCHANGED|PluginEventTypeConstants.PET_OPERATORSCHANGED);


        }

        #region IResultFormatter Members

        public string Formatresult(object resultvalue)
        {
            return this.ResultToString_private(resultvalue);
        }

        #endregion
    }
}
