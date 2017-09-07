using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Rock.Orm.Common.Design;

namespace Rock.Orm.Common
{
    /// <summary>
    /// 实体类型
    /// </summary>
    [Serializable]
    public class DynEntityType
    {
        #region Construct

        /// <summary>
        /// 给定类名 构造新的实体类型
        /// <param name="name">类名称</param>
        /// </summary>
        public DynEntityType(string name)
            : this(name, null)
        {
            _name = name;
        }

        /// <summary>
        /// 基于指定父类构造新的实体类型
        /// </summary>
        /// <param name="name">类名称</param>
        /// <param name="parentClass">父类</param>
        public DynEntityType(string name, string baseEntityName)
        {
            _name = name;

            if (!string.IsNullOrEmpty(baseEntityName))
            {
                _baseEntityName = baseEntityName;
            }
            _properties = new DynPropertyConfigurationCollection(this);
        }

        //{
        //    _baseEntityName = baseEntityName;
        //    _name = name;

        //    _properties = new DynPropertyConfigurationCollection(this);

        //    foreach (DynPropertyConfiguration property in BaseEntityType.Properties)
        //    {
        //        DynPropertyConfiguration inheritedProperty = new DynPropertyConfiguration(property);

        //        if (property.IsInherited == false)
        //        {
        //            inheritedProperty.IsInherited = true;
        //            inheritedProperty.InheritEntityMappingName = BaseEntityType.Name;
        //        }

        //        _properties.Add(inheritedProperty);
        //    }
        //}

        /// <summary>
        /// 基于指定父类构造新的实体类型
        /// </summary>
        /// <param name="name">类名称</param>
        /// <param name="parentClass">父类</param>
        //ToDo: 需要清理,zml
        public DynEntityType(string name, string baseEntityName, string namespaceName)
        {
            _name = name;

            if (!string.IsNullOrEmpty(baseEntityName))
            {
                _baseEntityName = baseEntityName;
            }

            _nameSpace = namespaceName;

            _properties = new DynPropertyConfigurationCollection(this);

            //_properties = new DynPropertyConfigurationCollection(this);

            //foreach (DynPropertyConfiguration property in BaseEntityType.Properties)
            //{
            //    DynPropertyConfiguration inheritedProperty = new DynPropertyConfiguration(property);

            //    if (property.IsInherited == false)
            //    {
            //        inheritedProperty.IsInherited = true;
            //        inheritedProperty.InheritEntityMappingName = BaseEntityType.Name;
            //    }

            //    _properties.Add(inheritedProperty);
            //}
        }

        #endregion

        #region Basic Properties

        private string _name;

        /// <summary>
        /// 类型名称, 不包含命名空间
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value.Trim(); }
        }

        private string _nameSpace;

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace
        {
            get
            {
                if (string.IsNullOrEmpty(_nameSpace))
                {
                    OutputNamespaceDynAttribute outNsAttr = DynCodeGenHelper.GetEntityAttribute<OutputNamespaceDynAttribute>(_name);
                    if (outNsAttr != null)
                    {
                        _nameSpace = outNsAttr.Namespace;
                    }
                    else
                    {
                        _nameSpace = "Entities";
                    }
                }

                return _nameSpace;
            }
            set
            {
                _nameSpace = value.Trim();
            }
        }

        /// <summary>
        /// 类型全名（命名空间.类型名称）
        /// </summary>
        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(_nameSpace))
                {
                    return _nameSpace + "." + _name;
                }
                else
                {
                    return _name;
                }

            }
        }

        #endregion

        /// <summary>
        /// Whether the entity is a relation type.
        /// </summary>
        private bool _isRelation;

        public bool IsRelation
        {
            get { return _isRelation; }
            set { _isRelation = value; }
        }

        /// <summary>
        /// Base entity of this entity.
        /// </summary>
        private string _baseEntityName;

        public string BaseEntityName
        {
            get { return _baseEntityName; }
            set { _baseEntityName = value; }
        }

        public DynEntityType BaseEntityType
        {
            get
            {
                if (_baseEntityName == null)
                    return null;

                return DynEntityTypeManager.GetEntityType(_baseEntityName);
            }
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

        /// <summary>
        /// Combined additional sql script clips which will be included into the sql script batch.
        /// </summary>
        private string _additionalSqlScript;

        public string AdditionalSqlScript
        {
            get { return _additionalSqlScript; }
            set { _additionalSqlScript = value; }
        }

        private string _mappingName;

        /// <summary>
        /// Whether the entity is readonly.
        /// </summary>
        private bool _isReadOnly;

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }

        /// <summary>
        /// Whether the entity is save all property related values in a batch to improve performance.
        /// </summary>
        private bool _isBatchUpdate;

        public bool IsBatchUpdate
        {
            get { return _isBatchUpdate; }
            set { _isBatchUpdate = value; }
        }

        /// <summary>
        /// Whether instances of the entity are automatically preloaded.
        /// </summary>
        private bool _isAutoPreLoad;

        public bool IsAutoPreLoad
        {
            get { return _isAutoPreLoad; }
            set { _isAutoPreLoad = value; }
        }

        /// <summary>
        /// _comment of entity
        /// </summary>
        private string _comment;

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        private int _batchSize;

        /// <summary>
        /// The batch size when an entity is marked as _isBatchUpdate.
        /// </summary>
        public int BatchSize
        {
            get
            {
                if (_batchSize <= 0)
                {
                    _batchSize = 10;
                }
                return _batchSize;
            }
            set
            {
                _batchSize = value;
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
                return _mappingName ?? RemoveTypePrefix(Name);
            }
            set
            {
                _mappingName = value;
            }
        }

        /// <summary>
        /// Gets the name of the view to select data.
        /// </summary>
        /// <value>The name of the view.</value>
        public string ViewName
        {
            get
            {
                return _mappingName;
            }
            set
            {
                _mappingName = value;
            }
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

                DynEntityType parentType = this.BaseEntityType;
                while (parentType != null)
                {
                    attrs.AddRange(parentType.Attributes);

                    parentType = parentType.BaseEntityType;
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

                DynEntityType parentType = this.BaseEntityType;
                while (parentType != null)
                {
                    foreach (EntityDynAttribute attr in parentType.Attributes)
                    {
                        if (attributeType.IsAssignableFrom(attr.GetType()))
                            attrs.Add(attr);
                    }

                    parentType = parentType.BaseEntityType;
                }

                if (attrs.Count > 0)
                    return attrs.ToArray();
                else
                    return null;
            }
        }

        public EntityDynAttribute GetCustomAttribute(Type attributeType)
        {
            EntityDynAttribute entityDynAttribute = null;
            foreach (EntityDynAttribute attr in _attributes)
            {
                if (attributeType.IsAssignableFrom(attr.GetType()))
                {
                    entityDynAttribute = attr;
                    break;
                }
            }
            return entityDynAttribute;
        }

        // 原来的方法名：GetInterface()
        public DynEntityType[] GetAllBaseEntityTypes()
        {
            List<DynEntityType> lstEntityTypes = new List<DynEntityType>();

            DynEntityType parentType = this.BaseEntityType;
            while (parentType != null)
            {
                lstEntityTypes.Add(parentType);

                parentType = parentType.BaseEntityType;
            }

            return lstEntityTypes.ToArray();
        }

        public bool IsAssignableFrom(DynEntityType entityType)
        {
            if (entityType.Name == this.Name)
            {
                return true;
            }
            else
            {
                DynEntityType parentType = this.BaseEntityType;

                while (parentType != null)
                {
                    if (parentType.Name == this.Name)
                        return true;
                    else
                        parentType = parentType.BaseEntityType;
                }
            }

            return false;
        }

        #endregion

        #region Properties

        public void AddProperty(DynPropertyConfiguration dynProperty)
        {
            _properties.Add(dynProperty);
        }

        public void RemoveProperty(string propertyName)
        {
            _properties.Remove(propertyName);
        }

        public DynPropertyConfiguration GetProperty(string propertyName)
        {
            if (_properties.Contains(propertyName))
            {
                return _properties[propertyName];
            }
            else
            {
                if (!string.IsNullOrEmpty(_baseEntityName) && BaseEntityType != null)
                {
                    DynPropertyConfiguration inheritedProperty = BaseEntityType.GetProperty(propertyName);
                    inheritedProperty.IsInherited = true;
                    if (inheritedProperty.InheritEntityMappingName == null)
                    {
                        inheritedProperty.InheritEntityMappingName = BaseEntityType.Name;
                    }
                    return inheritedProperty;
                }

                throw new ApplicationException("当前类或基类没有名称为" + propertyName + "的属性！");
            }
        }

        /// <summary>
        /// 是否包含属性
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>是否</returns>
        public bool ContainsProperty(string propertyName)
        {
            if (_properties.Contains(propertyName))
            {
                return true;
            }
            else
            {
                if (!string.IsNullOrEmpty(_baseEntityName))
                {
                    return BaseEntityType.ContainsProperty(propertyName);
                }

                return false;
            }
        }

        private DynPropertyConfigurationCollection _properties;

        public DynPropertyConfigurationCollection GetProperties()
        {
            DynPropertyConfigurationCollection properties = new DynPropertyConfigurationCollection(this);

            properties.AddRange(_properties);

            if (!string.IsNullOrEmpty(_baseEntityName) && BaseEntityType != null)
            {
                DynPropertyConfigurationCollection inheritedProperties = BaseEntityType.GetProperties();

                foreach (DynPropertyConfiguration inheritedProperty in inheritedProperties)
                {
                    inheritedProperty.IsInherited = true;
                    if (inheritedProperty.InheritEntityMappingName == null)
                    {
                        inheritedProperty.InheritEntityMappingName = BaseEntityType.Name;
                    }
                }

                properties.AddRange(inheritedProperties);
            }

            return properties;
        }

        #endregion

        #region QueryCode

        public class _
        {
            private _() { }

            public static PropertyItem ID = new PropertyItem("ID");
        }

        #endregion
    }
}
