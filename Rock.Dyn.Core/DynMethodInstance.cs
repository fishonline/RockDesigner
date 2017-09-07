using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Dyn.Core
{
    public class DynMethodInstance
    {
        private DynMethod _dynMethod;
        private string _fullName;

        private Dictionary<string, object> _paramsValues = new Dictionary<string, object>();
        private object _result = null;

        public DynMethodInstance(string interfaceName, string methodName)
        {
            DynMethod dynMethod = DynTypeManager.GetInterface(interfaceName).GetMethod(methodName);

            if (dynMethod == null)
            {
                throw new ApplicationException(string.Format("接口【{0}】下面找不到，方法名为【{1}】的方法", interfaceName, methodName));
            }

            _dynMethod = dynMethod;
            _fullName = interfaceName + "_" + methodName;

            foreach (DynParameter dynParameter in _dynMethod.GetParameters())
            {
                SetDefaultValue(dynParameter);
            }

        }

        public DynMethod DynMethod
        {
            get { return _dynMethod; }
        }

        public string FullName
        {
            get { return _fullName; }
        }

        public object Result
        {
            get { return _result; }
            set { _result = value; }
        }

        private void SetDefaultValue(DynParameter dynParameter)
        {
            switch (dynParameter.CollectionType)
            {
                case CollectionType.None:
                    switch (dynParameter.DynType)
                    {
                        case DynType.Void:
                            break;
                        case DynType.Bool:
                            _paramsValues[dynParameter.Name] = false;
                            break;
                        case DynType.Byte:
                        case DynType.String:
                        case DynType.Struct:
                        case DynType.DateTime:
                            _paramsValues[dynParameter.Name] = null;
                            break;
                        case DynType.Double:
                        case DynType.Decimal:
                            _paramsValues[dynParameter.Name] = null;
                            break;
                        case DynType.I16:
                        case DynType.I32:
                        case DynType.I64:
                            _paramsValues[dynParameter.Name] = null;
                            break;
                        case DynType.Binary:
                            _paramsValues[dynParameter.Name] = null;
                            break;
                        default:
                            break;
                    }

                    break;

                case CollectionType.List:
                    switch (dynParameter.DynType)
                    {
                        case DynType.Void:
                        case DynType.Bool:
                            _paramsValues[dynParameter.Name] = new List<bool>();
                            break;
                        case DynType.Byte:
                            _paramsValues[dynParameter.Name] = new List<Byte>();
                            break;
                        case DynType.String:
                            _paramsValues[dynParameter.Name] = new List<String>();
                            break;
                        case DynType.Struct:
                            _paramsValues[dynParameter.Name] = new List<DynObject>();
                            break;
                        case DynType.Double:
                            _paramsValues[dynParameter.Name] = new List<Double>();
                            break;
                        case DynType.Decimal:
                            _paramsValues[dynParameter.Name] = new List<Decimal>();
                            break;
                        case DynType.I16:
                            _paramsValues[dynParameter.Name] = new List<Int16>();
                            break;
                        case DynType.I32:
                            _paramsValues[dynParameter.Name] = new List<Int32>();
                            break;
                        case DynType.I64:
                            _paramsValues[dynParameter.Name] = new List<Int64>();
                            break;
                        case DynType.DateTime:
                            _paramsValues[dynParameter.Name] = new List<DateTime>();
                            break;
                        default:
                            break;
                    }

                    break;

                case CollectionType.Map:
                    _paramsValues[dynParameter.Name] = new Dictionary<string, object>();
                    break;
                case CollectionType.Set:
                    throw new ApplicationException("暂不支持Set集合类型");
            }
        }


        public object GetParameterValue(string paramName)
        {
            if (!_dynMethod.ContainsParameter(paramName))
                throw new ApplicationException(string.Format("方法定义【{0}】中不包含名为【{1}】的参数", _dynMethod.Name, paramName));

            object value = null;
            if (_paramsValues.TryGetValue(paramName, out value))
                return value;
            else
                throw new ApplicationException(string.Format("方法实例【{0}】中不包含名为【{1}】的参数值", _dynMethod.Name, paramName));

        }

        public void SetParameterValue(string paramName, object paramValue)
        {
            if (_dynMethod.ContainsParameter(paramName))
            {
                DynParameter dynParameter = _dynMethod.GetParameter(paramName);

                CollectionType collectionType = dynParameter.CollectionType;
                DynType dynType = dynParameter.DynType;
                string structName = dynParameter.StructName;

                if (paramValue != null)
                {
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
                                    isCorrectType = paramValue is Boolean;
                                    break;
                                case DynType.Byte:
                                    isCorrectType = paramValue is Byte;
                                    break;
                                case DynType.Double:
                                    isCorrectType = paramValue is Double || paramValue is Single || paramValue is Int64 || paramValue is Int32 || paramValue is UInt32 || paramValue is UInt16 || paramValue is Int16;
                                    break;
                                case DynType.Decimal:
                                    isCorrectType = paramValue is Decimal || paramValue is Double || paramValue is Single || paramValue is Int64 || paramValue is Int32 || paramValue is UInt32 || paramValue is UInt16 || paramValue is Int16;
                                    break;
                                case DynType.I16:
                                    isCorrectType = paramValue is Int16;
                                    break;
                                case DynType.I32:
                                    isCorrectType = paramValue is Int32 || paramValue is UInt16 || paramValue is Int16;
                                    break;
                                case DynType.I64:
                                    isCorrectType = paramValue is Int64 || paramValue is Int32 || paramValue is UInt32 || paramValue is UInt16 || paramValue is Int16;
                                    break;
                                case DynType.String:
                                    isCorrectType = paramValue is String;
                                    break;
                                case DynType.DateTime:
                                    isCorrectType = paramValue is DateTime;
                                    break;
                                case DynType.Struct:
                                    isCorrectType = paramValue is DynObject;
                                    break;
                                case DynType.Binary:
                                    isCorrectType = paramValue is byte[];
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
                                    isCorrectType = paramValue is List<Boolean>;
                                    break;
                                case DynType.Byte:
                                    isCorrectType = paramValue is List<Byte>;
                                    break;
                                case DynType.Double:
                                    isCorrectType = paramValue is List<Double>;
                                    break;
                                case DynType.Decimal:
                                    isCorrectType = paramValue is List<Decimal>;
                                    break;
                                case DynType.I16:
                                    isCorrectType = paramValue is List<Int16>;
                                    break;
                                case DynType.I32:
                                    isCorrectType = paramValue is List<Int32>;
                                    break;
                                case DynType.I64:
                                    isCorrectType = paramValue is List<Int64>;
                                    break;
                                case DynType.String:
                                    isCorrectType = paramValue is List<String>;
                                    break;
                                case DynType.DateTime:
                                    isCorrectType = paramValue is List<String>;
                                    break;
                                case DynType.Struct:
                                    isCorrectType = paramValue is List<DynObject>;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case CollectionType.Set:
                            break;
                        case CollectionType.Map:
                            isCorrectType = paramValue is Dictionary<string, object>;
                            break;
                        default:
                            break;
                    }
                    #endregion

                    if (isCorrectType)
                    {
                        _paramsValues[paramName] = paramValue;
                    }
                    else
                    {
                        string msg = string.Format("参数{0}应该是{1}:{2}类型，而把{3}类型赋给它", paramName, Enum.GetName(typeof(CollectionType), collectionType), Enum.GetName(typeof(DynType), dynType), paramValue.GetType().ToString());
                        throw new ApplicationException(msg);
                    }
                }
                else
                {
                    //赋默认值
                    //SetDefaultValue(dynParameter);
                }
            }
            else
            {
                throw new ApplicationException(string.Format("方法{0}中，不包含参数{1}", _dynMethod.Name, paramName));
            }
        }

        public object this[string paramName]
        {
            get { return GetParameterValue(paramName); }
            set { SetParameterValue(paramName, value); }
        }

        public object[] GetParameterValues()
        {
            return _paramsValues.Values.ToArray();
        }
    }
}
