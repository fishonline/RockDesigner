using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Data;
using Rock.Orm.Common.Design;

namespace Rock.Orm.Common
{
    /// <summary>
    /// 动态属性
    /// </summary>
    public class DynPropertyConfiguration
    {
        #region Construct

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propertyName">属性名</param>
        public DynPropertyConfiguration(string propertyName)
        {
            _name = propertyName;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propertyName">属性名</param>
        public DynPropertyConfiguration(DynPropertyConfiguration orientProperty)
        {
            _name = orientProperty._name;
            _propertyType = orientProperty._propertyType;
            _isArray = orientProperty._isArray;
            _isInherited = orientProperty._isInherited;
            _inheritEntityMappingName = orientProperty._inheritEntityMappingName;
            _entityType = orientProperty._entityType;
            _propertyOriginalEntityType = orientProperty._propertyOriginalEntityType;
            _relatedType = orientProperty._relatedType;

            _propertyType = orientProperty.PropertyType;
            _isInherited = orientProperty.IsInherited;
            _isQueryProperty = orientProperty.IsQueryProperty;
            _isReadOnly = orientProperty.IsReadOnly;
            _inheritEntityMappingName = orientProperty.InheritEntityMappingName;
            //_isCompoundUnit = orientProperty.IsCompoundUnit;
            _comment = orientProperty.Comment;
            _customData = orientProperty.CustomData;
            _isContained = orientProperty.IsContained;
            _isFriendKey = orientProperty.IsFriendKey;
            _isIndexProperty = orientProperty.IsIndexProperty;
            _isIndexPropertyDesc = orientProperty.IsIndexPropertyDesc;
            _isInherited = orientProperty.IsInherited;
            _isLazyLoad = orientProperty.IsLazyLoad;
            _isNotNull = orientProperty.IsNotNull;
            _isPrimaryKey = orientProperty.IsPrimaryKey;
            _isQueryProperty = orientProperty.IsQueryProperty;
            _isReadOnly = orientProperty.IsReadOnly;
            _isRelationKey = orientProperty.IsRelationKey;
            _isSerializationIgnore = orientProperty.IsSerializationIgnore;
            _mappingName = orientProperty.MappingName;
            _name = orientProperty.Name;
            _propertyMappingColumnType = orientProperty.PropertyMappingColumnType;
            _propertyType = orientProperty.PropertyType;
            _queryOrderBy = orientProperty.QueryOrderBy;
            _queryType = orientProperty.QueryType;
            _queryWhere = orientProperty.QueryWhere;
            _relatedForeignKey = orientProperty.RelatedForeignKey;
            _relatedType = orientProperty.RelatedType;
            _relationType = orientProperty.RelationType;
            _sqlDefaultValue = orientProperty.SqlDefaultValue;
            _sqlType = orientProperty.SqlType;

            _attributes.AddRange(orientProperty.Attributes);
        }


        #endregion

        #region Basic Properties

        private string _name;
        private string _propertyType;
        private bool _isArray;
        private bool _isInherited = false;
        private string _inheritEntityMappingName = null;
        private DynEntityType _entityType;
        private string _propertyOriginalEntityType;

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// 属性的数据类型
        /// </summary>
        public string PropertyType
        {
            get { return _propertyType; }
            set { _propertyType = value; }
        }

        /// <summary>
        /// 是否是集合类型
        /// string[], EntityArrayList
        /// </summary>
        public bool IsArray
        {
            get { return _isArray; }
            set { _isArray = value; }
        }

        /// <summary>
        /// 是否是继承属性
        /// </summary>
        public bool IsInherited
        {
            get { return _isInherited; }
            set { _isInherited = value; }
        }

        /// <summary>
        /// 继承的实体MappingName
        /// </summary>
        public string InheritEntityMappingName
        {
            get { return _inheritEntityMappingName; }
            set { _inheritEntityMappingName = value; }
        }

        /// <summary>
        /// 属性所在的实体类型
        /// </summary>
        public DynEntityType EntityType
        {
            get { return _entityType; }
            set { _entityType = value; }
        }

        /// <summary>
        /// 属性本身的EntityType
        /// 当PropertyType类型为EntityArrayList时，返回对应的EntityType
        /// </summary>
        public string PropertyOriginalEntityType
        {
            get { return _propertyOriginalEntityType; }
            set { _propertyOriginalEntityType = value; }
        }

        #endregion

        /// <summary>
        /// _comment of entity
        /// </summary>
        private string _comment;

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// Custom data.
        /// </summary>
        private string _customData;

        public string CustomData
        {
            get { return _customData; }
            set { _customData = value; }
        }

        ///// <summary>
        ///// Whether the property is a CompoundUnit.
        ///// </summary>
        //private bool _isCompoundUnit;

        //public bool IsCompoundUnit
        //{
        //    get { return _isCompoundUnit; }
        //    set { _isCompoundUnit = value; }
        //}

        /// <summary>
        /// Whether the property is a contained property, which means if the entity saved or deleted, all contained property will be included in update object list.
        /// </summary>
        private bool _isContained;

        public bool IsContained
        {
            get { return _isContained; }
            set { _isContained = value; }
        }

        /// <summary>
        /// Whether a property is a friend DEFAULT_KEY.
        /// </summary>
        private bool _isFriendKey;

        public bool IsFriendKey
        {
            get { return _isFriendKey; }
            set { _isFriendKey = value; }
        }

        /// <summary>
        /// Whether need to add index for the property when creating the table in database.
        /// </summary>
        private bool _isIndexProperty;

        public bool IsIndexProperty
        {
            get { return _isIndexProperty; }
            set { _isIndexProperty = value; }
        }

        /// <summary>
        /// Whether the index property is desc.
        /// </summary>
        private bool _isIndexPropertyDesc;

        public bool IsIndexPropertyDesc
        {
            get { return _isIndexPropertyDesc; }
            set { _isIndexPropertyDesc = value; }
        }

        /// <summary>
        /// Whether the property is a lazyload query property. It is only used by query entity.
        /// </summary>
        private bool _isLazyLoad;

        public bool IsLazyLoad
        {
            get { return _isLazyLoad; }
            set { _isLazyLoad = value; }
        }

        private bool _isNotNull;

        private bool _isPrimaryKey;

        /// <summary>
        /// Whether the property is readonly.
        /// </summary>
        private bool _isReadOnly;

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }

        /// <summary>
        /// Whether the property is a relationkey. It is only used by relation entity.
        /// </summary>
        private bool _isRelationKey;

        public bool IsRelationKey
        {
            get { return _isRelationKey; }
            set { _isRelationKey = value; }
        }

        /// <summary>
        /// Whether this property should not included in default XML serialization.
        /// </summary>
        private bool _isSerializationIgnore;

        public bool IsSerializationIgnore
        {
            get { return _isSerializationIgnore; }
            set { _isSerializationIgnore = value; }
        }

        private string _mappingName;

        private string _propertyMappingColumnType;

        /// <summary>
        /// The order by condition used by query property.
        /// </summary>
        private string _queryOrderBy;

        public string QueryOrderBy
        {
            get { return _queryOrderBy; }
            set { _queryOrderBy = value; }
        }

        /// <summary>
        /// The type of the query property.
        /// </summary>
        private string _queryType;

        public string QueryType
        {
            get { return _queryType; }
            set { _queryType = value; }
        }

        /// <summary>
        /// The where condition used by query property.
        /// </summary>
        private string _queryWhere;

        public string QueryWhere
        {
            get { return _queryWhere; }
            set { _queryWhere = value; }
        }

        /// <summary>
        /// The related entity type's foreignkey relating to this relationkey. It is only used by relation entity.
        /// </summary>
        private string _relatedForeignKey;

        public string RelatedForeignKey
        {
            get { return _relatedForeignKey; }
            set { _relatedForeignKey = value; }
        }

        /// <summary>
        /// The relation type of the query property.
        /// </summary>
        private string _relationType;

        public string RelationType
        {
            get { return _relationType; }
            set { _relationType = value; }
        }

        /// <summary>
        /// The sql default value
        /// </summary>
        private string _sqlDefaultValue;

        public string SqlDefaultValue
        {
            get { return _sqlDefaultValue; }
            set { _sqlDefaultValue = value; }
        }

        private string _sqlType;

        #region Persist Properties

        private string _relatedType;

        /// <summary>
        /// 是否是查询属性
        /// </summary>
        private bool _isQueryProperty;

        public bool IsQueryProperty
        {
            get { return _isQueryProperty; }
            set { _isQueryProperty = value; }
        }
        //{
        //    get
        //    {
        //        foreach (DynAttribute attribute in _attributes)
        //        {
        //            if (attribute is QueryDynAttribute)
        //                return true;
        //        }

        //        return false;
        //    }
        //}

        /// <summary>
        /// 关联类型
        /// </summary>
        public string RelatedType
        {
            get { return _relatedType; }
            set { _relatedType = value; }
        }

        #endregion

        #region Attributes

        private List<EntityDynAttribute> _attributes = new List<EntityDynAttribute>();

        public List<EntityDynAttribute> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        public EntityDynAttribute[] GetCustomAttributes(bool inherit)
        {
            if (inherit == false)
            {
                if (_attributes.Count > 0)
                    return _attributes.ToArray();
                else
                    return null;
            }
            else
            {
                List<EntityDynAttribute> attrs = new List<EntityDynAttribute>();
                attrs.AddRange(_attributes);

                DynEntityType parentType = DynEntityTypeManager.GetEntityType(this.EntityType.BaseEntityName);
                while (parentType != null)
                {
                    attrs.AddRange(parentType.GetProperty(_name).Attributes);

                    parentType = DynEntityTypeManager.GetEntityType(parentType.BaseEntityName);
                }

                if (attrs.Count > 0)
                    return attrs.ToArray();
                else
                    return null;
            }
        }

        public EntityDynAttribute[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (inherit == false)
            {
                List<EntityDynAttribute> attrs = new List<EntityDynAttribute>();
                foreach (EntityDynAttribute attr in _attributes)
                {
                    if (attributeType.IsAssignableFrom(attr.GetType()))
                        attrs.Add(attr);
                }

                if (attrs.Count > 0)
                    return attrs.ToArray();
                else
                    return null;
            }
            else
            {
                List<EntityDynAttribute> attrs = new List<EntityDynAttribute>();
                foreach (EntityDynAttribute attr in _attributes)
                {
                    if (attributeType.IsAssignableFrom(attr.GetType()))
                        attrs.Add(attr);
                }

                DynEntityType parentType = DynEntityTypeManager.GetEntityType(this.EntityType.BaseEntityName);
                while (parentType != null)
                {
                    if (parentType.ContainsProperty(_name))
                    {
                        foreach (EntityDynAttribute attr in parentType.GetProperty(_name).Attributes)
                        {
                            if (attributeType.IsAssignableFrom(attr.GetType()))
                                attrs.Add(attr);
                        }
                    }

                    parentType = DynEntityTypeManager.GetEntityType(parentType.BaseEntityName);
                }

                if (attrs.Count > 0)
                    return attrs.ToArray();
                else
                    return null;
            }
        }

        public EntityDynAttribute GetPropertyAttribute(Type attributeType)
        {
            EntityDynAttribute entityDynAttribute = null;
            foreach (EntityDynAttribute attr in _attributes)
            {
                if (attributeType.IsAssignableFrom(attr.GetType()))
                {
                    entityDynAttribute = attr;
                }                   
            }
            return entityDynAttribute;
        }

        public QueryDynAttribute GetPropertyQueryAttribute()
        {
            QueryDynAttribute queryDynAttribute = null;
            foreach (EntityDynAttribute attr in _attributes)
            {
                if (typeof(QueryDynAttribute).IsAssignableFrom(attr.GetType()))
                {
                    queryDynAttribute = attr as QueryDynAttribute;
                }
            }
            //QueryDynAttribute qa = GetPropertyAttribute(typeof(QueryDynAttribute));
            //if (qa == null) qa = GetPropertyAttribute<FkQueryDynAttribute>(dynEntityProperty);
            //if (qa == null) qa = GetPropertyAttribute<CustomQueryDynAttribute>(dynEntityProperty);
            //if (qa == null) qa = GetPropertyAttribute<PkReverseQueryDynAttribute>(dynEntityProperty);
            //if (qa == null) qa = GetPropertyAttribute<FkReverseQueryDynAttribute>(dynEntityProperty);
            //if (qa == null) qa = GetPropertyAttribute<ManyToManyQueryDynAttribute>(dynEntityProperty);

            return queryDynAttribute;
        }

        #endregion

        //#region IXMLSerialize Membership


        //#endregion

        public System.Data.DbType DbType
        {
            get
            {
                switch (SqlType.TrimStart().Split(' ', '(')[0].ToLower())
                {
                    case "bigint":
                        return System.Data.DbType.Int64;
                    case "int":
                        return System.Data.DbType.Int32;
                    case "smallint":
                        return System.Data.DbType.Int16;
                    case "tinyint":
                        return System.Data.DbType.Byte;
                    case "bit":
                        return System.Data.DbType.Boolean;
                    case "decimal":
                        return System.Data.DbType.Decimal;
                    case "numberic":
                        return System.Data.DbType.Decimal;
                    case "money":
                        return System.Data.DbType.Decimal;
                    case "smallmoney":
                        return System.Data.DbType.Decimal;
                    case "float":
                        return System.Data.DbType.Double;
                    case "real":
                        return System.Data.DbType.Double;
                    case "datetime":
                        return System.Data.DbType.DateTime;
                    case "smalldatetime":
                        return System.Data.DbType.DateTime;
                    case "timestamp":
                        return System.Data.DbType.DateTime;
                    case "char":
                        return System.Data.DbType.AnsiStringFixedLength;
                    case "varchar":
                        return System.Data.DbType.AnsiString;
                    case "text":
                        return System.Data.DbType.AnsiString;
                    case "nchar":
                        return System.Data.DbType.StringFixedLength;
                    case "nvarchar":
                        return System.Data.DbType.String;
                    case "ntext":
                        return System.Data.DbType.String;
                    case "binary":
                        return System.Data.DbType.Binary;
                    case "varbinary":
                        return System.Data.DbType.Binary;
                    case "image":
                        return System.Data.DbType.Binary;
                    case "uniqueidentifier":
                        return System.Data.DbType.Guid;
                }

                //should not reach here
                return System.Data.DbType.String;
            }
        }

        /// <summary>
        /// whether the property could not be NULL.
        /// </summary>
        public bool IsNotNull
        {
            get
            {
                return _isNotNull || _isPrimaryKey || IsIndexProperty;
            }
            set
            {
                _isNotNull = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is primary DEFAULT_KEY.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is primary DEFAULT_KEY; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimaryKey
        {
            get
            {
                return _isPrimaryKey || ((!IsQueryProperty) && IsRelationKey);
            }
            set
            {
                _isPrimaryKey = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the mapping.
        /// </summary>
        /// <value>The name of the mapping.</value>
        public string MappingName
        {
            get
            {
                if (IsQueryProperty && QueryType == "FkReverseQuery" && RelatedForeignKey != null)
                {
                    return _mappingName ?? Name + "_" + RelatedForeignKey;
                }
                else
                {
                    return _mappingName ?? Name;
                }
            }
            set
            {
                _mappingName = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the property mapping column.
        /// </summary>
        /// <value>The type of the property mapping column.</value>
        public string PropertyMappingColumnType
        {
            get
            {
                return _propertyMappingColumnType ?? PropertyType;
            }
            set
            {
                _propertyMappingColumnType = value;
            }
        }

        /// <summary>
        /// Gets or sets the mapping sql type.
        /// </summary>
        /// <value>The type of the SQL.</value>
        public string SqlType
        {
            get
            {
                if (string.IsNullOrEmpty(_sqlType))
                {
                    _sqlType = GetDefaultSqlType(Util.GetType(PropertyMappingColumnType) ?? typeof(string));
                }
                return _sqlType;
            }
            set
            {
                _sqlType = value;
            }
        }

        private string GetDefaultSqlType(Type type)
        {
            if (type.IsEnum)
            {
                return "int";
            }
            else if (type == typeof(long) || type == typeof(long?))
            {
                return "bigint";
            }
            else if (type == typeof(int) || type == typeof(int?))
            {
                return "int";
            }
            else if (type == typeof(short) || type == typeof(short?))
            {
                return "smallint";
            }
            else if (type == typeof(byte) || type == typeof(byte?))
            {
                return "tinyint";
            }
            else if (type == typeof(bool) || type == typeof(bool?))
            {
                return "bit";
            }
            else if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return "decimal";
            }
            else if (type == typeof(float) || type == typeof(float?))
            {
                return "real";
            }
            else if (type == typeof(double) || type == typeof(double?))
            {
                return "float";
            }
            else if (type == typeof(string))
            {
                return "nvarchar(127)";
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return "datetime";
            }
            else if (type == typeof(char) || type == typeof(char?))
            {
                return "nchar";
            }
            else if (type == typeof(string))
            {
                return "nvarchar(127)";
            }
            else if (type == typeof(byte[]))
            {
                return "image";
            }
            else if (type == typeof(Guid) || type == typeof(Guid?))
            {
                return "uniqueidentifier";
            }

            return "ntext";
        }
    }
}
