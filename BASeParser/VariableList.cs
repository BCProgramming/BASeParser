using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeParser
{
    /// <summary>
    /// Class used to allow for Indexing to use the name of the variable. and a few other things.
    /// </summary>
    public class VariableList : IEnumerable<Variable>
    {

        //events...
        public enum VariableListBeforeEventReturnConstants
        {
            /// <summary>
            /// VL_ALLOW: "accepts" that the event will occur (approves the add or remove)
            /// </summary>
            VL_ALLOW,
            /// <summary>
            /// VL_DENY: denies the change. later invocations may change this value.
            /// </summary>
            VL_DENY,
            

        }

        

        public delegate void SingleArgFunction(Variable variablearg);
        public event Func<Variable, VariableListBeforeEventReturnConstants> BeforeVariableAdd;
        public event Func<Variable, VariableListBeforeEventReturnConstants> BeforeVariableRemove;
        public event SingleArgFunction VariableAdded;
        public event SingleArgFunction VariableRemoved;


        #region event helper routines...
        public void RaiseVariableAdded(Variable variablearg)
        {
            SingleArgFunction arguse = VariableAdded;
            if (arguse != null)
                arguse.Invoke(variablearg);




        }
        public void RaiseVariableRemoved(Variable variablearg)
        {
            SingleArgFunction arguse = VariableRemoved;
            if (arguse != null)
                arguse.Invoke(variablearg);



        }

        public VariableListBeforeEventReturnConstants RaiseBeforeVariableRemove(Variable addedvar)
        {
            VariableListBeforeEventReturnConstants currentreturn;
            VariableListBeforeEventReturnConstants currreturn;
            currreturn = VariableListBeforeEventReturnConstants.VL_DENY;
            Delegate[] invokelist;
            Func<Variable, VariableListBeforeEventReturnConstants> addobj = BeforeVariableRemove;
            if (addobj != null)
            {
                invokelist = addobj.GetInvocationList();
                foreach (Func<Variable, VariableListBeforeEventReturnConstants> loopinvoke in invokelist)
                {
                    currentreturn = loopinvoke.Invoke(addedvar);
                    if (currentreturn == VariableListBeforeEventReturnConstants.VL_ALLOW)
                        currreturn = currentreturn;


                }






            }
            return currreturn;




        }

        public VariableListBeforeEventReturnConstants RaiseBeforeVariableAdd(Variable addedvar)
        {
             VariableListBeforeEventReturnConstants currentreturn;
             VariableListBeforeEventReturnConstants currreturn;
            currreturn =  VariableListBeforeEventReturnConstants.VL_DENY ;
            Delegate[] invokelist;
            Func<Variable,  VariableListBeforeEventReturnConstants> addobj = BeforeVariableAdd;
            if (addobj != null)
            {
                invokelist = addobj.GetInvocationList();
                foreach (Func<Variable,  VariableListBeforeEventReturnConstants> loopinvoke in invokelist)
                {
                    currentreturn = loopinvoke.Invoke(addedvar);
                    if(currentreturn==VariableListBeforeEventReturnConstants.VL_ALLOW)
                        currreturn = currentreturn;


                }






            }
            return currreturn;




        }



        #endregion 
        /// <summary>
        /// Private List used to actually hold variables.
        /// </summary>
        private Dictionary<String,Variable> mList;
        public Dictionary<String, Variable> GetDictionary()
        {

            return mList;
        }

        public VariableList()
        {
            mList = new Dictionary<string, Variable>();
            Add(new Variable("i", new Complex(0d, 1d)));
            Add(new Variable("e", Math.E));
            Add(new Variable("pi", Math.PI));
            Add(new Variable("nil", null));
        }

        public VariableList(VariableList Otherlist)
        {
            mList = new Dictionary<String,Variable>(Otherlist.GetDictionary());
        }

        public Variable this[int Index]
        {
            get { return mList.ElementAt(Index).Value; }
            
        }
        public Variable Add(Variable addvar)
        {
            mList.Add(addvar.Name.ToUpper(), addvar);
            return addvar;

        }

        public Variable Add(String Name, Object Value)
        {
            Variable addthis = new Variable(Name.ToUpper(), Value);
            mList.Add(Name.ToUpper(), addthis);
            return addthis;
        }

        public void Remove(String Name)
        {
            mList.Remove(Name.ToUpper());
            
        }

        public void Remove(Variable removeitem)
        {
            Remove(removeitem.Name.ToUpper());
        }

        public bool Exists(String Name)
        {
            return mList.ContainsKey(Name.ToUpper());
        }

        public Variable this[String index]
        {
            get {

                if (!Exists(index))
                    return Add(index.ToUpper(), 0);
                else
                    return mList[index.ToUpper()];
                    
                
            
            }
            set
            {

                mList[index.ToUpper()] = value;
            }
        }

        #region IEnumerable<Variable> Members

        public IEnumerator<Variable> GetEnumerator()
        {
            // throw new NotImplementedException();
            return mList.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return mList.Values.GetEnumerator();
        }

        #endregion
    }
}