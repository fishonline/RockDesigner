using Rock.TemplateTool;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Templating
{
    [Serializable]
    class TemplateException : ApplicationException
    {
        private Block _Block;
        /// <summary>代码块</summary>
        internal Block Block
        {
            get { return _Block; }
            private set { _Block = value; }
        }

        private CompilerError _Error;
        /// <summary>编译器错误</summary>
        public CompilerError Error
        {
            get
            {
                if (_Error == null && Block != null)
                {
                    _Error = new CompilerError(Block.Name, Block.StartLine, Block.StartColumn, null, Message);
                    _Error.IsWarning = false;
                }
                return _Error;
            }
            internal set { _Error = value; }
        }

          #region 构造
        /// <summary>初始化</summary>
        public TemplateException() { }

        /// <summary>初始化</summary>
        /// <param name="message"></param>
        public TemplateException(String message){ }

        /// <summary>初始化</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public TemplateException(String format, params Object[] args) : base(Utility.F(format, args)) { }     

        /// <summary>初始化</summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public TemplateException(String message, Exception innerException) : base(message, innerException) { }

        /// <summary>初始化</summary>
        /// <param name="innerException"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public TemplateException(Exception innerException, String format, params Object[] args) : base(Utility.F(format, args), innerException) { }

        /// <summary>初始化</summary>
        /// <param name="innerException"></param>
        public TemplateException(Exception innerException) : base((innerException != null ? innerException.Message : null), innerException) { }

        /// <summary>初始化</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected TemplateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        internal TemplateException(Block block, String message)           
        {
            Block = block;
        }
        #endregion
    }   

    /// <summary>异常事件参数</summary>
    public class ExceptionEventArgs : CancelEventArgs
    {
        private String _Action;
        /// <summary>发生异常时进行的动作</summary>
        public String Action { get { return _Action; } set { _Action = value; } }

        private Exception _Exception;
        /// <summary>异常</summary>
        public Exception Exception { get { return _Exception; } set { _Exception = value; } }
    }

    /// <summary>异常助手</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ExceptionHelper
    {
        /// <summary>是否对象已被释放异常</summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Boolean IsDisposed(this Exception ex) { return ex is ObjectDisposedException; }
    }
}
