using System;
using Rock.Dyn.Msg;
using Rock.Dyn.Core;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace Rock.Dyn.Comm
{
    /// <summary>
    /// 动态序列化类
    /// </summary>
    public static class DynSerialize
    {
        public const byte CalculationBase = 180;

        /// <summary>
        /// 从序列化串中读动态对象
        /// </summary>
        /// <param name="serializer">序列化方式</param>
        /// <param name="structName">对象名</param>
        /// <returns>动态对象</returns>
        public static DynObject ReadDynObject(TSerializer serializer)
        {

            TField field;
            TStruct tstruct = serializer.ReadStructBegin();
            string structName = tstruct.Name;
            DynClass dynClass = DynTypeManager.GetClass(structName);

            if (dynClass != null)
            {
                DynObject obj = new DynObject(structName, false);

                while (true)
                {
                    field = serializer.ReadFieldBegin();
                    if (field.Type == TType.Stop)
                    {
                        break;
                    }

                    DynProperty property = obj.DynClass.GetProperty(field.ID);

                    switch (property.CollectionType)
                    {
                        case CollectionType.None:
                            switch (property.DynType)
                            {
                                case DynType.Bool:
                                    obj.SetPropertyValueWithOutCheck(property, serializer.ReadBool());
                                    break;
                                case DynType.Byte:
                                    obj.SetPropertyValueWithOutCheck(property, serializer.ReadByte());
                                    break;
                                case DynType.Double:
                                    obj.SetPropertyValueWithOutCheck(property, serializer.ReadDouble());
                                    break;
                                case DynType.Decimal:
                                    obj.SetPropertyValueWithOutCheck(property, serializer.ReadDecimal());
                                    break;
                                case DynType.I16:
                                    obj.SetPropertyValueWithOutCheck(property, serializer.ReadI16());
                                    break;
                                case DynType.I32:
                                    obj.SetPropertyValueWithOutCheck(property, serializer.ReadI32());
                                    break;
                                case DynType.I64:
                                    obj.SetPropertyValueWithOutCheck(property, serializer.ReadI64());
                                    break;
                                case DynType.String:
                                    obj.SetPropertyValueWithOutCheck(property, serializer.ReadString());
                                    break;
                                case DynType.DateTime:
                                    string dt = serializer.ReadString();
                                    obj.SetPropertyValueWithOutCheck(property, Convert.ToDateTime(dt));
                                    break;
                                case DynType.Binary:
                                    obj.SetPropertyValueWithOutCheck(property, serializer.ReadBinary());
                                    break;
                                case DynType.Struct:
                                    obj.SetPropertyValueWithOutCheck(property, DynSerialize.ReadDynObject(serializer));
                                    break;
                                default:
                                    TSerializerUtil.Skip(serializer, field.Type);
                                    break;
                            }
                            break;

                        case CollectionType.List:
                            obj.SetPropertyValueWithOutCheck(property, DynSerialize.ReadList(serializer, property.StructName));
                            break;
                            //Set未实现
                        case CollectionType.Set:
                            break;

                        case CollectionType.Map:
                            obj.SetPropertyValueWithOutCheck(property, DynSerialize.ReadMap(serializer));
                            break;
                        default:
                            break;
                    }
                    serializer.ReadFieldEnd();
                }
                serializer.ReadStructEnd();

                return obj;
            }

            return null;
        }

        /// <summary>
        /// 向序列化串中写入动态对象
        /// </summary>
        /// <param name="serializer">序列化方式</param>
        /// <param name="obj">动态对象</param>
        public static void WriteDynObject(TSerializer serializer, DynObject obj)
        {
            TStruct struc = new TStruct(obj.DynClass.Name);
            serializer.WriteStructBegin(struc);

            TField field = new TField();

            foreach (DynProperty property in obj.DynClass.GetProperties())
            {
                if (obj[property.Name] != null)
                {
                    field.Name = property.Name;
                    field.ID = property.ID;
                    switch (property.CollectionType)
                    {
                        case CollectionType.None:
                            field.Type = (TType)(byte)property.DynType;
                            serializer.WriteFieldBegin(field);

                            switch (field.Type)
                            {
                                case TType.Bool:
                                    serializer.WriteBool((Boolean)obj[property.Name]);
                                    break;
                                case TType.Byte:
                                    serializer.WriteByte((Byte)obj[property.Name]);
                                    break;
                                case TType.Double:
                                    serializer.WriteDouble((Double)obj[property.Name]);
                                    break;
                                case TType.Decimal:
                                    serializer.WriteDecimal((Decimal)obj[property.Name]);
                                    break;
                                case TType.I16:
                                    serializer.WriteI16((Int16)obj[property.Name]);
                                    break;
                                case TType.I32:
                                    serializer.WriteI32((Int32)obj[property.Name]);
                                    break;
                                case TType.I64:
                                    serializer.WriteI64((Int64)obj[property.Name]);
                                    break;
                                case TType.String:
                                    serializer.WriteString(obj[property.Name].ToString());
                                    break;
                                case TType.DateTime:
                                    string dt = Convert.ToDateTime(obj[property.Name]).ToString("yyyy-MM-dd HH:mm:ss");
                                    serializer.WriteString(dt);
                                    break;
                                case TType.Binary:
                                    serializer.WriteBinary((byte[])obj[property.Name]);
                                    break;
                                case TType.Struct:
                                    DynSerialize.WriteDynObject(serializer, obj[property.Name] as DynObject);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case CollectionType.List:
                            field.Type = TType.List;
                            serializer.WriteFieldBegin(field);

                            DynSerialize.WriteList(serializer, obj[property.Name], property.DynType);
                            break;

                        case CollectionType.Set:
                            field.Type = TType.Set;
                            serializer.WriteFieldBegin(field);
                            break;

                        case CollectionType.Map:
                            field.Type = TType.Map;
                            serializer.WriteFieldBegin(field);

                            DynSerialize.WriteMap(serializer, obj[property.Name] as IDictionary);
                            break;

                        default:
                            break;
                    }
                    serializer.WriteFieldEnd();
                }
            }
            serializer.WriteFieldStop();
            serializer.WriteStructEnd();
        }

        /// <summary>
        /// 读取动态方法实例
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="interfaceName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static DynMethodInstance ReadDynMethodInstance(TSerializer serializer, string interfaceName, string methodName)
        {
            TField field;
            TStruct tstruct = serializer.ReadStructBegin();

            DynMethod dynMethod = DynTypeManager.GetInterface(interfaceName).GetMethod(methodName);

            if (dynMethod != null)
            {
                DynMethodInstance obj = new DynMethodInstance(interfaceName, methodName);

                while (true)
                {
                    field = serializer.ReadFieldBegin();
                    if (field.Type == TType.Stop)
                    {
                        break;
                    }

                    DynParameter property = obj.DynMethod.GetParameter(field.ID);

                    switch (property.CollectionType)
                    {
                        case CollectionType.None:
                            switch (property.DynType)
                            {
                                case DynType.Bool:
                                    obj[property.Name] = serializer.ReadBool();
                                    break;
                                case DynType.Byte:
                                    obj[property.Name] = serializer.ReadByte();
                                    break;
                                case DynType.Double:
                                    obj[property.Name] = serializer.ReadDouble();
                                    break;
                                case DynType.Decimal:
                                    obj[property.Name] = serializer.ReadDecimal();
                                    break;
                                case DynType.I16:
                                    obj[property.Name] = serializer.ReadI16();
                                    break;
                                case DynType.I32:
                                    obj[property.Name] = serializer.ReadI32();
                                    break;
                                case DynType.I64:
                                    obj[property.Name] = serializer.ReadI64();
                                    break;
                                case DynType.String:
                                    obj[property.Name] = serializer.ReadString();
                                    break;
                                case DynType.DateTime:
                                    string dt = serializer.ReadString();
                                    obj[property.Name] = Convert.ToDateTime(dt);
                                    break;
                                case DynType.Binary:
                                    obj[property.Name] = serializer.ReadBinary();
                                    break;
                                case DynType.Struct:
                                    obj[property.Name] = DynSerialize.ReadDynObject(serializer);
                                    break;
                                default:
                                    TSerializerUtil.Skip(serializer, field.Type);
                                    break;
                            }
                            break;

                        case CollectionType.List:
                            obj[property.Name] = DynSerialize.ReadList(serializer, property.StructName);
                            break;

                        case CollectionType.Set:
                            break;

                        case CollectionType.Map:
                            obj[property.Name] = DynSerialize.ReadMap(serializer);
                            break;

                        default:
                            break;
                    }
                    serializer.ReadFieldEnd();
                }
                serializer.ReadStructEnd();

                return obj;
            }

            return null;
        }

        /// <summary>
        /// 写入动态方法实例
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="instance"></param>
        public static void WriteDynMethodInstance(TSerializer serializer, DynMethodInstance instance)
        {
            TStruct struc = new TStruct(instance.DynMethod.Name);
            //就是写入instance.DynMethod.Name
            serializer.WriteStructBegin(struc);
            TField field = new TField();

            foreach (DynParameter parameter in instance.DynMethod.GetParameters())
            {
                if (instance[parameter.Name] != null)
                {
                    field.Name = parameter.Name;
                    field.ID = parameter.ID;
                    switch (parameter.CollectionType)
                    {
                        case CollectionType.None:
                            field.Type = (TType)(byte)parameter.DynType;
                            //写入两给信息WriteByte,fieldType 和WriteI16 fieldID
                            serializer.WriteFieldBegin(field);

                            switch (field.Type)
                            {
                                case TType.Bool:
                                    serializer.WriteBool((Boolean)instance[parameter.Name]);
                                    break;
                                case TType.Byte:
                                    serializer.WriteByte((Byte)instance[parameter.Name]);
                                    break;
                                case TType.Double:
                                    serializer.WriteDouble((Double)instance[parameter.Name]);
                                    break;
                                case TType.Decimal:
                                    serializer.WriteDecimal((Decimal)instance[parameter.Name]);
                                    break;
                                case TType.I16:
                                    serializer.WriteI16((Int16)instance[parameter.Name]);
                                    break;
                                case TType.I32:
                                    serializer.WriteI32((Int32)instance[parameter.Name]);
                                    break;
                                case TType.I64:
                                    serializer.WriteI64((Int64)instance[parameter.Name]);
                                    break;
                                case TType.String:
                                    serializer.WriteString(instance[parameter.Name].ToString());
                                    break;
                                case TType.DateTime:
                                    string dt = Convert.ToDateTime(instance[parameter.Name]).ToString("yyyy-MM-dd HH:mm:ss");
                                    serializer.WriteString(dt);
                                    break;
                                case TType.Binary:
                                    serializer.WriteBinary((byte[])instance[parameter.Name]);
                                    break;
                                case TType.Struct:
                                    DynSerialize.WriteDynObject(serializer, instance[parameter.Name] as DynObject);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case CollectionType.List:
                            field.Type = TType.List;
                            serializer.WriteFieldBegin(field);

                            DynSerialize.WriteList(serializer, instance[parameter.Name], parameter.DynType);
                            break;

                        case CollectionType.Set:
                            field.Type = TType.Set;
                            serializer.WriteFieldBegin(field);
                            break;

                        case CollectionType.Map:
                            field.Type = TType.Map;
                            serializer.WriteFieldBegin(field);

                            DynSerialize.WriteMap(serializer, instance[parameter.Name] as IDictionary);
                            break;

                        default:
                            break;
                    }
                    //什么都不做
                    serializer.WriteFieldEnd();
                }
            }
            //WriteByte((byte)TType.Stop)将field的结束标记写入流
            serializer.WriteFieldStop();
            //什么都不做
            serializer.WriteStructEnd();
        }

        /// <summary>
        /// 读取方法执行结果
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static object ReadResult(TSerializer serializer, DynMethodInstance instance)
        {
            TField field;
            object result = null;
            //返回Tstruct的实例,这里的作用是从流里面读出Tstruct的name
            serializer.ReadStructBegin();

            while (true)
            {
                //开始读取field 
                field = serializer.ReadFieldBegin();
                //一直读取,直到读到的field类型是TType.Stop 完成field的读取
                if (field.Type == TType.Stop)
                {
                    break;
                }
                //判断返回结果的集合类型根据类型的不同分别处理
                switch (instance.DynMethod.Result.CollectionType)
                {
                    case CollectionType.None:
                        switch (instance.DynMethod.Result.DynType)
                        {
                            //如果不是集合类型判断是不是基本类型或者是对象类型Struct 基本类型可以直接用读取基本类型的方法读取,对象类型用
                            case DynType.Bool:
                                result = serializer.ReadBool();
                                break;
                            case DynType.Byte:
                                result = serializer.ReadByte();
                                break;
                            case DynType.Double:
                                result = serializer.ReadDouble();
                                break;
                            case DynType.Decimal:
                                result = serializer.ReadDecimal();
                                break;
                            case DynType.I16:
                                result = serializer.ReadI16();
                                break;
                            case DynType.I32:
                                result = serializer.ReadI32();
                                break;
                            case DynType.I64:
                                result = serializer.ReadI64();
                                break;
                            case DynType.String:
                                result = serializer.ReadString();
                                break;
                            case DynType.DateTime:
                                string dt = serializer.ReadString();
                                result = Convert.ToDateTime(dt);
                                break;
                            case DynType.Binary:
                                result = serializer.ReadBinary();
                                break;
                            //对象类型的读取
                            case DynType.Struct:
                                result = DynSerialize.ReadDynObject(serializer);
                                break;
                            default:
                                TSerializerUtil.Skip(serializer, field.Type);
                                break;
                        }
                        break;
                    case CollectionType.List:
                        result = DynSerialize.ReadList(serializer, instance.DynMethod.Result.StructName);
                        break;
                    case CollectionType.Set:
                        break;
                    case CollectionType.Map:
                        result = DynSerialize.ReadMap(serializer);
                        break;
                    default:
                        break;
                }

                serializer.ReadFieldEnd();
            }
            serializer.ReadStructEnd();

            return result;
        }

        /// <summary>
        /// 写入方法执行结果
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="instance"></param>
        public static void WriteResult(TSerializer serializer, DynMethodInstance instance)
        {
            TStruct struc = new TStruct(instance.FullName);
            serializer.WriteStructBegin(struc);
            TField field = new TField();

            if (instance.DynMethod.Result.Name != null && instance.Result != null)
            {
                field.Name = instance.DynMethod.Result.Name;
                TType temp = (TType)(byte)instance.DynMethod.Result.DynType;
                if (temp != TType.Void)
                {
                    field.ID = instance.DynMethod.Result.ID;

                    switch (instance.DynMethod.Result.CollectionType)
                    {
                        case CollectionType.None:
                            field.Type = temp;
                            serializer.WriteFieldBegin(field);

                            switch (field.Type)
                            {
                                case TType.Stop:
                                    break;
                                case TType.Void:
                                    break;
                                case TType.Bool:
                                    serializer.WriteBool((Boolean)instance.Result);
                                    break;
                                case TType.Byte:
                                    serializer.WriteByte((Byte)instance.Result);
                                    break;
                                case TType.Double:
                                    serializer.WriteDouble((Double)instance.Result);
                                    break;
                                case TType.Decimal:
                                    serializer.WriteDecimal((Decimal)instance.Result);
                                    break;
                                case TType.I16:
                                    serializer.WriteI16((Int16)instance.Result);
                                    break;
                                case TType.I32:
                                    serializer.WriteI32((Int32)instance.Result);
                                    break;
                                case TType.I64:
                                    serializer.WriteI64((Int64)instance.Result);
                                    break;
                                case TType.String:
                                    string value = instance.Result == null ? "" : instance.Result.ToString();
                                    serializer.WriteString(value);
                                    break;
                                case TType.DateTime:
                                    string dt = Convert.ToDateTime(instance.Result).ToString("yyyy-MM-dd HH:mm:ss");
                                    serializer.WriteString(dt);
                                    break;
                                case TType.Binary:
                                    serializer.WriteBinary((byte[])instance.Result);
                                    break;
                                case TType.Struct:
                                    DynSerialize.WriteDynObject(serializer, instance.Result as DynObject);
                                    break;
                                case TType.Map:
                                    //oprot.WriteString(this[parameter.Name] as string);
                                    break;
                                case TType.Set:
                                    //oprot.WriteString(this[parameter.Name] as string);
                                    break;
                                case TType.List:
                                    //oprot.WriteString(this[parameter.Name] as string);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case CollectionType.List:
                            field.Type = TType.List;
                            serializer.WriteFieldBegin(field);
                            DynSerialize.WriteList(serializer, instance.Result, instance.DynMethod.Result.DynType);
                            break;
                        case CollectionType.Set:
                            field.Type = TType.Set;
                            serializer.WriteFieldBegin(field);
                            break;
                        case CollectionType.Map:
                            field.Type = TType.Map;
                            serializer.WriteFieldBegin(field);
                            DynSerialize.WriteMap(serializer, instance.Result as IDictionary);
                            break;
                        default:
                            break;
                    }
                    serializer.WriteFieldEnd();
                }
            }
            serializer.WriteFieldStop();
            serializer.WriteStructEnd();
        }

        /// <summary>
        /// 从序列化串中读取参数
        /// </summary>
        /// <param name="serializer">序列化方式</param>
        /// <param name="method">动态方法</param>
        public static void ReadParameters(TSerializer serializer, DynMethod method)
        {
            TField field;
            TStruct ts = serializer.ReadStructBegin();
            while (true)
            {
                field = serializer.ReadFieldBegin();
                if (field.Type == TType.Stop)
                {
                    break;
                }

                DynParameter parameter = method.GetParameter(field.ID);

                switch (parameter.CollectionType)
                {
                    case CollectionType.None:
                        switch (parameter.DynType)
                        {
                            case DynType.Bool:
                                parameter.Value = serializer.ReadBool();
                                break;
                            case DynType.Byte:
                                parameter.Value = serializer.ReadByte();
                                break;
                            case DynType.Double:
                                parameter.Value = serializer.ReadDouble();
                                break;
                            case DynType.Decimal:
                                parameter.Value = serializer.ReadDecimal();
                                break;
                            case DynType.I16:
                                parameter.Value = serializer.ReadI16();
                                break;
                            case DynType.I32:
                                parameter.Value = serializer.ReadI32();
                                break;
                            case DynType.I64:
                                parameter.Value = serializer.ReadI64();
                                break;
                            case DynType.String:
                                parameter.Value = serializer.ReadString();
                                break;
                            case DynType.DateTime:
                                string dt = serializer.ReadString();
                                parameter.Value = Convert.ToDateTime(dt);
                                break;
                            case DynType.Binary:
                                parameter.Value = serializer.ReadBinary();
                                break;
                            case DynType.Struct:
                                parameter.Value = DynSerialize.ReadDynObject(serializer);
                                break;
                            default:
                                TSerializerUtil.Skip(serializer, field.Type);
                                break;
                        }
                        break;
                    case CollectionType.List:
                        parameter.Value = DynSerialize.ReadList(serializer, parameter.StructName);
                        break;
                    case CollectionType.Set:
                        break;
                    case CollectionType.Map:
                        parameter.Value = DynSerialize.ReadMap(serializer);
                        break;
                    default:
                        break;
                }

                serializer.ReadFieldEnd();
            }
            serializer.ReadStructEnd();
        }

        /// <summary>
        /// 向序列化串中写入动态方法
        /// </summary>
        /// <param name="serializer">序列化方式</param>
        /// <param name="method">动态方法</param>
        public static void WriteParameters(TSerializer serializer, DynMethod method)
        {
            TStruct struc = new TStruct(method.Name);

            serializer.WriteStructBegin(struc);
            TField field = new TField();

            foreach (DynParameter parameter in method.Parameters.Values)
            {
                if (!string.IsNullOrEmpty(parameter.Name))
                {
                    field.Name = parameter.Name;
                    field.ID = parameter.ID;

                    switch (parameter.CollectionType)
                    {
                        case CollectionType.None:
                            field.Type = (TType)(byte)parameter.DynType;
                            serializer.WriteFieldBegin(field);

                            switch (field.Type)
                            {
                                case TType.Stop:
                                    break;
                                case TType.Void:
                                    break;
                                case TType.Bool:
                                    serializer.WriteBool((Boolean)parameter.Value);
                                    break;
                                case TType.Byte:
                                    serializer.WriteByte((Byte)parameter.Value);
                                    break;
                                case TType.Double:
                                    serializer.WriteDouble((Double)parameter.Value);
                                    break;
                                case TType.Decimal:
                                    serializer.WriteDecimal((Decimal)parameter.Value);
                                    break;
                                case TType.I16:
                                    serializer.WriteI16((Int16)parameter.Value);
                                    break;
                                case TType.I32:
                                    serializer.WriteI32((Int32)parameter.Value);
                                    break;
                                case TType.I64:
                                    serializer.WriteI64((Int64)parameter.Value);
                                    break;
                                case TType.String:
                                    serializer.WriteString(parameter.Value.ToString());
                                    break;
                                case TType.DateTime:
                                    string dt = Convert.ToDateTime(parameter.Value).ToString("yyyy-MM-dd HH:mm:ss");
                                    serializer.WriteString(dt);
                                    break;
                                case TType.Binary:
                                    serializer.WriteBinary((byte[])parameter.Value);
                                    break;
                                case TType.Struct:
                                    DynSerialize.WriteDynObject(serializer, parameter.Value as DynObject);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case CollectionType.List:
                            field.Type = TType.List;
                            serializer.WriteFieldBegin(field);

                            DynSerialize.WriteList(serializer, parameter.Value, parameter.DynType);
                            break;
                        case CollectionType.Set:
                            field.Type = TType.Set;
                            serializer.WriteFieldBegin(field);

                            break;
                        case CollectionType.Map:
                            field.Type = TType.Map;
                            serializer.WriteFieldBegin(field);

                            DynSerialize.WriteMap(serializer, parameter.Value as IDictionary);
                            break;
                        default:
                            break;
                    }

                    serializer.WriteFieldEnd();
                }
            }
            serializer.WriteFieldStop();
            serializer.WriteStructEnd();
        }

        /// <summary>
        /// 从序列化串中读取返回结果
        /// </summary>
        /// <param name="serializer">序列化方式</param>
        /// <param name="method">动态方法</param>
        public static void ReadResult(TSerializer serializer, DynMethod method)
        {
            TField field;
            serializer.ReadStructBegin();
            while (true)
            {
                field = serializer.ReadFieldBegin();
                if (field.Type == TType.Stop)
                {
                    break;
                }

                switch (method.Result.CollectionType)
                {
                    case CollectionType.None:
                        switch (method.Result.DynType)
                        {
                            case DynType.Bool:
                                method.Result.Value = serializer.ReadBool();
                                break;
                            case DynType.Byte:
                                method.Result.Value = serializer.ReadByte();
                                break;
                            case DynType.Double:
                                method.Result.Value = serializer.ReadDouble();
                                break;
                            case DynType.Decimal:
                                method.Result.Value = serializer.ReadDecimal();
                                break;
                            case DynType.I16:
                                method.Result.Value = serializer.ReadI16();
                                break;
                            case DynType.I32:
                                method.Result.Value = serializer.ReadI32();
                                break;
                            case DynType.I64:
                                method.Result.Value = serializer.ReadI64();
                                break;
                            case DynType.String:
                                method.Result.Value = serializer.ReadString();
                                break;
                            case DynType.DateTime:
                                string dt = serializer.ReadString();
                                method.Result.Value = Convert.ToDateTime(dt);
                                break;
                            case DynType.Binary:
                                method.Result.Value = serializer.ReadBinary();
                                break;
                            case DynType.Struct:
                                method.Result.Value = DynSerialize.ReadDynObject(serializer);
                                break;
                            default:
                                TSerializerUtil.Skip(serializer, field.Type);
                                break;
                        }
                        break;
                    case CollectionType.List:
                        method.Result.Value = DynSerialize.ReadList(serializer, method.Result.StructName);
                        break;
                    case CollectionType.Set:
                        break;
                    case CollectionType.Map:
                        method.Result.Value = DynSerialize.ReadMap(serializer);
                        break;
                    default:
                        break;
                }

                serializer.ReadFieldEnd();
            }
            serializer.ReadStructEnd();
        }

        /// <summary>
        /// 向序列化串中写入返回结果
        /// </summary>
        /// <param name="serializer">序列化方式</param>
        /// <param name="method">动态方法</param>
        public static void WriteResult(TSerializer serializer, DynMethod method)
        {
            TStruct struc = new TStruct(method.ClassName + "_" + method.Name);
            serializer.WriteStructBegin(struc);
            TField field = new TField();

            if (method.Result.Name != null)
            {
                field.Name = method.Result.Name;
                TType temp = (TType)(byte)method.Result.DynType;
                if (temp != TType.Void)
                {
                    field.ID = method.Result.ID;

                    switch (method.Result.CollectionType)
                    {
                        case CollectionType.None:
                            field.Type = temp;
                            serializer.WriteFieldBegin(field);

                            switch (field.Type)
                            {
                                case TType.Stop:
                                    break;
                                case TType.Void:
                                    break;
                                case TType.Bool:
                                    serializer.WriteBool((Boolean)method.Result.Value);
                                    break;
                                case TType.Byte:
                                    serializer.WriteByte((Byte)method.Result.Value);
                                    break;
                                case TType.Double:
                                    serializer.WriteDouble((Double)method.Result.Value);
                                    break;
                                case TType.Decimal:
                                    serializer.WriteDecimal((Decimal)method.Result.Value);
                                    break;
                                case TType.I16:
                                    serializer.WriteI16((Int16)method.Result.Value);
                                    break;
                                case TType.I32:
                                    serializer.WriteI32((Int32)method.Result.Value);
                                    break;
                                case TType.I64:
                                    serializer.WriteI64((Int64)method.Result.Value);
                                    break;
                                case TType.String:
                                    serializer.WriteString(method.Result.Value.ToString());
                                    break;
                                case TType.DateTime:
                                    string dt = Convert.ToDateTime(method.Result.Value).ToString("yyyy-MM-dd HH:mm:ss");
                                    serializer.WriteString(dt);
                                    break;
                                case TType.Binary:
                                    serializer.WriteBinary((byte[])method.Result.Value);
                                    break;
                                case TType.Struct:
                                    DynSerialize.WriteDynObject(serializer, method.Result.Value as DynObject);
                                    break;
                                case TType.Map:
                                    //oprot.WriteString(this[parameter.Name] as string);
                                    break;
                                case TType.Set:
                                    //oprot.WriteString(this[parameter.Name] as string);
                                    break;
                                case TType.List:
                                    //oprot.WriteString(this[parameter.Name] as string);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case CollectionType.List:
                            field.Type = TType.List;
                            serializer.WriteFieldBegin(field);
                            DynSerialize.WriteList(serializer, method.Result.Value, method.Result.DynType);
                            break;
                        case CollectionType.Set:
                            field.Type = TType.Set;
                            serializer.WriteFieldBegin(field);
                            break;
                        case CollectionType.Map:
                            field.Type = TType.Map;
                            serializer.WriteFieldBegin(field);
                            DynSerialize.WriteMap(serializer, method.Result.Value as IDictionary);
                            break;
                        default:
                            break;
                    }
                    serializer.WriteFieldEnd();
                }
            }
            serializer.WriteFieldStop();
            serializer.WriteStructEnd();
        }

        /// <summary>
        /// 从序列化串中读取列表（暂不支持集合类型中再包含集合类型）
        /// </summary>
        /// <param name="serializer">序列化方式</param>
        /// <param name="structName">对象名</param>
        /// <returns>列表对象</returns>
        public static object ReadList(TSerializer serializer, string structName)
        {
            object obj = null;

            TList tList = serializer.ReadListBegin();

            switch ((DynType)(byte)tList.ElementType)
            {
                case DynType.Bool:
                    List<bool> objBools = new List<bool>();

                    for (int i = 0; i < tList.Count; i++)
                    {
                        objBools.Add(serializer.ReadBool());
                    }
                    obj = objBools;
                    break;
                case DynType.Byte:
                    List<byte> objBytes = new List<byte>();

                    for (int i = 0; i < tList.Count; i++)
                    {
                        objBytes.Add(serializer.ReadByte());
                    }
                    obj = objBytes;
                    break;
                case DynType.Double:
                    List<double> objDoubles = new List<double>();
                    for (int i = 0; i < tList.Count; i++)
                    {
                        objDoubles.Add(serializer.ReadDouble());
                    }
                    obj = objDoubles;
                    break;
                case DynType.Decimal:
                    List<decimal> objDecimals = new List<decimal>();
                    for (int i = 0; i < tList.Count; i++)
                    {
                        objDecimals.Add(serializer.ReadDecimal());
                    }
                    obj = objDecimals;
                    break;
                case DynType.I16:
                    List<short> objI16s = new List<short>();
                    for (int i = 0; i < tList.Count; i++)
                    {
                        objI16s.Add(serializer.ReadI16());
                    }
                    obj = objI16s;
                    break;
                case DynType.I32:
                    List<int> objI32s = new List<int>();
                    for (int i = 0; i < tList.Count; i++)
                    {
                        objI32s.Add(serializer.ReadI32());
                    }
                    obj = objI32s;
                    break;
                case DynType.I64:
                    List<long> objI64s = new List<long>();
                    for (int i = 0; i < tList.Count; i++)
                    {
                        objI64s.Add(serializer.ReadI64());
                    }
                    obj = objI64s;
                    break;
                case DynType.String:
                    List<string> objStrs = new List<string>();
                    for (int i = 0; i < tList.Count; i++)
                    {
                        objStrs.Add(serializer.ReadString());
                    }
                    obj = objStrs;
                    break;
                case DynType.DateTime:
                    List<DateTime> objDateTimes = new List<DateTime>();
                    for (int i = 0; i < tList.Count; i++)
                    {
                        string dt = serializer.ReadString();
                        objDateTimes.Add(Convert.ToDateTime(dt));
                    }
                    obj = objDateTimes;
                    break;
                case DynType.Struct:
                    List<DynObject> objDyns = new List<DynObject>();
                    for (int i = 0; i < tList.Count; i++)
                    {
                        objDyns.Add(DynSerialize.ReadDynObject(serializer));
                    }
                    obj = objDyns;
                    break;
                default:
                    break;
            }
            serializer.ReadListEnd();

            return obj;
        }

        /// <summary>
        /// 向序列化串中写入列表（暂不支持集合类型中再包含集合类型）
        /// </summary>
        /// <param name="serializer">序列化方式</param>
        /// <param name="obj">列表对象</param>
        /// <param name="dynType">动态类型</param>
        public static void WriteList(TSerializer serializer, object obj, DynType dynType)
        {
            TType ttype = (TType)(byte)dynType;

            TList tList = new TList();
            tList.ElementType = ttype;
            tList.Count = (obj as IList).Count;

            serializer.WriteListBegin(tList);

            if (tList.Count > 0)
            {
                switch (ttype)
                {
                    case TType.Stop:
                        break;
                    case TType.Void:
                        break;
                    case TType.Bool:
                        foreach (var item in obj as ICollection<bool>)
                        {
                            serializer.WriteBool((Boolean)item);
                        }
                        break;
                    case TType.Byte:
                        //serializer.WriteBinary(obj as byte[]);
                        foreach (var item in obj as ICollection<byte>)
                        {
                            serializer.WriteByte((byte)item);
                        }
                        break;
                    case TType.Double:
                        foreach (var item in obj as ICollection<double>)
                        {
                            serializer.WriteDouble((Double)item);
                        }
                        break;
                    case TType.Decimal:
                        foreach (var item in obj as ICollection<decimal>)
                        {
                            serializer.WriteDecimal((Decimal)item);
                        }
                        break;
                    case TType.I16:
                        foreach (var item in obj as ICollection<short>)
                        {
                            serializer.WriteI16((Int16)item);
                        }
                        break;
                    case TType.I32:
                        foreach (var item in obj as ICollection<int>)
                        {
                            serializer.WriteI32((Int32)item);
                        }
                        break;
                    case TType.I64:
                        foreach (var item in obj as ICollection<long>)
                        {
                            serializer.WriteI64((Int64)item);
                        }
                        break;
                    case TType.String:
                        foreach (var item in obj as ICollection<string>)
                        {
                            serializer.WriteString(item.ToString());
                        }
                        break;
                    case TType.DateTime:
                        foreach (var item in obj as ICollection<DateTime>)
                        {
                            string dt = Convert.ToDateTime(item).ToString("yyyy-MM-dd HH:mm:ss");
                            serializer.WriteString(dt);
                        }
                        break;
                    case TType.Struct:
                        foreach (var item in obj as List<DynObject>)
                        {
                            DynSerialize.WriteDynObject(serializer, item as DynObject);
                        }

                        break;
                    default:
                        break;
                }
            }
            serializer.WriteListEnd();
        }

        /// <summary>
        /// 从序列化串中读取属性
        /// </summary>
        /// <param name="serializer">序列化方式</param>
        /// <param name="attributes">属性</param>
        public static void ReadAttributes(TSerializer serializer, List<DynObject> attributes)
        {
            TStruct struc = serializer.ReadStructBegin();

            while (true)
            {
                TField field = serializer.ReadFieldBegin();

                if (field.Type == TType.Stop)
                {
                    break;
                }
                //string className = serializer.ReadString();
                attributes.Add(ReadDynObject(serializer));
                serializer.ReadFieldEnd();
            }

            serializer.ReadStructEnd();
        }

        /// <summary>
        /// 向序列化串中写入属性
        /// </summary>
        /// <param name="serializer">序列化方式</param>
        /// <param name="attributes">属性列表</DynObject></param>
        public static void WriteAttributes(TSerializer serializer, List<DynObject> attributes)
        {
            TStruct struc = new TStruct("Attributes");
            serializer.WriteStructBegin(struc);

            TField field = new TField();
            short i = 0;
            foreach (DynObject attribute in attributes)
            {
                field.ID = i++;
                field.Type = TType.Struct;
                field.Name = attribute.DynClass.Name;
                serializer.WriteFieldBegin(field);
                //serializer.WriteString(attribute.DynClass.Name);
                WriteDynObject(serializer, attribute);
                serializer.WriteFieldEnd();
            }

            serializer.WriteStructEnd();
        }

        public static Dictionary<string, object> ReadMap(TSerializer serializer)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            TMap map = serializer.ReadMapBegin();
            for (int i = 0; i < map.Count; i++)
            {
                string key = serializer.ReadString();
                object value = ReadObj(serializer);
                dict.Add(key, value);
            }

            serializer.ReadMapEnd();
            return dict;
        }

        public static void WriteMap(TSerializer serializer, IDictionary dict)
        {
            TMap map = new TMap();
            map.KeyType = TType.String;
            map.ValueType = TType.Struct;
            map.Count = dict.Count;
            serializer.WriteMapBegin(map);

            if (dict != null)
            {
                foreach (DictionaryEntry dictitem in (dict as IDictionary))
                {
                    serializer.WriteString(dictitem.Key as string);
                    //value
                    WriteObj(serializer, dictitem.Value);
                }
            }

            serializer.WriteMapEnd();
        }

        public static Object ReadObj(TSerializer serializer)
        {
            TField field;
            TStruct tstruct = serializer.ReadStructBegin();
            object obj = null;

            while (true)
            {
                field = serializer.ReadFieldBegin();
                if (field.Type == TType.Stop)
                {
                    break;
                }

                CollectionType collection = (CollectionType)(byte)(field.ID / CalculationBase);
                DynType dynType = (DynType)(byte)(field.ID % CalculationBase);
                switch (collection)
                {
                    case CollectionType.None:
                        switch (dynType)
                        {
                            case DynType.Bool:
                                obj = serializer.ReadBool();
                                break;
                            case DynType.Byte:
                                obj = serializer.ReadByte();
                                break;
                            case DynType.Double:
                                obj = serializer.ReadDouble();
                                break;
                            case DynType.Decimal:
                                obj = serializer.ReadDecimal();
                                break;
                            case DynType.I16:
                                obj = serializer.ReadI16();
                                break;
                            case DynType.I32:
                                obj = serializer.ReadI32();
                                break;
                            case DynType.I64:
                                obj = serializer.ReadI64();
                                break;
                            case DynType.String:
                                obj = serializer.ReadString();
                                break;
                            case DynType.DateTime:
                                string dt = serializer.ReadString();
                                obj = Convert.ToDateTime(dt);
                                break;
                            case DynType.Binary:
                                obj = serializer.ReadBinary();
                                break;
                            case DynType.Struct:
                                obj = DynSerialize.ReadDynObject(serializer);
                                break;
                            default:
                                TSerializerUtil.Skip(serializer, field.Type);
                                break;
                        }
                        break;

                    case CollectionType.List:
                        obj = DynSerialize.ReadList(serializer, "Object");
                        break;

                    case CollectionType.Set:
                        break;

                    case CollectionType.Map:
                        obj = DynSerialize.ReadMap(serializer);
                        break;

                    default:
                        break;
                }
                serializer.ReadFieldEnd();
            }
            serializer.ReadStructEnd();

            return obj;

        }

        public static void WriteObj(TSerializer serializer, object value)
        {
            TStruct struc = new TStruct("Obj");
            CollectionType collectionType;
            DynType dynType;
            string structName;

            //value
            serializer.WriteStructBegin(struc);

            if (!IsNullOrEmptyValue(value))
            {
                try
                {
                    GetTypeDynType(value, out collectionType, out dynType, out structName);
                }
                catch
                {
                    return;
                }
                TField field = new TField();
                field.Name = "obj";
                field.ID = (short)((byte)collectionType * CalculationBase + (byte)dynType);

                switch (collectionType)
                {
                    case CollectionType.None:
                        field.Type = (TType)(byte)dynType;
                        serializer.WriteFieldBegin(field);
                        switch (field.Type)
                        {
                            case TType.Bool:
                                serializer.WriteBool((Boolean)value);
                                break;
                            case TType.Byte:
                                serializer.WriteByte((Byte)value);
                                break;
                            case TType.Double:
                                serializer.WriteDouble((Double)value);
                                break;
                            case TType.Decimal:
                                serializer.WriteDecimal((Decimal)value);
                                break;
                            case TType.I16:
                                serializer.WriteI16((Int16)value);
                                break;
                            case TType.I32:
                                serializer.WriteI32((Int32)value);
                                break;
                            case TType.I64:
                                serializer.WriteI64((Int64)value);
                                break;
                            case TType.String:
                                serializer.WriteString(value.ToString());
                                break;
                            case TType.DateTime:
                                string dt = Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss");
                                serializer.WriteString(dt);
                                break;
                            case TType.Binary:
                                serializer.WriteBinary((byte[])value);
                                break;
                            case TType.Struct:
                                DynSerialize.WriteDynObject(serializer, value as DynObject);
                                break;
                            default:
                                break;
                        }
                        break;
                    case CollectionType.List:
                        field.Type = TType.List;
                        serializer.WriteFieldBegin(field);
                        DynSerialize.WriteList(serializer, value, dynType);
                        break;

                    case CollectionType.Map:
                        field.Type = TType.Map;
                        serializer.WriteFieldBegin(field);
                        DynSerialize.WriteMap(serializer, value as IDictionary);
                        break;
                    case CollectionType.Set:
                        break;
                    default:
                        break;
                }
                serializer.WriteFieldEnd();

            }

            serializer.WriteFieldStop();

            serializer.WriteStructEnd();
        }

        /// <summary>
        /// 使用此东西前 请判断是否为空或空集合 为空不能判断类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="collectionType"></param>
        /// <param name="dynType"></param>
        /// <param name="structName"></param>
        public static void GetTypeDynType(object obj, out CollectionType collectionType, out DynType dynType, out string structName)
        {
            if (IsNullOrEmptyValue(obj))
            {
                throw new ApplicationException("不能对空对象或者空数组检测类型");
            }
            structName = null;
            var sample = obj;

            if (obj is IList)
            {
                collectionType = CollectionType.List;
                for (int i = 0; i < (obj as IList).Count; i++)
                {
                    sample = (obj as IList)[i];
                    if (!IsNullOrEmptyValue(sample))
                    {
                        break; ;
                    }
                }

            }
            else if (obj is IDictionary)
            {
                collectionType = CollectionType.Map;
                foreach (DictionaryEntry dictionaryEntry in (obj as IDictionary))
                {
                    sample = dictionaryEntry.Value;
                    if (!IsNullOrEmptyValue(sample))
                    {
                        break; ;
                    }
                }
            }
            else if (obj is ICollection)
            {
                collectionType = CollectionType.Set;
                foreach (var value in (obj as ICollection))
                {
                    sample = value;
                    if (!IsNullOrEmptyValue(sample))
                    {
                        break; ;
                    }
                }
            }
            else
            {
                collectionType = CollectionType.None;
                sample = obj;
            }

            if (!IsNullOrEmptyValue(sample))
            {
                if (sample is Boolean)
                {
                    dynType = DynType.Bool;
                }
                else if (sample is Byte)
                {
                    dynType = DynType.Byte;
                }
                else if (sample is Double)
                {
                    dynType = DynType.Double;
                }
                else if (sample is Int16)
                {
                    dynType = DynType.I16;
                }
                else if (sample is Int32)
                {
                    dynType = DynType.I32;
                }
                else if (sample is Int64)
                {
                    dynType = DynType.I64;
                }
                else if (sample is String)
                {
                    dynType = DynType.String;
                }
                else if (sample is DynObject)
                {
                    dynType = DynType.Struct;
                    structName = (sample as DynObject).DynClass.Name;
                }
                else if (sample is DateTime)
                {
                    dynType = DynType.DateTime;
                }
                else if (sample is Byte[])
                {
                    dynType = DynType.Binary;
                }
                else if (sample is Decimal)
                {
                    dynType = DynType.Decimal;
                }
                else
                {
                    dynType = DynType.Struct;
                    structName = "Object";
                }
            }
            else
            {
                throw new ApplicationException("不能对只由空对象或者空数组组成的对象检测类型");
            }
        }

        public static bool IsNullOrEmptyValue(object obj)
        {
            return obj == null || (obj is ICollection && (obj as ICollection).Count == 0);
        }

    }
}
