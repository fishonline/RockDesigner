using System;

namespace Rock.Orm.Common.Design
{
    /// <summary>
    /// Mark a property as one of the primary keys of the owned entity.
    /// </summary>
    public class PrimaryKeyDynAttribute : EntityDynAttribute
    {
    }

    /// <summary>
    /// Mark a property as a FriendKey property. You must specify the related entity type, which this friend key depending on.
    /// </summary>
    public class FriendKeyDynAttribute : EntityDynAttribute
    {
        private string relatedEntityType;

        /// <summary>
        /// Gets the type of the related entity.
        /// </summary>
        /// <value>The type of the related entity.</value>
        public string RelatedEntityType
        {
            get
            {
                return relatedEntityType;
            }
        }

        /// <summary>
        /// Gets the related entity pk.
        /// </summary>
        /// <value>The related entity pk.</value>
        public string RelatedEntityPk
        {
            get
            {
                return DynQueryDescriber.GetPkPropertyName(relatedEntityType);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FriendKeyDynAttribute"/> class.
        /// </summary>
        /// <param name="relatedstring">Type of the related entity.</param>
        public FriendKeyDynAttribute(string relatedstring)
        {
            this.relatedEntityType = relatedstring;
        }
    }

    /// <summary>
    /// A draft entity will be ignored in code generating.
    /// </summary>
    public class DraftDynAttribute : EntityDynAttribute
    {
    }

    /// <summary>
    /// Interfaces makred with this attribute will be generated into the implementing interface list of output entities.
    /// The interface specified here must be defined in Entities assembly or a shared assembly that Entities assembly referencing.
    /// </summary>
    public class ImplementInterfaceDynAttribute : EntityDynAttribute
    {
        private string interfaceFullName;

        /// <summary>
        /// Gets the full name of the interface.
        /// </summary>
        /// <value>The full name of the interface.</value>
        public string InterfaceFullName
        {
            get
            {
                return interfaceFullName;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementInterfaceDynAttribute"/> class.
        /// </summary>
        /// <param name="interfaceFullName">Full name of the interface.</param>
        public ImplementInterfaceDynAttribute(string interfaceFullName)
        {
            this.interfaceFullName = interfaceFullName;
        }
    }

    /// <summary>
    /// Mark a property as needed to add index when creating the table in database.
    /// </summary>
    public class IndexPropertyDynAttribute : EntityDynAttribute
    {
        private bool isDesc = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexPropertyDynAttribute"/> class.
        /// </summary>
        public IndexPropertyDynAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexPropertyDynAttribute"/> class.
        /// </summary>
        /// <param name="isDesc">if set to <c>true</c> [is desc].</param>
        public IndexPropertyDynAttribute(bool isDesc)
        {
            this.isDesc = isDesc;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is desc.
        /// </summary>
        /// <value><c>true</c> if this instance is desc; otherwise, <c>false</c>.</value>
        public bool IsDesc
        {
            get { return isDesc; }
        }
    }

    /// <summary>
    /// Whether the mapping column in database could not be NULL.
    /// </summary>
    public class NotNullDynAttribute : EntityDynAttribute
    {
    }

    /// <summary>
    /// Whether a property should not included in default XML serialization. This attribute maps to a XmlIgnore attribute in actual generated entity code by EntityDesignToEntity.exe tool.
    /// </summary>
    public class SerializationIgnoreDynAttribute : EntityDynAttribute
    {
    }

    /// <summary>
    /// Mark an entity as readonly, which means it can be used in finding entities and cannot be updated. Generally, a readonly entity maps to a database view.
    /// </summary>
    public class ReadOnlyDynAttribute : EntityDynAttribute
    {
    }

    /// <summary>
    /// Mark an entity as a relation entity, which is used to realize many to many relation mapping.
    /// </summary>
    public class RelationDynAttribute : EntityDynAttribute
    {
    }

    /// <summary>
    /// Additional sql script clip which will be included into the sql script batch.
    /// </summary>
    public class AdditionalSqlScriptDynAttribute : EntityDynAttribute
    {
        private string sql;
        private string preCleanSql = null;

        public AdditionalSqlScriptDynAttribute(string sql)
        {
            this.sql = sql;
        }

        /// <summary>
        /// Gets the SQL.
        /// </summary>
        /// <value>The SQL.</value>
        public string Sql
        {
            get
            {
                return sql;
            }
        }

        /// <summary>
        /// Gets or sets the pre clean SQL.
        /// </summary>
        /// <value>The pre clean SQL.</value>
        public string PreCleanSql
        {
            get
            {
                return preCleanSql;
            }
            set
            {
                preCleanSql = value;
            }
        }
    }

    /// <summary>
    /// Mark an entity as needed to save all related base entity and property entity values in a batch gateway to improve performance.
    /// </summary>
    public class BatchUpdateDynAttribute : EntityDynAttribute
    {
        private int batchSize;

        /// <summary>
        /// Gets the size of the batch save.
        /// </summary>
        /// <value>The size of the batch save.</value>
        public int BatchSize
        {
            get
            {
                return batchSize;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchUpdateDynAttribute"/> class.
        /// </summary>
        /// <param name="batchSize">Size of the batch save.</param>
        public BatchUpdateDynAttribute(int batchSize)
        {
            this.batchSize = batchSize;
        }
    }

    /// <summary>
    /// Whether instances of the entity are automatically preloaded.
    /// </summary>
    public class AutoPreLoadDynAttribute : EntityDynAttribute
    {
    }

    /// <summary>
    /// Mark a property of a relation entity as a relation key, which is used to relate an entity's primary key.
    /// </summary>
    public class RelationKeyDynAttribute : EntityDynAttribute
    {
        private string relatedType;

        /// <summary>
        /// Gets the type of the related entity.
        /// </summary>
        /// <value>The type of the related.</value>
        public string RelatedType
        {
            get
            {
                return relatedType;
            }
        }

        /// <summary>
        /// Gets the related pk.
        /// </summary>
        /// <value>The pk.</value>
        public string RelatedPk
        {
            get
            {
                return DynQueryDescriber.GetPkPropertyName(relatedType);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationKeyDynAttribute"/> class.
        /// </summary>
        /// <param name="relatedType">Type of the related.</param>
        public RelationKeyDynAttribute(string relatedType)
        {
            this.relatedType = relatedType;
        }
    }

    /// <summary>
    /// Mark a property as a CompoundUnit property.
    /// </summary>
    /// <remarks>
    /// A compound unit can be a serializable struct or class. It can contains properties which is mapping to an single data column. A classic compound unit example is UserName: FirstName, LastName.
    /// </remarks>
    public class CompoundUnitDynAttribute : EntityDynAttribute
    {
    }

    /// <summary>
    /// Specify the actual mapping name  of an entity to a table/view/procedure or a property to a data column. 
    /// By default, entity/properties with same names map to table/view/procedure/data columns with same name.
    /// </summary>
    public class MappingNameDynAttribute : EntityDynAttribute
    {
        private string name;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingNameDynAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MappingNameDynAttribute(string name)
        {
            this.name = name;
        }
    }

    /// <summary>
    /// _comment content set with this attribute will be generated into the generated Entities code.
    /// </summary>
    public class CommentDynAttribute : EntityDynAttribute
    {
        private string content;

        /// <summary>
        /// Gets the Content content.
        /// </summary>
        /// <value>The content.</value>
        public string Content
        {
            get
            {
                return content;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentDynAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CommentDynAttribute(string content)
        {
            this.content = content;
        }
    }

    /// <summary>
    /// Specify the output namespace of this entity
    /// </summary>
    public class OutputNamespaceDynAttribute : EntityDynAttribute
    {
        private string ns;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Namespace
        {
            get
            {
                return ns;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputNamespaceDynAttribute"/> class.
        /// </summary>
        /// <param name="ns">The namespace.</param>
        public OutputNamespaceDynAttribute(string ns)
        {
            this.ns = ns;
        }
    }

    /// <summary>
    /// Custom data of entity or property, by which you can take advantage of for custom use. 
    /// When you set some custom data for an entity or property, you can get these data at runtime 
    /// by calling Rock.Orm.Common.MetaDataManager.GetEntityConfiguration() and EntityConfiguration.GetPropertyConfiguration().
    /// </summary>
    public class CustomDataDynAttribute : EntityDynAttribute
    {
        private string data;

        /// <summary>
        /// Gets the custom data.
        /// </summary>
        /// <value>The name.</value>
        public string Data
        {
            get
            {
                return data;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDataDynAttribute"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public CustomDataDynAttribute(string data)
        {
            this.data = data;
        }
    }

    /// <summary>
    /// Specify the database column's sql type mapping to the property. If there is no SqlType specified for a property, Rock will generate a default according to it's .Net type.
    /// By default, value types maps to relevant value types in database, and string type maps to database type - nvarchar(127).
    /// </summary>
    public class SqlTypeDynAttribute : EntityDynAttribute
    {
        private string type;
        private string defaultValue;

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type
        {
            get
            {
                return type;
            }
        }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>The default value.</value>
        public string DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                defaultValue = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTypeDynAttribute"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public SqlTypeDynAttribute(string type)
        {
            this.type = type;
        }
    }
}
