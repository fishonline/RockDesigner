using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using System.Reflection;
using System.Text;
using Rock.Orm.Common.Design;

namespace Rock.Orm.Common
{
    /// <summary>
    /// The dynamic entity class
    /// </summary>
    [Serializable]
    public class DynEntity : MarshalByRefObject
    {
        private DynEntityType _entityType;

        private List<object> _normalPropertyValues = new List<object>();
        private List<object> _queryPropertyValues = new List<object>();
        private Dictionary<string, int> _normalPropertyNameIdMap = new Dictionary<string, int>();
        private Dictionary<string, int> _queryPropertyNameIdMap = new Dictionary<string, int>();
        private Dictionary<string, string> _fkPropertyNameNormalPropertyNameMap = new Dictionary<string, string>();
        private Dictionary<string, object> changedProperties = new Dictionary<string, object>();

        /// <summary>
        /// 实体类型
        /// </summary>
        public DynEntityType EntityType
        {
            get { return _entityType; }
        }

        #region Nested types

        /// <summary>
        /// The event arg used by PropertyChanged event.
        /// </summary>
        public class PropertyChangedEventArgs : EventArgs
        {
            private string propertyName;
            private object oldValue;
            private object newValue;

            /// <summary>
            /// Gets or sets the name of the property.
            /// </summary>
            /// <value>The name of the property.</value>
            public string PropertyName
            {
                get
                {
                    return propertyName;
                }
                set
                {
                    propertyName = value;
                }
            }

            /// <summary>
            /// Gets or sets the old value.
            /// </summary>
            /// <value>The old value.</value>
            public object OldValue
            {
                get
                {
                    return oldValue;
                }
                set
                {
                    oldValue = value;
                }
            }

            /// <summary>
            /// Gets or sets the new value.
            /// </summary>
            /// <value>The new value.</value>
            public object NewValue
            {
                get
                {
                    return newValue;
                }
                set
                {
                    newValue = value;
                }
            }
        }

        /// <summary>
        /// Delegate stands for a property changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public delegate void PropertyChangedHandler(DynEntity sender, PropertyChangedEventArgs args);

        #endregion

        #region Attach & Detach

        protected bool isAttached;

        /// <summary>
        /// Determines whether this instance is attached. Not attached means an entity is newly created, or the entity is already persisted.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is attached; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAttached()
        {
            return isAttached;
        }

        /// <summary>
        /// Not attached means an entity is newly created, or the entity is already persisted. 
        /// Set entity's is-attached status as true also means entity's PropertyChanged event SHOULD be raised on property changing.
        /// </summary>
        public void Attach()
        {
            ResetModifiedPropertyStates();
            isAttached = true;
        }

        /// <summary>
        /// Not attached means an entity is newly created, or the entity is already persisted. 
        /// Set entity's is-attached status as false also means entity's PropertyChanged event SHOULD NOT be raised on property changing.
        /// </summary>
        public void Detach()
        {
            isAttached = false;
        }

        #endregion

        #region Constructors      

        /// <summary>
        /// 基于实体类型构造对象
        /// </summary>
        /// <param name="entityType"></param>
        public DynEntity(DynEntityType entityType)
        {
            if (entityType == null)
                throw new ApplicationException("实体类型不能为空");

            this.PropertyChanged += new PropertyChangedHandler(Entity_PropertyChanged);
            isAttached = false;

            _entityType = entityType;

            int normalCnt = 0;
            int queryCnt = 0;
            foreach (DynPropertyConfiguration entityProperty in _entityType.GetProperties())
            {
                if (entityProperty.IsQueryProperty == true)
                {
                    if (entityProperty.IsArray)
                    {
                        _queryPropertyNameIdMap.Add("_" + entityProperty.Name, queryCnt++);
                        _queryPropertyValues.Add(null);
                    }
                    else
                    {
                        QueryDynAttribute qa = entityProperty.GetPropertyQueryAttribute();
                        DynQueryDescriber describer = new DynQueryDescriber(qa, entityProperty, entityProperty.PropertyOriginalEntityType, _entityType.Name);

                        _normalPropertyNameIdMap.Add("_" + entityProperty.Name + "_" + describer.RelatedForeignKey, normalCnt++);
                        _fkPropertyNameNormalPropertyNameMap.Add("_" + entityProperty.Name + "_" + describer.RelatedForeignKey, entityProperty.Name);
                        _normalPropertyValues.Add(null);

                        _queryPropertyNameIdMap.Add("_" + entityProperty.Name, queryCnt++);
                        _queryPropertyValues.Add(null);
                    }
                }
                else
                {
                    _normalPropertyNameIdMap.Add("_" + entityProperty.Name, normalCnt++);
                    _normalPropertyValues.Add(null);
                }
            }
        }

        /// <summary>
        /// 基于实体类型构造对象
        /// </summary>
        /// <param name="entityType"></param>
        public DynEntity(string typeName)
        {
            // 这里应该是从数据库中加载
            DynEntityType entityType = DynEntityTypeManager.GetEntityType(typeName);

            if (entityType == null)
                throw new ApplicationException("实体类型不能为空");

            this.PropertyChanged += new PropertyChangedHandler(Entity_PropertyChanged);
            isAttached = false;

            _entityType = entityType;

            int normalCnt = 0;
            int queryCnt = 0;
            foreach (DynPropertyConfiguration entityProperty in _entityType.GetProperties())
            {
                if (entityProperty.IsQueryProperty == true)
                {
                    if (entityProperty.IsArray)
                    {
                        _queryPropertyNameIdMap.Add("_" + entityProperty.Name, queryCnt++);
                        _queryPropertyValues.Add(null);
                    }
                    else
                    {
                        QueryDynAttribute qa = entityProperty.GetPropertyQueryAttribute();
                        DynQueryDescriber describer = new DynQueryDescriber(qa, entityProperty, entityProperty.PropertyOriginalEntityType, _entityType.Name);

                        _normalPropertyNameIdMap.Add("_" + entityProperty.Name + "_" + describer.RelatedForeignKey, normalCnt++);
                        _fkPropertyNameNormalPropertyNameMap.Add("_" + entityProperty.Name + "_" + describer.RelatedForeignKey, entityProperty.Name);
                        _normalPropertyValues.Add(null);

                        _queryPropertyNameIdMap.Add("_" + entityProperty.Name, queryCnt++);
                        _queryPropertyValues.Add(null);
                    }
                }
                else
                {
                    _normalPropertyNameIdMap.Add("_" + entityProperty.Name, normalCnt++);
                    _normalPropertyValues.Add(null);
                }
            }
        }        

        #endregion

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性</returns>
        public DynPropertyConfiguration GetEntityProperty(string propertyName)
        {
            return _entityType.GetProperty(propertyName);
        }

        public DynPropertyConfiguration GetEntityPropertyIncludeQueryProperty(string propertyName)
        {
            string newPropertyName = null;
            if (_fkPropertyNameNormalPropertyNameMap.TryGetValue("_" + propertyName, out newPropertyName))
            {
                propertyName = newPropertyName;
            }
            return _entityType.GetProperty(propertyName);
        }

        /// <summary>
        /// 是否包含属性
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>是否</returns>
        public bool ContainsProperty(string propertyName)
        {
            if (_normalPropertyNameIdMap.ContainsKey("_" + propertyName) || _queryPropertyNameIdMap.ContainsKey("_" + propertyName))
            {
                return true;
            }                
            else
            {
                return false;
            }               
        }

        /// <summary>
        /// Determines whether this entity is modified.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is modified; otherwise, <c>false</c>.
        /// </returns>
        public bool IsModified()
        {
            return (changedProperties.Count > 0 || toDeleteRelatedPropertyObjects.Count > 0);
        }

        /// <summary>
        /// Gets the modified properties of this entity, not including query properties and properties of base entity's.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The modified properties</returns>
        public Dictionary<string, object> GetModifiedProperties(DynEntityType entityType)
        {
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            if (string.IsNullOrEmpty(entityType.BaseEntityName))
            {
                return changedProperties;
            }

            List<string> toRemoveItems = new List<string>();
            foreach (string item in changedProperties.Keys)
            {
                PropertyConfiguration pc = ec.GetPropertyConfiguration(item);
                if (pc == null || pc.IsInherited || pc.IsPrimaryKey)
                {
                    toRemoveItems.Add(item);
                }
            }
            Dictionary<string, object> retProperties = new Dictionary<string, object>();
            foreach (string item in changedProperties.Keys)
            {
                if (!toRemoveItems.Contains(item))
                {
                    retProperties.Add(item, changedProperties[item]);
                }
            }
            return retProperties;
        }

        /// <summary>
        /// Gets all the modified properties in the entire entity hierachy.
        /// </summary>
        /// <returns>All the modified properties</returns>
        public Dictionary<string, object> GetModifiedProperties()
        {
            return changedProperties;
        }

        /// <summary>
        /// Sets the modified properties.
        /// </summary>
        /// <param name="changedProperties">The changed properties.</param>
        public void SetModifiedProperties(Dictionary<string, object> changedProperties)
        {
            if (changedProperties != null)
            {
                this.changedProperties = changedProperties;
            }
            else
            {
                changedProperties = new Dictionary<string, object>();
            }
        }

        private void Entity_PropertyChanged(DynEntity sender, DynEntity.PropertyChangedEventArgs args)
        {
            if (sender == this && sender.isAttached)
            {
                //if ((args.OldValue == null && args.NewValue != null) || (args.OldValue != null && (!args.OldValue.Equals(args.NewValue))))
                //{
                lock (changedProperties)
                {
                    changedProperties[args.PropertyName] = args.NewValue;
                }
                //}
            }
        }

        /// <summary>
        /// Resets the modified property states.
        /// </summary>
        public void ResetModifiedPropertyStates()
        {
            changedProperties.Clear();
            toDeleteRelatedPropertyObjects.Clear();
            toSaveRelatedPropertyObjects.Clear();
        }

        /// <summary>
        /// The property changed event.
        /// </summary>
        public event PropertyChangedHandler PropertyChanged;

        /// <summary>
        /// Called when property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldVal">The old val.</param>
        /// <param name="newVal">The new val.</param>
        protected void OnPropertyChanged(string propertyName, object oldVal, object newVal)
        {
            if (oldVal == newVal)
            {
                return;
            }

            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs();
                args.PropertyName = propertyName;
                args.OldValue = oldVal;
                args.NewValue = newVal;

                PropertyChanged(this, args);
            }
        }

        /// <summary>
        /// Sets all property as modified.
        /// </summary>
        public void SetAllPropertiesAsModified()
        {
            string[] columnNames = GetPropertyMappingColumnNames();
            object[] columnValues = GetPropertyValues();
            PropertyConfiguration[] pcs = MetaDataManager.GetEntityConfiguration(_entityType.FullName).Properties;

            lock (changedProperties)
            {
                changedProperties.Clear();

                int j = 0;
                for (int i = 0; i < columnNames.Length; i++)
                {
                    while (pcs[j].MappingName != columnNames[i])
                    {
                        j++;
                    }

                    if (!pcs[j].IsPrimaryKey)
                    {
                        changedProperties.Add(pcs[j].Name, columnValues[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Reloads all queries properties's value.
        /// </summary>
        /// <param name="includeLazyLoadQueries">if set to <c>true</c> [include lazy load queries].</param>
        public void ReloadQueries(bool includeLazyLoadQueries)
        {
            foreach (DynPropertyConfiguration item in _entityType.GetProperties())
            {
                if (item.IsQueryProperty)
                {
                    if (includeLazyLoadQueries || (!MetaDataManager.IsLazyLoad(_entityType.FullName, item.Name)))
                    {
                        _queryPropertyValues[_queryPropertyNameIdMap["_" + item.Name]] = QueryOne(new DynEntity(item.Name), item.Name, this);
                        //  _queryPropertyValues[_queryPropertyNameIdMap["_" + item.Name]] = QueryOne(new DynEntity(item.PropertyOriginalEntityType), item.Name, this);
                    }
                }
            }
        }

        #region GetPropertyMappingColumnNames

        private static void GuessPrimaryKey(EntityConfiguration ec, List<string> primaryKeys)
        {
            //check name = ID or GUID column first
            foreach (PropertyConfiguration pc in ec.Properties)
            {
                if (pc.MappingName.ToUpper() == "ID" || pc.MappingName.ToUpper() == "GUID")
                {
                    primaryKeys.Add(pc.MappingName);
                    return;
                }
            }

            //check the first ends with ID or Guid column
            foreach (PropertyConfiguration pc in ec.Properties)
            {
                if (pc.MappingName.ToUpper().EndsWith("ID") || pc.MappingName.ToUpper().EndsWith("GUID"))
                {
                    primaryKeys.Add(pc.MappingName);
                    return;
                }
            }

            //or threat the first column as DEFAULT_KEY column
            primaryKeys.Add(ec.Properties[0].MappingName);
        }

        /// <summary>
        /// Gets the primary key mapping column names.
        /// </summary>
        /// <param name="ec">The ec.</param>
        /// <returns></returns>
        public static string[] GetPrimaryKeyMappingColumnNames(EntityConfiguration ec)
        {
            List<string> primaryKeys = new List<string>();
            foreach (PropertyConfiguration pc in ec.Properties)
            {
                if (pc.IsPrimaryKey)
                {
                    primaryKeys.Add(pc.MappingName);
                }
            }

            if (primaryKeys.Count == 0)
            {
                //take the most possible column as a single primary DEFAULT_KEY
                GuessPrimaryKey(ec, primaryKeys);
            }

            return primaryKeys.ToArray();
        }

        /// <summary>
        /// Gets the property mapping column names.
        /// </summary>
        /// <param name="ec">The ec.</param>
        /// <returns>Column names</returns>
        public static string[] GetPropertyMappingColumnNames(EntityConfiguration ec)
        {
            List<string> columnNames = new List<string>();
            foreach (PropertyConfiguration pc in ec.Properties)
            {
                if ((!pc.IsQueryProperty) || (pc.QueryType == "FkReverseQuery"))
                {
                    columnNames.Add(pc.MappingName);
                }
            }

            return columnNames.ToArray();
        }

        /// <summary>
        /// Gets the create property mapping column names.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string[] GetCreatePropertyMappingColumnNames(DynEntityType entityType)
        {
            return GetCreatePropertyMappingColumnNames(MetaDataManager.GetEntityConfiguration(entityType.FullName));
        }

        /// <summary>
        /// Gets the create property mapping column names.
        /// </summary>
        /// <param name="ec">The ec.</param>
        /// <returns>Column names</returns>
        public static string[] GetCreatePropertyMappingColumnNames(EntityConfiguration ec)
        {
            List<string> insertColumnNames = new List<string>();
            foreach (PropertyConfiguration pc in ec.Properties)
            {
                if ((!(pc.IsReadOnly && ec.BaseEntity == null)) && (!pc.IsQueryProperty) && ((!pc.IsInherited) || pc.IsPrimaryKey))
                {
                    if (!insertColumnNames.Contains(pc.MappingName))
                    {
                        if (!insertColumnNames.Contains(pc.MappingName))
                        {
                            insertColumnNames.Add(pc.MappingName);
                        }       
                    }                   
                }
            }

            return insertColumnNames.ToArray();
        }

        public static DbType[] GetCreatePropertyMappingColumnTypes(DynEntityType entityType)
        {
            return GetCreatePropertyMappingColumnTypes(MetaDataManager.GetEntityConfiguration(entityType.FullName));
        }

        /// <summary>
        /// Gets the create property mapping column types.
        /// </summary>
        /// <param name="ec">The ec.</param>
        /// <returns></returns>
        public static DbType[] GetCreatePropertyMappingColumnTypes(EntityConfiguration ec)
        {
            List<DbType> insertColumnTypes = new List<DbType>();
            foreach (PropertyConfiguration pc in ec.Properties)
            {
                if ((!(pc.IsReadOnly && ec.BaseEntity == null)) && (!pc.IsQueryProperty) && ((!pc.IsInherited) || pc.IsPrimaryKey))
                {
                    insertColumnTypes.Add(pc.DbType);
                }
            }

            return insertColumnTypes.ToArray();
        }

        /// <summary>
        /// Gets the create property mapping column names.
        /// </summary>
        /// <returns>Column names</returns>
        public string[] GetPropertyMappingColumnNames()
        {
            return GetPropertyMappingColumnNames(MetaDataManager.GetEntityConfiguration(_entityType.FullName));
        }

        #endregion

        #region Get & Set PropertyValue

        /// <summary>
        /// Get the property value.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns>The value.</returns>
        public object GetPropertyValue(string propertyName)
        {
            string entityPropertyName = propertyName;

            bool isFKProperty = false;

            string newpropertyName = null;
            if (_fkPropertyNameNormalPropertyNameMap.TryGetValue("_" + propertyName, out newpropertyName))
            {
                entityPropertyName = newpropertyName;
                isFKProperty = true;
            }

            DynPropertyConfiguration entityProperty = GetEntityProperty(entityPropertyName);

            if (entityProperty.IsQueryProperty == true && isFKProperty == false)
            {
                if (entityProperty.IsArray)
                {
                    if (!IsQueryPropertyLoaded(propertyName))
                    {
                        DynEntityArrayList _al = new DynEntityArrayList();
                        _al.AddRange(Query(new DynEntity(entityProperty.PropertyOriginalEntityType), propertyName, this));
                        OnQueryPropertyChanged(propertyName, _queryPropertyValues[_queryPropertyNameIdMap["_" + propertyName]], _al);
                        _queryPropertyValues[_queryPropertyNameIdMap["_" + propertyName]] = _al;
                    }
                    if (_queryPropertyValues[_queryPropertyNameIdMap["_" + propertyName]] == null)
                    {
                        DynEntityArrayList _al = new DynEntityArrayList();
                        BindArrayListEventHandlers(propertyName, _al);
                        _queryPropertyValues[_queryPropertyNameIdMap["_" + propertyName]] = _al;
                    }
                    return _queryPropertyValues[_queryPropertyNameIdMap["_" + propertyName]];
                }
                else
                {
                    if (!IsQueryPropertyLoaded(propertyName))
                    {
                        _queryPropertyValues[_queryPropertyNameIdMap["_" + propertyName]] = QueryOne(new DynEntity(entityProperty.PropertyOriginalEntityType), propertyName, this);
                    }
                    return _queryPropertyValues[_queryPropertyNameIdMap["_" + propertyName]];
                }
            }
            else
            {
                if (_normalPropertyNameIdMap.ContainsKey("_" + propertyName))
                    return _normalPropertyValues[_normalPropertyNameIdMap["_" + propertyName]];
                else
                    return null;
            }
        }

        /// <summary>
        /// Set the property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void SetProportyValue(string propertyName, object propertyValue)
        {
            string entityPropertyName = propertyName;

            bool isFKProperty = false;

            string normalPropertyName = null;
            if (_fkPropertyNameNormalPropertyNameMap.TryGetValue("_" + propertyName, out normalPropertyName))
            {
                entityPropertyName = normalPropertyName;
                isFKProperty = true;
            }

            DynPropertyConfiguration entityProperty = GetEntityProperty(entityPropertyName);

            if (entityProperty.IsQueryProperty == true && isFKProperty == false)
            {
                if (entityProperty.IsArray)
                {
                    OnPropertyChanged(propertyName, _queryPropertyValues[_queryPropertyNameIdMap[propertyName]], propertyValue);

                    int index = 0;
                    if (_queryPropertyNameIdMap.TryGetValue("_" + propertyName, out index))
                    {
                        _queryPropertyValues[index] = propertyValue;
                    }
                }
                else
                {
                    QueryDynAttribute qa = entityProperty.GetPropertyQueryAttribute();
                    DynQueryDescriber describer = new DynQueryDescriber(qa, entityProperty, entityProperty.PropertyOriginalEntityType, _entityType.Name);

                    OnQueryOnePropertyChanged(propertyName, GetPropertyValue(propertyName), propertyValue);
                    _queryPropertyValues[_queryPropertyNameIdMap["_" + propertyName]] = propertyValue;
                    if (propertyValue == null)
                    {
                        //OnPropertyChanged(propertyName, _normalPropertyValues[_queryPropertyNameIdMap["_" + propertyName + "_" + describer.RelatedForeignKey]], null);
                        //_normalPropertyValues[_queryPropertyNameIdMap["_" + propertyName + "_" + describer.RelatedForeignKey]] = null;
                        OnPropertyChanged(propertyName, _normalPropertyValues[_queryPropertyNameIdMap["_" + propertyName]], null);
                        _normalPropertyValues[_queryPropertyNameIdMap["_" + propertyName]] = null;
                    }
                    else
                    {
                        //OnPropertyChanged(propertyName, _normalPropertyValues[_queryPropertyNameIdMap["_" + propertyName + "_" + describer.RelatedForeignKey]], (propertyValue as DynEntity).GetPropertyValue(describer.RelatedForeignKey));
                        //_normalPropertyValues[_queryPropertyNameIdMap["_" + propertyName + "_" + describer.RelatedForeignKey]] = (propertyValue as DynEntity).GetPropertyValue(describer.RelatedForeignKey);
                        OnPropertyChanged(propertyName, _normalPropertyValues[_queryPropertyNameIdMap["_" + propertyName]], (propertyValue as DynEntity).GetPropertyValue(describer.RelatedForeignKey));
                        _normalPropertyValues[_queryPropertyNameIdMap["_" + propertyName]] = (propertyValue as DynEntity).GetPropertyValue(describer.RelatedForeignKey);
                    }
                }
            }
            else
            {
                OnPropertyChanged(propertyName, GetPropertyValue(propertyName), propertyValue);

                int index = 0;
                if (_normalPropertyNameIdMap.TryGetValue("_" + propertyName, out index))
                {
                    _normalPropertyValues[index] = propertyValue;
                }
            }
        }

        /// <summary>
        /// 属性赋值取值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object this[string propertyName]
        {
            get { return GetPropertyValue(propertyName); }
            set { SetProportyValue(propertyName, value); }
        }

        #endregion

        #region Get & Set PropertyValues

        /// <summary>
        /// Gets the property values.
        /// </summary>
        /// <returns>The values.</returns>
        public object[] GetPropertyValues()
        {
            return _normalPropertyValues.ToArray();
        }

        /// <summary>
        /// Sets the property values.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public void SetPropertyValues(IDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (!reader.IsDBNull(i))
                    _normalPropertyValues[i] = reader[i];
            }
            ReloadQueries(false);
        }

        /// <summary>
        /// Sets the property values.
        /// </summary>
        /// <param name="row">The row.</param>
        public void SetPropertyValues(DataRow row)
        {
            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                if (!row.IsNull(i))
                {
                    _normalPropertyValues[i] = row[i];
                }
            }
            ReloadQueries(false);
        }

        /// <summary>
        /// Gets the property values.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="columnNames">The column names.</param>
        /// <returns>The values.</returns>
        public static object[] GetPropertyValues(object obj, params string[] columnNames)
        {
            Check.Require(obj != null, "obj could not be null.");
            Check.Require(obj is DynEntity, "obj must be an DynEntity.");
            Check.Require(columnNames != null, "columnNames could not be null.");
            Check.Require(columnNames.Length > 0, "columnNames's length could not be 0.");

            DynEntity entityObj = obj as DynEntity;

            string[] names = entityObj.GetPropertyMappingColumnNames();
            object[] values = entityObj.GetPropertyValues();
            List<object> lsValues = new List<object>();
            foreach (string item in columnNames)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i] == item)
                    {
                        lsValues.Add(values[i]);
                        break;
                    }
                }
            }

            return lsValues.ToArray();
        }

        /// <summary>
        /// Gets the primary DEFAULT_KEY values.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="obj">The obj.</param>
        /// <returns>The values.</returns>
        public static object[] GetPrimaryKeyValues(DynEntity obj)
        {
            Check.Require(obj != null, "obj could not be null.");

            return GetPropertyValues(obj, GetPrimaryKeyMappingColumnNames(MetaDataManager.GetEntityConfiguration(((DynEntity)obj).EntityType.FullName)));
        }

        /// <summary>
        /// Gets the create property values.
        /// </summary>
        /// <param name="entityType">The type.</param>
        /// <param name="obj">The obj.</param>
        /// <returns>The values.</returns>
        public static object[] GetCreatePropertyValues(DynEntityType entityType, DynEntity obj)
        {
            Check.Require(obj != null, "obj could not be null.");

            return GetPropertyValues(obj, GetCreatePropertyMappingColumnNames(entityType));
        }

        #endregion

        #region MarshalByRefObject

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"></see> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"></see> property.
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
        /// <PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure"/></PermissionSet>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region QueryProxy

        private List<string> queryLoadedProperties = new List<string>();

        /// <summary>
        /// Sets the property as loaded.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void SetPropertyLoaded(string propertyName)
        {
            lock (queryLoadedProperties)
            {
                if (!queryLoadedProperties.Contains(propertyName))
                {
                    queryLoadedProperties.Add(propertyName);
                }
            }
        }

        /// <summary>
        /// Determines whether query property is loaded.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// 	<c>true</c> if query property is loaded; otherwise, <c>false</c>.
        /// </returns>
        public bool IsQueryPropertyLoaded(string propertyName)
        {
            return (!isAttached) || queryLoadedProperties.Contains(propertyName);
        }

        /// <summary>
        /// The proxy to do actual query property loading.
        /// </summary>
        /// <param name="returnEntityType">return type of the query.</param>
        /// <param name="propertyName">related property name</param>
        /// <param name="where">where sql clip.</param>
        /// <param name="orderBy">order by  sql clip.</param>
        /// <param name="baseEntity">instance of the owner entity.</param>
        /// <returns>The query result.</returns>
        public delegate DynEntity[] QueryProxyHandler(DynEntity returnEntityType, string propertyName, string where, string orderBy, DynEntity baseEntity);

        internal QueryProxyHandler onQuery;

        /// <summary>
        /// Set the actual query proxy handler binded to this entity.
        /// </summary>
        /// <param name="onQuery">The on query.</param>
        public void SetQueryProxy(QueryProxyHandler onQuery)
        {
            this.onQuery = onQuery;
        }

        /// <summary>
        /// Queries array of the specified return entity type.
        /// </summary>
        /// <param name="returnEntityType">Type of the return entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="baseEntity">The base entity.</param>
        /// <returns>The query result.</returns>
        protected DynEntity[] Query(DynEntity returnEntityType, string propertyName, DynEntity baseEntity)
        {
            if (onQuery != null)
            {
                EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(baseEntity.EntityType.FullName);
                PropertyConfiguration pc = ec.GetPropertyConfiguration(propertyName);

                try
                {
                    return onQuery(returnEntityType, propertyName, pc.QueryWhere, pc.QueryOrderBy, baseEntity);
                }
                catch (Exception ex)
                {
                    onQuery = null;
                    throw ex;
                }
            }
            return null;
        }

        /// <summary>
        /// Queries a single entity instance.
        /// </summary>
        /// <param name="returnEntityType">Type of the return entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="baseEntity">The base entity.</param>
        /// <returns>The query result.</returns>
        protected object QueryOne(DynEntity returnEntityType, string propertyName, DynEntity baseEntity)
        {
            Array objs = (Array)Query(returnEntityType, propertyName, baseEntity);
            if (objs != null && objs.Length > 0)
            {
                return objs.GetValue(0);
            }
            return null;
        }

        #endregion

        #region Add & Remove Array BatchSize

        private Dictionary<string, List<object>> toDeleteRelatedPropertyObjects = new Dictionary<string, List<object>>();
        private Dictionary<string, List<object>> toSaveRelatedPropertyObjects = new Dictionary<string, List<object>>();

        /// <summary>
        /// Return the dictionary contains all the objects need to be cascade deleted when this object is deleted or saved.
        /// </summary>
        /// <returns>The dictionary contains all the objects need to be cascade deleted</returns>
        public Dictionary<string, List<object>> GetToDeleteRelatedPropertyObjects()
        {
            return toDeleteRelatedPropertyObjects;
        }

        /// <summary>
        /// Gets to save related property objects.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<object>> GetToSaveRelatedPropertyObjects()
        {
            return toSaveRelatedPropertyObjects;
        }

        /// <summary>
        /// Clears to delete related property objects.
        /// </summary>
        public void ClearToDeleteRelatedPropertyObjects()
        {
            toDeleteRelatedPropertyObjects.Clear();
        }

        /// <summary>
        /// Clears to save related property objects.
        /// </summary>
        public void ClearToSaveRelatedPropertyObjects()
        {
            toSaveRelatedPropertyObjects.Clear();
        }

        /// <summary>
        /// Called when query one property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected void OnQueryOnePropertyChanged(string propertyName, object oldValue, object newValue)
        {
            bool isLoadedBefore = IsQueryPropertyLoaded(propertyName);
            SetPropertyLoaded(propertyName);

            if (!isLoadedBefore)
            {
                return;
            }

            if (oldValue == newValue)
            {
                return;
            }

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(EntityType.FullName);
            PropertyConfiguration pc = ec.GetPropertyConfiguration(propertyName);

            if ((!(pc.IsContained || pc.QueryType == "ManyToManyQuery")))
            {
                return;
            }

            if (oldValue != null)
            {
                OnQueryPropertyItemRemove(propertyName, oldValue);
            }

            if (newValue != null)
            {
                OnQueryPropertyItemAdd(propertyName, newValue);
            }
        }

        /// <summary>
        /// Called when added item query property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="item">The item.</param>
        protected void OnQueryPropertyItemAdd(string propertyName, object item)
        {
            Check.Require(item != null, "item to add could not be null.");

            bool isLoadedBefore = IsQueryPropertyLoaded(propertyName);
            SetPropertyLoaded(propertyName);

            if (!isLoadedBefore)
            {
                return;
            }

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(EntityType.FullName);
            PropertyConfiguration pc = ec.GetPropertyConfiguration(propertyName);
            if ((!(pc.IsContained || pc.QueryType == "ManyToManyQuery")))
            {
                return;
            }

            bool isInDeleteList = false;

            lock (toDeleteRelatedPropertyObjects)
            {
                List<object> values = null;
                if (toDeleteRelatedPropertyObjects.TryGetValue(propertyName, out values) && toDeleteRelatedPropertyObjects[propertyName].Contains(item))
                {
                    values.Remove(item);
                    isInDeleteList = true;
                }
            }

            if (!isInDeleteList)
            {
                lock (toSaveRelatedPropertyObjects)
                {
                    List<object> values = null;
                    if (!toSaveRelatedPropertyObjects.TryGetValue(propertyName, out values))
                    {
                        values = new List<object>();
                        toSaveRelatedPropertyObjects.Add(propertyName, values);
                    }
                    if (!values.Contains(item))
                    {
                        values.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Called when removed item from query property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="item">The item.</param>
        protected void OnQueryPropertyItemRemove(string propertyName, object item)
        {
            Check.Require(item != null, "item to remove could not be null.");

            bool isLoadedBefore = IsQueryPropertyLoaded(propertyName);
            SetPropertyLoaded(propertyName);

            if (!isLoadedBefore)
            {
                return;
            }

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(EntityType.FullName);
            PropertyConfiguration pc = ec.GetPropertyConfiguration(propertyName);
            if ((!(pc.IsContained || pc.QueryType == "ManyToManyQuery")))
            {
                return;
            }

            bool isInSaveList = false;

            lock (toSaveRelatedPropertyObjects)
            {
                List<object> values = null;
                if (toSaveRelatedPropertyObjects.TryGetValue(propertyName, out values) && toSaveRelatedPropertyObjects[propertyName].Contains(item))
                {
                    values.Remove(item);
                    isInSaveList = true;
                }
            }

            if (!isInSaveList)
            {
                lock (toDeleteRelatedPropertyObjects)
                {
                    List<object> values = null;
                    if (!toDeleteRelatedPropertyObjects.TryGetValue(propertyName, out values))
                    {
                        values = new List<object>();
                        toDeleteRelatedPropertyObjects.Add(propertyName, values);
                    }

                    if (!values.Contains(item))
                    {
                        values.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Called when query property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValues">The old values.</param>
        /// <param name="newValues">The new values.</param>
        protected void OnQueryPropertyChanged(string propertyName, object oldValues, object newValues)
        {
            bool isLoadedBefore = IsQueryPropertyLoaded(propertyName);
            SetPropertyLoaded(propertyName);

            if (newValues != null)
            {
                BindArrayListEventHandlers(propertyName, newValues);
            }

            if (!isLoadedBefore)
            {
                return;
            }

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(EntityType.FullName);
            PropertyConfiguration pc = ec.GetPropertyConfiguration(propertyName);

            if ((!(pc.IsContained || pc.QueryType == "ManyToManyQuery")) || oldValues == newValues)
            {
                return;
            }

            if (oldValues != null)
            {
                foreach (object oldValue in (IEnumerable)oldValues)
                {
                    OnQueryPropertyItemRemove(propertyName, oldValue);
                }
            }

            if (newValues != null)
            {
                foreach (object newValue in (IEnumerable)newValues)
                {
                    OnQueryPropertyItemAdd(propertyName, newValue);
                }
            }
        }

        /// <summary>
        /// Binds the array list event handlers.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="newValues">The new values.</param>
        protected void BindArrayListEventHandlers(string propertyName, object newValues)
        {
            IDynEntityArrayList entityArrayList = (IDynEntityArrayList)newValues;
            entityArrayList.OnAddCallbackHandler = new DynEntityArrayItemChangeHandler(OnQueryPropertyItemAdd);
            entityArrayList.OnRemoveCallbackHandler = new DynEntityArrayItemChangeHandler(OnQueryPropertyItemRemove);
            entityArrayList.PropertyName = propertyName;
        }

        #endregion

        #region Entity to DataTable

        /// <summary>
        /// Convert an entity array to a data table.
        /// </summary>
        /// <param name="objs">The entity array.</param>
        /// <returns>The data table.</returns>
        public static DataTable EntityArrayToDataTable(DynEntityType entityType, DynEntity[] objs)
        {
            EntityConfiguration ec;

            if (objs != null && objs.Length > 0)
            {
                ec = MetaDataManager.GetEntityConfiguration(objs[0].EntityType.FullName);
            }
            else
            {
                ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);
            }

            DataTable table = new DataTable(ec.Name);

            foreach (PropertyConfiguration pc in ec.Properties)
            {
                if ((!pc.IsQueryProperty) || (pc.QueryType == "FkReverseQuery"))
                {
                    DataColumn column = new DataColumn(pc.QueryType == "FkReverseQuery" ? pc.MappingName : pc.Name, Util.GetType(pc.PropertyMappingColumnType.Replace("System.Nullable`1[", "").Replace("]", "")));
                    table.Columns.Add(column);
                }
            }

            if (objs != null && objs.Length > 0)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    object[] values = DynEntity.GetPropertyValues(objs[i], DynEntity.GetPropertyMappingColumnNames(MetaDataManager.GetEntityConfiguration(objs[0].EntityType.Name)));
                    DataRow row = table.NewRow();

                    int j = 0;
                    foreach (PropertyConfiguration pc in ec.Properties)
                    {
                        if ((!pc.IsQueryProperty) || (pc.QueryType == "FkReverseQuery"))
                        {
                            object value = (values[j] == null ? DBNull.Value : values[j]);
                            //if (pc.IsCompoundUnit)
                            //{
                            //    value = SerializationManager.Serialize(value);
                            //}
                            row[j] = value;
                        }

                        j++;
                    }

                    table.Rows.Add(row);
                }
            }
            table.AcceptChanges();
            return table;
        }

        /// <summary>
        /// Data table to entity array.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="setEntitiesAsModified">if set to <c>true</c> set entities as modified.</param>
        /// <returns></returns>
        public static DynEntity[] DataTableToEntityArray(DynEntityType entityType, DataTable dt, bool setEntitiesAsModified)
        {
            if (dt == null)
            {
                return null;
            }

            DynEntity[] objs = new DynEntity[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                objs[i] = new DynEntity(entityType);
                objs[i].SetPropertyValues(dt.Rows[i]);
                if (setEntitiesAsModified)
                {
                    objs[i].Attach();
                    objs[i].SetAllPropertiesAsModified();
                }
            }

            return objs;
        }

        #endregion

        #region Get Value Helper

        /// <summary>
        /// Gets the GUID.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        protected Guid GetGuid(IDataReader reader, int index)
        {
            if (reader.GetFieldType(index) == typeof(Guid))
            {
                return reader.GetGuid(index);
            }
            else
            {
                return new Guid(reader.GetValue(index).ToString());
            }
        }

        /// <summary>
        /// Gets the GUID.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        protected Guid GetGuid(DataRow row, int index)
        {
            if (row.Table.Columns[index].DataType == typeof(Guid))
            {
                return (Guid)row[index];
            }
            else
            {
                return new Guid(row[index].ToString());
            }
        }

        #endregion
    }
}