using System;

namespace Rock.Dyn.Msg
{
    public static class TSerializerUtil
    {
        public static void Skip(TSerializer prot, TType type)
        {
            switch (type)
            {
                case TType.Bool:
                    prot.ReadBool();
                    break;
                case TType.Byte:
                    prot.ReadByte();
                    break;
                case TType.I16:
                    prot.ReadI16();
                    break;
                case TType.I32:
                    prot.ReadI32();
                    break;
                case TType.I64:
                    prot.ReadI64();
                    break;
                case TType.Double:
                    prot.ReadDouble();
                    break;
                case TType.String:
                    // Don't try to decode the string, just skip it.
                    prot.ReadBinary();
                    break;
                case TType.Struct:
                    prot.ReadStructBegin();
                    while (true)
                    {
                        TField field = prot.ReadFieldBegin();
                        if (field.Type == TType.Stop)
                        {
                            break;
                        }
                        Skip(prot, field.Type);
                        prot.ReadFieldEnd();
                    }
                    prot.ReadStructEnd();
                    break;
                case TType.Map:
                    TMap map = prot.ReadMapBegin();
                    for (int i = 0; i < map.Count; i++)
                    {
                        Skip(prot, map.KeyType);
                        Skip(prot, map.ValueType);
                    }
                    prot.ReadMapEnd();
                    break;
                case TType.Set:
                    TSet set = prot.ReadSetBegin();
                    for (int i = 0; i < set.Count; i++)
                    {
                        Skip(prot, set.ElementType);
                    }
                    prot.ReadSetEnd();
                    break;
                case TType.List:
                    TList list = prot.ReadListBegin();
                    for (int i = 0; i < list.Count; i++)
                    {
                        Skip(prot, list.ElementType);
                    }
                    prot.ReadListEnd();
                    break;
            }
        }
    }
}
