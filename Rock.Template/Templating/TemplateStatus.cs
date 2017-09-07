using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Templating
{
    /// <summary>模版引擎状态</summary>
    public enum TemplateStatus
    {
        /// <summary>准备</summary>
        Prepare = 0,

        /// <summary>分析处理</summary>
        Process,

        /// <summary>编译</summary>
        Compile,
    }
}
