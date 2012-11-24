using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeParser
{

    public sealed class ParserSettings
    {
        ParserSettings()
        {
        }

        public static ParserSettings Instance
        {
            get
            {
                return Nested.instance;
            }
        }
        public int CacheMinStackSize { get; set; }
        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly ParserSettings instance = new ParserSettings();
        }
    }
}