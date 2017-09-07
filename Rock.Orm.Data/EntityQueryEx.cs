using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

using Rock.Orm.Common;

namespace Rock.Orm.Data
{
    public sealed class FromSection : IQuery
    {
        #region Private & Internal Members

        internal Gateway gateway;
        internal Rock.Orm.Common.FromClip fromClip;
        internal EntityConfiguration ec;
        internal string aliasName;

        #endregion

        #region Constructors

        public FromSection(Gateway gateway, EntityConfiguration ec, string aliasName)
        {
            Check.Require(gateway != null, "gateway could not be null.");
            Check.Require(ec != null, "ec could not be null.");
            Check.Require(aliasName != null, "tableAliasName could not be null.");

            this.gateway = gateway;
            this.ec = ec;
            string aliasNamePrefix = aliasName == ec.ViewName ? string.Empty : aliasName + '_';
            this.fromClip = new Rock.Orm.Common.FromClip(ec.ViewName, aliasNamePrefix + ec.ViewName);
            this.aliasName = aliasName;
        }

        public FromSection(Gateway gateway, EntityConfiguration ec, Rock.Orm.Common.FromClip fromClip)
        {
            Check.Require(gateway != null, "gateway could not be null.");
            Check.Require(ec != null, "ec could not be null.");
            Check.Require(fromClip != null, "fromClip could not be null.");

            this.gateway = gateway;
            this.ec = ec;
            this.fromClip = fromClip;
        }

        #endregion

        #region Public Methods

        public FromSection Join<EntityType>(WhereClip onWhere) where EntityType : Entity, new()
        {
            EntityConfiguration joinEc = new EntityType().GetEntityConfiguration();
            return Join(joinEc, joinEc.ViewName, onWhere);
        }

        public FromSection Join<EntityType>(string aliasName, WhereClip onWhere) where EntityType : Entity, new()
        {
            return Join(new EntityType().GetEntityConfiguration(), aliasName, onWhere);
        }

        public FromSection Join(string typeName, WhereClip onWhere)
        {
            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration joinEc = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            return Join(joinEc, joinEc.ViewName, onWhere);
        }

        public FromSection Join(string typeName, string aliasName, WhereClip onWhere)
        {
            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            EntityConfiguration ec = MetaDataManager.GetEntityConfiguration(entityType.FullName);

            return Join(ec, aliasName, onWhere);
        }

        public FromSection Join(EntityConfiguration joinEc, string aliasName, WhereClip onWhere)
        {
            Check.Require(joinEc != null, "joinEc could not be null.");
            Check.Require(aliasName != null, "tableAliasName could not be null.");

            string aliasNamePrefix = aliasName == joinEc.ViewName ? string.Empty : aliasName + '_';
            if (joinEc.BaseEntity == null)
            {
                fromClip.Join(joinEc.ViewName, aliasNamePrefix + joinEc.ViewName, new WhereClip().And(onWhere));
            }
            else
            {
                Rock.Orm.Common.FromClip appendFromClip = gateway.ConstructFrom(joinEc, aliasNamePrefix);
                fromClip.Join(appendFromClip.TableOrViewName, appendFromClip.AliasName, onWhere);
                Dictionary<string, KeyValuePair<string, WhereClip>>.Enumerator en = appendFromClip.Joins.GetEnumerator();
                while (en.MoveNext())
                {
                    fromClip.Join(en.Current.Value.Key, en.Current.Key, en.Current.Value.Value);
                }
            }
            return this;
        }

        #endregion

        #region IQuery Members

        public QuerySection Where(WhereClip where)
        {
            Check.Require(((object)where) != null, "where could not be null.");

            return new QuerySection(this).Where(where);
        }

        public QuerySection OrderBy(OrderByClip orderBy)
        {
            Check.Require(orderBy != null, "orderBy could not be null.");

            return new QuerySection(this).OrderBy(orderBy);
        }

        public QuerySection GroupBy(GroupByClip groupBy)
        {
            Check.Require(groupBy != null, "groupBy could not be null.");

            return new QuerySection(this).GroupBy(groupBy);
        }

        public QuerySection GroupBy(params PropertyItem[] properties)
        {
            Check.Require(properties != null && properties.Length > 0, "properties could not be null or empty.");

            return new QuerySection(this).GroupBy(properties);
        }

        public QuerySection Select(params Rock.Orm.Common.ExpressionClip[] properties)
        {
            return new QuerySection(this).Select(properties);
        }

        public EntityType[] ToArray<EntityType>(int topItemCount) where EntityType : Rock.Orm.Common.Entity, new()
        {
            return new QuerySection(this).ToArray<EntityType>(topItemCount);
        }

        public EntityType[] ToArray<EntityType>(int topItemCount, int skipItemCount) where EntityType : Rock.Orm.Common.Entity, new()
        {
            return new QuerySection(this).ToArray<EntityType>(topItemCount, skipItemCount);
        }

        public EntityType[] ToArray<EntityType>() where EntityType : Rock.Orm.Common.Entity, new()
        {
            return new QuerySection(this).ToArray<EntityType>();
        }

        public EntityArrayList<EntityType> ToArrayList<EntityType>(int topItemCount) where EntityType : Entity, new()
        {
            return new QuerySection(this).ToArrayList<EntityType>(topItemCount);
        }

        public EntityArrayList<EntityType> ToArrayList<EntityType>(int topItemCount, int skipItemCount) where EntityType : Entity, new()
        {
            return new QuerySection(this).ToArrayList<EntityType>(topItemCount, skipItemCount);
        }

        public EntityArrayList<EntityType> ToArrayList<EntityType>() where EntityType : Entity, new()
        {
            return new QuerySection(this).ToArrayList<EntityType>();
        }

        public IDataReader ToDataReader(int topItemCount)
        {
            return new QuerySection(this).ToDataReader(topItemCount);
        }

        public IDataReader ToDataReader(int topItemCount, int skipItemCount)
        {
            return new QuerySection(this).ToDataReader(topItemCount, skipItemCount);
        }

        public IDataReader ToDataReader()
        {
            return new QuerySection(this).ToDataReader();
        }

        public DataSet ToDataSet(int topItemCount)
        {
            return new QuerySection(this).ToDataSet(topItemCount);
        }

        public DataSet ToDataSet(int topItemCount, int skipItemCount)
        {
            return new QuerySection(this).ToDataSet(topItemCount, skipItemCount);
        }

        public DataSet ToDataSet()
        {
            return new QuerySection(this).ToDataSet();
        }

        public EntityType ToFirst<EntityType>() where EntityType : Entity, new()
        {
            return new QuerySection(this).ToFirst<EntityType>();
        }

        public DynEntity ToFirst(string typeName)
        {
            return new QuerySection(this).ToFirst(typeName);
        }

        public object ToScalar()
        {
            return new QuerySection(this).ToScalar();
        }

        public object[] ToSinglePropertyArray()
        {
            return new QuerySection(this).ToSinglePropertyArray();
        }

        public object[] ToSinglePropertyArray(int topItemCount)
        {
            return new QuerySection(this).ToSinglePropertyArray(topItemCount);
        }

        public object[] ToSinglePropertyArray(int topItemCount, int skipItemCount)
        {
            return new QuerySection(this).ToSinglePropertyArray(topItemCount, skipItemCount);
        }

        public System.Data.Common.DbCommand ToDbCommand()
        {
            return new QuerySection(this).ToDbCommand();
        }

        public System.Data.Common.DbCommand ToDbCommand(int topItemCount)
        {
            return new QuerySection(this).ToDbCommand(topItemCount);
        }

        public System.Data.Common.DbCommand ToDbCommand(int topItemCount, int skipItemCount)
        {
            return new QuerySection(this).ToDbCommand(topItemCount, skipItemCount);
        }

        #endregion
    }

    public sealed class QuerySection : Rock.Orm.Data.IQuery
    {
        #region Private Members

        private WhereClip whereClip;
        private EntityConfiguration entityConfig;
        private Gateway gateway;
        private bool selectColumnsChanged = false;
        private List<string> selectColumns = new List<string>();
        private DbType firstColumnDbType = DbType.Int32;
        private string identyColumnName = null;
        private bool identyColumnIsNumber = false;

        private void PrepareWhere()
        {
            bool isWhereWithoutAliasName = whereClip.From.AliasName == whereClip.From.TableOrViewName;

            if (isWhereWithoutAliasName)
            {
                gateway.AdjustWhereForAutoCascadeJoin(whereClip, entityConfig, selectColumns);
            }
            else
            {
                if (whereClip.Sql.Contains("/*"))
                {
                    throw new NotSupportedException("A Gateway.From query with cascade where expression could not begin from an entity with alias name!");
                }
            }
        }

        private DbCommand GetDbCommand()
        {
            return gateway.queryFactory.CreateSelectCommand(whereClip, selectColumns.ToArray());
        }

        private DbCommand GetDbCommand(int topItemCount, int skipItemCount)
        {
            return gateway.queryFactory.CreateSelectRangeCommand(whereClip, selectColumns.ToArray(), topItemCount, skipItemCount, identyColumnName, identyColumnIsNumber);
        }

        private IDataReader FindDataReader()
        {
            DbCommand cmd = GetDbCommand();
            return gateway.Db.ExecuteReader(cmd);
        }

        private IDataReader FindDataReader(int topItemCount, int skipItemCount)
        {
            DbCommand cmd = GetDbCommand(topItemCount, skipItemCount);
            return gateway.Db.ExecuteReader(cmd);
        }

        private DataSet FindDataSet()
        {
            DbCommand cmd = GetDbCommand();
            return gateway.Db.ExecuteDataSet(cmd);
        }

        private DataSet FindDataSet(int topItemCount, int skipItemCount)
        {
            DbCommand cmd = GetDbCommand(topItemCount, skipItemCount);
            return gateway.Db.ExecuteDataSet(cmd);
        }

        private object FindScalar()
        {
            DbCommand cmd = GetDbCommand();
            return gateway.Db.ExecuteScalar(cmd);
        }

        private static object[] DataSetToSinglePropertyArray(DataSet ds)
        {
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                object[] list = new object[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    list[i] = dt.Rows[i][0];
                }
                return list;
            }
            return null;
        }

        #endregion

        #region Constructors

        public QuerySection(FromSection from)
        {
            Check.Require(from != null, "could not be null!");
            
            this.whereClip = new WhereClip(from.fromClip);
            this.entityConfig = from.ec;
            this.gateway = from.gateway;
            string[] selectColumnNames = entityConfig.GetAllSelectColumns();
            //string autoIdColumn = MetaDataManager.GetEntityAutoId(entityConfig.Name); 这里标识列的判断以主键为准,如果是自增列也一定是主键,所以自增列的判断就没有必要了
            string aliasNamePrefix = (string.IsNullOrEmpty(from.aliasName) || from.aliasName == entityConfig.ViewName ? string.Empty : from.aliasName + '_');

            List<PropertyConfiguration> pkConfigs = entityConfig.GetPrimaryKeyProperties();
            PropertyConfiguration pkConfig = null;
            if (pkConfigs.Count > 0)
            {
                pkConfig = pkConfigs[0];
            }
            Check.Require(pkConfig != null, "查询的对象不存在主键,请检查!");
            //else
            //{
            //    List<string> list = new List<string>();
            //    Entity.GuessPrimaryKey(entityConfig, list);
            //    pkConfig = entityConfig.GetPropertyConfiguration(list[0]);
            //}
            identyColumnName = aliasNamePrefix + entityConfig.MappingName + '.' + pkConfig.MappingName;
            if (pkConfig.DbType == DbType.Int16 || pkConfig.DbType == DbType.Int32 || pkConfig.DbType == DbType.Int64 ||
                pkConfig.DbType == DbType.Single || pkConfig.DbType == DbType.Double)
            {
                identyColumnIsNumber = true;
            }
            
            for (int i =0; i < selectColumnNames.Length; ++i)
            {
                selectColumns.Add(selectColumnNames[i]);

                //selectColumns[i] = aliasNamePrefix + selectColumns[i];

                //if (autoIdColumn != null && selectColumns[i].EndsWith('.' + autoIdColumn))
                //{
                //    identyColumnName = selectColumns[i];
                //    identyColumnIsNumber = true;
                //}
                //else
                //{
                //    if(identyColumnName == null)
                //    {
                       
                //    }                  
                //}
            }

            if (entityConfig.BaseEntity != null)
            {
                gateway.AppendBaseEntitiesJoins(entityConfig, aliasNamePrefix, whereClip.From);
            }
        }

        #endregion

        #region Public Methods

        public QuerySection Where(WhereClip where)
        {
            Check.Require(((object)where) != null, "where could not be null.");

            this.whereClip.And(where);
            return this;
        }

        public QuerySection OrderBy(OrderByClip orderBy)
        {
            Check.Require(orderBy != null, "orderBy could not be null.");

            this.whereClip.SetOrderBy(orderBy.OrderBys.ToArray());
            return this;
        }

        public QuerySection GroupBy(GroupByClip groupBy)
        {
            Check.Require(groupBy != null, "groupBy could not be null.");

            this.whereClip.SetGroupBy(groupBy.GroupBys.ToArray());
            return this;
        }

        public QuerySection GroupBy(params PropertyItem[] properties)
        {
            Check.Require(properties != null && properties.Length > 0, "properties cound not be null or empty!");

            GroupByClip groupBy = properties[0].GroupBy;
            for (int i = 1; i < properties.Length; ++i)
            {
                groupBy = groupBy & properties[i].GroupBy;
            }

            return GroupBy(groupBy);
        }

        public QuerySection Select(params Rock.Orm.Common.ExpressionClip[] properties)
        {
            Check.Require(properties != null && properties.Length > 0, "properties could not be null or empty!");

            selectColumns.Clear();
            for (int i =0; i < properties.Length; ++i)
            {
                //selectColumns.Add(properties[i].ColumnName);
                selectColumns.Add(properties[i].ToString());
                if (i == 0)
                {
                    firstColumnDbType = properties[i].DbType;
                }
            }
            return this;
        }

        public object ToScalar()
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && gateway.IfFindFromPreload(entityConfig, whereClip))
            {
                try
                {
                    //return gateway.FindScalarFromPreLoadEx(ExpressionFactory.CreateColumnExpression(selectColumns[0], firstColumnDbType), entityConfig, whereClip);
                    System.Reflection.MethodInfo mi = Gateway.GetGatewayMethodInfo("System.Object FindScalarFromPreLoadEx[EntityType](Rock.Orm.Common.ExpressionClip, Rock.Orm.Common.EntityConfiguration, Rock.Orm.Common.WhereClip)");
                    Type entityType = Util.GetType(entityConfig.Name);
                    return mi.MakeGenericMethod(entityType).Invoke(gateway, new object[] { ExpressionFactory.CreateColumnExpression(selectColumns[0], firstColumnDbType), entityConfig, whereClip });
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(entityConfig.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(entityConfig.Name + "|ToScalar_" + string.Join("_", selectColumns.ToArray()), whereClip);
                //if (Gateway.cache.Contains(cacheKey))
                //{
                //    return gateway.GetCache(cacheKey);
                //}
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return cacheObj;
                }
            }

            object obj = FindScalar();

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, obj, expireSeconds);
            }

            return obj;
        }

        public EntityType ToFirst<EntityType>() where EntityType : Entity, new()
        {
            PrepareWhere();
            if (gateway.IsCacheTurnedOn && typeof(EntityType).ToString() == entityConfig.Name && gateway.IfFindFromPreload(entityConfig, whereClip) && (!selectColumnsChanged))
            {
                try
                {
                    return gateway.FindFromPreLoadEx<EntityType>(entityConfig, whereClip);
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            EntityConfiguration ec = typeof(EntityType).ToString() == entityConfig.Name ? entityConfig : new EntityType().GetEntityConfiguration();

            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(ec.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(ec.Name + "|ToFirst_" + string.Join("_", selectColumns.ToArray()), whereClip);
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (EntityType)cacheObj;
                }
            }

            IDataReader reader = FindDataReader();
            EntityType obj = null;
            if (reader.Read())
            {
                obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(reader);
            }
            reader.Close();
            reader.Dispose();

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, obj, expireSeconds);
            }

            return obj;
        }

        public DynEntity ToFirst(string name)
        {
            PrepareWhere();
            if (gateway.IsCacheTurnedOn && name == entityConfig.Name && gateway.IfFindFromPreload(entityConfig, whereClip) && (!selectColumnsChanged))
            {
                try
                {
                    return gateway.FindFromPreLoadEx(name, entityConfig, whereClip);
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            EntityConfiguration ec = name == entityConfig.Name ? entityConfig : MetaDataManager.GetEntityConfiguration(name);

            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(ec.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(ec.Name + "|ToFirst_" + string.Join("_", selectColumns.ToArray()), whereClip);
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (DynEntity)cacheObj;
                }
            }

            IDataReader reader = FindDataReader();
            DynEntity obj = null;

            if (reader.Read())
            {
                string typeName = name.Substring(name.LastIndexOf(".") + 1);
                obj = gateway.CreateEntity(typeName);
                obj.SetPropertyValues(reader);
            }
            reader.Close();
            reader.Dispose();

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, obj, expireSeconds);
            }

            return obj;
        }

        public EntityType[] ToArray<EntityType>(int topItemCount) where EntityType : Rock.Orm.Common.Entity, new()
        {
            return ToArray<EntityType>(topItemCount, 0);
        }

        public DynEntity[] ToArray(string entityType, int topItemCount)
        {
            return ToArray(entityType, topItemCount, 0);
        }

        public EntityType[] ToArray<EntityType>(int topItemCount, int skipItemCount) where EntityType : Rock.Orm.Common.Entity, new()
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && typeof(EntityType).ToString() == entityConfig.Name && gateway.IfFindFromPreload(entityConfig, whereClip) && (!selectColumnsChanged))
            {
                try
                {
                    return gateway.FindArrayFromPreLoadEx<EntityType>(entityConfig, whereClip, topItemCount, skipItemCount);
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            EntityConfiguration ec = typeof(EntityType).ToString() == entityConfig.Name ? entityConfig : new EntityType().GetEntityConfiguration();
            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(ec.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(ec.Name + "|ToArrayList_" + topItemCount.ToString() + '_' + skipItemCount.ToString() + '_' + string.Join("_", selectColumns.ToArray()), whereClip);
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (EntityType[])cacheObj;
                }
            }

            IDataReader reader = FindDataReader(topItemCount, skipItemCount);
            List<EntityType> list = new List<EntityType>();
            while (reader.Read())
            {
                EntityType obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(reader);
                list.Add(obj);
            }
            reader.Close();
            reader.Dispose();

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, list.ToArray(), expireSeconds);
            }

            return list.ToArray();
        }

        public DynEntity[] ToArray(string entityType, int topItemCount, int skipItemCount)
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && entityType == entityConfig.Name && gateway.IfFindFromPreload(entityConfig, whereClip) && (!selectColumnsChanged))
            {
                try
                {
                    return gateway.FindArrayFromPreLoadEx(entityType, entityConfig, whereClip, topItemCount, skipItemCount);
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            EntityConfiguration ec = entityType == entityConfig.Name ? entityConfig : MetaDataManager.GetEntityConfiguration(entityType);
            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(ec.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(ec.Name + "|ToArrayList_" + topItemCount.ToString() + '_' + skipItemCount.ToString() + '_' + string.Join("_", selectColumns.ToArray()), whereClip);
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (DynEntity[])cacheObj;
                }
            }

            IDataReader reader = FindDataReader(topItemCount, skipItemCount);
            List<DynEntity> list = new List<DynEntity>();
            while (reader.Read())
            {
                DynEntity obj = gateway.CreateEntity(entityType);
                obj.SetPropertyValues(reader);
                list.Add(obj);
            }
            reader.Close();
            reader.Dispose();

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, list.ToArray(), expireSeconds);
            }

            return list.ToArray();
        }

        public EntityType[] ToArray<EntityType>() where EntityType : Rock.Orm.Common.Entity, new()
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && typeof(EntityType).ToString() == entityConfig.Name && gateway.IfFindFromPreload(entityConfig, whereClip) && (!selectColumnsChanged))
            {
                try
                {
                    return gateway.FindArrayFromPreLoadEx<EntityType>(entityConfig, whereClip);
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            EntityConfiguration ec = typeof(EntityType).ToString() == entityConfig.Name ? entityConfig : new EntityType().GetEntityConfiguration();
            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(ec.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(ec.Name + "|ToArrayList_" + string.Join("_", selectColumns.ToArray()), whereClip);

                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (EntityType[])cacheObj;
                }
            }

            IDataReader reader = FindDataReader();
            List<EntityType> list = new List<EntityType>();
            while (reader.Read())
            {
                EntityType obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(reader);
                list.Add(obj);
            }
            reader.Close();
            reader.Dispose();

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, list.ToArray(), expireSeconds);
            }

            return list.ToArray();
        }

        public DynEntity[] ToArray(string typeName)
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && typeName == entityConfig.Name && gateway.IfFindFromPreload(entityConfig, whereClip) && (!selectColumnsChanged))
            {
                try
                {
                    return gateway.FindArrayFromPreLoadEx(typeName, entityConfig, whereClip);
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);

            EntityConfiguration ec = entityType.FullName == entityConfig.Name ? entityConfig : MetaDataManager.GetEntityConfiguration(entityType.FullName);
            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(ec.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(ec.Name + "|ToArrayList_" + string.Join("_", selectColumns.ToArray()), whereClip);

                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (DynEntity[])cacheObj;
                }
            }

            IDataReader reader = FindDataReader();
            List<DynEntity> list = new List<DynEntity>();
            while (reader.Read())
            {

                DynEntity obj = gateway.CreateEntity(typeName.Substring(typeName.LastIndexOf('.') + 1));
                obj.SetPropertyValues(reader);
                list.Add(obj);
            }
            reader.Close();
            reader.Dispose();

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, list.ToArray(), expireSeconds);
            }

            return list.ToArray();
        }

        public EntityArrayList<EntityType> ToArrayList<EntityType>() where EntityType : Entity, new()
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && typeof(EntityType).ToString() == entityConfig.Name && gateway.IfFindFromPreload(entityConfig, whereClip) && (!selectColumnsChanged))
            {
                try
                {
                    EntityArrayList<EntityType> al = new EntityArrayList<EntityType>();
                    al.AddRange(gateway.FindArrayFromPreLoadEx<EntityType>(entityConfig, whereClip));
                    return al;                
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            EntityConfiguration ec = typeof(EntityType).ToString() == entityConfig.Name ? entityConfig : new EntityType().GetEntityConfiguration();
            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(ec.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(ec.Name + "|ToArrayList_" + string.Join("_", selectColumns.ToArray()), whereClip);
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (EntityArrayList<EntityType>)cacheObj;
                }
            }

            IDataReader reader = FindDataReader();
            EntityArrayList<EntityType> list = new EntityArrayList<EntityType>();
            while (reader.Read())
            {
                EntityType obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(reader);
                list.Add(obj);
            }
            reader.Close();
            reader.Dispose();

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, list, expireSeconds);
            }

            return list;
        }

        public EntityArrayList<EntityType> ToArrayList<EntityType>(int topItemCount) where EntityType : Entity, new()
        {
            return ToArrayList<EntityType>(topItemCount, 0);
        }

        public EntityArrayList<EntityType> ToArrayList<EntityType>(int topItemCount, int skipItemCount) where EntityType : Entity, new()
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && typeof(EntityType).ToString() == entityConfig.Name && gateway.IfFindFromPreload(entityConfig, whereClip) && (!selectColumnsChanged))
            {
                try
                {
                    EntityArrayList<EntityType> al = new EntityArrayList<EntityType>();
                    al.AddRange(gateway.FindArrayFromPreLoadEx<EntityType>(entityConfig, whereClip, topItemCount, skipItemCount));
                    return al;
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            EntityConfiguration ec = typeof(EntityType).ToString() == entityConfig.Name ? entityConfig : new EntityType().GetEntityConfiguration();
            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(ec.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(ec.Name + "|ToArrayList_" + topItemCount.ToString() + '_' + skipItemCount.ToString() + '_' + string.Join("_", selectColumns.ToArray()), whereClip);
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (EntityArrayList<EntityType>)cacheObj;
                }
            }

            IDataReader reader = FindDataReader(topItemCount, skipItemCount);
            EntityArrayList<EntityType> list = new EntityArrayList<EntityType>();
            while (reader.Read())
            {
                EntityType obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(reader);
                list.Add(obj);
            }
            reader.Close();
            reader.Dispose();

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, list, expireSeconds);
            }

            return list;
        }

        public IDataReader ToDataReader()
        {
            PrepareWhere();
            return FindDataReader();
        }

        public IDataReader ToDataReader(int topItemCount)
        {
            PrepareWhere();
            return ToDataReader(topItemCount, 0);
        }

        public IDataReader ToDataReader(int topItemCount, int skipItemCount)
        {
            PrepareWhere();
            return FindDataReader(topItemCount, skipItemCount);
        }

        public DataSet ToDataSet()
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && gateway.IfFindFromPreload(entityConfig, whereClip) && (!selectColumnsChanged))
            {
                try
                {
                    DataSet dataSet = new DataSet(entityConfig.ViewName);

                    System.Reflection.MethodInfo mi = Gateway.GetGatewayMethodInfo("System.Data.DataTable FindTableFromPreLoadEx[EntityType](Rock.Orm.Common.EntityConfiguration, Rock.Orm.Common.WhereClip)");
                    Type entityType = Util.GetType(entityConfig.Name);
                    DataTable dt = (DataTable)mi.MakeGenericMethod(entityType).Invoke(gateway, new object[] { entityConfig, whereClip });

                    dataSet.Tables.Add(dt);
                    return dataSet;
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(entityConfig.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(entityConfig.Name + "|ToDataSet_" + string.Join("_", selectColumns.ToArray()), whereClip);
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (DataSet)cacheObj;
                }
            }

            DataSet ds = FindDataSet();

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, ds, expireSeconds);
            }

            return ds;
        }

        public DataSet ToDataSet(int topItemCount)
        {
            return ToDataSet(topItemCount, 0);
        }

        public DataSet ToDataSet(int topItemCount, int skipItemCount)
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && gateway.IfFindFromPreload(entityConfig, whereClip) && (!selectColumnsChanged))
            {
                try
                {
                    DataSet dataSet = new DataSet(entityConfig.ViewName);

                    System.Reflection.MethodInfo mi = Gateway.GetGatewayMethodInfo("System.Data.DataTable FindTableFromPreLoadEx[EntityType](Rock.Orm.Common.EntityConfiguration, Rock.Orm.Common.WhereClip, Int32, Int32)");
                    Type entityType = Util.GetType(entityConfig.Name);
                    DataTable dt = (DataTable)mi.MakeGenericMethod(entityType).Invoke(gateway, new object[] { entityConfig, whereClip, topItemCount, skipItemCount });

                    dataSet.Tables.Add(dt);
                    return dataSet;
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(entityConfig.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(entityConfig.Name + "|ToDataSet_" + topItemCount.ToString() + '_' + skipItemCount.ToString() + '_' + string.Join("_", selectColumns.ToArray()), whereClip);
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (DataSet)cacheObj;
                }
            }

            DataSet ds = FindDataSet(topItemCount, skipItemCount);

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, ds, expireSeconds);
            }

            return ds;
        }

        public object[] ToSinglePropertyArray()
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && gateway.IfFindFromPreload(entityConfig, whereClip))
            {
                try
                {
                    System.Reflection.MethodInfo mi = Gateway.GetGatewayMethodInfo("System.Object[] FindSinglePropertyArrayFromPreLoadEx[EntityType](System.String, Rock.Orm.Common.EntityConfiguration, Rock.Orm.Common.WhereClip)");
                    Type entityType = Util.GetType(entityConfig.Name);
                    return (object[])mi.MakeGenericMethod(entityType).Invoke(gateway, new object[] { selectColumns[0], entityConfig, whereClip });
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(entityConfig.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(entityConfig.Name + "|ToSinglePropertyArray_" + string.Join("_", selectColumns.ToArray()), whereClip);
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (object[])cacheObj;
                }
            }

            object[] objs = DataSetToSinglePropertyArray(FindDataSet());

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, objs, expireSeconds);
            }

            return objs;
        }

        public object[] ToSinglePropertyArray(int topItemCount)
        {
            return ToSinglePropertyArray(topItemCount, 0);
        }

        public object[] ToSinglePropertyArray(int topItemCount, int skipItemCount)
        {
            PrepareWhere();

            if (gateway.IsCacheTurnedOn && gateway.IfFindFromPreload(entityConfig, whereClip))
            {
                try
                {
                    System.Reflection.MethodInfo mi = Gateway.GetGatewayMethodInfo("System.Object[] FindSinglePropertyArrayFromPreLoadEx[EntityType](System.String, Rock.Orm.Common.EntityConfiguration, Rock.Orm.Common.WhereClip, Int32, Int32)");
                    Type entityType = Util.GetType(entityConfig.Name);
                    return (object[])mi.MakeGenericMethod(entityType).Invoke(gateway, new object[] { selectColumns[0], entityConfig, whereClip, topItemCount, skipItemCount });
                }
                catch
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("find from auto-preload failed, visit database directly...");
#endif
                }
            }

            string cacheKey = null;
            int expireSeconds = gateway.GetTableCacheExpireSeconds(entityConfig.ViewName);
            bool cacheEnabled = gateway.IsCacheTurnedOn && expireSeconds > 0;
            if (cacheEnabled)
            {
                cacheKey = gateway.ComputeCacheKey(entityConfig.Name + "|ToSinglePropertyArray_" + string.Join("_", selectColumns.ToArray()), whereClip);
                object cacheObj = gateway.GetCache(cacheKey);
                if (cacheObj != null)
                {
                    return (object[])cacheObj;
                }
            }

            object[] objs = DataSetToSinglePropertyArray(FindDataSet(topItemCount, skipItemCount));

            if (cacheEnabled && cacheKey != null)
            {
                gateway.AddCache(cacheKey, objs, expireSeconds);
            }

            return objs;
        }

        public System.Data.Common.DbCommand ToDbCommand()
        {
            return GetDbCommand();
        }

        public System.Data.Common.DbCommand ToDbCommand(int topItemCount)
        {
            return GetDbCommand(topItemCount, 0);
        }

        public System.Data.Common.DbCommand ToDbCommand(int topItemCount, int skipItemCount)
        {
            return GetDbCommand(topItemCount, skipItemCount);
        }

        #endregion
    }

    public sealed class CustomSqlSection
    {
        #region Private Members

        private Gateway gateway;
        private string sql;
        private DbTransaction tran;
        private List<string> inputParamNames = new List<string>();
        private List<DbType> inputParamTypes = new List<DbType>();
        private List<object> inputParamValues = new List<object>();

        private IDataReader FindDataReader()
        {
            DbCommand cmd = gateway.queryFactory.CreateCustomSqlCommand(sql, inputParamNames.ToArray(),
                inputParamTypes.ToArray(), inputParamValues.ToArray());
            return tran == null ? gateway.Db.ExecuteReader(cmd) : gateway.Db.ExecuteReader(cmd, tran);
        }

        private DataSet FindDataSet()
        {
            DbCommand cmd = gateway.queryFactory.CreateCustomSqlCommand(sql, inputParamNames.ToArray(),
                inputParamTypes.ToArray(), inputParamValues.ToArray());
            return tran == null ? gateway.Db.ExecuteDataSet(cmd) : gateway.Db.ExecuteDataSet(cmd, tran);
        }

        #endregion

        #region Constructors

        public CustomSqlSection(Gateway gateway, string sql)
        {
            Check.Require(gateway != null, "gateway could not be null.");
            Check.Require(sql != null, "sql could not be null.");

            this.gateway = gateway;
            this.sql = sql;
        }

        #endregion

        #region Public Members

        public CustomSqlSection AddInputParameter(string name, DbType type, object value)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name could not be null or empty!");

            inputParamNames.Add(name);
            inputParamTypes.Add(type);
            inputParamValues.Add(value);

            return this;
        }
        
        public CustomSqlSection SetTransaction(DbTransaction tran)
        {
            this.tran = tran;

            return this;
        }

        public int ExecuteNonQuery()
        {
            DbCommand cmd = gateway.queryFactory.CreateCustomSqlCommand(sql, inputParamNames.ToArray(),
                inputParamTypes.ToArray(), inputParamValues.ToArray());
            return tran == null ? gateway.Db.ExecuteNonQuery(cmd) : gateway.Db.ExecuteNonQuery(cmd, tran);
        }

        public object ToScalar()
        {
            IDataReader reader = FindDataReader();
            object retObj = null;
            if (reader.Read())
            {
                retObj = reader.GetValue(0);
            }
            reader.Close();
            reader.Dispose();

            return retObj;
        }

        public EntityType ToFirst<EntityType>() where EntityType : Entity, new()
        {
            IDataReader reader = FindDataReader();
            EntityType obj = null;
            if (reader.Read())
            {
                obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(reader);
            }
            reader.Close();
            reader.Dispose();

            return obj;
        }

        public EntityArrayList<EntityType> ToArrayList<EntityType>() where EntityType : Entity, new()
        {
            IDataReader reader = FindDataReader();
            EntityArrayList<EntityType> list = new EntityArrayList<EntityType>();
            while (reader.Read())
            {
                EntityType obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(reader);
                list.Add(obj);
            }
            reader.Close();
            reader.Dispose();

            return list;
        }

        public EntityType[] ToArray<EntityType>() where EntityType : Entity, new()
        {
            return ToArrayList<EntityType>().ToArray();
        }

        public IDataReader ToDataReader()
        {
            return FindDataReader();
        }

        public DataSet ToDataSet()
        {
            return FindDataSet();
        }

        #endregion
    }

    public sealed class StoredProcedureSection
    {
        #region Private Members

        private Gateway gateway;
        private string spName;
        private DbTransaction tran;

        private List<string> inputParamNames = new List<string>();
        private List<DbType> inputParamTypes = new List<DbType>();
        private List<object> inputParamValues = new List<object>();

        private List<string> outputParamNames = new List<string>();
        private List<DbType> outputParamTypes = new List<DbType>();
        private List<int> outputParamSizes = new List<int>();

        private List<string> inputOutputParamNames = new List<string>();
        private List<DbType> inputOutputParamTypes = new List<DbType>();
        private List<object> inputOutputParamValues = new List<object>();
        private List<int> inputOutputParamSizes = new List<int>();

        private string returnValueParamName;
        private DbType returnValueParamType;
        private int returnValueParamSize;

        private IDataReader FindDataReader()
        {
            DbCommand cmd = gateway.queryFactory.CreateStoredProcedureCommand(spName, 
                inputParamNames.ToArray(), inputParamTypes.ToArray(), inputParamValues.ToArray(),
                outputParamNames.ToArray(), outputParamTypes.ToArray(), outputParamSizes.ToArray(),
                inputOutputParamNames.ToArray(), inputOutputParamTypes.ToArray(), inputOutputParamSizes.ToArray(), inputOutputParamValues.ToArray(),
                returnValueParamName, returnValueParamType, returnValueParamSize);
            return tran == null ? gateway.Db.ExecuteReader(cmd) : gateway.Db.ExecuteReader(cmd, tran);
        }

        private DataSet FindDataSet()
        {
            DbCommand cmd = gateway.queryFactory.CreateStoredProcedureCommand(spName, 
                inputParamNames.ToArray(), inputParamTypes.ToArray(), inputParamValues.ToArray(),
                outputParamNames.ToArray(), outputParamTypes.ToArray(), outputParamSizes.ToArray(),
                inputOutputParamNames.ToArray(), inputOutputParamTypes.ToArray(), inputOutputParamSizes.ToArray(), inputOutputParamValues.ToArray(),
                returnValueParamName, returnValueParamType, returnValueParamSize);
            return tran == null ? gateway.Db.ExecuteDataSet(cmd) : gateway.Db.ExecuteDataSet(cmd, tran);
        }

        private DataSet FindDataSet(out Dictionary<string, object> outValues)
        {
            DbCommand cmd = gateway.queryFactory.CreateStoredProcedureCommand(spName, 
                inputParamNames.ToArray(), inputParamTypes.ToArray(), inputParamValues.ToArray(),
                outputParamNames.ToArray(), outputParamTypes.ToArray(), outputParamSizes.ToArray(),
                inputOutputParamNames.ToArray(), inputOutputParamTypes.ToArray(), inputOutputParamSizes.ToArray(), inputOutputParamValues.ToArray(),
                returnValueParamName, returnValueParamType, returnValueParamSize);
            DataSet ds = (tran == null ? gateway.Db.ExecuteDataSet(cmd) : gateway.Db.ExecuteDataSet(cmd, tran));
            outValues = GetOutputParameterValues(cmd);
            return ds;
        }

        private static Dictionary<string, object> GetOutputParameterValues(DbCommand cmd)
        {
            Dictionary<string, object> outValues;
            outValues = new Dictionary<string, object>();
            for (int i = 0; i < cmd.Parameters.Count; ++i)
            {
                if (cmd.Parameters[i].Direction == ParameterDirection.InputOutput || cmd.Parameters[i].Direction == ParameterDirection.Output || cmd.Parameters[i].Direction == ParameterDirection.ReturnValue)
                {
                    outValues.Add(cmd.Parameters[i].ParameterName.Substring(1, cmd.Parameters[i].ParameterName.Length - 1),
                        cmd.Parameters[i].Value);
                }
            }
            return outValues;
        }

        #endregion

        #region Constructors

        public StoredProcedureSection(Gateway gateway, string spName) : base()
        {
            Check.Require(gateway != null, "gateway could not be null.");
            Check.Require(spName != null, "spName could not be null.");

            this.gateway = gateway;
            this.spName = spName;
        }

        #endregion

        #region Public Members

        public StoredProcedureSection AddInputParameter(string name, DbType type, object value)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name could not be null or empty!");

            inputParamNames.Add(name);
            inputParamTypes.Add(type);
            inputParamValues.Add(value);

            return this;
        }

        public StoredProcedureSection AddOutputParameter(string name, DbType type, int size)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name could not be null or empty!");

            outputParamNames.Add(name);
            outputParamTypes.Add(type);
            outputParamSizes.Add(size);

            return this;
        }

        public StoredProcedureSection AddInputOutputParameter(string name, DbType type, int size, object value)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name could not be null or empty!");

            inputOutputParamNames.Add(name);
            inputOutputParamTypes.Add(type);
            inputOutputParamSizes.Add(size);
            inputOutputParamValues.Add(value);

            return this;
        }

        public StoredProcedureSection SetReturnParameter(string name, DbType type, int size)
        {
            Check.Require(!string.IsNullOrEmpty(name), "name could not be null or empty!");

            returnValueParamName = name;
            returnValueParamType = type;
            returnValueParamSize = size;

            return this;
        }
        
        public StoredProcedureSection SetTransaction(DbTransaction tran)
        {
            this.tran = tran;

            return this;
        }

        public int ExecuteNonQuery()
        {
            DbCommand cmd = gateway.queryFactory.CreateStoredProcedureCommand(spName, 
                inputParamNames.ToArray(), inputParamTypes.ToArray(), inputParamValues.ToArray(),
                outputParamNames.ToArray(), outputParamTypes.ToArray(), outputParamSizes.ToArray(),
                inputOutputParamNames.ToArray(), inputOutputParamTypes.ToArray(), inputOutputParamSizes.ToArray(), inputOutputParamValues.ToArray(),
                returnValueParamName, returnValueParamType, returnValueParamSize);
            return tran == null ? gateway.Db.ExecuteNonQuery(cmd) : gateway.Db.ExecuteNonQuery(cmd, tran);
        }

        public int ExecuteNonQuery(out Dictionary<string, object> outValues)
        {
            DbCommand cmd = gateway.queryFactory.CreateStoredProcedureCommand(spName, 
                inputParamNames.ToArray(), inputParamTypes.ToArray(), inputParamValues.ToArray(),
                outputParamNames.ToArray(), outputParamTypes.ToArray(), outputParamSizes.ToArray(),
                inputOutputParamNames.ToArray(), inputOutputParamTypes.ToArray(), inputOutputParamSizes.ToArray(), inputOutputParamValues.ToArray(),
                returnValueParamName, returnValueParamType, returnValueParamSize);
            int affactRows = (tran == null ? gateway.Db.ExecuteNonQuery(cmd) : gateway.Db.ExecuteNonQuery(cmd, tran));
            outValues = GetOutputParameterValues(cmd);
            return affactRows;
        }

        public object ToScalar()
        {
            IDataReader reader = FindDataReader();
            object retObj = null;
            if (reader.Read())
            {
                retObj = reader.GetValue(0);
            }
            reader.Close();
            reader.Dispose();

            return retObj;
        }

        public EntityType ToFirst<EntityType>() where EntityType : Entity, new()
        {
            IDataReader reader = FindDataReader();
            EntityType obj = null;
            if (reader.Read())
            {
                obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(reader);
            }
            reader.Close();
            reader.Dispose();

            return obj;
        }

        public EntityArrayList<EntityType> ToArrayList<EntityType>() where EntityType : Entity, new()
        {
            IDataReader reader = FindDataReader();
            EntityArrayList<EntityType> list = new EntityArrayList<EntityType>();
            while (reader.Read())
            {
                EntityType obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(reader);
                list.Add(obj);
            }
            reader.Close();
            reader.Dispose();

            return list;
        }

        public EntityType[] ToArray<EntityType>() where EntityType : Entity, new()
        {
            return ToArrayList<EntityType>().ToArray();
        }

        public IDataReader ToDataReader()
        {
            return FindDataReader();
        }

        public DataSet ToDataSet()
        {
            return FindDataSet();
        }

        public object ToScalar(out Dictionary<string, object> outValues)
        {
            DataSet ds = FindDataSet(out outValues);
            object retObj = null;
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                retObj = ds.Tables[0].Rows[0][0];
            }
            ds.Dispose();

            return retObj;
        }

        public EntityType ToFirst<EntityType>(out Dictionary<string, object> outValues) where EntityType : Entity, new()
        {
            DataSet ds = FindDataSet(out outValues);
            EntityType obj = null;
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(ds.Tables[0].Rows[0]);
            }
            ds.Dispose();

            return obj;
        }

        public EntityArrayList<EntityType> ToArrayList<EntityType>(out Dictionary<string, object> outValues) where EntityType : Entity, new()
        {
            DataSet ds = FindDataSet(out outValues);
            EntityArrayList<EntityType> list = new EntityArrayList<EntityType>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; ++i)
            {
                EntityType obj = gateway.CreateEntity<EntityType>();
                obj.SetPropertyValues(ds.Tables[0].Rows[i]);
                list.Add(obj);
            }
            ds.Dispose();

            return list;
        }

        public EntityType[] ToArray<EntityType>(out Dictionary<string, object> outValues) where EntityType : Entity, new()
        {
            return ToArrayList<EntityType>(out outValues).ToArray();
        }

        public DataSet ToDataSet(out Dictionary<string, object> outValues)
        {
            return FindDataSet(out outValues);
        }

        #endregion
    }
}