using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
namespace BASeParser
{
    public sealed class CStackCache
    {
        //CStackCache: used to cache formulastacks (to help avoid parsing the same expression multiple times).
        class NestedClass
        {
            internal static readonly CStackCache StackCache = new CStackCache();

        }
        public List<KeyValuePair<String, LinkedList<CFormItem>>> cachedstacks { get; set; }

        public static CStackCache instance()
        {
            return NestedClass.StackCache;

        }

        public CStackCache()
        {
            cachedstacks = new List<KeyValuePair<string, LinkedList<CFormItem>>>();

        }
        public void CacheStack(String expression, LinkedList<CFormItem> formulastack)
        {
            if (ParserSettings.Instance.CacheMinStackSize < formulastack.Count())
            {
                if (!cachedstacks.Exists((w) => w.Key.Equals(expression, StringComparison.OrdinalIgnoreCase)))
                {

                    //item not already in the collection...
                    Debug.Print("caching stack for formula:" + expression + ", Length= " + formulastack.Count());
                    cachedstacks.Add(new KeyValuePair<string, LinkedList<CFormItem>>(expression, formulastack));

                }


            }

        }
        public bool Exists(String expression)
        {
            return cachedstacks.Any((w) => w.Key.Equals(expression, StringComparison.OrdinalIgnoreCase));


        }

        public LinkedList<CFormItem> this[String Indexvalue]
        {
            get { 
            if (cachedstacks.Exists((w)=>w.Key.Equals(Indexvalue,StringComparison.OrdinalIgnoreCase)))
                {
                    var returnthis =
                        cachedstacks.FirstOrDefault((w) => w.Key.Equals(Indexvalue, StringComparison.OrdinalIgnoreCase));
                    return returnthis.Value;

                }
                return null;
            }

            set {CacheStack(Indexvalue,value); }


        }
    


    }
    }

