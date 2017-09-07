using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.TemplateTool
{
    public static class Utility
    {
        private static DefaultConvert _Convert = new DefaultConvert();
        public static String F(String value, params Object[] args)
        {
            if (String.IsNullOrEmpty(value)) return value;

            // 特殊处理时间格式化。这些年，无数项目实施因为时间格式问题让人发狂
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is DateTime)
                {
                    // 没有写格式化字符串的时间参数，一律转为标准时间字符串
                    if (value.Contains("{" + i + "}")) args[i] = ToFullString(((DateTime)args[i]));
                }
            }

            return String.Format(value, args);
        }

        public static String ToFullString(DateTime value) 
        { 
            return _Convert.ToFullString(value); 
        }

        public static Boolean IsNullOrWhiteSpace(String value)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!Char.IsWhiteSpace(value[i])) return false;
                }
            }
            return true;
        }

        public static Boolean EndsWithIgnoreCase(String value, params String[] strs)
        {
            if (String.IsNullOrEmpty(value)) return false;

            foreach (var item in strs)
            {
                if (value.EndsWith(item, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static String GetName(Type type, Boolean isfull = false)
        {
            if (type.IsNested) return GetName(type.DeclaringType, isfull) + "." + type.Name;

            if (!type.IsGenericType) return isfull ? type.FullName : type.Name;

            var sb = new StringBuilder();
            var typeDef = type.GetGenericTypeDefinition();
            var name = isfull ? typeDef.FullName : typeDef.Name;
            var p = name.IndexOf("`");
            if (p >= 0)
                sb.Append(name.Substring(0, p));
            else
                sb.Append(name);
            sb.Append("<");
            var ts = type.GetGenericArguments();
            for (int i = 0; i < ts.Length; i++)
            {
                if (i > 0) sb.Append(",");
                if (!ts[i].IsGenericParameter) sb.Append(GetName(ts[i], isfull));
            }
            sb.Append(">");
            return sb.ToString();
        }

        //public static TResult ChangeType<TResult>(Object value)
        //{
        //    if (value is TResult) return (TResult)value;

        //    return (TResult)ChangeType(value, typeof(TResult));
        //}
        //public static Object ChangeType(this Object value, Type conversionType) {
        //    return TypeX.ChangeType(value, conversionType);
        //}

        ///// <summary>类型转换</summary>
        ///// <param name="value">数值</param>
        ///// <param name="conversionType"></param>
        ///// <returns></returns>
        //public static Object ChangeType(object value, Type conversionType)
        //{
        //    Type vtype = null;
        //    if (value != null) vtype = value.GetType();
        //    //if (vtype == conversionType || conversionType.IsAssignableFrom(vtype)) return value;
        //    if (vtype == conversionType) return value;

        //    var cx = Create(conversionType);

        //    // 处理可空类型
        //    if (!cx.IsValueType && IsNullable(conversionType))
        //    {
        //        if (value == null) return null;

        //        conversionType = Nullable.GetUnderlyingType(conversionType);
        //    }

        //    if (cx.IsEnum)
        //    {
        //        if (vtype == _.String)
        //            return Enum.Parse(conversionType, (String)value, true);
        //        else
        //            return Enum.ToObject(conversionType, value);
        //    }

        //    // 字符串转为货币类型，处理一下
        //    if (vtype == _.String)
        //    {
        //        var str = (String)value;
        //        if (Type.GetTypeCode(conversionType) == TypeCode.Decimal)
        //        {
        //            value = str.TrimStart(new Char[] { '$', '￥' });
        //        }
        //        else if (typeof(Type).IsAssignableFrom(conversionType))
        //        {
        //            return GetType((String)value, true);
        //        }

        //        // 字符串转为简单整型，如果长度比较小，满足32位整型要求，则先转为32位再改变类型
        //        if (cx.IsInt && str.Length <= 10) return Convert.ChangeType(value.ToInt(), conversionType);
        //    }

        //    if (value != null)
        //    {
        //        // 尝试基础类型转换
        //        switch (Type.GetTypeCode(conversionType))
        //        {
        //            case TypeCode.Boolean:
        //                return value.ToBoolean();
        //            case TypeCode.DateTime:
        //                return value.ToDateTime();
        //            case TypeCode.Double:
        //                return value.ToDouble();
        //            case TypeCode.Int16:
        //                return (Int16)value.ToInt();
        //            case TypeCode.Int32:
        //                return value.ToInt();
        //            case TypeCode.UInt16:
        //                return (UInt16)value.ToInt();
        //            case TypeCode.UInt32:
        //                return (UInt32)value.ToInt();
        //            default:
        //                break;
        //        }

        //        if (value is IConvertible)
        //        {
        //            // 上海石头 发现这里导致Json序列化问题
        //            // http://www.newlifex.com/showtopic-282.aspx
        //            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        //            {
        //                var nullableConverter = new NullableConverter(conversionType);
        //                conversionType = nullableConverter.UnderlyingType;
        //            }
        //            value = Convert.ChangeType(value, conversionType);
        //        }
        //        //else if (conversionType.IsInterface)
        //        //    value = DuckTyping.Implement(value, conversionType);
        //    }
        //    else
        //    {
        //        // 如果原始值是null，要转为值类型，则new一个空白的返回
        //        if (cx.IsValueType) value = CreateInstance(conversionType);
        //    }

        //    if (conversionType.IsAssignableFrom(vtype)) return value;
        //    return value;
        //}

        //public static TypeX Create(Type type)
        //{
        //    if (type == null) throw new ArgumentNullException("type");

        //    return cache.GetItem(type, key => new TypeX(key));
        //}
    }

    /// <summary>默认转换</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DefaultConvert
    {
        /// <summary>转为整数</summary>
        /// <param name="value">待转换对象</param>
        /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
        /// <returns></returns>
        public virtual Int32 ToInt(Object value, Int32 defaultValue)
        {
            if (value == null) return defaultValue;

            // 特殊处理字符串，也是最常见的
            if (value is String)
            {
                var str = value as String;
                str = ToDBC(str).Trim();
                if (String.IsNullOrEmpty(str)) return defaultValue;

                var n = defaultValue;
                if (Int32.TryParse(str, out n)) return n;
                return defaultValue;
            }
            else if (value is Byte[])
            {
                var buf = (Byte[])value;
                if (buf == null || buf.Length < 1) return defaultValue;

                switch (buf.Length)
                {
                    case 1:
                        return buf[0];
                    case 2:
                        return BitConverter.ToInt16(buf, 0);
                    case 3:
                        return BitConverter.ToInt32(new Byte[] { buf[0], buf[1], buf[2], 0 }, 0);
                    case 4:
                        return BitConverter.ToInt32(buf, 0);
                    default:
                        break;
                }
            }

            //var tc = Type.GetTypeCode(value.GetType());
            //if (tc >= TypeCode.Char && tc <= TypeCode.Decimal) return Convert.ToInt32(value);

            try
            {
                return Convert.ToInt32(value);
            }
            catch { return defaultValue; }
        }

        /// <summary>转为浮点数</summary>
        /// <param name="value">待转换对象</param>
        /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
        /// <returns></returns>
        public virtual Double ToDouble(Object value, Double defaultValue)
        {
            if (value == null) return defaultValue;

            // 特殊处理字符串，也是最常见的
            if (value is String)
            {
                var str = value as String;
                str = ToDBC(str).Trim();
                if (String.IsNullOrEmpty(str)) return defaultValue;

                var n = defaultValue;
                if (Double.TryParse(str, out n)) return n;
                return defaultValue;
            }
            else if (value is Byte[])
            {
                var buf = (Byte[])value;
                if (buf == null || buf.Length < 1) return defaultValue;

                switch (buf.Length)
                {
                    case 1:
                        return buf[0];
                    case 2:
                        return BitConverter.ToInt16(buf, 0);
                    case 3:
                        return BitConverter.ToInt32(new Byte[] { buf[0], buf[1], buf[2], 0 }, 0);
                    case 4:
                        return BitConverter.ToInt32(buf, 0);
                    default:
                        // 凑够8字节
                        if (buf.Length < 8)
                        {
                            var bts = new Byte[8];
                            Buffer.BlockCopy(buf, 0, bts, 0, buf.Length);
                            buf = bts;
                        }
                        return BitConverter.ToDouble(buf, 0);
                }
            }

            try
            {
                return Convert.ToDouble(value);
            }
            catch { return defaultValue; }
        }

        //static readonly String[] trueStr = new String[] { "True", "Y", "Yes", "On" };
        //static readonly String[] falseStr = new String[] { "False", "N", "N", "Off" };

        /// <summary>转为布尔型。支持大小写True/False、0和非零</summary>
        /// <param name="value">待转换对象</param>
        /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
        /// <returns></returns>
        public virtual Boolean ToBoolean(Object value, Boolean defaultValue)
        {
            if (value == null) return defaultValue;

            // 特殊处理字符串，也是最常见的
            if (value is String)
            {
                var str = value as String;
                str = ToDBC(str).Trim();
                if (String.IsNullOrEmpty(str)) return defaultValue;

                var b = defaultValue;
                if (Boolean.TryParse(str, out b)) return b;

                if (String.Equals(str, Boolean.TrueString, StringComparison.OrdinalIgnoreCase)) return true;
                if (String.Equals(str, Boolean.FalseString, StringComparison.OrdinalIgnoreCase)) return false;

                // 特殊处理用数字0和1表示布尔型
                var n = 0;
                if (Int32.TryParse(str, out n)) return n > 0;

                return defaultValue;
            }

            try
            {
                return Convert.ToBoolean(value);
            }
            catch { return defaultValue; }
        }

        /// <summary>转为时间日期</summary>
        /// <param name="value">待转换对象</param>
        /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
        /// <returns></returns>
        public virtual DateTime ToDateTime(Object value, DateTime defaultValue)
        {
            if (value == null) return defaultValue;

            // 特殊处理字符串，也是最常见的
            if (value is String)
            {
                var str = value as String;
                str = ToDBC(str).Trim();
                if (String.IsNullOrEmpty(str)) return defaultValue;

                var n = defaultValue;
                if (DateTime.TryParse(str, out n)) return n;
                if (str.Contains("-") && DateTime.TryParseExact(str, "yyyy-M-d", null, DateTimeStyles.None, out n)) return n;
                if (str.Contains("/") && DateTime.TryParseExact(str, "yyyy/M/d", null, DateTimeStyles.None, out n)) return n;
                if (DateTime.TryParse(str, out n)) return n;
                return defaultValue;
            }

            try
            {
                return Convert.ToDateTime(value);
            }
            catch { return defaultValue; }
        }

        /// <summary>全角为半角</summary>
        /// <remarks>全角半角的关系是相差0xFEE0</remarks>
        /// <param name="str"></param>
        /// <returns></returns>
        String ToDBC(String str)
        {
            var ch = str.ToCharArray();
            for (int i = 0; i < ch.Length; i++)
            {
                // 全角空格
                if (ch[i] == 0x3000)
                    ch[i] = (char)0x20;
                else if (ch[i] > 0xFF00 && ch[i] < 0xFF5F)
                    ch[i] = (char)(ch[i] - 0xFEE0);
            }
            return new string(ch);
        }

        /// <summary>时间日期转为yyyy-MM-dd HH:mm:ss完整字符串</summary>
        /// <param name="value">待转换对象</param>
        /// <param name="emptyValue">字符串空值时显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
        /// <returns></returns>
        public virtual String ToFullString(DateTime value, String emptyValue = null)
        {
            if (emptyValue != null && value <= DateTime.MinValue) return emptyValue;

            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>时间日期转为指定格式字符串</summary>
        /// <param name="value">待转换对象</param>
        /// <param name="format">格式化字符串</param>
        /// <param name="emptyValue">字符串空值时显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
        /// <returns></returns>
        public virtual String ToString(DateTime value, String format, String emptyValue)
        {
            if (emptyValue != null && value <= DateTime.MinValue) return emptyValue;

            return value.ToString(format ?? "yyyy-MM-dd HH:mm:ss");
        }
    }
}
