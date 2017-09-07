using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;

namespace Rock.Orm.Common
{
    /// <summary>
    /// The entity meta data manager.
    /// </summary>
    public sealed class MetaDataManager
    {
        private static List<string> _entityNames = new List<string>();
        private static List<EntityConfiguration> _entities = new List<EntityConfiguration>();
        private static Dictionary<string, List<EntityConfiguration>> _childEntitiesMap = new Dictionary<string, List<EntityConfiguration>>();
        private static Dictionary<string, List<string>> _isLazyLoadMap = new Dictionary<string, List<string>>();
       
        private static Dictionary<string, List<string>> _sqlTypeWithDefaultValueColumns = new Dictionary<string, List<string>>();
        private static List<string> _nonRelatedEntities = new List<string>();
        private static Dictionary<string, string> _autoIdEntities = new Dictionary<string, string>();

        private static void LoadEmbeddedEntityConfigurationsFromLoadedEntities()
        {
            List<EntityConfiguration> list = new List<EntityConfiguration>();

            try
            {
                System.Reflection.Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();

                for (int i = asses.Length - 1; i >= 0; i--)
                {
                    System.Reflection.Assembly ass = asses[i];
                    try
                    {
                        foreach (Type t in ass.GetTypes())
                        {
                            if (t.IsSubclassOf(typeof(Entity)) && t != typeof(Entity) && (!t.IsAbstract))
                            {
                                EntityConfiguration item = GetEmbeddedEntityConfigurationsFromEntityType(t);
                                if (item != null)
                                {
                                    list.Add(item);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            if (list.Count > 0)
            {
                MetaDataManager.AddEntityConfigurations(list.ToArray());
            }
        }

        private static EntityConfiguration GetEmbeddedEntityConfigurationsFromEntityType(Type type)
        {
            if (type == null)
                return null;

            object[] attrs = type.GetCustomAttributes(typeof(EmbeddedEntityConfigurationAttribute), false);

            if (attrs != null && attrs.Length > 0)
            {
                EmbeddedEntityConfigurationAttribute embeddedConfigAttr = (EmbeddedEntityConfigurationAttribute)attrs[0];
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(EntityConfiguration));
                StringReader sr = new StringReader(embeddedConfigAttr.Content);
                EntityConfiguration embeddedEc = (EntityConfiguration)serializer.Deserialize(sr);
                sr.Close();
                return embeddedEc;
            }

            return null;
        }

        /// <summary>
        /// Initializes the <see cref="MetaDataManager"/> class.
        /// </summary>
        static MetaDataManager()
        {           
        }

        public static List<EntityConfiguration> Entities
        {
            get
            {
                return _entities;
            }
        }

        public static bool EntitiesIsChange(EntityConfiguration ec)
        {
            bool ok = false;
            EntityConfiguration oec = new EntityConfiguration();
            foreach (EntityConfiguration item in _entities)
            {
                if (item.Name == ec.Name)
                {
                    oec = item;
                    ok = item.Properties.Length == ec.Properties.Length;
                    break;
                }
            }
            if (!ok)
            {
                _entities.Remove(oec);
                _entities.Add(ec);
            }
            return ok;
        }

        //public static void ModifyEntityConfigurations(EntityConfiguration ec)
        //{
        //    EntityConfiguration ecf = new EntityConfiguration();
        //    foreach (EntityConfiguration item in _entities)
        //    {
        //        if (item.Name == ec.Name)
        //        {
        //            ecf = item;
        //        }
        //    }
        //}

        public static void AddEntityConfigurations(EntityConfiguration[] ecs)
        {
            if (ecs != null)
            {
                foreach (EntityConfiguration ec in ecs)
                {


                    if (_entityNames.Contains(ec.Name))
                    {
                        if (EntitiesIsChange(ec))
                        {
                            continue;
                        }
                    }
                    lock (_entityNames) //这边加把大锁
                    {
                        if (!_entityNames.Contains(ec.Name))
                        {
                            _entityNames.Add(ec.Name);
                            _entities.Add(ec);

                            if (ec.BaseEntity != null)
                            {
                                List<EntityConfiguration> entityConfigurations = null;
                                if (!_childEntitiesMap.TryGetValue(ec.BaseEntity, out entityConfigurations))
                                {
                                    entityConfigurations = new List<EntityConfiguration>();
                                    _childEntitiesMap.Add(ec.BaseEntity, entityConfigurations);
                                }

                                entityConfigurations.Add(ec);
                            }

                            //byteArrayColumns.Add(obj.Name, new List<string>());
                            //nullableNumberColumns.Add(obj.Name, new List<string>());
                            _sqlTypeWithDefaultValueColumns.Add(ec.Name, new List<string>());

                            List<string> lazyLoadProperties = new List<string>();
                            foreach (PropertyConfiguration pc in ec.Properties)
                            {
                                if (pc.IsQueryProperty && pc.IsLazyLoad)
                                {
                                    lazyLoadProperties.Add(pc.Name);
                                }

                                //if (pc.PropertyMappingColumnType == typeof(byte[]).ToString())
                                //{
                                //    byteArrayColumns[obj.Name].Add(pc.MappingName);
                                //}

                                //if (pc.PropertyMappingColumnType == typeof(int?).ToString() || pc.PropertyType == typeof(long?).ToString() || pc.PropertyType == typeof(short?).ToString() || pc.PropertyType == typeof(byte?).ToString() || pc.PropertyType == typeof(bool?).ToString() || pc.PropertyType == typeof(decimal?).ToString() || pc.PropertyType == typeof(float?).ToString() || pc.PropertyType == typeof(double?).ToString())
                                //{
                                //    nullableNumberColumns[obj.Name].Add(pc.MappingName);
                                //}

                                if (pc.SqlDefaultValue != null)
                                {
                                    _sqlTypeWithDefaultValueColumns[ec.Name].Add(pc.MappingName);
                                }
                            }
                            _isLazyLoadMap.Add(ec.Name, lazyLoadProperties);

                            foreach (PropertyConfiguration pc in ec.Properties)
                            {
                                if (pc.IsReadOnly && pc.IsPrimaryKey && (pc.DbType == System.Data.DbType.Int16 || pc.DbType == System.Data.DbType.Int32 || pc.DbType == System.Data.DbType.Int64))
                                {
                                    _autoIdEntities.Add(ec.Name, pc.Name);
                                    break;
                                }
                            }

                        }
                    }

                }
            }
        }

        public static void ParseNonRelatedEntities()
        {
            foreach (EntityConfiguration ec in _entities)
            {
                if (!_nonRelatedEntities.Contains(ec.Name))
                {
                    lock (_nonRelatedEntities)     //大锁
                    {
                        if (!_nonRelatedEntities.Contains(ec.Name))
                        {
                            bool isNonRelatedEntity = true;

                            if (ec.BaseEntity == null && GetChildEntityConfigurations(ec.Name).Count == 0)
                            {
                                foreach (PropertyConfiguration pc in ec.Properties)
                                {
                                    if (pc.IsQueryProperty && (pc.IsContained || pc.QueryType == "ManyToManyQuery"))
                                    {
                                        isNonRelatedEntity = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                isNonRelatedEntity = false;
                            }

                            if (isNonRelatedEntity)
                            {
                                _nonRelatedEntities.Add(ec.Name);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the entity configuration.
        /// </summary>
        /// <param name="typeFullName">Name of the type.</param>
        /// <returns>The entity configuration</returns>
        public static EntityConfiguration GetEntityConfiguration(string typeFullName)
        {

            if (_entities != null)
            {
                foreach (EntityConfiguration item in _entities)
                {
                    if (item.Name == typeFullName)
                    {
                        return item;
                    }
                }
            }

            Type type = Util.GetType(typeFullName);
            EntityConfiguration embeddedEc = GetEmbeddedEntityConfigurationsFromEntityType(type);
            if (embeddedEc != null)
            {
                LoadEmbeddedEntityConfigurationsFromLoadedEntities();
                ParseNonRelatedEntities();
            }

            //check again
            foreach (EntityConfiguration item in _entities)
            {
                if (item.Name == typeFullName)
                {
                    return item;
                }
            }

            throw new CouldNotFoundEntityConfigurationOfEntityException(typeFullName);
        }

        /// <summary>
        /// Gets the child entity configurations.
        /// </summary>
        /// <param name="baseTypeName">Name of the base type.</param>
        /// <returns>The entity configurations.</returns>
        public static List<EntityConfiguration> GetChildEntityConfigurations(string baseTypeName)
        {
            Check.Require(baseTypeName != null, "baseTypeName could not be null.");

            List<EntityConfiguration> entityConfigurations = null;
            if (!_childEntitiesMap.TryGetValue(baseTypeName, out entityConfigurations))
            {
                entityConfigurations = new List<EntityConfiguration>();
            }

            return entityConfigurations;
        }

        /// <summary>
        /// to tell whether the entityconfiguration named "typeName" has been existed in entities
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool isExistEntityConfiguration(string typeName)
        {
            if (_entities != null)
            {
                foreach (EntityConfiguration item in _entities)
                {
                    if (item.Name == typeName)
                    {
                        return true;
                    }
                }
            }

            return false;    // entities is null  or the ec named typeName does not exist
        }

        /// <summary>
        /// Determines whether a specified property of an entity is lazyload.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// 	<c>true</c> if is lazy load ; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLazyLoad(string entityName, string propertyName)
        {
            return _isLazyLoadMap[entityName].Contains(propertyName);
        }

        /// <summary>
        /// Determines whether the specified entity is non related entity, a non related entity is an entity without base/child entities and related contained query properties.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <returns>Wthether true.</returns>
        public static bool IsNonRelatedEntity(string entityName)
        {
            return _nonRelatedEntities.Contains(entityName);
        }

        /// <summary>
        /// Gets name of the entity's auto id column.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <returns>The auto id column name.</returns>
        public static string GetEntityAutoId(string entityName)
        {
            string autoID = null;
            _autoIdEntities.TryGetValue(entityName, out autoID);
            return autoID;
        }

        /// <summary>
        /// Gets the sqltype with default value columns.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <returns></returns>
        public static List<string> GetSqlTypeWithDefaultValueColumns(string entityName)
        {
            return _sqlTypeWithDefaultValueColumns[entityName];
        }
    }

    #region Meta data configuration

    /// <summary>
    /// The entity configuration section.
    /// </summary>
    public class EntityConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Whether encrpyted entity config file.
        /// </summary>
        [ConfigurationProperty("encrpyt")]
        public bool Encrpyt
        {
            get { return this["encrpyt"] == null ? false : (bool)this["encrpyt"]; }
            set { this["encrpyt"] = value; }
        }

        /// <summary>
        /// The encrpyt/decrypt key.
        /// </summary>
        [ConfigurationProperty("key")]
        public string Key
        {
            get { return this["key"] == null ? CryptographyManager.DEFAULT_KEY : (string)this["key"]; }
            set { this["key"] = value; }
        }

        /// <summary>
        /// Gets or sets the includes.
        /// </summary>
        /// <value>The includes.</value>
        [ConfigurationProperty("includes", IsRequired = true, IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(KeyValueConfigurationCollection))]
        public KeyValueConfigurationCollection Includes
        {
            get
            {
                return (KeyValueConfigurationCollection)this["includes"];
            }
            set
            {
                this["includes"] = value;
            }
        }
    }

    /// <summary>
    /// An entity configuration
    /// </summary>
    [Serializable]
    public class EntityConfiguration
    {
        private string RemoveTypePrefix(string typeName)
        {
            string name = typeName;
            while (name.Contains("."))
            {
                name = name.Substring(name.IndexOf(".")).TrimStart('.');
            }
            return name;
        }

        private string _name;

        /// <summary>
        /// Name of entity.
        /// </summary>
        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private int _batchSize;

        private string _mappingName;

        /// <summary>
        /// Gets or sets the name of the mapping.
        /// </summary>
        /// <value>The name of the mapping.</value>
        [XmlAttribute("mappingName")]
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

        private string _comment;

        /// <summary>
        /// _comment of entity
        /// </summary>
        [XmlAttribute("comment")]
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }


        /// <summary>
        /// Whether the entity is readonly.
        /// </summary>
        private bool _isReadOnly;

        /// <summary>
        /// Whether the entity is readonly.
        /// </summary>
        [XmlAttribute("isReadOnly")]
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }

        private bool _isBatchUpdate;

        /// <summary>
        /// Whether the entity is save all property related values in a batch to improve performance.
        /// </summary>
        [XmlAttribute("isBatchUpdate")]
        public bool IsBatchUpdate
        {
            get { return _isBatchUpdate; }
            set { _isBatchUpdate = value; }
        }

        private bool _isAutoPreLoad;

        /// <summary>
        /// Whether instances of the entity are automatically preloaded.
        /// </summary>
        [XmlAttribute("isAutoPreLoad")]
        public bool IsAutoPreLoad
        {
            get { return _isAutoPreLoad; }
            set { _isAutoPreLoad = value; }
        }


        /// <summary>
        /// Gets the name of the view to select data.
        /// </summary>
        /// <value>The name of the view.</value>
        public string ViewName
        {
            get
            {
                //return (_baseEntityName == null ? MappingName : "v" + MappingName);
                return MappingName;
            }
        }

        /// <summary>
        /// The batch size when an entity is marked as _isBatchUpdate.
        /// </summary>
        [XmlAttribute("batchSize")]
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
        /// Whether the entity is a relation type.
        /// </summary>
        private bool _isRelation;

        /// <summary>
        /// Whether the entity is a relation type.
        /// </summary>
        [XmlAttribute("isRelation")]
        public bool IsRelation
        {
            get { return _isRelation; }
            set { _isRelation = value; }
        }

        private string _baseEntity;

        /// <summary>
        /// Base entity of this entity.
        /// </summary>
        [XmlAttribute("baseType")]
        public string BaseEntity
        {
            get { return _baseEntity; }
            set { _baseEntity = value; }
        }


        private string _customData;

        /// <summary>
        /// Custom data.
        /// </summary>
        [XmlAttribute("customData")]
        public string CustomData
        {
            get { return _customData; }
            set { _customData = value; }
        }

        private string _additionalSqlScript;

        /// <summary>
        /// Combined additional sql script clips which will be included into the sql script batch.
        /// </summary>
        [XmlAttribute("additionalSqlScript")]
        public string AdditionalSqlScript
        {
            get { return _additionalSqlScript; }
            set { _additionalSqlScript = value; }
        }

        private List<PropertyConfiguration> _properties = new List<PropertyConfiguration>();

        /// <summary>
        /// Gets or sets the properties configuration.
        /// </summary>
        /// <value>The properties.</value>
        [XmlArray("Properties"), XmlArrayItem("Property")]
        public PropertyConfiguration[] Properties
        {
            get
            {
                return _properties.ToArray();
            }
            set
            {
                _properties = new List<PropertyConfiguration>();
                if (value != null)
                {
                    foreach (PropertyConfiguration item in value)
                    {
                        _properties.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the property configuration.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The property configuration</returns>
        public PropertyConfiguration GetPropertyConfiguration(string propertyName)
        {
            foreach (PropertyConfiguration item in _properties)
            {
                if (item.Name == propertyName)
                {
                    return item;
                }
            }

            return null;
        }

        public List<PropertyConfiguration> GetPrimaryKeyProperties()
        {
            List<PropertyConfiguration> list = new List<PropertyConfiguration>();

            foreach (PropertyConfiguration item in _properties)
            {
                if (item.IsPrimaryKey)
                {
                    list.Add(item);
                }
            }

            //if (list.Count == 0)
            //{
            //    throw new ConfigurationErrorsException(string.Format("Entity - {0} must have at least one primary key property column!", this.Name));
            //}

            return list;
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(PropertyConfiguration item)
        {
            lock (_properties)
            {
                _properties.Add(item);
            }
        }

        /// <summary>
        /// Gets the mapping column pks.
        /// </summary>
        /// <param name="propertyNames">The property pks.</param>
        /// <returns>The column pks.</returns>
        public string[] GetMappingColumnNames(string[] propertyNames)
        {
            if (propertyNames == null)
            {
                return null;
            }

            string[] mappingColumnNames = new string[propertyNames.Length];

            for (int i = 0; i < propertyNames.Length; i++)
            {
                mappingColumnNames[i] = GetPropertyConfiguration(propertyNames[i]).MappingName;
            }

            return mappingColumnNames;
        }

        public System.Data.DbType[] GetMappingColumnTypes(string[] propertyNames)
        {
            if (propertyNames == null)
            {
                return null;
            }

            System.Data.DbType[] mappingColumnTypes = new System.Data.DbType[propertyNames.Length];

            for (int i = 0; i < propertyNames.Length; i++)
            {
                mappingColumnTypes[i] = GetPropertyConfiguration(propertyNames[i]).DbType;
            }

            return mappingColumnTypes;
        }

        /// <summary>
        /// Gets the name of the mapping column.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The column pks.</returns>
        public string GetMappingColumnName(string propertyName)
        {
            if (propertyName == null)
            {
                return null;
            }

            PropertyConfiguration pc = GetPropertyConfiguration(propertyName);
            if (pc != null)
            {
                return pc.MappingName;
            }
            else
            {
                return propertyName;
            }
        }

        public string[] GetAllSelectColumns()
        {
            Check.Require(!string.IsNullOrEmpty(this.MappingName), "tableAliasName could not be null!");

            List<string> list = new List<string>();

            for (int i = 0; i < this._properties.Count; ++i)
            {
                if (this._properties[i].IsQueryProperty && this._properties[i].QueryType != "FkReverseQuery")
                {
                    continue;
                }

                if (this._properties[i].IsInherited && this._properties[i].InheritEntityMappingName != null)
                {
                    list.Add(this._properties[i].InheritEntityMappingName + "." + this._properties[i].MappingName);
                }
                else
                {
                    list.Add(this.MappingName + "." + this._properties[i].MappingName);
                }
            }

            return list.ToArray();
        }
    }

    /// <summary>
    /// A property configuration.
    /// </summary>
    [Serializable]
    public class PropertyConfiguration
    {
        private string _mappingName;
        private string _sqlType;
        private string _propertyMappingColumnType;

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
        /// Name of the property.
        /// </summary>
        private string _name;

        /// <summary>
        /// Name of the property.
        /// </summary>
        [XmlAttribute("name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the name of the mapping.
        /// </summary>
        /// <value>The name of the mapping.</value>
        [XmlAttribute("mappingName")]
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
        /// _comment of entity
        /// </summary>
        private string _comment;

        /// <summary>
        /// _comment of entity
        /// </summary>
        [XmlAttribute("_comment")]
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// Type of the property.
        /// </summary>
        private string _propertyType;

        /// <summary>
        /// Type of the property.
        /// </summary>
        [XmlAttribute("type")]
        public string PropertyType
        {
            get { return _propertyType; }
            set { _propertyType = value; }
        }

        /// <summary>
        /// Gets or sets the type of the property mapping column.
        /// </summary>
        /// <value>The type of the property mapping column.</value>
        [XmlAttribute("mappingColumnType")]
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
        /// Whether the property is inherited from a base entity.
        /// </summary>
        private bool _isInherited = false;

        /// <summary>
        /// Whether the property is inherited from a base entity.
        /// </summary>
        [XmlAttribute("isInherited")]
        public bool IsInherited
        {
            get { return _isInherited; }
            set { _isInherited = value; }
        }

        private string _inheritEntityMappingName = null;

        [XmlAttribute("inheritEntityMappingName")]
        public string InheritEntityMappingName
        {
            get { return _inheritEntityMappingName; }
            set { _inheritEntityMappingName = value; }
        }


        /// <summary>
        /// Gets or sets the mapping sql type.
        /// </summary>
        /// <value>The type of the SQL.</value>
        [XmlAttribute("sqlType")]
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

        /// <summary>
        /// The sql default value
        /// </summary>
        private string _sqlDefaultValue;

        /// <summary>
        /// The sql default value
        /// </summary>
        [XmlAttribute("sqlDefaultValue")]
        public string SqlDefaultValue
        {
            get { return _sqlDefaultValue; }
            set { _sqlDefaultValue = value; }
        }


        //private bool _isCompoundUnit;

        ///// <summary>
        ///// Whether the property is a CompoundUnit.
        ///// </summary>
        //[XmlAttribute("isCompoundUnit")]
        //public bool IsCompoundUnit
        //{
        //    get { return _isCompoundUnit; }
        //    set { _isCompoundUnit = value; }
        //}

        private bool _isContained;

        /// <summary>
        /// Whether the property is a contained property, which means if the entity saved or deleted, all contained property will be included in update object list.
        /// </summary>
        [XmlAttribute("isContained")]
        public bool IsContained
        {
            get { return _isContained; }
            set { _isContained = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is primary DEFAULT_KEY.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is primary DEFAULT_KEY; otherwise, <c>false</c>.
        /// </value>
        private bool _isPrimaryKey;

        [XmlAttribute("isPrimaryKey")]
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
        /// 是否是查询属性
        /// </summary>
        private bool _isQueryProperty;

        /// <summary>
        /// 是否是查询属性
        /// </summary>
        [XmlAttribute("isQuery")]
        public bool IsQueryProperty
        {
            get { return _isQueryProperty; }
            set { _isQueryProperty = value; }
        }

        /// <summary>
        /// Whether the property is readonly.
        /// </summary>
        private bool _isReadOnly;

        /// <summary>
        /// Whether the property is readonly.
        /// </summary>
        [XmlAttribute("isReadOnly")]
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }

        /// <summary>
        /// Whether a property is a friend DEFAULT_KEY.
        /// </summary>
        private bool _isFriendKey;

        [XmlAttribute("isFriendKey")]
        public bool IsFriendKey
        {
            get { return _isFriendKey; }
            set { _isFriendKey = value; }
        }

        /// <summary>
        /// Whether need to add index for the property when creating the table in database.
        /// </summary>
        private bool _isIndexProperty;

        /// <summary>
        /// Whether need to add index for the property when creating the table in database.
        /// </summary>
        [XmlAttribute("isIndexProperty")]
        public bool IsIndexProperty
        {
            get { return _isIndexProperty; }
            set { _isIndexProperty = value; }
        }

        /// <summary>
        /// Whether the index property is desc.
        /// </summary>
        private bool _isIndexPropertyDesc;

        /// <summary>
        /// Whether the index property is desc.
        /// </summary>
        [XmlAttribute("isIndexPropertyDesc")]
        public bool IsIndexPropertyDesc
        {
            get { return _isIndexPropertyDesc; }
            set { _isIndexPropertyDesc = value; }
        }


        /// <summary>
        /// Whether the property is a lazyload query property. It is only used by query entity.
        /// </summary>
        private bool _isLazyLoad;

        /// <summary>
        /// Whether the property is a lazyload query property. It is only used by query entity.
        /// </summary>
        [XmlAttribute("isLazyLoad")]
        public bool IsLazyLoad
        {
            get { return _isLazyLoad; }
            set { _isLazyLoad = value; }
        }

        /// <summary>
        /// whether the property could not be NULL.
        /// </summary>
        private bool _isNotNull;

        /// <summary>
        /// whether the property could not be NULL.
        /// </summary>
        [XmlAttribute("isNotNull")]
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
        /// The where condition used by query property.
        /// </summary>
        private string _queryWhere;

        /// <summary>
        /// The where condition used by query property.
        /// </summary>
        [XmlAttribute("queryWhere")]
        public string QueryWhere
        {
            get { return _queryWhere; }
            set { _queryWhere = value; }
        }

        /// <summary>
        /// The type of the query property.
        /// </summary>
        private string _queryType;

        /// <summary>
        /// The type of the query property.
        /// </summary>
        [XmlAttribute("queryType")]
        public string QueryType
        {
            get { return _queryType; }
            set { _queryType = value; }
        }

        /// <summary>
        /// The order by condition used by query property.
        /// </summary>
        private string _queryOrderBy;

        /// <summary>
        /// The order by condition used by query property.
        /// </summary>
        [XmlAttribute("queryOrderBy")]
        public string QueryOrderBy
        {
            get { return _queryOrderBy; }
            set { _queryOrderBy = value; }
        }

        /// <summary>
        /// Whether the property is a relationkey. It is only used by relation entity.
        /// </summary>
        private bool _isRelationKey;

        /// <summary>
        /// Whether the property is a relationkey. It is only used by relation entity.
        /// </summary>
        [XmlAttribute("isRelationKey")]
        public bool IsRelationKey
        {
            get { return _isRelationKey; }
            set { _isRelationKey = value; }
        }

        /// <summary>
        /// 关联类型
        /// </summary>
        private string _relatedType;

        /// <summary>
        /// 关联类型
        /// </summary>
        [XmlAttribute("relatedType")]
        public string RelatedType
        {
            get { return _relatedType; }
            set { _relatedType = value; }
        }

        /// <summary>
        /// The relation type of the query property.
        /// </summary>
        private string _relationType;

        /// <summary>
        /// The relation type of the query property.
        /// </summary>
        [XmlAttribute("relationType")]
        public string RelationType
        {
            get { return _relationType; }
            set { _relationType = value; }
        }

        /// <summary>
        /// The related entity type's foreignkey relating to this relationkey. It is only used by relation entity.
        /// </summary>
        private string _relatedForeignKey;

        /// <summary>
        /// The related entity type's foreignkey relating to this relationkey. It is only used by relation entity.
        /// </summary>
        [XmlAttribute("relatedForeignKey")]
        public string RelatedForeignKey
        {
            get { return _relatedForeignKey; }
            set { _relatedForeignKey = value; }
        }

        /// <summary>
        /// Whether this property should not included in default XML serialization.
        /// </summary>
        private bool _isSerializationIgnore;

        /// <summary>
        /// Whether this property should not included in default XML serialization.
        /// </summary>
        [XmlAttribute("isSerializationIgnore")]
        public bool IsSerializationIgnore
        {
            get { return _isSerializationIgnore; }
            set { _isSerializationIgnore = value; }
        }

        private string _customData;

        /// <summary>
        /// Custom data.
        /// </summary>
        [XmlAttribute("customData")]
        public string CustomData
        {
            get { return _customData; }
            set { _customData = value; }
        }
    }

    #endregion

    #region Configuration Attributes

    public class EmbeddedEntityConfigurationAttribute : Attribute
    {
        private string content;

        public string Content
        {
            get
            {
                return content;
            }
        }

        public EmbeddedEntityConfigurationAttribute(string configContent)
        {
            this.content = configContent;
        }
    }

    #endregion
}
