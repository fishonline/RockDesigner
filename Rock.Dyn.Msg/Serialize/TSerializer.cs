using System;
using System.Text;
using System.IO;

namespace Rock.Dyn.Msg
{
    public abstract class TSerializer
    {
        public MemoryStream _memoryStream = new MemoryStream();

        public int Read(byte[] buf, int off, int len)
        {
            return _memoryStream.Read(buf, off, len);
        }

        public int ReadAll(byte[] buf, int off, int len)
        {
            int got = 0;
            int ret = 0;

            while (got < len)
            {
                ret = Read(buf, off + got, len - got);
                if (ret <= 0)
                {
                    throw new ApplicationException("Cannot read, Remote side has closed");
                }
                got += ret;
            }

            return got;
        }

        public void Write(byte[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public void Write(byte[] buf, int off, int len)
        {
            _memoryStream.Write(buf, off, len);
        }

        public void Flush()
        {
            _memoryStream.Flush();
        }

        public byte[] ToBytes()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            byte[] bytes = new byte[_memoryStream.Length];
            _memoryStream.Read(bytes, 0, (int)_memoryStream.Length);

            return bytes;
        }

        public void FromBytes(byte[] bytes)
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            _memoryStream.Write(bytes, 0, bytes.Length);
            _memoryStream.Seek(0, SeekOrigin.Begin);
        }

        public abstract void WriteMessageBegin(TMessage message);
        public abstract void WriteMessageEnd();
        public abstract void WriteStructBegin(TStruct struc);
        public abstract void WriteStructEnd();
        public abstract void WriteFieldBegin(TField field);
        public abstract void WriteFieldEnd();
        public abstract void WriteFieldStop();
        public abstract void WriteMapBegin(TMap map);
        public abstract void WriteMapEnd();
        public abstract void WriteListBegin(TList list);
        public abstract void WriteListEnd();
        public abstract void WriteSetBegin(TSet set);
        public abstract void WriteSetEnd();
        public abstract void WriteBool(bool b);
        public abstract void WriteByte(byte b);
        public abstract void WriteI16(short i16);
        public abstract void WriteI32(int i32);
        public abstract void WriteI64(long i64);
        public abstract void WriteDouble(double d);
        public abstract void WriteDecimal(decimal d);
        public virtual void WriteString(string s)
        {
            WriteBinary(Encoding.UTF8.GetBytes(s));
        }
        public abstract void WriteBinary(byte[] b);

        public abstract TMessage ReadMessageBegin();
        public abstract void ReadMessageEnd();
        public abstract TStruct ReadStructBegin();
        public abstract void ReadStructEnd();
        public abstract TField ReadFieldBegin();
        public abstract void ReadFieldEnd();
        public abstract TMap ReadMapBegin();
        public abstract void ReadMapEnd();
        public abstract TList ReadListBegin();
        public abstract void ReadListEnd();
        public abstract TSet ReadSetBegin();
        public abstract void ReadSetEnd();
        public abstract bool ReadBool();
        public abstract byte ReadByte();
        public abstract short ReadI16();
        public abstract int ReadI32();
        public abstract long ReadI64();
        public abstract double ReadDouble();
        public abstract decimal ReadDecimal();
        public virtual string ReadString()
        {
            return Encoding.UTF8.GetString(ReadBinary());
        }
        public abstract byte[] ReadBinary();
    }
}
