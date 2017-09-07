using Rock.TemplateTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Templating
{
    /// <summary>模版执行错误异常</summary>
    public class TemplateExecutionException : ApplicationException
    {
        #region 构造
        /// <summary>初始化</summary>
        public TemplateExecutionException() { }

        /// <summary>初始化</summary>
        /// <param name="message"></param>
        public TemplateExecutionException(String message) : base(message) { }

        /// <summary>初始化</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public TemplateExecutionException(String format, params Object[] args) : base(Utility.F(format,args)) { }      

        /// <summary>初始化</summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public TemplateExecutionException(String message, Exception innerException) : base(message, innerException) { }

        /// <summary>初始化</summary>
        /// <param name="innerException"></param>
        public TemplateExecutionException(Exception innerException, String format, params Object[] args) : base(Utility.F(format, args), innerException) { }

        /// <summary>初始化</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected TemplateExecutionException(Exception innerException) : base((innerException != null ? innerException.Message : null), innerException) { }

          /// <summary>初始化</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected TemplateExecutionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion
    }
}
