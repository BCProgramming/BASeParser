using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeParser
{
    public interface ICorePlugin
    {
        bool ParseLocation(CParser withparser,ref String Expression,ref  int position, ref CFormItem CurrentItem);
        void HandleItem(CParser withparser,CFormItem itemhandle,ref EventStack<Object> EvalStack,ref LinkedList<CFormItem> execlist);
        bool CanHandleItem(CFormItem itemtest);
        
    }
}
