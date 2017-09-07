using System;

namespace Rock.Dyn.Msg
{
    class TSerializerException : ApplicationException
    {
        public const int UNKNOWN = 0;
        public const int INVALID_DATA = 1;
        public const int NEGATIVE_SIZE = 2;
        public const int SIZE_LIMIT = 3;
        public const int BAD_VERSION = 4;
        public const int NOT_IMPLEMENTED = 5;

        protected int type_ = UNKNOWN;

        public TSerializerException()
            : base()
        {
        }

        public TSerializerException(int type)
            : base()
        {
            type_ = type;
        }

        public TSerializerException(int type, String message)
            : base(message)
        {
            type_ = type;
        }

        public TSerializerException(String message)
            : base(message)
        {
        }

        public int getType()
        {
            return type_;
        }
    }
}
