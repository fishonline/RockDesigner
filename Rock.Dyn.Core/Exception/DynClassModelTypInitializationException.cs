using System;

namespace Rock.Dyn.Core
{
    public class DynClassModelTypInitializationException : ApplicationException
    {
        public DynClassModelTypInitializationException(DynClass dynClass, ClassMainType[] classMainType)
        {

            string types = null;
            string message = string.Format("{0}类{1} 必须将属性ClassMainType设定为{2}之一值", dynClass.GetType().Name, dynClass.Name, types);
        }

    }
}
