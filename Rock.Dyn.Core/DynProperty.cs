using System.Collections.Generic;
using System.Linq;
using System;

namespace Rock.Dyn.Core
{
    /// <summary>
    /// 动态属性
    /// </summary>
    public class DynProperty
    {
        #region Basic Properties

        private short _id;
        private string _name;
        private DynType _dynType;
        private bool _isArray;
        private bool _isInherited = false;
        private DynClass _dynClass;
        private DynClass _currentDynClass;
        private string _inheritClassName;
        private CollectionType _collectionType;
        private string _structName;
        private string _displayName;
        private string _description;
        private DynClass _structClass;

        private Dictionary<string, DynObject> _attributes = new Dictionary<string, DynObject>();
        //private Dictionary<string, IValidate> _validater = new Dictionary<string, IValidate>();
        //private Dictionary<string, DynObject> _validateConfig = new Dictionary<string, DynObject>();

        /// <summary>
        /// 显示名
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        /// <summary>
        /// 继承的实体名称
        /// </summary>
        public string InheritClassName
        {
            get { return _inheritClassName; }
            set { _inheritClassName = value; }
        }

        /// <summary>
        /// 属性ID
        /// </summary>
        public short ID
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// 属性的数据类型
        /// </summary>
        public DynType DynType
        {
            get { return _dynType; }
            set { _dynType = value; }
        }

        /// <summary>
        /// 集合类型，包含None,List,Set,Map
        /// </summary>
        public CollectionType CollectionType
        {
            get { return _collectionType; }
            set { _collectionType = value; }
        }

        /// <summary>
        ///  当DynType为Struct时，这个名称有意义
        /// </summary>
        public string StructName
        {
            get { return _structName; }
            set { _structName = value; }
        }

        /// <summary>
        /// 是否是集合类型
        /// string[], EntityArrayList
        /// </summary>
        public bool IsArray
        {
            get { return _isArray; }
            set { _isArray = value; }
        }

        private bool _isNotNull = false;
        public bool IsNotNull
        {
            get { return _isNotNull; }
            set { _isNotNull = value; }
        }

        /// <summary>
        /// 是否是继承属性
        /// </summary>
        public bool IsInherited
        {
            get { return _isInherited; }
            set { _isInherited = value; }
        }

        /// <summary>
        /// 属性所在的实体类型
        /// </summary>
        public DynClass DynClass
        {
            get { return _dynClass; }
            set { _dynClass = value; }
        }

        /// <summary>
        /// 属性本身的EntityType
        /// 当PropertyType类型为EntityArrayList时，返回对应的EntityType
        /// </summary>
        public DynClass CurrentDynClass
        {
            get { return _currentDynClass; }
            set { _currentDynClass = value; }
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
        /// 属性列表
        /// </summary>
        public DynObject[] Attributes
        {
            get
            {
                return _attributes.Values.ToArray();
            }
        }
        /// <summary>
        /// 验证字典 IValidate为 验证器 DynObject为验证标记实例 记录特性的内容
        /// </summary>
        //public Dictionary<string, DynObject> ValidateConfig
        //{
        //    get { return _validateConfig; }
        //    set { _validateConfig = value; }
        //}
        //public Dictionary<string, IValidate> Validater
        //{
        //    get { return _validater; }
        //    set { _validater = value; }
        //}
        #endregion

        #region Construct

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">属性名</param>
        public DynProperty(short id, string name)
        {
            _id = id;
            _name = name;

            _isArray = false;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="propertyType"></param>
        public DynProperty(short id, string name, DynType propertyType)
        {
            _id = id;
            _name = name;

            _collectionType = CollectionType.None;
            _dynType = propertyType;
            _structName = "";

            _isArray = false;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="collectionType"></param>
        /// <param name="propertyType"></param>
        /// <param name="structName"></param>
        public DynProperty(short id, string name, CollectionType collectionType, DynType propertyType, string structName)
        {
            _id = id;
            _name = name;

            _collectionType = collectionType;
            _dynType = propertyType;
            _structName = structName;

            _isArray = false;
        }

        #endregion

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
                throw new ApplicationException("属性名为空或null");
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
                if (_attributes.ContainsKey(attributeName))
                {
                    return _attributes[attributeName];
                }
                else
                {
                    throw new ApplicationException(string.Format("不包含属性名称为{0}的属性", attributeName));
                }
            }
            else
            {
                throw new ApplicationException("属性名为空或null");
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
                    throw new ApplicationException(string.Format("已经包含属性名称为{0}的属性", attribute.DynClass.Name));
                }
            }
            else
            {
                throw new ApplicationException("要添加的属性为null");
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
                throw new ApplicationException(string.Format("不存在属性名称为{0}的属性", attributeName));
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
            return _attributes.TryGetValue(attributeName, out attributeDynObject);
        }

        //public void AddValidate(string name, IValidate validater, DynObject validateConfig)
        //{
        //    if (!string.IsNullOrEmpty(name) && validater != null && validateConfig != null)
        //    {
        //        _validater[name] = validater;
        //        _validateConfig[name] = validateConfig;
        //    }
        //}

        //public void ClearValidate()
        //{
        //    _validater.Clear();
        //    _validateConfig.Clear();
        //}

        //public void RemoveValidate(string name)
        //{
        //    _validater.Remove(name);
        //    _validateConfig.Remove(name);
        //}


        public DynClass GetStructName()
        {
            if (!string.IsNullOrEmpty(this.StructName))
            {
                if (_structClass == null || _structClass.Name != this.StructName)
                {
                    _structClass = DynTypeManager.GetClass(this.StructName);
                }
                return _structClass;
            }
            return null;
        }

        /// <summary>
        /// 转化符合属性类型的返回值
        /// </summary>
        /// <param name="obj">输入的值</param>
        /// <param name="dynProperty">输入的参数的属性</param>
        /// <param name="result">返回的转化好类型的值</param>
        /// <returns>是否转换成功</returns>
        public bool TryPasentValue(object obj, DynProperty dynProperty, ref object result)
        {
            bool isSucces = false;

            try
            {
                switch (dynProperty.CollectionType)
                {
                    case CollectionType.None:
                        switch (dynProperty.DynType)
                        {
                            case DynType.Void:
                                break;
                            case DynType.Bool:
                                result = Convert.ToBoolean(obj);
                                isSucces = true;
                                break;
                            case DynType.Byte:
                                result = Convert.ToByte(obj);
                                isSucces = true;
                                break;
                            case DynType.String:
                                result = Convert.ToString(obj);
                                isSucces = true;
                                break;
                            case DynType.Struct:
                                isSucces = false;
                                break;
                            case DynType.DateTime:
                                result = Convert.ToDateTime(obj);
                                isSucces = true;
                                break;
                            case DynType.Double:
                                result = Convert.ToDouble(obj);
                                isSucces = true;
                                break;
                            case DynType.Decimal:
                                result = Convert.ToDecimal(obj);
                                isSucces = true;
                                break;
                            case DynType.I16:
                                result = Convert.ToInt16(obj);
                                isSucces = true;
                                break;
                            case DynType.I32:
                                result = Convert.ToInt32(obj);
                                isSucces = true;
                                break;
                            case DynType.I64:
                                result = Convert.ToInt64(obj);
                                isSucces = true;
                                break;
                            default:
                                break;
                        }
                        break;
                    case CollectionType.List:
                        switch (dynProperty.DynType)
                        {
                            case DynType.Void:
                            case DynType.Bool:
                                if (obj is string)
                                {
                                    string[] trueObjs = obj.ToString().Split(',');
                                    List<Boolean> trueValue = new List<Boolean>();
                                    foreach (var trueObj in trueObjs)
                                    {
                                        trueValue.Add(Convert.ToBoolean(trueObj));
                                    }
                                    result = trueValue;
                                }
                                else
                                {
                                    isSucces = obj is List<Boolean>;
                                    result = obj as List<Boolean>;
                                }
                                break;
                            case DynType.Byte:
                                if (obj is string)
                                {
                                    string[] trueObjs = obj.ToString().Split(',');
                                    List<Byte> trueValue = new List<Byte>();
                                    foreach (var trueObj in trueObjs)
                                    {
                                        trueValue.Add(Convert.ToByte(trueObj));
                                    }
                                    result = trueValue;
                                }
                                else
                                {
                                    isSucces = obj is List<Byte>;
                                    result = obj as List<Byte>;
                                }
                                break;
                            case DynType.String:
                                if (obj is string)
                                {
                                    string[] trueObjs = obj.ToString().Split(',');
                                    List<String> trueValue = new List<String>();
                                    foreach (var trueObj in trueObjs)
                                    {
                                        trueValue.Add(Convert.ToString(trueObj));
                                    }
                                    result = trueValue;
                                }
                                else
                                {
                                    isSucces = obj is List<String>;
                                    result = obj as List<String>;
                                }
                                break;
                            case DynType.Struct:
                                isSucces = false;
                                break;
                            case DynType.Double:
                                if (obj is string)
                                {
                                    string[] trueObjs = obj.ToString().Split(',');
                                    List<Double> trueValue = new List<Double>();
                                    foreach (var trueObj in trueObjs)
                                    {
                                        trueValue.Add(Convert.ToDouble(trueObj));
                                    }
                                    result = trueValue;
                                }
                                else
                                {
                                    isSucces = obj is List<Double>;
                                    result = obj as List<Double>;
                                }
                                break;
                            case DynType.Decimal:
                                if (obj is string)
                                {
                                    string[] trueObjs = obj.ToString().Split(',');
                                    List<Decimal> trueValue = new List<Decimal>();
                                    foreach (var trueObj in trueObjs)
                                    {
                                        trueValue.Add(Convert.ToDecimal(trueObj));
                                    }
                                    result = trueValue;
                                }
                                else
                                {
                                    isSucces = obj is List<Decimal>;
                                    result = obj as List<Decimal>;
                                }
                                break;
                            case DynType.I16:
                                if (obj is string)
                                {
                                    string[] trueObjs = obj.ToString().Split(',');
                                    List<Int16> trueValue = new List<Int16>();
                                    foreach (var trueObj in trueObjs)
                                    {
                                        trueValue.Add(Convert.ToInt16(trueObj));
                                    }
                                    result = trueValue;
                                }
                                else
                                {
                                    isSucces = obj is List<Int16>;
                                    result = obj as List<Int16>;
                                }
                                break;
                            case DynType.I32:
                                if (obj is string)
                                {
                                    string[] trueObjs = obj.ToString().Split(',');
                                    List<Int32> trueValue = new List<Int32>();
                                    foreach (var trueObj in trueObjs)
                                    {
                                        trueValue.Add(Convert.ToInt32(trueObj));
                                    }
                                    result = trueValue;
                                }
                                else
                                {
                                    isSucces = obj is List<Int32>;
                                    result = obj as List<Int32>;
                                }
                                break;
                            case DynType.I64:
                                if (obj is string)
                                {
                                    string[] trueObjs = obj.ToString().Split(',');
                                    List<Int64> trueValue = new List<Int64>();
                                    foreach (var trueObj in trueObjs)
                                    {
                                        trueValue.Add(Convert.ToInt64(trueObj));
                                    }
                                    result = trueValue;
                                }
                                else
                                {
                                    isSucces = obj is List<Int64>;
                                    result = obj as List<Int64>;
                                }
                                break;
                            case DynType.DateTime:
                                if (obj is string)
                                {
                                    string[] trueObjs = obj.ToString().Split(',');
                                    List<DateTime> trueValue = new List<DateTime>();
                                    foreach (var trueObj in trueObjs)
                                    {
                                        trueValue.Add(Convert.ToDateTime(trueObj));
                                    }
                                    result = trueValue;
                                }
                                else
                                {
                                    isSucces = obj is List<DateTime>;
                                    result = obj as List<DateTime>;
                                }
                                break;
                            default:
                                break;
                        }

                        break;

                    case CollectionType.Map:
                        if (obj is string)
                        {
                            string objStr = obj.ToString();
                            if (objStr.Contains(":"))
                            {
                                string[] trueObjs = obj.ToString().Split(',');
                                Dictionary<string, object> trueValue = new Dictionary<string, object>();
                                foreach (var trueObj in trueObjs)
                                {
                                    string[] trueObjKeyValue = trueObj.Split(':');
                                    trueValue.Add(trueObjKeyValue[0], trueObj.Substring(trueObjKeyValue[0].Length));
                                }
                                isSucces = true;
                                result = trueValue;
                            }
                        }
                        break;
                }
            }
            catch
            {
                isSucces = false;
            }
            return isSucces;
        }

        /// <summary>
        /// 转化符合属性类型的返回值
        /// </summary>
        /// <param name="obj">输入的值</param>
        /// <param name="dynPropertyName">输入的参数的属性名</param>
        /// <param name="result">返回的转化好类型的值</param>
        /// <returns>是否转换成功</returns>
        public bool TryPasentValue(object obj, string dynPropertyName, ref object result)
        {
            if (_dynClass.ContainsProperty(dynPropertyName))
            {
                return TryPasentValue(obj, _dynClass.GetProperty(dynPropertyName), ref result);
            }
            else
            {
                throw new ApplicationException(string.Format("转化值【{2}】失败：类【{0}】不包含属性【{1}】", this._dynClass.Name, dynPropertyName, obj));
            }
        }

        public object GetDefaultValue()
        {
            DynObject dynDefaultValue = null;
            if (ContainsAttribute("DefaultValue"))
            {
                dynDefaultValue = GetAttribute("DefaultValue");
            }

            bool isHaveDefaultValue = false;
            if (dynDefaultValue != null && !string.IsNullOrEmpty(dynDefaultValue["Value"] as string))
            {
                string defaultType = dynDefaultValue["Type"] as string;
                string defaultValue = dynDefaultValue["Value"] as string;
                switch (defaultType)
                {
                    case "Direct":
                        object trueValue = null;
                        if (TryPasentValue(defaultValue, this, ref trueValue))
                        {
                            isHaveDefaultValue = true;
                            return trueValue;
                        }
                        break;
                    case "Compute":
                        return DynStringResolver.Resolve(defaultValue);
                    default:
                        break;
                }
            }

            if (!isHaveDefaultValue)
            {
                switch (CollectionType)
                {
                    case CollectionType.None:
                        if (IsNotNull)
                        {
                            switch (DynType)
                            {
                                case DynType.Void:
                                case DynType.Bool:
                                    return false;
                                case DynType.Byte:
                                    return null;
                                case DynType.String:
                                    return "";
                                case DynType.Struct:
                                    return null;
                                case DynType.DateTime:
                                    return DateTime.Now;
                                case DynType.Double:
                                    return 0.0;
                                case DynType.Decimal:
                                    return (decimal)0.0;
                                case DynType.I16:
                                    return (short)0;
                                case DynType.I32:
                                    return (int)0;
                                case DynType.I64:
                                    return (long)0;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            return null; ;
                        }

                        break;

                    case CollectionType.List:
                        switch (DynType)
                        {
                            case DynType.Void:
                            case DynType.Bool:
                                return new List<bool>();
                            case DynType.Byte:
                                return new List<Byte>();
                            case DynType.String:
                                return new List<String>();
                            case DynType.Struct:
                                return new List<DynObject>();
                            case DynType.Double:
                                return new List<Double>();
                            case DynType.Decimal:
                                return new List<Decimal>();
                            case DynType.I16:
                                return new List<Int16>();
                            case DynType.I32:
                                return new List<Int32>();
                            case DynType.I64:
                                return new List<Int64>();
                            case DynType.DateTime:
                                return new List<DateTime>();
                            default:
                                break;
                        }

                        break;

                    case CollectionType.Map:
                        return new Dictionary<string, object>();
                    case CollectionType.Set:
                        throw new ApplicationException("暂不支持Set集合类型");
                }
            }
            return null;
        }

    }
}
