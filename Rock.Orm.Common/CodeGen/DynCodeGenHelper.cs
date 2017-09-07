using System;
using System.Collections.Generic;
using System.Text;
using Rock.Orm.Common.Design;

namespace Rock.Orm.Common
{
    public class DynCodeGenHelper
    {
        private string outNs;

        public DynCodeGenHelper(string outNs)
        {
            this.outNs = outNs;
        }

        #region Helper Methods

        //internal static AttributeType GetPropertyAttribute<AttributeType>(DynPropertyConfiguration dynEntityProperty)
        //{
        //    object[] attrs = dynEntityProperty.GetCustomAttributes(typeof(AttributeType), false);
        //    if (attrs != null && attrs.Length > 0)
        //    {
        //        return (AttributeType)attrs[0];
        //    }
        //    return default(AttributeType);
        //}

        internal static AttributeType GetEntityAttribute<AttributeType>(string typeName)
        {
            object[] attrs = DynEntityTypeManager.GetEntityTypeMandatory(typeName).GetCustomAttributes(typeof(AttributeType), false);
            if (attrs != null && attrs.Length > 0)
            {
                return (AttributeType)attrs[0];
            }
            return default(AttributeType);
        }

        internal static AttributeType[] GetEntityAttributes<AttributeType>(string typeName)
        {
            object[] attrs = DynEntityTypeManager.GetEntityTypeMandatory(typeName).GetCustomAttributes(typeof(AttributeType), false);
            if (attrs != null && attrs.Length > 0)
            {
                AttributeType[] objs = new AttributeType[attrs.Length];
                for (int i = 0; i < attrs.Length; i++)
                {
                    objs[i] = (AttributeType)attrs[i];
                }
                return objs;
            }
            return null;
        }

        //internal static QueryDynAttribute GetPropertyQueryAttribute(DynPropertyConfiguration dynEntityProperty)
        //{
        //    QueryDynAttribute qa = GetPropertyAttribute<PkQueryDynAttribute>(dynEntityProperty);
        //    if (qa == null) qa = GetPropertyAttribute<FkQueryDynAttribute>(dynEntityProperty);
        //    if (qa == null) qa = GetPropertyAttribute<CustomQueryDynAttribute>(dynEntityProperty);
        //    if (qa == null) qa = GetPropertyAttribute<PkReverseQueryDynAttribute>(dynEntityProperty);
        //    if (qa == null) qa = GetPropertyAttribute<FkReverseQueryDynAttribute>(dynEntityProperty);
        //    if (qa == null) qa = GetPropertyAttribute<ManyToManyQueryDynAttribute>(dynEntityProperty);

        //    return qa;
        //}

        private string GetOutputNamespace(string type)
        {
            OutputNamespaceDynAttribute outNsAttr = GetEntityAttribute<OutputNamespaceDynAttribute>(type);
            if (outNsAttr != null)
            {
                return outNsAttr.Namespace;
            }
            else
            {
                return outNs;
            }
        }

        #endregion

        #region Gen Entities

        public string GenEntities()
        {
            return GenEntities(DynEntityTypeManager.Entities);
        }

        public string GenEntities(List<DynEntityType> entityTypes)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("// IMPORTANT NOTICE: \r\n// You should never modify classes in this file manully. \r\n// To attach additional functions to entity classes, you should write partial classes in separate files.\r\n");

            sb.Append("\r\nusing System;\r\nusing System.Xml.Serialization;\r\nusing Rock.Orm.Common;\r\n\r\n");

            foreach (DynEntityType entityType in entityTypes)
            {
                sb.Append("namespace " + GetOutputNamespace(entityType.Name) + "\r\n{\r\n");
                GenEntity(sb, entityType);
                sb.Append("}\r\n\r\n");
            }

            return sb.ToString();
        }

        // 基于类型生成实体
        private void GenEntity(StringBuilder sb, DynEntityType entityType)
        {
            sb.Append("\t[Serializable]\r\n");
            sb.Append("\tpublic partial class ");
            sb.Append(entityType.Name);
            sb.Append("ArrayList : EntityArrayList<");
            sb.Append(entityType.Name);
            sb.Append("> { }\r\n\r\n");

            CommentDynAttribute ca = GetEntityAttribute<CommentDynAttribute>(entityType.Name);
            if (ca != null)
            {
                sb.Append("\t/// <summary>\r\n");
                sb.Append("\t/// ");
                sb.Append(ca.Content.Replace("\n", "\n\t/// "));
                sb.Append("\r\n\t/// </summary>\r\n");
            }

            sb.Append("\t[Serializable]\r\n");
            sb.Append("\tpublic partial class ");
            sb.Append(entityType.Name);
            sb.Append(" : ");

            DynEntityType[] interfaces = entityType.GetAllBaseEntityTypes();
            bool findNonEntityBaseEntity = false;
            foreach (DynEntityType item in interfaces)
            {
                if (item.IsAssignableFrom(entityType) && (entityType.Name != item.Name))
                {
                    sb.Append(item.Name);
                    findNonEntityBaseEntity = true;
                    break;
                }
            }
            if (!findNonEntityBaseEntity)
            {
                sb.Append("Entity");
            }

            //append custom implement interfaces
            EntityDynAttribute[] attrs = entityType.GetCustomAttributes(true);
            if (attrs != null)
            {
                foreach (EntityDynAttribute obj in attrs)
                {
                    if (obj.GetType() == typeof(ImplementInterfaceDynAttribute))
                    {
                        sb.Append(", global::");
                        sb.Append(((ImplementInterfaceDynAttribute)obj).InterfaceFullName);
                    }
                }
            }

            sb.Append("\r\n\t{\r\n");

            //generate GetEntityConfiguration()
            sb.Append("\t\tprotected static EntityConfiguration _");
            sb.Append(entityType.Name);
            sb.Append("EntityConfiguration;\r\n\r\n");
            sb.Append("\t\tpublic override EntityConfiguration GetEntityConfiguration()\r\n");
            sb.Append("\t\t{\r\n");
            sb.Append("\t\t\tif (_");
            sb.Append(entityType.Name);
            sb.Append("EntityConfiguration == null) _");
            sb.Append(entityType.Name);
            sb.Append("EntityConfiguration = MetaDataManager.GetEntityConfiguration(\"");
            sb.Append(GetOutputNamespace(entityType.Name) + "." + entityType.Name);
            sb.Append("\");\r\n");
            sb.Append("\t\t\treturn _");
            sb.Append(entityType.Name);
            sb.Append("EntityConfiguration;\r\n");
            sb.Append("\t\t}\r\n\r\n");

            //generate properties

            StringBuilder sbFields = new StringBuilder();
            StringBuilder sbProperties = new StringBuilder();
            StringBuilder sbReloadQueries = new StringBuilder();

            GenProperties(sbFields, sbProperties, sbReloadQueries, entityType);

            sb.AppendLine(sbFields.ToString());
            sb.Append(sbProperties.ToString());

            //generate Get & Set PropertyValues
            sb.Append("\t\t#region Get & Set PropertyValues\r\n\r\n");

            sb.Append("\t\tpublic override void ReloadQueries(bool includeLazyLoadQueries)\r\n\t\t{\r\n");
            if (findNonEntityBaseEntity)
            {
                sb.Append("\t\t\tbase.ReloadQueries(includeLazyLoadQueries);\r\n");
            }
            sb.Append(sbReloadQueries.ToString());
            sb.Append("\t\t}\r\n\r\n");

            List<string> generatedProperties = new List<string>();

            sb.Append("\t\tpublic override object[] GetPropertyValues()\r\n\t\t{\r\n");
            sb.Append("\t\t\treturn new object[] { ");
            StringBuilder sbPropertyValuesList = new StringBuilder();
            GenGetPropertyValues(sbPropertyValuesList, entityType, generatedProperties);
            sb.Append(sbPropertyValuesList.ToString().TrimEnd(' ', ','));
            sb.Append(" };\r\n\t\t}\r\n\r\n");

            sb.Append("\t\tpublic override void SetPropertyValues(System.Data.IDataReader reader)\r\n\t\t{\r\n");
            generatedProperties.Clear();
            GenSetPropertyValuesFromReader(sb, entityType, generatedProperties);
            sb.Append("\t\t\tReloadQueries(false);\r\n");
            sb.Append("\t\t}\r\n\r\n");

            sb.Append("\t\tpublic override void SetPropertyValues(System.Data.DataRow row)\r\n\t\t{\r\n");
            generatedProperties.Clear();
            GenSetPropertyValuesFromDataRow(sb, entityType, generatedProperties);
            sb.Append("\t\t\tReloadQueries(false);\r\n");
            sb.Append("\t\t}\r\n");

            sb.Append("\r\n\t\t#endregion\r\n\r\n");

            //generate Entity Equals
            sb.Append("\t\t#region Equals\r\n\r\n");

            string entityOutputTypeName = "global::" + GetOutputNamespace(entityType.Name) + "." + entityType.Name;

            sb.Append("\t\tpublic override int GetHashCode() { return base.GetHashCode(); }\r\n\r\n");
            sb.Append("\t\tpublic static bool operator ==(" + entityOutputTypeName + " left, " + entityOutputTypeName + " right) { return ((object)left) != null ? left.Equals(right) : ((object)right) == null ? true : false; }\r\n\r\n");
            sb.Append("\t\tpublic static bool operator !=(" + entityOutputTypeName + " left, " + entityOutputTypeName + " right) { return ((object)left) != null ? !left.Equals(right) : ((object)right) == null ? false : true; }\r\n\r\n");
            sb.Append("\t\tpublic override bool Equals(object obj)\r\n\t\t{\r\n\t\t\treturn obj == null || (!(obj is " + entityOutputTypeName + ")) ? false : ((object)this) == ((object)obj) ? true : this.isAttached && ((" + entityOutputTypeName + ")obj).isAttached");
            DynPropertyConfigurationCollection pis = entityType.GetProperties();
            bool hasPk = false;
            if (pis != null)
            {
                foreach (DynPropertyConfiguration pi in pis)
                {
                    if (pi.GetPropertyAttribute(typeof(PrimaryKeyDynAttribute)) != null || pi.GetPropertyAttribute(typeof(RelationKeyDynAttribute)) != null)
                    {
                        sb.Append(string.Format(" && this.{0} == (({1})obj).{0}", pi.Name, entityOutputTypeName));
                        hasPk = true;
                    }
                }
            }
            if (!hasPk)
            {
                sb.Append(" && base.Equals(obj)");
            }
            sb.Append(";\r\n\t\t}\r\n");

            sb.Append("\r\n\t\t#endregion\r\n\r\n");

            //generate Query Code
            sb.Append("\t\t#region QueryCode\r\n\r\n");
            sb.Append("\t\tpublic ");
            if (findNonEntityBaseEntity)
            {
                sb.Append("new ");
            }
            sb.Append("abstract class _\r\n\t\t{\r\n");
            sb.Append("\t\t\tprivate _() { }\r\n\r\n");
            generatedProperties.Clear();
            GenPropertyQueryCode(sb, entityType, generatedProperties);
            sb.Append("\t\t}\r\n");
            sb.Append("\r\n\t\t#endregion\r\n");

            sb.Append("\t}\r\n");
        }

        private void GenPropertyQueryCode(StringBuilder sb, DynEntityType entityType, List<string> generatedProperties)
        {
            foreach (DynEntityType item in entityType.GetAllBaseEntityTypes())
            {
                //if (typeof(DymORM.Common.Design.Entity).IsAssignableFrom(item) && typeof(DymORM.Common.Design.Entity) != item)
                if (entityType.Name != item.Name)
                {
                    GenPropertyQueryCode(sb, item, generatedProperties);
                }
            }

            foreach (DynPropertyConfiguration item in entityType.GetProperties())
            {
                QueryDynAttribute qa = item.GetPropertyQueryAttribute();
                if ((qa == null || (qa.QueryType == QueryType.FkReverseQuery)) && (!generatedProperties.Contains(item.Name)))
                {
                    sb.Append("\t\t\tpublic static PropertyItem ");
                    sb.Append(item.Name);
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.IsArray))
                    {
                        sb.Append("ID");
                    }
                    sb.Append(" = new PropertyItem(\"");
                    sb.Append(item.Name);
                    sb.Append("\");\r\n");

                    generatedProperties.Add(item.Name);
                }
            }
        }

        // 生成由DataRow设置一组属性值代码
        private void GenSetPropertyValuesFromDataRow(StringBuilder sb, DynEntityType type, List<string> generatedProperties)
        {
            foreach (DynEntityType item in type.GetAllBaseEntityTypes())
            {
                if (type.Name != item.Name)
                {
                    GenSetPropertyValuesFromDataRow(sb, item, generatedProperties);
                }
            }

            foreach (DynPropertyConfiguration item in type.GetProperties())
            {
                QueryDynAttribute qa = item.GetPropertyQueryAttribute();
                DynQueryDescriber describer = null;
                if ((qa == null || (qa.QueryType == QueryType.FkReverseQuery && (!item.IsArray))) && (!generatedProperties.Contains(item.Name)))
                {
                    sb.Append("\t\t\tif (!row.IsNull(");
                    sb.Append(generatedProperties.Count);
                    sb.Append(")) _");
                    sb.Append(item.Name);
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.IsArray))
                    {
                        describer = new DynQueryDescriber(qa, item, item.PropertyOriginalEntityType, type.Name);
                        sb.Append("_");
                        sb.Append(describer.RelatedForeignKey);
                    }
                    sb.Append(" = ");
                    sb.Append("(");
                    if (item.GetPropertyAttribute(typeof(CompoundUnitDynAttribute)) != null)
                    {
                        sb.Append("string");
                        sb.Append(")row");
                        sb.Append("[");
                        sb.Append(generatedProperties.Count);
                        sb.Append("];\r\n");
                    }
                    else
                    {
                        if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.IsArray))
                        {
                            sb.Append(describer.RelatedForeignKeyType.ToString());
                            if (describer.RelatedForeignKeyType == typeof(Guid).ToString())
                            {
                                sb.Append(")GetGuid(row, ");
                                sb.Append(generatedProperties.Count);
                                sb.Append(");\r\n");
                            }
                            else
                            {
                                sb.Append(")row");
                                sb.Append("[");
                                sb.Append(generatedProperties.Count);
                                sb.Append("];\r\n");
                            }
                        }
                        else
                        {
                            sb.Append(GenType(item.PropertyType.ToString()));
                            if (item.PropertyType == typeof(Guid).ToString())
                            {
                                sb.Append(")GetGuid(row, ");
                                sb.Append(generatedProperties.Count);
                                sb.Append(");\r\n");
                            }
                            else
                            {
                                sb.Append(")row");
                                sb.Append("[");
                                sb.Append(generatedProperties.Count);
                                sb.Append("];\r\n");
                            }
                        }
                    }

                    generatedProperties.Add(item.Name);
                }
            }
        }

        // 生成由Reader设置一组属性值代码
        private void GenSetPropertyValuesFromReader(StringBuilder sb, DynEntityType type, List<string> generatedProperties)
        {
            foreach (DynEntityType item in type.GetAllBaseEntityTypes())
            {
                //if (typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(item) && typeof(Rock.Orm.Common.Design.Entity) != item)
                if (type.Name != item.Name)
                {
                    GenSetPropertyValuesFromReader(sb, item, generatedProperties);
                }
            }

            foreach (DynPropertyConfiguration item in type.GetProperties())
            {
                QueryDynAttribute qa = item.GetPropertyQueryAttribute();
                DynQueryDescriber describer = null;
                if ((qa == null || (qa.QueryType == QueryType.FkReverseQuery && (!item.IsArray))) && (!generatedProperties.Contains(item.Name)))
                {
                    sb.Append("\t\t\tif (!reader.IsDBNull(");
                    sb.Append(generatedProperties.Count);
                    sb.Append(")) _");
                    sb.Append(item.Name);
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.IsArray))
                    {
                        describer = new DynQueryDescriber(qa, item, item.PropertyOriginalEntityType, type.Name);

                        sb.Append("_");
                        sb.Append(describer.RelatedForeignKey);
                    }
                    sb.Append(" = ");
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.IsArray))
                    {
                        GenReaderGet(sb, describer.RelatedForeignKeyEntityProperty);
                        sb.Append("(");
                        if (describer.RelatedForeignKeyEntityProperty.PropertyType == typeof(Guid).ToString())
                        {
                            sb.Append("reader, ");
                        }
                    }
                    else
                    {
                        GenReaderGet(sb, item);
                        sb.Append("(");
                        if (item.PropertyType == typeof(Guid).ToString())
                        {
                            sb.Append("reader, ");
                        }
                    }
                    sb.Append(generatedProperties.Count);
                    sb.Append(");\r\n");

                    generatedProperties.Add(item.Name);
                }
            }
        }

        // 生成获取一组属性值
        private void GenGetPropertyValues(StringBuilder sb, DynEntityType type, List<string> generatedProperties)
        {
            foreach (DynEntityType item in type.GetAllBaseEntityTypes())
            {
                //if (typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(item) && typeof(Rock.Orm.Common.Design.Entity) != item)
                if (type.Name != item.Name)
                {
                    GenGetPropertyValues(sb, item, generatedProperties);
                }
            }

            foreach (DynPropertyConfiguration item in type.GetProperties())
            {
                QueryDynAttribute qa = item.GetPropertyQueryAttribute();

                if ((qa == null || (qa.QueryType == QueryType.FkReverseQuery && (!item.IsArray))) && (!generatedProperties.Contains(item.Name)))
                {
                    sb.Append("_");
                    sb.Append(item.Name);
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.IsArray))
                    {
                        DynQueryDescriber describer = new DynQueryDescriber(qa, item, item.PropertyOriginalEntityType, type.Name);
                        sb.Append("_");
                        sb.Append(describer.RelatedForeignKey);
                    }
                    sb.Append(", ");

                    generatedProperties.Add(item.Name);
                }
            }
        }

        private void GenReaderGet(StringBuilder sb, DynPropertyConfiguration item)
        {
            if (item.PropertyType == typeof(bool).ToString())
            {
                sb.Append("reader.GetBoolean");
            }
            else if (item.PropertyType == typeof(byte).ToString())
            {
                sb.Append("reader.GetByte");
            }
            else if (item.PropertyType == typeof(char).ToString())
            {
                sb.Append("reader.GetChar");
            }
            else if (item.PropertyType == typeof(DateTime).ToString())
            {
                sb.Append("reader.GetDateTime");
            }
            else if (item.PropertyType == typeof(decimal).ToString())
            {
                sb.Append("reader.GetDecimal");
            }
            else if (item.PropertyType == typeof(double).ToString())
            {
                sb.Append("reader.GetDouble");
            }
            else if (item.PropertyType == typeof(float).ToString())
            {
                sb.Append("reader.GetFloat");
            }
            else if (item.PropertyType == typeof(Guid).ToString())
            {
                sb.Append("GetGuid");
            }
            else if (item.PropertyType == typeof(short).ToString())
            {
                sb.Append("reader.GetInt16");
            }
            else if (item.PropertyType == typeof(int).ToString())
            {
                sb.Append("reader.GetInt32");
            }
            else if (item.PropertyType == typeof(long).ToString())
            {
                sb.Append("reader.GetInt64");
            }
            else if (item.PropertyType == typeof(string).ToString())
            {
                sb.Append("reader.GetString");
            }
            else
            {
                if (item.GetPropertyAttribute(typeof(CompoundUnitDynAttribute)) != null)
                {
                    sb.Append("reader.GetString");
                }
                else
                {
                    sb.Append("(");
                    sb.Append(GenType(item.PropertyType.ToString()));
                    sb.Append(")reader.GetValue");
                }
            }
        }

        private void GenProperties(StringBuilder sbFields, StringBuilder sbProperties, StringBuilder sbReloadQueries, DynEntityType entityType)
        {
            List<DynPropertyConfiguration> list = new List<DynPropertyConfiguration>();
            foreach (DynPropertyConfiguration pi in entityType.GetProperties())
            {
                list.Add(pi);
            }
            //if (entityType.DeepGetProperties != null)
            //{
            //    foreach (EntityProperty pi in entityType.DeepGetProperties)
            //    {
            //        list.Add(pi);
            //    }
            //}
            foreach (DynPropertyConfiguration item in list)
            {
                CommentDynAttribute ca = item.GetPropertyAttribute(typeof(CommentDynAttribute)) as CommentDynAttribute;
                if (ca != null)
                {
                    sbProperties.Append("\t\t/// <summary>\r\n");
                    sbProperties.Append("\t\t/// ");
                    sbProperties.Append(ca.Content.Replace("\n", "\n\t\t/// "));
                    sbProperties.Append("\r\n\t\t/// </summary>\r\n");
                }

                if (item.GetPropertyAttribute(typeof(CompoundUnitDynAttribute)) != null)
                {
                    GenCompoundUnitProperty(sbFields, sbProperties, item);
                }
                else if (item.GetPropertyQueryAttribute() != null)
                {
                    GenQueryProperty(sbFields, sbProperties, sbReloadQueries, item, entityType);
                }
                else
                {
                    GenNormalProperty(sbFields, sbProperties, item);
                }
            }
        }

        private DynEntityType[] GetContractInterfaceTypes(DynEntityType type)
        {
            List<DynEntityType> list = new List<DynEntityType>();
            DynEntityType[] interfaceTypes = type.GetAllBaseEntityTypes();
            foreach (DynEntityType interfaceType in interfaceTypes)
            {
                if (!interfaceType.IsAssignableFrom(type))
                {
                    bool isInOtherInterfaces = false;
                    foreach (DynEntityType item in interfaceTypes)
                    {
                        if (item != interfaceType)
                        {
                            foreach (DynEntityType obj in item.GetAllBaseEntityTypes())
                            {
                                if (interfaceType == obj)
                                {
                                    isInOtherInterfaces = true;
                                    break;
                                }
                            }

                            if (isInOtherInterfaces)
                            {
                                break;
                            }
                        }
                    }

                    if (!isInOtherInterfaces)
                    {
                        list.Add(interfaceType);
                    }
                }
            }

            return list.ToArray();
        }

        private void GenQueryProperty(StringBuilder sbFields, StringBuilder sbProperties, StringBuilder sbReloadQuery, DynPropertyConfiguration item, DynEntityType type)
        {
            QueryDynAttribute qa = item.GetPropertyQueryAttribute();
            DynQueryDescriber describer = new DynQueryDescriber(qa, item, item.PropertyOriginalEntityType, type.Name);
            sbFields.Append("\t\tprotected ");
            string propertyType;
            if (item.IsArray)
            {
                propertyType = GetOutputNamespace(item.PropertyOriginalEntityType) + "." + RemoveTypePrefix(GenType(item.PropertyType)) + "ArrayList";
            }
            else
            {
                propertyType = GetOutputNamespace(item.PropertyOriginalEntityType) + "." + RemoveTypePrefix(GenType(item.PropertyType));
            }
            propertyType = "global::" + propertyType;
            sbFields.Append(propertyType);
            sbFields.Append(" _");
            sbFields.Append(item.Name);
            sbFields.Append(";\r\n");

            if (qa != null && qa.QueryType == QueryType.FkReverseQuery)
            {
                string fkType = describer.RelatedForeignKeyType;

                sbFields.Append("\t\tprotected ");
                sbFields.Append(GenType(fkType.ToString()));
                // zml??? if (fkType.IsValueType)
                {
                    sbFields.Append("?");
                }
                sbFields.Append(" _");
                sbFields.Append(item.Name);
                sbFields.Append("_");
                sbFields.Append(describer.RelatedForeignKey);
                sbFields.Append(";\r\n");
            }

            if (item.GetPropertyAttribute(typeof(SerializationIgnoreDynAttribute)) != null)
            {
                sbProperties.Append("\t\t[XmlIgnore]\r\n");
            }
            sbProperties.Append("\t\tpublic ");
            sbProperties.Append(propertyType);
            sbProperties.Append(" ");
            sbProperties.Append(item.Name);
            sbProperties.Append("\r\n\t\t{\r\n");
            sbProperties.Append("\t\t\tget\r\n\t\t\t{\r\n\t\t\t\tif (!IsQueryPropertyLoaded(\"");
            sbProperties.Append(item.Name);
            sbProperties.Append("\")) ");
            GenReloadQuery(sbProperties, qa, item, type);
            if (item.IsArray)
            {
                sbProperties.Append("\t\t\t\tif (_");
                sbProperties.Append(item.Name);
                sbProperties.Append(" == null) { ");
                sbProperties.Append(propertyType);
                sbProperties.Append(" _al = new ");
                sbProperties.Append(propertyType);
                sbProperties.Append("(); BindArrayListEventHandlers(\"");
                sbProperties.Append(item.Name);
                sbProperties.Append("\", _al); _");
                sbProperties.Append(item.Name);
                sbProperties.Append(" = _al; }\r\n");
            }
            sbProperties.Append("\t\t\t\treturn _");
            sbProperties.Append(item.Name);
            sbProperties.Append(";\r\n\t\t\t}\r\n");

            sbReloadQuery.Append("\t\t\t");
            sbReloadQuery.Append("if (includeLazyLoadQueries || (!MetaDataManager._isLazyLoad(\"");
            sbReloadQuery.Append(GetOutputNamespace(type.Name) + "." + type.Name);
            sbReloadQuery.Append("\", \"");
            sbReloadQuery.Append(item.Name);
            sbReloadQuery.Append("\"))) ");
            GenReloadQuery(sbReloadQuery, qa, item, type);

            if (!item.IsReadOnly)
            {
                if (item.IsArray)
                {
                    sbProperties.Append("\t\t\tset { OnQueryPropertyChanged(\"");
                }
                else
                {
                    sbProperties.Append("\t\t\tset { OnQueryOnePropertyChanged(\"");
                }
                sbProperties.Append(item.Name);
                sbProperties.Append("\", ");
                sbProperties.Append(item.Name);
                sbProperties.Append(", value); _");
                sbProperties.Append(item.Name);
                sbProperties.Append(" = value; ");

                if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.IsArray))
                {
                    string fkType = describer.RelatedForeignKeyType;

                    sbProperties.Append("if (value == null) { ");

                    sbProperties.Append("OnPropertyChanged(\"");
                    sbProperties.Append(item.Name);
                    sbProperties.Append("\", _");
                    sbProperties.Append(item.Name);
                    sbProperties.Append("_");
                    sbProperties.Append(describer.RelatedForeignKey);
                    sbProperties.Append(", null); _");

                    sbProperties.Append(item.Name);
                    sbProperties.Append("_");
                    sbProperties.Append(describer.RelatedForeignKey);
                    sbProperties.Append(" = null; } else {");

                    sbProperties.Append("OnPropertyChanged(\"");
                    sbProperties.Append(item.Name);
                    sbProperties.Append("\", _");
                    sbProperties.Append(item.Name);
                    sbProperties.Append("_");
                    sbProperties.Append(describer.RelatedForeignKey);
                    sbProperties.Append(", value.");
                    sbProperties.Append(describer.RelatedForeignKey);
                    sbProperties.Append("); _");

                    sbProperties.Append(item.Name);
                    sbProperties.Append("_");
                    sbProperties.Append(describer.RelatedForeignKey);
                    sbProperties.Append(" = value.");
                    sbProperties.Append(describer.RelatedForeignKey);
                    sbProperties.Append("; } ");
                }
                sbProperties.Append("}\r\n");
            }
            sbProperties.Append("\t\t}\r\n\r\n");
        }

        private void GenReloadQuery(StringBuilder sb, QueryDynAttribute qa, DynPropertyConfiguration item, DynEntityType type)
        {
            DynQueryDescriber describer = new DynQueryDescriber(qa, item, item.PropertyOriginalEntityType, type.Name);
            string propertyType;
            if (item.IsArray)
            {
                propertyType = GetOutputNamespace(item.PropertyOriginalEntityType) + "." + RemoveTypePrefix(GenType(item.PropertyType)) + "ArrayList";
            }
            else
            {
                propertyType = GetOutputNamespace(item.PropertyOriginalEntityType) + "." + RemoveTypePrefix(GenType(item.PropertyType));
            }
            propertyType = "global::" + propertyType;

            sb.Append("{ ");

            if (item.IsArray)
            {
                sb.Append(propertyType);
                sb.Append(" _al = new ");
                sb.Append(propertyType);
                sb.Append(" (); _al.AddRange((");
                sb.Append(propertyType.Substring(0, propertyType.Length - "ArrayList".Length));
                sb.Append("[])");
                sb.Append("Query(");
                sb.Append(string.Format("typeof({0}), \"{1}\", this)", propertyType.Substring(0, propertyType.Length - "ArrayList".Length), item.Name));
            }
            else
            {
                sb.Append("_");
                sb.Append(item.Name);
                sb.Append(" = (");
                sb.Append(propertyType);
                sb.Append(")");
                sb.Append("QueryOne(");
                sb.Append(string.Format("typeof({0}), \"{1}\", this)", propertyType, item.Name));
            }

            if (item.IsArray)
            {
                sb.Append("); ");
                sb.Append("OnQueryPropertyChanged(\"");
                sb.Append(item.Name);
                sb.Append("\", _");
                sb.Append(item.Name);
                sb.Append(", _al); _");
                sb.Append(item.Name);
                sb.Append(" = _al;");
            }
            else
            {
                sb.Append(";");
            }
            sb.Append(" }\r\n");
        }

        private string RemoveTypePrefix(string typeName)
        {
            string name = typeName;
            while (name.Contains("."))
            {
                name = name.Substring(name.IndexOf(".")).TrimStart('.');
            }
            return name;
        }

        private void GenNormalProperty(StringBuilder sbFields, StringBuilder sbProperties, DynPropertyConfiguration item)
        {
            sbFields.Append("\t\tprotected ");
            sbFields.Append(GenType(item.PropertyType.ToString()));
            sbFields.Append(" _");
            sbFields.Append(item.Name);
            sbFields.Append(";\r\n");

            if (item.GetPropertyAttribute(typeof(SerializationIgnoreDynAttribute)) != null)
            {
                sbProperties.Append("\t\t[XmlIgnore]\r\n");
            }
            sbProperties.Append("\t\tpublic ");
            sbProperties.Append(GenType(item.PropertyType.ToString()));
            sbProperties.Append(" ");
            sbProperties.Append(item.Name);
            sbProperties.Append("\r\n\t\t{\r\n");
            if (item.IsReadOnly)
            {
                sbProperties.Append("\t\t\tget { return _");
                sbProperties.Append(item.Name);
                sbProperties.Append("; }\r\n");
            }
            //if (item.CanWrite)
            //{
            sbProperties.Append("\t\t\tset { OnPropertyChanged(\"");
            sbProperties.Append(item.Name);
            sbProperties.Append("\", _");
            sbProperties.Append(item.Name);
            sbProperties.Append(", value); _");
            sbProperties.Append(item.Name);
            sbProperties.Append(" = value; }\r\n");
            //}
            sbProperties.Append("\t\t}\r\n\r\n");
        }

        private void GenCompoundUnitProperty(StringBuilder sbFields, StringBuilder sbProperties, DynPropertyConfiguration item)
        {
            sbFields.Append("\t\tprotected string _");
            sbFields.Append(item.Name);
            sbFields.Append(";\r\n");

            if (item.GetPropertyAttribute(typeof(SerializationIgnoreDynAttribute)) != null)
            {
                sbProperties.Append("\t\t[XmlIgnore]\r\n");
            }
            sbProperties.Append("\t\tpublic ");
            sbProperties.Append(GenType(item.PropertyType.ToString()));
            sbProperties.Append(" ");
            sbProperties.Append(item.Name);
            sbProperties.Append("\r\n\t\t{\r\n");
            if (item.IsReadOnly)
            {
                sbProperties.Append("\t\t\tget { return (");
                sbProperties.Append(GenType(item.PropertyType.ToString()).TrimEnd('?'));
                sbProperties.Append(")SerializationManager.Deserialize(typeof(");
                sbProperties.Append(GenType(item.PropertyType.ToString()).TrimEnd('?'));
                sbProperties.Append("), _");
                sbProperties.Append(item.Name);
                sbProperties.Append("); }\r\n");
            }
            //if (item.CanWrite)
            //{
            sbProperties.Append("\t\t\tset { OnPropertyChanged(\"");
            sbProperties.Append(item.Name);
            sbProperties.Append("\", _");
            sbProperties.Append(item.Name);
            sbProperties.Append(", value); _");
            sbProperties.Append(item.Name);
            sbProperties.Append(" = SerializationManager.Serialize(value); }\r\n");
            //}
            sbProperties.Append("\t\t}\r\n");
        }

        private string GenType(string typeStr)
        {
            if (typeStr.StartsWith("System.Nullable`1["))
            {
                return GenType(typeStr.Substring("System.Nullable`1[".Length).Trim('[', ']')) + "?";
            }

            if (typeStr == typeof(string).ToString())
            {
                return "string";
            }
            else if (typeStr == typeof(int).ToString())
            {
                return "int";
            }
            else if (typeStr == typeof(long).ToString())
            {
                return "long";
            }
            else if (typeStr == typeof(short).ToString())
            {
                return "short";
            }
            else if (typeStr == typeof(byte).ToString())
            {
                return "byte";
            }
            else if (typeStr == typeof(byte[]).ToString())
            {
                return "byte[]";
            }
            else if (typeStr == typeof(bool).ToString())
            {
                return "bool";
            }
            else if (typeStr == typeof(decimal).ToString())
            {
                return "decimal";
            }
            else if (typeStr == typeof(char).ToString())
            {
                return "char";
            }
            else if (typeStr == typeof(sbyte).ToString())
            {
                return "sbyte";
            }
            else if (typeStr == typeof(float).ToString())
            {
                return "float";
            }
            else if (typeStr == typeof(double).ToString())
            {
                return "double";
            }
            else if (typeStr == typeof(object).ToString())
            {
                return "object";
            }
            else if (typeStr == typeof(Guid).ToString())
            {
                return "Guid";
            }
            else if (typeStr == typeof(DateTime).ToString())
            {
                return "DateTime";
            }
            else
            {
                return typeStr;
            }
        }

        #endregion

        #region Gen Entity Configs

        public string GenEntityConfigurations()
        {
            List<EntityConfiguration> configs = DoGenEntityConfigurations();
            string retStr = SerializationManager.Serialize(configs.ToArray());
            retStr = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + retStr.TrimStart().Substring("<?xml version=\"1.0\" encoding=\"utf-16\"?>".Length);

            return retStr;
        }

        private List<EntityConfiguration> DoGenEntityConfigurations()
        {
            List<EntityConfiguration> configs = new List<EntityConfiguration>();

            foreach (DynEntityType entityType in DynEntityTypeManager.Entities)
            {
                string typeName = entityType.Name;
                EntityConfiguration ec = new EntityConfiguration();
                ec.Name = GetOutputNamespace(typeName) + "." + typeName;
                if (GetEntityAttribute<MappingNameDynAttribute>(typeName) != null)
                {
                    ec.MappingName = GetEntityAttribute<MappingNameDynAttribute>(typeName).Name;
                }
                if (GetEntityAttribute<CommentDynAttribute>(typeName) != null)
                {
                    ec.Comment = GetEntityAttribute<CommentDynAttribute>(typeName).Content;
                }
                if (GetEntityAttribute<CustomDataDynAttribute>(typeName) != null)
                {
                    ec.CustomData = GetEntityAttribute<CustomDataDynAttribute>(typeName).Data;
                }
                if (GetEntityAttribute<ReadOnlyDynAttribute>(typeName) != null)
                {
                    ec.IsReadOnly = true;
                }
                if (GetEntityAttribute<AutoPreLoadDynAttribute>(typeName) != null)
                {
                    ec.IsAutoPreLoad = true;
                }
                if (GetEntityAttribute<RelationDynAttribute>(typeName) != null)
                {
                    ec.IsRelation = true;
                }
                BatchUpdateDynAttribute bsla = GetEntityAttribute<BatchUpdateDynAttribute>(typeName);
                if (bsla != null)
                {
                    ec.IsBatchUpdate = true;
                    ec.BatchSize = bsla.BatchSize;
                }
                AdditionalSqlScriptDynAttribute[] addSqls = GetEntityAttributes<AdditionalSqlScriptDynAttribute>(typeName);
                if (addSqls != null && addSqls.Length > 0)
                {
                    ec.AdditionalSqlScript = string.Empty;

                    foreach (AdditionalSqlScriptDynAttribute addSql in addSqls)
                    {
                        if (!string.IsNullOrEmpty(addSql.PreCleanSql))
                        {
                            ec.AdditionalSqlScript += addSql.PreCleanSql + "\n";
                        }
                        ec.AdditionalSqlScript += addSql.Sql + "\n\n";
                    }
                }

                DynEntityType[] baseEntityTypes = entityType.GetAllBaseEntityTypes();
                foreach (DynEntityType item in baseEntityTypes)
                {
                    if (entityType.Name != item.Name)
                    {
                        ec.BaseEntity = GetOutputNamespace(item.Name) + "." + item.Name;
                        break;
                    }
                }

                List<string> generatedProperties = new List<string>();
                GenPropertyConfig(ec, entityType, typeName, generatedProperties, false);

                configs.Add(ec);
            }

            return configs;
        }

        private void GenPropertyConfig(EntityConfiguration ec, DynEntityType entityType, string entityTypeName, List<string> generatedProperties, bool isInherited)
        {
            foreach (DynEntityType item in entityType.GetAllBaseEntityTypes())
            {
                GenPropertyConfig(ec, item, entityTypeName, generatedProperties, item.IsAssignableFrom(entityType));
            }

            List<DynPropertyConfiguration> list = new List<DynPropertyConfiguration>();
            if (entityType.GetProperties() != null)
            {
                foreach (DynPropertyConfiguration pi in entityType.GetProperties())
                {
                    ///出事概不负责
                    ///
                    if (entityType.Name != entityTypeName)
                    {
                        pi.IsInherited = true;
                        pi.InheritEntityMappingName = entityType.Name;
                    }
                    ///
                    list.Add(pi);
                }
            }

            foreach (DynPropertyConfiguration item in list)
            {
                if (!generatedProperties.Contains(item.Name))
                {
                    PropertyConfiguration pc = new PropertyConfiguration();
                    //pc.IsCompoundUnit = (GetPropertyAttribute<CompoundUnitDynAttribute>(item) != null);
                    pc.IsPrimaryKey = (item.GetPropertyAttribute(typeof(PrimaryKeyDynAttribute)) != null);
                    IndexPropertyDynAttribute ipa = item.GetPropertyAttribute(typeof(IndexPropertyDynAttribute)) as IndexPropertyDynAttribute;
                    if (ipa != null)
                    {
                        pc.IsIndexProperty = true;
                        pc.IsIndexPropertyDesc = ipa.IsDesc;
                    }
                    pc.IsNotNull = (item.GetPropertyAttribute(typeof(NotNullDynAttribute)) != null);
                    pc.IsSerializationIgnore = (item.GetPropertyAttribute(typeof(SerializationIgnoreDynAttribute)) != null);
                    RelationKeyDynAttribute rka = item.GetPropertyAttribute(typeof(RelationKeyDynAttribute)) as RelationKeyDynAttribute;
                    if (rka != null)
                    {
                        pc.IsRelationKey = true;
                        pc.RelatedType = GetOutputNamespace(rka.RelatedType) + "." + rka.RelatedType;
                        pc.RelatedForeignKey = rka.RelatedPk;
                    }
                    FriendKeyDynAttribute fka = item.GetPropertyAttribute(typeof(FriendKeyDynAttribute)) as FriendKeyDynAttribute;
                    if (fka != null)
                    {
                        pc.IsFriendKey = true;
                        pc.RelatedForeignKey = fka.RelatedEntityPk;
                        pc.RelatedType = GetOutputNamespace(fka.RelatedEntityType) + "." + fka.RelatedEntityType;
                    }
                    MappingNameDynAttribute mna = item.GetPropertyAttribute(typeof(MappingNameDynAttribute)) as MappingNameDynAttribute;
                    if (mna != null)
                    {
                        pc.MappingName = mna.Name;
                    }
                    CommentDynAttribute ca = item.GetPropertyAttribute(typeof(CommentDynAttribute)) as CommentDynAttribute;
                    if (ca != null)
                    {
                        pc.Comment = ca.Content;
                    }
                    CustomDataDynAttribute cd = item.GetPropertyAttribute(typeof(CustomDataDynAttribute)) as CustomDataDynAttribute;
                    if (cd != null)
                    {
                        pc.CustomData = ((CustomDataDynAttribute)cd).Data;
                    }
                    pc.IsReadOnly = item.IsReadOnly;
                    pc.Name = item.Name;
                    SqlTypeDynAttribute sta = item.GetPropertyAttribute(typeof(SqlTypeDynAttribute)) as SqlTypeDynAttribute;
                    if (sta != null && (!string.IsNullOrEmpty(sta.Type)))
                    {
                        pc.SqlType = sta.Type;
                        pc.SqlDefaultValue = sta.DefaultValue;
                    }

                    QueryDynAttribute qa = item.GetPropertyQueryAttribute();

                    if (qa != null)
                    {
                        pc.IsQueryProperty = true;
                        pc.QueryType = qa.QueryType.ToString();

                        string propertyEntityType = item.PropertyType;
                        if (item.IsArray)
                        {
                            propertyEntityType = item.PropertyType;
                        }
                        DynQueryDescriber describer = new DynQueryDescriber(qa, item, propertyEntityType, entityTypeName);

                        pc.IsLazyLoad = describer.LazyLoad;
                        pc.QueryOrderBy = describer.OrderBy;
                        pc.QueryWhere = describer.Where;
                        if (describer.RelationType != null)
                        {
                            pc.RelationType = GetOutputNamespace(describer.RelationType) + "." + describer.RelationType;
                        }
                        pc.IsContained = describer.Contained;
                        pc.RelatedForeignKey = describer.RelatedForeignKey;
                        if (describer.RelatedType != null)
                        {
                            pc.RelatedType = GetOutputNamespace(describer.RelatedType) + "." + describer.RelatedType;
                        }

                        if (item.IsArray)
                        {
                            pc.PropertyType = GetOutputNamespace(item.PropertyType) + "." + item.PropertyType + "ArrayList";
                        }
                        else
                        {
                            pc.PropertyType = GetOutputNamespace(item.PropertyType) + "." + item.PropertyType;
                        }

                        if (qa.QueryType == QueryType.FkReverseQuery)
                        {
                            if (string.IsNullOrEmpty(describer.RelatedForeignKeyType) != true)
                            {
                                pc.PropertyMappingColumnType = describer.RelatedForeignKeyType ?? typeof(string).ToString();
                            }
                            else
                            {
                                pc.PropertyMappingColumnType = string.IsNullOrEmpty(describer.RelatedForeignKeyType) ? "" : typeof(string).ToString();
                            }
                        }

                        if (qa.QueryType == QueryType.FkQuery)
                        {
                            pc.RelatedForeignKey = describer.RelatedForeignKey;
                            pc.RelatedType = GetOutputNamespace(describer.RelatedType) + "." + describer.RelatedType;
                        }
                    }
                    else
                    {
                        pc.PropertyType = item.PropertyType.ToString();
                    }

                    pc.IsInherited = item.IsInherited;

                    ec.Add(pc);



                    generatedProperties.Add(item.Name);

                }
            }
        }

        #endregion

        #region Gen Db Script

        public string GenAddTableColumnScript(string entityName, string propertyName)
        {
            StringBuilder sbPK = new StringBuilder();
            StringBuilder sbIndex = new StringBuilder();
            StringBuilder sbTable = new StringBuilder();
            List<EntityConfiguration> configs = DoGenEntityConfigurations();

            string retSql = "";

            foreach (EntityConfiguration ec in configs)
            {
                if (ec.IsReadOnly)
                    continue;

                if (ec.Name == outNs + "." + entityName)
                {
                    foreach (PropertyConfiguration pc in ec.Properties)
                    {
                        if (pc.Name == propertyName)
                        {
                            if (!pc.IsInherited)
                            {
                                GenerateColumn(ec, sbPK, sbIndex, sbTable, pc);
                                retSql += "ALTER TABLE [" + ec.MappingName + "] ADD " + sbTable.ToString().Replace(",\r\n", "\r\n");
                                retSql += "GO\r\n";

                                if (sbIndex.Length > 0)
                                {
                                    retSql += "\r\n";
                                    retSql += sbIndex.ToString();
                                }

                                if (sbPK.Length > 0)
                                {
                                    retSql += "\r\n";
                                    retSql += string.Format("ALTER TABLE [dbo].[{0}] WITH NOCHECK ADD\r\nCONSTRAINT [{1}] PRIMARY KEY CLUSTERED\r\n(\r\n", ec.MappingName, "PK_" + (ec.MappingName).Replace(" ", "_")) + sbPK.ToString();
                                    retSql = retSql.Substring(0, retSql.Length - 3);
                                    retSql += "\r\n)\r\nON [PRIMARY]\r\nGO\r\n\r\n";
                                }

                                return retSql;
                            }
                            else
                            {
                                throw new ApplicationException("Inherited property can't be add to entity!");
                            }
                        }
                    }

                    throw new ApplicationException("There is no this property in this entity!");
                }
            }

            throw new ApplicationException("There is no this entity!");
        }

        public string GenDropTableColumnScript(string entityName, string propertyName)
        {
            StringBuilder sbPK = new StringBuilder();
            StringBuilder sbIndex = new StringBuilder();
            StringBuilder sbTable = new StringBuilder();
            List<EntityConfiguration> configs = DoGenEntityConfigurations();

            string retSql = "";

            foreach (EntityConfiguration ec in configs)
            {
                if (ec.IsReadOnly)
                    continue;

                if (ec.Name == outNs + "." + entityName)
                {
                    foreach (PropertyConfiguration pc in ec.Properties)
                    {
                        if (pc.Name == propertyName)
                        {
                            //if (!pc.IsInherited)
                            {
                                retSql = "ALTER TABLE [" + ec.MappingName + "] DROP COLUMN [" + pc.Name + "]";

                                return retSql;
                            }
                            //else
                            //{
                            //    throw new ApplicationException("Inherited property can't be add to entity!");
                            //}
                        }
                    }

                    throw new ApplicationException("There is no this property in this entity!");
                }
            }

            throw new ApplicationException("There is no this entity!");
        }

        public string GenCreateTableScript(string entityName)
        {
            StringBuilder sb = new StringBuilder();
            List<string> tables = new List<string>();
            List<KeyValuePair<string, string>> fkRelations = new List<KeyValuePair<string, string>>();
            List<PropertyConfiguration> fkRelationKeys = new List<PropertyConfiguration>();
            List<EntityConfiguration> configs = DoGenEntityConfigurations();

            foreach (EntityConfiguration ec in configs)
            {
                if (ec.IsReadOnly || ec.Name != outNs + "." + entityName)
                    continue;

                //for creating primary DEFAULT_KEY
                StringBuilder sbPK = new StringBuilder();
                //for creating index
                StringBuilder sbIndex = new StringBuilder();
                //for creating table
                StringBuilder sbTable = new StringBuilder();
                //for creating fkRelation
                StringBuilder sbFkRelation = new StringBuilder();

                tables.Add(ec.MappingName);

                //create table
                sbTable.Append(string.Format("CREATE TABLE [dbo].[{0}] (\r\n", ec.MappingName));

                if (ec.BaseEntity != null)
                {
                    List<PropertyConfiguration> pkConfigs = new List<PropertyConfiguration>();
                    foreach (PropertyConfiguration pc in ec.Properties)
                    {
                        if (pc.IsPrimaryKey)
                        {
                            pkConfigs.Add(pc);
                        }
                    }

                    if (pkConfigs.Count == 1 && (!pkConfigs[0].IsFriendKey))
                    {
                        fkRelations.Add(new KeyValuePair<string, string>(ec.MappingName, GetEntityConfigurationInConfigs(configs, ec.BaseEntity).MappingName));
                        fkRelationKeys.Add(pkConfigs[0]);
                    }
                }

                if (ec.IsRelation)
                {
                    Dictionary<string, int> relatedEntities = new Dictionary<string, int>();
                    Dictionary<string, PropertyConfiguration> relatedEntityKeys = new Dictionary<string, PropertyConfiguration>();
                    foreach (PropertyConfiguration pc in ec.Properties)
                    {
                        if (pc.IsRelationKey)
                        {
                            if (!relatedEntities.ContainsKey(pc.RelatedType))
                            {
                                relatedEntities.Add(pc.RelatedType, 1);
                                relatedEntityKeys.Add(pc.RelatedType, pc);
                            }
                            else
                            {
                                relatedEntities[pc.RelatedType]++;
                            }
                        }
                    }

                    foreach (string key in relatedEntities.Keys)
                    {
                        if (relatedEntities[key] == 1)
                        {
                            fkRelations.Add(new KeyValuePair<string, string>(ec.MappingName, GetEntityConfigurationInConfigs(configs, key).MappingName));
                            fkRelationKeys.Add(relatedEntityKeys[key]);
                        }
                    }
                }

                //create primary key
                sbPK.Append(string.Format("ALTER TABLE [dbo].[{0}] WITH NOCHECK ADD\r\nCONSTRAINT [{1}] PRIMARY KEY CLUSTERED\r\n(\r\n", ec.MappingName, "PK_" + (ec.MappingName).Replace(" ", "_")));

                Dictionary<string, List<string>> inherittedColumns = new Dictionary<string, List<string>>();

                List<string> pks = new List<string>();
                foreach (PropertyConfiguration pc in ec.Properties)
                {
                    if (((!pc.IsInherited) || pc.IsPrimaryKey) && ((!pc.IsQueryProperty) || (pc.QueryType == "FkReverseQuery")))
                    {
                        if (pc.IsFriendKey)
                        {
                            fkRelations.Add(new KeyValuePair<string, string>(ec.MappingName, GetEntityConfigurationInConfigs(configs, pc.RelatedType).MappingName));
                            fkRelationKeys.Add(pc);
                        }

                        GenerateColumn(ec, sbPK, sbIndex, sbTable, pc);
                    }
                    if (pc.IsPrimaryKey)
                    {
                        pks.Add(pc.MappingName);
                    }
                }

                sbTable.Append(") ON [PRIMARY]\r\nGO\r\n\r\n");
                sbPK.Append(") ON [PRIMARY]\r\nGO\r\n\r\n");

                sb.Append(sbTable.ToString().Replace(",\r\n)", "\r\n)"));

                if (!sbPK.ToString().Contains("(\r\n)"))
                {
                    sb.Append(sbPK.ToString().Replace(",\r\n)", "\r\n)"));
                    sb.Append(sbIndex);
                }

                break;
            }

            StringBuilder finalSb = new StringBuilder();
            for (int i = 0; i < fkRelations.Count; i++)
            {
                KeyValuePair<string, string> fkRelation = fkRelations[i];
                PropertyConfiguration pc = fkRelationKeys[i];

                //delete existing fkRelation
                finalSb.Append(string.Format("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[FK_{0}_{2}_{1}]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)\r\nALTER TABLE [dbo].[{0}] DROP CONSTRAINT FK_{0}_{2}_{1}\r\nGO\r\n\r\n", fkRelation.Key, fkRelation.Value, pc.MappingName));
            }

            foreach (string table in tables)
            {
                //delete existing table
                finalSb.Append(string.Format("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\ndrop table [dbo].[{0}]\r\nGO\r\n\r\n", table));
            }

            finalSb.Append(sb.ToString());

            for (int i = 0; i < fkRelations.Count; i++)
            {
                //create fk relations
                KeyValuePair<string, string> fkRelation = fkRelations[i];
                PropertyConfiguration pc = fkRelationKeys[i];

                string relatedKeyMappingName = null;
                if (pc.RelatedForeignKey != null)
                {
                    EntityConfiguration ec = GetEntityConfigurationInConfigs(configs, pc.RelatedType);
                    relatedKeyMappingName = ec.GetPropertyConfiguration(pc.RelatedForeignKey).MappingName;
                }
                finalSb.Append(string.Format("ALTER TABLE [dbo].[{0}] ADD CONSTRAINT [FK_{0}_{2}_{1}] FOREIGN KEY ( [{2}] ) REFERENCES [dbo].[{1}] ( [{3}] ) NOT FOR REPLICATION\r\nGO\r\n\r\n", fkRelation.Key, fkRelation.Value, pc.MappingName, relatedKeyMappingName ?? pc.MappingName));
            }

            foreach (EntityConfiguration ec in configs)
            {
                if (!string.IsNullOrEmpty(ec.AdditionalSqlScript))
                {
                    finalSb.Append(ec.AdditionalSqlScript);
                }
            }

            return finalSb.ToString();
        }

        public string GenDropTableScript(string entityName)
        {
            StringBuilder sb = new StringBuilder();
            List<string> tables = new List<string>();
            List<KeyValuePair<string, string>> fkRelations = new List<KeyValuePair<string, string>>();
            List<PropertyConfiguration> fkRelationKeys = new List<PropertyConfiguration>();
            List<EntityConfiguration> configs = DoGenEntityConfigurations();

            foreach (EntityConfiguration ec in configs)
            {
                if (ec.IsReadOnly || ec.Name != outNs + "." + entityName)
                    continue;

                //for creating primary DEFAULT_KEY
                StringBuilder sbPK = new StringBuilder();
                //for creating index
                StringBuilder sbIndex = new StringBuilder();
                //for creating table
                StringBuilder sbTable = new StringBuilder();
                //for creating fkRelation
                StringBuilder sbFkRelation = new StringBuilder();

                tables.Add(ec.MappingName);

                if (ec.BaseEntity != null)
                {
                    List<PropertyConfiguration> pkConfigs = new List<PropertyConfiguration>();
                    foreach (PropertyConfiguration pc in ec.Properties)
                    {
                        if (pc.IsPrimaryKey)
                        {
                            pkConfigs.Add(pc);
                        }
                    }

                    if (pkConfigs.Count == 1 && (!pkConfigs[0].IsFriendKey))
                    {
                        fkRelations.Add(new KeyValuePair<string, string>(ec.MappingName, GetEntityConfigurationInConfigs(configs, ec.BaseEntity).MappingName));
                        fkRelationKeys.Add(pkConfigs[0]);
                    }
                }

                if (ec.IsRelation)
                {
                    Dictionary<string, int> relatedEntities = new Dictionary<string, int>();
                    Dictionary<string, PropertyConfiguration> relatedEntityKeys = new Dictionary<string, PropertyConfiguration>();
                    foreach (PropertyConfiguration pc in ec.Properties)
                    {
                        if (pc.IsRelationKey)
                        {
                            if (!relatedEntities.ContainsKey(pc.RelatedType))
                            {
                                relatedEntities.Add(pc.RelatedType, 1);
                                relatedEntityKeys.Add(pc.RelatedType, pc);
                            }
                            else
                            {
                                relatedEntities[pc.RelatedType]++;
                            }
                        }
                    }

                    foreach (string key in relatedEntities.Keys)
                    {
                        if (relatedEntities[key] == 1)
                        {
                            fkRelations.Add(new KeyValuePair<string, string>(ec.MappingName, GetEntityConfigurationInConfigs(configs, key).MappingName));
                            fkRelationKeys.Add(relatedEntityKeys[key]);
                        }
                    }
                }

                Dictionary<string, List<string>> inherittedColumns = new Dictionary<string, List<string>>();

                List<string> pks = new List<string>();
                foreach (PropertyConfiguration pc in ec.Properties)
                {
                    if (((!pc.IsInherited) || pc.IsPrimaryKey) && ((!pc.IsQueryProperty) || (pc.QueryType == "FkReverseQuery")))
                    {
                        if (pc.IsFriendKey)
                        {
                            fkRelations.Add(new KeyValuePair<string, string>(ec.MappingName, GetEntityConfigurationInConfigs(configs, pc.RelatedType).MappingName));
                            fkRelationKeys.Add(pc);
                        }
                    }
                    if (pc.IsPrimaryKey)
                    {
                        pks.Add(pc.MappingName);
                    }
                }


                sb.Append(sbTable.ToString().Replace(",\r\n)", "\r\n)"));

                if (!sbPK.ToString().Contains("(\r\n)"))
                {
                    sb.Append(sbPK.ToString().Replace(",\r\n)", "\r\n)"));
                    sb.Append(sbIndex);
                }

                break;
            }

            StringBuilder finalSb = new StringBuilder();
            for (int i = 0; i < fkRelations.Count; i++)
            {
                KeyValuePair<string, string> fkRelation = fkRelations[i];
                PropertyConfiguration pc = fkRelationKeys[i];

                //delete existing fkRelation
                finalSb.Append(string.Format("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[FK_{0}_{2}_{1}]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)\r\nALTER TABLE [dbo].[{0}] DROP CONSTRAINT FK_{0}_{2}_{1}\r\nGO\r\n\r\n", fkRelation.Key, fkRelation.Value, pc.MappingName));
            }

            foreach (string table in tables)
            {
                //delete existing table
                finalSb.Append(string.Format("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\ndrop table [dbo].[{0}]\r\nGO\r\n\r\n", table));
            }

            finalSb.Append(sb.ToString());

            for (int i = 0; i < fkRelations.Count; i++)
            {
                //create fk relations
                KeyValuePair<string, string> fkRelation = fkRelations[i];
                PropertyConfiguration pc = fkRelationKeys[i];

                string relatedKeyMappingName = null;
                if (pc.RelatedForeignKey != null)
                {
                    EntityConfiguration ec = GetEntityConfigurationInConfigs(configs, pc.RelatedType);
                    relatedKeyMappingName = ec.GetPropertyConfiguration(pc.RelatedForeignKey).MappingName;
                }
            }

            foreach (EntityConfiguration ec in configs)
            {
                if (!string.IsNullOrEmpty(ec.AdditionalSqlScript))
                {
                    finalSb.Append(ec.AdditionalSqlScript);
                }
            }

            return finalSb.ToString();
        }

        public string GenDbScript()
        {
            StringBuilder sb = new StringBuilder();
            List<string> tables = new List<string>();
            List<KeyValuePair<string, string>> fkRelations = new List<KeyValuePair<string, string>>();
            List<PropertyConfiguration> fkRelationKeys = new List<PropertyConfiguration>();
            List<EntityConfiguration> configs = DoGenEntityConfigurations();

            foreach (EntityConfiguration ec in configs)
            {
                if (ec.IsReadOnly)
                {
                    continue;
                }

                //for creating primary DEFAULT_KEY
                StringBuilder sbPK = new StringBuilder();
                //for creating index
                StringBuilder sbIndex = new StringBuilder();
                //for creating table
                StringBuilder sbTable = new StringBuilder();
                //for creating fkRelation
                StringBuilder sbFkRelation = new StringBuilder();

                tables.Add(ec.MappingName);

                //create table
                sbTable.Append(string.Format("CREATE TABLE [dbo].[{0}] (\r\n", ec.MappingName));

                if (ec.BaseEntity != null)
                {
                    List<PropertyConfiguration> pkConfigs = new List<PropertyConfiguration>();
                    foreach (PropertyConfiguration pc in ec.Properties)
                    {
                        if (pc.IsPrimaryKey)
                        {
                            pkConfigs.Add(pc);
                        }
                    }

                    if (pkConfigs.Count == 1 && (!pkConfigs[0].IsFriendKey))
                    {
                        fkRelations.Add(new KeyValuePair<string, string>(ec.MappingName, GetEntityConfigurationInConfigs(configs, ec.BaseEntity).MappingName));
                        fkRelationKeys.Add(pkConfigs[0]);
                    }
                }

                if (ec.IsRelation)
                {
                    Dictionary<string, int> relatedEntities = new Dictionary<string, int>();
                    Dictionary<string, PropertyConfiguration> relatedEntityKeys = new Dictionary<string, PropertyConfiguration>();
                    foreach (PropertyConfiguration pc in ec.Properties)
                    {
                        if (pc.IsRelationKey)
                        {
                            if (!relatedEntities.ContainsKey(pc.RelatedType))
                            {
                                relatedEntities.Add(pc.RelatedType, 1);
                                relatedEntityKeys.Add(pc.RelatedType, pc);
                            }
                            else
                            {
                                relatedEntities[pc.RelatedType]++;
                            }
                        }
                    }

                    foreach (string key in relatedEntities.Keys)
                    {
                        if (relatedEntities[key] == 1)
                        {
                            fkRelations.Add(new KeyValuePair<string, string>(ec.MappingName, GetEntityConfigurationInConfigs(configs, key).MappingName));
                            fkRelationKeys.Add(relatedEntityKeys[key]);
                        }
                    }
                }

                //create primary key
                sbPK.Append(string.Format("ALTER TABLE [dbo].[{0}] WITH NOCHECK ADD\r\nCONSTRAINT [{1}] PRIMARY KEY CLUSTERED\r\n(\r\n", ec.MappingName, "PK_" + (ec.MappingName).Replace(" ", "_")));

                Dictionary<string, List<string>> inherittedColumns = new Dictionary<string, List<string>>();

                List<string> pks = new List<string>();
                foreach (PropertyConfiguration pc in ec.Properties)
                {
                    if (((!pc.IsInherited) || pc.IsPrimaryKey) && ((!pc.IsQueryProperty) || (pc.QueryType == "FkReverseQuery")))
                    {
                        if (pc.IsFriendKey)
                        {
                            fkRelations.Add(new KeyValuePair<string, string>(ec.MappingName, GetEntityConfigurationInConfigs(configs, pc.RelatedType).MappingName));
                            fkRelationKeys.Add(pc);
                        }

                        GenerateColumn(ec, sbPK, sbIndex, sbTable, pc);
                    }
                    if (pc.IsPrimaryKey)
                    {
                        pks.Add(pc.MappingName);
                    }
                }

                sbTable.Append(") ON [PRIMARY]\r\nGO\r\n\r\n");
                sbPK.Append(") ON [PRIMARY]\r\nGO\r\n\r\n");

                sb.Append(sbTable.ToString().Replace(",\r\n)", "\r\n)"));

                if (!sbPK.ToString().Contains("(\r\n)"))
                {
                    sb.Append(sbPK.ToString().Replace(",\r\n)", "\r\n)"));
                    sb.Append(sbIndex);
                }
            }

            StringBuilder finalSb = new StringBuilder();
            for (int i = 0; i < fkRelations.Count; i++)
            {
                KeyValuePair<string, string> fkRelation = fkRelations[i];
                PropertyConfiguration pc = fkRelationKeys[i];

                //delete existing fkRelation
                finalSb.Append(string.Format("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[FK_{0}_{2}_{1}]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)\r\nALTER TABLE [dbo].[{0}] DROP CONSTRAINT FK_{0}_{2}_{1}\r\nGO\r\n\r\n", fkRelation.Key, fkRelation.Value, pc.MappingName));
            }

            foreach (string table in tables)
            {
                //delete existing table
                finalSb.Append(string.Format("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\ndrop table [dbo].[{0}]\r\nGO\r\n\r\n", table));
            }

            finalSb.Append(sb.ToString());

            for (int i = 0; i < fkRelations.Count; i++)
            {
                //create fk relations
                KeyValuePair<string, string> fkRelation = fkRelations[i];
                PropertyConfiguration pc = fkRelationKeys[i];

                string relatedKeyMappingName = null;
                if (pc.RelatedForeignKey != null)
                {
                    EntityConfiguration ec = GetEntityConfigurationInConfigs(configs, pc.RelatedType);
                    relatedKeyMappingName = ec.GetPropertyConfiguration(pc.RelatedForeignKey).MappingName;
                }
                finalSb.Append(string.Format("ALTER TABLE [dbo].[{0}] ADD CONSTRAINT [FK_{0}_{2}_{1}] FOREIGN KEY ( [{2}] ) REFERENCES [dbo].[{1}] ( [{3}] ) NOT FOR REPLICATION\r\nGO\r\n\r\n", fkRelation.Key, fkRelation.Value, pc.MappingName, relatedKeyMappingName ?? pc.MappingName));
            }

            foreach (EntityConfiguration ec in configs)
            {
                if (!string.IsNullOrEmpty(ec.AdditionalSqlScript))
                {
                    finalSb.Append(ec.AdditionalSqlScript);
                }
            }

            return finalSb.ToString();
        }

        private EntityConfiguration GetEntityConfigurationInConfigs(List<EntityConfiguration> configs, string entityName)
        {
            foreach (EntityConfiguration ec in configs)
            {
                if (ec.Name == entityName)
                {
                    return ec;
                }
            }

            return null;
        }

        private static void GenerateColumn(EntityConfiguration ec, StringBuilder sbPK, StringBuilder sbIndex, StringBuilder sbTable, PropertyConfiguration pc)
        {
            string columnTypeAppendix = string.Empty;

            if (pc.IsReadOnly && (pc.SqlType == "int" || pc.SqlType == "bigint" || pc.SqlType == "smallint") && ec.BaseEntity == null)
            {
                columnTypeAppendix = " IDENTITY (1, 1)";
            }

            sbTable.Append(string.Format("[{0}] {1}{2},\r\n", pc.MappingName, pc.SqlType + columnTypeAppendix, pc.IsPrimaryKey || pc.IsIndexProperty || pc.IsNotNull || pc.IsReadOnly ? " NOT NULL" : " NULL"));

            if (pc.SqlDefaultValue != null)
            {
                sbIndex.Append(string.Format("ALTER TABLE [dbo].[{0}] ADD\r\n	CONSTRAINT [DF_{0}_{1}] DEFAULT ({2}) FOR [{1}]\r\nGO\r\n\r\n", ec.MappingName, pc.MappingName, pc.SqlDefaultValue));
            }

            if (pc.IsPrimaryKey)
            {
                sbPK.Append(string.Format("[{0}],\r\n", pc.MappingName));
            }

            if (pc.IsIndexProperty && (!pc.IsPrimaryKey))
            {
                sbIndex.Append(string.Format("CREATE INDEX [{0}] ON [dbo].[{1}]([{0}]{2}) ON [PRIMARY]\r\nGO\r\n\r\n", pc.MappingName, ec.MappingName, pc.IsIndexPropertyDesc ? " DESC" : string.Empty));
            }
        }

        #endregion
    }
}
