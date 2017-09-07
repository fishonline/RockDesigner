using System;
using System.Text;

namespace Rock.Dyn.Msg
{
    public class TBinarySerializer : TSerializer
    {
        protected const uint VERSION_MASK = 0xffff0000;
        protected const uint VERSION_1 = 0x80010000;

        protected bool strictRead_ = false;
        protected bool strictWrite_ = true;

        protected int readLength_;
        protected bool checkReadLength_ = false;

        public TBinarySerializer()
            : this(false, true)
        {
        }

        public TBinarySerializer(bool strictRead, bool strictWrite)
        {
            strictRead_ = strictRead;
            strictWrite_ = strictWrite;
        }

        #region Write Methods

        public override void WriteMessageBegin(TMessage message)
        {
            if (strictWrite_)
            {
                uint version = VERSION_1 | (uint)(message.Type);
                WriteI32((int)version);
                WriteString(message.Name);
                WriteString(message.MsgID);
            }
            else
            {
                WriteString(message.Name);
                WriteByte((byte)message.Type);
                WriteString(message.MsgID);
            }
            WriteI16(message.LiveLife);
            WriteString(message.Sender);
            WriteString(message.SenderQueueName);
            WriteString(message.Receiver);
            WriteString(message.ReceiverQueueName);
        }

        public override void WriteMessageEnd()
        {
        }

        public override void WriteStructBegin(TStruct struc)
        {
            WriteString(struc.Name);
        }

        public override void WriteStructEnd()
        {
        }

        public override void WriteFieldBegin(TField field)
        {
            WriteByte((byte)field.Type);
            WriteI16(field.ID);
        }

        public override void WriteFieldEnd()
        {
        }

        public override void WriteFieldStop()
        {
            WriteByte((byte)TType.Stop);
        }

        public override void WriteMapBegin(TMap map)
        {
            WriteByte((byte)map.KeyType);
            WriteByte((byte)map.ValueType);
            WriteI32(map.Count);
        }

        public override void WriteMapEnd()
        {
        }

        public override void WriteListBegin(TList list)
        {
            WriteByte((byte)list.ElementType);
            WriteI32(list.Count);
        }

        public override void WriteListEnd()
        {
        }

        public override void WriteSetBegin(TSet set)
        {
            WriteByte((byte)set.ElementType);
            WriteI32(set.Count);
        }

        public override void WriteSetEnd()
        {
        }

        public override void WriteBool(bool b)
        {
            WriteByte(b ? (byte)1 : (byte)0);
        }

        private byte[] bout = new byte[1];
        public override void WriteByte(byte b)
        {
            bout[0] = b;
            _memoryStream.Write(bout, 0, 1);
        }

        private byte[] i16out = new byte[2];
        public override void WriteI16(short s)
        {
            i16out[0] = (byte)(0xff & (s >> 8));
            i16out[1] = (byte)(0xff & s);
            _memoryStream.Write(i16out, 0, 2);
        }

        private byte[] i32out = new byte[4];
        public override void WriteI32(int i32)
        {
            i32out[0] = (byte)(0xff & (i32 >> 24));
            i32out[1] = (byte)(0xff & (i32 >> 16));
            i32out[2] = (byte)(0xff & (i32 >> 8));
            i32out[3] = (byte)(0xff & i32);
            _memoryStream.Write(i32out, 0, 4);
        }

        private byte[] i64out = new byte[8];
        public override void WriteI64(long i64)
        {
            i64out[0] = (byte)(0xff & (i64 >> 56));
            i64out[1] = (byte)(0xff & (i64 >> 48));
            i64out[2] = (byte)(0xff & (i64 >> 40));
            i64out[3] = (byte)(0xff & (i64 >> 32));
            i64out[4] = (byte)(0xff & (i64 >> 24));
            i64out[5] = (byte)(0xff & (i64 >> 16));
            i64out[6] = (byte)(0xff & (i64 >> 8));
            i64out[7] = (byte)(0xff & i64);
            _memoryStream.Write(i64out, 0, 8);
        }

        public override void WriteDouble(double d)
        {
            WriteI64(BitConverter.DoubleToInt64Bits(d));
        }

        public override void WriteDecimal(decimal d)
        {
            int[] ints = Decimal.GetBits(d);

            if (ints.Length != 4)
            {
                throw new ApplicationException("deciaml转换为int时为非4位int数组 请尽快联系开发人员");
            }

            for (int i = 0; i < ints.Length; i++)
            {
                WriteI32(ints[i]);
            }
        }


        public override void WriteBinary(byte[] b)
        {
            WriteI32(b.Length);
            _memoryStream.Write(b, 0, b.Length);
        }

        #endregion

        #region ReadMethods

        public override TMessage ReadMessageBegin()
        {
            //初始化消息对象
            TMessage message = new TMessage();
            //读取第一部分 version strictRead_ == TRUE   整个读取的过程和写入时相反
            int size = ReadI32();
            if (size < 0)
            {
                uint version = (uint)size & VERSION_MASK;
                if (version != VERSION_1)
                {
                    throw new TSerializerException(TSerializerException.BAD_VERSION, "Bad version in ReadMessageBegin: " + version);
                }
                message.Type = (TMessageType)(size & 0x000000ff);
                //读取消息的名称
                message.Name = ReadString();
                //读取消息的msgID
                message.MsgID = ReadString();
            }
            else
            {
                if (strictRead_)
                {
                    throw new TSerializerException(TSerializerException.BAD_VERSION, "Missing version in readMessageBegin, old client?");
                }
                message.Name = ReadStringBody(size);
                message.Type = (TMessageType)ReadByte();
                message.MsgID = ReadString();
            }
            //读取超时时间
            message.LiveLife = ReadI16();
            //读取发送者
            message.Sender = ReadString();
            //读取发送队列名称
            message.SenderQueueName = ReadString();
            //读取接受者
            message.Receiver = ReadString();
            //读取接收者队列名称
            message.ReceiverQueueName = ReadString();

            return message;
        }

        public override void ReadMessageEnd()
        {
        }

        public override TStruct ReadStructBegin()
        {
            TStruct ts = new TStruct();
            ts.Name = ReadString();
            return ts;
        }

        public override void ReadStructEnd()
        {
        }

        public override TField ReadFieldBegin()
        {
            TField field = new TField();
            field.Type = (TType)ReadByte();

            if (field.Type != TType.Stop)
            {
                field.ID = ReadI16();
            }

            return field;
        }

        public override void ReadFieldEnd()
        {
        }

        public override TMap ReadMapBegin()
        {
            TMap map = new TMap();
            map.KeyType = (TType)ReadByte();
            map.ValueType = (TType)ReadByte();
            map.Count = ReadI32();

            return map;
        }

        public override void ReadMapEnd()
        {
        }

        public override TList ReadListBegin()
        {
            TList list = new TList();
            list.ElementType = (TType)ReadByte();
            list.Count = ReadI32();

            return list;
        }

        public override void ReadListEnd()
        {
        }

        public override TSet ReadSetBegin()
        {
            TSet set = new TSet();
            set.ElementType = (TType)ReadByte();
            set.Count = ReadI32();

            return set;
        }

        public override void ReadSetEnd()
        {
        }

        public override bool ReadBool()
        {
            return ReadByte() == 1;
        }

        private byte[] bin = new byte[1];
        public override byte ReadByte()
        {
            ReadAll(bin, 0, 1);
            return bin[0];
        }

        private byte[] i16in = new byte[2];
        public override short ReadI16()
        {
            ReadAll(i16in, 0, 2);
            return (short)(((i16in[0] & 0xff) << 8) | ((i16in[1] & 0xff)));
        }

        private byte[] i32in = new byte[4];
        public override int ReadI32()
        {
            ReadAll(i32in, 0, 4);
            return (int)(((i32in[0] & 0xff) << 24) | ((i32in[1] & 0xff) << 16) | ((i32in[2] & 0xff) << 8) | ((i32in[3] & 0xff)));
        }

        private byte[] i64in = new byte[8];
        public override long ReadI64()
        {
            ReadAll(i64in, 0, 8);
            return (long)(((long)(i64in[0] & 0xff) << 56) | ((long)(i64in[1] & 0xff) << 48) | ((long)(i64in[2] & 0xff) << 40) | ((long)(i64in[3] & 0xff) << 32) |
                ((long)(i64in[4] & 0xff) << 24) | ((long)(i64in[5] & 0xff) << 16) | ((long)(i64in[6] & 0xff) << 8) | ((long)(i64in[7] & 0xff)));
        }

        public override double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadI64());
        }

        public override decimal ReadDecimal()
        {
            int[] ints = new int[4];
            for (int i = 0; i < ints.Length; i++)
            {
                ints[i] = ReadI32();
            }
            return new decimal(ints);
        }

        public void SetReadLength(int readLength)
        {
            readLength_ = readLength;
            checkReadLength_ = true;
        }

        protected void CheckReadLength(int length)
        {
            if (checkReadLength_)
            {
                readLength_ -= length;
                if (readLength_ < 0)
                {
                    throw new ApplicationException("Message length exceeded: " + length);
                }
            }
        }

        public override byte[] ReadBinary()
        {
            int size = ReadI32();
            CheckReadLength(size);
            byte[] buf = new byte[size];
            ReadAll(buf, 0, size);
            return buf;
        }
        private string ReadStringBody(int size)
        {
            CheckReadLength(size);
            byte[] buf = new byte[size];
            ReadAll(buf, 0, size);
            return Encoding.UTF8.GetString(buf);
        }

        private new int ReadAll(byte[] buf, int off, int len)
        {
            CheckReadLength(len);
            return base.ReadAll(buf, off, len);
        }

        #endregion
    }
}
