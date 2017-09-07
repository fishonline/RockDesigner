using System.Collections.Generic;
using System.Linq;
using System;

namespace Rock.Dyn.Core
{
    public class DynClass
    {
        private string _name;
        private List<string> _interfaceNames = new List<string>();
        private string _nameSpace; 
        private string _description;
        private string _displayName;
        private ClassMainType _classMainType;
        private string _baseClassName;
        private DynClass _baseClass;

        private Dictionary<short, DynProperty> _quickFindProperty;
        //private List<DynProperty> _sortedProperties = new List<DynProperty>();
        private Dictionary<string, DynMethod> _methods = new Dictionary<string, DynMethod>();
        private DynClass[] _allBaseClasses;
        private Dictionary<string, DynObject> _attributes = new Dictionary<string, DynObject>();
        private Dictionary<string, DynProperty> _propertiesByName = new Dictionary<string, DynProperty>();
        private Dictionary<short, DynProperty> _propertiesByID = new Dictionary<short, DynProperty>();

        public Dictionary<string, DynClass> RelationDynClassDict = new Dictionary<string, DynClass>();
        public Dictionary<string, DynProperty> RelationDynPropertyDict = new Dictionary<string, DynProperty>();
        /// <summary>
        /// 类型名称, 不包含命名空间
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value.Trim(); }
        }
        /// <summary>
        /// 接口名称列表
        /// </summary>
        public List<string> InterfaceNames
        {
            get { return _interfaceNames; }
        }
        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace
        {
            get
            {
                return _nameSpace;
            }
            set
            {
                _nameSpace = value.Trim();
            }
        }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value.Trim(); }
        }
        /// <summary>
        ///  显示名称
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }
        /// <summary>
        ///  类的类型
        /// </summary>
        public virtual ClassMainType ClassMainType
        {
            get { return _classMainType; }
            set
            {
                if (_propertiesByName.Count != 0 && (value == Rock.Dyn.Core.ClassMainType.Control))
                {
                    throw new ApplicationException("控制类无法拥有属性");
                }
                _classMainType = value;
            }
        }
        /// <summary>
        /// Base class of this class.
        /// </summary>
        public DynClass BaseClass
        {
            get
            {
                if (!string.IsNullOrEmpty(_baseClassName) && (_baseClass == null || _baseClass.Name != _baseClassName))
                {
                    _baseClass = DynTypeManager.GetClass(_baseClassName);
                }

                return _baseClass;
            }
        }

        /// <summary>
        /// 按ID，Name标识排序后的属性
        /// </summary>
        //public List<DynProperty> SortedProperties
        //{
        //    get
        //    {
        //        if (_sortedProperties.Count == 0)
        //        {
        //            SortProperties();
        //        }
        //        return _sortedProperties;
        //    }
        //}
        /// <summary>
        /// 当前接口的所有方法的字典集合
        /// </summary>
        public Dictionary<string, DynMethod> Methods
        {
            get { return _methods; }
            set { _methods = value; }
        }

        /// <summary>
        /// 属性列表
        /// </summary>
        public DynObject[] Attributes
        {
            get
            {
                return _attributes.Values.ToArray();
            }
        }


        #region Construct

        /// <summary>
        /// 给定类名 构造新的实体类型
        /// <param name="name">类名称</param>
        /// </summary>
        public DynClass(string name)
        {
            _name = name;
        }

        /// <summary>
        /// 基于指定父类构造新的实体类型
        /// <param name="name">类名称</param>
        /// <param name="parentClass">父类</param>
        /// </summary>
        public DynClass(string name, string baseClassName)
            : this(name, baseClassName, null)
        {
        }

        /// <summary>
        /// 给定类名 构造新的实体类型
        /// <param name="name">类名称</param>
        /// </summary>
        public DynClass(string name, List<string> interfaceNames)
            : this(name, null, interfaceNames)
        {
        }

        /// <summary>
        /// 基于指定父类构造新的实体类型
        /// </summary>
        /// <param name="name">类名称</param>
        /// <param name="parentClass">父类</param>
        public DynClass(string name, string baseClassName, List<string> interfaceNames)
        {
            _name = name;

            if (!string.IsNullOrEmpty(baseClassName))
            {
                _baseClassName = baseClassName;
            }

            this.ImplementInterfaces(interfaceNames);

        }

        /// <summary>
        /// 实现接口
        /// </summary>
        /// <param name="interfaceNames"></param>
        public void ImplementInterfaces(List<string> interfaceNames)
        {
            // ToDo:可能有问题,zyw
            if (interfaceNames != null && interfaceNames.Count != 0)
            {
                _interfaceNames.AddRange(interfaceNames);

                //// 实现接口
                //foreach (string interfaceName in interfaceNames)
                //{
                //    DynInterface dynInterface = DynInterfaceFactory.GetInterface(interfaceName);
                //    foreach (DynMethod method in dynInterface.GetMethods())
                //    {
                //        AddMethod(method);
                //    }
                //}
            }
        }
        #endregion

        #region Attributes  
        /// <summary>
        /// 是否包含当前属性
        /// </summary>
        /// <param name="attributeName">属性名称</param>
        /// <returns></returns>
        public bool ContainsAttribute(string attributeName)
        {
            if (!string.IsNullOrEmpty(attributeName))
            {
                return _attributes.ContainsKey(attributeName);
            }
            else
            {
                throw new ApplicationException(string.Format("动态类{0}在检查是否存在标记时 输入参数为空或null", _name));
            }
        }

        /// <summary>
        /// 根据名称获取属性
        /// </summary>
        /// <param name="attributeName">属性名称</param>
        /// <returns></returns>
        public DynObject GetAttribute(string attributeName)
        {
            if (!string.IsNullOrEmpty(attributeName))
            {
                DynObject attribute = null;
                if (_attributes.TryGetValue(attributeName, out attribute))
                {
                    return attribute;
                }
                else
                {
                    throw new ApplicationException(string.Format("动态类{0}不包含标记名称为{1}的属性", _name, attributeName));
                }
            }
            else
            {
                throw new ApplicationException(string.Format("动态类{0}在获取是否存在标记时 输入参数为空或null", _name));
            }
        }

        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="attribute">属性（属性是动态对象）</param>
        public void AddAttribute(DynObject attribute)
        {
            if (attribute != null)
            {
                if (!_attributes.ContainsKey(attribute.DynClass.Name))
                {
                    _attributes[attribute.DynClass.Name] = attribute;
                }
                else
                {
                    throw new ApplicationException(string.Format("类{0}已经包含标记名称为{1}的属性", _name, attribute.DynClass.Name));
                }
            }
            else
            {
                throw new ApplicationException(string.Format("类{0}要添加的标记为null", _name));
            }
        }

        /// <summary>
        /// 移除属性
        /// </summary>
        /// <param name="attributeName">属性名</param>
        public void RemoveAttribute(string attributeName)
        {
            if (_attributes.ContainsKey(attributeName))
            {
                _attributes.Remove(attributeName);
            }
            else
            {
                throw new ApplicationException(string.Format("类{0}不存在标记名称为{1}的属性", _name, attributeName));
            }
        }

        /// <summary>
        /// 获取所有属性
        /// </summary>
        /// <returns>属性组</returns>
        public DynObject[] GetAttributes()
        {
            return _attributes.Values.ToArray();
        }

        /// <summary>
        /// 尝试获取属性
        /// </summary>
        /// <param name="attributeName">属性名</param>
        /// <param name="attributeDynObject">返回属性对象</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetAttribute(string attributeName, out DynObject attributeDynObject)
        {
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new ApplicationException(string.Format("类{0}在尝试获取标记时 输入参数为空或null", _name));
            }

            return  _attributes.TryGetValue(attributeName,out attributeDynObject);
        }

        #endregion

        #region BaseClass

        /// <summary>
        /// 获取所有的动态基类
        /// </summary>
        /// <returns></returns>
        public DynClass[] GetAllBaseClasses()
        {
            if (_allBaseClasses == null)
            {
                List<DynClass> lstEntityTypes = new List<DynClass>();

                DynClass parendynType = this.BaseClass;
                while (parendynType != null)
                {
                    lstEntityTypes.Add(parendynType);

                    parendynType = parendynType.BaseClass;
                }

                _allBaseClasses= lstEntityTypes.ToArray();
            }

            return _allBaseClasses;
        }

        /// <summary>
        /// 当前类是否和传入的类兼容
        /// </summary>
        /// <param name="dynClass">动态类</param>
        /// <returns>true , false</returns>
        public bool IsAssignableFrom(DynClass dynClass)
        {
            if (dynClass.Name == this.Name)
            {
                return true;
            }
            else
            {
                DynClass parendynType = this.BaseClass;

                while (parendynType != null)
                {
                    if (parendynType.Name == dynClass.Name)
                        return true;
                    else
                        parendynType = parendynType.BaseClass;
                }
            }

            return false;
        }

        #endregion

        #region Properties    

        /// <summary>
        /// 添加动态属性
        /// </summary>
        /// <param name="dynProperty">动态属性</param>
        public void AddProperty(DynProperty dynProperty)
        {
            if (dynProperty != null)
            {
                if (!_propertiesByName.ContainsKey(dynProperty.Name) && !_propertiesByID.ContainsKey(dynProperty.ID))
                {
                    _propertiesByName.Add(dynProperty.Name, dynProperty);
                    _propertiesByID.Add(dynProperty.ID, dynProperty);
                }
                else
                {
                    throw new ApplicationException(string.Format("当前类{0}已经存在名称为{1}或ID为{2}的属性", _name, dynProperty.Name, dynProperty.ID));
                }
            }
            else
            {
                throw new ApplicationException("添加的属性不能为null");
            }
        }

        /// <summary>
        /// 根据名称移除属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        public void RemoveProperty(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (_propertiesByName.ContainsKey(propertyName))
                {
                    DynProperty dynProperty = _propertiesByName[propertyName];
                    _propertiesByName.Remove(propertyName);
                    _propertiesByID.Remove(dynProperty.ID);
                }
                else
                {
                    throw new ApplicationException(string.Format("当前类{0}不存在属性名为{1}的属性", _name, propertyName));
                }
            }
            else
            {
                throw new ApplicationException("属性名不能为空或null");
            }

        }

        /// <summary>
        /// 根据ID移除属性
        /// </summary>
        /// <param name="propertyID">属性ID</param>
        public void RemoveProperty(short propertyID)
        {
            DynProperty dynProperty = null;
            if (_propertiesByID.TryGetValue(propertyID, out dynProperty))
            {
                _propertiesByID.Remove(propertyID);
                _propertiesByName.Remove(dynProperty.Name);
            }
            else
            {
                throw new ApplicationException(string.Format("不存在属性ID为{0}的属性", propertyID));
            }

        }

        /// <summary>
        /// 根据属性名获取动态属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns>动态属性</returns>
        public DynProperty GetProperty(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                DynProperty property;
                if (_propertiesByName.TryGetValue(propertyName, out property))
                {
                    return property;
                }
                else
                {
                    if (!string.IsNullOrEmpty(_baseClassName))
                    {
                        return BaseClass.GetProperty(propertyName);
                    }
                    else
                    {
                        throw new ApplicationException(string.Format("当前类{0}或基类没有名称为{1}的属性！", _name, propertyName));
                    }
                }
            }
            else
            {
                throw new ApplicationException("属性名称不能为空或null");
            }
        }

        /// <summary>
        /// 根据属性ID获取动态属性
        /// </summary>
        /// <param name="propertyID">属性ID</param>
        /// <returns>动态属性</returns>
        public DynProperty GetProperty(short propertyID)
        {
            if (_quickFindProperty == null)
            {
                _quickFindProperty = new Dictionary<short, DynProperty>();
            }

            DynProperty result = null;
            if (_quickFindProperty.TryGetValue(propertyID,out result))
            {
                return result;
            }
         
            if (_propertiesByID.TryGetValue(propertyID, out result))
            {
                _quickFindProperty[propertyID] = result;
                return result;
            }
            else
            {
                if (!string.IsNullOrEmpty(_baseClassName))
                {
                    result = BaseClass.GetProperty(propertyID);
                    _quickFindProperty[propertyID] = result;
                    return result;
                }

                throw new ApplicationException(string.Format("当前类{0}或基类没有ID为{1}的属性！", _name, propertyID));
            }
        }

        /// <summary>
        /// 是否包含属性
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>是否</returns>
        public bool ContainsProperty(string propertyName)
        {
            if (_propertiesByName.ContainsKey(propertyName))
            {
                return true;
            }
            else
            {
                if (!string.IsNullOrEmpty(_baseClassName))
                {
                    return BaseClass.ContainsProperty(propertyName);
                }

                return false;
            }
        }

        /// <summary>
        /// 获取所有的属性名
        /// </summary>
        /// <returns>属性名称集合</returns>
        public string[] GetPropertyNames()
        {
            List<string> names = new List<string>();

            names.AddRange(_propertiesByName.Keys.ToArray());

            if (!string.IsNullOrEmpty(_baseClassName))
            {
                names.AddRange(BaseClass.GetPropertyNames());
            }

            return names.ToArray();
        }

        /// <summary>
        /// 获取所有的属性集
        /// </summary>
        /// <returns></returns>
        public DynProperty[] GetProperties()
        {
            List<DynProperty> properties = new List<DynProperty>();

            if (!string.IsNullOrEmpty(_baseClassName))
            {
                properties.AddRange(BaseClass.GetProperties());
            }

            properties.AddRange(_propertiesByName.Values.ToArray());          

            return properties.ToArray();
        }

        /// <summary>
        /// 获取私有属性集
        /// </summary>
        /// <returns></returns>
        public DynProperty[] GetSelfProperties()
        {
            List<DynProperty> properties = new List<DynProperty>();
            properties.AddRange(_propertiesByName.Values.ToArray());
            return properties.ToArray();
        }

        /// <summary>
        /// 获取当前类属性（不包含继承的属性）
        /// </summary>
        /// <returns></returns>
        public DynProperty[] GetLocalProperties()
        {
            List<DynProperty> properties = new List<DynProperty>();

            properties.AddRange(_propertiesByName.Values.ToArray());

            return properties.ToArray();
        }

        /// <summary>
        /// 对属性集合中的属性排序
        /// </summary>
        //private void SortProperties()
        //{
        //    _sortedProperties.Clear();

        //    Dictionary<string, DynProperty> propertiesWithID = new Dictionary<string, DynProperty>();
        //    Dictionary<string, DynProperty> propertiesWithName = new Dictionary<string, DynProperty>();

        //    List<DynProperty> lastProperties = new List<DynProperty>();

        //    DynProperty[] list = GetProperties();

        //    foreach (var item in list)
        //    {
        //        string lowerName = item.Name.ToLower();

        //        //如果属性为：QueryID那么，query就是className
        //        string className = null;
        //        if (lowerName.Contains("id"))
        //        {
        //            className = lowerName.Substring(0, lowerName.IndexOf("id"));
        //            propertiesWithID.Add(className, item);
        //        }
        //        else if (lowerName.Contains("name"))
        //        {
        //            className = lowerName.Substring(0, lowerName.IndexOf("name"));
        //            propertiesWithName.Add(className, item);
        //        }
        //        else
        //        {
        //            lastProperties.Add(item);
        //        }
        //    }

        //    foreach (var item in propertiesWithID)
        //    {
        //        _sortedProperties.Add(item.Value);
        //        DynProperty property = null;
        //        if (propertiesWithName.TryGetValue(item.Key,out property))
        //        {
        //            _sortedProperties.Add(property);
        //            propertiesWithName.Remove(item.Key);
        //        }

        //        if (string.IsNullOrEmpty(item.Key))
        //        {
        //            continue;
        //        }

        //        GetSimilarProperties(lastProperties, item.Key);
        //    }

        //    foreach (var item in propertiesWithName)
        //    {
        //        _sortedProperties.Add(item.Value);
        //        GetSimilarProperties(lastProperties, item.Key);
        //    }

        //    foreach (var item in lastProperties)
        //    {
        //        _sortedProperties.Add(item);
        //    }
        //}

        /// <summary>
        /// 寻找类似属性
        /// </summary>
        /// <param name="dynProperties">属性集合</param>
        /// <param name="similarName">类似的名称</param>
        //private void GetSimilarProperties(List<DynProperty> dynProperties, string similarName)
        //{
        //    bool isSame = true;
        //    for (int index = dynProperties.Count - 1; index >= 0; index--)
        //    {
        //        DynProperty dynProperty = dynProperties[index];
        //        isSame = true;
        //        if (dynProperty.Name.ToLower().IndexOf(similarName) == 0)
        //        {
        //            string name = dynProperty.Name.Substring(similarName.Length);
        //            if (!string.IsNullOrEmpty(name))
        //            {
        //                int j = 0;

        //                for (int i = 0; i < name.Length; i++)
        //                {
        //                    if (name[i] >= 'A' && name[i] <= 'Z')
        //                    {
        //                        j++;
        //                        if (j >= 2)
        //                        {
        //                            isSame = false;
        //                            break;
        //                        }
        //                    }
        //                }

        //                if (isSame)
        //                {
        //                    _sortedProperties.Add(dynProperty);
        //                    dynProperties.Remove(dynProperty);
        //                }
        //            }

        //        }
        //    }
        //}

        #endregion

        #region Methods

        /// <summary>
        /// 判断当前接口是否包含本方法
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <returns>true, false</returns>
        public bool ContainsMethod(string methodName)
        {
            if (!string.IsNullOrEmpty(methodName))
            {
                return _methods.ContainsKey(methodName);
            }
            else
            {
                throw new ApplicationException("方法名不能为空或null");
            }
        }

        /// <summary>
        /// 获取方法
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <returns>动态方法</returns>
        public DynMethod GetMethod(string methodName)
        {
            if (!string.IsNullOrEmpty(methodName))
            {
                DynMethod method = null;
                if (_methods.TryGetValue(methodName, out method))
                {
                    return method;
                }
                else
                {
                    throw new ApplicationException(string.Format("当前类{0}不存在方法名为{1}的方法", _name, methodName));
                }
            }
            else
            {
                throw new ApplicationException("方法名不能为空或null");
            }
        }

        /// <summary>
        /// 添加方法
        /// </summary>
        /// <param name="method">动态方法</param>
        public void AddMethod(DynMethod method)
        {
            if (method != null)
            {
                DynMethod oldmethod = null;
                if (!_methods.TryGetValue(method.Name,out oldmethod))
                {
                    _methods.Add(method.Name, method);
                    method.ClassName = _name;
                }
                else
                {
                    throw new ApplicationException(string.Format("当前类{0}已经存在名为{1}的方法", _name, oldmethod.Name));
                }
            }
            else
            {
                throw new ApplicationException("方法不能为null");
            }
        }

        /// <summary>
        /// 移除方法
        /// </summary>
        /// <param name="methodName">方法名称</param>
        public void RemoveMethod(string methodName)
        {
            if (!string.IsNullOrEmpty(methodName))
            {
                if (_methods.ContainsKey(methodName))
                {
                    _methods.Remove(methodName);
                }
                else
                {
                    throw new ApplicationException(string.Format("当前类{0}不存在方法名为{1}的方法", _name, methodName));
                }
            }
            else
            {
                throw new ApplicationException("方法名不能为空或null");
            }
        }

        /// <summary>
        /// 获取方法集合
        /// </summary>
        /// <returns></returns>
        public DynMethod[] GetMethods()
        {
            return _methods.Values.ToArray();
        }
        #endregion

        ///// <summary>
        ///// 类方法调用 TODO:对象内部方法作为服务,暂时不支持
        ///// </summary>
        ///// <param name="methodName">方法短名，不包含类名称</param>
        ///// <param name="dicParams">参数和值的字典列表</param>
        ///// <returns>返回结果</returns>
        //public object Call(string methodName, Dictionary<string, object> dicParams)
        //{
        //    return DynTypeManager.MethodHandler(null, _name + "_" + methodName, dicParams);
        //}

        #region 关联字典       

        public void AddRelation(DynProperty dynProperty)
        {
            if (dynProperty == null || !this.ContainsProperty(dynProperty.Name))
            {
                throw new ApplicationException("寻求关联的类不存在");
            }
            if (dynProperty.CollectionType == CollectionType.None && dynProperty.DynType == DynType.Struct)
            {
                DynClass relationDynClass = DynTypeManager.GetClass(dynProperty.StructName);
                if (relationDynClass == null)
                {
                    throw new ApplicationException(string.Format("类{0}的属性{1}所添加的关联类{2}不存在", this.Name, dynProperty.Name, dynProperty.StructName));
                }
               
                    relationDynClass.RelationDynClassDict[this.Name]= this;
                    relationDynClass.RelationDynPropertyDict[this.Name]= dynProperty;
            }
        }

        public void RemoveRelation(DynProperty dynProperty)
        {
            if (dynProperty == null || !this.ContainsProperty(dynProperty.Name))
            {
                throw new ApplicationException("寻求关联的类不存在");
            }
            if (dynProperty.CollectionType == CollectionType.None && dynProperty.DynType == DynType.Struct)
            {
                DynClass relationDynClass = DynTypeManager.GetClass(dynProperty.StructName);
                if (relationDynClass == null)
                {
                    throw new ApplicationException(string.Format("类{0}的属性{1}所添加的关联类{2}不存在", this.Name, dynProperty.Name, dynProperty.StructName));
                }
               
                    relationDynClass.RelationDynClassDict.Remove(this.Name);
                    relationDynClass.RelationDynPropertyDict.Remove(this.Name);
            }
        }

        internal void MakeRelation()
        {
            foreach (DynProperty dynProperty in _propertiesByName.Values)
            {
                AddRelation(dynProperty);
            }
        }
        #endregion
    }
}
