using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Xml;

namespace Rock.Orm.Data.Sqlite
{
	/// <summary>
	/// <para>Represents a Access Server Database Provider.</para>
	/// </summary>
	/// <remarks> 
	/// <para>
	/// Internally uses Access Server .NET Managed Provider from Microsoft (System.Data.OleDb) to connect to the database.
	/// </para>  
	/// </remarks>
    public class SqliteDbProvider : DbProvider
    {
        #region Private Members

        private const char PARAMETER_TOKEN = '?';

        #endregion

        #region Public Members

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessDbProvider"/> class.
        /// </summary>
        /// <param name="connStr">The conn STR.</param>
        public SqliteDbProvider(string connStr)
            : base(connStr, System.Data.SQLite.SQLiteFactory.Instance)
        {
        }

        /// <summary>
        /// Discovers the params.
        /// </summary>
        /// <param name="sql">The sql.</param>
        /// <returns></returns>
        public override string[] DiscoverParams(string sql)
        {
            if (sql == null)
            {
                return null;
            }

            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(PARAMETER_TOKEN + @"([\w\d_]+)");
            System.Text.RegularExpressions.MatchCollection ms = r.Matches(sql);

            if (ms.Count == 0)
            {
                return null;
            }

            string[] paramNames = new string[ms.Count];
            for (int i = 0; i < ms.Count; i++)
            {
                paramNames[i] = ms[i].Value;
            }
            return paramNames;
        }

        /// <summary>
        /// Adjust Db Parameter value
        /// </summary>
        /// <param name="param"></param>
        [Obsolete]
        public override void AdjustParameter(DbParameter param)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, creates an <see cref="IPageSplit"/> for a SQL page splitable select query.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="selectStatement">The text of the basic select query for all rows.</param>
        /// <param name="keyColumn">The sigle main DEFAULT_KEY of the query.</param>
        /// <param name="paramValues">The param values of the query.</param>
        /// <returns>
        /// The <see cref="IPageSplit"/> for the SQL query.
        /// </returns>
        [Obsolete]
        public override IPageSplit CreatePageSplit(Database db, string selectStatement, string keyColumn, object[] paramValues)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the SQL statement factory.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public override IStatementFactory CreateStatementFactory()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the name of the parameter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override string BuildParameterName(string name)
        {
            string nameStr = name.Trim('[', ']');
            if (nameStr[0] != PARAMETER_TOKEN)
            {
                return nameStr.Insert(0, new string(PARAMETER_TOKEN, 1));
            }
            return nameStr;
        }

        /// <summary>
        /// Builds the name of the column.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override string BuildColumnName(string name)
        {
            if ((!name.StartsWith("[")) && (!name.EndsWith("]")))
            {
                return "[" + name + "]";
            }
            return name;
        }

        /// <summary>
        /// Gets the select last inserted row auto ID statement.
        /// </summary>
        /// <value>The select last inserted row auto ID statement.</value>
        public override string SelectLastInsertedRowAutoIDStatement
        {
            get 
            {
                return "SELECT last_insert_rowid()";
            }
        }

        /// <summary>
        /// Gets a value indicating whether [support AD o20 transaction].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [support AD o20 transaction]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportADO20Transaction
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the left token of table name or column name.
        /// </summary>
        /// <value>The left token.</value>
        public override string LeftToken
        {
            get { return "["; }
        }

        /// <summary>
        /// Gets the right token of table name or column name.
        /// </summary>
        /// <value>The right token.</value>
        public override string RightToken
        {
            get { return "]"; }
        }

        /// <summary>
        /// Gets the param prefix.
        /// </summary>
        /// <value>The param prefix.</value>
        public override string ParamPrefix
        {
            get { return PARAMETER_TOKEN.ToString(); }
        }

        public override Rock.Orm.Common.ISqlQueryFactory QueryFactory
        {
            get { return new Rock.Orm.Common.Sqlite.SqliteQueryFactory(); }
        }
        #endregion
    }
}