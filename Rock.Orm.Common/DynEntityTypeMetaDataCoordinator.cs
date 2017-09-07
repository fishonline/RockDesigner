using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rock.Orm.Common.Design;

namespace Rock.Orm.Common
{
    public class DynEntityTypeMetaDataCoordinator
    {
        /// <summary>
        /// flush Entities in EntityTypeManager into MetaDataManager
        /// </summary>
        public static void EntityTypes2EntityConfiguration()
        {
            List<EntityConfiguration> ecs = new List<EntityConfiguration>();
            foreach (DynEntityType et in DynEntityTypeManager.Entities)
            {
                if (!MetaDataManager.isExistEntityConfiguration(et.Name))
                {
                    ecs.Add(EntityType2EntityConfiguration(et));
                }
            }

            MetaDataManager.AddEntityConfigurations(ecs.ToArray());
        }

        /// <summary>
        /// Flush all EntityConfigurations in MetaDataManager into EntityTypeManager
        /// </summary>
        public static void EntityConfiguration2EntityTypes()
        {
            foreach (EntityConfiguration ec in MetaDataManager.Entities)
            {
                string nameSpace, typeName;
                Util.SplitFullName(ec.Name, out nameSpace, out typeName);

                if (DynEntityTypeManager.GetEntityType(typeName) == null)
                {
                    DynEntityTypeManager.Entities.Add(EntityConfiguration2EntityType(ec));
                }
            }
        }

        /// <summary>
        /// new an EntityType from an EntityConfiguration
        /// </summary>
        /// <param name="ec"></param>
        /// <returns></returns>
        public static DynEntityType EntityConfiguration2EntityType(EntityConfiguration ec)
        {
            string nameSpace, typeName, baseName;
            Util.SplitFullName(ec.Name, out nameSpace, out typeName);

            DynEntityType et;

            if (string.IsNullOrEmpty(ec.BaseEntity))
            {
                et = new DynEntityType(typeName);
            }
            else
            {
                Util.SplitFullName(ec.BaseEntity, out nameSpace, out baseName);
                et = new DynEntityType(typeName, baseName);
            }
            et.AdditionalSqlScript = ec.AdditionalSqlScript;
            et.BaseEntityName = ec.BaseEntity;
            et.BatchSize = ec.BatchSize;
            et.Comment = ec.Comment;
            et.CustomData = ec.CustomData;
            et.IsAutoPreLoad = ec.IsAutoPreLoad;
            et.IsBatchUpdate = ec.IsBatchUpdate;
            et.IsReadOnly = ec.IsReadOnly;
            et.IsRelation = ec.IsRelation;
            et.MappingName = ec.MappingName;
            et.Namespace = nameSpace;
            et.Name = typeName;
            et.ViewName = ec.ViewName;


            if (string.IsNullOrEmpty(nameSpace) == false)
                et.Attributes.Add(new OutputNamespaceDynAttribute(nameSpace));

            if (string.IsNullOrEmpty(ec.MappingName) == false | string.IsNullOrEmpty(ec.ViewName) == false)
                et.Attributes.Add(new MappingNameDynAttribute(ec.MappingName));

            if (string.IsNullOrEmpty(ec.Comment) == false)
                et.Attributes.Add(new CommentDynAttribute(ec.Comment));

            if (string.IsNullOrEmpty(ec.CustomData) == false)
                et.Attributes.Add(new CustomDataDynAttribute(ec.CustomData));

            if (string.IsNullOrEmpty(ec.AdditionalSqlScript) == false)
                et.Attributes.Add(new AdditionalSqlScriptDynAttribute(ec.AdditionalSqlScript));

            if (ec.IsReadOnly)
                et.Attributes.Add(new ReadOnlyDynAttribute());

            if (ec.IsAutoPreLoad)
                et.Attributes.Add(new AutoPreLoadDynAttribute());

            if (ec.IsBatchUpdate)
                et.Attributes.Add(new BatchUpdateDynAttribute(ec.BatchSize));

            if (ec.IsRelation)
                et.Attributes.Add(new RelationDynAttribute());

            foreach (PropertyConfiguration pc in ec.Properties)
            {
                if (pc.IsInherited)
                    continue;

                DynPropertyConfiguration property = new DynPropertyConfiguration(pc.Name);
                property.PropertyType = pc.PropertyType;
                property.IsInherited = pc.IsInherited;
                property.IsQueryProperty = pc.IsQueryProperty;
                property.IsReadOnly = pc.IsReadOnly;
                property.InheritEntityMappingName = pc.InheritEntityMappingName;
                //property.IsCompoundUnit = pc.IsCompoundUnit;
                property.Comment = pc.Comment;
                property.CustomData = pc.CustomData;
                property.IsContained = pc.IsContained;
                property.IsFriendKey = pc.IsFriendKey;
                property.IsIndexProperty = pc.IsIndexProperty;
                property.IsIndexPropertyDesc = pc.IsIndexPropertyDesc;
                property.IsInherited = pc.IsInherited;
                property.IsLazyLoad = pc.IsLazyLoad;
                property.IsNotNull = pc.IsNotNull;
                property.IsPrimaryKey = pc.IsPrimaryKey;
                property.IsQueryProperty = pc.IsQueryProperty;
                property.IsReadOnly = pc.IsReadOnly;
                property.IsRelationKey = pc.IsRelationKey;
                property.IsSerializationIgnore = pc.IsSerializationIgnore;
                property.MappingName = pc.MappingName;
                property.Name = pc.Name;
                property.PropertyMappingColumnType = pc.PropertyMappingColumnType;
                property.PropertyType = pc.PropertyType;
                property.QueryOrderBy = pc.QueryOrderBy;
                property.QueryType = pc.QueryType;
                property.QueryWhere = pc.QueryWhere;
                property.RelatedForeignKey = pc.RelatedForeignKey;
                property.RelatedType = pc.RelatedType;
                property.RelationType = pc.RelationType;
                property.SqlDefaultValue = pc.SqlDefaultValue;
                property.SqlType = pc.SqlType;

                if (pc.IsFriendKey)
                    property.Attributes.Add(new FriendKeyDynAttribute(pc.RelatedType));

                if (string.IsNullOrEmpty(pc.SqlType) == false)
                    property.Attributes.Add(new SqlTypeDynAttribute(pc.SqlType));

                if (pc.IsReadOnly)
                    property.Attributes.Add(new ReadOnlyDynAttribute());

                //if (pc.IsCompoundUnit)
                //    property.Attributes.Add(new CompoundUnitDynAttribute());

                if (pc.IsPrimaryKey)
                    property.Attributes.Add(new PrimaryKeyDynAttribute());

                if (pc.IsRelationKey)
                    property.Attributes.Add(new RelationKeyDynAttribute(pc.RelatedForeignKey));

                if (pc.IsIndexPropertyDesc)
                    property.Attributes.Add(new PrimaryKeyDynAttribute());

                if (pc.IsNotNull)
                    property.Attributes.Add(new NotNullDynAttribute());

                if (pc.IsSerializationIgnore)
                    property.Attributes.Add(new SerializationIgnoreDynAttribute());

                if (string.IsNullOrEmpty(pc.MappingName) == false)
                    property.Attributes.Add(new MappingNameDynAttribute(pc.MappingName));

                if (string.IsNullOrEmpty(pc.Comment) == false)
                    property.Attributes.Add(new CommentDynAttribute(pc.Comment));

                if (string.IsNullOrEmpty(pc.CustomData) == false)
                    property.Attributes.Add(new CustomDataDynAttribute(pc.CustomData));

                if (pc.IsQueryProperty)
                {
                    QueryType queryType = (QueryType)Enum.Parse(typeof(QueryType), pc.QueryType);

                    switch (queryType)
                    {
                        case QueryType.CustomQuery:
                            CustomQueryDynAttribute customQueryDynAttribute = new CustomQueryDynAttribute(pc.QueryWhere);
                            customQueryDynAttribute.OrderBy = pc.QueryOrderBy;
                            customQueryDynAttribute.RelationType = pc.RelationType;
                            customQueryDynAttribute.LazyLoad = pc.IsLazyLoad;
                            property.Attributes.Add(customQueryDynAttribute);
                            break;

                        case QueryType.FkQuery:
                            FkQueryDynAttribute fkQueryDynAttribute = new FkQueryDynAttribute(pc.RelatedForeignKey); //zml ???
                            fkQueryDynAttribute.AdditionalWhere = pc.QueryWhere;
                            fkQueryDynAttribute.LazyLoad = pc.IsLazyLoad;
                            fkQueryDynAttribute.OrderBy = pc.QueryOrderBy;
                            property.Attributes.Add(fkQueryDynAttribute);

                            property.IsArray = true;
                            break;

                        case QueryType.FkReverseQuery:
                            FkReverseQueryDynAttribute fkReverseQueryDynAttribute = new FkReverseQueryDynAttribute(true);
                            fkReverseQueryDynAttribute.LazyLoad = pc.IsLazyLoad;
                            property.Attributes.Add(fkReverseQueryDynAttribute);
                            break;

                        case QueryType.ManyToManyQuery:
                            ManyToManyQueryDynAttribute manyToManyQueryDynAttribute = new ManyToManyQueryDynAttribute(pc.RelationType);
                            manyToManyQueryDynAttribute.AdditionalWhere = pc.QueryWhere;
                            manyToManyQueryDynAttribute.LazyLoad = pc.IsLazyLoad;
                            manyToManyQueryDynAttribute.OrderBy = pc.QueryOrderBy;
                            property.Attributes.Add(manyToManyQueryDynAttribute);

                            property.IsArray = true;
                            break;

                        case QueryType.PkQuery:
                            PkQueryDynAttribute pkQueryDynAttribute = new PkQueryDynAttribute();
                            pkQueryDynAttribute.LazyLoad = pc.IsLazyLoad;
                            property.Attributes.Add(pkQueryDynAttribute);
                            break;

                        case QueryType.PkReverseQuery:
                            PkReverseQueryDynAttribute pkReverseQueryDynAttribute = new PkReverseQueryDynAttribute(true);
                            pkReverseQueryDynAttribute.LazyLoad = pc.IsLazyLoad;
                            property.Attributes.Add(pkReverseQueryDynAttribute);
                            break;
                    }
                }

                if (pc.IsIndexProperty)
                {
                    IndexPropertyDynAttribute indexPropertyDynAttribute = new IndexPropertyDynAttribute(pc.IsIndexPropertyDesc);
                    property.Attributes.Add(indexPropertyDynAttribute);
                }

                if (property.IsQueryProperty)
                {
                    if (property.IsArray)
                    {
                        property.PropertyOriginalEntityType = property.PropertyType.Replace(nameSpace + ".", "").Replace("ArrayList", "");
                    }
                    else
                    {
                        property.PropertyOriginalEntityType = property.PropertyType.Replace(nameSpace + ".", "");
                    }
                }
                else
                {
                    property.PropertyOriginalEntityType = property.PropertyType;
                }


                et.AddProperty(property);
            }
            return et;
        }


        /// <summary>
        /// new an EntityConfiguration from an EntityType
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static EntityConfiguration EntityType2EntityConfiguration(DynEntityType entityType)
        {
            EntityConfiguration ec = new EntityConfiguration();

            ec.Name = entityType.FullName;

            EntityDynAttribute attr = entityType.GetCustomAttribute(typeof(MappingNameDynAttribute));
            if (attr != null){
                ec.MappingName = (attr as MappingNameDynAttribute).Name;
            }
            else
            {
                ec.MappingName = entityType.Name;
            }              

            entityType.MappingName = ec.MappingName;


            attr = entityType.GetCustomAttribute(typeof(CommentDynAttribute));
            if (attr != null){
                ec.Comment = (attr as CommentDynAttribute).Content;
            }
            else
            {
                ec.Comment = "";
            }               

            entityType.Comment = ec.Comment;

            attr = entityType.GetCustomAttribute(typeof(BatchUpdateDynAttribute));
            if (attr != null){
                ec.BatchSize = (attr as BatchUpdateDynAttribute).BatchSize;
            }
            else
            {
                ec.BatchSize = 0;
            }                

            entityType.BatchSize = ec.BatchSize;

            if (string.IsNullOrEmpty(entityType.BaseEntityName))
            {
                ec.BaseEntity = null;
            }

            else
            {
                ec.BaseEntity = entityType.Namespace + "." + entityType.BaseEntityName;
            }

            attr = entityType.GetCustomAttribute(typeof(CustomDataDynAttribute));
            if (attr != null)
            {
                ec.CustomData = (attr as CustomDataDynAttribute).Data;
            }
            else
            {
                ec.CustomData = "";
            }

            entityType.CustomData = ec.CustomData;

            attr = entityType.GetCustomAttribute(typeof(AdditionalSqlScriptDynAttribute));
            if (attr != null)
            {
                ec.AdditionalSqlScript = (attr as AdditionalSqlScriptDynAttribute).Sql;
            }
            else
            {
                ec.AdditionalSqlScript = "";
            }

            entityType.AdditionalSqlScript = ec.AdditionalSqlScript;

            attr = entityType.GetCustomAttribute(typeof(ReadOnlyDynAttribute));
            if (attr != null)
            {
                ec.IsReadOnly = true;
            }
            else
            {
                ec.IsReadOnly = false;
            }
            entityType.IsReadOnly = ec.IsReadOnly;

            attr = entityType.GetCustomAttribute(typeof(AutoPreLoadDynAttribute));
            if (attr != null)
            {
                ec.IsAutoPreLoad = true;
            }
            else
            {
                ec.IsAutoPreLoad = false;
            }
            entityType.IsAutoPreLoad = ec.IsAutoPreLoad;

            attr = entityType.GetCustomAttribute(typeof(RelationDynAttribute));
            if (attr != null)
            {
                ec.IsRelation = true;
            }
            else
            {
                ec.IsRelation = false;
            }
            entityType.IsRelation = ec.IsRelation;

            //根据DynPropertyConfigurationCollection构造PropertyConfiguration
            List<PropertyConfiguration> pcList = new List<PropertyConfiguration>();
            //TODO:此处如果支持继承关系需要调整
            DynPropertyConfigurationCollection allProperties = entityType.GetProperties();
            foreach (DynPropertyConfiguration property in allProperties)
            {
                PropertyConfiguration pc = new PropertyConfiguration();

                pc.IsInherited = property.IsInherited;
                pc.InheritEntityMappingName = property.InheritEntityMappingName;

                attr = property.GetPropertyAttribute(typeof(PrimaryKeyDynAttribute));
                pc.IsPrimaryKey = (attr != null);

                IndexPropertyDynAttribute ipa = property.GetPropertyAttribute(typeof(IndexPropertyDynAttribute)) as IndexPropertyDynAttribute;
                if (ipa != null)
                {
                    pc.IsIndexProperty = true;
                    pc.IsIndexPropertyDesc = ipa.IsDesc;
                    property.IsIndexProperty = true;
                    property.IsIndexPropertyDesc = ipa.IsDesc;
                }

                attr = property.GetPropertyAttribute(typeof(NotNullDynAttribute));
                pc.IsNotNull = (attr != null);

                attr = property.GetPropertyAttribute(typeof(SerializationIgnoreDynAttribute));
                pc.IsSerializationIgnore = (attr != null);

                RelationKeyDynAttribute rka = property.GetPropertyAttribute(typeof(RelationKeyDynAttribute)) as RelationKeyDynAttribute;
                if (rka != null)
                {
                    pc.IsRelationKey = true;
                    pc.RelatedType = GetOutputNamespace(rka.RelatedType) + "." + rka.RelatedType;
                    pc.RelatedForeignKey = rka.RelatedPk;
                    property.IsRelationKey = true;
                    property.RelatedType = pc.RelatedType;
                    property.RelatedForeignKey = rka.RelatedPk;
                }

                FriendKeyDynAttribute fka = property.GetPropertyAttribute(typeof(FriendKeyDynAttribute)) as FriendKeyDynAttribute;
                if (fka != null)
                {
                    pc.IsFriendKey = true;
                    pc.RelatedForeignKey = fka.RelatedEntityPk;
                    pc.RelatedType = GetOutputNamespace(fka.RelatedEntityType) + "." + fka.RelatedEntityType;
                    property.IsFriendKey = true;
                    property.RelatedForeignKey = fka.RelatedEntityPk;
                    property.RelatedType = pc.RelatedType;
                }

                MappingNameDynAttribute mna = property.GetPropertyAttribute(typeof(MappingNameDynAttribute)) as MappingNameDynAttribute;
                if (mna != null)
                {
                    pc.MappingName = mna.Name;
                    property.MappingName = mna.Name;
                }

                CommentDynAttribute ca = property.GetPropertyAttribute(typeof(CommentDynAttribute)) as CommentDynAttribute;
                if (ca != null)
                {
                    pc.Comment = ca.Content;
                    property.Comment = ca.Content;
                }

                attr = property.GetPropertyAttribute(typeof(CustomDataDynAttribute));
                if (attr != null)
                {
                    pc.CustomData = ((CustomDataDynAttribute)attr).Data;
                }

                pc.IsReadOnly = property.IsReadOnly;
                pc.Name = property.Name;


                SqlTypeDynAttribute sta = property.GetPropertyAttribute(typeof(SqlTypeDynAttribute)) as SqlTypeDynAttribute;
                if (sta != null && (!string.IsNullOrEmpty(sta.Type)))
                {
                    pc.SqlType = sta.Type;
                    pc.SqlDefaultValue = sta.DefaultValue;
                    property.SqlType = sta.Type;
                    property.SqlDefaultValue = sta.DefaultValue;
                }

                QueryDynAttribute qa = property.GetPropertyQueryAttribute();                
                if (qa != null)
                {
                    pc.IsQueryProperty = true;
                    pc.QueryType = qa.QueryType.ToString();

                    property.PropertyOriginalEntityType = property.PropertyType;
                    property.IsQueryProperty = true;


                    string propertyEntityType = property.PropertyType;
                    //if (property.IsArray)
                    //{
                    //    propertyEntityType = property.PropertyType;
                    //}
                    DynQueryDescriber describer = new DynQueryDescriber(qa, property, propertyEntityType, entityType.Name);

                    pc.IsLazyLoad = describer.LazyLoad;
                    pc.QueryOrderBy = describer.OrderBy;
                    pc.QueryWhere = describer.Where;
                    property.IsLazyLoad = describer.LazyLoad;
                    property.QueryOrderBy = describer.OrderBy;
                    property.QueryWhere = describer.Where;

                    if (describer.RelationType != null)
                    {
                        pc.RelationType = GetOutputNamespace(describer.RelationType) + "." + describer.RelationType;
                    }
                    pc.IsContained = describer.Contained;
                    pc.RelatedForeignKey = describer.RelatedForeignKey;
                    property.IsContained = describer.Contained;
                    property.RelatedForeignKey = describer.RelatedForeignKey;
                    if (describer.RelatedType != null)
                    {
                        pc.RelatedType = GetOutputNamespace(describer.RelatedType) + "." + describer.RelatedType;
                        property.RelatedType = pc.RelatedType;
                    }

                    if (property.IsArray)
                    {
                        pc.PropertyType = GetOutputNamespace(property.PropertyType) + "." + property.PropertyType + "ArrayList";

                    }
                    else
                    {
                        pc.PropertyType = GetOutputNamespace(property.PropertyType) + "." + property.PropertyType;
                    }
                    property.PropertyType = pc.PropertyType;

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
                        property.PropertyMappingColumnType = pc.PropertyMappingColumnType;
                    }

                    if (qa.QueryType == QueryType.FkQuery)
                    {
                        pc.RelatedForeignKey = describer.RelatedForeignKey;
                        pc.RelatedType = GetOutputNamespace(describer.RelatedType) + "." + describer.RelatedType;

                        property.RelatedForeignKey = describer.RelatedForeignKey;
                        property.RelatedType = pc.RelatedType;
                    }
                }
                else
                {
                    pc.PropertyType = property.PropertyType;
                }

                pcList.Add(pc);
            }
            ec.Properties = pcList.ToArray();

            return ec;
        }

        private static string GetOutputNamespace(string typeName)
        {
            string outNs = "Entities";

            DynEntityType entityType = DynEntityTypeManager.GetEntityType(typeName);
            //OutputNamespaceDynAttribute outNsAttr = DynCodeGenHelper.GetEntityAttribute<OutputNamespaceDynAttribute>(typeName);
            //if (outNsAttr != null)
            //{
            //    return outNsAttr.Namespace;
            //}
            //else
            //{
            //    return outNs;
            //}

            if (entityType != null)
                return entityType.Namespace;
            else
                return outNs;
        }

    }
}
