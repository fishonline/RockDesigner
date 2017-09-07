using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Rock.Orm.Common.Design
{
    /// <summary>
    /// Used by Rock self to describe the details of a EntityProperty.
    /// </summary>
    public class DynQueryDescriber
    {
        #region Private Members

        private QueryDynAttribute qa;
        private string propertyName;
        private string relatedEntityPropertyName;
        private string relatedEntityType;
        private string relatedEntityPk;
        private string relatedEntityPkType;
        private DynPropertyConfiguration relatedEntityPkEntityProperty;
        private string entityPk;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the where.
        /// </summary>
        /// <value>The where.</value>
        public string Where
        {
            get
            {
                switch (qa.QueryType)
                {
                    case QueryType.PkQuery:
                        return string.Format("[{0}] = @{1}", relatedEntityPk, entityPk).Replace("[", "{").Replace("]", "}");
                    case QueryType.PkReverseQuery:
                        return string.Format("[{0}] = @{1}", relatedEntityPk, entityPk).Replace("[", "{").Replace("]", "}");
                    case QueryType.FkQuery:
                        string additionalWhere = ((FkQueryDynAttribute)qa).AdditionalWhere;
                        return string.Format("[{0}] = @{1}{2}", relatedEntityPropertyName, entityPk, string.IsNullOrEmpty(additionalWhere) ? string.Empty : " AND ( " + additionalWhere + ")").Replace("[", "{").Replace("]", "}");
                    case QueryType.FkReverseQuery:
                        return string.Format("[{0}] = @{1}", relatedEntityPk, propertyName).Replace("[", "{").Replace("]", "}");
                    case QueryType.ManyToManyQuery:
                        return ((ManyToManyQueryDynAttribute)qa).AdditionalWhere;
                    case QueryType.CustomQuery:
                        return ((CustomQueryDynAttribute)qa).Where;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the order by.
        /// </summary>
        /// <value>The order by.</value>
        public string OrderBy
        {
            get
            {
                switch (qa.QueryType)
                {
                    case QueryType.CustomQuery:
                        return ((CustomQueryDynAttribute)qa).OrderBy;
                    case QueryType.FkQuery:
                        return ((FkQueryDynAttribute)qa).OrderBy;
                    case QueryType.ManyToManyQuery:
                        return ((ManyToManyQueryDynAttribute)qa).OrderBy;
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets the type of the relation.
        /// </summary>
        /// <value>The type of the relation.</value>
        public string RelationType
        {
            get
            {
                switch (qa.QueryType)
                {
                    case QueryType.CustomQuery:
                        return ((CustomQueryDynAttribute)qa).RelationType;
                    case QueryType.ManyToManyQuery:
                        return ((ManyToManyQueryDynAttribute)qa).RelationType;
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="DynQueryDescriber"/> is contained in cascade update.
        /// </summary>
        /// <value><c>true</c> if contained; otherwise, <c>false</c>.</value>
        public bool Contained
        {
            get
            {
                if (qa.QueryType == QueryType.PkQuery)
                {
                    return true;
                }
                else
                {
                    switch (qa.QueryType)
                    {
                        case QueryType.FkQuery:
                            return ((FkQueryDynAttribute)qa).Contained;
                        case QueryType.ManyToManyQuery:
                            return ((ManyToManyQueryDynAttribute)qa).Contained;
                        case QueryType.CustomQuery:
                            return ((CustomQueryDynAttribute)qa).Contained;
                        default:
                            return false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the related foreign key.
        /// </summary>
        /// <value>The related foreign key.</value>
        public string RelatedForeignKey
        {
            get
            {
                switch (qa.QueryType)
                {
                    case QueryType.ManyToManyQuery:
                        return relatedEntityPk;
                    case QueryType.CustomQuery:
                        if (((CustomQueryDynAttribute)qa).RelationType != null)
                        {
                            return relatedEntityPk;
                        }
                        else
                        {
                            return relatedEntityPropertyName;
                        }
                    default:
                        return relatedEntityPropertyName ?? relatedEntityPk;
                }
            }
        }

        /// <summary>
        /// Gets the type of the related foreign key.
        /// </summary>
        /// <value>The type of the related foreign key.</value>
        public string RelatedForeignKeyType
        {
            get
            {
                return relatedEntityPkType;
            }
        }

        /// <summary>
        /// Gets the related foreign key property info.
        /// </summary>
        /// <value>The related foreign key property info.</value>
        public DynPropertyConfiguration RelatedForeignKeyEntityProperty
        {
            get
            {
                return relatedEntityPkEntityProperty;
            }
        }

        /// <summary>
        /// Gets the type of the related entity.
        /// </summary>
        /// <value>The type of the related.</value>
        public string RelatedType
        {
            get
            {
                return relatedEntityType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the property is lazyload.
        /// </summary>
        /// <value><c>true</c> if lazyload; otherwise, <c>false</c>.</value>
        public bool LazyLoad
        {
            get
            {
                return qa.LazyLoad;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Gets the name of the pk property or specified entity type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The pk name.</returns>
        internal static string GetPkPropertyName(string typeName)
        {
            string retName = null;
            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);

            foreach (DynPropertyConfiguration item in entityType.GetProperties())
            {
                if (item.GetCustomAttributes(typeof(PrimaryKeyDynAttribute), item.IsInherited) != null)
                {
                    retName = item.Name;
                    break;
                }
            }

            if (retName == null)
            {
                //check base entities
                foreach (DynEntityType baseType in entityType.GetAllBaseEntityTypes())
                {
                    foreach (DynPropertyConfiguration item in baseType.GetProperties())
                    {
                        if (item.GetCustomAttributes(typeof(PrimaryKeyDynAttribute), item.IsInherited) != null)
                        {
                            retName = item.Name;
                            break;
                        }
                    }
                    if (retName != null)
                    {
                        break;
                    }
                }
            }

            return retName;
        }

        /// <summary>
        /// Gets the pk property info of the specified entity type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The property info.</returns>
        internal static DynPropertyConfiguration GetPkEntityProperty(string typeName)
        {
            DynPropertyConfiguration retPi = null;

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            foreach (DynPropertyConfiguration item in entityType.GetProperties())
            {
                if (item.GetCustomAttributes(typeof(PrimaryKeyDynAttribute), item.IsInherited)!=null)
                {
                    retPi = item;
                    break;
                }
            }

            if (retPi == null)
            {
                //check base entities
                foreach (DynEntityType baseType in entityType.GetAllBaseEntityTypes())
                {
                    foreach (DynPropertyConfiguration item in baseType.GetProperties())
                    {
                        if (item.GetCustomAttributes(typeof(PrimaryKeyDynAttribute), item.IsInherited) != null)
                        {
                            retPi = item;
                            break;
                        }
                    }
                    if (retPi != null)
                    {
                        break;
                    }
                }
            }

            return retPi;
        }

        /// <summary>
        /// Gets the type of the pk property.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The type.</returns>
        internal static string GetPkPropertyType(string typeName)
        {
            string retType = null;

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            foreach (DynPropertyConfiguration item in entityType.GetProperties())
            {
                if (item.GetCustomAttributes(typeof(PrimaryKeyDynAttribute), item.IsInherited) != null)
                {
                    retType = item.PropertyType;
                    break;
                }
            }

            if (retType == null)
            {
                //check base entities
                foreach (DynEntityType baseType in entityType.GetAllBaseEntityTypes())
                {
                    foreach (DynPropertyConfiguration item in baseType.GetProperties())
                    {
                        if (item.GetCustomAttributes(typeof(PrimaryKeyDynAttribute), item.IsInherited) != null)
                        {
                            retType = item.PropertyType;
                            break;
                        }
                    }
                    if (retType != null)
                    {
                        break;
                    }
                }
            }

            return retType;
        }

        private string GetRelatedEntityPropertyName(QueryDynAttribute qa)
        {
            switch (qa.QueryType)
            {
                case QueryType.FkQuery:
                    return ((FkQueryDynAttribute)qa).RelatedManyToOneQueryPropertyName;
                default:
                    return null;
            }
        }

        private Dictionary<string, string> GetRelationKeysMap(QueryDynAttribute qa)
        {
            switch (qa.QueryType)
            {
                case QueryType.ManyToManyQuery:
                    return GetRelationKeysMapFromRelationType(((ManyToManyQueryDynAttribute)qa).RelationType);
                case QueryType.CustomQuery:
                    if (((CustomQueryDynAttribute)qa).RelationType != null)
                    {
                        return GetRelationKeysMapFromRelationType(((CustomQueryDynAttribute)qa).RelationType);
                    }
                    else
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }

        private Dictionary<string, string> GetRelationKeysMapFromRelationType(string typeName)
        {
            Dictionary<string, string> retMap = new Dictionary<string, string>();
            DynEntityType type = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            foreach (DynPropertyConfiguration item in type.GetProperties())
            {
                object[] attrs = item.GetCustomAttributes(typeof(RelationKeyDynAttribute), item.IsInherited);
                if (attrs.Length > 0)
                {
                    RelationKeyDynAttribute rka = (RelationKeyDynAttribute)attrs[0];
                    try
                    {
                        retMap.Add(rka.RelatedType, item.Name);
                    }
                    catch
                    {
                        throw new NotSupportedException("Many to many relaion only supports single primary key entities.");
                    }
                }
            }

            if (retMap.Count != 2)
            {
                throw new NotSupportedException("Relation entity could and must exactly contain two related key properties.");
            }

            return retMap;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynQueryDescriber"/> class.
        /// </summary>
        /// <param name="qa">The query attribute instance.</param>
        /// <param name="qa">The property's property info.</param>
        /// <param name="pi">The entity type that the property returns.</param>
        /// <param name="entityType">The entity type.</param>
        public DynQueryDescriber(QueryDynAttribute qa, DynPropertyConfiguration pi, string propertyEntityType, string entityType)
        {
            this.qa = qa;

            //parse info from pi
            propertyName = pi.Name;

            relatedEntityPropertyName = GetRelatedEntityPropertyName(qa);

            entityPk = GetPkPropertyName(entityType);

            relatedEntityType = propertyEntityType;
            relatedEntityPk = GetPkPropertyName(propertyEntityType);
            relatedEntityPkEntityProperty = GetPkEntityProperty(propertyEntityType);
            relatedEntityPkType = GetPkPropertyType(propertyEntityType);
        }

        #endregion
    }
}
