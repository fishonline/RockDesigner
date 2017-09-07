using System;
using System.Collections.Generic;
using System.Text;

namespace Rock.Dyn.Msg
{
    public struct TStruct
    {
        private string name;

        public TStruct(string name)
            : this()
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
