﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NewLife;
using NewLife.Reflection;

namespace XTemplate.Templating
{
    /// <summary>模版基类，所有模版继承自该类</summary>
    /// <remarks>模版的原理其实就是生成一个继承自该类的模版类，并重载Render方法</remarks>
    [Serializable]
    public abstract class TemplateBase : DisposeBase
    {
        #region 构造和释放
        /// <summary>释放</summary>
        /// <param name="disposing"></param>
        protected override void OnDispose(bool disposing)
        {
            base.OnDispose(disposing);

            if (_Output != null) _Output = null;
        }
        #endregion

        #region 缩进
        private String _CurrentIndent = "";
        /// <summary>当前缩进</summary>
        public String CurrentIndent
        {
            get { return _CurrentIndent; }
        }

        private List<Int32> _indentLengths;
        /// <summary>缩进长度集合</summary>
        private List<Int32> indentLengths
        {
            get
            {
                if (_indentLengths == null) _indentLengths = new List<Int32>();

                return _indentLengths;
            }
        }

        /// <summary>清除缩进</summary>
        public void ClearIndent()
        {
            indentLengths.Clear();
            _CurrentIndent = "";
        }

        /// <summary>弹出缩进</summary>
        /// <returns></returns>
        public String RemoveIndent()
        {
            String str = "";
            if (indentLengths.Count > 0)
            {
                Int32 num = indentLengths[indentLengths.Count - 1];
                indentLengths.RemoveAt(indentLengths.Count - 1);
                if (num > 0)
                {
                    str = _CurrentIndent.Substring(_CurrentIndent.Length - num);
                    _CurrentIndent = _CurrentIndent.Remove(_CurrentIndent.Length - num);
                }
            }
            return str;
        }

        /// <summary>压入缩进</summary>
        /// <param name="indent"></param>
        public void AddIndent(String indent)
        {
            if (indent == null) throw new ArgumentNullException("indent");

            _CurrentIndent = _CurrentIndent + indent;
            indentLengths.Add(indent.Length);
        }
        #endregion

        #region 输出
        private Boolean endsWithNewline;
        /// <summary>写入文本</summary>
        /// <param name="str"></param>
        public void Write(String str)
        {
            if (String.IsNullOrEmpty(str)) return;

            if ((Output.Length == 0) || endsWithNewline)
            {
                Output.Append(_CurrentIndent);
                endsWithNewline = false;
            }
            if (str.EndsWith(Environment.NewLine, StringComparison.CurrentCulture)) endsWithNewline = true;
            if (_CurrentIndent.Length == 0)
            {
                Output.Append(str);
            }
            else
            {
                str = str.Replace(Environment.NewLine, Environment.NewLine + _CurrentIndent);
                if (endsWithNewline)
                    Output.Append(str, 0, str.Length - _CurrentIndent.Length);
                else
                    Output.Append(str);
            }
        }

        /// <summary>写入文本</summary>
        /// <param name="obj"></param>
        public void Write(Object obj)
        {
            if (obj == null) return;
            Write(obj.ToString());
        }

        /// <summary>写入文本</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Write(String format, params Object[] args)
        {
            if (String.IsNullOrEmpty(format)) return;

            if (args != null && args.Length > 0)
                Write(String.Format(CultureInfo.CurrentCulture, format, args));
            else
                Write(format);
        }

        /// <summary>写入文本</summary>
        /// <param name="str"></param>
        public void WriteLine(String str)
        {
            Write(str);

            Output.AppendLine();
            endsWithNewline = true;
        }

        /// <summary>写入文本</summary>
        /// <param name="obj"></param>
        public void WriteLine(Object obj)
        {
            if (obj != null) Write(obj.ToString());

            Output.AppendLine();
            endsWithNewline = true;
        }

        /// <summary>写入行</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteLine(String format, params Object[] args)
        {
            if (!String.IsNullOrEmpty(format))
            {
                if (args != null && args.Length > 0)
                    Write(String.Format(CultureInfo.CurrentCulture, format, args));
                else
                    Write(format);
            }

            Output.AppendLine();
            endsWithNewline = true;
        }

        /// <summary>写入一个换行</summary>
        public void WriteLine()
        {
            Output.AppendLine();
            endsWithNewline = true;
        }
        #endregion

        #region 属性
        private Template _Template;
        /// <summary>模版引擎实例</summary>
        public Template Template { get { return _Template; } set { _Template = value; } }

        private TemplateItem _TemplateItem;
        /// <summary>模版项实例</summary>
        public TemplateItem TemplateItem { get { return _TemplateItem; } set { _TemplateItem = value; } }
        #endregion

        #region 生成
        /// <summary>初始化</summary>
        public virtual void Initialize() { }

        /// <summary>转换文本</summary>
        /// <returns></returns>
        public virtual String Render() { return Output.ToString(); }

        private StringBuilder _Output;
        /// <summary>输出</summary>
        protected StringBuilder Output
        {
            get
            {
                if (_Output == null) _Output = new StringBuilder();

                return _Output;
            }
            set { _Output = value; }
        }
        #endregion

        #region 数据属性
        private IDictionary<String, Object> _Data;
        /// <summary>数据</summary>
        public IDictionary<String, Object> Data
        {
            get { return _Data ?? (_Data = new Dictionary<String, Object>()); }
            set { _Data = value; }
        }

        /// <summary>获取数据，主要处理数据字典中不存在的元素</summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected Object GetData(String name)
        {
            Object obj = null;
            return Data.TryGetValue(name, out obj) ? obj : null;
        }

        /// <summary>获取数据，主要处理类型转换</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        protected T GetData<T>(String name)
        {
            Object obj = GetData(name);
            if (obj == null) return default(T);

            return (T)TypeX.ChangeType(obj, typeof(T));
        }
        #endregion

        #region 模版变量
        private IDictionary<String, Type> _Vars;
        /// <summary>模版变量集合</summary>
        public IDictionary<String, Type> Vars
        {
            get { return _Vars ?? (_Vars = new Dictionary<String, Type>()); }
            set { _Vars = value; }
        }
        #endregion
    }
}