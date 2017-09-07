using ExpressionEvaluation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Dyn.Core
{
    public class RockEval
    {
        private ExpressionEval eval = new ExpressionEval();
        public Dictionary<string, object> Context;

        public RockEval(Dictionary<string, object> context)
        {
            eval.AdditionalFunctionEventHandler += new AdditionalFunctionEventHandler(eval_AdditionalFunctionEventHandler);
            Context = context;
        }

        private void eval_AdditionalFunctionEventHandler(object sender, AdditionalFunctionEventArgs e)
        {
            object[] parameters = e.GetParameters();
            object value;

            //检查是否在默认的函数库中
            switch (e.Name)
            {
                case "GetProperty":
                    break;
                case "Get":
                    if (Context == null)
                    {
                        throw new ApplicationException("执行函数失败 上下文为空");
                    }

                    if (Context == null || parameters == null || parameters.Length < 1 || string.IsNullOrEmpty(parameters[0] as string))
                    {
                        throw new ApplicationException("执行函数失败 入参输入为空");
                    }
                    if (!Context.TryGetValue(parameters[0] as string, out value))
                    {
                        throw new ApplicationException("执行函数失败 获取上下文时 没有键为" + parameters[0] + "的值");
                    }
                    e.ReturnValue = value;
                    return;
                default:
                    string className = null;
                    //检查是否在当前应用的函数库中
                    if (DynStringResolver.MethodDict.TryGetValue(e.Name, out className))
                    {
                        string methodName = e.Name;

                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        DynClass function = DynTypeManager.GetFunction(className);

                        if (function == null)
                        {
                            throw new ApplicationException("执行函数失败 没有主键为" + className + "的函数类 请查看初始化");
                        }
                        DynMethod method = function.GetMethod(methodName);

                        DynParameter[] dynParameters = method.GetParameters();

                        for (int i = 0; i < dynParameters.Length; i++)
                        {
                            value = parameters.Length > i ? parameters[i] : null;

                            switch (dynParameters[i].CollectionType)
                            {
                                case CollectionType.None:
                                    value = DynStringResolver.GetTrueType(value, dynParameters[i].DynType);
                                    break;
                                case CollectionType.List:
                                    IList valueList = value as IList;
                                    for (int j = 0; j < valueList.Count; j++)
                                    {
                                        valueList[j] = DynStringResolver.GetTrueType(valueList[j], dynParameters[i].DynType);
                                    }
                                    break;
                                case CollectionType.Set:
                                case CollectionType.Map:
                                    break;
                                default:
                                    break;
                            }

                            dict.Add(dynParameters[i].Name, value);
                        }

                        e.ReturnValue = DynTypeManager.MethodHandler(null, className + "_" + methodName, dict);
                    }
                    else
                    {
                        throw new ApplicationException("无此函数:" + e.Name);
                    }
                    break;
            }


        }

        public object Resolve(string str)
        {
            if (DynStringResolver.MethodDict == null)
            {
                throw new ApplicationException("字符串解析器未能成功初始化 请检查");
            }

            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            object result = null;

            try
            {
                eval.Expression = str;
                result = eval.Evaluate();
            }
            catch (Exception ex)
            {
                ApplicationException newEx = new ApplicationException("解析函数失败", ex);
                throw newEx;
            }

            return result;
        }

        public object Resolve(string str, Dictionary<string, object> conntext)
        {
            Context = conntext;
            return Resolve(str);
        }
    }
}
