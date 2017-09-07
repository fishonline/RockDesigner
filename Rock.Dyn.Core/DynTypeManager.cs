using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rock.Dyn.Core
{
    /// <summary>
    /// 实体类型管理器
    /// </summary>
    public sealed class DynTypeManager
    {
        /// <summary>
        /// 调用方法的委托
        /// </summary>
        public static MethodHandleDelegate MethodHandler;

        private static Dictionary<string, DynClass> _dynClasses = new Dictionary<string, DynClass>();

        private static Dictionary<string, DynClass> _dynFunctions = new Dictionary<string, DynClass>();

        private static Dictionary<string, DynClass> _dynClassesQuick = new Dictionary<string, DynClass>();

        private static Dictionary<string, DynInterface> _dynInterfaces = new Dictionary<string, DynInterface>();

        private static Dictionary<string, Type> _staticClassType = new Dictionary<string, Type>();

        /// <summary>
        /// Initializes the <see cref="DynTypeManager"/> class.
        /// </summary>
        static DynTypeManager()
        {
        }

        /// <summary>
        /// 实体类型集合
        /// </summary>
        public static Dictionary<string, DynClass> DynClasses
        {
            get { return _dynClasses; }
        }

        /// <summary>
        /// 实体类型集合
        /// </summary>
        public static Dictionary<string, DynClass> DynFunctions
        {
            get { return _dynFunctions; }
        }

        /// <summary>
        /// 清除工厂内类型
        /// </summary>
        public static void Clear()
        {
            _dynClasses.Clear();

            _dynFunctions.Clear();

            _dynInterfaces.Clear();

            _staticClassType.Clear();

            _dynClassesQuick.Clear();
        }

        /// <summary>
        /// 注册动态类
        /// </summary>
        /// <param name="dynClass"></param>
        public static void RegistClass(DynClass dynClass)
        {
            if (dynClass != null)
            {
                string key = dynClass.Namespace + "^" + dynClass.Name;
                if (!_dynClasses.ContainsKey(key))
                {
                    _dynClasses.Add(key, dynClass);
                }
                else
                {
                    throw new ApplicationException("已经存在同名的Class");
                }
            }
        }

        /// <summary>
        /// 注册函数类
        /// </summary>
        /// <param name="dynClass"></param>
        public static void RegistFunction(DynClass dynClass)
        {
            if (dynClass != null)
            {
                string key = dynClass.Namespace + "^" + dynClass.Name;
                if (!_dynFunctions.ContainsKey(key))
                {
                    _dynFunctions.Add(key, dynClass);
                }
                else
                {
                    throw new ApplicationException("已经存在同名的FunctionClass");
                }
            }
        }

        /// <summary>
        /// 创建并返回动态类型
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static DynClass CreateClass(string className)
        {
            DynClass dynClass = GetClass(className);

            if (dynClass != null)
                return dynClass;

            dynClass = new DynClass(className);

            _dynClasses.Add(dynClass.Namespace + "^" + dynClass.Name, dynClass);

            return dynClass;
        }

        /// <summary>
        /// 创建并返回动态类型
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static DynClass CreateFunction(string className)
        {
            DynClass dynClass = GetFunction(className);

            if (dynClass != null)
                return dynClass;

            dynClass = new DynClass(className);

            _dynFunctions.Add(dynClass.Namespace + "^" + dynClass.Name, dynClass);

            return dynClass;
        }

        /// <summary>
        /// 创建类的实例
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static DynObject CreateObject(string className)
        {
            DynClass dynClass = GetClass(className);

            if (dynClass != null)
                return new DynObject(className);
            else
                throw new ApplicationException(string.Format("{0}类型不存在", className));
        }

        /// <summary>
        /// 根据名称获取动态类,如果没有则返回null
        /// </summary>
        /// <param name="className">Namespace + _ + Name of the type.</param>
        /// <returns>The entity configuration</returns>
        public static DynClass GetClass(string className)
        {
            DynClass value = null;
            if (_dynClassesQuick.TryGetValue(className,out value))
            {
                return value;
            }
            else
            {
                string[] namespaceAndClassName;

                foreach (var item in _dynClasses)
                {
                    namespaceAndClassName = item.Key.Split('^');
                    if (namespaceAndClassName.Length > 0 && namespaceAndClassName[namespaceAndClassName.Length - 1] == className)
                    {
                        _dynClassesQuick[className] = item.Value;
                        return item.Value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 根据名称获取动态类,如果没有则返回null
        /// </summary>
        /// <param name="className">Namespace + _ + Name of the type.</param>
        /// <returns>The entity configuration</returns>
        public static DynClass GetClassByFullName(string classFullName)
        {
            DynClass value = null;
            if (_dynClasses.TryGetValue(classFullName, out value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// 根据名称获取动态类,如果没有则返回null
        /// </summary>
        /// <param name="className">Namespace + _ + Name of the type.</param>
        /// <returns>The entity configuration</returns>
        public static DynClass GetFunction(string className)
        {
            string[] namespaceAndClassName;
            foreach (var item in _dynFunctions)
            {
                namespaceAndClassName = item.Key.Split('^');
                if (namespaceAndClassName.Length > 0 && namespaceAndClassName[namespaceAndClassName.Length - 1] == className)
                {
                    return item.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// 是否包含该名称的动态类
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static bool ContainsClass(string className)
        {
            string[] namespaceAndClassName;
            foreach (var item in _dynClasses)
            {
                namespaceAndClassName = item.Key.Split('^');
                if (namespaceAndClassName[namespaceAndClassName.Length - 1] == className)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否包含该名称的动态类
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static bool ContainsFunction(string className)
        {
            string[] namespaceAndClassName;
            foreach (var item in _dynFunctions)
            {
                namespaceAndClassName = item.Key.Split('^');
                if (namespaceAndClassName[namespaceAndClassName.Length - 1] == className)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 方法调用
        /// </summary>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="dicParams"></param>
        /// <returns></returns>
        public static object Call(string className, string methodName, Dictionary<string, object> dicParams)
        {
            return DynTypeManager.MethodHandler(null, className + "_" + methodName, dicParams);
        }

        /// <summary>
        /// 动态类型的Xml序列化
        /// </summary>
        /// <param name="dynClass"></param>
        /// <returns></returns>
        public static XElement ToXml(DynClass dynClass)
        {
            XElement eleDynClass = new XElement("Class");
            eleDynClass.SetAttributeValue("Namespace", dynClass.Namespace ?? "");
            eleDynClass.SetAttributeValue("Name", dynClass.Name ?? "");
            eleDynClass.SetAttributeValue("BaseName", dynClass.BaseClass.Name ?? "");
            eleDynClass.SetAttributeValue("InterfaceNames", string.Join(",", dynClass.InterfaceNames.ToArray()));

            XElement eleDynProperties = new XElement("Properties");
            eleDynClass.Add(eleDynProperties);

            foreach (DynProperty dynProperty in dynClass.GetProperties())
            {
                XElement eleDynProperty = new XElement("Property");
                eleDynProperties.Add(eleDynProperty);

                // 设置DynProperty
                eleDynProperty.SetAttributeValue("ID", dynProperty.ID.ToString());
                eleDynProperty.SetAttributeValue("Name", dynProperty.Name);

                //ToDo:这两个属性是对象，等待程序完善后，再仔细考虑怎样处理
                eleDynProperty.SetAttributeValue("CurrentClassName", dynProperty.CurrentDynClass.Name);
                eleDynProperty.SetAttributeValue("ClassName", dynProperty.DynClass.Name);

                eleDynProperty.SetAttributeValue("Type", Enum.GetName(typeof(DynType), dynProperty.DynType));
                //eleDynProperty.SetAttributeValue("InheritEntityName", dynProperty.InheritEntityName);
                //eleDynProperty.SetAttributeValue("IsArray", dynProperty.IsArray.ToString());
                //eleDynProperty.SetAttributeValue("IsInherited", dynProperty.IsInherited.ToString());
                //eleDynProperty.SetAttributeValue("IsQueryProperty", dynProperty.IsQueryProperty.ToString());

                XElement eleDynAttributes = new XElement("Attributes");
                eleDynProperty.Add(eleDynAttributes);

                foreach (DynObject dynAttribute in dynProperty.Attributes)
                {
                    XElement eleDynAttribute = new XElement("Attribute");
                    eleDynAttributes.Add(eleDynAttribute);

                    //ToDo: 等待DynAttribute完善后添加
                    eleDynAttribute.SetAttributeValue("Name", "");
                }
            }

            XElement eleClassDynAttributes = new XElement("Attributes");
            eleDynClass.Add(eleClassDynAttributes);

            foreach (DynObject dynAttribute in dynClass.Attributes)
            {
                XElement eleDynAttribute = new XElement("Attribute");
                eleClassDynAttributes.Add(eleDynAttribute);

                //ToDo: 等待DynAttribute完善后添加
                eleDynAttribute.SetAttributeValue("Name", "");
            }

            return eleDynClass;
        }

        /// <summary>
        /// 动态对象的Xml序列化
        /// </summary>
        /// <returns></returns>
        public static XElement ToXml(DynObject dynObject)
        {
            XElement eleDynClass = new XElement("Object");
            eleDynClass.SetAttributeValue("Class", dynObject.DynClass.Name);

            XElement eleDynProperties = new XElement("Properties");
            eleDynClass.Add(eleDynProperties);

            foreach (DynProperty entityProperty in dynObject.DynClass.GetProperties())
            {
                XElement eleDynProperty = new XElement("Property");
                eleDynProperties.Add(eleDynProperty);

                eleDynProperty.SetAttributeValue(entityProperty.Name, dynObject.GetPropertyValue(entityProperty.Name));
            }

            return eleDynClass;
        }

        /// <summary>
        /// 动态接口集合
        /// </summary>
        public static List<DynInterface> DynInterfaces
        {
            get { return _dynInterfaces.Values.ToList<DynInterface>(); }
        }

        /// <summary>
        /// 添加接口
        /// </summary>
        /// <param name="dynInterface">接口</param>
        public static void RegistInterface(DynInterface dynInterface)
        {
            if (dynInterface != null)
            {
                if (!_dynInterfaces.ContainsKey(dynInterface.Name))
                {
                    _dynInterfaces.Add(dynInterface.Name, dynInterface);
                }
                else
                {
                    throw new ApplicationException(string.Format("已经存在名称为{0}的接口", dynInterface.Name));
                }
            }
            else
            {
                throw new ApplicationException("动态接口不能为null");
            }
        }

        /// <summary>
        /// 是否包含接口
        /// </summary>
        /// <param name="interfaceName">接口名</param>
        /// <returns></returns>
        public static bool ContainsInterface(string interfaceName)
        {
            if (!string.IsNullOrEmpty(interfaceName))
            {
                return _dynInterfaces.ContainsKey(interfaceName);
            }
            else
            {
                throw new ApplicationException("接口名不能为空或null");
            }
        }

        /// <summary>
        /// 生成接口实例
        /// </summary>
        /// <param name="interfaceName">接口名</param>
        /// <returns></returns>
        public static DynInterface CreateInterface(string interfaceName)
        {
            if (!string.IsNullOrEmpty(interfaceName))
            {
                DynInterface dynInterface = null;
                if (!_dynInterfaces.TryGetValue(interfaceName, out dynInterface))
                {
                    dynInterface = new DynInterface(interfaceName);
                    _dynInterfaces.Add(interfaceName, dynInterface);
                }
                return dynInterface;
            }
            else
            {
                throw new ApplicationException("接口名不能为空或null");
            }
        }

        /// <summary>
        /// 获取接口,如果没有则返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DynInterface GetInterface(string interfaceName)
        {
            if (!string.IsNullOrEmpty(interfaceName))
            {
                DynInterface value = null;
                if (_dynInterfaces.TryGetValue(interfaceName, out value))
                {
                    return value;
                }
                else
                {
                    throw new ApplicationException("不存在接口" + interfaceName);
                }
            }
            else
            {
                throw new ApplicationException("接口名不能为空或null");
            }
        }

        public static void RegistStaticType(Type type)
        {
            if (type != null)
            {
                if (!_staticClassType.ContainsKey(type.Name))
                {
                    _staticClassType.Add(type.Name, type);
                }
                else
                {
                    throw new ApplicationException("已经存在同名的Class");
                }
            }
        }

        public static Type GetStaticType(string typeName)
        {
            if (!string.IsNullOrEmpty(typeName))
            {
                Type value = null;
                if (_staticClassType.TryGetValue(typeName, out  value))
                {
                    return value;
                }
            }
            return null;
        }

        public static bool ContainsStaticType(string typeName)
        {
            if (!string.IsNullOrEmpty(typeName))
            {
                return _staticClassType.ContainsKey(typeName);
            }
            return false;
        }

        /// <summary>
        /// 根据特性名获取拥有此特性的类
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public List<DynClass> GetDynClassesByAttribute(string attributeName)
        {
            List<DynClass> attrs = new List<DynClass>();

            foreach (DynClass dclass in _dynClasses.Values)
            {
                DynObject dynObject = null;
                if (dclass.TryGetAttribute(attributeName, out dynObject))
                {
                    attrs.Add(dclass);
                }
            }

            return attrs;
        }

        /// <summary>
        /// 根据特性名获取拥有此特性的属性
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public List<DynProperty> GetDynPropertiesByAttribute(string attributeName)
        {
            List<DynProperty> attrs = new List<DynProperty>();

            foreach (DynClass dclass in _dynClasses.Values)
            {
                foreach (DynProperty dproperty in dclass.GetProperties())
                {
                    DynObject dynObject = null;
                    if (dproperty.TryGetAttribute(attributeName, out dynObject))
                    {
                        attrs.Add(dproperty);
                    }
                }
            }

            return attrs;
        }

        /// <summary>
        /// 根据特性名获取拥有此特性的方法
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public List<DynMethod> GetDynMethodesByAttribute(string attributeName)
        {
            List<DynMethod> attrs = new List<DynMethod>();

            foreach (DynClass dclass in _dynClasses.Values)
            {
                foreach (DynMethod dmethod in dclass.GetMethods())
                {
                    DynObject dynObject = null;
                    if (dmethod.TryGetAttribute(attributeName, out dynObject))
                    {
                        attrs.Add(dmethod);
                    }
                }
            }

            return attrs;
        }


        public static void MakeRelation()
        {
            foreach (DynClass dynClass in DynClasses.Values)
            {
                dynClass.RelationDynClassDict.Clear();
                dynClass.RelationDynPropertyDict.Clear();
            }

            foreach (DynClass dynClass in DynClasses.Values)
            {
                dynClass.MakeRelation();
            }
        }
    }
}
