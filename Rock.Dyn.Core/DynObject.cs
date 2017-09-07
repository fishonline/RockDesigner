using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Dynamic;

namespace Rock.Dyn.Core
{
    /// <summary>
    /// The dynamic entity class
    /// </summary>
    public class DynObject : DynamicObject
    {
        private Dictionary<string, object> _propertyValues = new Dictionary<string, object>();
        private DynClass _dynClass;

        //public delegate void ReferValidateDelegate(DynObject dynObject);
        //public static ReferValidateDelegate ReferValidateHandle = new ReferValidateDelegate(DefaultReferValidate);
        /// <summary>
        /// 属性的值集合
        /// </summary>
        public Dictionary<string, object> PropertyValues
        {
            get
            {
                if (_propertyValues == null)
                {
                    _propertyValues = new Dictionary<string, object>();
                }
                return _propertyValues;
            }
        }
        /// <summary>
        /// 实体类型
        /// </summary>
        public DynClass DynClass
        {
            get { return _dynClass; }
        }
        /// <summary>
        /// 基于实体类型构造对象
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="isHaveDefualt">是否成带默认值的对象</param>
        public DynObject(string className, bool isHaveDefualt)
        {
            DynClass dynClass = DynTypeManager.GetClass(className);

            if (dynClass == null)
                throw new ApplicationException(string.Format("不存在名称为{0}的动态类", className));

            _dynClass = dynClass;

            if (isHaveDefualt)
            {
                var properties = _dynClass.GetProperties();
                foreach (DynProperty dynProperty in properties)
                {
                    SetDefaultValue(dynProperty);
                }
            }
        }

        public DynObject(string className)
            : this(className, true)
        {
        }

        /// <summary>
        /// 刷新为空的默认值
        /// </summary>
        public void FlushDefault()
        {
            foreach (var propertyValue in _propertyValues)
            {
                if (propertyValue.Value == null)
                {
                    DynProperty p = this.DynClass.GetProperty(propertyValue.Key);
                    SetDefaultValue(p);
                }
            }
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        /// <param name="dynProperty">属性</param>
        public void SetDefaultValue(DynProperty dynProperty)
        {
            if (dynProperty == null)
            {
                throw new ApplicationException("设置属性默认值时 入参不能为空");
            }
            _propertyValues[dynProperty.Name] = dynProperty.GetDefaultValue();
        }

        /// <summary>
        /// 根据名称获取属性
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性</returns>
        public DynProperty GetProperty(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                return _dynClass.GetProperty(propertyName);
            }
            else
            {
                throw new ApplicationException(string.Format("动态对象{0}获取属性方法中的属性名称不能为空或null", _dynClass.Name));
            }
        }

        /// <summary>
        /// 根据属性ID获取属性
        /// </summary>
        /// <param name="propertyID"></param>
        /// <returns></returns>
        public DynProperty GetProperty(short propertyID)
        {
            return _dynClass.GetProperty(propertyID);
        }

        /// <summary>
        /// Get the property value.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns>The value.</returns>
        public object GetPropertyValue(string propertyName)
        {
            DynProperty dynProperty = GetProperty(propertyName);

            object value = null;
            if (_propertyValues.TryGetValue(propertyName, out value))
                return value;
            else if (dynProperty != null)
            {
                SetPropertyValue(propertyName, null);
                return _propertyValues[propertyName];
            }
            else
                throw new ApplicationException(string.Format("类{0}不存在属性{1}", _dynClass == null ? "[无]" : _dynClass.Name, propertyName));
        }

        /// <summary>
        /// Set the property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void SetPropertyValue(string propertyName, object propertyValue)
        {
            if (_dynClass.ContainsProperty(propertyName))
            {
                DynProperty dynProperty = _dynClass.GetProperty(propertyName);
                SetPropertyValue(dynProperty, propertyValue);
            }
            else
            {
                throw new ApplicationException(string.Format("不存在属性{0}", propertyName));
            }
        }

        /// <summary>
        /// Set the property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void SetPropertyValue(DynProperty property, object propertyValue)
        {

            if (propertyValue != null)
            {
                CollectionType collectionType = property.CollectionType;
                DynType dynType = property.DynType;
                string structName = property.StructName;

                bool isCorrectType = false;

                #region 类型判断

                switch (collectionType)
                {
                    case CollectionType.None:
                        switch (dynType)
                        {
                            case DynType.Void:
                                break;
                            case DynType.Bool:
                                isCorrectType = propertyValue is Boolean;
                                break;
                            case DynType.Byte:
                                isCorrectType = propertyValue is Byte;
                                break;
                            case DynType.Double:
                                isCorrectType = propertyValue is Double || propertyValue is Single || propertyValue is Int64 || propertyValue is Int32 || propertyValue is UInt32 || propertyValue is UInt16 || propertyValue is Int16;
                                break;
                            case DynType.Decimal:
                                isCorrectType = propertyValue is Decimal || propertyValue is Double || propertyValue is Single || propertyValue is Int64 || propertyValue is Int32 || propertyValue is UInt32 || propertyValue is UInt16 || propertyValue is Int16;
                                break;

                            case DynType.I16:
                                isCorrectType = propertyValue is Int16;
                                break;
                            case DynType.I32:
                                isCorrectType = propertyValue is Int32 || propertyValue is UInt16 || propertyValue is Int16;
                                break;
                            case DynType.I64:
                                isCorrectType = propertyValue is Int64 || propertyValue is Int32 || propertyValue is UInt32 || propertyValue is UInt16 || propertyValue is Int16;
                                break;
                            case DynType.String:
                                isCorrectType = propertyValue is String;
                                break;
                            case DynType.DateTime:
                                isCorrectType = propertyValue is DateTime;
                                break;
                            case DynType.Struct:
                                isCorrectType = propertyValue is DynObject;
                                break;
                            case DynType.Binary:
                                isCorrectType = propertyValue is byte[];
                                break;
                            default:
                                break;
                        }
                        break;
                    case CollectionType.List:

                        switch (dynType)
                        {
                            case DynType.Void:
                                break;
                            case DynType.Bool:
                                isCorrectType = propertyValue is List<Boolean>;
                                break;
                            case DynType.Byte:
                                isCorrectType = propertyValue is List<Byte>;
                                break;
                            case DynType.Double:
                                isCorrectType = propertyValue is List<Double>;
                                break;
                            case DynType.Decimal:
                                isCorrectType = propertyValue is List<Decimal>;
                                break;
                            case DynType.I16:
                                isCorrectType = propertyValue is List<Int16>;
                                break;
                            case DynType.I32:
                                isCorrectType = propertyValue is List<Int32>;
                                break;
                            case DynType.I64:
                                isCorrectType = propertyValue is List<Int64>;
                                break;
                            case DynType.String:
                                isCorrectType = propertyValue is List<String>;
                                break;
                            case DynType.DateTime:
                                isCorrectType = propertyValue is List<String>;
                                break;
                            case DynType.Struct:
                                isCorrectType = propertyValue is List<DynObject>;
                                break;
                            default:
                                break;
                        }
                        break;
                    case CollectionType.Set:
                        break;
                    case CollectionType.Map:
                        isCorrectType = propertyValue is Dictionary<string, object>;
                        break;
                    default:
                        break;
                }
                #endregion

                if (isCorrectType)
                {
                    _propertyValues[property.Name] = propertyValue;
                }
                else
                {
                    string msg = string.Format("属性{0}应该是{1}:{2}类型，而把{3}类型赋给它", property.Name, Enum.GetName(typeof(CollectionType), collectionType), Enum.GetName(typeof(DynType), dynType), propertyValue.GetType().ToString());
                    throw new ApplicationException(msg);
                }
            }
            else
            {
                SetDefaultValue(property);
            }
        }

        /// <summary>
        /// 强制赋值 但是值是否正确不做检查 后果自负
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void SetPropertyValueWithOutCheck(string propertyName, object propertyValue)
        {
            if (_dynClass.ContainsProperty(propertyName))
            {
                DynProperty dynProperty = _dynClass.GetProperty(propertyName);
                SetPropertyValueWithOutCheck(dynProperty, propertyValue);
            }
            else
            {
                throw new ApplicationException(string.Format("不存在属性{0}", propertyName));
            }
        }

        /// <summary>
        ///  强制赋值 但是值是否正确不做检查 后果自负
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void SetPropertyValueWithOutCheck(DynProperty property, object propertyValue)
        {
            if (propertyValue != null)
            {
                _propertyValues[property.Name] = propertyValue;
            }
            else
            {
                SetDefaultValue(property);
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
            set { SetPropertyValue(propertyName, value); }
        }

        /// <summary>
        /// Gets the property values.
        /// </summary>
        /// <returns>The values.</returns>
        public object[] GetPropertyValues()
        {
            return _propertyValues.Values.ToArray();
        }

        #region Call

        /// <summary>
        /// 调用实例方法
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="dicParams"></param>
        /// <returns></returns>
        public object Call(string methodName, Dictionary<string, object> dicParams)
        {
            return DynTypeManager.MethodHandler(this, _dynClass.Name + "_" + methodName, dicParams);
        }

        #endregion

        /// <summary>
        /// 浅度克隆
        /// </summary>
        /// <returns></returns>
        public DynObject Clone()
        {
            DynObject cloneDynObject = new DynObject(this.DynClass.Name);

            DynProperty[] dynProperties = this._dynClass.GetProperties();

            foreach (var dynProperty in dynProperties)
            {

                switch (dynProperty.CollectionType)
                {
                    case CollectionType.None:
                        switch (dynProperty.DynType)
                        {
                            case DynType.Void:
                                break;
                            case DynType.Bool:
                            case DynType.Byte:
                            case DynType.Double:
                            case DynType.Decimal:
                            case DynType.I16:
                            case DynType.I32:
                            case DynType.I64:
                            case DynType.String:
                            case DynType.DateTime:
                                cloneDynObject[dynProperty.Name] = this.GetPropertyValue(dynProperty.Name);
                                break;
                            case DynType.Struct:
                                break;
                            default:
                                break;
                        }
                        break;
                    case CollectionType.List:
                        IList valueList = null;
                        switch (dynProperty.DynType)
                        {
                            case DynType.Void:
                                break;
                            case DynType.Bool:
                                valueList = new List<Boolean>();
                                break;
                            case DynType.Byte:
                                valueList = new List<Byte>();
                                break;
                            case DynType.Double:
                                valueList = new List<Double>();
                                break;
                            case DynType.Decimal:
                                valueList = new List<Decimal>();
                                break;
                            case DynType.I16:
                                valueList = new List<Int16>();
                                break;
                            case DynType.I32:
                                valueList = new List<Int32>();
                                break;
                            case DynType.I64:
                                valueList = new List<Int64>();
                                break;
                            case DynType.String:
                                valueList = new List<String>();
                                break;
                            case DynType.Struct:
                                break;
                            case DynType.DateTime:
                                valueList = new List<String>();
                                break;
                            default:
                                break;
                        }

                        if (valueList != null)
                        {
                            IList baseList = this.GetPropertyValue(dynProperty.Name) as IList;
                            foreach (var item in baseList)
                            {
                                valueList.Add(item);
                            }

                            cloneDynObject[dynProperty.Name] = valueList;
                        }

                        break;
                    case CollectionType.Set:

                        break;
                    case CollectionType.Map:
                        Dictionary<string, object> valueMap = new Dictionary<string, object>();
                        IDictionary oldmap = this.GetPropertyValue(dynProperty.Name) as IDictionary;
                        foreach (DictionaryEntry item in oldmap)
                        {
                            if (item.Value is DynObject || item.Value is ICollection)
                            {
                                valueMap.Add(item.Key as string, null);
                            }
                            else
                            {
                                valueMap.Add(item.Key as string, item.Value);
                            }
                        }
                        cloneDynObject[dynProperty.Name] = valueMap;
                        break;
                    default:
                        break;
                }
            }


            return cloneDynObject;
        }


        //public void SelfValidate()
        //{
        //    SelfValidate(_propertyValues.Keys.ToArray<string>());
        //}

        //public void SelfValidate(string[] feilds)
        //{
        //    if (feilds != null)
        //    {
        //        foreach (var propertyName in feilds)
        //        {
        //            DynProperty dynProperty = GetProperty(propertyName);
        //            foreach (var validater in dynProperty.Validater)
        //            {
        //                DynObject vailidateConfig = dynProperty.ValidateConfig[validater.Key];
        //                if (vailidateConfig != null)
        //                {
        //                    validater.Value.ValidateProperty(_propertyValues[propertyName], dynProperty.ValidateConfig[validater.Key], dynProperty, this);
        //                }
        //                else
        //                {
        //                    throw new ApplicationException("给定的验证器没有 验证器配置");
        //                }
        //            }
        //        }
        //    }
        //}




        //private static void DefaultReferValidate(DynObject dynObject)
        //{
        //    //默认关联验证
        //    //不增加的
        //}

        //public void ReferValidate()
        //{
        //    ReferValidateHandle(this);
        //}
        /// <summary>
        /// 尝试获取属性
        /// </summary>
        /// <param name="attributeName">属性名</param>
        /// <param name="attributeDynObject">返回属性对象</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetClassAttribute(string attributeName, out DynObject attributeDynObject)
        {
            return this.DynClass.TryGetAttribute(attributeName, out attributeDynObject);
        }



        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_dynClass == null)
            {
                return false;
            }
            else //if (DynClass.ContainsProperty(binder.Name))
            {
                SetPropertyValue(binder.Name, value);
                return true;
            }
            //else
            //{
            //    return false;
            //}
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            //throw new Exception("23423423");

            if (_dynClass == null)
            {
                result = null;
                return false;
            }
            else //if (DynClass.ContainsProperty(binder.Name))
            {
                result = GetPropertyValue(binder.Name);
                return true;
            }
            //else
            //{
            //    result = null;
            //    return false;
            //}
        }    

    }
}
