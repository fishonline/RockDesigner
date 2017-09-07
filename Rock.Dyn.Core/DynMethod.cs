using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;

namespace Rock.Dyn.Core
{
    public class DynMethod
    {
        private string _name;
        private string _body;
        private string _displayName;
        private bool _isAsync = false;
        private ScriptType _scriptType;
        private DynParameter _result = new DynParameter(0, "result", DynType.Void);
        private string _className;
        private string _description;

        private Dictionary<string, DynParameter> _parameters = new Dictionary<string, DynParameter>();
        private Dictionary<short, DynParameter> _params = new Dictionary<short, DynParameter>();
        private Dictionary<string, DynObject> _attributes = new Dictionary<string, DynObject>();


        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// 显示名
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        /// <summary>
        /// 方法体
        /// </summary>
        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }

        /// <summary>
        /// 方法体的脚本类型
        /// </summary>
        public ScriptType ScriptType
        {
            get { return _scriptType; }
            set { _scriptType = value; }
        }      

        /// <summary>
        /// 是否是异步，也即在执行之前返回结果
        /// </summary>
        public bool IsAsync
        {
            get { return _isAsync; }
            set { _isAsync = value; }
        }

        /// <summary>
        /// 返回结果
        /// </summary>
        public DynParameter Result
        {
            get { return _result; }
        }
        /// <summary>
        /// 方法所在的类名
        /// </summary>
        public string ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        /// <summary>
        /// 方法的全名
        /// </summary>
        public string FullName
        {
            get { return _className + "_" + _name; }
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
        /// 获取参数的字典集合
        /// </summary>
        public Dictionary<string, DynParameter> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        public DynMethod(string name)
        {
            _name = name;
        }
      

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="dynParameter">参数</param>
        public void AddParameter(DynParameter dynParameter)
        {
            if (dynParameter != null)
            {
                if (!_parameters.ContainsKey(dynParameter.Name))
                {
                    _parameters.Add(dynParameter.Name, dynParameter);
                    _params.Add(dynParameter.ID, dynParameter);
                }
                else
                {
                    throw new ApplicationException(string.Format("已经存在参数名为{0}的参数", dynParameter.Name));
                }
            }
            else
            {
                throw new ApplicationException("参数不能为null");
            }
        }

        /// <summary>
        /// 根据参数名移除参数
        /// </summary>
        /// <param name="parameterName">参数名</param>
        public void RemoveParameter(string parameterName)
        {
            if (!string.IsNullOrEmpty(parameterName))
            {
                if (_parameters.ContainsKey(parameterName))
                {
                    DynParameter dynParameter = _parameters[parameterName];
                    _parameters.Remove(parameterName);
                    _params.Remove(dynParameter.ID);
                }
                else
                {
                    throw new ApplicationException(string.Format("不存在存在参数名为{0}的参数", parameterName));
                }
            }
            else
            {
                throw new ApplicationException("参数名不能为空或null");
            }
        }

        /// <summary>
        /// 根据参数ID移除参数
        /// </summary>
        /// <param name="parameterID">参数ID</param>
        public void RemoveParameter(short parameterID)
        {
            if (_params.ContainsKey(parameterID))
            {
                DynParameter dynParameter = _params[parameterID];
                _parameters.Remove(dynParameter.Name);
                _params.Remove(parameterID);
            }
            else
            {
                throw new ApplicationException(string.Format("不存在存在参数ID为{0}的参数", parameterID));
            }
        }

        /// <summary>
        /// 根据参数名获取参数
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <returns>参数</returns>
        public DynParameter GetParameter(string parameterName)
        {
            if (!string.IsNullOrEmpty(parameterName))
            {
                DynParameter value = null;
                if (_parameters.TryGetValue(parameterName, out value))
                {
                    return value;
                }
                else
                {
                    throw new ApplicationException(string.Format("不存在存在参数名为{0}的参数", parameterName));
                }
            }
            else
            {
                throw new ApplicationException("参数名不能为空或null");
            }
        }

        /// <summary>
        /// 根据参数ID获取参数
        /// </summary>
        /// <param name="parameterID">参数ID</param>
        /// <returns>参数</returns>
        public DynParameter GetParameter(short parameterID)
        {
            DynParameter value = null;
            if (_params.TryGetValue(parameterID, out value))
            {
                return value;
            }
            else
            {
                throw new ApplicationException(string.Format("不存在存在参数ID为{0}的参数", parameterID));
            }
        }

        /// <summary>
        /// 是否包含参数
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <returns>true, false</returns>
        public bool ContainsParameter(string parameterName)
        {
            if (!string.IsNullOrEmpty(parameterName))
            {
                return _parameters.ContainsKey(parameterName);
            }
            else
            {
                throw new ApplicationException("参数名不能为空或null");
            }
        }

        /// <summary>
        /// 获取所有参数的名称集合
        /// </summary>
        /// <returns></returns>
        public string[] GetParameterNames()
        {
            return _parameters.Keys.ToArray();
        }

        /// <summary>
        /// 获取所有参数集合
        /// </summary>
        /// <returns></returns>
        public DynParameter[] GetParameters()
        {
            return _parameters.Values.ToArray();
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
                DynObject value = null;
                if (_attributes.TryGetValue(attributeName, out value))
                {
                    return value;
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
    }
}
