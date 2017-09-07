using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Transactions;
using System.Reflection;

using Rock.Orm.Common;
using Rock.Orm.Common.Caching;
using System.Collections;

namespace Rock.Orm.Data
{
    /// <summary>
    /// Type of a database.
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// Common SqlServer, including SQL Server 7.X, 8.X and 9.X
        /// </summary>
        SqlServer = 0,
        /// <summary>
        /// Access
        /// </summary>
        MsAccess = 1,
        /// <summary>
        /// Other
        /// </summary>
        Other = 2,
        /// <summary>
        /// SQL Server 9.X (2005) only
        /// </summary>
        SqlServer9 = 3,
        /// <summary>
        /// Oracle
        /// </summary>
        Oracle = 4,
        /// <summary>
        /// MySql
        /// </summary>
        MySql = 5,
        /// <summary>
        /// Sqlite
        /// </summary>
        Sqlite = 6
    }

    /// <summary>
    /// The data access gateway.
    /// </summary>
    public sealed class Gateway
    {
        #region Default Gateway

        /// <summary>
        /// Get the default gateway, which mapping to the default Database.
        /// </summary>
        public static Gateway Default;

        /// <summary>
        /// Sets the default database.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="connStr">The conn STR.</param>
        public static void SetDefaultDatabase(DatabaseType dt, string connStr)
        {
            if (dt == DatabaseType.Other)
            {
                throw new NotSupportedException("Please use \"SetDefaultDatabase(string assemblyName, string className, string connStr)\" for databases other than SqlServer, MsAccess, MySql or Oracle Database!");
            }

            DbProvider provider = CreateDbProvider(dt, connStr);

            Default = new Gateway(new Database(provider));
        }

        /// <summary>
        /// Creates the db provider.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="connStr">The conn STR.</param>
        /// <returns>The db provider.</returns>
        private static DbProvider CreateDbProvider(DatabaseType dt, string connStr)
        {
            DbProvider provider = null;
            if (dt == DatabaseType.SqlServer9)
            {
                provider = DbProviderFactory.CreateDbProvider(null, typeof(Rock.Orm.Data.SqlServer9.SqlDbProvider9).FullName, connStr);
            }
            else if (dt == DatabaseType.SqlServer)
            {
                provider = DbProviderFactory.CreateDbProvider(null, typeof(Rock.Orm.Data.SqlServer.SqlDbProvider).FullName, connStr);
            }
            else if (dt == DatabaseType.Oracle)
            {
                provider = DbProviderFactory.CreateDbProvider(null, typeof(Rock.Orm.Data.Oracle.OracleDbProvider).FullName, connStr);
            }
            else if (dt == DatabaseType.MySql)
            {
                provider = DbProviderFactory.CreateDbProvider(null, typeof(Rock.Orm.Data.MySql.MySqlDbProvider).FullName, connStr);
            }
            else  //Ms Access
            {
                provider = DbProviderFactory.CreateDbProvider(null, typeof(Rock.Orm.Data.MsAccess.AccessDbProvider).FullName, connStr);
            }
            return provider;
        }

        /// <summary>
        /// Sets the default database.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="connStr">The conn STR.</param>
        public static void SetDefaultDatabase(string assemblyName, string className, string connStr)
        {
            DbProvider provider = DbProviderFactory.CreateDbProvider(assemblyName, className, connStr);
            if (provider == null)
            {
                throw new NotSupportedException(string.Format("Cannot construct DbProvider by specified parameters: {0}, {1}, {2}",
                    assemblyName, className, connStr));
            }

            Default = new Gateway(new Database(provider));
        }

        /// <summary>
        /// Sets the default database.
        /// </summary>
        /// <param name="connStrName">Name of the conn STR.</param>
        public static void SetDefaultDatabase(string connStrName)
        {
            DbProvider provider = DbProviderFactory.CreateDbProvider(connStrName);
            if (provider == null)
            {
                throw new NotSupportedException(string.Format("Cannot construct DbProvider by specified ConnectionStringName: {0}", connStrName));
            }

            Default = new Gateway(new Database(provider));
        }

        #endregion

        #region Private Members

        private Database db;

        //private DbHelper dbHelper;

        internal Rock.Orm.Common.ISqlQueryFactory queryFactory;

        private void InitGateway(Database db)
        {
            Check.Require(db != null, "db could not be null!");

            this.db = db;
            //this.dbHelper = new DbHelper(db);
            this.queryFactory = db.DbProvider.QueryFactory;

            object cacheConfig = System.Configuration.ConfigurationManager.GetSection("cacheConfig");
            if (cacheConfig != null)
            {
                cacheConfigSection = (CacheConfigurationSection)cacheConfig;
                tableExpireSecondsMap = new Dictionary<string, int>();

                foreach (string key in cacheConfigSection.CachingTables.AllKeys)
                {
                    if (key.Contains("."))
                    {
                        string[] splittedKey = key.Split('.');
                        System.Configuration.ConnectionStringSettings connStr = System.Configuration.ConfigurationManager.ConnectionStrings[splittedKey[0].Trim()];
                        if (connStr != null)
                        {
                            int expireSeconds = CacheConfigurationSection.DEFAULT_EXPIRE_SECONDS;
                            try
                            {
                                expireSeconds = int.Parse(cacheConfigSection.CachingTables[key].Value);
                            }
                            catch
                            {
                            }

                            string tableName = splittedKey[1].ToLower().Trim();

                            tableExpireSecondsMap[tableName] = expireSeconds;
                        }
                    }
                }
            }
        }

        internal static MethodInfo GetGatewayMethodInfo(string signiture)
        {
            MethodInfo mi = null;
            foreach (MethodBase mb in typeof(Gateway).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (mb.ToString() == signiture)
                {
                    mi = (MethodInfo)mb;
                    break;
                }
            }
            return mi;
        }

        private static WhereClip BuildEqualWhereClip(EntityConfiguration ec, object[] values, List<PropertyConfiguration> properties)
        {
            WhereClip where = new WhereClip();

            for (int i = 0; i < properties.Count; i++)
            {
                Rock.Orm.Common.ExpressionFactory.AppendWhereAnd(
                    where,
                    Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(ec.ViewName + '.' + properties[i].MappingName, properties[i].DbType),
                    Rock.Orm.Common.QueryOperator.Equal,
                    Rock.Orm.Common.ExpressionFactory.CreateParameterExpression(properties[i].DbType, values[i])
                );
            }
            return where;
        }

        private object GetAggregateValueEx(Rock.Orm.Common.ExpressionClip expr, EntityConfiguration ec, WhereClip where)
        {
            return From(ec, ec.MappingName).Where(where).Select(expr).ToScalar();
        }

        private void DoCascadeUpdate(Entity obj, DbTransaction tran, EntityConfiguration ec, WhereClip where, Dictionary<string, object> modifiedProperties)
        {
            string[] columnNames = null;
            DbType[] columnTypes = null;
            object[] columnValues = null;

            if (modifiedProperties.Count > 0)
            {
                columnNames = ec.GetMappingColumnNames(new List<string>(modifiedProperties.Keys).ToArray());
                columnTypes = ec.GetMappingColumnTypes(new List<string>(modifiedProperties.Keys).ToArray());
                columnValues = new List<object>(modifiedProperties.Values).ToArray();
                Update(ec.MappingName, where, columnNames, columnTypes, columnValues, tran);
                RemovedUpdatedModifiedProperties(obj, modifiedProperties);
            }

            //update base entities
            if (ec.BaseEntity != null)
            {
                Type baseType = Util.GetType(ec.BaseEntity);
                while (baseType != null)
                {
                    EntityConfiguration baseEc = MetaDataManager.GetEntityConfiguration(baseType.FullName);

                    modifiedProperties = obj.GetModifiedProperties(baseType);
                    if (modifiedProperties.Count > 0)
                    {
                        columnNames = baseEc.GetMappingColumnNames(new List<string>(modifiedProperties.Keys).ToArray());
                        columnTypes = baseEc.GetMappingColumnTypes(new List<string>(modifiedProperties.Keys).ToArray());
                        columnValues = new List<object>(modifiedProperties.Values).ToArray();
                        Update(baseEc.MappingName, where, columnNames, columnTypes, columnValues, tran);
                        RemovedUpdatedModifiedProperties(obj, modifiedProperties);
                    }
                    if (baseEc.BaseEntity == null)
                    {
                        baseType = null;
                    }
                    else
                    {
                        baseType = Util.GetType(baseEc.BaseEntity);
                    }
                }
            }
        }

        private void DoCascadeUpdate(DynEntity obj, DbTransaction tran, EntityConfiguration ec, WhereClip where, Dictionary<string, object> modifiedProperties)
        {
            string[] columnNames = null;
            DbType[] columnTypes = null;
            object[] columnValues = null;

            if (modifiedProperties.Count > 0)
            {
                columnNames = ec.GetMappingColumnNames(new List<string>(modifiedProperties.Keys).ToArray());
                columnTypes = ec.GetMappingColumnTypes(new List<string>(modifiedProperties.Keys).ToArray());
                columnValues = new List<object>(modifiedProperties.Values).ToArray();
                Update(ec.MappingName, where, columnNames, columnTypes, columnValues, tran);
                RemovedUpdatedModifiedProperties(obj, modifiedProperties);
            }

            //update base entities
            if (ec.BaseEntity != null)
            {
                string baseTypeName = ec.BaseEntity.Substring(ec.BaseEntity.LastIndexOf(".") + 1);
                DynEntityType baseType = DynEntityTypeManager.GetEntityType(baseTypeName);
                while (baseType != null)
                {
                    EntityConfiguration baseEc = MetaDataManager.GetEntityConfiguration(baseType.FullName);

                    modifiedProperties = obj.GetModifiedProperties(baseType);
                    if (modifiedProperties.Count > 0)
                    {
                        columnNames = baseEc.GetMappingColumnNames(new List<string>(modifiedProperties.Keys).ToArray());
                        columnTypes = baseEc.GetMappingColumnTypes(new List<string>(modifiedProperties.Keys).ToArray());
                        columnValues = new List<object>(modifiedProperties.Values).ToArray();
                        Update(baseEc.MappingName, where, columnNames, columnTypes, columnValues, tran);
                        RemovedUpdatedModifiedProperties(obj, modifiedProperties);
                    }
                    if (baseEc.BaseEntity == null)
                    {
                        baseType = null;
                    }
                    else
                    {
                        string baseBaseTypeName = baseEc.BaseEntity.Substring(ec.BaseEntity.LastIndexOf(".") + 1);
                        baseType = DynEntityTypeManager.GetEntityType(baseBaseTypeName);
                    }
                }
            }
        }

        private void DoCascadeDelete(Entity obj, DbTransaction tran, EntityConfiguration ec, WhereClip where)
        {
            foreach (PropertyConfiguration pc in ec.Properties)
            {
                if (pc.IsQueryProperty && (pc.IsContained || pc.QueryType == "ManyToManyQuery"))
                {
                    Type propertyType = Util.GetType(pc.PropertyType);
                    if (typeof(IEntityArrayList).IsAssignableFrom(propertyType))
                    {
                        propertyType = ((IEntityArrayList)Activator.CreateInstance(propertyType)).GetArrayItemType();
                    }

                    EntityConfiguration propertyTypeEc = MetaDataManager.GetEntityConfiguration(propertyType.FullName);
                    PropertyInfo[] pis = Util.DeepGetProperties(obj.GetType());

                    object propertyValue = null;
                    foreach (PropertyInfo pi in pis)
                    {
                        if (pi.Name == pc.Name)
                        {
                            propertyValue = pi.GetValue(obj, null);
                            break;
                        }
                    }

                    MethodInfo deleteMethod = null;

                    if (propertyValue != null)
                    {
                        if (pc.IsContained && pc.QueryType == "FkQuery" && MetaDataManager.IsNonRelatedEntity(pc.RelatedType))
                        {
                            deleteMethod = GetGatewayMethodInfo("Void Delete[EntityType](Rock.Orm.Common.WhereClip, System.Data.Common.DbTransaction)");
                            deleteMethod.MakeGenericMethod(propertyType).Invoke(this, new object[] { new PropertyItem(pc.RelatedForeignKey, propertyTypeEc) == Entity.GetPrimaryKeyValues(obj)[0], tran });
                        }
                        else
                        {
                            if (pc.QueryType == "ManyToManyQuery")
                            {
                                Type relationType = Util.GetType(pc.RelationType);
                                EntityConfiguration relationEc = MetaDataManager.GetEntityConfiguration(pc.RelationType);

                                deleteMethod = GetGatewayMethodInfo("Void Delete[EntityType](Rock.Orm.Common.WhereClip, System.Data.Common.DbTransaction)");
                                PropertyConfiguration relationKeyPc = null;
                                foreach (PropertyConfiguration item in relationEc.Properties)
                                {
                                    if (item.RelatedType != null)
                                    {
                                        Type pcRelatedType = Util.GetType(item.RelatedType);
                                        if (pcRelatedType.IsAssignableFrom(obj.GetType()))
                                        {
                                            relationKeyPc = item;
                                            break;
                                        }
                                    }
                                }

                                WhereClip relationWhere = new WhereClip();
                                Rock.Orm.Common.ExpressionFactory.AppendWhereAnd(
                                    relationWhere,
                                    Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(relationEc.ViewName + '.' + relationKeyPc.MappingName, relationKeyPc.DbType),
                                    Rock.Orm.Common.QueryOperator.Equal,
                                    Rock.Orm.Common.ExpressionFactory.CreateParameterExpression(relationKeyPc.DbType, Entity.GetPrimaryKeyValues(obj)[0])
                                );

                                deleteMethod.MakeGenericMethod(relationType).Invoke(this, new object[] { relationWhere, tran });
                            }

                            if (pc.IsContained)
                            {
                                if (propertyValue is IEntityArrayList)
                                {
                                    foreach (object item in (IEnumerable)propertyValue)
                                    {
                                        Delete(((Entity)item).GetEntityConfiguration(), (Entity)item, tran);
                                    }
                                }
                                else
                                {
                                    Delete(((Entity)propertyValue).GetEntityConfiguration(), (Entity)propertyValue, tran);
                                }
                            }
                        }
                    }
                }
            }

            Delete(ec.MappingName, where, tran);

            if (ec.BaseEntity != null)
            {
                Type baseType = Util.GetType(ec.BaseEntity);
                while (baseType != null)
                {
                    EntityConfiguration baseEc = MetaDataManager.GetEntityConfiguration(baseType.FullName);

                    Delete(baseEc.MappingName, where, tran);

                    if (baseEc.BaseEntity == null)
                    {
                        baseType = null;
                    }
                    else
                    {
                        baseType = Util.GetType(baseEc.BaseEntity);
                    }
                }
            }
        }

        private void DoCascadeDelete(DynEntity obj, DbTransaction tran, EntityConfiguration ec, WhereClip where)
        {
            foreach (PropertyConfiguration pc in ec.Properties)
            {
                if (pc.IsQueryProperty && (pc.IsContained || pc.QueryType == "ManyToManyQuery"))
                {
                    Type propertyType = Util.GetType(pc.PropertyType);
                    if (typeof(IEntityArrayList).IsAssignableFrom(propertyType))
                    {
                        propertyType = ((IEntityArrayList)Activator.CreateInstance(propertyType)).GetArrayItemType();
                    }

                    EntityConfiguration propertyTypeEc = MetaDataManager.GetEntityConfiguration(propertyType.FullName);
                    PropertyInfo[] pis = Util.DeepGetProperties(obj.GetType());

                    object propertyValue = null;
                    foreach (PropertyInfo pi in pis)
                    {
                        if (pi.Name == pc.Name)
                        {
                            propertyValue = pi.GetValue(obj, null);
                            break;
                        }
                    }

                    MethodInfo deleteMethod = null;

                    if (propertyValue != null)
                    {
                        if (pc.IsContained && pc.QueryType == "FkQuery" && MetaDataManager.IsNonRelatedEntity(pc.RelatedType))
                        {
                            deleteMethod = GetGatewayMethodInfo("Void Delete[EntityType](Rock.Orm.Common.WhereClip, System.Data.Common.DbTransaction)");
                            deleteMethod.MakeGenericMethod(propertyType).Invoke(this, new object[] { new PropertyItem(pc.RelatedForeignKey, propertyTypeEc) == Entity.GetPrimaryKeyValues(obj)[0], tran });
                        }
                        else
                        {
                            if (pc.QueryType == "ManyToManyQuery")
                            {
                                Type relationType = Util.GetType(pc.RelationType);
                                EntityConfiguration relationEc = MetaDataManager.GetEntityConfiguration(pc.RelationType);

                                deleteMethod = GetGatewayMethodInfo("Void Delete[EntityType](Rock.Orm.Common.WhereClip, System.Data.Common.DbTransaction)");
                                PropertyConfiguration relationKeyPc = null;
                                foreach (PropertyConfiguration item in relationEc.Properties)
                                {
                                    if (item.RelatedType != null)
                                    {
                                        Type pcRelatedType = Util.GetType(item.RelatedType);
                                        if (pcRelatedType.IsAssignableFrom(obj.GetType()))
                                        {
                                            relationKeyPc = item;
                                            break;
                                        }
                                    }
                                }

                                WhereClip relationWhere = new WhereClip();
                                Rock.Orm.Common.ExpressionFactory.AppendWhereAnd(
                                    relationWhere,
                                    Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(relationEc.ViewName + '.' + relationKeyPc.MappingName, relationKeyPc.DbType),
                                    Rock.Orm.Common.QueryOperator.Equal,
                                    Rock.Orm.Common.ExpressionFactory.CreateParameterExpression(relationKeyPc.DbType, Entity.GetPrimaryKeyValues(obj)[0])
                                );

                                deleteMethod.MakeGenericMethod(relationType).Invoke(this, new object[] { relationWhere, tran });
                            }

                            if (pc.IsContained)
                            {
                                if (propertyValue is IEntityArrayList)
                                {
                                    foreach (object item in (IEnumerable)propertyValue)
                                    {
                                        Delete(((Entity)item).GetEntityConfiguration(), (Entity)item, tran);
                                    }
                                }
                                else
                                {
                                    Delete(((Entity)propertyValue).GetEntityConfiguration(), (Entity)propertyValue, tran);
                                }
                            }
                        }
                    }
                }
            }

            Delete(ec.MappingName, where, tran);

            if (ec.BaseEntity != null)
            {
                string baseTypeName = ec.BaseEntity.Substring(ec.BaseEntity.LastIndexOf(".") + 1);
                DynEntityType baseType = DynEntityTypeManager.GetEntityType(baseTypeName);
                while (baseType != null)
                {
                    EntityConfiguration baseEc = MetaDataManager.GetEntityConfiguration(baseType.FullName);

                    Delete(baseEc.MappingName, where, tran);

                    if (baseEc.BaseEntity == null)
                    {
                        baseType = null;
                    }
                    else
                    {
                        string baseBaseTypeName = baseEc.BaseEntity.Substring(ec.BaseEntity.LastIndexOf(".") + 1);
                        baseType = DynEntityTypeManager.GetEntityType(baseBaseTypeName);
                    }
                }
            }
        }

        private bool DeleteAsChildEntity(string typeName, object[] pkValues, DbTransaction tran)
        {
            bool deletedAsChildEntity = false;

            List<EntityConfiguration> childEntities = MetaDataManager.GetChildEntityConfigurations(typeName);
            foreach (EntityConfiguration ec in childEntities)
            {
                Type childType = Util.GetType(ec.Name);
                MethodInfo findMethod = GetGatewayMethodInfo("EntityType Find[EntityType](System.Object[])");
                Gateway findGateway = this;
                if (db.IsBatchConnection)
                {
                    findGateway = new Gateway(new Database(db.DbProvider));
                    findGateway.Db.OnLog += new LogHandler(db.WriteLog);
                }
                object childObj = findMethod.MakeGenericMethod(childType).Invoke(findGateway, new object[] { pkValues });
                if (childObj != null)
                {
                    if (MetaDataManager.GetChildEntityConfigurations(ec.Name).Count == 0)
                    {
                        try
                        {
                            Delete(ec, (Entity)childObj, tran);
                        }
                        catch
                        {
                        }

                        deletedAsChildEntity = true;
                    }
                    else
                    {
                        deletedAsChildEntity = DeleteAsChildEntity(ec.Name, pkValues, tran);
                    }
                    if (deletedAsChildEntity)
                    {
                        return true;
                    }
                }
            }

            return deletedAsChildEntity;
        }

        private bool DeleteAsChildDynEntity(string typeName, object[] pkValues, DbTransaction tran)
        {
            bool deletedAsChildEntity = false;

            List<EntityConfiguration> childEntities = MetaDataManager.GetChildEntityConfigurations(typeName);
            foreach (EntityConfiguration ec in childEntities)
            {
                Gateway findGateway = this;
                if (db.IsBatchConnection)
                {
                    findGateway = new Gateway(new Database(db.DbProvider));
                    findGateway.Db.OnLog += new LogHandler(db.WriteLog);
                }
                DynEntity childObj = findGateway.Find(ec.Name.Substring(ec.Name.LastIndexOf(".") + 1), pkValues);
                if (childObj != null)
                {
                    if (MetaDataManager.GetChildEntityConfigurations(ec.Name).Count == 0)
                    {
                        try
                        {
                            Delete(ec, childObj, tran);
                        }
                        catch
                        {
                        }

                        deletedAsChildEntity = true;
                    }
                    else
                    {
                        deletedAsChildEntity = DeleteAsChildDynEntity(ec.Name, pkValues, tran);
                    }
                    if (deletedAsChildEntity)
                    {
                        return true;
                    }
                }
            }

            return deletedAsChildEntity;
        }

        private int DoCascadeInsert(Entity obj, DbTransaction tran, EntityConfiguration ec, string keyColumn)
        {
            int retAutoID = 0;

            string[] columnNames = null;
            object[] columnValues = null;
            DbType[] columnTypes = null;
            List<string> sqlDefaultValueColumns = null;

            if (ec.BaseEntity != null)
            {
                Stack<EntityConfiguration> stackEc = new Stack<EntityConfiguration>();
                Stack<Type> stackBaseType = new Stack<Type>();

                Type baseType = Util.GetType(ec.BaseEntity);
                while (baseType != null)
                {
                    EntityConfiguration baseEc = MetaDataManager.GetEntityConfiguration(baseType.FullName);

                    stackEc.Push(baseEc);
                    stackBaseType.Push(baseType);

                    if (baseEc.BaseEntity == null)
                    {
                        baseType = null;
                    }
                    else
                    {
                        baseType = Util.GetType(baseEc.BaseEntity);
                    }
                }

                while (stackEc.Count > 0)
                {
                    EntityConfiguration ecToInsert = stackEc.Pop();
                    Type baseTypeToInsert = stackBaseType.Pop();

                    columnNames = Entity.GetCreatePropertyMappingColumnNames(baseTypeToInsert);
                    columnTypes = Entity.GetCreatePropertyMappingColumnTypes(baseTypeToInsert);
                    columnValues = Entity.GetCreatePropertyValues(baseTypeToInsert, obj);

                    sqlDefaultValueColumns = MetaDataManager.GetSqlTypeWithDefaultValueColumns(ecToInsert.Name);
                    if (sqlDefaultValueColumns.Count > 0)
                    {
                        FilterNullSqlDefaultValueColumns(sqlDefaultValueColumns, columnNames, columnTypes, columnValues, out columnNames, out columnTypes, out columnValues);
                    }

                    if (ecToInsert.BaseEntity == null)
                    {
                        //retAutoID = dbHelper.Insert(ecToInsert.MappingName, columnNames, columnTypes, columnValues, tran, keyColumn);
                        retAutoID = Insert(ecToInsert.MappingName, columnNames, columnTypes, columnValues, tran, keyColumn);

                        if (retAutoID > 0)
                        {
                            //save the retAutoID value to entity's ID property
                            string autoIdProperty = MetaDataManager.GetEntityAutoId(ec.Name);
                            if (autoIdProperty != null)
                            {
                                Util.DeepGetProperty(obj.GetType(), autoIdProperty).SetValue(obj, retAutoID, null);
                            }
                        }
                    }
                    else
                    {
                        //dbHelper.Insert(ecToInsert.MappingName, columnNames, columnTypes, columnValues, tran, keyColumn);
                        Insert(ecToInsert.MappingName, columnNames, columnTypes, columnValues, tran, null);
                    }
                }
            }

            columnNames = Entity.GetCreatePropertyMappingColumnNames(obj.GetEntityConfiguration());
            columnTypes = Entity.GetCreatePropertyMappingColumnTypes(obj.GetEntityConfiguration());
            columnValues = Entity.GetCreatePropertyValues(obj.GetType(), obj);

            sqlDefaultValueColumns = MetaDataManager.GetSqlTypeWithDefaultValueColumns(ec.Name);
            if (sqlDefaultValueColumns.Count > 0)
            {
                FilterNullSqlDefaultValueColumns(sqlDefaultValueColumns, columnNames, columnTypes, columnValues, out columnNames, out columnTypes, out columnValues);
            }

            if (retAutoID == 0)
            {
                //retAutoID = dbHelper.Insert(ec.MappingName, columnNames, columnTypes, columnValues, tran, keyColumn);
                retAutoID = Insert(ec.MappingName, columnNames, columnTypes, columnValues, tran, keyColumn);
            }
            else
            {
                //dbHelper.Insert(ec.MappingName, columnNames, columnTypes, columnValues, tran, keyColumn);
                Insert(ec.MappingName, columnNames, columnTypes, columnValues, tran, null);
            }

            if (ec.BaseEntity == null && retAutoID > 0)
            {
                //save the retAutoID value to entity's ID property
                string autoIdProperty = MetaDataManager.GetEntityAutoId(ec.Name);
                if (autoIdProperty != null)
                {
                    Util.DeepGetProperty(obj.GetType(), autoIdProperty).SetValue(obj, retAutoID, null);
                }
            }

            return retAutoID;
        }

        private int DoCascadeInsert(DynEntity obj, DbTransaction tran, EntityConfiguration ec, string keyColumn)
        {
            int retAutoID = 0;

            string[] columnNames = null;
            object[] columnValues = null;
            DbType[] columnTypes = null;
            List<string> sqlDefaultValueColumns = null;

            if (ec.BaseEntity != null)
            {
                Stack<EntityConfiguration> stackEc = new Stack<EntityConfiguration>();
                Stack<DynEntityType> stackBaseType = new Stack<DynEntityType>();

                string baseTypeName = ec.BaseEntity.Substring(ec.BaseEntity.LastIndexOf(".") + 1);
                DynEntityType baseType = DynEntityTypeManager.GetEntityType(baseTypeName);
                while (baseType != null)
                {
                    EntityConfiguration baseEc = MetaDataManager.GetEntityConfiguration(baseType.FullName);

                    stackEc.Push(baseEc);
                    stackBaseType.Push(baseType);

                    if (baseEc.BaseEntity == null)
                    {
                        baseType = null;
                    }
                    else
                    {
                        string baseBaseTypeName = baseEc.BaseEntity.Substring(ec.BaseEntity.LastIndexOf(".") + 1);
                        baseType = DynEntityTypeManager.GetEntityType(baseBaseTypeName);
                    }
                }

                while (stackEc.Count > 0)
                {
                    EntityConfiguration ecToInsert = stackEc.Pop();
                    DynEntityType baseTypeToInsert = stackBaseType.Pop();

                    columnNames = DynEntity.GetCreatePropertyMappingColumnNames(baseTypeToInsert);
                    columnTypes = DynEntity.GetCreatePropertyMappingColumnTypes(baseTypeToInsert);
                    columnValues = DynEntity.GetCreatePropertyValues(baseTypeToInsert, obj);

                    sqlDefaultValueColumns = MetaDataManager.GetSqlTypeWithDefaultValueColumns(ecToInsert.Name);
                    if (sqlDefaultValueColumns.Count > 0)
                    {
                        FilterNullSqlDefaultValueColumns(sqlDefaultValueColumns, columnNames, columnTypes, columnValues, out columnNames, out columnTypes, out columnValues);
                    }

                    if (ecToInsert.BaseEntity == null)
                    {
                        retAutoID = Insert(ecToInsert.MappingName, columnNames, columnTypes, columnValues, tran, keyColumn);

                        if (retAutoID > 0)
                        {
                            //save the retAutoID value to entity's ID property
                            string autoIdProperty = MetaDataManager.GetEntityAutoId(ec.Name);
                            if (autoIdProperty != null)
                            {
                                Util.DeepGetProperty(obj.GetType(), autoIdProperty).SetValue(obj, retAutoID, null);
                            }
                        }
                    }
                    else
                    {
                        Insert(ecToInsert.MappingName, columnNames, columnTypes, columnValues, tran, null);
                    }
                }
            }

            columnNames = DynEntity.GetCreatePropertyMappingColumnNames(obj.EntityType);
            columnTypes = DynEntity.GetCreatePropertyMappingColumnTypes(obj.EntityType);
            columnValues = DynEntity.GetCreatePropertyValues(obj.EntityType, obj);

            sqlDefaultValueColumns = MetaDataManager.GetSqlTypeWithDefaultValueColumns(ec.Name);
            if (sqlDefaultValueColumns.Count > 0)
            {
                FilterNullSqlDefaultValueColumns(sqlDefaultValueColumns, columnNames, columnTypes, columnValues, out columnNames, out columnTypes, out columnValues);
            }

            if (retAutoID == 0)
            {
                retAutoID = Insert(ec.MappingName, columnNames, columnTypes, columnValues, tran, keyColumn);
            }
            else
            {
                Insert(ec.MappingName, columnNames, columnTypes, columnValues, tran, null);
            }

            if (ec.BaseEntity == null && retAutoID > 0)
            {
                //save the retAutoID value to entity's ID property
                string autoIdProperty = MetaDataManager.GetEntityAutoId(ec.Name);
                if (autoIdProperty != null)
                {
                    Util.DeepGetProperty(obj.GetType(), autoIdProperty).SetValue(obj, retAutoID, null);
                }
            }

            return retAutoID;
        }

        private void DoCascadePropertySave(object obj, DbTransaction tran, EntityConfiguration ec)
        {
            foreach (PropertyInfo pi in Util.DeepGetProperties(obj.GetType()))
            {
                PropertyConfiguration pc = ec.GetPropertyConfiguration(pi.Name);
                if (pc != null && pc.IsQueryProperty && (pc.IsContained || pc.QueryType == "ManyToManyQuery") && (pc.QueryType == "ManyToManyQuery" || pc.QueryType == "FkQuery" || pc.QueryType == "PkQuery"))
                {
                    if (((Entity)obj).IsQueryPropertyLoaded(pi.Name))
                    {
                        object propertyValue = pi.GetValue(obj, null);
                        Dictionary<string, List<object>> toSaveRelatedObjectsAll = ((Entity)obj).GetToSaveRelatedPropertyObjects();
                        List<object> toSaveRelatedObjs = null;
                        toSaveRelatedObjectsAll.TryGetValue(pi.Name, out toSaveRelatedObjs);

                        if (propertyValue != null)
                        {
                            object ownerPkValue = null;
                            if (pc.RelationType == null)
                            {
                                ownerPkValue = Entity.GetPrimaryKeyValues(obj)[0];
                            }

                            if (typeof(IEntityArrayList).IsAssignableFrom(pi.PropertyType))
                            {
                                foreach (object item in (IEnumerable)propertyValue)
                                {
                                    if (pc.RelationType == null)
                                    {
                                        SetRelatedObjFriendKeyValue(pc, obj, ownerPkValue, item);
                                    }
                                    DoPropertySave(obj, toSaveRelatedObjs, item, pc, pc.RelationType, tran);
                                }
                            }
                            else
                            {
                                if (pc.RelationType == null)
                                {
                                    SetRelatedObjFriendKeyValue(pc, obj, ownerPkValue, propertyValue);
                                }
                                DoPropertySave(obj, toSaveRelatedObjs, propertyValue, pc, pc.RelationType, tran);
                            }
                        }
                    }
                }
            }

            ((Entity)obj).ClearToSaveRelatedPropertyObjects();
        }

        private void DoCascadePropertySave(DynEntity obj, DbTransaction tran, EntityConfiguration ec)
        {
            foreach (PropertyInfo pi in Util.DeepGetProperties(obj.GetType()))
            {
                PropertyConfiguration pc = ec.GetPropertyConfiguration(pi.Name);
                if (pc != null && pc.IsQueryProperty && (pc.IsContained || pc.QueryType == "ManyToManyQuery") && (pc.QueryType == "ManyToManyQuery" || pc.QueryType == "FkQuery" || pc.QueryType == "PkQuery"))
                {
                    if (((DynEntity)obj).IsQueryPropertyLoaded(pi.Name))
                    {
                        object propertyValue = pi.GetValue(obj, null);
                        Dictionary<string, List<object>> toSaveRelatedObjectsAll = obj.GetToSaveRelatedPropertyObjects();
                        List<object> toSaveRelatedObjs = null;
                        toSaveRelatedObjectsAll.TryGetValue(pi.Name, out toSaveRelatedObjs);

                        if (propertyValue != null)
                        {
                            object ownerPkValue = null;
                            if (pc.RelationType == null)
                            {
                                ownerPkValue = DynEntity.GetPrimaryKeyValues(obj)[0];
                            }

                            if (typeof(IDynEntityArrayList).IsAssignableFrom(pi.PropertyType))
                            {
                                foreach (DynEntity item in (IEnumerable)propertyValue)
                                {
                                    if (pc.RelationType == null)
                                    {
                                        SetRelatedObjFriendKeyValue(pc, obj, ownerPkValue, item);
                                    }
                                    DoPropertySave(obj, toSaveRelatedObjs, item, pc, pc.RelationType, tran);
                                }
                            }
                            else
                            {
                                if (pc.RelationType == null)
                                {
                                    SetRelatedObjFriendKeyValue(pc, obj, ownerPkValue, propertyValue);
                                }
                                DoPropertySave(obj, toSaveRelatedObjs, propertyValue, pc, pc.RelationType, tran);
                            }
                        }
                    }
                }
            }

            obj.ClearToSaveRelatedPropertyObjects();
        }

        private void SetRelatedObjFriendKeyValue(PropertyConfiguration pc, object obj, object objPkValue, object relatedObj)
        {
            if (pc.RelationType == null)
            {
                //set relatedobj's related key = ownerObj's id
                foreach (PropertyConfiguration item in ((Entity)relatedObj).GetEntityConfiguration().Properties)
                {
                    if (item.Name == pc.RelatedForeignKey)
                    {
                        if (item.IsQueryProperty)
                        {
                            if (item.QueryType == "FkReverseQuery")
                            {
                                Util.DeepGetField(relatedObj.GetType(), "_" + item.Name + "_" + item.RelatedForeignKey, false).SetValue(relatedObj, objPkValue);
                            }
                            else
                            {
                                Util.DeepGetProperty(relatedObj.GetType(), item.Name).SetValue(relatedObj, obj, null);
                            }
                        }
                        else
                        {
                            Util.DeepGetProperty(relatedObj.GetType(), item.Name).SetValue(relatedObj, objPkValue, null);
                        }
                        break;
                    }
                }
            }
        }

        private void DoPropertySave(object ownerObj, List<object> toSaveRelatedObjs, object relatedObj, PropertyConfiguration propertyConfig, string relationTypeName, DbTransaction tran)
        {
            if (relatedObj != null)
            {
                Type ownerType = ownerObj.GetType();
                Type relatedType = relatedObj.GetType();

                if (propertyConfig.IsContained)
                {
                    Save((Entity)relatedObj, tran);
                }

                if (relationTypeName != null && propertyConfig.QueryType == "ManyToManyQuery" && toSaveRelatedObjs != null && toSaveRelatedObjs.Contains(relatedObj))
                {
                    if (relationTypeName != relatedType.ToString())
                    {
                        //create relation relatedType instance
                        Type relationType = Util.GetType(relationTypeName);
                        EntityConfiguration relationEc = MetaDataManager.GetEntityConfiguration(relationTypeName);
                        object relationTypeInstance = Activator.CreateInstance(relationType);
                        foreach (PropertyConfiguration pc in relationEc.Properties)
                        {
                            if (pc.RelatedType != null)
                            {
                                Type pcRelatedType = Util.GetType(pc.RelatedType);
                                if (pcRelatedType.IsAssignableFrom(ownerType))
                                {
                                    relationType.GetProperty(pc.Name).SetValue(relationTypeInstance, Entity.GetPropertyValues(ownerObj, MetaDataManager.GetEntityConfiguration(pc.RelatedType).GetMappingColumnName(pc.RelatedForeignKey))[0], null);
                                }
                                else if (pcRelatedType.IsAssignableFrom(relatedType))
                                {
                                    relationType.GetProperty(pc.Name).SetValue(relationTypeInstance, Entity.GetPropertyValues(relatedObj, MetaDataManager.GetEntityConfiguration(pc.RelatedType).GetMappingColumnName(pc.RelatedForeignKey))[0], null);
                                }
                            }
                        }

                        if (((Entity)relatedObj).IsAttached())
                        {
                            Save((Entity)relationTypeInstance, tran);
                        }
                    }
                }
            }
        }

        private void DoPropertySave(DynEntity ownerObj, List<DynEntity> toSaveRelatedObjs, DynEntity relatedObj, PropertyConfiguration propertyConfig, string relationTypeName, DbTransaction tran)
        {
            if (relatedObj != null)
            {
                Type ownerType = ownerObj.GetType();
                Type relatedType = relatedObj.GetType();

                if (propertyConfig.IsContained)
                {
                    Save(relatedObj, tran);
                }

                if (relationTypeName != null && propertyConfig.QueryType == "ManyToManyQuery" && toSaveRelatedObjs != null && toSaveRelatedObjs.Contains(relatedObj))
                {
                    if (relationTypeName != relatedType.ToString())
                    {
                        //create relation relatedType instance
                        Type relationType = Util.GetType(relationTypeName);
                        EntityConfiguration relationEc = MetaDataManager.GetEntityConfiguration(relationTypeName);
                        object relationTypeInstance = Activator.CreateInstance(relationType);
                        foreach (PropertyConfiguration pc in relationEc.Properties)
                        {
                            if (pc.RelatedType != null)
                            {
                                Type pcRelatedType = Util.GetType(pc.RelatedType);
                                if (pcRelatedType.IsAssignableFrom(ownerType))
                                {
                                    relationType.GetProperty(pc.Name).SetValue(relationTypeInstance, DynEntity.GetPropertyValues(ownerObj, MetaDataManager.GetEntityConfiguration(pc.RelatedType).GetMappingColumnName(pc.RelatedForeignKey))[0], null);
                                }
                                else if (pcRelatedType.IsAssignableFrom(relatedType))
                                {
                                    relationType.GetProperty(pc.Name).SetValue(relationTypeInstance, DynEntity.GetPropertyValues(relatedObj, MetaDataManager.GetEntityConfiguration(pc.RelatedType).GetMappingColumnName(pc.RelatedForeignKey))[0], null);
                                }
                            }
                        }

                        if (relatedObj.IsAttached())
                        {
                            Save((Entity)relationTypeInstance, tran);
                        }
                    }
                }
            }
        }

        private void DoDeleteToDeleteObjects(Entity obj, DbTransaction tran)
        {
            if (obj.GetToDeleteRelatedPropertyObjects() != null)
            {
                foreach (string propertyName in obj.GetToDeleteRelatedPropertyObjects().Keys)
                {
                    //create relation relatedType instance
                    EntityConfiguration ec = obj.GetEntityConfiguration();
                    PropertyConfiguration propertyConfig = ec.GetPropertyConfiguration(propertyName);
                    string relationTypeName = propertyConfig.RelationType;
                    Type relationType = null;
                    EntityConfiguration relationEc = null;
                    object relationTypeInstance = null;
                    if (relationTypeName != null)
                    {
                        relationType = Util.GetType(relationTypeName);
                        relationEc = MetaDataManager.GetEntityConfiguration(relationTypeName);
                        relationTypeInstance = Activator.CreateInstance(relationType);
                        foreach (PropertyConfiguration pc in relationEc.Properties)
                        {
                            if (pc.RelatedType != null)
                            {
                                Type pcRelatedType = Util.GetType(pc.RelatedType);
                                if (pcRelatedType.IsAssignableFrom(obj.GetType()))
                                {
                                    relationType.GetProperty(pc.Name).SetValue(relationTypeInstance, Entity.GetPropertyValues(obj, MetaDataManager.GetEntityConfiguration(pc.RelatedType).GetMappingColumnName(pc.RelatedForeignKey))[0], null);
                                }
                            }
                        }
                    }

                    foreach (object item in obj.GetToDeleteRelatedPropertyObjects()[propertyName])
                    {
                        if (item != null && item is Entity)
                        {
                            MethodInfo deleteMethod = GetGatewayMethodInfo("Void Delete[EntityType](EntityType, System.Data.Common.DbTransaction)");

                            if (relationTypeName != null)
                            {
                                //delete relationTypeInstance
                                foreach (PropertyConfiguration pc in relationEc.Properties)
                                {
                                    if (pc.RelatedType != null)
                                    {
                                        Type pcRelatedType = Util.GetType(pc.RelatedType);
                                        if (pcRelatedType.IsAssignableFrom(item.GetType()))
                                        {
                                            relationType.GetProperty(pc.Name).SetValue(relationTypeInstance, Entity.GetPropertyValues(item, MetaDataManager.GetEntityConfiguration(pc.RelatedType).GetMappingColumnName(pc.RelatedForeignKey))[0], null);
                                        }
                                    }
                                }
                                Delete(((Entity)relationTypeInstance).GetEntityConfiguration(), (Entity)relationTypeInstance, tran);
                            }

                            if (propertyConfig.IsContained)
                            {
                                //delete property obj
                                Delete(((Entity)item).GetEntityConfiguration(), (Entity)item, tran);
                            }
                        }
                    }
                }

                obj.ClearToDeleteRelatedPropertyObjects();
            }
        }

        private void DoDeleteToDeleteObjects(DynEntity obj, DbTransaction tran)
        {
            if (obj.GetToDeleteRelatedPropertyObjects() != null)
            {
                foreach (string propertyName in obj.GetToDeleteRelatedPropertyObjects().Keys)
                {
                    //create relation relatedType instance
                    EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(obj.EntityType.FullName);
                    PropertyConfiguration propertyConfig = ec.GetPropertyConfiguration(propertyName);
                    string relationTypeName = propertyConfig.RelationType;
                    Type relationType = null;
                    EntityConfiguration relationEc = null;
                    object relationTypeInstance = null;
                    if (relationTypeName != null)
                    {
                        relationType = Util.GetType(relationTypeName);
                        relationEc = MetaDataManager.GetEntityConfiguration(relationTypeName);
                        relationTypeInstance = Activator.CreateInstance(relationType);
                        foreach (PropertyConfiguration pc in relationEc.Properties)
                        {
                            if (pc.RelatedType != null)
                            {
                                Type pcRelatedType = Util.GetType(pc.RelatedType);
                                if (pcRelatedType.IsAssignableFrom(obj.GetType()))
                                {
                                    relationType.GetProperty(pc.Name).SetValue(relationTypeInstance, DynEntity.GetPropertyValues(obj, MetaDataManager.GetEntityConfiguration(pc.RelatedType).GetMappingColumnName(pc.RelatedForeignKey))[0], null);
                                }
                            }
                        }
                    }

                    foreach (object item in obj.GetToDeleteRelatedPropertyObjects()[propertyName])
                    {
                        if (item != null && item is DynEntity)
                        {
                            MethodInfo deleteMethod = GetGatewayMethodInfo("Void Delete[EntityType](EntityType, System.Data.Common.DbTransaction)");

                            if (relationTypeName != null)
                            {
                                //delete relationTypeInstance
                                foreach (PropertyConfiguration pc in relationEc.Properties)
                                {
                                    if (pc.RelatedType != null)
                                    {
                                        Type pcRelatedType = Util.GetType(pc.RelatedType);
                                        if (pcRelatedType.IsAssignableFrom(item.GetType()))
                                        {
                                            relationType.GetProperty(pc.Name).SetValue(relationTypeInstance, DynEntity.GetPropertyValues(item, MetaDataManager.GetEntityConfiguration(pc.RelatedType).GetMappingColumnName(pc.RelatedForeignKey))[0], null);
                                        }
                                    }
                                }
                                Delete(((Entity)relationTypeInstance).GetEntityConfiguration(), (Entity)relationTypeInstance, tran);
                            }

                            if (propertyConfig.IsContained)
                            {
                                //delete property obj
                                Delete(((Entity)item).GetEntityConfiguration(), (Entity)item, tran);
                            }
                        }
                    }
                }

                obj.ClearToDeleteRelatedPropertyObjects();
            }
        }

        private static void RemovedUpdatedModifiedProperties(Entity obj, Dictionary<string, object> modifiedProperties)
        {
            Dictionary<string, object> otherModifiedProperties = new Dictionary<string, object>();
            Dictionary<string, object> oldModifiedPropeties = obj.GetModifiedProperties();
            foreach (string key in oldModifiedPropeties.Keys)
            {
                otherModifiedProperties.Add(key, oldModifiedPropeties[key]);
            }
            lock (otherModifiedProperties)
            {
                foreach (string propertyName in modifiedProperties.Keys)
                {
                    otherModifiedProperties.Remove(propertyName);
                }
            }
            obj.SetModifiedProperties(otherModifiedProperties);
        }

        private static void RemovedUpdatedModifiedProperties(DynEntity obj, Dictionary<string, object> modifiedProperties)
        {
            Dictionary<string, object> otherModifiedProperties = new Dictionary<string, object>();
            Dictionary<string, object> oldModifiedPropeties = obj.GetModifiedProperties();
            foreach (string key in oldModifiedPropeties.Keys)
            {
                otherModifiedProperties.Add(key, oldModifiedPropeties[key]);
            }
            lock (otherModifiedProperties)
            {
                foreach (string propertyName in modifiedProperties.Keys)
                {
                    otherModifiedProperties.Remove(propertyName);
                }
            }
            obj.SetModifiedProperties(otherModifiedProperties);
        }

        private static void SetModifiedProperties(Dictionary<string, object> changedProperties, Entity obj)
        {
            Dictionary<string, object> cloneProperties = new Dictionary<string, object>();
            foreach (string key in changedProperties.Keys)
            {
                cloneProperties.Add(key, changedProperties[key]);
            }
            obj.SetModifiedProperties(cloneProperties);
        }

        private static void SetModifiedProperties(Dictionary<string, object> changedProperties, DynEntity obj)
        {
            Dictionary<string, object> cloneProperties = new Dictionary<string, object>();
            foreach (string key in changedProperties.Keys)
            {
                cloneProperties.Add(key, changedProperties[key]);
            }
            obj.SetModifiedProperties(cloneProperties);
        }

        private string FilterNTextPrefix(string sql)
        {
            if (sql == null)
            {
                return sql;
            }

            return sql.Replace(" N'", " '");
        }

        private void FilterNullSqlDefaultValueColumns(List<string> sqlDefaultValueColumns, string[] createColumnNames, DbType[] createColumnTypes, object[] createColumnValues, out string[] outNames, out DbType[] outTypes, out object[] outValues)
        {
            List<string> names = new List<string>();
            List<DbType> types = new List<DbType>();
            List<object> values = new List<object>();
            for (int i = 0; i < createColumnNames.Length; i++)
            {
                if (!(sqlDefaultValueColumns.Contains(createColumnNames[i]) && createColumnValues[i] == null))
                {
                    names.Add(createColumnNames[i]);
                    types.Add(createColumnTypes[i]);
                    values.Add(createColumnValues[i]);
                }
            }
            outNames = names.ToArray();
            outTypes = types.ToArray();
            outValues = values.ToArray();
        }

        public string ParseExpressionByMetaData(EntityConfiguration ec, string sql)
        {
            if (sql == null)
            {
                return null;
            }
            sql = PropertyItem.ParseExpressionByMetaData(sql, new PropertyToColumnMapHandler(ec.GetMappingColumnName), db.DbProvider.LeftToken, db.DbProvider.RightToken, db.DbProvider.ParamPrefix);
            return sql;
        }

        internal string ToFlatWhereClip(WhereClip where, EntityConfiguration ec)
        {
            Check.Require(ec != null, "ec could not be null.");

            if (WhereClip.IsNullOrEmpty(where))
            {
                return null;
            }
            else
            {
                //string whereStr = where.ToString();
                string whereStr = where.Sql;
                if (where.Parameters.Count > 0)
                {
                    Dictionary<string, KeyValuePair<DbType, object>>.ValueCollection.Enumerator en = where.Parameters.Values.GetEnumerator();

                    while (en.MoveNext())
                    {
                        object p = en.Current.Value;
                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("(" + "@" + @"[\w\d_]+)");
                        if (p != null && p is string)
                        {
                            whereStr = r.Replace(whereStr, Util.FormatParamVal(p.ToString().Replace("@", "\007")), 1);
                        }
                        else
                        {
                            whereStr = r.Replace(whereStr, Util.FormatParamVal(p), 1);
                        }
                    }
                }
                whereStr = whereStr.Replace("\007", "@");
                return where.IsNot ? "NOT (" + whereStr + ")" : whereStr;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="Gateway"/> class.
        /// </summary>
        static Gateway()
        {
            if (Database.Default != null)
            {
                Default = new Gateway(Database.Default);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gateway"/> class.
        /// </summary>
        /// <param name="connStrName">Name of the conn STR.</param>
        public Gateway(string connStrName)
        {
            InitGateway(new Database(DbProviderFactory.CreateDbProvider(connStrName)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gateway"/> class.
        /// </summary>
        /// <param name="db">The db.</param>
        public Gateway(Database db)
        {
            InitGateway(db);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gateway"/> class.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="connStr">The conn STR.</param>
        public Gateway(DatabaseType dt, string connStr)
        {
            if (dt == DatabaseType.Other)
            {
                throw new NotSupportedException("Please use \"new Gateway(string assemblyName, string className, string connStr)\" for databases other than SqlServer, MsAccess, MySql or Oracle Database!");
            }

            DbProvider provider = CreateDbProvider(dt, connStr);

            InitGateway(new Database(provider));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gateway"/> class.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="connStr">The conn STR.</param>
        public Gateway(string assemblyName, string className, string connStr)
        {
            DbProvider provider = DbProviderFactory.CreateDbProvider(assemblyName, className, connStr);
            if (provider == null)
            {
                throw new NotSupportedException(string.Format("Cannot construct DbProvider by specified parameters: {0}, {1}, {2}",
                    assemblyName, className, connStr));
            }

            InitGateway(new Database(provider));
        }

        #endregion

        #region Database

        /// <summary>
        /// Registers the SQL logger.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public void RegisterSqlLogger(LogHandler handler)
        {
            db.OnLog += handler;
        }

        /// <summary>
        /// Unregisters the SQL logger.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public void UnregisterSqlLogger(LogHandler handler)
        {
            db.OnLog -= handler;
        }

        /// <summary>
        /// Gets the db.
        /// </summary>
        /// <value>The db.</value>
        public Database Db
        {
            get
            {
                return this.db;
            }
        }

        ///// <summary>
        ///// Gets the db helper.
        ///// </summary>
        ///// <value>The db helper.</value>
        //[Obsolete]
        //public DbHelper DbHelper
        //{
        //    get
        //    {
        //        return dbHelper;
        //    }
        //}

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <returns>The begined transaction.</returns>
        public DbTransaction BeginTransaction()
        {
            return db.BeginTransaction();
        }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <param name="il">The il.</param>
        /// <returns>The begined transaction.</returns>
        public DbTransaction BeginTransaction(System.Data.IsolationLevel il)
        {
            return db.BeginTransaction(il);
        }

        /// <summary>
        /// Closes the transaction.
        /// </summary>
        /// <param name="tran">The tran.</param>
        public void CloseTransaction(DbTransaction tran)
        {
            if (tran.Connection != null && tran.Connection.State != ConnectionState.Closed)
            {
                db.CloseConnection(tran);
            }
        }

        /// <summary>
        /// Builds the name of the db param.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The name of the db param</returns>
        public string BuildDbParamName(string name)
        {
            Check.Require(name != null, "Arguments error.", new ArgumentNullException("name"));

            return db.DbProvider.BuildParameterName(name);
        }

        /// <summary>
        /// Builds the name of the db column.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The name of the db column</returns>
        public string BuildDbColumnName(string name)
        {
            Check.Require(name != null, "Arguments error.", new ArgumentNullException("name"));

            return db.DbProvider.BuildColumnName(name);
        }

        #endregion

        #region Create Entity

        /// <summary>
        /// The query handler.
        /// </summary>
        /// <param name="returnEntityType">Type of the return entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="baseEntity">The base entity.</param>
        /// <returns>The query result.</returns>
        public object OnQueryHandler(Type returnEntityType, string propertyName, string where, string orderBy, Entity baseEntity)
        {
            EntityConfiguration returnEntityEc = MetaDataManager.GetEntityConfiguration(returnEntityType.ToString());
            EntityConfiguration baseEntityEc = MetaDataManager.GetEntityConfiguration(baseEntity.GetType().ToString());

            Gateway findGateway = this;
            if (db.IsBatchConnection)
            {
                findGateway = new Gateway(new Database(db.DbProvider));
                findGateway.Db.OnLog += new LogHandler(db.WriteLog);
            }
            FromSection from = new FromSection(findGateway, returnEntityEc, new Rock.Orm.Common.FromClip(returnEntityEc.ViewName));

            //replace {BatchSize Name}s -> {Mapping Name}s
            where = ReplacePropertyNamesWithColumnNames(where, returnEntityEc);
            orderBy = ReplacePropertyNamesWithColumnNames(orderBy, returnEntityEc);

            where = ParseExpressionByMetaData(returnEntityEc, where);
            orderBy = ParseExpressionByMetaData(returnEntityEc, orderBy);

            WhereClip additionalWhere = new WhereClip();

            if (((object)where) != null)
            {
                string[] paramNames = db.ParseParamNames(where);
                string[] paramMappingColumnNames = null;
                DbType[] paramTypes = null;
                object[] paramValues = null;
                if (paramNames != null && paramNames.Length > 0)
                {
                    paramTypes = new DbType[paramNames.Length];
                    paramMappingColumnNames = new string[paramNames.Length];

                    for (int i = 0; i < paramNames.Length; i++)
                    {
                        paramNames[i] = paramNames[i].TrimStart('@');
                        PropertyConfiguration paramPc = baseEntityEc.GetPropertyConfiguration(paramNames[i]);
                        paramTypes[i] = paramPc.DbType;
                        paramMappingColumnNames[i] = paramPc.MappingName;
                    }

                    paramValues = Entity.GetPropertyValues(baseEntity, paramMappingColumnNames);
                }
                else
                {
                    paramNames = null;
                }

                additionalWhere = new WhereClip(where, paramNames, paramTypes, paramValues);
            }

            PropertyConfiguration pc = baseEntityEc.GetPropertyConfiguration(propertyName);

            if (pc.RelationType != null)
            {
                EntityConfiguration relationEc = MetaDataManager.GetEntityConfiguration(pc.RelationType);

                WhereClip onWhere = new WhereClip();

                List<string> foreignKeys = new List<string>();
                List<PropertyConfiguration> baseRelatedColumnConfigs = new List<PropertyConfiguration>();
                List<PropertyConfiguration> returnRelatedColumnConfigs = new List<PropertyConfiguration>();
                foreach (PropertyConfiguration item in relationEc.Properties)
                {
                    if (item.IsRelationKey && Util.GetType(item.RelatedType).IsAssignableFrom(baseEntity.GetType()))
                    {
                        foreignKeys.Add(MetaDataManager.GetEntityConfiguration(item.RelatedType).GetMappingColumnName(item.RelatedForeignKey));
                        baseRelatedColumnConfigs.Add(item);
                    }
                    else if (item.IsRelationKey && Util.GetType(item.RelatedType).IsAssignableFrom(returnEntityType))
                    {
                        returnRelatedColumnConfigs.Add(item);
                    }
                }
                object[] foreignKeyValues = Entity.GetPropertyValues(baseEntity, foreignKeys.ToArray());
                for (int i = 0; i < 1; i++)
                {
                    Rock.Orm.Common.ExpressionFactory.AppendWhereAnd(
                        additionalWhere,
                        Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(relationEc.ViewName + '.' + baseRelatedColumnConfigs[i].MappingName, baseRelatedColumnConfigs[i].DbType),
                        Rock.Orm.Common.QueryOperator.Equal,
                        Rock.Orm.Common.ExpressionFactory.CreateParameterExpression(baseRelatedColumnConfigs[i].DbType, foreignKeyValues[i])
                    );

                    PropertyConfiguration pkConfig = returnEntityEc.GetPrimaryKeyProperties()[0];
                    Rock.Orm.Common.ExpressionFactory.AppendWhereAnd(
                        onWhere,
                        Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(relationEc.ViewName + '.' + returnRelatedColumnConfigs[i].MappingName, returnRelatedColumnConfigs[i].DbType),
                        Rock.Orm.Common.QueryOperator.Equal,
                        Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(returnEntityEc.ViewName + '.' + pkConfig.MappingName, pkConfig.DbType)
                    );

                    from.Join(relationEc, relationEc.ViewName, onWhere);
                }
            }

            QuerySection query = from.Where(additionalWhere).OrderBy(new OrderByClip(orderBy));
            MethodInfo mi = typeof(QuerySection).GetMethod("ToArray", Type.EmptyTypes);
            return mi.MakeGenericMethod(returnEntityType).Invoke(query, null);
        }

        /// <summary>
        /// The query handler.
        /// </summary>
        /// <param name="returnEntityType">Type of the return entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="baseEntityType">The base entity.</param>
        /// <returns>The query result.</returns>
        public DynEntity[] OnQueryHandler(DynEntity returnEntityType, string propertyName, string where, string orderBy, DynEntity baseEntity)
        {
            EntityConfiguration returnEntityEc = MetaDataManager.GetEntityConfiguration(returnEntityType.EntityType.FullName);
            EntityConfiguration baseEntityEc = MetaDataManager.GetEntityConfiguration(baseEntity.EntityType.FullName);

            Gateway findGateway = this;
            if (db.IsBatchConnection)
            {
                findGateway = new Gateway(new Database(db.DbProvider));
                findGateway.Db.OnLog += new LogHandler(db.WriteLog);
            }
            FromSection from = new FromSection(findGateway, returnEntityEc, new Rock.Orm.Common.FromClip(returnEntityEc.ViewName));

            //replace {BatchSize Name}s -> {Mapping Name}s
            where = ReplacePropertyNamesWithColumnNames(where, returnEntityEc);
            orderBy = ReplacePropertyNamesWithColumnNames(orderBy, returnEntityEc);

            where = ParseExpressionByMetaData(returnEntityEc, where);
            orderBy = ParseExpressionByMetaData(returnEntityEc, orderBy);

            WhereClip additionalWhere = new WhereClip();

            if (((object)where) != null)
            {
                string[] paramNames = db.ParseParamNames(where);
                string[] paramMappingColumnNames = null;
                DbType[] paramTypes = null;
                object[] paramValues = null;
                if (paramNames != null && paramNames.Length > 0)
                {
                    paramTypes = new DbType[paramNames.Length];
                    paramMappingColumnNames = new string[paramNames.Length];

                    for (int i = 0; i < paramNames.Length; i++)
                    {
                        paramNames[i] = paramNames[i].TrimStart('@');
                        PropertyConfiguration paramPc = baseEntityEc.GetPropertyConfiguration(paramNames[i]);
                        paramTypes[i] = paramPc.DbType;
                        paramMappingColumnNames[i] = paramPc.MappingName;
                    }

                    paramValues = DynEntity.GetPropertyValues(baseEntity, paramMappingColumnNames);
                }
                else
                {
                    paramNames = null;
                }

                additionalWhere = new WhereClip(where, paramNames, paramTypes, paramValues);
            }

            PropertyConfiguration pc = baseEntityEc.GetPropertyConfiguration(propertyName);

            if (pc.RelationType != null)
            {
                EntityConfiguration relationEc = MetaDataManager.GetEntityConfiguration(pc.RelationType);

                WhereClip onWhere = new WhereClip();

                List<string> foreignKeys = new List<string>();
                List<PropertyConfiguration> baseRelatedColumnConfigs = new List<PropertyConfiguration>();
                List<PropertyConfiguration> returnRelatedColumnConfigs = new List<PropertyConfiguration>();
                foreach (PropertyConfiguration item in relationEc.Properties)
                {
                    if (item.IsRelationKey && DynEntityTypeManager.GetEntityTypeMandatory(item.RelatedType).IsAssignableFrom(baseEntity.EntityType))
                    {
                        foreignKeys.Add(MetaDataManager.GetEntityConfiguration(item.RelatedType).GetMappingColumnName(item.RelatedForeignKey));
                        baseRelatedColumnConfigs.Add(item);
                    }
                    else if (item.IsRelationKey && DynEntityTypeManager.GetEntityTypeMandatory(item.RelatedType).IsAssignableFrom(returnEntityType.EntityType))
                    {
                        returnRelatedColumnConfigs.Add(item);
                    }
                }

                object[] foreignKeyValues = DynEntity.GetPropertyValues(baseEntity, foreignKeys.ToArray());
                //  object[] foreignKeyValues = Entity.GetPropertyValues(baseEntity, foreignKeys.ToArray());

                for (int i = 0; i < 1; i++)
                {
                    Rock.Orm.Common.ExpressionFactory.AppendWhereAnd(
                        additionalWhere,
                        Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(relationEc.ViewName + '.' + baseRelatedColumnConfigs[i].MappingName, baseRelatedColumnConfigs[i].DbType),
                        Rock.Orm.Common.QueryOperator.Equal,
                        Rock.Orm.Common.ExpressionFactory.CreateParameterExpression(baseRelatedColumnConfigs[i].DbType, foreignKeyValues[i])
                    );

                    PropertyConfiguration pkConfig = returnEntityEc.GetPrimaryKeyProperties()[0];
                    Rock.Orm.Common.ExpressionFactory.AppendWhereAnd(
                        onWhere,
                        Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(relationEc.ViewName + '.' + returnRelatedColumnConfigs[i].MappingName, returnRelatedColumnConfigs[i].DbType),
                        Rock.Orm.Common.QueryOperator.Equal,
                        Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(returnEntityEc.ViewName + '.' + pkConfig.MappingName, pkConfig.DbType)
                    );

                    from.Join(relationEc, relationEc.ViewName, onWhere);
                }
            }

            QuerySection query = from.Where(additionalWhere).OrderBy(new OrderByClip(orderBy));
            return query.ToArray(returnEntityType.EntityType.FullName);
        }

        private string RemoveTypePrefix(string typeName)
        {
            string name = typeName;
            while (name.Contains("."))
            {
                name = name.Substring(name.LastIndexOf(".")).TrimStart('.');
            }
            return name;
        }

        private static string ReplacePropertyNamesWithColumnNames(string sql, EntityConfiguration ec)
        {
            if (sql == null)
            {
                return null;
            }

            while (sql.Contains("{"))
            {
                int begin = sql.IndexOf("{");
                int end = sql.IndexOf("}");
                string name = sql.Substring(begin, end - begin).Trim('{', '}');
                sql = sql.Replace("{" + name + "}", "[" + ec.MappingName + "].[" + ec.GetMappingColumnName(name) + "]");
            }
            sql = sql.Replace("[", "{").Replace("]", "}");
            return sql;
        }

        /// <summary>
        /// Creates and initiate an entity.
        /// </summary>
        /// <returns>The entity.</returns>
        internal EntityType CreateEntity<EntityType>()
            where EntityType : Entity, new()
        {
            EntityType obj = new EntityType();
            obj.SetQueryProxy(new Entity.QueryProxyHandler(OnQueryHandler));
            obj.Attach();
            return obj;
        }

        /// <summary>
        /// Creates and initiate an entity.
        /// </summary>
        /// <returns>The entity.</returns>
        internal DynEntity CreateEntity(string typeName)
        {
            DynEntity obj = new DynEntity(typeName);
            obj.SetQueryProxy(new DynEntity.QueryProxyHandler(OnQueryHandler));
            obj.Attach();
            return obj;
        }

        #endregion

        #region Batch Gateway

        /// <summary>
        /// Begins the batch gateway.
        /// </summary>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns>The begined batch gateway.</returns>
        public Gateway BeginBatchGateway(int batchSize)
        {
            Gateway gateway = new Gateway(new Database(db.DbProvider));
            gateway.db.OnLog += new LogHandler(this.db.WriteLog);
            gateway.BeginBatch(batchSize);
            return gateway;
        }

        /// <summary>
        /// Begins the batch gateway.
        /// </summary>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="tran">The tran.</param>
        /// <returns>The begined batch gateway.</returns>
        public Gateway BeginBatchGateway(int batchSize, DbTransaction tran)
        {
            Gateway gateway = new Gateway(new Database(db.DbProvider));
            gateway.db.OnLog += new LogHandler(this.db.WriteLog);
            gateway.BeginBatch(batchSize, tran);
            return gateway;
        }

        /// <summary>
        /// Begins the gateway as a batch gateway.
        /// </summary>
        /// <param name="batchSize">Size of the batch.</param>
        public void BeginBatch(int batchSize)
        {
            db.BeginBatchConnection(batchSize >= 1 ? batchSize : 1);
        }

        /// <summary>
        /// Begins the gateway as a batch gateway.
        /// </summary>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="tran">The tran.</param>
        public void BeginBatch(int batchSize, DbTransaction tran)
        {
            db.BeginBatchConnection(batchSize >= 1 ? batchSize : 1, tran);
        }

        /// <summary>
        /// Ends the batch.
        /// </summary>
        public void EndBatch()
        {
            db.EndBatchConnection();
        }

        /// <summary>
        /// Executes the pending batch operations.
        /// </summary>
        public void ExecutePendingBatchOperations()
        {
            db.ExecutePendingBatchOperations();
        }

        #endregion

        #region StrongTyped Gateways

        #region Help methods for StrongTyped methods

        internal void AdjustWhereForAutoCascadeJoin(WhereClip where, EntityConfiguration ec)
        {
            AdjustWhereForAutoCascadeJoin(where, ec, null);
        }

        internal void AdjustWhereForAutoCascadeJoin(WhereClip where, EntityConfiguration ec, List<string> columnListToReplace)
        {
            if (where == null)
            {
                return;
            }

            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"(_/\*[^\*]+\*/)+");
            System.Text.RegularExpressions.MatchCollection ms = r.Matches(where.ToString());
            if (ms.Count > 0)
            {
                for (int i = 0; i < ms.Count; ++i)
                {
                    string aliasName = ec.MappingName;

                    System.Text.RegularExpressions.Regex r2 = new System.Text.RegularExpressions.Regex(@"_/\*[^\*]+\*/");
                    System.Text.RegularExpressions.MatchCollection ms2 = r2.Matches(ms[i].Value);
                    EntityConfiguration previousEc = ec;
                    EntityConfiguration joinEc = null;
                    for (int j = 0; j < ms2.Count; ++j)
                    {
                        string propertyName = ms2[j].Value.TrimStart('_').Trim('/', '*');
                        string currentAliasName = aliasName;
                        aliasName += '_' + propertyName;

                        PropertyConfiguration pc = previousEc.GetPropertyConfiguration(propertyName);
                        joinEc = MetaDataManager.GetEntityConfiguration(pc.PropertyType);
                        //list.Add(joinEc.Name);

                        string pcMappingTableName = pc.InheritEntityMappingName == null ? previousEc.MappingName : pc.InheritEntityMappingName;

                        if (!where.From.Joins.ContainsKey(aliasName + '_' + joinEc.MappingName))
                        {
                            PropertyConfiguration joinPc = joinEc.GetPrimaryKeyProperties()[0];
                            WhereClip onWhere = new WhereClip();
                            if (pc.QueryType == "FkReverseQuery")
                            {
                                Rock.Orm.Common.ExpressionFactory.AppendWhereAnd(onWhere,
                                    Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(aliasName + '_' + joinEc.MappingName + '.' + joinPc.MappingName, joinPc.DbType),
                                    Rock.Orm.Common.QueryOperator.Equal,
                                    Rock.Orm.Common.ExpressionFactory.CreateColumnExpression((currentAliasName == ec.MappingName ? pcMappingTableName : currentAliasName + '_' + pcMappingTableName) + '.' + pc.MappingName, pc.DbType)
                                    );
                            }
                            else
                            {
                                PropertyConfiguration pkConfig = previousEc.GetPrimaryKeyProperties()[0];
                                Rock.Orm.Common.ExpressionFactory.AppendWhereAnd(onWhere,
                                    Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(aliasName + '_' + joinEc.MappingName + '.' + joinPc.MappingName, joinPc.DbType),
                                    Rock.Orm.Common.QueryOperator.Equal,
                                    Rock.Orm.Common.ExpressionFactory.CreateColumnExpression((currentAliasName == ec.MappingName ? pcMappingTableName : currentAliasName + '_' + pcMappingTableName) + '.' + pkConfig.MappingName, pkConfig.DbType)
                                    );
                            }

                            where.From.Join(joinEc.MappingName, aliasName + '_' + joinEc.MappingName, onWhere);
                        }

                        previousEc = joinEc;
                    }

                    string left = '[' + ms[i].Value + '_' + joinEc.MappingName;
                    string right = '[' + aliasName + '_' + joinEc.MappingName;
                    where.Sql = where.Sql.Replace(left, right);
                    if (where.GroupBy != null)
                    {
                        where.GroupBy = where.GroupBy.Replace(left, right);
                    }
                    if (where.OrderBy != null)
                    {
                        where.OrderBy = where.OrderBy.Replace(left, right);
                    }
                    if (columnListToReplace != null)
                    {
                        for (int k = 0; k < columnListToReplace.Count; ++k)
                        {
                            columnListToReplace[k] = columnListToReplace[k].Replace(left, right);
                        }
                    }
                }
            }

            //return list;
        }

        internal Rock.Orm.Common.FromClip ConstructFrom(EntityConfiguration ec)
        {
            return ConstructFrom(ec, string.Empty);
        }

        internal Rock.Orm.Common.FromClip ConstructFrom(EntityConfiguration ec, string aliasNamePrefix)
        {
            if (aliasNamePrefix == null)
            {
                aliasNamePrefix = string.Empty;
            }

            Rock.Orm.Common.FromClip from = new Rock.Orm.Common.FromClip(ec.ViewName, aliasNamePrefix + ec.ViewName);

            AppendBaseEntitiesJoins(ec, aliasNamePrefix, from);

            return from;
        }

        internal void AppendBaseEntitiesJoins(EntityConfiguration ec, string aliasNamePrefix, Rock.Orm.Common.FromClip from)
        {
            //join all base entities
            if (ec.BaseEntity != null)
            {
                //get pk column name
                PropertyConfiguration pkColumn = null;
                for (int i = 0; i < ec.Properties.Length; ++i)
                {
                    if (ec.Properties[i].IsPrimaryKey)
                    {
                        pkColumn = ec.Properties[i];
                        break;
                    }
                }

                EntityConfiguration baseEc = MetaDataManager.GetEntityConfiguration(ec.BaseEntity);
                while (baseEc != null)
                {
                    WhereClip onWhere = new WhereClip();
                    Rock.Orm.Common.ExpressionFactory.AppendWhereAnd(
                        onWhere,
                        Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(aliasNamePrefix + baseEc.MappingName + '.' + pkColumn.MappingName, pkColumn.DbType),
                        Rock.Orm.Common.QueryOperator.Equal,
                        Rock.Orm.Common.ExpressionFactory.CreateColumnExpression(aliasNamePrefix + ec.MappingName + '.' + pkColumn.MappingName, pkColumn.DbType));
                    from.Join(baseEc.ViewName, aliasNamePrefix + baseEc.ViewName, onWhere);

                    if (baseEc.BaseEntity == null)
                    {
                        break;
                    }

                    baseEc = MetaDataManager.GetEntityConfiguration(baseEc.BaseEntity);
                }
            }
        }

        private int Insert(string tableName, string[] columns, DbType[] types, object[] values, DbTransaction tran, string identyColumn)
        {
            DbCommand cmd = queryFactory.CreateInsertCommand(tableName, columns, types, values);

            if ((!db.IsBatchConnection) && identyColumn != null && db.DbProvider.SelectLastInsertedRowAutoIDStatement != null)
            {
                object retVal = 0;

                if (!db.DbProvider.SelectLastInsertedRowAutoIDStatement.Contains("{1}"))
                {
                    cmd.CommandText = cmd.CommandText + ';' + db.DbProvider.SelectLastInsertedRowAutoIDStatement;
                    if (tran == null)
                    {
                        retVal = db.ExecuteScalar(cmd);
                    }
                    else
                    {
                        retVal = db.ExecuteScalar(cmd, tran);
                    }

                    if (retVal != DBNull.Value)
                    {
                        return Convert.ToInt32(retVal);
                    }
                }
                else
                {
                    if (db.DbProvider.SelectLastInsertedRowAutoIDStatement.StartsWith("SELECT SEQ_"))
                    {
                        if (tran == null)
                        {
                            retVal = db.ExecuteScalar(CommandType.Text, string.Format(db.DbProvider.SelectLastInsertedRowAutoIDStatement, null, tableName));
                            db.ExecuteNonQuery(cmd);
                        }
                        else
                        {
                            retVal = db.ExecuteScalar(tran, CommandType.Text, string.Format(db.DbProvider.SelectLastInsertedRowAutoIDStatement, null, tableName));
                            db.ExecuteNonQuery(cmd, tran);
                        }

                        if (retVal != DBNull.Value)
                        {
                            return Convert.ToInt32(retVal);
                        }
                    }
                    else if ((!db.DbProvider.SelectLastInsertedRowAutoIDStatement.Contains("{0}")) && (!db.DbProvider.SelectLastInsertedRowAutoIDStatement.Contains("{1}")))
                    {
                        DbTransaction t = (tran == null ? BeginTransaction() : tran);
                        try
                        {
                            db.ExecuteNonQuery(cmd, t);
                            retVal = db.ExecuteScalar(t, CommandType.Text, db.DbProvider.SelectLastInsertedRowAutoIDStatement);

                            if (tran == null)
                            {
                                t.Commit();
                            }
                        }
                        catch (DbException)
                        {
                            if (tran == null)
                            {
                                t.Rollback();
                            }
                            throw;
                        }
                        finally
                        {
                            if (tran == null)
                            {
                                CloseTransaction(t);
                                t.Dispose();
                            }
                        }

                        if (retVal != DBNull.Value)
                        {
                            return Convert.ToInt32(retVal);
                        }
                    }
                    else
                    {
                        if (tran == null)
                        {
                            db.ExecuteNonQuery(cmd);
                            retVal = db.ExecuteScalar(CommandType.Text, string.Format(db.DbProvider.SelectLastInsertedRowAutoIDStatement, db.DbProvider.BuildColumnName(identyColumn), db.DbProvider.BuildColumnName(tableName)));
                            return Convert.ToInt32(retVal);
                        }
                        else
                        {
                            if (tran.IsolationLevel != System.Data.IsolationLevel.ReadUncommitted)
                            {
                                retVal = db.ExecuteScalar(tran, CommandType.Text, string.Format(db.DbProvider.SelectLastInsertedRowAutoIDStatement, db.DbProvider.BuildColumnName(identyColumn), db.DbProvider.BuildColumnName(tableName)));
                            }
                            db.ExecuteNonQuery(cmd, tran);
                            if (tran.IsolationLevel == System.Data.IsolationLevel.ReadUncommitted)
                            {
                                retVal = db.ExecuteScalar(tran, CommandType.Text, string.Format(db.DbProvider.SelectLastInsertedRowAutoIDStatement, db.DbProvider.BuildColumnName(identyColumn), db.DbProvider.BuildColumnName(tableName)));
                            }

                            if (retVal != DBNull.Value)
                            {
                                return tran.IsolationLevel == System.Data.IsolationLevel.ReadUncommitted ? Convert.ToInt32(retVal) : Convert.ToInt32(retVal) + 1;
                            }
                        }
                    }
                }
            }
            else
            {
                if (tran == null)
                {
                    db.ExecuteNonQuery(cmd);
                }
                else
                {
                    db.ExecuteNonQuery(cmd, tran);
                }
            }

            return 0;
        }

        private int Update(string tableName, WhereClip where, string[] columns, DbType[] types, object[] values, DbTransaction tran)
        {
            DbCommand cmd = queryFactory.CreateUpdateCommand(tableName, where, columns, types, values);
            return tran == null ? db.ExecuteNonQuery(cmd) : db.ExecuteNonQuery(cmd, tran);
        }

        private int Delete(string tableName, WhereClip where, DbTransaction tran)
        {
            DbCommand cmd = queryFactory.CreateDeleteCommand(tableName, where);
            return tran == null ? db.ExecuteNonQuery(cmd) : db.ExecuteNonQuery(cmd, tran);
        }

        private static System.Text.RegularExpressions.Regex regForIfFindFromPreload =
            new System.Text.RegularExpressions.Regex(@"[a-zA-Z]+\(");

        internal bool IfFindFromPreload(EntityConfiguration ec, WhereClip where)
        {
            string whereStr = where.ToString();
            bool ret = IsCacheTurnedOn && ec.IsAutoPreLoad && (!whereStr.Contains("/*"));

            if (!ret)
            {
                return false;
            }

            System.Text.RegularExpressions.MatchCollection matches = regForIfFindFromPreload.Matches(whereStr);
            for (int i = 0; i < matches.Count; ++i)
            {
                if (matches[i].Value != "COUNT(" && matches[i].Value != "SUM(" && matches[i].Value != "MIN(" &&
                    matches[i].Value != "MAX(" && matches[i].Value != "AVG(" &&
                    matches[i].Value != "LEN(" && matches[i].Value != "SUBSTRING(")
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Find

        /// <summary>
        /// Finds the specified pk values.
        /// </summary>
        /// <param name="pkValues">The pk values.</param>
        /// <returns>The result.</returns>
        public EntityType Find<EntityType>(params object[] pkValues)
            where EntityType : Entity, new()
        {
            Check.Require(pkValues != null && pkValues.Length > 0, "pkValues could not be null or empty.");
            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            List<PropertyConfiguration> pks = ec.GetPrimaryKeyProperties();

            WhereClip where = BuildEqualWhereClip(ec, pkValues, pks);

            return From<EntityType>().Where(where).ToFirst<EntityType>();
        }

        /// <summary>
        /// Finds the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>The result.</returns>
        public EntityType Find<EntityType>(WhereClip where)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            return From<EntityType>().Where(where).ToFirst<EntityType>();
        }

        /// <summary>
        /// Whether entity exists.
        /// </summary>
        /// <param name="pkValues">The pk values.</param>
        /// <returns>Whether existing.</returns>
        public bool Exists<EntityType>(params object[] pkValues)
            where EntityType : Entity, new()
        {
            Check.Require(pkValues != null && pkValues.Length > 0, "pkValues could not be null or empty.");
            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            List<PropertyConfiguration> pks = ec.GetPrimaryKeyProperties();

            WhereClip where = BuildEqualWhereClip(ec, pkValues, pks);

            return Convert.ToInt32(From<EntityType>().Where(where).Select(PropertyItem.All.Count()).ToScalar()) > 0;
        }

        /// <summary>
        /// Whether entity exists.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>Whether existing.</returns>
        public bool Exists<EntityType>(WhereClip where)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            return Convert.ToInt32(From<EntityType>().Where(where).Select(PropertyItem.All.Count()).ToScalar()) > 0;
        }

        /// <summary>
        /// Finds the array.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns>The result.</returns>
        public EntityType[] FindArray<EntityType>(WhereClip where, OrderByClip orderBy)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(orderBy != null, "orderBy could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            return From<EntityType>().Where(where).OrderBy(orderBy).ToArray<EntityType>();
        }

        /// <summary>
        /// Finds the array.
        /// </summary>
        /// <param name="orderBy">The order by.</param>
        /// <returns></returns>
        public EntityType[] FindArray<EntityType>(OrderByClip orderBy)
            where EntityType : Entity, new()
        {
            return FindArray<EntityType>(WhereClip.All, orderBy);
        }

        /// <summary>
        /// Finds the array.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns></returns>
        public EntityType[] FindArray<EntityType>(WhereClip where)
            where EntityType : Entity, new()
        {
            return FindArray<EntityType>(where, OrderByClip.Default);
        }

        /// <summary>
        /// Finds the array.
        /// </summary>
        /// <returns></returns>
        public EntityType[] FindArray<EntityType>()
            where EntityType : Entity, new()
        {
            return FindArray<EntityType>(WhereClip.All, OrderByClip.Default);
        }

        //        /// <summary>
        //        /// Finds the array.
        //        /// </summary>
        //        /// <param name="ec">The ec.</param>
        //        /// <param name="viewName">Name of the view.</param>
        //        /// <param name="where">The where.</param>
        //        /// <param name="orderBy">The order by.</param>
        //        /// <returns>The result.</returns>
        //        [Obsolete]
        //        internal EntityType[] FindArray<EntityType>(EntityConfiguration ec, string viewName, WhereClip where, OrderByClip orderBy)
        //            where EntityType : Entity, new()
        //        {
        //            Check.Require(((object)where) != null, "where could not be null.");
        //            Check.Require(orderBy != null, "orderBy could not be null.");

        //            if (IsCacheTurnedOn && ec.IsAutoPreLoad && ec.ViewName == viewName)
        //            {
        //                try
        //                {
        //                    return FindArrayFromPreLoadEx<EntityType>(ec, where);
        //                }
        //                catch
        //                {
        //#if DEBUG
        //                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
        //#endif
        //                }
        //            }

        //            where = PrepareWhere(where, ec, orderBy);

        //            string cacheKey = null;
        //            if (IsCacheTurnedOn && GetTableCacheExpireSeconds(ec.ViewName) > 0)
        //            {
        //                cacheKey = ComputeCacheKey(typeof(EntityType).ToString() + "|FindArray_" + viewName, where);
        //                object cachedObj = GetCache(cacheKey);
        //                if (cachedObj != null)
        //                {
        //                    return (EntityType[])cachedObj;
        //                }
        //            }

        //            IDataReader reader = FindDataReader(where, ec.GetAllSelectColumns());
        //            List<EntityType> objs = new List<EntityType>();
        //            while (reader.Read())
        //            {
        //                EntityType obj = CreateEntity<EntityType>();
        //                obj.SetPropertyValues(reader);
        //                objs.Add(obj);
        //            }
        //            reader.Close();
        //            reader.Dispose();

        //            if (IsCacheTurnedOn && GetTableCacheExpireSeconds(ec.ViewName) > 0 && cacheKey != null)
        //            {
        //                AddCache(cacheKey, objs.ToArray(), GetTableCacheExpireSeconds(ec.ViewName));
        //            }

        //            return objs.ToArray();
        //        }

        /// <summary>
        /// Finds the data table.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns></returns>
        public DataTable FindDataTable<EntityType>(WhereClip where, OrderByClip orderBy)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(orderBy != null, "orderBy could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            DataSet ds = From<EntityType>().Where(where).OrderBy(orderBy).ToDataSet();
            if (ds != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }
            return null;
        }

        /// <summary>
        /// Finds the single property array.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="property">The property.</param>
        /// <returns>The result.</returns>
        public object[] FindSinglePropertyArray<EntityType>(WhereClip where, OrderByClip orderBy, Rock.Orm.Common.ExpressionClip property)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(orderBy != null, "orderBy could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            return From<EntityType>().Where(where).OrderBy(orderBy).Select(property).ToSinglePropertyArray();
        }

        /// <summary>
        /// Finds the scalar.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="property">The property.</param>
        /// <returns>The result.</returns>
        public object FindScalar<EntityType>(WhereClip where, OrderByClip orderBy, Rock.Orm.Common.ExpressionClip property)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(orderBy != null, "orderBy could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            return From<EntityType>().Where(where).OrderBy(orderBy).Select(property).ToScalar();
        }

        #endregion

        #region Aggregation

        /// <summary>
        /// Counts the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="property">The property.</param>
        /// <param name="isDistinct">if set to <c>true</c> [is distinct].</param>
        /// <returns>The result.</returns>
        public int Count<EntityType>(WhereClip where, Rock.Orm.Common.ExpressionClip property, bool isDistinct)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            return Convert.ToInt32(GetAggregateValueEx(property.Count(isDistinct), ec, where));
        }

        /// <summary>
        /// Counts the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>The result.</returns>
        public int Count<EntityType>(WhereClip where)
            where EntityType : Entity, new()
        {
            return Count<EntityType>(where, PropertyItem.All, false);
        }

        /// <summary>
        /// Counts the pages.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns></returns>
        public int CountPage<EntityType>(WhereClip where, int pageSize)
            where EntityType : Entity, new()
        {
            Check.Require(pageSize > 0, "pageSize must > 0!");

            int rowCount = Count<EntityType>(where);
            return rowCount % pageSize == 0 ? rowCount / pageSize : (rowCount / pageSize) + 1;
        }

        ///// <summary>
        ///// Maxes the specified where.
        ///// </summary>
        ///// <param name="where">The where.</param>
        ///// <param name="property">The property.</param>
        ///// <returns>The result.</returns>
        //public object Max<EntityType>(WhereClip where, PropertyItem property)
        //    where EntityType : Entity, new()
        //{
        //    Check.Require(((object)where) != null, "where could not be null.");
        //    Check.Require(!property.Equals(null), "property could not be null.");

        //    EntityConfiguration ec = new EntityType().GetEntityConfiguration();

        //    return GetAggregateValueEx(property.Max(), ec, where, true);
        //}

        /// <summary>
        /// Maxes the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public object Max<EntityType>(WhereClip where, Rock.Orm.Common.ExpressionClip property)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            return GetAggregateValueEx(property.Max(), ec, where);
        }

        /// <summary>
        /// Mins the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public object Min<EntityType>(WhereClip where, Rock.Orm.Common.ExpressionClip property)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            return GetAggregateValueEx(property.Min(), ec, where);
        }

        /// <summary>
        /// Sums the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public object Sum<EntityType>(WhereClip where, Rock.Orm.Common.ExpressionClip property)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            return GetAggregateValueEx(property.Sum(), ec, where);
        }

        /// <summary>
        /// Avgs the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public object Avg<EntityType>(WhereClip where, Rock.Orm.Common.ExpressionClip property)
            where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            return GetAggregateValueEx(property.Avg(), ec, where);
        }

        #endregion

        #region Save

        /// <summary>
        /// Creates the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="tran">The tran.</param>
        /// <returns>The auto incrememtal column value. If there is no auto column, the value is 0.</returns>
        internal int Create(Entity obj, DbTransaction tran)
        {
            Check.Require(obj != null, "relatedObj could not be null.");
            EntityConfiguration ec = obj.GetEntityConfiguration();

            if ((!db.IsBatchConnection) && ec.IsBatchUpdate)
            {
                Gateway batchGateway = BeginBatchGateway(ec.BatchSize, tran);
                int retInt = batchGateway.Create(obj, tran);
                batchGateway.EndBatch();
                return retInt;
            }

            string[] pks = Entity.GetPrimaryKeyMappingColumnNames(ec);
            string keyColumn = null;
            if (pks != null && pks.Length == 1)
            {
                PropertyConfiguration pc = null;

                foreach (PropertyConfiguration item in ec.Properties)
                {
                    if (item.MappingName == pks[0])
                    {
                        pc = item;
                        if (item.IsReadOnly)
                        {
                            keyColumn = pks[0];
                        }
                        break;
                    }
                }

                object pkValue = Entity.GetPropertyValues(obj, pks)[0];
                if (pc.IsPrimaryKey && (!ec.IsRelation) && pc.SqlType.Trim().ToLower() == "uniqueidentifier" && (pkValue == null || ((Guid)pkValue) == default(Guid)))
                {
                    Util.DeepGetProperty(obj.GetType(), pc.Name).SetValue(obj, Guid.NewGuid(), null);
                }
            }

            //bind query proxy handler to relatedObj
            obj.SetQueryProxy(new Entity.QueryProxyHandler(OnQueryHandler));

            int retAutoID = 0;

            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                string[] createColumnNames = Entity.GetCreatePropertyMappingColumnNames(obj.GetEntityConfiguration());
                DbType[] createColumnTypes = Entity.GetCreatePropertyMappingColumnTypes(obj.GetEntityConfiguration());
                object[] createColumnValues = Entity.GetCreatePropertyValues(obj.GetType(), obj);
                List<string> sqlDefaultValueColumns = MetaDataManager.GetSqlTypeWithDefaultValueColumns(ec.Name);
                if (sqlDefaultValueColumns.Count > 0)
                {
                    FilterNullSqlDefaultValueColumns(sqlDefaultValueColumns, createColumnNames, createColumnTypes, createColumnValues, out createColumnNames, out createColumnTypes, out createColumnValues);
                }
                retAutoID = Insert(ec.MappingName, createColumnNames, createColumnTypes, createColumnValues, tran, keyColumn);

                if (retAutoID > 0)
                {
                    //save the retAutoID value to entity's ID property
                    string autoIdProperty = MetaDataManager.GetEntityAutoId(ec.Name);
                    if (autoIdProperty != null)
                    {
                        PropertyInfo pi = Util.DeepGetProperty(obj.GetType(), autoIdProperty);
                        if (pi != null)
                        {
                            pi.SetValue(obj, Convert.ChangeType(retAutoID, pi.PropertyType), null);
                        }
                    }
                }

                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(ec.Name);
            }
            else
            {
                if (db.DbProvider.SupportADO20Transaction && tran == null)
                {
                    //ado2.0 tran
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        retAutoID = DoCascadeInsert(obj, tran, ec, keyColumn);
                        DoCascadePropertySave(obj, tran, ec);

                        scope.Complete();
                    }
                }
                else
                {
                    DbTransaction t;
                    if (tran == null)
                    {
                        t = BeginTransaction();
                    }
                    else
                    {
                        t = tran;
                    }

                    try
                    {
                        retAutoID = DoCascadeInsert(obj, t, ec, keyColumn);
                        DoCascadePropertySave(obj, tran, ec);

                        if (tran == null)
                        {
                            t.Commit();
                        }
                    }
                    catch
                    {
                        if (tran == null)
                        {
                            t.Rollback();
                        }
                        throw;
                    }
                    finally
                    {
                        if (tran == null)
                        {
                            CloseTransaction(t);
                        }
                    }
                }

                CascadeRemoveEntityCaches(ec);
            }

            obj.Attach();
            obj.SetQueryProxy(new Entity.QueryProxyHandler(OnQueryHandler));
            return retAutoID;
        }

        /// <summary>
        /// Creates the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The auto incrememtal column value. If there is no auto column, the value is 0.</returns>
        internal int Create(Entity obj)
        {
            return Create(obj, null);
        }

        /// <summary>
        /// Updates the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="tran">The tran.</param>
        internal void Update(Entity obj, DbTransaction tran)
        {
            Check.Require(obj != null, "relatedObj could not be null.");
            EntityConfiguration ec = obj.GetEntityConfiguration();

            if ((!db.IsBatchConnection) && ec.IsBatchUpdate)
            {
                Gateway batchGateway = BeginBatchGateway(ec.BatchSize, tran);
                batchGateway.Update(obj, tran);
                batchGateway.EndBatch();
                return;
            }

            string[] pks = Entity.GetPrimaryKeyMappingColumnNames(ec);
            object[] pkValues = Entity.GetPropertyValues(obj, pks);
            List<PropertyConfiguration> pks2 = ec.GetPrimaryKeyProperties();

            WhereClip where = BuildEqualWhereClip(ec, pkValues, pks2);

            Dictionary<string, object> modifiedProperties = obj.GetModifiedProperties(obj.GetType());

            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                if (modifiedProperties.Count > 0)
                {
                    string[] columnNames = ec.GetMappingColumnNames(new List<string>(modifiedProperties.Keys).ToArray());
                    DbType[] columnTypes = ec.GetMappingColumnTypes(new List<string>(modifiedProperties.Keys).ToArray());
                    object[] columnValues = new List<object>(modifiedProperties.Values).ToArray();
                    Update(ec.MappingName, where, columnNames, columnTypes, columnValues, tran);
                }

                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(ec.Name);
            }
            else
            {
                if (db.DbProvider.SupportADO20Transaction && tran == null)
                {
                    //ado2.0 tran
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        DoDeleteToDeleteObjects(obj, tran);
                        DoCascadeUpdate(obj, tran, ec, where, modifiedProperties);
                        DoCascadePropertySave(obj, tran, ec);

                        scope.Complete();
                    }
                }
                else
                {
                    DbTransaction t;
                    if (tran == null)
                    {
                        t = BeginTransaction();
                    }
                    else
                    {
                        t = tran;
                    }

                    try
                    {
                        DoDeleteToDeleteObjects(obj, tran);
                        DoCascadeUpdate(obj, t, ec, where, modifiedProperties);
                        DoCascadePropertySave(obj, tran, ec);

                        if (tran == null)
                        {
                            t.Commit();
                        }
                    }
                    catch
                    {
                        if (tran == null)
                        {
                            t.Rollback();
                        }
                        throw;
                    }
                    finally
                    {
                        if (tran == null)
                        {
                            CloseTransaction(t);
                        }
                    }
                }

                CascadeRemoveEntityCaches(ec);
            }

            obj.SetQueryProxy(new Entity.QueryProxyHandler(OnQueryHandler));
        }

        /// <summary>
        /// Updates the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        internal void Update(Entity obj)
        {
            Update(obj, null);
        }

        /// <summary>
        /// Saves the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="tran">The tran.</param>
        /// <returns>The auto incrememtal column value. If there is no auto column or the entity is already persisted, the value is 0.</returns>
        public int Save(Entity obj, DbTransaction tran)
        {
            Check.Require(obj != null, "relatedObj could not be null.");

            int retAutoID = 0;

            if (obj.IsAttached())
            {
                Update(obj, tran);
            }
            else
            {
                retAutoID = Create(obj, tran);
            }

            return retAutoID;
        }

        /// <summary>
        /// Saves the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="tran">The tran.</param>
        /// <returns>The auto incrememtal column value. If there is no auto column or the entity is already persisted, the value is 0.</returns>
        public int Save<EntityType>(EntityType obj, DbTransaction tran)
            where EntityType : Entity, new()
        {
            return Save((Entity)obj, tran);
        }

        /// <summary>
        /// Saves the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The auto incrememtal column value. If there is no auto column or the entity is already persisted, the value is 0.</returns>
        public int Save<EntityType>(EntityType obj)
            where EntityType : Entity, new()
        {
            return Save(obj, null);
        }

        internal void Delete(EntityConfiguration ec, Entity obj, DbTransaction tran)
        {
            object[] pkValues = Entity.GetPrimaryKeyValues(obj);

            if ((!db.IsBatchConnection) && ec.IsBatchUpdate)
            {
                Gateway batchGateway = BeginBatchGateway(ec.BatchSize, tran);
                batchGateway.Delete(ec, obj, tran);
                batchGateway.EndBatch();
                return;
            }

            List<PropertyConfiguration> pks = ec.GetPrimaryKeyProperties();

            WhereClip where = BuildEqualWhereClip(ec, pkValues, pks);

            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                Delete(ec.MappingName, where, tran);
                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(ec.Name);
            }
            else
            {
                bool deletedAsChildEntity = DeleteAsChildEntity(ec.Name, pkValues, tran);

                if (!deletedAsChildEntity)
                {
                    if (db.DbProvider.SupportADO20Transaction && tran == null)
                    {
                        //ado2.0 tran
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                        {
                            DoCascadeDelete(obj, tran, ec, where);

                            scope.Complete();
                        }
                    }
                    else
                    {
                        DbTransaction t;
                        if (tran == null)
                        {
                            t = BeginTransaction();
                        }
                        else
                        {
                            t = tran;
                        }

                        try
                        {
                            DoCascadeDelete(obj, t, ec, where);

                            if (tran == null)
                            {
                                t.Commit();
                            }
                        }
                        catch
                        {
                            if (tran == null)
                            {
                                t.Rollback();
                            }
                            throw;
                        }
                        finally
                        {
                            if (tran == null)
                            {
                                CloseTransaction(t);
                            }
                        }
                    }
                }

                CascadeRemoveEntityCaches(ec);
            }

            obj.SetQueryProxy(null);
            obj.Detach();
        }

        /// <summary>
        /// Deletes the specified tran.
        /// </summary>
        /// <param name="tran">The tran.</param>
        /// <param name="pkValues">The pk values.</param>
        public void Delete<EntityType>(DbTransaction tran, params object[] pkValues)
            where EntityType : Entity, new()
        {
            Check.Require(pkValues != null && pkValues.Length > 0, "relatedObj could not be null or empty.");
            EntityConfiguration ec = new EntityType().GetEntityConfiguration();
            List<PropertyConfiguration> pks = ec.GetPrimaryKeyProperties();

            WhereClip where = BuildEqualWhereClip(ec, pkValues, pks);

            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                //dbHelper.Delete(ec.MappingName, ParseExpressionByMetaData(ec, where.ToString()), where.ParamValues, tran);
                Delete(ec.MappingName, where, tran);
                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(typeof(EntityType).ToString());
            }
            else
            {
                Gateway findGateway = this;
                if (db.IsBatchConnection)
                {
                    findGateway = new Gateway(new Database(db.DbProvider));
                    findGateway.Db.OnLog += new LogHandler(db.WriteLog);
                }

                EntityType obj = findGateway.Find<EntityType>(pkValues);
                if (obj != null)
                {
                    Delete(ec, obj, tran);
                }

                CascadeRemoveEntityCaches(ec);
            }
        }

        /// <summary>
        /// Deletes the specified pk values.
        /// </summary>
        /// <param name="pkValues">The pk values.</param>
        public void Delete<EntityType>(params object[] pkValues)
            where EntityType : Entity, new()
        {
            Delete<EntityType>(null, pkValues);
        }

        /// <summary>
        /// Deletes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="tran">The tran.</param>
        public void Delete<EntityType>(EntityType obj, DbTransaction tran)
            where EntityType : Entity, new()
        {
            Check.Require(obj != null, "relatedObj could not be null.");

            EntityConfiguration ec = obj.GetEntityConfiguration();
            Delete(ec, obj, tran);
        }

        /// <summary>
        /// Deletes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public void Delete<EntityType>(EntityType obj)
            where EntityType : Entity, new()
        {
            Delete<EntityType>(obj, null);
        }

        /// <summary>
        /// Batch delete.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="tran">The tran.</param>
        public void Delete<EntityType>(WhereClip where, DbTransaction tran)
           where EntityType : Entity, new()
        {
            Check.Require(((object)where) != null, "where could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                //dbHelper.Delete(ec.MappingName, ParseExpressionByMetaData(ec, where.ToString()), where.ParamValues, tran);
                Delete(ec.MappingName, where, tran);
                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(typeof(EntityType).ToString());
            }
            else
            {
                Gateway findGateway = this;
                if (db.IsBatchConnection)
                {
                    findGateway = new Gateway(new Database(db.DbProvider));
                    findGateway.Db.OnLog += new LogHandler(db.WriteLog);
                }

                EntityType[] objs = findGateway.FindArray<EntityType>(where, OrderByClip.Default);

                if (db.DbProvider.SupportADO20Transaction && tran == null)
                {
                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                    {
                        foreach (EntityType obj in objs)
                        {
                            Delete(ec, obj, null);
                        }

                        scope.Complete();
                    }
                }
                else
                {
                    DbTransaction t = (tran == null ? BeginTransaction(System.Data.IsolationLevel.ReadUncommitted) : tran);

                    try
                    {
                        foreach (EntityType obj in objs)
                        {
                            Delete(ec, obj, t);
                        }

                        if (tran == null)
                        {
                            t.Commit();
                        }
                    }
                    catch
                    {
                        if (tran == null)
                        {
                            t.Rollback();
                        }
                        throw;
                    }
                    finally
                    {
                        if (tran == null)
                        {
                            CloseTransaction(t);
                        }
                    }
                }

                CascadeRemoveEntityCaches(ec);
            }
        }

        /// <summary>
        /// Batch Deletes the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        public void Delete<EntityType>(WhereClip where)
            where EntityType : Entity, new()
        {
            Delete<EntityType>(where, null);
        }

        /// <summary>
        /// Batch update.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="values">The values.</param>
        /// <param name="where">The where.</param>
        /// <param name="tran">The tran.</param>
        public void Update<EntityType>(PropertyItem[] properties, object[] values, WhereClip where, DbTransaction tran)
           where EntityType : Entity, new()
        {
            Check.Require(properties != null && properties.Length > 0, "properties to update could not be null or empty.");
            Check.Require(values != null && values.Length > 0, "values to update could not be null or empty.");
            Check.Require(properties.Length == values.Length, "length of properties and values should be equal.");
            Check.Require(((object)where) != null, "where could not be null.");

            EntityConfiguration ec = new EntityType().GetEntityConfiguration();

            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                string[] columns = new string[properties.Length];
                DbType[] types = new DbType[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    columns[i] = properties[i].ColumnNameWithoutPrefix;
                    types[i] = properties[i].DbType;
                }

                //dbHelper.Update(ec.MappingName, columns, values, ParseExpressionByMetaData(ec, where.ToString()), where.ParamValues, tran);
                Update(ec.MappingName, where, columns, types, values, tran);
                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(typeof(EntityType).ToString());
            }
            else
            {
                Gateway findGateway = this;
                if (db.IsBatchConnection)
                {
                    findGateway = new Gateway(new Database(db.DbProvider));
                    findGateway.Db.OnLog += new LogHandler(db.WriteLog);
                }
                EntityType[] objs = findGateway.FindArray<EntityType>(where, OrderByClip.Default);
                Dictionary<string, object> changedProperties = new Dictionary<string, object>();
                for (int i = 0; i < properties.Length; i++)
                {
                    changedProperties.Add(properties[i].PropertyName, values[i]);
                }

                if (db.DbProvider.SupportADO20Transaction && tran == null)
                {
                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                    {
                        foreach (EntityType obj in objs)
                        {
                            SetModifiedProperties(changedProperties, obj);
                            Update(obj);
                        }

                        scope.Complete();
                    }
                }
                else
                {
                    DbTransaction t = (tran == null ? BeginTransaction(System.Data.IsolationLevel.ReadUncommitted) : tran);

                    try
                    {
                        foreach (EntityType obj in objs)
                        {
                            SetModifiedProperties(changedProperties, obj);
                            Update(obj, t);
                        }

                        if (tran == null)
                        {
                            t.Commit();
                        }
                    }
                    catch
                    {
                        if (tran == null)
                        {
                            t.Rollback();
                        }
                        throw;
                    }
                    finally
                    {
                        if (tran == null)
                        {
                            CloseTransaction(t);
                        }
                    }
                }

                CascadeRemoveEntityCaches(ec);
            }
        }

        /// <summary>
        /// Batches the update.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="values">The values.</param>
        /// <param name="where">The where.</param>
        public void Update<EntityType>(PropertyItem[] properties, object[] values, WhereClip where)
           where EntityType : Entity, new()
        {
            Update<EntityType>(properties, values, where, null);
        }

        #endregion

        #endregion

        #region DynamicTyped Gateways

        #region Find

        /// <summary>
        /// Finds the specified pk values.
        /// </summary>
        /// <param name="typeName">EntityType name.</param>
        /// <param name="pkValues">The pk values.</param>
        /// <returns>The result.</returns>
        public DynEntity Find(string typeName, params object[] pkValues)
        {
            Check.Require(pkValues != null && pkValues.Length > 0, "pkValues could not be null or empty.");

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            List<PropertyConfiguration> pks = ec.GetPrimaryKeyProperties();

            WhereClip where = BuildEqualWhereClip(ec, pkValues, pks);

            return From(ec,ec.ViewName).Where(where).ToFirst(ec.Name);
        }

        /// <summary>
        /// Finds the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>The result.</returns>
        public DynEntity Find(string typeName, WhereClip where)
        {
            Check.Require(((object)where) != null, "where could not be null.");

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            return From(ec.Name).Where(where).ToFirst(ec.Name);
        }

        /// <summary>
        /// Whether entity exists.
        /// </summary>
        /// <param name="pkValues">The pk values.</param>
        /// <returns>Whether existing.</returns>
        public bool Exists(string typeName, params object[] pkValues)
        {
            Check.Require(pkValues != null && pkValues.Length > 0, "pkValues could not be null or empty.");

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            List<PropertyConfiguration> pks = ec.GetPrimaryKeyProperties();

            WhereClip where = BuildEqualWhereClip(ec, pkValues, pks);

            return Convert.ToInt32(From(ec.Name).Where(where).Select(PropertyItem.All.Count()).ToScalar()) > 0;
        }

        /// <summary>
        /// Whether entity exists.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>Whether existing.</returns>
        public bool Exists(string typeName, WhereClip where)
        {
            Check.Require(((object)where) != null, "where could not be null.");

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            return Convert.ToInt32(From(ec.Name).Where(where).Select(PropertyItem.All.Count()).ToScalar()) > 0;
        }

        /// <summary>
        /// Finds the array.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns>The result.</returns>
        public DynEntity[] FindArray(string typeName, WhereClip where, OrderByClip orderBy)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(orderBy != null, "orderBy could not be null.");

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            return From(ec, ec.ViewName).Where(where).OrderBy(orderBy).ToArray(typeName);
        }

        /// <summary>
        /// Finds the array.
        /// </summary>
        /// <param name="orderBy">The order by.</param>
        /// <returns></returns>
        public DynEntity[] FindArray(string typeName, OrderByClip orderBy)
        {
            return FindArray(typeName, WhereClip.All, orderBy);
        }

        /// <summary>
        /// Finds the array.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns></returns>
        public DynEntity[] FindArray(string typeName, WhereClip where)
        {
            return FindArray(typeName, where, OrderByClip.Default);
        }

        /// <summary>
        /// Finds the array.
        /// </summary>
        /// <returns></returns>
        public DynEntity[] FindArray(string typeName)
        {
            return FindArray(typeName, WhereClip.All, OrderByClip.Default);
        }

        /// <summary>
        /// Finds the data table.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns></returns>
        public DataTable FindDataTable(string typeName, WhereClip where, OrderByClip orderBy)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(orderBy != null, "orderBy could not be null.");

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            DataSet ds = From(typeName).Where(where).OrderBy(orderBy).ToDataSet();
            if (ds != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }
            return null;
        }

        /// <summary>
        /// Finds the single property array.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="property">The property.</param>
        /// <returns>The result.</returns>
        public object[] FindSinglePropertyArray(string typeName, WhereClip where, OrderByClip orderBy, PropertyItem property)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(orderBy != null, "orderBy could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            return From(typeName).Where(where).OrderBy(orderBy).Select(property).ToSinglePropertyArray();
        }

        /// <summary>
        /// Finds the scalar.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="property">The property.</param>
        /// <returns>The result.</returns>
        public object FindScalar(string typeName, WhereClip where, OrderByClip orderBy, PropertyItem property)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(orderBy != null, "orderBy could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            return From(typeName).Where(where).OrderBy(orderBy).Select(property).ToScalar();
        }

        #endregion

        #region Aggregation

        /// <summary>
        /// Counts the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="property">The property.</param>
        /// <param name="isDistinct">if set to <c>true</c> [is distinct].</param>
        /// <returns>The result.</returns>
        public int Count(string entityType, WhereClip where, Rock.Orm.Common.ExpressionClip property, bool isDistinct)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType);

            return Convert.ToInt32(GetAggregateValueEx(property.Count(isDistinct), ec, where));
        }

        /// <summary>
        /// Counts the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>The result.</returns>
        public int Count(string entityType, WhereClip where)
        {
            return Count(entityType, where, PropertyItem.All, false);
        }

        /// <summary>
        /// Counts the pages.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns></returns>
        public int CountPage(string entityType, WhereClip where, int pageSize)
        {
            Check.Require(pageSize > 0, "pageSize must > 0!");

            int rowCount = Count(entityType, where);
            return rowCount % pageSize == 0 ? rowCount / pageSize : (rowCount / pageSize) + 1;
        }

        /// <summary>
        /// Maxes the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public object Max(string entityType, WhereClip where, Rock.Orm.Common.ExpressionClip property)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType);

            return GetAggregateValueEx(property.Max(), ec, where);
        }

        /// <summary>
        /// Mins the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public object Min(string entityType, WhereClip where, Rock.Orm.Common.ExpressionClip property)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType);

            return GetAggregateValueEx(property.Min(), ec, where);
        }

        /// <summary>
        /// Sums the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public object Sum(string entityType, WhereClip where, Rock.Orm.Common.ExpressionClip property)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType);

            return GetAggregateValueEx(property.Sum(), ec, where);
        }

        /// <summary>
        /// Avgs the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public object Avg(string entityType, WhereClip where, Rock.Orm.Common.ExpressionClip property)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(!property.Equals(null), "property could not be null.");

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType);

            return GetAggregateValueEx(property.Avg(), ec, where);
        }

        #endregion

        #region Save

        /// <summary>
        /// Creates the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="tran">The tran.</param>
        /// <returns>The auto incrememtal column value. If there is no auto column, the value is 0.</returns>
        internal int Create(DynEntity obj, DbTransaction tran)
        {
            Check.Require(obj != null, "relatedObj could not be null.");
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(obj.EntityType.FullName);

            if ((!db.IsBatchConnection) && ec.IsBatchUpdate)
            {
                Gateway batchGateway = BeginBatchGateway(ec.BatchSize, tran);
                int retInt = batchGateway.Create(obj, tran);
                batchGateway.EndBatch();
                return retInt;
            }

            string[] pks = Entity.GetPrimaryKeyMappingColumnNames(ec);
            string keyColumn = null;
            if (pks != null && pks.Length == 1)
            {
                PropertyConfiguration pc = null;

                foreach (PropertyConfiguration item in ec.Properties)
                {
                    if (item.MappingName == pks[0])
                    {
                        pc = item;
                        if (item.IsReadOnly)
                        {
                            keyColumn = pks[0];
                        }
                        break;
                    }
                }

                object pkValue = DynEntity.GetPropertyValues(obj, pks)[0];
                if (pc.IsPrimaryKey && (!ec.IsRelation) && pc.SqlType.Trim().ToLower() == "uniqueidentifier" && (pkValue == null || ((Guid)pkValue) == default(Guid)))
                {
                    Util.DeepGetProperty(obj.GetType(), pc.Name).SetValue(obj, Guid.NewGuid(), null);
                }
            }

            //bind query proxy handler to relatedObj
            obj.SetQueryProxy(new DynEntity.QueryProxyHandler(OnQueryHandler)); //zml ???

            int retAutoID = 0;

            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                EntityConfiguration entityConfiguration = MetaDataManager.GetEntityConfiguration(obj.EntityType.FullName);
                string[] createColumnNames = DynEntity.GetCreatePropertyMappingColumnNames(entityConfiguration);
                DbType[] createColumnTypes = DynEntity.GetCreatePropertyMappingColumnTypes(entityConfiguration);
                object[] createColumnValues = DynEntity.GetCreatePropertyValues(obj.EntityType, obj);
                List<string> sqlDefaultValueColumns = MetaDataManager.GetSqlTypeWithDefaultValueColumns(ec.Name);
                if (sqlDefaultValueColumns.Count > 0)
                {
                    FilterNullSqlDefaultValueColumns(sqlDefaultValueColumns, createColumnNames, createColumnTypes, createColumnValues, out createColumnNames, out createColumnTypes, out createColumnValues);
                }
                retAutoID = Insert(ec.MappingName, createColumnNames, createColumnTypes, createColumnValues, tran, keyColumn);

                if (retAutoID > 0)
                {
                    //save the retAutoID value to entity's ID property
                    string autoIdProperty = MetaDataManager.GetEntityAutoId(ec.Name);
                    if (autoIdProperty != null)
                    {
                        PropertyInfo pi = Util.DeepGetProperty(obj.GetType(), autoIdProperty);
                        if (pi != null)
                        {
                            pi.SetValue(obj, Convert.ChangeType(retAutoID, pi.PropertyType), null);
                        }
                    }
                }

                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(obj.EntityType.Name);
            }
            else
            {
                if (db.DbProvider.SupportADO20Transaction && tran == null)
                {
                    //ado2.0 tran
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        retAutoID = DoCascadeInsert(obj, tran, ec, keyColumn);
                        DoCascadePropertySave(obj, tran, ec);

                        scope.Complete();
                    }
                }
                else
                {
                    DbTransaction t;
                    if (tran == null)
                    {
                        t = BeginTransaction();
                    }
                    else
                    {
                        t = tran;
                    }

                    try
                    {
                        retAutoID = DoCascadeInsert(obj, t, ec, keyColumn);
                        DoCascadePropertySave(obj, tran, ec);

                        if (tran == null)
                        {
                            t.Commit();
                        }
                    }
                    catch
                    {
                        if (tran == null)
                        {
                            t.Rollback();
                        }
                        throw;
                    }
                    finally
                    {
                        if (tran == null)
                        {
                            CloseTransaction(t);
                        }
                    }
                }

                CascadeRemoveEntityCaches(ec);
            }

            obj.Attach();
            //obj.SetQueryProxy(new Entity.QueryProxyHandler(OnQueryHandler));zml ???
            return retAutoID;
        }

        /// <summary>
        /// Creates the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The auto incrememtal column value. If there is no auto column, the value is 0.</returns>
        internal int Create(DynEntity obj)
        {
            return Create(obj, null);
        }

        /// <summary>
        /// Updates the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="tran">The tran.</param>
        internal void Update(DynEntity obj, DbTransaction tran)
        {
            Check.Require(obj != null, "relatedObj could not be null.");
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(obj.EntityType.FullName);

            if ((!db.IsBatchConnection) && ec.IsBatchUpdate)
            {
                Gateway batchGateway = BeginBatchGateway(ec.BatchSize, tran);
                batchGateway.Update(obj, tran);
                batchGateway.EndBatch();
                return;
            }

            string[] pks = DynEntity.GetPrimaryKeyMappingColumnNames(ec);
            object[] pkValues = DynEntity.GetPropertyValues(obj, pks);
            List<PropertyConfiguration> pks2 = ec.GetPrimaryKeyProperties();

            WhereClip where = BuildEqualWhereClip(ec, pkValues, pks2);

            Dictionary<string, object> modifiedProperties = obj.GetModifiedProperties(obj.EntityType);
            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                if (modifiedProperties.Count > 0)
                {
                    string[] columnNames = ec.GetMappingColumnNames(new List<string>(modifiedProperties.Keys).ToArray());
                    DbType[] columnTypes = ec.GetMappingColumnTypes(new List<string>(modifiedProperties.Keys).ToArray());
                    object[] columnValues = new List<object>(modifiedProperties.Values).ToArray();
                    Update(ec.MappingName, where, columnNames, columnTypes, columnValues, tran);
                }

                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(ec.Name);
            }
            else
            {
                if (db.DbProvider.SupportADO20Transaction && tran == null)
                {
                    //ado2.0 tran
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        DoDeleteToDeleteObjects(obj, tran);
                        DoCascadeUpdate(obj, tran, ec, where, modifiedProperties);
                        DoCascadePropertySave(obj, tran, ec);

                        scope.Complete();
                    }
                }
                else
                {
                    DbTransaction t;
                    if (tran == null)
                    {
                        t = BeginTransaction();
                    }
                    else
                    {
                        t = tran;
                    }

                    try
                    {
                        DoDeleteToDeleteObjects(obj, tran);
                        DoCascadeUpdate(obj, t, ec, where, modifiedProperties);
                        DoCascadePropertySave(obj, tran, ec);

                        if (tran == null)
                        {
                            t.Commit();
                        }
                    }
                    catch
                    {
                        if (tran == null)
                        {
                            t.Rollback();
                        }
                        throw;
                    }
                    finally
                    {
                        if (tran == null)
                        {
                            CloseTransaction(t);
                        }
                    }
                }

                CascadeRemoveEntityCaches(ec);
            }

            //obj.SetQueryProxy(new Entity.QueryProxyHandler(OnQueryHandler)); by zml ???
        }

        /// <summary>
        /// Updates the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        internal void Update(DynEntity obj)
        {
            Update(obj, null);
        }

        /// <summary>
        /// Saves the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="tran">The tran.</param>
        /// <returns>The auto incrememtal column value. If there is no auto column or the entity is already persisted, the value is 0.</returns>
        public int Save(DynEntity obj, DbTransaction tran)
        {
            Check.Require(obj != null, "relatedObj could not be null.");

            int retAutoID = 0;

            if (obj.IsAttached())
            {
                Update(obj, tran);
            }
            else
            {
                retAutoID = Create(obj, tran);
            }

            return retAutoID;
        }

        /// <summary>
        /// Saves the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The auto incrememtal column value. If there is no auto column or the entity is already persisted, the value is 0.</returns>
        public int Save(DynEntity obj)
        {
            return Save(obj, null);
        }

        internal void Delete(EntityConfiguration ec, DynEntity obj, DbTransaction tran)
        {
            object[] pkValues = DynEntity.GetPrimaryKeyValues(obj);

            if ((!db.IsBatchConnection) && ec.IsBatchUpdate)
            {
                Gateway batchGateway = BeginBatchGateway(ec.BatchSize, tran);
                batchGateway.Delete(ec, obj, tran);
                batchGateway.EndBatch();
                return;
            }

            List<PropertyConfiguration> pks = ec.GetPrimaryKeyProperties();

            WhereClip where = BuildEqualWhereClip(ec, pkValues, pks);

            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                Delete(ec.MappingName, where, tran);
                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(ec.Name);
            }
            else
            {
                bool deletedAsChildEntity = DeleteAsChildDynEntity(ec.Name, pkValues, tran);

                if (!deletedAsChildEntity)
                {
                    if (db.DbProvider.SupportADO20Transaction && tran == null)
                    {
                        //ado2.0 tran
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                        {
                            DoCascadeDelete(obj, tran, ec, where);

                            scope.Complete();
                        }
                    }
                    else
                    {
                        DbTransaction t;
                        if (tran == null)
                        {
                            t = BeginTransaction();
                        }
                        else
                        {
                            t = tran;
                        }

                        try
                        {
                            DoCascadeDelete(obj, t, ec, where);

                            if (tran == null)
                            {
                                t.Commit();
                            }
                        }
                        catch
                        {
                            if (tran == null)
                            {
                                t.Rollback();
                            }
                            throw;
                        }
                        finally
                        {
                            if (tran == null)
                            {
                                CloseTransaction(t);
                            }
                        }
                    }
                }

                CascadeRemoveEntityCaches(ec);
            }

            obj.SetQueryProxy(null);
            obj.Detach();
        }

        /// <summary>
        /// Deletes the specified tran.
        /// </summary>
        /// <param name="tran">The tran.</param>
        /// <param name="pkValues">The pk values.</param>
        public void Delete(string typeName, DbTransaction tran, params object[] pkValues)
        {
            Check.Require(pkValues != null && pkValues.Length > 0, "relatedObj could not be null or empty.");
            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);
            List<PropertyConfiguration> pks = ec.GetPrimaryKeyProperties();

            WhereClip where = BuildEqualWhereClip(ec, pkValues, pks);

            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                Delete(ec.MappingName, where, tran);
                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(typeName);
            }
            else
            {
                Gateway findGateway = this;
                if (db.IsBatchConnection)
                {
                    findGateway = new Gateway(new Database(db.DbProvider));
                    findGateway.Db.OnLog += new LogHandler(db.WriteLog);
                }

                DynEntity obj = findGateway.Find(typeName, pkValues);
                if (obj != null)
                {
                    Delete(ec, obj, tran);
                }

                CascadeRemoveEntityCaches(ec);
            }
        }

        /// <summary>
        /// Deletes the specified pk values.
        /// </summary>
        /// <param name="pkValues">The pk values.</param>
        public void Delete(string typeName, params object[] pkValues)
        {
            Delete(typeName, null, pkValues);
        }

        /// <summary>
        /// Deletes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="tran">The tran.</param>
        public void Delete(DynEntity obj, DbTransaction tran)
        {
            Check.Require(obj != null, "relatedObj could not be null.");

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(obj.EntityType.FullName);
            Delete(ec, obj, tran);
        }

        /// <summary>
        /// Deletes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public void Delete(DynEntity obj)
        {
            Check.Require(obj != null, "relatedObj could not be null.");

            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(obj.EntityType.FullName);
            Delete(ec, obj, null);
        }

        /// <summary>
        /// Batch delete.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="tran">The tran.</param>
        public void Delete(WhereClip where, string typeName, DbTransaction tran)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);
            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                Delete(ec.MappingName, where, tran);
                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(typeName);
            }
            else
            {
                Gateway findGateway = this;
                if (db.IsBatchConnection)
                {
                    findGateway = new Gateway(new Database(db.DbProvider));
                    findGateway.Db.OnLog += new LogHandler(db.WriteLog);
                }

                DynEntity[] objs = findGateway.FindArray(typeName, where, OrderByClip.Default);

                if (db.DbProvider.SupportADO20Transaction && tran == null)
                {
                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                    {
                        foreach (DynEntity obj in objs)
                        {
                            Delete(ec, obj, null);
                        }

                        scope.Complete();
                    }
                }
                else
                {
                    DbTransaction t = (tran == null ? BeginTransaction(System.Data.IsolationLevel.ReadUncommitted) : tran);

                    try
                    {
                        foreach (DynEntity obj in objs)
                        {
                            Delete(ec, obj, t);
                        }

                        if (tran == null)
                        {
                            t.Commit();
                        }
                    }
                    catch
                    {
                        if (tran == null)
                        {
                            t.Rollback();
                        }
                        throw;
                    }
                    finally
                    {
                        if (tran == null)
                        {
                            CloseTransaction(t);
                        }
                    }
                }

                CascadeRemoveEntityCaches(ec);
            }
        }

        /// <summary>
        /// Batch Deletes the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        public void Delete(string entityType, WhereClip where)
        {
            Delete(entityType, where, null);
        }

        /// <summary>
        /// Batch update.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="values">The values.</param>
        /// <param name="where">The where.</param>
        /// <param name="tran">The tran.</param>
        public void Update(string typeName, PropertyItem[] properties, object[] values, WhereClip where, DbTransaction tran)
        {
            Check.Require(properties != null && properties.Length > 0, "properties to update could not be null or empty.");
            Check.Require(values != null && values.Length > 0, "values to update could not be null or empty.");
            Check.Require(properties.Length == values.Length, "length of properties and values should be equal.");
            Check.Require(((object)where) != null, "where could not be null.");

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            if (MetaDataManager.IsNonRelatedEntity(ec.Name))
            {
                string[] columns = new string[properties.Length];
                DbType[] types = new DbType[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    columns[i] = properties[i].ColumnNameWithoutPrefix;
                    types[i] = properties[i].DbType;
                }

                Update(ec.MappingName, where, columns, types, values, tran);
                if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad) RemoveCaches(typeName);
            }
            else
            {
                Gateway findGateway = this;
                if (db.IsBatchConnection)
                {
                    findGateway = new Gateway(new Database(db.DbProvider));
                    findGateway.Db.OnLog += new LogHandler(db.WriteLog);
                }
                DynEntity[] objs = findGateway.FindArray(typeName, where, OrderByClip.Default);
                Dictionary<string, object> changedProperties = new Dictionary<string, object>();
                for (int i = 0; i < properties.Length; i++)
                {
                    changedProperties.Add(properties[i].PropertyName, values[i]);
                }

                if (db.DbProvider.SupportADO20Transaction && tran == null)
                {
                    TransactionOptions option = new TransactionOptions();
                    option.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                    {
                        foreach (DynEntity obj in objs)
                        {
                            SetModifiedProperties(changedProperties, obj);
                            Update(obj);
                        }

                        scope.Complete();
                    }
                }
                else
                {
                    DbTransaction t = (tran == null ? BeginTransaction(System.Data.IsolationLevel.ReadUncommitted) : tran);

                    try
                    {
                        foreach (DynEntity obj in objs)
                        {
                            SetModifiedProperties(changedProperties, obj);
                            Update(obj, t);
                        }

                        if (tran == null)
                        {
                            t.Commit();
                        }
                    }
                    catch
                    {
                        if (tran == null)
                        {
                            t.Rollback();
                        }
                        throw;
                    }
                    finally
                    {
                        if (tran == null)
                        {
                            CloseTransaction(t);
                        }
                    }
                }

                CascadeRemoveEntityCaches(ec);
            }
        }

        /// <summary>
        /// Batches the update.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="values">The values.</param>
        /// <param name="where">The where.</param>
        public void Update(string entityType, PropertyItem[] properties, object[] values, WhereClip where)
        {
            Update(entityType, properties, values, where, null);
        }

        #endregion

        #endregion

        #region Caching

        private string FilterParams(string sql, Dictionary<string, KeyValuePair<DbType, object>> parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return sql;
            }

            Dictionary<string, KeyValuePair<DbType, object>>.Enumerator en = parameters.GetEnumerator();
            while (en.MoveNext())
            {
                sql = sql.Replace('@' + en.Current.Key, "?");
            }

            return sql;
        }

        /// <summary>
        /// Computes the cache key.
        /// </summary>
        /// <param name="customPrefix">The custom prefix.</param>
        /// <param name="where">The where.</param>
        /// <returns>The cache key.</returns>
        public string ComputeCacheKey(string customPrefix, WhereClip where)
        {
            Check.Require(((object)where) != null, "where could not be null.");

            StringBuilder sb = new StringBuilder();
            sb.Append(customPrefix);
            sb.Append('_');
            sb.Append(FilterParams(where.ToString(), where.Parameters));
            if (where.Parameters.Values.Count > 0)
            {
                Dictionary<string, KeyValuePair<DbType, object>>.ValueCollection.Enumerator en = where.Parameters.Values.GetEnumerator();
                while (en.MoveNext())
                {
                    sb.Append(SerializationManager.Serialize(en.Current.Value));
                }
            }
            sb.Append(db.ConnectionString);
            return sb.ToString();
        }

        /// <summary>
        /// Computes the cache key.
        /// </summary>
        /// <param name="customPrefix">The custom prefix.</param>
        /// <param name="where">The where.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns>The cache key.</returns>
        [Obsolete]
        public string ComputeCacheKey(string customPrefix, WhereClip where, OrderByClip orderBy)
        {
            Check.Require(((object)where) != null, "where could not be null.");
            Check.Require(orderBy != null, "orderBy could not be null.");

            return ComputeCacheKey(string.Format("{0}{1}", customPrefix, orderBy.ToString()), where);
        }

        internal static readonly Cache cache = new Cache();

        private static readonly Dictionary<string, List<string>> cascadeExpireCacheItems = new Dictionary<string, List<string>>();

        private CacheConfigurationSection cacheConfigSection = null;

        private Dictionary<string, int> tableExpireSecondsMap = null;

        /// <summary>
        /// Gets the table cache expire seconds.
        /// </summary>
        /// <param name="tableOrViewName">Name of the table or view.</param>
        /// <returns>The cache key.</returns>
        public int GetTableCacheExpireSeconds(string tableOrViewName)
        {
            if (!IsCacheTurnedOn)
            {
                return 0;
            }

            string key = tableOrViewName.ToLower();
            int second = 0;
            tableExpireSecondsMap.TryGetValue(key, out second);
            return second;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is cache turned on.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is cache turned on; otherwise, <c>false</c>.
        /// </value>
        public bool IsCacheTurnedOn
        {
            get
            {
                return (cacheConfigSection != null && cacheConfigSection.Enable);
            }
        }

        /// <summary>
        /// Turns on the cache of this gateway.
        /// </summary>
        public void TurnOnCache()
        {
            if (cacheConfigSection != null)
            {
                cacheConfigSection.Enable = true;
            }
        }

        /// <summary>
        /// Turns off the cache of this gateway.
        /// </summary>
        public void TurnOffCache()
        {
            if (cacheConfigSection != null)
            {
                cacheConfigSection.Enable = false;
            }
        }

        /// <summary>
        /// Adds object into the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expireAfterSeconds">The expire after seconds.</param>
        public void AddCache(string key, object value, int expireAfterSeconds)
        {
            Check.Require(key != null, "key could not be null.");
            //Check.Require(value != null, "value could not be null.");
            Check.Require(expireAfterSeconds > 0, "expireAfterSeconds must > 0.");

            cache.Add(key, value, new AbsoluteTime(DateTime.Now.AddSeconds(expireAfterSeconds)));
        }

        /// <summary>
        /// Adds the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expirations">The expirations.</param>
        public void AddCache(string key, object value, params ICacheItemExpiration[] expirations)
        {
            Check.Require(key != null, "key could not be null.");

            cache.Add(key, value, expirations);
        }

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The cached object, if not exists, return null.</returns>
        public object GetCache(string key)
        {
            Check.Require(key != null, "key could not be null.");

            return cache.Get(key);
        }

        /// <summary>
        /// Removes the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        public void RemoveCache(string key)
        {
            Check.Require(key != null, "key could not be null.");

            cache.Remove(key);

            RemoveCascadeExpireCacheItems(key);
        }

        public void AddCascadeExpireCacheItem(string baseKey, string itemKey)
        {
            List<string> values;
            lock (cascadeExpireCacheItems)
            {
                if (!cascadeExpireCacheItems.TryGetValue(baseKey, out values))
                {
                    values = new List<string>();
                    cascadeExpireCacheItems.Add(baseKey, values);
                }

                values.Add(itemKey);
            }
        }

        private void RemoveCascadeExpireCacheItems(string key)
        {
            List<string> values;
            if (cascadeExpireCacheItems.TryGetValue(key, out values))
            {
                lock (cascadeExpireCacheItems)
                {
                    if (cascadeExpireCacheItems.TryGetValue(key, out values))
                    {
                        foreach (string cascadeKey in values)
                        {
                            RemoveCache(cascadeKey);
                        }
                        cascadeExpireCacheItems.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Removes all the caches related to specified table, view or stored procedure.
        /// </summary>
        /// <param name="keyPrefix">Name of the table view or stored proc.</param>
        public void RemoveCaches(string keyPrefix)
        {
            Check.Require(keyPrefix != null, "keyPrefix could not be null.");

            if (!IsCacheTurnedOn)
            {
                return;
            }

            cache.RemoveByKeyPrefix(keyPrefix + "|");
            RemoveCascadeExpireCacheItems(keyPrefix);
        }

        /// <summary>
        /// Removes the caches.
        /// </summary>
        public void RemoveCaches<EntityType>()
        {
            RemoveCaches(typeof(EntityType).ToString());
        }

        private void CascadeRemoveEntityCaches(EntityConfiguration ec)
        {
            if (!IsCacheTurnedOn)
            {
                return;
            }

            if (GetTableCacheExpireSeconds(ec.ViewName) > 0 || ec.IsAutoPreLoad)
            {
                //remove cache of self
                RemoveCaches(ec.Name);

                //remove cache of contained properties
                foreach (PropertyConfiguration pc in ec.Properties)
                {
                    if (pc.IsContained || pc.QueryType == "ManyToManyQuery")
                    {
                        //if (!string.IsNullOrEmpty(pc.RelatedType))
                        //{
                        RemoveCaches(pc.RelatedType);
                        //}
                        if (!string.IsNullOrEmpty(pc.RelationType))
                        {
                            RemoveCaches(pc.RelationType);
                        }
                    }
                }
            }

            List<EntityConfiguration> childList = MetaDataManager.GetChildEntityConfigurations(ec.Name);
            foreach (EntityConfiguration childEc in childList)
            {
                CascadeRemoveEntityCaches(childEc);
            }
        }

        internal void PreLoadEntities<EntityType>(EntityConfiguration ec)
            where EntityType : Entity, new()
        {
            if (ec.IsAutoPreLoad)
            {
                //preload all
                int cacheSeconds = GetTableCacheExpireSeconds(ec.ViewName);
                if (cacheSeconds == 0)
                {
                    cacheSeconds = int.MaxValue; //never expire by time
                }

                IDataReader reader = From(ec, ec.ViewName).ToDataReader();

                List<EntityType> list = new List<EntityType>();
                while (reader.Read())
                {
                    EntityType obj = CreateEntity<EntityType>();
                    obj.SetPropertyValues(reader);
                    list.Add(obj);
                }
                reader.Close();
                reader.Dispose();

                DataTable dt = Entity.EntityArrayToDataTable<EntityType>(list.ToArray());
                AddCache(ComputeCacheKey(ec.Name + "|PreLoad", WhereClip.All), dt, cacheSeconds);
            }
        }

        internal void PreLoadEntities(string typeName, EntityConfiguration ec)
        {
            if (ec.IsAutoPreLoad)
            {
                //preload all
                int cacheSeconds = GetTableCacheExpireSeconds(ec.ViewName);
                if (cacheSeconds == 0)
                {
                    cacheSeconds = int.MaxValue; //never expire by time
                }

                IDataReader reader = From(ec, ec.ViewName).ToDataReader();

                List<DynEntity> list = new List<DynEntity>();
                while (reader.Read())
                {
                    DynEntity obj = CreateEntity(typeName);
                    obj.SetPropertyValues(reader);
                    list.Add(obj);
                }
                reader.Close();
                reader.Dispose();

                DataTable dt = DynEntity.EntityArrayToDataTable(DynEntityTypeManager.GetEntityTypeMandatory(typeName), list.ToArray());
                AddCache(ComputeCacheKey(ec.Name + "|PreLoad", WhereClip.All), dt, cacheSeconds);
            }
        }

        private static string RemoveTableAliasNamePrefixes(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return sql;
            }

            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\[([\w\d\s_]+)\].\[([\w\d\s_]+)\]");
            sql = r.Replace(sql, "[$2]");
            return sql;
        }

        internal object FindScalarFromPreLoadEx<EntityType>(Rock.Orm.Common.ExpressionClip expr, EntityConfiguration ec, WhereClip where)
            where EntityType : Entity, new()
        {
            string whereStr = FilterNTextPrefix(RemoveTableAliasNamePrefixes(ToFlatWhereClip(where, ec)));
            string column = RemoveTableAliasNamePrefixes(expr.ToString());

            string cacheKey = ComputeCacheKey(ec.Name + "|PreLoad", WhereClip.All);
            if (cache.Get(cacheKey) == null)
            {
                PreLoadEntities<EntityType>(ec);
            }
            DataTable dt = (DataTable)GetCache(cacheKey);
            if (dt == null)
            {
                PreLoadEntities<EntityType>(ec);
                dt = (DataTable)GetCache(cacheKey);
            }
            if (dt != null)
            {
                if (column.Contains("("))
                {
                    //aggregate query
                    if (column.StartsWith("COUNT(DISTINCT "))
                    {
                        string columName = column.Substring(15).Trim(' ', ')').Replace(db.DbProvider.LeftToken, string.Empty).Replace(db.DbProvider.RightToken, string.Empty);
                        List<object> list = new List<object>();
                        foreach (DataRow row in dt.Rows)
                        {
                            object columnValue = row[columName];
                            if (!list.Contains(columnValue))
                            {
                                list.Add(columnValue);
                            }
                        }
                        return list.Count;
                    }
                    else if (column.StartsWith("COUNT("))
                    {
                        return dt.Rows.Count;
                    }
                    else
                    {
                        return dt.Compute(column, WhereClip.IsNullOrEmpty(where) ? null : ToFlatWhereClip(where, ec).ToString());
                    }
                }
                else
                {
                    //scalar query
                    DataRow[] rows;
                    if (WhereClip.IsNullOrEmpty(where))
                    {
                        rows = dt.Select();
                    }
                    else
                    {
                        rows = dt.Select(whereStr);
                    }
                    if (rows != null && rows.Length > 0)
                    {
                        return rows[0][column.TrimStart('[').TrimEnd(']')];
                    }
                }
            }

            return 0;
        }

        //internal object[] FindSinglePropertyArrayFromPreLoadEx<EntityType>(PropertyItem property, EntityConfiguration ec, WhereClip where)
        //    where EntityType : Entity, new()
        //{
        //    string column = property.ColumnName;
        //    column = RemoveTableAliasNamePrefixes(column);
        //    string[] splittedColumn = column.Split('.');
        //    column = splittedColumn[splittedColumn.Length - 1];

        //    DataTable dt = FindTableFromPreLoadEx<EntityType>(ec, where);
        //    List<object> list = new List<object>();

        //    if (dt != null && dt.Rows.Count > 0)
        //    {
        //        column = column.TrimStart('[').TrimEnd(']');
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            list.Add(row[column]);
        //        }
        //    }

        //    return list.ToArray();
        //}

        internal DataTable FindTableFromPreLoadEx<EntityType>(EntityConfiguration ec, WhereClip where, int topCount, int skipCount)
            where EntityType : Entity, new()
        {
            string whereStr = FilterNTextPrefix(RemoveTableAliasNamePrefixes(ToFlatWhereClip(where, ec)));
            string orderByStr = RemoveTableAliasNamePrefixes(where.OrderBy);

            string cacheKey = ComputeCacheKey(ec.Name + "|PreLoad", WhereClip.All);
            if (cache.Get(cacheKey) == null)
            {
                PreLoadEntities<EntityType>(ec);
            }
            DataTable dt = (DataTable)GetCache(cacheKey);
            if (dt == null)
            {
                PreLoadEntities<EntityType>(ec);
                dt = (DataTable)GetCache(cacheKey);
            }
            if (dt != null)
            {
                DataRow[] rows;
                if (string.IsNullOrEmpty(whereStr) && string.IsNullOrEmpty(orderByStr))
                {
                    rows = dt.Select();
                }
                else if (string.IsNullOrEmpty(whereStr))
                {
                    rows = dt.Select(null, orderByStr);
                }
                else if (string.IsNullOrEmpty(orderByStr))
                {
                    rows = dt.Select(whereStr);
                }
                else
                {
                    rows = dt.Select(whereStr, orderByStr);
                }
                if (rows != null && rows.Length > 0)
                {
                    DataTable retDt = dt.Clone();
                    int i = 0;
                    foreach (DataRow row in rows)
                    {
                        if (i >= skipCount)
                        {
                            DataRow newRow = retDt.NewRow();
                            newRow.ItemArray = row.ItemArray;
                            retDt.Rows.Add(newRow);
                        }

                        i++;

                        if (retDt.Rows.Count >= topCount)
                        {
                            break;
                        }
                    }
                    return retDt;
                }
            }
            return null;
        }

        internal object[] FindSinglePropertyArrayFromPreLoadEx<EntityType>(string column, EntityConfiguration ec, WhereClip where, int topCount, int skipCount)
            where EntityType : Entity, new()
        {
            //string column = property.ColumnName;
            column = RemoveTableAliasNamePrefixes(column);
            string[] splittedColumn = column.Split('.');
            column = splittedColumn[splittedColumn.Length - 1];

            DataTable dt = FindTableFromPreLoadEx<EntityType>(ec, where);
            List<object> list = new List<object>();

            if (dt != null && dt.Rows.Count > 0)
            {
                column = column.TrimStart('[').TrimEnd(']');
                int i = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (i >= skipCount)
                    {
                        list.Add(row[column]);
                    }

                    i++;

                    if (list.Count >= topCount)
                    {
                        break;
                    }
                }
            }

            return list.ToArray();
        }

        internal object[] FindSinglePropertyArrayFromPreLoadEx<EntityType>(string column, EntityConfiguration ec, WhereClip where)
            where EntityType : Entity, new()
        {
            return FindSinglePropertyArrayFromPreLoadEx<EntityType>(column, ec, where, int.MaxValue, 0);
        }

        internal DataTable FindTableFromPreLoadEx<EntityType>(EntityConfiguration ec, WhereClip where, int topCount)
            where EntityType : Entity, new()
        {
            return FindTableFromPreLoadEx<EntityType>(ec, where, topCount, 0);
        }

        internal DataTable FindTableFromPreLoadEx<EntityType>(EntityConfiguration ec, WhereClip where)
            where EntityType : Entity, new()
        {
            return FindTableFromPreLoadEx<EntityType>(ec, where, int.MaxValue, 0);
        }

        internal EntityType[] FindArrayFromPreLoadEx<EntityType>(EntityConfiguration ec, WhereClip where, int topCount, int skipCount)
            where EntityType : Entity, new()
        {
            string whereStr = FilterNTextPrefix(RemoveTableAliasNamePrefixes(ToFlatWhereClip(where, ec)));
            string orderByStr = RemoveTableAliasNamePrefixes(where.OrderBy);

            string cacheKey = ComputeCacheKey(ec.Name + "|PreLoad", WhereClip.All);
            if (cache.Get(cacheKey) == null)
            {
                PreLoadEntities<EntityType>(ec);
            }
            DataTable dt = (DataTable)GetCache(cacheKey);
            if (dt == null)
            {
                PreLoadEntities<EntityType>(ec);
                dt = (DataTable)GetCache(cacheKey);
            }
            if (dt != null)
            {
                DataRow[] rows;
                if (string.IsNullOrEmpty(whereStr) && string.IsNullOrEmpty(orderByStr))
                {
                    rows = dt.Select();
                }
                else if (string.IsNullOrEmpty(whereStr))
                {
                    rows = dt.Select(null, orderByStr);
                }
                else if (string.IsNullOrEmpty(orderByStr))
                {
                    rows = dt.Select(whereStr);
                }
                else
                {
                    rows = dt.Select(whereStr, orderByStr);
                }
                if (rows != null && rows.Length > 0)
                {
                    List<EntityType> list = new List<EntityType>();
                    int i = 0;
                    foreach (DataRow row in rows)
                    {
                        if (i >= skipCount)
                        {
                            EntityType retObj = CreateEntity<EntityType>();
                            retObj.SetPropertyValues(row);
                            list.Add(retObj);
                        }

                        i++;

                        if (list.Count >= topCount)
                        {
                            break;
                        }
                    }
                    return list.ToArray();
                }
            }
            return new EntityType[0];
        }

        internal DynEntity[] FindArrayFromPreLoadEx(string entityType, EntityConfiguration ec, WhereClip where, int topCount, int skipCount)
        {
            string whereStr = FilterNTextPrefix(RemoveTableAliasNamePrefixes(ToFlatWhereClip(where, ec)));
            string orderByStr = RemoveTableAliasNamePrefixes(where.OrderBy);

            string cacheKey = ComputeCacheKey(ec.Name + "|PreLoad", WhereClip.All);
            if (cache.Get(cacheKey) == null)
            {
                PreLoadEntities(entityType, ec);
            }
            DataTable dt = (DataTable)GetCache(cacheKey);
            if (dt == null)
            {
                PreLoadEntities(entityType, ec);
                dt = (DataTable)GetCache(cacheKey);
            }
            if (dt != null)
            {
                DataRow[] rows;
                if (string.IsNullOrEmpty(whereStr) && string.IsNullOrEmpty(orderByStr))
                {
                    rows = dt.Select();
                }
                else if (string.IsNullOrEmpty(whereStr))
                {
                    rows = dt.Select(null, orderByStr);
                }
                else if (string.IsNullOrEmpty(orderByStr))
                {
                    rows = dt.Select(whereStr);
                }
                else
                {
                    rows = dt.Select(whereStr, orderByStr);
                }
                if (rows != null && rows.Length > 0)
                {
                    List<DynEntity> list = new List<DynEntity>();
                    int i = 0;
                    foreach (DataRow row in rows)
                    {
                        if (i >= skipCount)
                        {
                            DynEntity retObj = CreateEntity(entityType);
                            retObj.SetPropertyValues(row);
                            list.Add(retObj);
                        }

                        i++;

                        if (list.Count >= topCount)
                        {
                            break;
                        }
                    }
                    return list.ToArray();
                }
            }
            return new DynEntity[0];
        }

        internal EntityType[] FindArrayFromPreLoadEx<EntityType>(EntityConfiguration ec, WhereClip where, int topCount)
            where EntityType : Entity, new()
        {
            return FindArrayFromPreLoadEx<EntityType>(ec, where, topCount, 0);
        }

        internal DynEntity[] FindArrayFromPreLoadEx(string entityType, EntityConfiguration ec, WhereClip where, int topCount)
        {
            return FindArrayFromPreLoadEx(entityType, ec, where, topCount, 0);
        }

        internal EntityType[] FindArrayFromPreLoadEx<EntityType>(EntityConfiguration ec, WhereClip where)
            where EntityType : Entity, new()
        {
            return FindArrayFromPreLoadEx<EntityType>(ec, where, int.MaxValue, 0);
        }

        internal DynEntity[] FindArrayFromPreLoadEx(string entityType, EntityConfiguration ec, WhereClip where)
        {
            return FindArrayFromPreLoadEx(entityType, ec, where, int.MaxValue, 0);
        }

        internal EntityType FindFromPreLoadEx<EntityType>(EntityConfiguration ec, WhereClip where)
            where EntityType : Entity, new()
        {
            string whereStr = FilterNTextPrefix(RemoveTableAliasNamePrefixes(ToFlatWhereClip(where, ec)));
            string orderByStr = RemoveTableAliasNamePrefixes(where.OrderBy);

            string cacheKey = ComputeCacheKey(ec.Name + "|PreLoad", WhereClip.All);
            if (cache.Get(cacheKey) == null)
            {
                PreLoadEntities<EntityType>(ec);
            }
            DataTable dt = (DataTable)GetCache(cacheKey);
            if (dt == null)
            {
                PreLoadEntities<EntityType>(ec);
                dt = (DataTable)GetCache(cacheKey);
            }
            if (dt != null)
            {
                DataRow[] rows;
                if (string.IsNullOrEmpty(whereStr) && string.IsNullOrEmpty(orderByStr))
                {
                    rows = dt.Select();
                }
                else if (string.IsNullOrEmpty(whereStr))
                {
                    rows = dt.Select(null, orderByStr);
                }
                else if (string.IsNullOrEmpty(orderByStr))
                {
                    rows = dt.Select(whereStr);
                }
                else
                {
                    rows = dt.Select(whereStr, orderByStr);
                }
                if (rows != null && rows.Length > 0)
                {
                    EntityType retObj = CreateEntity<EntityType>();
                    retObj.SetPropertyValues(rows[0]);
                    return retObj;
                }
            }
            return null;
        }

        internal DynEntity FindFromPreLoadEx(string entityType, EntityConfiguration ec, WhereClip where)
        {
            string whereStr = FilterNTextPrefix(RemoveTableAliasNamePrefixes(ToFlatWhereClip(where, ec)));
            string orderByStr = RemoveTableAliasNamePrefixes(where.OrderBy);

            string cacheKey = ComputeCacheKey(ec.Name + "|PreLoad", WhereClip.All);
            if (cache.Get(cacheKey) == null)
            {
                PreLoadEntities(entityType, ec);
            }
            DataTable dt = (DataTable)GetCache(cacheKey);
            if (dt == null)
            {
                PreLoadEntities(entityType, ec);
                dt = (DataTable)GetCache(cacheKey);
            }
            if (dt != null)
            {
                DataRow[] rows;
                if (string.IsNullOrEmpty(whereStr) && string.IsNullOrEmpty(orderByStr))
                {
                    rows = dt.Select();
                }
                else if (string.IsNullOrEmpty(whereStr))
                {
                    rows = dt.Select(null, orderByStr);
                }
                else if (string.IsNullOrEmpty(orderByStr))
                {
                    rows = dt.Select(whereStr);
                }
                else
                {
                    rows = dt.Select(whereStr, orderByStr);
                }
                if (rows != null && rows.Length > 0)
                {
                    DynEntity retObj = CreateEntity(entityType);
                    retObj.SetPropertyValues(rows[0]);
                    return retObj;
                }
            }
            return null;
        }

        #endregion

        #region EntityQueryEx Code

        public FromSection From<EntityType>() where EntityType : Entity, new()
        {
            EntityConfiguration ec = new EntityType().GetEntityConfiguration();
            return From(ec, ec.ViewName);
        }

        public FromSection From<EntityType>(string aliasName) where EntityType : Entity, new()
        {
            Check.Require(aliasName != null, "tableAliasName could not be null.");

            return From(new EntityType().GetEntityConfiguration(), aliasName);
        }

        public FromSection From(string typeName)
        {
            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            return From(ec, ec.ViewName);
        }

        public FromSection From(string typeName, string aliasName)
        {
            Check.Require(aliasName != null, "tableAliasName could not be null.");

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);

            return From(MetaDataManager.GetEntityConfiguration(entityType.FullName), aliasName);
        }

        internal FromSection From(EntityConfiguration ec, string aliasName)
        {
            Check.Require(ec != null, "ec could not be null.");
            Check.Require(aliasName != null, "tableAliasName could not be null.");

            return new FromSection(this, ec, aliasName);
        }

        public CustomSqlSection FromCustomSql(string sql)
        {
            Check.Require(!string.IsNullOrEmpty(sql), "sql could not be null or empty!");

            return new CustomSqlSection(this, sql);
        }

        public StoredProcedureSection FromStoredProcedure(string spName)
        {
            Check.Require(!string.IsNullOrEmpty(spName), "spName could not be null or empty!");

            return new StoredProcedureSection(this, spName);
        }

        #endregion


        #region  Rock添加
        //static Dictionary<string, Gateway> _gateways = new Dictionary<string, Gateway>();
        //private static Dictionary<string, DbTransaction> _transDictionary = new Dictionary<string, DbTransaction>();
        //private static Dictionary<string, DateTime> _transTime = new Dictionary<string, DateTime>();
        //private static Dictionary<string, string> _connStr = new Dictionary<string, string>();

        //public static void BeginTransaction(string key)
        //{
        //    if (!_gateways.TryGetValue("Default"))
        //    {
        //        _gateways.Add("Default", new Gateway("Default"));
        //    }

        //    DbTransaction trans = _gateways["Default"].BeginTransaction();
        //    _transDictionary.Add(key, trans);
        //    _transTime.Add(key, DateTime.Now);
        //}

        //public static void BeginTransaction(string key, string connStrName)
        //{
        //    if (!_gateways.TryGetValue(connStrName))
        //    {
        //        _gateways.Add(connStrName, new Gateway(connStrName));
        //    }

        //    DbTransaction trans = _gateways[connStrName].BeginTransaction();
        //    _transDictionary.Add(key, trans);
        //    _transTime.Add(key, DateTime.Now);
        //}

        //public static string DefultDBName = "Default";

        //public static DbTransaction GetTransaction(string dbkey)
        //{
        //    Gateway gateway = null;
        //    if (!_gateways.TryGetValue(dbkey, out gateway))
        //    {
        //        gateway = new Gateway(dbkey);
        //        _gateways.Add(dbkey, gateway);
        //    }

        //    return gateway.BeginTransaction();
        //}

        //public static DbTransaction GetTransaction()
        //{
        //    return GetTransaction(DefultDBName);
        //}

        //public static void Commit(string key)
        //{
        //    if (_transDictionary.ContainsKey(key))
        //    {
        //        _transDictionary[key].Commit();
        //        _transDictionary[key].Dispose();
        //        _transDictionary.Remove(key);
        //    }

        //    顺便清理过期的事务
        //    CleanTransaction();
        //}

        //public static void Rollback(string key)
        //{
        //    if (_transDictionary.ContainsKey(key))
        //    {
        //        _transDictionary[key].Rollback();
        //        _transDictionary[key].Dispose();
        //        _transDictionary.Remove(key);
        //    }
        //}

        //public static void CleanTransaction()
        //{
        //    foreach (var key in _transTime.Keys)
        //    {
        //        TimeSpan ts = DateTime.Now - _transTime[key];
        //        if (ts.Minutes > 10)
        //        {
        //            Rollback(key);
        //            _transDictionary[key].Dispose();
        //            _transDictionary.Remove(key);
        //        }
        //    }
        //}

        #endregion
    }
}
