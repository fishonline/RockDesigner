using System;
using System.Collections.Generic;
using ExpressionEvaluation;
using System.Collections;

namespace Rock.Dyn.Core
{
    public sealed class DynStringResolver
    {
        public static Dictionary<string, string> MethodDict = null;
        private static RockEval eval;

        /// <summary>
        /// Initializes the <see cref="DynTypeManager"/> class.
        /// </summary>
        static DynStringResolver()
        {
        }

        public static object GetTrueType(object value, DynType dynType)
        {
            switch (dynType)
            {
                case DynType.Void:
                    return null;
                case DynType.Bool:
                    return Convert.ToBoolean(value);
                case DynType.Byte:
                    return Convert.ToByte(value);
                case DynType.Double:
                    return Convert.ToDouble(value);
                case DynType.I16:
                    return Convert.ToInt16(value);
                case DynType.I32:
                    return Convert.ToInt32(value);
                case DynType.I64:
                    return Convert.ToInt64(value);
                case DynType.String:
                    return Convert.ToString(value);
                case DynType.Struct:
                    return value;
                case DynType.DateTime:
                    return Convert.ToDateTime(value);
                case DynType.Binary:
                    return value;
                case DynType.Decimal:
                    return Convert.ToDecimal(value);
                default:
                    return null;
            }
        }


        public static object Resolve(string str,Dictionary<string,object> context)
        {
            if (eval == null)
            {
                eval = new RockEval(new Dictionary<string, object>()); 
            }

            eval.Context = context;
            return eval.Resolve(str);
        }

        public static object Resolve(string str)
        {
            return Resolve(str,new Dictionary<string,object>());
        }

        //private static object GetMethodValue(string name)
        //{
        //    string[] methodConfig = name.Split('(');
        //    if (methodConfig.Length >= 1)
        //    {
        //        string methodName = methodConfig[0];
        //        string methodBoby;
        //        if (!MethodDict.TryGetValue(methodName, out methodBoby))
        //        {
        //            throw new ApplicationException("不包含此方法 请检查定义:" + methodName);
        //        }
        //        string className = MethodDict[methodName];

        //        Dictionary<string, object> dict = new Dictionary<string, object>();
        //        DynMethod method = DynTypeManager.GetClass(className).GetMethod(methodName);

        //        List<string> getParamsStrList = GetMethodParamsString(name);
        //        DynParameter[] parameters = method.GetParameters();

        //        for (int i = 0; i < parameters.Length; i++)
        //        {
        //            string valuestr = getParamsStrList.Count > i ? getParamsStrList[i] : null;

        //            if (string.IsNullOrEmpty(valuestr))
        //            {
        //                dict.Add(parameters[i].Name, null);
        //            }
        //            else if (IsMethodStr(valuestr))
        //            {
        //                dict.Add(parameters[i].Name, GetMethodValue(valuestr));
        //            }
        //            else
        //            {
        //                throw new ApplicationException("无法解析一下字符串:" + name);
        //            }

        //        }

        //        object baseValue = DynTypeManager.MethodHandler(null, className + "_" + methodName, dict);
        //        return baseValue;

        //    }
        //    else
        //    {
        //        throw new ApplicationException("解析的字符不符合逻辑：" + name);
        //    }
        //}

        //private static bool IsMethodStr(string valuestr)
        //{
        //    throw new NotImplementedException();
        //}

        //private static List<string> GetMethodParamsString(string name)
        //{
        //    throw new NotImplementedException();
        //}


    }
}
