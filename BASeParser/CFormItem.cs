using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization; 
namespace BASeParser
{

/// <summary>
/// represents a formula item/token on the stack
/// </summary>



    public class CFormItem : IFormattable 
    {
        
        public enum FormItemTypeConstants

        {
            IT_PLUGIN=-1,
            IT_NULL,
            IT_VALUE,
            IT_VARIABLE,
            IT_FUNCTION,
            IT_OPERATOR,
            IT_UNARY,
            IT_ARRAYACCESS,
            IT_CALL,
            IT_SUBEXPRESSION,
            IT_OBJACCESS,
            IT_BACKTICK,
            IT_ASSIGNMENT 
            //Operation is  the assignment operator (+=, =, etc). 
            //Value is  a 2-element array of child Parsers
            //element 0 is the LValue. element 1 is the RValue.
        }
        /// <summary>
        /// ItemType: determines the type of stack item/token.
        /// </summary>
        /// <value>the type of token that this object represents in the formula stack.</value>
        /// <remarks>The ItemType is important to determine the valid values for and appropriate uses for some of the other properties, such as 
        /// Operation, Value, and tag. For the most part, these values are defined exclusively by the handler that created then during the parse stage. 
        /// To retrieve the handler that created this item, access the <see="handler"/> property.</remarks>
        public FormItemTypeConstants ItemType { get; set; }
        public String Operation{get;set;}
        public Object Value { get; set; }
        public Object Tag { get; set; }
        public int Position { get; set; }
        public int Priority { get; set; }
        public ICorePlugin handler { get; set; }





        #region IFormattable Members
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ItemType=" + ItemType.ToString());
            sb.Append(",");
            sb.Append("Operation=" + Operation);
            sb.Append(",");
            sb.Append("Position=" + Position);
            sb.Append("Value=" + (Value??""));

            return sb.ToString();
        }

    string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return this.ToString();
        }

        #endregion
    }
}
