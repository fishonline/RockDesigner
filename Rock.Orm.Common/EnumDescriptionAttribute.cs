//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Reflection;

//namespace Rock.Orm.Common
//{
//    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false, Inherited=true)]
//    public class EnumDescriptionAttribute : Attribute
//    {
//        private string defaultDesc;

//        public string DefaultDescription
//        {
//            get
//            {
//                return defaultDesc;
//            }
//            set
//            {
//                defaultDesc = value;
//            }
//        }

//        public virtual string GetDescription(object enumValue)
//        {
//            Check.Require(enumValue != null, "enumValue could not be null.");

//            return DefaultDescription ?? enumValue.ToString();
//        }

//        public static string GetDescription(Type enumType, int enumIntValue)
//        {
//            System.Text.StringBuilder sb = new System.Text.StringBuilder();
//            Dictionary<int, string> descs = EnumDescriptionAttribute.GetDescriptions(enumType);
//            Dictionary<int, string>.Enumerator en = descs.GetEnumerator();
//            while (en.MoveNext())
//            {
//                if ((enumIntValue & en.Current.Key) == en.Current.Key)
//                {
//                    if (sb.Length == 0)
//                    {
//                        sb.Append(en.Current.Value);
//                    }
//                    else
//                    {
//                        sb.Append(',');
//                        sb.Append(en.Current.Value);
//                    }
//                }
//            }

//            return sb.ToString();
//        }

//        public static Dictionary<int, string> GetDescriptions(Type enumType)
//        {
//            Check.Require(enumType != null && enumType.IsEnum, "enumType must be an enum type.");

//            FieldInfo[] fields = enumType.GetFields();
//            Dictionary<int, string> descs = new Dictionary<int, string>();
//            for (int i = 1; i < fields.Length; ++i)
//            {
//                object fieldValue = Enum.Parse(enumType, fields[i].Name);
//                object[] attrs = fields[i].GetCustomAttributes(true);
//                bool findAttr = false;
//                foreach (object attr in attrs)
//                {
//                    if (typeof(EnumDescriptionAttribute).IsAssignableFrom(attr.GetType()))
//                    {
//                        descs.Add((int)fieldValue, ((EnumDescriptionAttribute)attr).GetDescription(fieldValue));
//                        findAttr = true;
//                        break;
//                    }
//                }
//                if (!findAttr)
//                {
//                    descs.Add((int)fieldValue, fieldValue.ToString());
//                }
//            }

//            return descs;
//        }
//    }
//}
