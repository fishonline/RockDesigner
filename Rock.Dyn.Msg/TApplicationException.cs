using System;
using Rock.Dyn.Msg;

namespace Rock.Dyn.Msg
{
    public class TApplicationException : Exception
    {
        protected ExceptionType type;

        public TApplicationException()
        {
        }

        public TApplicationException(ExceptionType type)
        {
            this.type = type;
        }

        public TApplicationException(ExceptionType type, string message)
            : base(message)
        {
            this.type = type;
        }
        //从流里面读取异常消息
        public static TApplicationException Read(TSerializer iprot)
        {
            TField field;

            string message = null;
            ExceptionType type = ExceptionType.Unknown;
            //从流里面读取truct的name;这里只是读取并不用
            iprot.ReadStructBegin();
            while (true)
            {
                //读取两次,一次是field.Type = (TType)ReadByte() 另一次是:field.ID = ReadI16()
                field = iprot.ReadFieldBegin();
                if (field.Type == TType.Stop)
                {
                    //一直读取直到field.Type == TType.Stop为止
                    break;
                }

                switch (field.ID)
                {
                    case 1:
                        if (field.Type == TType.String)
                        {
                            message = iprot.ReadString();
                        }
                        else
                        {
                            //仅从流中把数据读取出来,不做任何操作
                            TSerializerUtil.Skip(iprot, field.Type);
                        }
                        break;
                    case 2:
                        if (field.Type == TType.I32)
                        {
                            type = (ExceptionType)iprot.ReadI32();
                        }
                        else
                        {
                            TSerializerUtil.Skip(iprot, field.Type);
                        }
                        break;
                    default:
                        TSerializerUtil.Skip(iprot, field.Type);
                        break;
                }
                //空方法(对二进制流)
                iprot.ReadFieldEnd();
            }
            //空方法
            iprot.ReadStructEnd();

            return new TApplicationException(type, message);
        }

        public void Write(TSerializer oprot)
        {
            TStruct struc = new TStruct("TApplicationException");
            TField field = new TField();
            //在流中写入TStruct的name:"TApplicationException"
            oprot.WriteStructBegin(struc);

            //判断Message是否为空
            if (!String.IsNullOrEmpty(Message))
            {
                field.Name = "message";
                field.Type = TType.String;
                field.ID = 1;
                oprot.WriteFieldBegin(field);
                oprot.WriteString(Message);
                oprot.WriteFieldEnd();//空
            }
            //再写一个field到流中
            field.Name = "type";
            field.Type = TType.I32;
            field.ID = 2;
            oprot.WriteFieldBegin(field);
            //在流中写入异常的类型
            oprot.WriteI32((int)type);
            oprot.WriteFieldEnd();
            //完成后写一个Stop标志 WriteByte((byte)TType.Stop)
            oprot.WriteFieldStop();
            oprot.WriteStructEnd();
        }

        public enum ExceptionType
        {
            Unknown,
            UnknownMethod,
            InvalidMessageType,
            WrongMethodName,
            BadSequenceID,
            MissingResult
        }
    }
}
