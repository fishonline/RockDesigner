
namespace Rock.Dyn.Core
{
    public class DynParameter
    {
        private DynType _dynType;
        private ParameterDirection _direction;
        private bool _isNullable;
        private string _name;
        private object _value;
        private object _defaultValue;
        private CollectionType _collectionType;
        private string _structName;

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        /// <summary>
        /// 参数数据类型
        /// </summary>
        public DynType DynType
        {
            get { return _dynType; }
            set { _dynType = value; }
        }

        /// <summary>
        /// 集合类型，包含None,List,Set,Map
        /// </summary>
        public CollectionType CollectionType
        {
            get { return _collectionType; }
            set { _collectionType = value; }
        }

        /// <summary>
        ///  当DynType为Struct时，这个名称有意义
        /// </summary>
        public string StructName
        {
            get { return _structName; }
            set { _structName = value; }
        }

        /// <summary>
        /// 输入输出类型
        /// </summary>
        public ParameterDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        /// <summary>
        /// 参数名
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// 参数值
        /// </summary>
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private short _id;

        /// <summary>
        /// 字段顺序，从0开始计数
        /// </summary>
        public short ID
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">属性名</param>
        public DynParameter(string name)
        {
            _id = 0;
            _name = name;
            _direction = ParameterDirection.Input;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">参数序号，从0开始</param>
        /// <param name="name">参数名</param>
        /// <param name="parameterType">参数的基本数据类型</param>
        public DynParameter(short id, string name, DynType parameterType)
        {
            _id = id;
            _name = name;
            _collectionType = CollectionType.None;
            _dynType = parameterType;
            _structName = "";

            _direction = ParameterDirection.Input;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">参数序号，从0开始</param>
        /// <param name="name">参数名</param>
        /// <param name="collectionType">集合类型，是CollectionType的枚举类型：None,List,Set,Map等</param>
        /// <param name="parameterType">参数的基本数据类型</param>
        /// <param name="structName">当参数的类型为Struct时，此名称有意义</param>
        public DynParameter(short id, string name, CollectionType collectionType, DynType parameterType, string structName)
        {
            _id = id;
            _name = name;
            _collectionType = collectionType;
            _dynType = parameterType;
            _structName = structName;

            _direction = ParameterDirection.Input;
        }
    }
}
