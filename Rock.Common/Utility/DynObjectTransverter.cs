using Rock.Dyn.Comm;
using Rock.Dyn.Core;
using Rock.Dyn.Msg;
using Rock.Orm.Common;
using Rock.Orm.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Common
{
    public static class DynObjectTransverter
    {
        public static string DynObjectListToJson(List<DynObject> value)
        {
            if (value == null)
            {
                return null;
            }
            try
            {
                TSerializer serializerJsonReq = new TJSONSerializer();
                DynSerialize.WriteAttributes(serializerJsonReq, value);
                byte[] jsonBytes = serializerJsonReq.ToBytes();
                return Encoding.UTF8.GetString(jsonBytes);
            }
            catch (Exception ex)
            {
                ex.Data.Add("DynObjectList", value);
                throw ex;
            }
        }

        public static List<DynObject> JsonToDynObjectList(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new List<DynObject>();
            }
            try
            {
                List<DynObject> dynObjs = new List<DynObject>();
                TSerializer serializerJsonReq = new TJSONSerializer();
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                serializerJsonReq.FromBytes(jsonBytes);

                DynSerialize.ReadAttributes(serializerJsonReq, dynObjs);
                return dynObjs;
            }
            catch (Exception ex)
            {
                ex.Data.Add("Json", json);
                throw ex;
            }
        }

        public static string DynObjectToJson(DynObject dynObject)
        {
            if (dynObject == null)
            {
                return null;
            }
            try
            {
                List<DynObject> values = new List<DynObject>();
                values.Add(dynObject);
                TSerializer serializerJsonReq = new TJSONSerializer();
                DynSerialize.WriteAttributes(serializerJsonReq, values);
                byte[] jsonBytes = serializerJsonReq.ToBytes();
                return Encoding.UTF8.GetString(jsonBytes);
            }
            catch (Exception ex)
            {
                ex.Data.Add("DynObject", dynObject);
                throw ex;
            }
        }

        public static DynObject JsonToDynObject(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            try
            {
                List<DynObject> dynObjs = new List<DynObject>();
                if (!string.IsNullOrEmpty(json))
                {
                    TSerializer serializerJsonReq = new TJSONSerializer();
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                    serializerJsonReq.FromBytes(jsonBytes);
                    DynSerialize.ReadAttributes(serializerJsonReq, dynObjs);

                    return dynObjs[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("Json", json);
                throw ex;
            }
        }

        public static DynObject ToTransDataTable(DataTable dataTable)
        {
            DynObject dt = new DynObject("DataTable");
            List<DynObject> dcList = new List<DynObject>();
            if (dataTable != null)
            {
                List<DynObject> rows = new List<DynObject>();
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    DynObject dr = new DynObject("DataRow");

                    List<string> row = new List<string>();
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        row.Add(dataTable.Rows[i][j].ToString());
                    }
                    dr.SetPropertyValue("Values", row);

                    rows.Add(dr);
                }

                dt.SetPropertyValue("Rows", rows);

                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    DynObject dc = new DynObject("DataColumn");
                    dc.SetPropertyValue("Name", dataTable.Columns[j].ColumnName);
                    dc.SetPropertyValue("Type", dataTable.Columns[j].DataType.ToString());
                    dcList.Add(dc);
                }
            }
            dt.SetPropertyValue("Columns", dcList);
            return dt;
        }

        public static DynObject StaticToDynObj(object instance)
        {
            if (instance == null)
            {
                return null;
            }
            Type type = instance.GetType();
            if (DynTypeManager.ContainsStaticType(type.Name))
            {

                DynObject dynObj = new DynObject(type.Name);

                DynProperty[] dynProperties = DynTypeManager.GetClass(type.Name).GetProperties();
                foreach (var item in dynProperties)
                {
                    //TODO:这里对递归序列化进行了一个限制,防止循环引用 (所有对象类型的不再进行序列化,仅序列化列表类型的对象)
                     if (item.DynType != DynType.Struct || (item.DynType == DynType.Struct && item.CollectionType == CollectionType.List))
                     {
                         PropertyInfo propertyInfo = type.GetProperty(item.Name);
                         if (propertyInfo != null)
                         {
                             switch (item.CollectionType)
                             {
                                 case CollectionType.List:
                                     switch (item.DynType)
                                     {
                                         case DynType.Bool:
                                         case DynType.Byte:
                                         case DynType.DateTime:
                                         case DynType.Double:
                                         case DynType.Decimal:
                                         case DynType.I16:
                                         case DynType.I32:
                                         case DynType.I64:
                                         case DynType.String:
                                             dynObj[item.Name] = propertyInfo.GetValue(instance, null);
                                             break;
                                         case DynType.Struct:
                                             IEnumerable list = propertyInfo.GetValue(instance, null) as IEnumerable;
                                             if (list != null)
                                             {
                                                 List<DynObject> dynList = new List<DynObject>();
                                                 foreach (object structValue in list)
                                                 {
                                                     dynList.Add(StaticToDynObj(structValue));
                                                 }
                                                 dynObj[item.Name] = dynList;
                                             }
                                             break;
                                         case DynType.Void:
                                             break;
                                         case DynType.Binary:
                                             break;
                                         default:
                                             break;
                                     }
                                     break;
                                 case CollectionType.Map:
                                     dynObj[item.Name] = propertyInfo.GetValue(instance, null);
                                     break;
                                 case CollectionType.None:
                                     var value = propertyInfo.GetValue(instance, null);
                                     if (value != null)
                                     {
                                         switch (item.DynType)
                                         {
                                             case DynType.Binary:
                                             case DynType.Bool:
                                             case DynType.Byte:
                                             case DynType.DateTime:
                                             case DynType.Double:
                                             case DynType.Decimal:
                                             case DynType.I16:
                                             case DynType.I32:
                                             case DynType.I64:
                                             case DynType.String:
                                                 dynObj[item.Name] = value;
                                                 break;
                                             case DynType.Struct:
                                                 dynObj[item.Name] = StaticToDynObj(value);
                                                 break;
                                             case DynType.Void:
                                                 break;
                                             default:
                                                 break;
                                         }
                                     }
                                     break;
                                 case CollectionType.Set:
                                     break;
                                 default:
                                     break;
                             }
                         }
                     }
                }
                return dynObj;
            }
            return null;
        }

        public static object DynObjToStatic(object dynObj)
        {
            if (dynObj == null)
            {
                return null;
            }

            string typeName = (dynObj as DynObject).DynClass.Name;
            if (DynTypeManager.ContainsStaticType(typeName))
            {
                Type type = DynTypeManager.GetStaticType(typeName);

                object instance = Activator.CreateInstance(type, null);

                PropertyInfo[] propertyInfos = type.GetProperties();
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    DynProperty dp = (dynObj as DynObject).DynClass.GetProperty(propertyInfo.Name);

                    switch (dp.CollectionType)
                    {
                        case CollectionType.List:
                            switch (dp.DynType)
                            {
                                case DynType.Bool:
                                case DynType.Byte:
                                case DynType.DateTime:
                                case DynType.Double:
                                case DynType.Decimal:
                                case DynType.I16:
                                case DynType.I32:
                                case DynType.I64:
                                case DynType.String:
                                    propertyInfo.SetValue(instance, (dynObj as DynObject)[propertyInfo.Name], null);
                                    break;
                                case DynType.Struct:
                                    IEnumerable list = (dynObj as DynObject)[propertyInfo.Name] as IEnumerable;

                                    string listTypeString = "System.Collections.Generic.List`1[{0}]";

                                    string fullName = string.Format(listTypeString, DynTypeManager.GetStaticType(dp.StructName).FullName);

                                    Type listType = Type.GetType(fullName);

                                    IList listValue = Activator.CreateInstance(listType, null) as IList;

                                    foreach (var item in list)
                                    {
                                        listValue.Add(DynObjToStatic(item));
                                    }
                                    propertyInfo.SetValue(instance, listValue, null);
                                    break;
                                case DynType.Void:
                                    break;
                                case DynType.Binary:
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case CollectionType.Map:
                            break;
                        case CollectionType.None:
                            switch (dp.DynType)
                            {
                                case DynType.Bool:
                                case DynType.Binary:
                                case DynType.Byte:
                                case DynType.DateTime:
                                case DynType.Double:
                                case DynType.Decimal:
                                case DynType.I16:
                                case DynType.I32:
                                case DynType.I64:
                                case DynType.String:
                                    propertyInfo.SetValue(instance, (dynObj as DynObject)[propertyInfo.Name], null);
                                    break;
                                case DynType.Struct:

                                    object value = DynObjToStatic((dynObj as DynObject)[propertyInfo.Name]);

                                    if (value == null && type.GetProperty(dp.Name + "ID") != null && type.GetProperty(dp.Name + "ID").GetValue(instance, null) != null)
                                    {
                                        //一对多不处理
                                    }
                                    else
                                    {
                                        propertyInfo.SetValue(instance, DynObjToStatic((dynObj as DynObject)[propertyInfo.Name]), null);
                                    }

                                    break;
                                case DynType.Void:
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case CollectionType.Set:
                            break;
                        default:
                            break;
                    }


                }

                return instance;
            }

            return null;
        }

        /// <summary>
        /// 把动态对象转成静态实体对象
        /// </summary>
        /// <param name="dynObj"></param>
        /// <returns></returns>
        public static object DynObjectToStatic(DynObject dynObj)
        {
            if (dynObj == null)
            {
                return null;
            }
            string typeName = dynObj.DynClass.Name;

            Type type = DynTypeManager.GetStaticType(typeName);
            object typeInstance = Activator.CreateInstance(type, null);

            MethodInfo attach = type.GetMethod("Attach");
            if (attach != null)
            {
                attach.Invoke(typeInstance, null);
            }
            PropertyInfo property;

            foreach (DynProperty dynProperty in dynObj.DynClass.GetProperties())
            {
                switch (dynProperty.CollectionType)
                {
                    case CollectionType.None:

                        if (dynProperty.DynType == DynType.Struct)
                        {

                            property = type.GetProperty(dynProperty.Name);

                            object value = DynObjectToStatic(dynObj[dynProperty.Name] as DynObject);

                            if (value == null && type.GetProperty(dynProperty.Name + "ID") != null && type.GetProperty(dynProperty.Name + "ID").GetValue(typeInstance, null) != null)
                            {
                                //一对多不处理
                            }
                            else
                            {
                                property.SetValue(typeInstance, value, null);
                            }

                        }
                        else
                        {
                            property = type.GetProperty(dynProperty.Name);

                            property.SetValue(typeInstance, dynObj[dynProperty.Name], null);
                        }
                        break;
                    case CollectionType.Map:
                        property = type.GetProperty(dynProperty.Name);
                        property.SetValue(typeInstance, dynObj[dynProperty.Name], null);
                        break;
                    case CollectionType.List:
                        property = type.GetProperty(dynProperty.Name);
                        Type propertyType = property.PropertyType;

                        if (dynProperty.DynType == DynType.Struct)
                        {
                            IEnumerable list = (dynObj as DynObject)[property.Name] as IEnumerable;

                            string listTypeString = "System.Collections.Generic.List`1[{0}]";
                            Type basePropertyType = DynTypeManager.GetStaticType(dynProperty.StructName);
                            string baseFullName = basePropertyType.FullName;
                            string fullListName = string.Format(listTypeString, baseFullName);
                            Type listType = propertyType.GetType().Assembly.GetType(fullListName);

                            //附加集合后可以构建 譬如int 附加后int[] 可以被构建
                            if (listType != null && listType.GetMethod("Add") != null)
                            {
                                MethodInfo addMethod = listType.GetMethod("Add");
                                object listValue = Activator.CreateInstance(listType, null);
                                foreach (var item in list)
                                {
                                    var childValue = DynObjToStatic(item);
                                    addMethod.Invoke(listValue, new object[] { childValue });
                                }

                                property.SetValue(typeInstance, listValue, null);
                            }
                            //自带集合类型 譬如 ActionListArray:IList 可以被构建
                            else if (propertyType.GetMethod("Add") != null)
                            {
                                MethodInfo addMethod = propertyType.GetMethod("Add");
                                object listValue = Activator.CreateInstance(propertyType, null);
                                foreach (var item in list)
                                {
                                    var childValue = DynObjToStatic(item);
                                    addMethod.Invoke(listValue, new object[] { childValue });
                                }

                                property.SetValue(typeInstance, listValue, null);
                            }
                            //if (listType != null)
                            //{
                            //    IList listValue = Activator.CreateInstance(listType, null) as IList;

                            //    foreach (var item in list)
                            //    {
                            //        listValue.Add(DynObjToStatic(item));
                            //    }
                            //    property.SetValue(typeInstance, listValue, null);
                            //}
                            else
                            {
                                int lengt = 0;
                                foreach (var item in list)
                                {
                                    lengt++;
                                }
                                object array = new object();
                                array = propertyType.InvokeMember("Set", BindingFlags.CreateInstance, null, array, new object[] { lengt });
                                if (array != null)
                                {
                                    lengt = 0;
                                    foreach (var item in list)
                                    {
                                        var itemValue = DynObjToStatic(item);
                                        propertyType.GetMethod("SetValue", new Type[2] { typeof(object), typeof(int) }).Invoke(array, new object[] { itemValue, lengt });
                                        lengt++;
                                    }
                                    property.SetValue(typeInstance, array, null);
                                }
                            }
                        }
                        else
                        {

                            property.SetValue(typeInstance, dynObj[dynProperty.Name], null);
                        }
                        break;
                }
            }

            return typeInstance;
        }

        public static string SimpleListToJson(IEnumerable list, DynType type)
        {
            if (list == null)
            {
                return null;
            }
            TSerializer serializerJsonReq = new TJSONSerializer();
            DynSerialize.WriteList(serializerJsonReq, list, type);
            byte[] jsonBytes = serializerJsonReq.ToBytes();
            return Encoding.UTF8.GetString(jsonBytes);
        }

        public static IEnumerable JsonToSimpleList(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            TSerializer serializerJsonReq = new TJSONSerializer();
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            serializerJsonReq.FromBytes(jsonBytes);
            IEnumerable list = DynSerialize.ReadList(serializerJsonReq, null) as IEnumerable;

            return list;
        }

        public static DynEntity DynObject2DynEntity(DynObject dynObject)
        {
            string[] sysClassName = { "Application", "Class", "Property", "Method", "Module" };
            if (dynObject == null)
            {
                return null;
            }

            //获取dynEntity上的类名
            string className = dynObject.DynClass.Name;

            //如果没有 就说明此类被重名
            // int isNew = 0;
            DynEntity dynEntity = null;//GetDynEntity(dynObject, out isNew, this.GateWay);

            List<string> pkName = new List<string>();
            foreach (DynProperty dynProperty in dynObject.DynClass.GetProperties())
            {
                DynObject[] attributes = dynProperty.Attributes;

                foreach (var attribute in attributes)
                {
                    if (attribute.DynClass.Name == "PrimaryKey")
                    {
                        pkName.Add(dynProperty.Name);
                        break;
                    }
                }
            }

            foreach (var item in sysClassName)
            {
                if (item == className && !pkName.Contains(className + "ID"))
                {

                    pkName.Add(className + "ID");
                    break;
                }
            }

            bool isNewEntity = true;
            foreach (var pkname in pkName)
            {
                if (dynObject[pkname] != null)
                {
                    isNewEntity = false;
                    break;
                }
            }

            //DynEntity returnDynEntity = null;

            if (!isNewEntity)
            {
                if (pkName.Count == 1)
                {
                    dynEntity = GatewayFactory.Default.Find(className, dynObject[pkName[0]]);
                }
                else if (pkName.Count == 2)
                {
                    dynEntity = GatewayFactory.Default.Find(className, dynObject[pkName[0]], dynObject[pkName[1]]);
                }
            }
            //isNew = 1;
            if (dynEntity == null)
            {
                dynEntity = new DynEntity(className);
                //isNew = 0;
            }

            //遍历dynObject属性赋值
            Object value = null;
            foreach (DynProperty dynProperty in dynObject.DynClass.GetProperties())
            {
                string entityPropertyName = dynProperty.Name;
                DynObject[] attributes = dynProperty.Attributes;
                bool isFK = false;
                bool isIgnore = false;
                bool isNeedSaveValue = true;

                foreach (DynObject attribute in attributes)
                {
                    switch (attribute.DynClass.Name)
                    {
                        case "MappingName":
                            string newPropertyName = attribute["Name"] as string;
                            if (dynEntity.ContainsProperty(newPropertyName))
                            {
                                entityPropertyName = newPropertyName;
                            }
                            break;
                        case "FkReverseQuery":
                            isFK = true;
                            break;
                        case "PersistIgnore":
                            isIgnore = true;
                            break;
                        default:
                            break;
                    }
                }
                if (isIgnore)
                {
                    continue;
                }

                if (isFK)
                {
                    if (entityPropertyName != dynProperty.Name)
                    {
                        entityPropertyName = dynProperty.Name;
                    }
                }

                if (dynEntity.ContainsProperty(entityPropertyName))
                {
                    bool isok = false;

                    value = dynObject[dynProperty.Name];

                    bool isStruct = false;
                    bool isList = false;

                    #region 普通版本
                    //查询类型一确认是否序列化
                    switch (dynProperty.CollectionType)
                    {
                        case CollectionType.List:
                            isList = true;

                            #region 类型转化
                            switch (dynProperty.DynType)
                            {
                                case DynType.Bool:
                                case DynType.Byte:
                                case DynType.DateTime:
                                case DynType.Double:
                                case DynType.Decimal:
                                case DynType.I16:
                                case DynType.I32:
                                case DynType.I64:
                                case DynType.String:
                                    value = DynObjectTransverter.SimpleListToJson(value as IList, dynProperty.DynType);
                                    break;
                                case DynType.Struct:
                                    isStruct = true;
                                    break;
                                case DynType.Void:
                                    break;
                                case DynType.Binary:
                                    break;
                                default:
                                    break;
                            }

                            #endregion

                            break;
                        case CollectionType.None:

                            if (dynProperty.DynType == DynType.Struct)
                            {
                                isStruct = true;
                            }

                            if (value != null)
                            {
                                DynPropertyConfiguration dynPropertyConfiguration = dynEntity.GetEntityPropertyIncludeQueryProperty(entityPropertyName);
                                #region 获取数据
                                switch (dynPropertyConfiguration.DbType)
                                {
                                    case global::System.Data.DbType.AnsiString:
                                        //当string处理 text对应的DB类型 老古董一个
                                        value = value.ToString();
                                        break;
                                    case global::System.Data.DbType.AnsiStringFixedLength:
                                        //不变长度
                                        value = value.ToString();
                                        break;
                                    case global::System.Data.DbType.Binary:
                                        if (!(value is Byte[]))
                                        {
                                            //未知操作
                                            value = null;
                                        }

                                        break;
                                    case global::System.Data.DbType.Boolean:
                                        bool resalutBool;
                                        if (!(value is Boolean))
                                        {
                                            isok = Boolean.TryParse(value.ToString(), out resalutBool);
                                            if (isok)
                                            {
                                                value = resalutBool;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Byte:
                                        Byte resalutByte;
                                        if (!(value is Byte))
                                        {
                                            isok = Byte.TryParse(value.ToString(), out resalutByte);
                                            if (isok)
                                            {
                                                value = resalutByte;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Currency:
                                        Double resalutDouble;
                                        if (!(value is Double))
                                        {
                                            isok = Double.TryParse(value.ToString(), out resalutDouble);
                                            if (isok)
                                            {
                                                value = resalutDouble;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Date:
                                        if (!(value is DateTime))
                                        {
                                            DateTime resalutDateTime;
                                            isok = DateTime.TryParse(value.ToString(), out resalutDateTime);
                                            if (isok)
                                            {
                                                value = resalutDateTime;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.DateTime:
                                        if (!(value is DateTime))
                                        {
                                            DateTime resalutDateTime;
                                            isok = DateTime.TryParse(value.ToString(), out resalutDateTime);
                                            if (isok)
                                            {
                                                value = resalutDateTime;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.DateTime2:
                                        if (!(value is DateTime))
                                        {
                                            DateTime resalutDateTime;
                                            isok = DateTime.TryParse(value.ToString(), out resalutDateTime);
                                            if (isok)
                                            {
                                                value = resalutDateTime;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.DateTimeOffset:
                                        if (!(value is DateTime))
                                        {
                                            DateTime resalutDateTime;
                                            isok = DateTime.TryParse(value.ToString(), out resalutDateTime);
                                            if (isok)
                                            {
                                                value = resalutDateTime;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Decimal:
                                        if (!(value is Decimal))
                                        {
                                            Decimal resalutDecimal;
                                            isok = Decimal.TryParse(value.ToString(), out resalutDecimal);
                                            if (isok)
                                            {
                                                value = resalutDecimal;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Double:
                                        if (!(value is Double))
                                        {
                                            Double resalutDouble2;
                                            isok = Double.TryParse(value.ToString(), out resalutDouble2);
                                            if (isok)
                                            {
                                                value = resalutDouble2;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Guid:
                                        if (!(value is Guid))
                                        {
                                            Guid resalutGuid;
                                            isok = Guid.TryParse(value.ToString(), out resalutGuid);
                                            if (isok)
                                            {
                                                value = resalutGuid;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Int16:
                                        if (!(value is Int16))
                                        {
                                            Int16 resalutInt16;
                                            isok = Int16.TryParse(value.ToString(), out resalutInt16);
                                            if (isok)
                                            {
                                                value = resalutInt16;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Int32:
                                        if (!(value is Int32))
                                        {
                                            Int32 resalutInt32;
                                            isok = Int32.TryParse(value.ToString(), out resalutInt32);
                                            if (isok)
                                            {
                                                value = resalutInt32;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Int64:
                                        if (!(value is Int64))
                                        {
                                            Int64 resalutInt64;
                                            isok = Int64.TryParse(value.ToString(), out resalutInt64);
                                            if (isok)
                                            {
                                                value = resalutInt64;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Object:

                                        //无需转化
                                        break;
                                    case global::System.Data.DbType.SByte:
                                        if (!(value is SByte))
                                        {
                                            SByte resalutSByte;
                                            isok = SByte.TryParse(value.ToString(), out resalutSByte);
                                            if (isok)
                                            {
                                                value = resalutSByte;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.Single:
                                        if (!(value is Single))
                                        {
                                            Single resalutSingle;
                                            isok = Single.TryParse(value.ToString(), out resalutSingle);
                                            if (isok)
                                            {
                                                value = resalutSingle;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.String:
                                        //if (dynProperty.ContainsAttribute("Encrypt"))
                                        //{
                                        //    value = Utilities.Encrypt(value.ToString());
                                        //}
                                        //else
                                        //{
                                        //    value = value.ToString();
                                        //}
                                        value = value.ToString();
                                        break;
                                    case global::System.Data.DbType.StringFixedLength:
                                        break;
                                    case global::System.Data.DbType.Time:
                                        if (!(value is DateTime))
                                        {
                                            DateTime resalutDateTime;
                                            isok = DateTime.TryParse(value.ToString(), out resalutDateTime);
                                            if (isok)
                                            {
                                                value = resalutDateTime;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.UInt16:
                                        if (!(value is UInt16))
                                        {
                                            UInt16 resalutUInt16;
                                            isok = UInt16.TryParse(value.ToString(), out resalutUInt16);
                                            if (isok)
                                            {
                                                value = resalutUInt16;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.UInt32:
                                        if (!(value is UInt32))
                                        {
                                            UInt32 resalutUInt32;
                                            isok = UInt32.TryParse(value.ToString(), out resalutUInt32);
                                            if (isok)
                                            {
                                                value = resalutUInt32;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.UInt64:
                                        if (!(value is UInt64))
                                        {
                                            UInt64 resalutUInt64;
                                            isok = UInt64.TryParse(value.ToString(), out resalutUInt64);
                                            if (isok)
                                            {
                                                value = resalutUInt64;
                                            }
                                        }
                                        break;
                                    case global::System.Data.DbType.VarNumeric:
                                        //不变
                                        break;
                                    case global::System.Data.DbType.Xml:
                                        //未知操作
                                        value = null;
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else
                            {
                                //value = dynProperty.GetDefaultValue();
                            }
                            break;
                        case CollectionType.Map:
                            //未知操作
                            value = null;
                            break;
                        case CollectionType.Set:
                            //未知操作
                            value = null;
                            break;
                        default:
                            break;
                    }

                    #endregion

                    //Struct类型特殊处理
                    if (isStruct)
                    {
                        DynObject[] attributesDynProperty = dynProperty.Attributes;

                        bool isSave = false;

                        foreach (DynObject attribute in attributesDynProperty)
                        {
                            if (attribute.DynClass.Name == "JsonSerialization")
                            {
                                List<string> removePropertyName = new List<string>();
                                if (isList)
                                {
                                    List<DynObject> dynObjectList = value as List<DynObject>;

                                    if (dynObjectList.Count > 0)
                                    {
                                        DynClass dynClass = dynObjectList[0].DynClass;

                                        removePropertyName = GetSerializationIgnorePropertyName(dynClass);

                                        foreach (var dynobject in dynObjectList)
                                        {
                                            foreach (var propertyName in removePropertyName)
                                            {
                                                dynobject[propertyName] = null;
                                            }
                                        }

                                    }

                                    value = DynObjectTransverter.DynObjectListToJson(dynObjectList);
                                    isSave = true;
                                    break;
                                }
                                else
                                {
                                    DynObject dynobject = value as DynObject;

                                    DynClass dynClass = dynobject.DynClass;

                                    removePropertyName = GetSerializationIgnorePropertyName(dynClass);


                                    foreach (var propertyName in removePropertyName)
                                    {
                                        dynobject[propertyName] = null;
                                    }

                                    value = DynObjectTransverter.DynObjectToJson(dynobject);
                                    isSave = true;
                                    break;
                                }
                            }
                            else if (attribute.DynClass.Name == "FkReverseQuery")
                            {
                                if (isNewEntity)
                                {
                                    value = null;
                                    List<string> pkNames = GetPrimaryKeyPropertyName(DynTypeManager.GetClass(dynProperty.StructName));
                                    dynEntity[dynProperty.Name + "_" + pkNames[0]] = dynObject[dynProperty.Name + "ID"];
                                    isNeedSaveValue = false;
                                }
                                else
                                {
                                    value = GetQueryPropertyValue(dynObject, dynProperty);
                                    isNeedSaveValue = true;
                                }

                                isSave = true;
                                break;
                            }
                        }

                        if (!isSave)
                        {
                            value = null;
                        }
                    }

                    if (isNeedSaveValue)
                    {
                        if (dynProperty.CollectionType != CollectionType.List || dynProperty.DynType != DynType.Struct)
                        {
                            if (dynEntity[entityPropertyName] != null && value != null && !isStruct)
                            {
                                if (dynEntity[entityPropertyName].ToString() != value.ToString())
                                {
                                    dynEntity[entityPropertyName] = value;
                                }
                            }
                            else
                            {
                                dynEntity[entityPropertyName] = value;
                            }
                        }
                    }
                }
            }

            return dynEntity;
        }

        private static List<string> GetSerializationIgnorePropertyName(DynClass dynClass)
        {
            List<string> ignore = new List<string>();

            foreach (var dynproperty in dynClass.GetProperties())
            {
                DynObject[] dynattributes = dynproperty.Attributes;

                foreach (DynObject dynattribute in dynattributes)
                {
                    if (dynattribute.DynClass.Name == "SerializationIgnore")
                    {
                        ignore.Add(dynproperty.Name);
                        break;
                    }
                }
            }

            return ignore;
        }

        private static List<string> GetPrimaryKeyPropertyName(DynClass dynClass)
        {
            List<string> pkproperties = new List<string>();

            foreach (DynProperty item in dynClass.GetProperties())
            {
                DynObject[] Attributes = item.Attributes;

                foreach (var Attribute in Attributes)
                {
                    if (Attribute.DynClass.Name == "PrimaryKey")
                    {
                        pkproperties.Add(item.Name);
                    }
                }
            }
            return pkproperties;
        }

        private static DynEntity GetQueryPropertyValue(DynObject dynObject, DynProperty dynProperty)
        {
            Check.Require(dynProperty.CollectionType == CollectionType.None && dynProperty.DynType == DynType.Struct, "给定的属性不符合查询属性的定义");
            string tableName = DynTypeManager.GetClass(dynProperty.StructName).Name;

            return GatewayFactory.Default.Find(dynProperty.StructName, dynObject[dynProperty.Name + "ID"]);
        }

        public static DynObject DynEntity2DynObject(DynEntity dynEntity)
        {
            if (dynEntity == null)
            {
                return null;
            }
            //获取dynEntity上的类名
            string entityTypeName = dynEntity.EntityType.Name;

            //如果没有 就说明此类被重名
            if (!DynTypeManager.ContainsClass(entityTypeName))
            {
                throw new Exception("查无此EntityType对应的DynClass");
            }

            DynObject dynObject = new DynObject(entityTypeName);
            DynProperty[] propertyList = dynObject.DynClass.GetProperties();

            //遍历dynObject属性赋值
            Object propertyValue = null;
            foreach (DynProperty dynProperty in propertyList)
            {
                string entityPropertyName = dynProperty.Name;
                string mappingPropertyName = null;               

                DynObject[] attributes = dynProperty.Attributes;

                //TODO:这里判断是否实体属性的方式可能需要调整
                bool isDynEntityProperty = true;

                //TODO:加密属性暂不考虑
                //bool isEncrypt = false;
                foreach (DynObject attribute in attributes)
                {
                    switch (attribute.DynClass.Name)
                    {
                        case "MappingName":
                            mappingPropertyName = attribute["Name"] as string;
                            break;
                        case "FkReverseQuery":
                            isDynEntityProperty = false;
                            break;
                        //case "FkQuery":
                        //    isDynEntityProperty = false;
                        //    break;
                        default:
                            break;
                    }                   
                }

                if (!isDynEntityProperty)
                {
                    continue;
                }

                if (dynEntity.ContainsProperty(entityPropertyName))
                {
                    propertyValue = dynEntity[entityPropertyName];
                }
                else if (dynEntity.ContainsProperty(mappingPropertyName))
                {
                    propertyValue = dynEntity[mappingPropertyName];
                }
                else
                {
                    propertyValue = null;
                }

                //if (isEncrypt)
                //{
                //    value = Utilities.Encrypt(value as string);
                //}

                if (propertyValue != null)
                {
                    propertyValue = ConvertDynType(propertyValue, dynProperty, dynObject);

                    dynObject.SetPropertyValueWithOutCheck(dynProperty, propertyValue);
                }
            }

            return dynObject;
        }

        private static Object ConvertDynType(object propertyValue, DynProperty dynProperty, DynObject dynObject)
        {
            switch (dynProperty.CollectionType)
            {
                case CollectionType.List:
                    if (dynProperty.DynType == DynType.Struct)
                    {
                        DynObject[] attributesDynProperty = dynProperty.Attributes;

                        foreach (DynObject attribute in attributesDynProperty)
                        {
                            if (attribute.DynClass.Name == "JsonSerialization")
                            {

                                return DynObjectTransverter.JsonToDynObjectList(propertyValue as string);
                            }
                        }

                        return null;
                    }
                    else
                    {
                        return DynObjectTransverter.JsonToSimpleList(propertyValue as string);
                    }
                case CollectionType.None:
                    if (dynProperty.DynType == DynType.Struct)
                    {
                        #region Struct类型特殊处理

                        if (dynProperty.ContainsAttribute("JsonSerialization"))
                        {
                            return DynObjectTransverter.JsonToDynObject(propertyValue as string);
                        }
                        else if (dynProperty.ContainsAttribute("FkReverseQuery"))
                        {
                            DynProperty[] dynPropertyKeys = DynTypeManager.GetClass(dynProperty.StructName).GetProperties();

                            if (dynPropertyKeys.Length == 1)
                            {
                                if (propertyValue is DynEntity)
                                {
                                    dynObject.SetPropertyValueWithOutCheck(dynProperty.Name + "ID", (propertyValue as DynEntity)[dynPropertyKeys[0].Name]);
                                }
                                else if (propertyValue != null)
                                {
                                    string idStr = propertyValue.ToString();
                                    int id = -1;
                                    if (int.TryParse(idStr, out id))
                                    {
                                        dynObject.SetPropertyValueWithOutCheck(dynProperty.Name + "ID", id);
                                    }
                                }
                            }                            
                        }
                        return null;

                        #endregion
                    }
                    else
                    {
                        Type destinationType = DynTypeMap.NoneType[dynProperty.DynType];
                        object returnValue = null;
                        if ((propertyValue == null) || destinationType.IsInstanceOfType(propertyValue))
                        {
                            return propertyValue;
                        }
                        string str = propertyValue as string;
                        if ((str != null) && (str.Length == 0))
                        {
                            return null;
                        }
                        TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
                        bool flag = converter.CanConvertFrom(propertyValue.GetType());
                        if (!flag)
                        {
                            converter = TypeDescriptor.GetConverter(propertyValue.GetType());
                        }
                        if (!flag && !converter.CanConvertTo(destinationType))
                        {
                            throw new InvalidOperationException("无法转换成类型：" + propertyValue.ToString() + "==>" + destinationType);
                        }
                        try
                        {
                            returnValue = flag ? converter.ConvertFrom(null, null, propertyValue) : converter.ConvertTo(null, null, propertyValue, destinationType);
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException("类型转换出错：" + propertyValue.ToString() + "==>" + destinationType, e);
                        }
                        return returnValue;
                    }
                case CollectionType.Map:
                case CollectionType.Set:
                default:
                    return null;
            }
        }       
    }

    public class DynTypeMap
    {
        public static Dictionary<DynType, Type> NoneType = new Dictionary<DynType, Type>();
        static DynTypeMap()
        {
            NoneType.Add(DynType.Struct, typeof(DynObject));
            NoneType.Add(DynType.Void, null);
            NoneType.Add(DynType.String, typeof(string));
            NoneType.Add(DynType.I64, typeof(Int64));
            NoneType.Add(DynType.I32, typeof(Int32));
            NoneType.Add(DynType.I16, typeof(Int16));
            NoneType.Add(DynType.Double, typeof(Double));
            NoneType.Add(DynType.Decimal, typeof(Decimal));
            NoneType.Add(DynType.DateTime, typeof(DateTime));
            NoneType.Add(DynType.Byte, typeof(Byte));
            NoneType.Add(DynType.Bool, typeof(Boolean));
            NoneType.Add(DynType.Binary, typeof(Byte[]));
        }
    }
}

