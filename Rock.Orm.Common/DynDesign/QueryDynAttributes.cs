using System;
using System.Collections.Generic;
using System.Text;

namespace Rock.Orm.Common.Design
{
    /// <summary>
    /// Base class of all query attributes.
    /// </summary>
    public abstract class QueryDynAttribute : EntityDynAttribute
    {
        #region Private Memebrs

        private bool lazyLoad = true;
        private QueryType queryType;

        #endregion

        #region Protected Members

        /// <summary>
        /// whether the related property values is contained in owner entity's cascade update.
        /// </summary>
        protected bool contained = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the property is lazyload.
        /// </summary>
        /// <value><c>true</c> if lazyload; otherwise, <c>false</c>.</value>
        public bool LazyLoad
        {
            get { return lazyLoad; }
            set { lazyLoad = true; }
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public QueryType QueryType
        {
            get { return queryType; }
        }

        /// <summary>
        /// Gets a value indicating whether this property is contained in cascade update.
        /// </summary>
        /// <value><c>true</c> if contained; otherwise, <c>false</c>.</value>
        public bool Contained
        {
            get
            {
                return contained;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryDynAttribute"/> class.
        /// </summary>
        /// <param name="queryType">Type of the query.</param>
        public QueryDynAttribute(QueryType queryType)
        {
            this.queryType = queryType;
        }

        #endregion
    }

    /// <summary>
    /// Mark a property as a primary key one to one related property.
    /// </summary>
    public class PkQueryDynAttribute : QueryDynAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PkQueryDynAttribute"/> class.
        /// </summary>
        public PkQueryDynAttribute()
            : base(QueryType.PkQuery)
        {
            base.contained = true;
        }

        #endregion
    }

    /// <summary>
    /// Mark a property as a friend key one to one / one to many related property.
    /// </summary>
    public class FkQueryDynAttribute : QueryDynAttribute
    {
        #region Private Members

        private string where;
        private string relatedManyToOneQueryPropertyName;
        private string orderBy;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the related property which is the friend key of the related type or a property marked by FkReverseQuery.
        /// </summary>
        /// <value>The name of the related many to one query property.</value>
        public string RelatedManyToOneQueryPropertyName
        {
            get
            {
                return relatedManyToOneQueryPropertyName;
            }
        }

        /// <summary>
        /// Gets or sets the order by condition, when is used when it is a onte to many related property.
        /// </summary>
        /// <value>The order by.</value>
        public string OrderBy
        {
            get { return orderBy; }
            set { orderBy = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this property values is contained in owner entity's cascade update.
        /// </summary>
        /// <value><c>true</c> if contained; otherwise, <c>false</c>.</value>
        public new bool Contained
        {
            get { return base.contained; }
            set { base.contained = value; }
        }

        /// <summary>
        /// Gets or sets the additional where.
        /// </summary>
        /// <value>The additional where.</value>
        public string AdditionalWhere
        {
            get { return where; }
            set { where = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FkQueryDynAttribute"/> class.
        /// </summary>
        /// <param name="relatedManyToOneQueryPropertyName">The name of the related property which is the friend key of the related type or a property marked by FkReverseQuery</param>
        public FkQueryDynAttribute(string relatedManyToOneQueryPropertyName)
            : base(QueryType.FkQuery)
        {
            this.relatedManyToOneQueryPropertyName = relatedManyToOneQueryPropertyName;
        }

        #endregion
    }

    /// <summary>
    /// Mark a property as a custom query property. A custom quey can use custom query creterias to query one to one or one to many related entities.
    /// </summary>
    public class CustomQueryDynAttribute : QueryDynAttribute
    {
        #region Private Members

        private string where;
        private string orderBy;
        private string relationType;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the where creteria.
        /// </summary>
        /// <value>The where.</value>
        public string Where
        {
            get { return where; }
        }

        /// <summary>
        /// Gets or sets the order by.
        /// </summary>
        /// <value>The order by.</value>
        public string OrderBy
        {
            get { return orderBy; }
            set { orderBy = value; }
        }

        /// <summary>
        /// Gets or sets the type of the relation entity.
        /// </summary>
        /// <value>The type of the relation.</value>
        public string RelationType
        {
            get { return relationType; }
            set { relationType = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomQueryDynAttribute"/> class.
        /// </summary>
        /// <param name="where">The where.</param>
        public CustomQueryDynAttribute(string where)
            : base(QueryType.CustomQuery)
        {
            this.where = where;
        }

        #endregion
    }

    /// <summary>
    /// Mark a property as a Reverse PkQuery property.
    /// </summary>
    public class PkReverseQueryDynAttribute : QueryDynAttribute
    {   
        /// <summary>
        /// Initializes a new instance of the <see cref="PkReverseQueryDynAttribute"/> class.
        /// </summary>
        public PkReverseQueryDynAttribute(bool lazyLoad)
            : base(QueryType.PkReverseQuery)
        {
            base.LazyLoad = lazyLoad;
        }        
    }

    /// <summary>
    /// Mark a property as a Reverse FkQuery property.
    /// </summary>
    public class FkReverseQueryDynAttribute : QueryDynAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FkReverseQueryDynAttribute"/> class.
        /// </summary>
        public FkReverseQueryDynAttribute(bool lazyLoad)
            : base(QueryType.FkReverseQuery)
        {
            base.contained = true;
            base.LazyLoad = lazyLoad;
        }

        #endregion
    }

    /// <summary>
    /// Mark a property as a many to many related property.
    /// </summary>
    public class ManyToManyQueryDynAttribute : QueryDynAttribute
    {
        #region Private Members

        private string where;
        private string relationType;
        private string orderBy;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this property is contained in owner entity's cascade update.
        /// </summary>
        /// <value><c>true</c> if contained; otherwise, <c>false</c>.</value>
        public new bool Contained
        {
            get { return base.contained; }
            set { base.contained = value; }
        }

        /// <summary>
        /// Gets the type of the relation entity.
        /// </summary>
        /// <value>The type of the relation.</value>
        public string RelationType
        {
            get
            {
                return relationType;
            }
        }

        /// <summary>
        /// Gets or sets the order by.
        /// </summary>
        /// <value>The order by.</value>
        public string OrderBy
        {
            get { return orderBy; }
            set { orderBy = value; }
        }

        /// <summary>
        /// Gets or sets the additional where.
        /// </summary>
        /// <value>The additional where.</value>
        public string AdditionalWhere
        {
            get { return where; }
            set { where = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManyToManyQueryDynAttribute"/> class.
        /// </summary>
        /// <param name="relationType">Type of the relation entity.</param>
        public ManyToManyQueryDynAttribute(string relationName)
            : base(QueryType.ManyToManyQuery)
        {
            DynEntityType relationType = DynEntityTypeManager.GetEntityType(relationName);
            if (relationType == null)
            {
                throw new NotSupportedException("A entity type's relation entity type is not find, if you use it as ManyToMany attribute's relation type parameter.");
            }
            EntityDynAttribute[] attrs = relationType.GetCustomAttributes(typeof(RelationDynAttribute), true);
            if (attrs==null||attrs.Length == 0)
            {
                throw new NotSupportedException("A entity type must be a relation entity type, if you use it as ManyToMany attribute's relation type parameter.");
            }


            this.relationType = relationName;
        }

        #endregion
    }
}
