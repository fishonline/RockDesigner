using System.Collections.Generic;
using System.Linq;
using System;

namespace Rock.Dyn.Core
{
    public class DynInterface
    {      
        private string _name;
        private string _nameSpace;
        private ClassMainType _classMainType = ClassMainType.Control;
        private Dictionary<string, DynMethod> _methods = new Dictionary<string, DynMethod>();
        /// <summary>
        /// 接口名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
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
        ///  类的类型
        /// </summary>
        public ClassMainType ClassMainType
        {
            get { return _classMainType; }
            set
            {
                if (value != ClassMainType.Interface)
                {
                    throw new ApplicationException("接口必须是接口类型!");
                }
                _classMainType = value;
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        public DynInterface(string name)
        {
            _name = name;
        }     

        /// <summary>
        /// 当前接口的所有方法的字典集合
        /// </summary>
        public Dictionary<string, DynMethod> Methods
        {
            get { return _methods; }
            set { _methods = value; }
        }

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
                if (_methods.ContainsKey(methodName))
                {
                    return _methods[methodName];
                }
                else
                {
                    throw new ApplicationException(string.Format("当前接口{0}不存在方法名为{1}的方法", _name, methodName));
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
                if (!_methods.ContainsKey(method.Name))
                {
                    _methods.Add(method.Name, method);
                    method.ClassName = _name;
                }
                else
                {
                    throw new ApplicationException(string.Format("当前接口{0}已经存在名为{1}的方法", _name, method.Name));
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
                    throw new ApplicationException(string.Format("当前接口{0}不存在方法名为{1}的方法", _name, methodName));
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
    }
}
