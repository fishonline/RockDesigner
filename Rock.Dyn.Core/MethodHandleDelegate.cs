using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Dyn.Core
{
    /// <summary>
    /// 方法处理委托
    /// </summary>
    /// <param name="self"></param>
    /// <param name="methodName"></param>
    /// <param name="dicParams"></param>
    /// <returns></returns>
    public delegate object MethodHandleDelegate(object self, string methodName, Dictionary<string, Object> dicParams);
}
