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
        //���������ȡ�쳣��Ϣ
        public static TApplicationException Read(TSerializer iprot)
        {
            TField field;

            string message = null;
            ExceptionType type = ExceptionType.Unknown;
            //���������ȡtruct��name;����ֻ�Ƕ�ȡ������
            iprot.ReadStructBegin();
            while (true)
            {
                //��ȡ����,һ����field.Type = (TType)ReadByte() ��һ����:field.ID = ReadI16()
                field = iprot.ReadFieldBegin();
                if (field.Type == TType.Stop)
                {
                    //һֱ��ȡֱ��field.Type == TType.StopΪֹ
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
                            //�������а����ݶ�ȡ����,�����κβ���
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
                //�շ���(�Զ�������)
                iprot.ReadFieldEnd();
            }
            //�շ���
            iprot.ReadStructEnd();

            return new TApplicationException(type, message);
        }

        public void Write(TSerializer oprot)
        {
            TStruct struc = new TStruct("TApplicationException");
            TField field = new TField();
            //������д��TStruct��name:"TApplicationException"
            oprot.WriteStructBegin(struc);

            //�ж�Message�Ƿ�Ϊ��
            if (!String.IsNullOrEmpty(Message))
            {
                field.Name = "message";
                field.Type = TType.String;
                field.ID = 1;
                oprot.WriteFieldBegin(field);
                oprot.WriteString(Message);
                oprot.WriteFieldEnd();//��
            }
            //��дһ��field������
            field.Name = "type";
            field.Type = TType.I32;
            field.ID = 2;
            oprot.WriteFieldBegin(field);
            //������д���쳣������
            oprot.WriteI32((int)type);
            oprot.WriteFieldEnd();
            //��ɺ�дһ��Stop��־ WriteByte((byte)TType.Stop)
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
