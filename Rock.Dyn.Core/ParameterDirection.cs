using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Dyn.Core
{

    // 摘要:
    //     指定查询内的有关 System.Data.DataSet 的参数的类型。
    public enum ParameterDirection
    {
        // 摘要:
        //     参数是输入参数。
        Input = 1,
        //
        // 摘要:
        //     参数是输出参数。
        Output = 2,
        //
        // 摘要:
        //     参数既能输入，也能输出。
        InputOutput = 3,
        //
        // 摘要:
        //     参数表示诸如存储过程、内置函数或用户定义函数之类的操作的返回值。
        ReturnValue = 6,
    }
}
