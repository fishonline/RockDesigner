using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Rock.Orm.Common.Design;

namespace Rock.Orm.Common
{
    public class StaCodeGenHelper
    {
        private string outNs;

        /// <summary>
        /// Global的前缀
        /// </summary>
        /// <param name="langIndex"></param>
        /// <returns></returns>
        private string GetGlobalPrefix(int langIndex)
        {
            return langIndex == 0 ? "global::" : "Global.";
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public StaCodeGenHelper(string outNs)
        {
            this.outNs = outNs;
        }

        #region Helper Methods
        /// <summary>
        /// 分析实体的属性上的attributes
        /// </summary>
        /// <typeparam name="AttributeType"></typeparam>
        /// <param name="pi"></param>
        /// <returns></returns>
        internal static AttributeType GetPropertyAttribute<AttributeType>(PropertyInfo pi)
        {
            object[] attrs = pi.GetCustomAttributes(typeof(AttributeType), false);
            if (attrs != null && attrs.Length > 0)
            {
                return (AttributeType)attrs[0];
            }
            return default(AttributeType);
        }
        /// <summary>
        /// 分析实体上[...]修饰的attribute
        /// </summary>
        /// <typeparam name="AttributeType"></typeparam>
        /// <param name="pi"></param>
        /// <returns></returns>
        internal static AttributeType[] GetEntityAttributes<AttributeType>(PropertyInfo pi)
        {
            object[] attrs = pi.GetCustomAttributes(typeof(AttributeType), false);
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

        internal static AttributeType GetEntityAttribute<AttributeType>(Type type)
        {
            object[] attrs = type.GetCustomAttributes(typeof(AttributeType), false);
            if (attrs != null && attrs.Length > 0)
            {
                return (AttributeType)attrs[0];
            }
            return default(AttributeType);
        }

        internal static AttributeType[] GetEntityAttributes<AttributeType>(Type type)
        {
            object[] attrs = type.GetCustomAttributes(typeof(AttributeType), false);
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

        internal static QueryAttribute GetPropertyQueryAttribute(PropertyInfo item)
        {
            QueryAttribute qa = GetPropertyAttribute<PkQueryAttribute>(item);
            if (qa == null) qa = GetPropertyAttribute<FkQueryAttribute>(item);
            if (qa == null) qa = GetPropertyAttribute<CustomQueryAttribute>(item);
            if (qa == null) qa = GetPropertyAttribute<PkReverseQueryAttribute>(item);
            if (qa == null) qa = GetPropertyAttribute<FkReverseQueryAttribute>(item);
            if (qa == null) qa = GetPropertyAttribute<ManyToManyQueryAttribute>(item);

            return qa;
        }

        private string GetOutputNamespace(Type type)
        {
            OutputNamespaceAttribute outNsAttr = GetEntityAttribute<OutputNamespaceAttribute>(type);
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

        #region Gen c# Entities

        /// <summary>
        /// 处理设计类中所有的实体
        /// </summary>
        /// <param name="ass"></param>
        /// <param name="outLang"></param>
        /// <returns></returns>
        public string GenEntitiesEx(Assembly ass, int outLang)
        {

            CodeCompileUnit unit = new CodeCompileUnit();
            CodeNamespace ns;
            List<EntityConfiguration> entityConfigs = DoGenEntityConfigurations(ass);
            int i = 0;
            foreach (Type type in ass.GetTypes())
            {
                //IsAssignableFrom(type c):1 c与当前类是同类；2 当前类是c的父类； 3 当前类是c的接口
                if (typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(type) 
                    && typeof(Rock.Orm.Common.Design.Entity) != type                     
                    )
                {
                    ns = new CodeNamespace(GetOutputNamespace(type));
                    unit.Namespaces.Add(ns);
                    ns.Imports.Add(new CodeNamespaceImport("System"));
                    ns.Imports.Add(new CodeNamespaceImport("System.Xml.Serialization"));
                    ns.Imports.Add(new CodeNamespaceImport("Rock.Orm.Common"));
                    GenEntityEx(ns, type, SerializationManager.Serialize(entityConfigs[i]), outLang);

                    ++i;
                }
            }

            CodeDomProvider provider = null;
            switch (outLang)
            {
                case 0: provider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                case 1: provider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
            }

            StringBuilder codeBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(codeBuilder);
            IndentedTextWriter indentedWriter = new IndentedTextWriter(stringWriter, "  ");
            indentedWriter.Indent = 2;
            provider.GenerateCodeFromCompileUnit(unit, indentedWriter, new CodeGeneratorOptions());
            return codeBuilder.ToString();
        }

        #region Generate C# Entities

        private void GenPropertyQueryCode(StringBuilder sb, Type type, List<string> generatedProperties)
        {
            foreach (Type item in type.GetInterfaces())
            {
                if (typeof(Rock.Orm.Common.Design.Entity) != item)
                {
                    GenPropertyQueryCode(sb, item, generatedProperties);
                }
            }

            foreach (PropertyInfo item in type.GetProperties())
            {
                QueryAttribute qa = GetPropertyQueryAttribute(item);
                if ((qa == null || (qa.QueryType == QueryType.FkReverseQuery)) && (!generatedProperties.Contains(item.Name)))
                {
                    sb.Append("\t\t\tpublic static PropertyItem ");
                    sb.Append(item.Name);
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
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

        private void GenSetPropertyValuesFromDataRow(StringBuilder sb, Type type, List<string> generatedProperties, int outLang)
        {
            foreach (Type item in type.GetInterfaces())
            {
                if (typeof(Rock.Orm.Common.Design.Entity) != item)
                {
                    GenSetPropertyValuesFromDataRow(sb, item, generatedProperties, outLang);
                }
            }

            foreach (PropertyInfo item in type.GetProperties())
            {
                QueryAttribute qa = GetPropertyQueryAttribute(item);
                QueryDescriber describer = null;
                if ((qa == null || (qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))) && (!generatedProperties.Contains(item.Name)))
                {
                    sb.Append("\t\t\tif (!row.IsNull(");
                    sb.Append(generatedProperties.Count);
                    sb.Append(")) _");
                    sb.Append(item.Name);
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
                    {
                        describer = new QueryDescriber(qa, item, item.PropertyType.IsArray ? Util.GetOriginalTypeOfArrayType(item.PropertyType) : item.PropertyType, type);
                        sb.Append("_");
                        sb.Append(describer.RelatedForeignKey);
                    }
                    sb.Append(" = ");
                    sb.Append("(");
                    if (GetPropertyAttribute<CompoundUnitAttribute>(item) != null)
                    {
                        sb.Append("string");
                        sb.Append(")row");
                        sb.Append("[");
                        sb.Append(generatedProperties.Count);
                        sb.Append("];\r\n");
                    }
                    else
                    {
                        if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
                        {
                            sb.Append(describer.RelatedForeignKeyType.ToString());
                            if (describer.RelatedForeignKeyType == typeof(Guid))
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
                            if (item.PropertyType == typeof(Guid))
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

        private void GenGetPropertyValues(StringBuilder sb, Type type, List<string> generatedProperties)
        {
            foreach (Type item in type.GetInterfaces())
            {
                if (typeof(Rock.Orm.Common.Design.Entity) != item)
                {
                    GenGetPropertyValues(sb, item, generatedProperties);
                }
            }

            foreach (PropertyInfo item in type.GetProperties())
            {
                QueryAttribute qa = GetPropertyAttribute<QueryAttribute>(item);

                if ((qa == null || (qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))) && (!generatedProperties.Contains(item.Name)))
                {
                    sb.Append("_");
                    sb.Append(item.Name);
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
                    {
                        QueryDescriber describer = new QueryDescriber(qa, item, item.PropertyType.IsArray ? Util.GetOriginalTypeOfArrayType(item.PropertyType) : item.PropertyType, type);
                        sb.Append("_");
                        sb.Append(describer.RelatedForeignKey);
                    }
                    sb.Append(", ");

                    generatedProperties.Add(item.Name);
                }
            }
        }

        private void GenProperties(StringBuilder sbFields, StringBuilder sbProperties, StringBuilder sbReloadQueries, Type type, int outLang)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            PropertyInfo[] pis = type.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                list.Add(pi);
            }
            foreach (PropertyInfo pi in Util.DeepGetProperties(GetContractInterfaceTypes(type)))
            {
                list.Add(pi);
            }
            foreach (PropertyInfo item in list)
            {
                CommentAttribute ca = GetPropertyAttribute<CommentAttribute>(item);
                if (ca != null)
                {
                    sbProperties.Append("\t\t/// <summary>\r\n");
                    sbProperties.Append("\t\t/// ");
                    sbProperties.Append(ca.Content.Replace("\n", "\n\t\t/// "));
                    sbProperties.Append("\r\n\t\t/// </summary>\r\n");
                }

                if (GetPropertyAttribute<CompoundUnitAttribute>(item) != null)
                {
                    GenCompoundUnitProperty(sbFields, sbProperties, item, outLang);
                }
                else if (GetPropertyQueryAttribute(item) != null)
                {
                    GenQueryProperty(sbFields, sbProperties, sbReloadQueries, item, type, outLang);
                }
                else
                {
                    GenNormalProperty(sbFields, sbProperties, item, outLang);
                }
            }
        }

        private Type[] GetContractInterfaceTypes(Type type)
        {
            List<Type> list = new List<Type>();
            Type[] interfaceTypes = type.GetInterfaces();
            foreach (Type interfaceType in interfaceTypes)
            {
                if (!typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(interfaceType))
                {
                    bool isInOtherInterfaces = false;
                    foreach (Type item in interfaceTypes)
                    {
                        if (item != interfaceType && typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(item))
                        {
                            foreach (Type obj in item.GetInterfaces())
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

        private void GenQueryProperty(StringBuilder sbFields, StringBuilder sbProperties, StringBuilder sbReloadQuery, PropertyInfo item, Type type, int outLang)
        {
            QueryAttribute qa = GetPropertyQueryAttribute(item);
            QueryDescriber describer = new QueryDescriber(qa, item, item.PropertyType.IsArray ? Util.GetOriginalTypeOfArrayType(item.PropertyType) : item.PropertyType, type);

            sbFields.Append("\t\tprotected ");
            string propertyType;
            if (item.PropertyType.ToString().EndsWith("[]"))
            {
                propertyType = GetOutputNamespace(Util.GetOriginalTypeOfArrayType(item.PropertyType)) + "." + RemoveTypePrefix(GenType(item.PropertyType.ToString().TrimEnd('[', ']'))) + "ArrayList";
            }
            else
            {
                propertyType = GetOutputNamespace(item.PropertyType) + "." + RemoveTypePrefix(GenType(item.PropertyType.ToString()));
            }
            propertyType = GetGlobalPrefix(0) + propertyType;
            sbFields.Append(propertyType);
            sbFields.Append(" _");
            sbFields.Append(item.Name);
            sbFields.Append(";\r\n");

            if (qa != null && qa.QueryType == QueryType.FkReverseQuery)
            {
                Type fkType = describer.RelatedForeignKeyType;

                sbFields.Append("\t\tprotected ");
                sbFields.Append(GenType(fkType.ToString()));
                if (fkType.IsValueType)
                {
                    sbFields.Append("?");
                }
                sbFields.Append(" _");
                sbFields.Append(item.Name);
                sbFields.Append("_");
                sbFields.Append(describer.RelatedForeignKey);
                sbFields.Append(";\r\n");
            }

            if (GetPropertyAttribute<SerializationIgnoreAttribute>(item) != null)
            {
                sbProperties.Append("\t\t[XmlIgnore]\r\n");
            }
            CustomAttribute[] customAttributes = GetEntityAttributes<CustomAttribute>(item);
            if (customAttributes != null)
            {
                foreach (CustomAttribute customAttribute in customAttributes)
                {
                    sbProperties.Append(string.Format("\t\t[{0}]\r\n", customAttribute.AttributeType.FullName));
                }
            }

            sbProperties.Append("\t\tpublic ");
            sbProperties.Append(propertyType);
            sbProperties.Append(" ");
            sbProperties.Append(item.Name);
            sbProperties.Append("\r\n\t\t{\r\n");
            sbProperties.Append("\t\t\tget\r\n\t\t\t{\r\n\t\t\t\tif (!IsQueryPropertyLoaded(\"");
            sbProperties.Append(item.Name);
            sbProperties.Append("\")) ");
            GenReloadQuery(sbProperties, qa, item, type, outLang);
            if (item.PropertyType.IsArray)
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
            sbReloadQuery.Append(GetOutputNamespace(type) + "." + type.Name);
            sbReloadQuery.Append("\", \"");
            sbReloadQuery.Append(item.Name);
            sbReloadQuery.Append("\"))) ");
            GenReloadQuery(sbReloadQuery, qa, item, type, outLang);

            if (item.CanWrite)
            {
                if (item.PropertyType.IsArray)
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

                if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
                {
                    Type fkType = describer.RelatedForeignKeyType;

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

        private void GenReloadQuery(StringBuilder sb, QueryAttribute qa, PropertyInfo item, Type type, int outLang)
        {
            QueryDescriber describer = new QueryDescriber(qa, item, item.PropertyType.IsArray ? Util.GetOriginalTypeOfArrayType(item.PropertyType) : item.PropertyType, type);

            string propertyType;
            if (item.PropertyType.ToString().EndsWith("[]"))
            {
                propertyType = GetOutputNamespace(Util.GetOriginalTypeOfArrayType(item.PropertyType)) + "." + RemoveTypePrefix(GenType(item.PropertyType.ToString().TrimEnd('[', ']'))) + "ArrayList";
            }
            else
            {
                propertyType = GetOutputNamespace(item.PropertyType) + "." + RemoveTypePrefix(GenType(item.PropertyType.ToString()));
            }
            propertyType = GetGlobalPrefix(0) + propertyType;

            sb.Append("{ ");

            if (item.PropertyType.IsArray)
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

            if (item.PropertyType.IsArray)
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

        private void GenNormalProperty(StringBuilder sbFields, StringBuilder sbProperties, PropertyInfo item, int outLang)
        {
            sbFields.Append("\t\tprotected ");
            sbFields.Append(GenType(item.PropertyType.ToString()));
            sbFields.Append(" _");
            sbFields.Append(item.Name);
            sbFields.Append(";\r\n");

            if (GetPropertyAttribute<SerializationIgnoreAttribute>(item) != null)
            {
                sbProperties.Append("\t\t[XmlIgnore]\r\n");
            }

            CustomAttribute[] customAttributes = GetEntityAttributes<CustomAttribute>(item);
            if (customAttributes != null)
            {
                foreach (CustomAttribute customAttribute in customAttributes)
                {
                    sbProperties.Append(string.Format("\t\t[{0}]\r\n", customAttribute.AttributeType.FullName));
                }
            }

            sbProperties.Append("\t\tpublic ");
            sbProperties.Append(GenType(item.PropertyType.ToString()));
            sbProperties.Append(" ");
            sbProperties.Append(item.Name);
            sbProperties.Append("\r\n\t\t{\r\n");
            if (item.CanRead)
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

        private void GenCompoundUnitProperty(StringBuilder sbFields, StringBuilder sbProperties, PropertyInfo item, int outLang)
        {
            sbFields.Append("\t\tprotected string _");
            sbFields.Append(item.Name);
            sbFields.Append(";\r\n");

            if (GetPropertyAttribute<SerializationIgnoreAttribute>(item) != null)
            {
                sbProperties.Append("\t\t[XmlIgnore]\r\n");
            }

            CustomAttribute[] customAttributes = GetEntityAttributes<CustomAttribute>(item);
            if (customAttributes != null)
            {
                foreach (CustomAttribute customAttribute in customAttributes)
                {
                    sbProperties.Append(string.Format("\t\t[{0}]\r\n", customAttribute.AttributeType.FullName));
                }
            }

            sbProperties.Append("\t\tpublic ");
            sbProperties.Append(GenType(item.PropertyType.ToString()));
            sbProperties.Append(" ");
            sbProperties.Append(item.Name);
            sbProperties.Append("\r\n\t\t{\r\n");
            if (item.CanRead)
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
                return GenTypeCSharp(typeStr);
        }

        private string GenTypeCSharp(string typeStr)
        {
            if (typeStr.StartsWith("System.Nullable`1["))
            {
                return GenTypeCSharp(typeStr.Substring("System.Nullable`1[".Length).Trim('[', ']')) + "?";
            }

            return typeStr;
        }

        #endregion

        #endregion

        #region new method
        /// <summary>
        /// 处理一个实体
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="type"></param>
        /// <param name="configXmlContent"></param>
        /// <param name="outLang"></param>
        private void GenEntityEx(CodeNamespace ns, Type type, string configXmlContent, int outLang)
        {
            CodeTypeDeclaration entity;
            StringBuilder sb = new StringBuilder();

            entity = new CodeTypeDeclaration(type.Name + "ArrayList");
            entity.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.SerializableAttribute))));
            entity.IsClass = true;
            entity.IsPartial = true;
            if (outLang == 0)
            {
                entity.BaseTypes.Add("Rock.Orm.Common.EntityArrayList<" + GetOutputNamespace(type) + "." + type.Name + ">");
            }
            else
            {
                entity.BaseTypes.Add("Rock.Orm.Common.EntityArrayList(Of " + GetOutputNamespace(type) + "." + type.Name + ")");
            }
            ns.Types.Add(entity);

            entity = new CodeTypeDeclaration(type.Name);
            ns.Types.Add(entity);
            entity.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.SerializableAttribute))));
            entity.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(EmbeddedEntityConfigurationAttribute)),
                new CodeAttributeArgument(new CodePrimitiveExpression(configXmlContent))));
            entity.IsClass = true;
            entity.IsPartial = true;
            Type[] interfaces = type.GetInterfaces();
            bool findNonEntityBaseEntity = false;
            string entityBaseTypeName = null;
            foreach (Type item in interfaces)
            {
                if (typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(item) && (typeof(Rock.Orm.Common.Design.Entity) != item))
                {
                    entityBaseTypeName = GetOutputNamespace(item) + '.' + item.Name;
                    entity.BaseTypes.Add(entityBaseTypeName);
                    findNonEntityBaseEntity = true;
                    break;
                }
            }
            if (!findNonEntityBaseEntity)
            {
                entity.BaseTypes.Add(typeof(Rock.Orm.Common.Entity));
            }

            //append custom implement interfaces
            foreach (object obj in type.GetCustomAttributes(true))
            {
                if (obj.GetType() == typeof(ImplementInterfaceAttribute))
                {
                    entity.BaseTypes.Add(new CodeTypeReference(((ImplementInterfaceAttribute)obj).InterfaceFullName, CodeTypeReferenceOptions.GlobalReference));
                }
            }

            CommentAttribute ca = GetEntityAttribute<CommentAttribute>(type);
            if (ca != null)
            {
                //sb.Append("\t/// <summary>\r\n");
                //sb.Append("\t/// ");
                //sb.Append(ca.Content.Replace("\n", "\n\t/// "));
                //sb.Append("\r\n\t/// </summary>\r\n");
                entity.Comments.Add(new CodeCommentStatement("<summary>", true));
                entity.Comments.Add(new CodeCommentStatement(ca.Content, true));
                entity.Comments.Add(new CodeCommentStatement("</summary>", true));
            }



            //generate GetEntityConfiguration()
            GenGetEntityConfigurationEx(entity, type);

            //generate properties
            CodeStatementCollection reloadQueryStatements = new CodeStatementCollection();
            GenPropertiesEx(entity, reloadQueryStatements, type, outLang);


            //StringBuilder sbFields = new StringBuilder();
            //StringBuilder sbProperties = new StringBuilder();
            //StringBuilder sbReloadQueries = new StringBuilder();

            //GenProperties(sbFields, sbProperties, sbReloadQueries, type);



            //sb.AppendLine(sbFields.ToString());
            //sb.Append(sbProperties.ToString());

            //generate Get & Set PropertyValues
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "ReloadQueries";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            method.ReturnType = null;
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(bool), "includeLazyLoadQueries"));
            //sb.Append("\t\t#region Get & Set PropertyValues\r\n\r\n");

            //sb.Append("\t\tpublic override void ReloadQueries(bool includeLazyLoadQueries)\r\n\t\t{\r\n");
            if (findNonEntityBaseEntity)
            {
                method.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "ReloadQueries", new CodeExpression[] { new CodeArgumentReferenceExpression("includeLazyLoadQueries") }));
                //sb.Append("\t\t\tbase.ReloadQueries(includeLazyLoadQueries);\r\n");
            }
            method.Statements.AddRange(reloadQueryStatements);
            //sb.Append(sbReloadQueries.ToString());
            //sb.Append("\t\t}\r\n\r\n");
            entity.Members.Add(method);

            List<string> generatedProperties = new List<string>();


            method = new CodeMemberMethod();
            method.Name = "GetPropertyValues";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            method.ReturnType = new CodeTypeReference(new CodeTypeReference(typeof(object)), 1);

            //sb.Append("\t\tpublic override object[] GetPropertyValues()\r\n\t\t{\r\n");
            //sb.Append("\t\t\treturn new object[] { ");
            StringBuilder sbPropertyValuesList = new StringBuilder();
            GenGetPropertyValues(sbPropertyValuesList, type, generatedProperties);
            //sb.Append(sbPropertyValuesList.ToString().TrimEnd(' ', ','));
            //sb.Append(" };\r\n\t\t}\r\n\r\n");
            CodeExpression[] arrayInit;
            string[] fieldsList = sbPropertyValuesList.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            arrayInit = new CodeExpression[generatedProperties.Count];
            for (int i = 0; i < generatedProperties.Count; i++)
            {
                arrayInit[i] = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), fieldsList[i].Trim());
            }
            method.Statements.Add(new CodeMethodReturnStatement(new CodeArrayCreateExpression(typeof(object), arrayInit)));
            entity.Members.Add(method);


            method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            method.Name = "SetPropertyValues";
            method.ReturnType = null;
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IDataReader), "reader"));
            //sb.Append("\t\tpublic override void SetPropertyValues(System.Data.IDataReader reader)\r\n\t\t{\r\n");
            generatedProperties.Clear();
            GenSetPropertyValuesFromReaderEx(method.Statements, type, generatedProperties, outLang);
            //sb.Append("\t\t\tReloadQueries(false);\r\n");
            //sb.Append("\t\t}\r\n\r\n");
            method.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "ReloadQueries", new CodeExpression[] { new CodePrimitiveExpression(false) }));
            entity.Members.Add(method);

            method = new CodeMemberMethod();
            method.Name = "SetPropertyValues";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            method.ReturnType = null;
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DataRow), "row"));


            //sb.Append("\t\tpublic override void SetPropertyValues(System.Data.DataRow row)\r\n\t\t{\r\n");
            generatedProperties.Clear();
            GenSetPropertyValuesFromDataRowEx(method.Statements, type, generatedProperties);
            //sb.Append("\t\t\tReloadQueries(false);\r\n");
            //sb.Append("\t\t}\r\n");
            method.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "ReloadQueries", new CodeExpression[] { new CodePrimitiveExpression(false) }));
            entity.Members.Add(method);

            //sb.Append("\r\n\t\t#endregion\r\n\r\n");

            //generate Entity Equals
            //sb.Append("\t\t#region Equals\r\n\r\n");

            string entityOutputTypeName = GetOutputNamespace(type) + "." + type.Name;
            CodeTypeReference entityOutputTypeNameRef = new CodeTypeReference(entityOutputTypeName, CodeTypeReferenceOptions.GlobalReference);
            //sb.Append("\t\tpublic override int GetHashCode() { return base.GetHashCode(); }\r\n\r\n");
            method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            method.Name = "GetHashCode";
            method.ReturnType = new CodeTypeReference(typeof(int));
            method.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "GetHashCode", new CodeExpression[] { })));
            entity.Members.Add(method);

            //sb.Append("\t\tpublic static bool operator ==(" + entityOutputTypeName + " left, " + entityOutputTypeName + " right) { return ((object)left) != null ? left.Equals(right) : ((object)right) == null ? true : false; }\r\n\r\n");
            //sb.Append("\t\tpublic static bool operator !=(" + entityOutputTypeName + " left, " + entityOutputTypeName + " right) { return ((object)left) != null ? !left.Equals(right) : ((object)right) == null ? false : true; }\r\n\r\n");
            if (outLang == 0)
            {
                entity.Members.Add(new CodeSnippetTypeMember("\t\tpublic static bool operator ==(" + GetGlobalPrefix(outLang) + entityOutputTypeName + " left, " + GetGlobalPrefix(outLang) + entityOutputTypeName + " right) { return ((object)left) != null ? left.Equals(right) : ((object)right) == null ? true : false; }\r\n\r\n"));
                entity.Members.Add(new CodeSnippetTypeMember("\t\tpublic static bool operator !=(" + GetGlobalPrefix(outLang) + entityOutputTypeName + " left, " + GetGlobalPrefix(outLang) + entityOutputTypeName + " right) { return ((object)left) != null ? !left.Equals(right) : ((object)right) == null ? false : true; }\r\n\r\n"));
            }
            else if (outLang == 1)
            {
                entity.Members.Add(new CodeSnippetTypeMember("\t\tPublic Shared Operator =(ByVal left As " + GetGlobalPrefix(outLang) + entityOutputTypeName + ", ByVal right As " + GetGlobalPrefix(outLang) + entityOutputTypeName + ") As Boolean\n\t\t\tIf CType(left, Object) is Nothing then\n\t\t\t\tReturn CType(right, Object) is Nothing\n\t\t\tElse\n\t\t\t\tReturn left.Equals(right)\n\t\t\tEnd If\n\t\tEnd Operator"));
                entity.Members.Add(new CodeSnippetTypeMember("\t\tPublic Shared Operator <>(ByVal left As " + GetGlobalPrefix(outLang) + entityOutputTypeName + ", ByVal right As " + GetGlobalPrefix(outLang) + entityOutputTypeName + ") As Boolean\n\t\t\tIf CType(left, Object) is Nothing then\n\t\t\t\tReturn (CType(right, Object) is Nothing) = False\n\t\t\tElse\n\t\t\t\tReturn left.Equals(right) = False\n\t\t\tEnd If\n\t\tEnd Operator"));
            }
            //sb.Append("\t\tpublic override bool Equals(object obj)\r\n\t\t{\r\n\t\t\treturn obj == null || (!(obj is " + entityOutputTypeName + ")) ? false : ((object)this) == ((object)obj) ? true : this.isAttached && ((" + entityOutputTypeName + ")obj).isAttached");
            method = new CodeMemberMethod();
            method.Name = "Equals";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            method.ReturnType = new CodeTypeReference(typeof(bool));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "obj"));
            //if (obj == null) return false;
            //if ((obj is global::Entities.LocalUser) == false) return false;
            //if (((object)this) == ((object)obj)) return true;
            //return this.isAttached && ((global::Entities.LocalUser)obj).isAttached && this.ID == ((global::Entities.LocalUser)obj).ID;
            method.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeArgumentReferenceExpression("obj"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null)), new CodeStatement[] { new CodeMethodReturnStatement(new CodePrimitiveExpression(false)) }));
            method.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePrimitiveExpression(false), CodeBinaryOperatorType.ValueEquality, new CodeMethodInvokeExpression(new CodeTypeOfExpression(entityOutputTypeNameRef), "IsAssignableFrom", new CodeExpression[] { new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("obj"), "GetType", new CodeExpression[] { }) })), new CodeStatement[] { new CodeMethodReturnStatement(new CodePrimitiveExpression(false)) }));
            method.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeCastExpression(typeof(object), new CodeThisReferenceExpression()), CodeBinaryOperatorType.IdentityEquality, new CodeCastExpression(typeof(object), new CodeArgumentReferenceExpression("obj"))), new CodeStatement[] { new CodeMethodReturnStatement(new CodePrimitiveExpression(true)) }));
            CodeExpressionCollection equalExpressionCollection = new CodeExpressionCollection();

            PropertyInfo[] pis = Util.DeepGetProperties(type);
            bool hasPk = false;
            foreach (PropertyInfo pi in pis)
            {
                if (GetPropertyAttribute<PrimaryKeyAttribute>(pi) != null || GetPropertyAttribute<RelationKeyAttribute>(pi) != null)
                {
                    //sb.Append(string.Format(" && this.{0} == (({1})obj).{0}", pi.Name, entityOutputTypeName));
                    equalExpressionCollection.Add(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), pi.Name), CodeBinaryOperatorType.ValueEquality, new CodePropertyReferenceExpression(new CodeCastExpression(entityOutputTypeNameRef, new CodeArgumentReferenceExpression("obj")), pi.Name)));
                    //                    method.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("ret"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("ret"), CodeBinaryOperatorType.BooleanAnd, new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), pi.Name), CodeBinaryOperatorType.ValueEquality, new CodePropertyReferenceExpression(new CodeCastExpression(entityOutputTypeName, new CodeArgumentReferenceExpression("obj")), pi.Name)))));
                    hasPk = true;
                }
            }
            if (!hasPk)
            {
                //sb.Append(" && base.Equals(obj)");
                equalExpressionCollection.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Equals", new CodeExpression[] { new CodeArgumentReferenceExpression("obj") }));
            }
            CodeBinaryOperatorExpression ret = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "isAttached"), CodeBinaryOperatorType.BooleanAnd, new CodePropertyReferenceExpression(new CodeCastExpression(entityOutputTypeNameRef, new CodeArgumentReferenceExpression("obj")), "isAttached"));
            for (int i = 0; i < equalExpressionCollection.Count; i++)
            {
                ret = new CodeBinaryOperatorExpression(ret, CodeBinaryOperatorType.BooleanAnd, equalExpressionCollection[i]);
            }
            method.Statements.Add(new CodeMethodReturnStatement(ret));
            //sb.Append(";\r\n\t\t}\r\n");
            entity.Members.Add(method);
            //sb.Append("\r\n\t\t#endregion\r\n\r\n");

            CodeTypeDeclaration queryClass = new CodeTypeDeclaration();
            entity.Members.Add(queryClass);
            queryClass.Attributes = MemberAttributes.Public;
            queryClass.Name = "__Columns";

            if (entityBaseTypeName != null)
            {
                queryClass.BaseTypes.Add(new CodeTypeReference(entityBaseTypeName + ".__Columns", CodeTypeReferenceOptions.GlobalReference));
            }

            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Name = "__Columns";
            queryClass.Members.Add(constructor);

            if (entityBaseTypeName == null)
            {
                CodeMemberField aliasNameField = new CodeMemberField(new CodeTypeReference(typeof(string)), "aliasName");
                aliasNameField.Attributes = MemberAttributes.Family;
                queryClass.Members.Add(aliasNameField);
            }

            CodeConstructor constructor2 = new CodeConstructor();
            constructor2.Attributes = MemberAttributes.Public;
            constructor2.Name = "__Columns";
            constructor2.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "aliasName"));
            constructor2.Statements.Add(new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "aliasName"),
                new CodeVariableReferenceExpression("aliasName")
                ));
            queryClass.Members.Add(constructor2);

            //generate Query Code
            //sb.Append("\t\t#region QueryCode\r\n\r\n");
            //sb.Append("\t\tpublic ");
            if (findNonEntityBaseEntity)
            {
                //sb.Append("new ");
                queryClass.Attributes = queryClass.Attributes | MemberAttributes.New;
            }
            //sb.Append("abstract class _\r\n\t\t{\r\n");
            //sb.Append("\t\t\tprivate _() { }\r\n\r\n");
            generatedProperties.Clear();
            GenPropertyQueryCodeEx(queryClass, type, generatedProperties);

            CodeMemberField columnsInstanceField = new CodeMemberField(new CodeTypeReference("__Columns"), outLang == 1 ? "__" : "_");
            columnsInstanceField.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            if (entityBaseTypeName != null)
            {
                columnsInstanceField.Attributes |= MemberAttributes.New;
            }
            columnsInstanceField.InitExpression = new CodeObjectCreateExpression("__Columns");
            entity.Members.Add(columnsInstanceField);

            CodeMemberMethod aliasColumnsMethod = new CodeMemberMethod();
            aliasColumnsMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            if (entityBaseTypeName != null)
            {
                aliasColumnsMethod.Attributes |= MemberAttributes.New;
            }
            aliasColumnsMethod.Name = "__Alias";
            aliasColumnsMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "aliasName"));
            aliasColumnsMethod.ReturnType = new CodeTypeReference("__Columns");
            aliasColumnsMethod.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(
                new CodeTypeReference("__Columns"),
                new CodeVariableReferenceExpression("aliasName")
                )));
            entity.Members.Add(aliasColumnsMethod);


            //sb.Append("\t\t}\r\n");
            //sb.Append("\r\n\t\t#endregion\r\n");

            //sb.Append("\t}\r\n");

        }
        /// <summary>
        /// generate GetEntityConfiguration()
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        private void GenGetEntityConfigurationEx(CodeTypeDeclaration entity, Type type)
        {
            CodeMemberField field = new CodeMemberField();
            field.Name = "_" + type.Name + "EntityConfiguration";
            field.Attributes = MemberAttributes.Static | MemberAttributes.Family;
            field.Type = new CodeTypeReference(typeof(Rock.Orm.Common.EntityConfiguration));
            entity.Members.Add(field);

            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "GetEntityConfiguration";
            method.ReturnType = new CodeTypeReference(typeof(Rock.Orm.Common.EntityConfiguration));
            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            CodeConditionStatement conditionStatement = new CodeConditionStatement();
            conditionStatement.Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(entity.Name), field.Name), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
            conditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(entity.Name), field.Name),
                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(Rock.Orm.Common.MetaDataManager)), "GetEntityConfiguration", new CodeExpression[] { new CodePrimitiveExpression(GetOutputNamespace(type) + "." + type.Name) })));
            method.Statements.Add(conditionStatement);
            method.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(entity.Name), field.Name)));
            entity.Members.Add(method);
        }

        private void GenPropertyQueryCodeEx(CodeTypeDeclaration entity, Type type, List<string> generatedProperties)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            PropertyInfo[] pis = type.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                list.Add(pi);
            }
            foreach (PropertyInfo pi in Util.DeepGetProperties(GetContractInterfaceTypes(type)))
            {
                list.Add(pi);
            }

            foreach (PropertyInfo item in list)
            {
                QueryAttribute qa = GetPropertyQueryAttribute(item);
                if ((qa == null || (qa.QueryType == QueryType.FkReverseQuery || qa.QueryType == QueryType.PkQuery || qa.QueryType == QueryType.PkReverseQuery)) && (!generatedProperties.Contains(item.Name)))
                {
                    generatedProperties.Add(item.Name);

                    //gen static field
                    CodeMemberField field = new CodeMemberField();
                    field.Name = '_' + item.Name;
                    //sb.Append("\t\t\tpublic static PropertyItem ");
                    //sb.Append(item.Name);
                    if (qa != null && (qa.QueryType == QueryType.FkReverseQuery || qa.QueryType == QueryType.PkQuery || qa.QueryType == QueryType.PkReverseQuery) && (!item.PropertyType.IsArray))
                    {
                        //sb.Append("ID");
                        field.Name = '_' + item.Name + "ID";

                        #region gen cascade property items of FkReverseQuery/PkQuery/PkReverseQuery property

                        CodeMemberProperty cascadeColumnsProperty = new CodeMemberProperty();
                        cascadeColumnsProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                        cascadeColumnsProperty.HasGet = true;
                        cascadeColumnsProperty.HasSet = false;
                        cascadeColumnsProperty.Name = item.Name;
                        cascadeColumnsProperty.Type = new CodeTypeReference(GetOutputNamespace(item.PropertyType) + '.' + item.PropertyType.Name + ".__Columns", CodeTypeReferenceOptions.GlobalReference);
                        cascadeColumnsProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(
                            new CodeTypeReference(GetOutputNamespace(item.PropertyType) + '.' + item.PropertyType.Name + ".__Columns", CodeTypeReferenceOptions.GlobalReference),
                            new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(new CodeTypeReference(typeof(string))),
                            "Format",
                            new CodePrimitiveExpression("{0}_/*{1}*/"),
                            new CodeVariableReferenceExpression("aliasName"),
                            new CodePrimitiveExpression(item.Name)
                            ))));
                        entity.Members.Add(cascadeColumnsProperty);

                        #endregion
                    }
                    //sb.Append(" = new PropertyItem(\"");
                    //sb.Append(item.Name);
                    //sb.Append("\");\r\n");

                    if (qa == null || (qa != null && qa.QueryType == QueryType.FkReverseQuery))
                    {
                        field.Attributes = MemberAttributes.Family;
                        field.Type = new CodeTypeReference(typeof(PropertyItem));
                        string typeName = GetOutputNamespace(type) + "." + type.Name;
                        field.InitExpression = new CodeObjectCreateExpression(typeof(PropertyItem), new CodeExpression[] { new CodePrimitiveExpression(item.Name), new CodePrimitiveExpression(typeName) });
                        entity.Members.Add(field);

                        //gen property
                        CodeMemberProperty property = new CodeMemberProperty();
                        property.Name = item.Name;
                        if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
                        {
                            property.Name = property.Name + "ID";
                        }
                        property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                        property.Type = new CodeTypeReference(typeof(PropertyItem));
                        property.HasGet = true;
                        property.HasSet = false;
                        CodeConditionStatement condition = new CodeConditionStatement();
                        condition.Condition = new CodeBinaryOperatorExpression(
                            new CodeVariableReferenceExpression("aliasName"),
                            CodeBinaryOperatorType.IdentityEquality,
                            new CodePrimitiveExpression(null));
                        condition.TrueStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(field.Name)));
                        condition.FalseStatements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(
                            new CodeTypeReference(typeof(PropertyItem)),
                            new CodePrimitiveExpression(item.Name),
                            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(field.Name), "EntityConfiguration"),
                            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(field.Name), "PropertyConfiguration"),
                            new CodeVariableReferenceExpression("aliasName")
                            )));
                        property.GetStatements.Add(condition);
                        entity.Members.Add(property);
                    }
                }
            }
        }

        private void GenSetPropertyValuesFromDataRowEx(CodeStatementCollection statements, Type type, List<string> generatedProperties)
        {
            foreach (Type item in type.GetInterfaces())
            {
                //if (typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(item) && typeof(Rock.Orm.Common.Design.Entity) != item)
                if (typeof(Rock.Orm.Common.Design.Entity) != item)
                {
                    GenSetPropertyValuesFromDataRowEx(statements, item, generatedProperties);
                }
            }

            foreach (PropertyInfo item in type.GetProperties())
            {
                QueryAttribute qa = GetPropertyQueryAttribute(item);
                QueryDescriber describer = null;
                if ((qa == null || (qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))) && (!generatedProperties.Contains(item.Name)))
                {
                    CodeConditionStatement condition = new CodeConditionStatement();
                    condition.Condition = new CodeBinaryOperatorExpression(new CodePrimitiveExpression(false), CodeBinaryOperatorType.ValueEquality,
                        new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("row"), "IsNull", new CodeExpression[] { new CodePrimitiveExpression(generatedProperties.Count) }));
                    CodeAssignStatement assign = new CodeAssignStatement();

                    //sb.Append("\t\t\tif (!row.IsNull(");
                    //sb.Append(generatedProperties.Count);
                    //sb.Append(")) _");
                    //sb.Append(item.Name);
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
                    {
                        describer = new QueryDescriber(qa, item, item.PropertyType.IsArray ? Util.GetOriginalTypeOfArrayType(item.PropertyType) : item.PropertyType, type);
                        //sb.Append("_");
                        //sb.Append(describer._relatedForeignKey);
                        assign.Left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name + "_" + describer.RelatedForeignKey);
                    }
                    else
                    {
                        assign.Left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name);
                    }
                    //sb.Append(" = ");
                    //sb.Append("(");
                    if (GetPropertyAttribute<CompoundUnitAttribute>(item) != null)
                    {
                        //sb.Append("string");
                        //sb.Append(")row");
                        //sb.Append("[");
                        //sb.Append(generatedProperties.Count);
                        //sb.Append("];\r\n");
                        assign.Right = new CodeCastExpression(typeof(string), new CodeIndexerExpression(new CodeArgumentReferenceExpression("row"), new CodeExpression[] { new CodePrimitiveExpression(generatedProperties.Count) }));
                    }
                    else
                    {
                        if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
                        {
                            //sb.Append(describer.RelatedForeignKeyType.ToString());
                            if (describer.RelatedForeignKeyType == typeof(Guid))
                            {
                                //sb.Append(")GetGuid(row, ");
                                //sb.Append(generatedProperties.Count);
                                //sb.Append(");\r\n");
                                assign.Right = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetGuid", new CodeExpression[] { new CodeArgumentReferenceExpression("row"), new CodePrimitiveExpression(generatedProperties.Count) });
                            }
                            else if (describer.RelatedForeignKeyType == typeof(DateTime) || describer.RelatedForeignKeyType == typeof(DateTime?))
                            {
                                assign.Right = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetDateTime", new CodeExpression[] { new CodeArgumentReferenceExpression("row"), new CodePrimitiveExpression(generatedProperties.Count) });
                            }
                            else
                            {
                                //sb.Append(")row");
                                //sb.Append("[");
                                //sb.Append(generatedProperties.Count);
                                //sb.Append("];\r\n");
                                assign.Right = new CodeCastExpression(describer.RelatedForeignKeyType, new CodeIndexerExpression(new CodeArgumentReferenceExpression("row"), new CodeExpression[] { new CodePrimitiveExpression(generatedProperties.Count) }));
                            }
                        }
                        else
                        {
                            //sb.Append(GenType(item.PropertyType.ToString()));
                            if (item.PropertyType == typeof(Guid))
                            {
                                //sb.Append(")GetGuid(row, ");
                                //sb.Append(generatedProperties.Count);
                                //sb.Append(");\r\n");
                                assign.Right = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetGuid", new CodeExpression[] { new CodeArgumentReferenceExpression("row"), new CodePrimitiveExpression(generatedProperties.Count) });
                            }
                            else if (item.PropertyType == typeof(DateTime) || item.PropertyType == typeof(DateTime?))
                            {
                                assign.Right = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetDateTime", new CodeExpression[] { new CodeArgumentReferenceExpression("row"), new CodePrimitiveExpression(generatedProperties.Count) });
                            }
                            else
                            {
                                //sb.Append(")row");
                                //sb.Append("[");
                                //sb.Append(generatedProperties.Count);
                                //sb.Append("];\r\n");
                                assign.Right = new CodeCastExpression(item.PropertyType, new CodeIndexerExpression(new CodeArgumentReferenceExpression("row"), new CodeExpression[] { new CodePrimitiveExpression(generatedProperties.Count) }));
                            }
                        }
                    }
                    condition.TrueStatements.Add(assign);
                    statements.Add(condition);
                    generatedProperties.Add(item.Name);
                }
            }
        }

        private void GenSetPropertyValuesFromReaderEx(CodeStatementCollection statements, Type type, List<string> generatedProperties, int outLang)
        {
            foreach (Type item in type.GetInterfaces())
            {
                //if (typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(item) && typeof(Rock.Orm.Common.Design.Entity) != item)
                if (typeof(Rock.Orm.Common.Design.Entity) != item)
                {
                    GenSetPropertyValuesFromReaderEx(statements, item, generatedProperties, outLang);
                }
            }

            foreach (PropertyInfo item in type.GetProperties())
            {
                QueryAttribute qa = GetPropertyQueryAttribute(item);
                QueryDescriber describer = null;
                if ((qa == null || (qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))) && (!generatedProperties.Contains(item.Name)))
                {
                    CodeConditionStatement condition = new CodeConditionStatement();
                    condition.Condition = new CodeBinaryOperatorExpression(new CodePrimitiveExpression(false), CodeBinaryOperatorType.ValueEquality,
                        new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "IsDBNull", new CodeExpression[] { new CodePrimitiveExpression(generatedProperties.Count) }));
                    CodeAssignStatement assign = new CodeAssignStatement();
                    //sb.Append("\t\t\tif (!reader.IsDBNull(");
                    //sb.Append(generatedProperties.Count);
                    //sb.Append(")) _");
                    //sb.Append(item.Name);
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
                    {
                        describer = new QueryDescriber(qa, item, item.PropertyType.IsArray ? Util.GetOriginalTypeOfArrayType(item.PropertyType) : item.PropertyType, type);

                        //sb.Append("_");
                        //sb.Append(describer._relatedForeignKey);

                        assign.Left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name + "_" + describer.RelatedForeignKey);
                    }
                    else
                    {
                        assign.Left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name);
                    }
                    //sb.Append(" = ");
                    if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
                    {
                        //GenReaderGet(sb, describer.RelatedForeignKeyPropertyInfo);
                        //sb.Append("(");
                        //if (describer.RelatedForeignKeyPropertyInfo.PropertyType == typeof(Guid))
                        //{
                        //    sb.Append("reader, ");
                        //}
                        assign.Right = GenReaderGetEx(describer.RelatedForeignKeyPropertyInfo, generatedProperties.Count, outLang);
                    }
                    else
                    {
                        //GenReaderGet(sb, item);
                        //sb.Append("(");
                        //if (item.PropertyType == typeof(Guid))
                        //{
                        //    sb.Append("reader, ");
                        //}
                        assign.Right = GenReaderGetEx(item, generatedProperties.Count, outLang);
                    }
                    //sb.Append(generatedProperties.Count);
                    //sb.Append(");\r\n");

                    generatedProperties.Add(item.Name);

                    condition.TrueStatements.Add(assign);
                    statements.Add(condition);

                }
            }
        }

        private CodeExpression GenReaderGetEx(PropertyInfo item, int itemIndex, int outLang)
        {
            if (item.PropertyType == typeof(bool) || item.PropertyType == typeof(bool?))
            {
                //sb.Append("reader.GetBoolean");
                return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetBoolean", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(byte) || item.PropertyType == typeof(byte?))
            {
                //sb.Append("reader.GetByte");
                return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetByte", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(char) || item.PropertyType == typeof(char?))
            {
                //sb.Append("reader.GetChar");
                return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetChar", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(DateTime) || item.PropertyType == typeof(DateTime?))
            {
                //sb.Append("GetDateTime");
                return new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetDateTime", new CodeExpression[] { new CodeArgumentReferenceExpression("reader"), new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(decimal) || item.PropertyType == typeof(decimal?))
            {
                //sb.Append("reader.GetDecimal");
                return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetDecimal", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(double) || item.PropertyType == typeof(double?))
            {
                //sb.Append("reader.GetDouble");
                return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetDouble", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(float) || item.PropertyType == typeof(float?))
            {
                //sb.Append("reader.GetFloat");
                return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetFloat", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(Guid))
            {
                //sb.Append("GetGuid");
                return new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetGuid", new CodeExpression[] { new CodeArgumentReferenceExpression("reader"), new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(short) || item.PropertyType == typeof(short?))
            {
                //sb.Append("reader.GetInt16");
                return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetInt16", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(int) || item.PropertyType == typeof(int?))
            {
                //sb.Append("reader.GetInt32");
                return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetInt32", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(long) || item.PropertyType == typeof(long?))
            {
                //sb.Append("reader.GetInt64");
                return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetInt64", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
            }
            else if (item.PropertyType == typeof(string))
            {
                //sb.Append("reader.GetString");
                return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetString", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
            }
            else
            {
                if (GetPropertyAttribute<CompoundUnitAttribute>(item) != null)
                {
                    //sb.Append("reader.GetString");
                    return new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetString", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) });
                }
                else
                {
                    //sb.Append("(");
                    //sb.Append(GenType(item.PropertyType.ToString()));
                    //sb.Append(")reader.GetValue");
                    return new CodeCastExpression(GenType(item.PropertyType.ToString()), new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("reader"), "GetValue", new CodeExpression[] { new CodePrimitiveExpression(itemIndex) }));
                }
            }
        }

        private void GenPropertiesEx(CodeTypeDeclaration entity, CodeStatementCollection reloadQueryStatements, Type type, int outLang)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            PropertyInfo[] pis = type.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                list.Add(pi);
            }
            foreach (PropertyInfo pi in Util.DeepGetProperties(GetContractInterfaceTypes(type)))
            {
                list.Add(pi);
            }
            List<string> generatedPropertyNames = new List<string>();
            foreach (PropertyInfo item in list)
            {
                if (!generatedPropertyNames.Contains(item.Name))
                {
                    if (GetPropertyAttribute<CompoundUnitAttribute>(item) != null)
                    {
                        GenCompoundUnitPropertyEx(entity, item, outLang);
                    }
                    else if (GetPropertyQueryAttribute(item) != null)
                    {
                        //GenQueryProperty(sbFields, sbProperties, sbReloadQueries, item, type);
                        GenQueryPropertyEx(entity, reloadQueryStatements, item, type, outLang);
                    }
                    else
                    {
                        //GenNormalProperty(sbFields, sbProperties, item);
                        GenNormalPropertyEx(entity, item, outLang);
                    }

                    generatedPropertyNames.Add(item.Name);
                }
            }
        }

        private void GenQueryPropertyEx(CodeTypeDeclaration entity, CodeStatementCollection reloadQueryStatements, PropertyInfo item, Type type, int outLang)
        {
            QueryAttribute qa = GetPropertyQueryAttribute(item);
            QueryDescriber describer = new QueryDescriber(qa, item, item.PropertyType.IsArray ? Util.GetOriginalTypeOfArrayType(item.PropertyType) : item.PropertyType, type);

            //sbFields.Append("\t\tprotected ");
            string propertyType;
            if (item.PropertyType.ToString().EndsWith("[]"))
            {
                propertyType = GetOutputNamespace(Util.GetOriginalTypeOfArrayType(item.PropertyType)) + "." + RemoveTypePrefix(GenType(item.PropertyType.ToString().TrimEnd('[', ']'))) + "ArrayList";
            }
            else
            {
                propertyType = GetOutputNamespace(item.PropertyType) + "." + RemoveTypePrefix(GenType(item.PropertyType.ToString()));
            }

            CodeMemberField field = new CodeMemberField();
            field.Type = new CodeTypeReference(GenType(propertyType), CodeTypeReferenceOptions.GlobalReference);
            field.Name = "_" + item.Name;
            field.Attributes = MemberAttributes.Family;
            entity.Members.Add(field);

            if (qa != null && qa.QueryType == QueryType.FkReverseQuery)
            {
                Type fkType = describer.RelatedForeignKeyType;
                string typename = fkType.ToString();
                //sbFields.Append("\t\tprotected ");
                //sbFields.Append(GenType(fkType.ToString()));
                //typename = GenType(fkType.ToString());
                if (fkType.IsValueType)
                {
                    //sbFields.Append("?");
                    typename += "?";
                }
                //sbFields.Append(" _");
                //sbFields.Append(item.Name);
                //sbFields.Append("_");
                //sbFields.Append(describer._relatedForeignKey);
                //sbFields.Append(";\r\n");

                CodeMemberField fieldFk = new CodeMemberField();
                fieldFk.Type = new CodeTypeReference(GenType(typename), CodeTypeReferenceOptions.GlobalReference);
                fieldFk.Name = "_" + item.Name + "_" + describer.RelatedForeignKey;
                fieldFk.Attributes = MemberAttributes.Family;
                entity.Members.Add(fieldFk);

                CodeMemberProperty propertyFk = new CodeMemberProperty();
                propertyFk.Type = fieldFk.Type;
                //propertyFk.Name = item.Name + '_' + describer._relatedForeignKey;
                propertyFk.Name = item.Name + "ID";
                propertyFk.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                propertyFk.HasSet = true;
                propertyFk.HasGet = true;
                propertyFk.GetStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(fieldFk.Name)));
                //propertyFk.SetStatements.Add(new CodeVariableDeclarationStatement(
                //    new CodeTypeReference(propertyType, CodeTypeReferenceOptions.GlobalReference),
                //    "__old_" + item.Name,
                //    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), item.Name)
                //    ));
                propertyFk.SetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "OnPropertyChanged",
                    new CodeExpression[] {new CodePrimitiveExpression(item.Name), 
					    new CodeVariableReferenceExpression(fieldFk.Name), new CodePropertySetValueReferenceExpression()}));
                propertyFk.SetStatements.Add(new CodeAssignStatement(
                    new CodeVariableReferenceExpression(fieldFk.Name),
                    new CodeVariableReferenceExpression("value")
                    ));
                propertyFk.SetStatements.Add(new CodeMethodInvokeExpression(
                    new CodeThisReferenceExpression(),
                    "SetPropertyUnloaded",
                    new CodePrimitiveExpression(item.Name)));
                //propertyFk.SetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "OnQueryOnePropertyChanged",
                //    new CodeExpression[] {new CodePrimitiveExpression(item.Name), 
                //        new CodeVariableReferenceExpression("__old_" + item.Name), new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), item.Name)}));
                entity.Members.Add(propertyFk);
            }

            CodeMemberProperty property = new CodeMemberProperty();
            CommentAttribute ca = GetPropertyAttribute<CommentAttribute>(item);
            if (ca != null)
            {
                //sbProperties.Append("\t\t/// <summary>\r\n");
                //sbProperties.Append("\t\t/// ");
                //sbProperties.Append(ca.Content.Replace("\n", "\n\t\t/// "));
                //sbProperties.Append("\r\n\t\t/// </summary>\r\n");
                property.Comments.Add(new CodeCommentStatement("<summary>", true));
                property.Comments.Add(new CodeCommentStatement(ca.Content, true));
                property.Comments.Add(new CodeCommentStatement("</summary>", true));
            }
            if (GetPropertyAttribute<SerializationIgnoreAttribute>(item) != null)
            {
                property.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(XmlIgnoreAttribute))));
                //sbProperties.Append("\t\t[XmlIgnore]\r\n");
            }

            CustomAttribute[] customAttributes = GetEntityAttributes<CustomAttribute>(item);
            if (customAttributes != null)
            {
                foreach (CustomAttribute customAttribute in customAttributes)
                {
                    property.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(customAttribute.AttributeType)));
                }
            }

            property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            property.Type = new CodeTypeReference(propertyType, CodeTypeReferenceOptions.GlobalReference);
            property.Name = item.Name;
            property.HasGet = true;
            CodeConditionStatement condition = new CodeConditionStatement();
            condition.Condition = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "IsQueryPropertyLoaded",
                new CodeExpression[] { new CodePrimitiveExpression(item.Name) });
            GenReloadQueryEx(condition.FalseStatements, qa, item, type, outLang);
            property.GetStatements.Add(condition);
            //sbProperties.Append("\t\tpublic ");
            //sbProperties.Append(propertyType);
            //sbProperties.Append(" ");
            //sbProperties.Append(item.Name);
            //sbProperties.Append("\r\n\t\t{\r\n");
            //sbProperties.Append("\t\t\tget\r\n\t\t\t{\r\n\t\t\t\tif (!IsQueryPropertyLoaded(\"");
            //sbProperties.Append(item.Name);
            //sbProperties.Append("\")) ");
            //GenReloadQuery(sbProperties, qa, item);
            if (item.PropertyType.IsArray)
            {
                condition = new CodeConditionStatement();
                condition.Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name),
                    CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
                condition.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(propertyType, CodeTypeReferenceOptions.GlobalReference), "_a1"));
                condition.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("_a1"), new CodeObjectCreateExpression(new CodeTypeReference(propertyType, CodeTypeReferenceOptions.GlobalReference), new CodeExpression[] { })));
                condition.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                    "BindArrayListEventHandlers", new CodeExpression[] { new CodePrimitiveExpression(item.Name), new CodeVariableReferenceExpression("_a1") }));
                condition.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name), new CodeVariableReferenceExpression("_a1")));
                property.GetStatements.Add(condition);

                //sbProperties.Append("\t\t\t\tif (_");
                //sbProperties.Append(item.Name);
                //sbProperties.Append(" == null) { ");
                //sbProperties.Append(propertyType);
                //sbProperties.Append(" _al = new ");
                //sbProperties.Append(propertyType);
                //sbProperties.Append("(); BindArrayListEventHandlers(\"");
                //sbProperties.Append(item.Name);
                //sbProperties.Append("\", _al); _");
                //sbProperties.Append(item.Name);
                //sbProperties.Append(" = _al; }\r\n");
            }
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name)));

            //sbProperties.Append("\t\t\t\treturn _");
            //sbProperties.Append(item.Name);
            //sbProperties.Append(";\r\n\t\t\t}\r\n");

            //reloadQueryStatements.Add();
            condition = new CodeConditionStatement();
            condition.Condition = new CodeBinaryOperatorExpression(new CodeArgumentReferenceExpression("includeLazyLoadQueries"),
                CodeBinaryOperatorType.BooleanOr,
                new CodeBinaryOperatorExpression(new CodePrimitiveExpression(false), CodeBinaryOperatorType.ValueEquality,
                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(MetaDataManager)), "_isLazyLoad", new CodeExpression[] { 
				new CodePrimitiveExpression(GetOutputNamespace(type)+"."+type.Name),new CodePrimitiveExpression(item.Name)})));
            GenReloadQueryEx(condition.TrueStatements, qa, item, type, outLang);
            reloadQueryStatements.Add(condition);

            //sbReloadQuery.Append("\t\t\t");
            //sbReloadQuery.Append("if (includeLazyLoadQueries || (!MetaDataManager._isLazyLoad(\"");
            //sbReloadQuery.Append(GetOutputNamespace(type) + "." + type.Name);
            //sbReloadQuery.Append("\", \"");
            //sbReloadQuery.Append(item.Name);
            //sbReloadQuery.Append("\"))) ");
            //GenReloadQuery(sbReloadQuery, qa, item);

            if (item.CanWrite)
            {
                property.HasSet = true;
                if (item.PropertyType.IsArray)
                {
                    property.SetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "OnQueryPropertyChanged", new CodeExpression[] { new CodePrimitiveExpression(item.Name), new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), item.Name), new CodePropertySetValueReferenceExpression() }));
                }
                else
                {
                    property.SetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "OnQueryOnePropertyChanged", new CodeExpression[] { new CodePrimitiveExpression(item.Name), new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), item.Name), new CodePropertySetValueReferenceExpression() }));
                }
                property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name),
                    new CodePropertySetValueReferenceExpression()));


                //if (item.PropertyType.IsArray)
                //{
                //  sbProperties.Append("\t\t\tset { OnQueryPropertyChanged(\"");
                //}
                //else
                //{
                //  sbProperties.Append("\t\t\tset { OnQueryOnePropertyChanged(\"");
                //}
                //sbProperties.Append(item.Name);
                //sbProperties.Append("\", ");
                //sbProperties.Append(item.Name);
                //sbProperties.Append(", value); _");
                //sbProperties.Append(item.Name);
                //sbProperties.Append(" = value; ");

                if (qa != null && qa.QueryType == QueryType.FkReverseQuery && (!item.PropertyType.IsArray))
                {
                    Type fkType = describer.RelatedForeignKeyType;
                    condition = new CodeConditionStatement();
                    condition.Condition = new CodeBinaryOperatorExpression(new CodePropertySetValueReferenceExpression(), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
                    condition.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                        "OnPropertyChanged", new CodeExpression[] { new CodePrimitiveExpression(item.Name), new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name + "_" + describer.RelatedForeignKey), new CodePrimitiveExpression(null) }));
                    condition.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name + "_" + describer.RelatedForeignKey), new CodePrimitiveExpression(null)));
                    condition.FalseStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                        "OnPropertyChanged", new CodeExpression[] { new CodePrimitiveExpression(item.Name), new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name + "_" + describer.RelatedForeignKey), new CodePropertyReferenceExpression(new CodePropertySetValueReferenceExpression(), describer.RelatedForeignKey) }));
                    condition.FalseStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name + "_" + describer.RelatedForeignKey), new CodePropertyReferenceExpression(new CodePropertySetValueReferenceExpression(), describer.RelatedForeignKey)));
                    property.SetStatements.Add(condition);
                    //sbProperties.Append("if (value == null) { ");

                    //sbProperties.Append("OnPropertyChanged(\"");
                    //sbProperties.Append(item.Name);
                    //sbProperties.Append("\", _");
                    //sbProperties.Append(item.Name);
                    //sbProperties.Append("_");
                    //sbProperties.Append(describer._relatedForeignKey);
                    //sbProperties.Append(", null); _");

                    //sbProperties.Append(item.Name);
                    //sbProperties.Append("_");
                    //sbProperties.Append(describer._relatedForeignKey);
                    //sbProperties.Append(" = null; } else {");

                    //sbProperties.Append("OnPropertyChanged(\"");
                    //sbProperties.Append(item.Name);
                    //sbProperties.Append("\", _");
                    //sbProperties.Append(item.Name);
                    //sbProperties.Append("_");
                    //sbProperties.Append(describer._relatedForeignKey);
                    //sbProperties.Append(", value.");
                    //sbProperties.Append(describer._relatedForeignKey);
                    //sbProperties.Append("); _");

                    //sbProperties.Append(item.Name);
                    //sbProperties.Append("_");
                    //sbProperties.Append(describer._relatedForeignKey);
                    //sbProperties.Append(" = value.");
                    //sbProperties.Append(describer._relatedForeignKey);
                    //sbProperties.Append("; } ");
                }
                //sbProperties.Append("}\r\n");
            }
            //sbProperties.Append("\t\t}\r\n\r\n");
            entity.Members.Add(property);
        }

        private void GenReloadQueryEx(CodeStatementCollection codeExpresses, QueryAttribute qa, PropertyInfo item, Type type, int outLang)
        {
            QueryDescriber describer = new QueryDescriber(qa, item, item.PropertyType.IsArray ? Util.GetOriginalTypeOfArrayType(item.PropertyType) : item.PropertyType, type);

            string propertyType;
            if (item.PropertyType.ToString().EndsWith("[]"))
            {
                propertyType = GetOutputNamespace(Util.GetOriginalTypeOfArrayType(item.PropertyType)) + "." + RemoveTypePrefix(GenType(item.PropertyType.ToString().TrimEnd('[', ']'))) + "ArrayList";
            }
            else
            {
                propertyType = GetOutputNamespace(item.PropertyType) + "." + RemoveTypePrefix(GenType(item.PropertyType.ToString()));
            }
            CodeTypeReference propertyTypeRef = new CodeTypeReference(propertyType, CodeTypeReferenceOptions.GlobalReference);
            //if (outLang == 0)
            //{
            //    propertyTypeArrayRef = new CodeTypeReference(propertyType.Substring(0, propertyType.Length - "ArrayList".Length)+"[]",
            //        CodeTypeReferenceOptions.GlobalReference);
            //}
            //else
            //{
            //    propertyTypeArrayRef = new CodeTypeReference(propertyType.Substring(0, propertyType.Length - "ArrayList".Length)+"()",
            //        CodeTypeReferenceOptions.GlobalReference);
            //}

            //sb.Append("{ ");

            if (item.PropertyType.IsArray)
            {
                string orgPropertyType = propertyType.Substring(0, propertyType.IndexOf("ArrayList"));
                propertyTypeRef = new CodeTypeReference(orgPropertyType, CodeTypeReferenceOptions.GlobalReference);
                CodeTypeReference propertyTypeArrayRef = new CodeTypeReference(propertyTypeRef, 1);

                codeExpresses.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(propertyType, CodeTypeReferenceOptions.GlobalReference), "_a1"));
                codeExpresses.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("_a1"), new CodeObjectCreateExpression(propertyType, new CodeExpression[] { })));
                codeExpresses.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("_a1"),
                    "AddRange", new CodeExpression[] { new CodeCastExpression(propertyTypeArrayRef,
						new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
						"Query",new CodeExpression[]{new CodeTypeOfExpression(propertyTypeArrayRef.ArrayElementType),new CodePrimitiveExpression(item.Name),new CodeThisReferenceExpression()}))}));

                //sb.Append(propertyType);
                //sb.Append(" _al = new ");
                //sb.Append(propertyType);
                //sb.Append(" (); _al.AddRange((");
                //sb.Append(propertyType.Substring(0, propertyType.Length - "ArrayList".Length));
                //sb.Append("[])");
                //sb.Append("Query(");
                //sb.Append(string.Format("typeof({0}), \"{1}\", this)", propertyType.Substring(0, propertyType.Length - "ArrayList".Length), item.Name));

                codeExpresses.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "OnQueryPropertyChanged",
                    new CodeExpression[] { 
						new CodePrimitiveExpression(item.Name),
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),"_"+item.Name),
						new CodeVariableReferenceExpression("_a1")}));
                codeExpresses.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name),
                    new CodeVariableReferenceExpression("_a1")));

            }
            else
            {
                codeExpresses.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(propertyType, CodeTypeReferenceOptions.GlobalReference), "_obj"));
                codeExpresses.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("_obj"),
                    new CodeCastExpression(new CodeTypeReference(propertyType, CodeTypeReferenceOptions.GlobalReference),
                    new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "QueryOne",
                    new CodeExpression[] { new CodeTypeOfExpression(propertyTypeRef), new CodePrimitiveExpression(item.Name), new CodeThisReferenceExpression() }))));

                codeExpresses.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "OnQueryOnePropertyChanged",
                    new CodeExpression[] { 
						new CodePrimitiveExpression(item.Name),
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),"_"+item.Name),
						new CodeVariableReferenceExpression("_obj")}));
                codeExpresses.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name),
                    new CodeVariableReferenceExpression("_obj")));

                //sb.Append("_");
                //sb.Append(item.Name);
                //sb.Append(" = (");
                //sb.Append(propertyType);
                //sb.Append(")");
                //sb.Append("QueryOne(");
                //sb.Append(string.Format("typeof({0}), \"{1}\", this)", propertyType, item.Name));
            }

            //if (item.PropertyType.IsArray)
            //{
            //  sb.Append("); ");
            //  sb.Append("OnQueryPropertyChanged(\"");
            //  sb.Append(item.Name);
            //  sb.Append("\", _");
            //  sb.Append(item.Name);
            //  sb.Append(", _al); _");
            //  sb.Append(item.Name);
            //  sb.Append(" = _al;");
            //}
            //else
            //{
            //  sb.Append(";");
            //}
            //sb.Append(" }\r\n");
        }


        private void GenNormalPropertyEx(CodeTypeDeclaration entity, PropertyInfo item, int outLang)
        {
            CodeMemberField field = new CodeMemberField();
            field.Name = "_" + item.Name;
            field.Attributes = MemberAttributes.Family;
            field.Type = new CodeTypeReference(GenType(item.PropertyType.ToString()), CodeTypeReferenceOptions.GlobalReference);
            entity.Members.Add(field);

            //sbFields.Append("\t\tprotected ");
            //sbFields.Append(GenType(item.PropertyType.ToString()));
            //sbFields.Append(" _");
            //sbFields.Append(item.Name);
            //sbFields.Append(";\r\n");

            CodeMemberProperty property = new CodeMemberProperty();

            if (GetPropertyAttribute<SerializationIgnoreAttribute>(item) != null)
            {
                property.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(XmlIgnoreAttribute))));
                //sbProperties.Append("\t\t[XmlIgnore]\r\n");
            }
            CustomAttribute[] customAttributes = GetEntityAttributes<CustomAttribute>(item);
            if (customAttributes != null)
            {
                foreach (CustomAttribute customAttribute in customAttributes)
                {
                    property.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(customAttribute.AttributeType)));
                }
            }

            property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            property.Type = new CodeTypeReference(GenType(item.PropertyType.ToString()), CodeTypeReferenceOptions.GlobalReference);
            property.Name = item.Name;
            CommentAttribute ca = GetPropertyAttribute<CommentAttribute>(item);
            if (ca != null)
            {
                //sbProperties.Append("\t\t/// <summary>\r\n");
                //sbProperties.Append("\t\t/// ");
                //sbProperties.Append(ca.Content.Replace("\n", "\n\t\t/// "));
                //sbProperties.Append("\r\n\t\t/// </summary>\r\n");
                property.Comments.Add(new CodeCommentStatement("<summary>", true));
                property.Comments.Add(new CodeCommentStatement(ca.Content, true));
                property.Comments.Add(new CodeCommentStatement("</summary>", true));
            }
            //sbProperties.Append("\t\tpublic ");
            //sbProperties.Append(GenType(item.PropertyType.ToString()));
            //sbProperties.Append(" ");
            //sbProperties.Append(item.Name);
            //sbProperties.Append("\r\n\t\t{\r\n");
            if (item.CanRead)
            {
                property.HasGet = true;
                property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name)));

                //sbProperties.Append("\t\t\tget { return _");
                //sbProperties.Append(item.Name);
                //sbProperties.Append("; }\r\n");
            }
            //if (item.CanWrite)
            //{

            property.HasSet = true;
            property.SetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "OnPropertyChanged",
                new CodeExpression[] {new CodePrimitiveExpression(item.Name), 
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),"_"+item.Name),new CodePropertySetValueReferenceExpression()}));
            property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + item.Name), new CodePropertySetValueReferenceExpression()));

            //sbProperties.Append("\t\t\tset { OnPropertyChanged(\"");
            //sbProperties.Append(item.Name);
            //sbProperties.Append("\", _");
            //sbProperties.Append(item.Name);
            //sbProperties.Append(", value); _");
            //sbProperties.Append(item.Name);
            //sbProperties.Append(" = value; }\r\n");


            //}

            entity.Members.Add(property);

            //sbProperties.Append("\t\t}\r\n\r\n");
        }

        private void GenCompoundUnitPropertyEx(CodeTypeDeclaration entity, PropertyInfo item, int outLang)
        {
            CodeMemberField field = new CodeMemberField();
            field.Name = "_" + item.Name;
            field.Attributes = MemberAttributes.Family;
            field.Type = new CodeTypeReference(typeof(string));
            entity.Members.Add(field);

            CodeMemberProperty property = new CodeMemberProperty();
            property.Name = item.Name;
            property.Type = new CodeTypeReference(GenType(item.PropertyType.ToString()), CodeTypeReferenceOptions.GlobalReference);
            property.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            CommentAttribute ca = GetPropertyAttribute<CommentAttribute>(item);
            if (ca != null)
            {
                //sbProperties.Append("\t\t/// <summary>\r\n");
                //sbProperties.Append("\t\t/// ");
                //sbProperties.Append(ca.Content.Replace("\n", "\n\t\t/// "));
                //sbProperties.Append("\r\n\t\t/// </summary>\r\n");
                property.Comments.Add(new CodeCommentStatement("<summary>", true));
                property.Comments.Add(new CodeCommentStatement(ca.Content, true));
                property.Comments.Add(new CodeCommentStatement("</summary>", true));
            }




            if (GetPropertyAttribute<SerializationIgnoreAttribute>(item) != null)
            {
                property.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(XmlIgnoreAttribute))));
            }

            CustomAttribute[] customAttributes = GetEntityAttributes<CustomAttribute>(item);
            if (customAttributes != null)
            {
                foreach (CustomAttribute customAttribute in customAttributes)
                {
                    property.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(customAttribute.AttributeType)));
                }
            }

            if (item.CanRead)
            {
                property.HasGet = true;
                property.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(GenType(item.PropertyType.ToString()).TrimEnd('?'),
                    new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(SerializationManager)), "Deserialize", new CodeExpression[] { 
						new CodeTypeOfExpression(GenType(item.PropertyType.ToString()).TrimEnd('?')),new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),field.Name)}))));
            }
            //if (item.CanWrite)
            //{
            property.HasSet = true;
            property.SetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "OnPropertyChanged", new CodeExpression[] { new CodePrimitiveExpression(item.Name), new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name), new CodePropertySetValueReferenceExpression() }));
            property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name),
                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(SerializationManager)), "Serialize", new CodeExpression[] { new CodePropertySetValueReferenceExpression() })));
            entity.Members.Add(property);
        }

        #endregion

        #region Gen Entity Configs

        public string GenEntityConfigurations(Assembly ass)
        {
            List<EntityConfiguration> configs = DoGenEntityConfigurations(ass);
            string retStr = SerializationManager.Serialize(configs.ToArray());
            retStr = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + retStr.TrimStart().Substring("<?xml version=\"1.0\" encoding=\"utf-16\"?>".Length);

            return retStr;
        }

        private List<EntityConfiguration> DoGenEntityConfigurations(Assembly ass)
        {
            List<EntityConfiguration> configs = new List<EntityConfiguration>();

            foreach (Type type in ass.GetTypes())
            {
                if (typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(type) && typeof(Rock.Orm.Common.Design.Entity) != type)
                {
                    EntityConfiguration ec = new EntityConfiguration();
                    ec.Name = GetOutputNamespace(type) + "." + type.Name;
                    if (GetEntityAttribute<MappingNameAttribute>(type) != null)
                    {
                        ec.MappingName = GetEntityAttribute<MappingNameAttribute>(type).Name;
                    }
                    if (GetEntityAttribute<CommentAttribute>(type) != null)
                    {
                        ec.Comment = GetEntityAttribute<CommentAttribute>(type).Content;
                    }
                    if (GetEntityAttribute<CustomDataAttribute>(type) != null)
                    {
                        ec.CustomData = GetEntityAttribute<CustomDataAttribute>(type).Data;
                    }
                    if (GetEntityAttribute<ReadOnlyAttribute>(type) != null)
                    {
                        ec.IsReadOnly = true;
                    }
                    if (GetEntityAttribute<AutoPreLoadAttribute>(type) != null)
                    {
                        ec.IsAutoPreLoad = true;
                    }
                    if (GetEntityAttribute<RelationAttribute>(type) != null)
                    {
                        ec.IsRelation = true;
                    }
                    BatchUpdateAttribute bsla = GetEntityAttribute<BatchUpdateAttribute>(type);
                    if (bsla != null)
                    {
                        ec.IsBatchUpdate = true;
                        ec.BatchSize = bsla.BatchSize;
                    }
                    AdditionalSqlScriptAttribute[] addSqls = GetEntityAttributes<AdditionalSqlScriptAttribute>(type);
                    if (addSqls != null && addSqls.Length > 0)
                    {
                        ec.AdditionalSqlScript = string.Empty;

                        foreach (AdditionalSqlScriptAttribute addSql in addSqls)
                        {
                            if (!string.IsNullOrEmpty(addSql.PreCleanSql))
                            {
                                ec.AdditionalSqlScript += addSql.PreCleanSql + "\n";
                            }
                            ec.AdditionalSqlScript += addSql.Sql + "\n\n";
                        }
                    }

                    Type[] interfaces = type.GetInterfaces();
                    foreach (Type item in interfaces)
                    {
                        if (typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(item) && (typeof(Rock.Orm.Common.Design.Entity) != item))
                        {
                            ec.BaseEntity = GetOutputNamespace(item) + "." + item.Name;
                            break;
                        }
                    }

                    List<string> generatedProperties = new List<string>();
                    GenPropertyConfig(ec, type, type, generatedProperties, false, null);

                    configs.Add(ec);
                }
            }

            return configs;
        }

        private void GenPropertyConfig(EntityConfiguration ec, Type type, Type entityType, List<string> generatedProperties, bool isInherited, string inheritEntityMappingName)
        {
            foreach (Type item in type.GetInterfaces())
            {
                if (typeof(Rock.Orm.Common.Design.Entity) != item)
                {
                    bool isContractType = false;
                    foreach (Type contractType in GetContractInterfaceTypes(entityType))
                    {
                        if (item == contractType)
                        {
                            isContractType = true;
                            break;
                        }
                    }
                    MappingNameAttribute mnaOfItem = GetEntityAttribute<MappingNameAttribute>(item);
                    MappingNameAttribute mnaOfType = GetEntityAttribute<MappingNameAttribute>(type);
                    string inherMappingName = isContractType ? (mnaOfType == null ? type.Name : mnaOfType.Name) : (mnaOfItem == null ? item.Name : mnaOfItem.Name);
                    if ((!typeof(Rock.Orm.Common.Design.Entity).IsAssignableFrom(item)) && inherMappingName == item.Name)
                    {
                        inherMappingName = inheritEntityMappingName;
                    }
                    GenPropertyConfig(ec, item, entityType, generatedProperties, (!isContractType), inherMappingName);
                }
            }

            List<PropertyInfo> list = new List<PropertyInfo>();
            PropertyInfo[] pis = type.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                list.Add(pi);
            }
            foreach (PropertyInfo pi in Util.DeepGetProperties(GetContractInterfaceTypes(type)))
            {
                list.Add(pi);
            }

            foreach (PropertyInfo item in list)
            {
                if (!generatedProperties.Contains(item.Name))
                {
                    PropertyConfiguration pc = new PropertyConfiguration();
                    //pc.IsCompoundUnit = (GetPropertyAttribute<CompoundUnitAttribute>(item) != null);
                    pc.IsPrimaryKey = (GetPropertyAttribute<PrimaryKeyAttribute>(item) != null);
                    IndexPropertyAttribute ipa = GetPropertyAttribute<IndexPropertyAttribute>(item);
                    if (ipa != null)
                    {
                        pc.IsIndexProperty = true;
                        pc.IsIndexPropertyDesc = ipa.IsDesc;
                    }
                    pc.IsNotNull = (GetPropertyAttribute<NotNullAttribute>(item) != null);
                    pc.IsSerializationIgnore = (GetPropertyAttribute<SerializationIgnoreAttribute>(item) != null);
                    RelationKeyAttribute rka = GetPropertyAttribute<RelationKeyAttribute>(item);
                    if (rka != null)
                    {
                        pc.IsRelationKey = true;
                        pc.RelatedType = GetOutputNamespace(rka.RelatedType) + "." + rka.RelatedType.Name;
                        pc.RelatedForeignKey = rka.RelatedPk;
                    }
                    FriendKeyAttribute fka = GetPropertyAttribute<FriendKeyAttribute>(item);
                    if (fka != null)
                    {
                        pc.IsFriendKey = true;
                        pc.RelatedForeignKey = fka.RelatedEntityPk;
                        pc.RelatedType = GetOutputNamespace(fka.RelatedEntityType) + "." + fka.RelatedEntityType.Name;
                    }
                    MappingNameAttribute mna = GetPropertyAttribute<MappingNameAttribute>(item);
                    if (mna != null)
                    {
                        pc.MappingName = mna.Name;
                    }
                    CommentAttribute ca = GetPropertyAttribute<CommentAttribute>(item);
                    if (ca != null)
                    {
                        pc.Comment = ca.Content;
                    }
                    if (GetPropertyAttribute<CustomDataAttribute>(item) != null)
                    {
                        pc.CustomData = GetPropertyAttribute<CustomDataAttribute>(item).Data;
                    }
                    pc.IsReadOnly = (!item.CanWrite);
                    pc.Name = item.Name;
                    SqlTypeAttribute sta = GetPropertyAttribute<SqlTypeAttribute>(item);
                    if (sta != null && (!string.IsNullOrEmpty(sta.Type)))
                    {
                        pc.SqlType = sta.Type;
                        pc.SqlDefaultValue = sta.DefaultValue;
                    }

                    QueryAttribute qa = GetPropertyQueryAttribute(item);

                    if (qa != null)
                    {
                        pc.IsQueryProperty = true;
                        pc.QueryType = qa.QueryType.ToString();

                        Type propertyEntityType = item.PropertyType;
                        if (propertyEntityType.IsArray)
                        {
                            propertyEntityType = Util.GetOriginalTypeOfArrayType(propertyEntityType);
                        }
                        QueryDescriber describer = new QueryDescriber(qa, item, propertyEntityType, entityType);

                        pc.IsLazyLoad = describer.LazyLoad;
                        pc.QueryOrderBy = describer.OrderBy;
                        pc.QueryWhere = describer.Where;
                        if (describer.RelationType != null)
                        {
                            pc.RelationType = GetOutputNamespace(describer.RelationType) + "." + describer.RelationType.Name;
                        }
                        pc.IsContained = describer.Contained;
                        pc.RelatedForeignKey = describer.RelatedForeignKey;
                        if (describer.RelatedType != null)
                        {
                            pc.RelatedType = GetOutputNamespace(describer.RelatedType) + "." + describer.RelatedType.Name;
                        }

                        if (item.PropertyType.IsArray)
                        {
                            pc.PropertyType = GetOutputNamespace(item.PropertyType.GetElementType()) + "." + item.PropertyType.Name.TrimEnd('[', ']') + "ArrayList";
                        }
                        else
                        {
                            pc.PropertyType = GetOutputNamespace(item.PropertyType) + "." + item.PropertyType.Name;
                        }


                        if (qa != null && qa.QueryType == QueryType.FkReverseQuery)
                        {
                            if (describer.RelatedForeignKeyType != null && describer.RelatedForeignKeyType.IsValueType)
                            {
                                pc.PropertyMappingColumnType = (string.Format("System.Nullable`1[{0}]", describer.RelatedForeignKeyType) ?? typeof(string).ToString());
                            }
                            else
                            {
                                pc.PropertyMappingColumnType = (describer.RelatedForeignKeyType ?? typeof(string)).ToString();
                            }
                        }

                        if (qa.QueryType == QueryType.FkQuery)
                        {
                            pc.RelatedForeignKey = describer.RelatedForeignKey;
                            pc.RelatedType = GetOutputNamespace(describer.RelatedType) + "." + describer.RelatedType.Name;
                        }
                    }
                    else
                    {
                        pc.PropertyType = item.PropertyType.ToString();
                    }

                    pc.IsInherited = isInherited;
                    pc.InheritEntityMappingName = inheritEntityMappingName;

                    ec.Add(pc);

                    generatedProperties.Add(item.Name);
                }
            }
        }

        #endregion

        #region Gen Db Script

        public string GenDbScript(Assembly ass)
        {
            StringBuilder sb = new StringBuilder();
            List<string> tables = new List<string>();
            List<KeyValuePair<string, string>> fkRelations = new List<KeyValuePair<string, string>>();
            List<PropertyConfiguration> fkRelationKeys = new List<PropertyConfiguration>();
            List<EntityConfiguration> configs = DoGenEntityConfigurations(ass);

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
                    if (ec != null)
                    {
                        relatedKeyMappingName = ec.GetPropertyConfiguration(pc.RelatedForeignKey).MappingName;
                    }
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
