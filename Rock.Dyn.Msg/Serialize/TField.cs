using System;
using System.Collections.Generic;
using System.Text;

namespace Rock.Dyn.Msg
{
    public struct TField
    {
        private string name;
        private TType type;
        private short id;

        public TField(string name, TType type, short id)
            : this()
        {
            this.name = name;
            this.type = type;
            this.id = id;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public TType Type
        {
            get { return type; }
            set { type = value; }
        }

        public short ID
        {
            get { return id; }
            set { id = value; }
        }
    }
}
