using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Rock.Orm.Common
{
    public interface IExpression
    {
        string Sql { get; set; }
        Dictionary<string, KeyValuePair<DbType, object>> Parameters { get; }
    }

    [Serializable]
    public class NameDuplicatedException : ApplicationException
    {
        public NameDuplicatedException() { }
        public NameDuplicatedException(string name) : base(name) { }
    }
}
