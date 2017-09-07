using System.Collections.Generic;
using System.Linq;
using System;
using System.Xml.Linq;

namespace Rock.Dyn.Core
{
    public class DynAttribute : DynClass
    {

        public DynAttribute(string name)
            : base(name)
        {

        }

        public DynAttribute(string name, List<string> interfaceNames)
            : this(name, null, interfaceNames)
        {

        }

        public DynAttribute(string name, string baseClassName, List<string> interfaceNames)
            : base(name, baseClassName, interfaceNames)
        {

        }       
    }
}
