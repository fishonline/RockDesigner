using System;
using System.Collections.Generic;

namespace Rock.Orm.Common
{
    /// <summary>
    /// 实体类型管理器
    /// </summary>
    public sealed class DynEntityTypeManager
    {
        private static List<DynEntityType> _entitytypes = new List<DynEntityType>();

        /// <summary>
        /// Initializes the <see cref="DynEntityTypeManager"/> class.
        /// </summary>
        static DynEntityTypeManager()
        {
        }

        /// <summary>
        /// 实体类型集合
        /// </summary>
        public static List<DynEntityType> Entities
        {
            get { return _entitytypes; }
        }

        public static void AddEntityTypes(DynEntityType[] objs)
        {
            if (objs != null)
            {
                foreach (DynEntityType obj in objs)
                {
                    if (GetEntityType(obj.Name) == null)
                        _entitytypes.Add(obj);
                }
            }
        }

        public static void AddEntityType(DynEntityType obj)
        {
            if (obj != null)
            {
                if (_entitytypes.Contains(obj) == false)
                    _entitytypes.Add(obj);
            }
        }

        public static void ClearEntityTypes()
        {
            _entitytypes.Clear();
        }

        /// <summary>
        /// Gets the entity configuration.if entity is not find, return null.
        /// </summary>
        /// <param name="name">Name of the type.</param>
        /// <returns>The entity configuration</returns>
        public static DynEntityType GetEntityType(string name)
        {
            if (_entitytypes != null)
            {
                foreach (DynEntityType item in _entitytypes)
                {
                    if (RemoveTypePrefix(item.Name) == RemoveTypePrefix(name))
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the entity configuration.if entity is not find, throw Exception
        /// </summary>
        /// <param name="name">Name of the type.</param>
        /// <returns>The entity configuration</returns>
        public static DynEntityType GetEntityTypeMandatory(string name)
        {
            if (_entitytypes != null)
            {
                foreach (DynEntityType item in _entitytypes)
                {
                    if (RemoveTypePrefix(item.Name) == RemoveTypePrefix(name))
                    {
                        return item;
                    }
                }
            }
            throw new ApplicationException("给定的类型不在ORM中 " + name);
        }

        private static string RemoveTypePrefix(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return typeName;
            }
            string name = typeName;
            while (name.Contains("."))
            {
                name = name.Substring(name.IndexOf(".")).TrimStart('.');
            }
            return name;
        }
    }

    public sealed class _
    {
        public static PropertyItem P(string typeName, string propertyName)
        {
            DynEntityType entityType = DynEntityTypeManager.GetEntityTypeMandatory(typeName);
            DynPropertyConfiguration property = entityType.GetProperty(propertyName);

            if (property.IsInherited)
            {
                DynEntityType baseEntityType = DynEntityTypeManager.GetEntityTypeMandatory(property.InheritEntityMappingName);
                return new PropertyItem(propertyName, baseEntityType.FullName);
            }
            else
            {
                return new PropertyItem(propertyName, entityType.FullName);
            }
        }
    }
}
