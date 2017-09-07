using System.Collections.Generic;
using System.Linq;
using Rock.Dyn.Core;

namespace Rock.Dyn.Core
{
    /// <summary>
    /// 摘要:
    ///     表示接口名和实现名的静态映射类
    /// </summary>
    public static class InterfaceImplementMap
    {
        /// <summary>
        /// 摘要:
        ///     表示接口名和实现名的键值对集合。
        /// 类型参数:
        ///     Key string:
        ///         接口名称。
        ///     Value string:
        ///         实现类的名称。
        /// </summary>
        public static Dictionary<string, string> InterfaceAndImplementMap = new Dictionary<string, string>();

        static InterfaceImplementMap()
        {
            InterfaceAndImplementMap.Clear();
            //InterfaceAndImplementMap.Add("IClassDesignService", "ClassDesignService");
        }

        /// <summary>
        /// 初始化表示接口名和实现名之间的映射
        /// </summary>
        public static void Init()
        {
            InterfaceAndImplementMap.Clear();
            //InterfaceAndImplementMap.Add("IClassDesignService", "ClassDesignService");

            List<DynClass> allDynClass = DynTypeManager.DynClasses.Values.ToList() ;

            List<DynInterface> allInterfaces = DynTypeManager.DynInterfaces;

            foreach (DynInterface dynInterface in allInterfaces)
            {
                string interfaceName = dynInterface.Name;
                bool isFind = false;

                foreach (DynClass dynClass in allDynClass)
                {
                    List<string> interfaceNames = dynClass.InterfaceNames;

                    if (interfaceNames != null && interfaceNames.Count > 0)
                    {
                        foreach (string interfaceNameString in interfaceNames)
                        {
                            if (interfaceNameString == interfaceName)
                            {
                                //这样也是在字典中添加新项
                                InterfaceAndImplementMap[interfaceName] = dynClass.Name;
                                isFind = true;
                                break;
                            }
                        }
                    }

                    if (isFind)
                    {
                        break;
                    }
                }
            }
        }
    }
}
