using System;
using System.Text;
using System.Reflection;

namespace Rock.Orm.Data.SqlServer
{
    /// <summary>
    /// Common Factory to create SQL statements for SqlServer and MS Access
    /// </summary>
    public class SqlStatementFactory : StatementFactory
    {
        #region Private Members

        private const char PARAMETER_TOKEN = '@';

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SqlStatementFactory"/> class.
        /// </summary>
        public SqlStatementFactory()
        {
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Creates the insert statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="includeColumns">The include columns.</param>
        /// <returns>The sql.</returns>
        public override string CreateInsertStatement(string tableName, params string[] includeColumns)
        {
            if (includeColumns.Length == 0)
            {
                return null;
            }

            string insertStatement = string.Empty;
            StringBuilder columnList = new StringBuilder();
            StringBuilder valueList = new StringBuilder();

            foreach (string item in includeColumns)
            {
                columnList.Append(string.Format("[{0}], ", item.Trim('[', ']')));
                valueList.Append(string.Format(PARAMETER_TOKEN + "{0}, ", item.Trim('[', ']')));
            }

            insertStatement = string.Format("INSERT INTO [{0}] ( {1} ) VALUES ( {2} )",
                tableName.Trim('[', ']'), columnList.ToString().TrimEnd(new char[] { ' ', ',' }), valueList.ToString().TrimEnd(new char[] { ' ', ',' }));

            return insertStatement;
        }

        /// <summary>
        /// Creates the update statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereStr">The sql STR.</param>
        /// <param name="includeColumns">The include columns.</param>
        /// <returns>The sql.</returns>
        public override string CreateUpdateStatement(string tableName, string whereStr, params string[] includeColumns)
        {
            if (includeColumns.Length == 0)
            {
                return null;
            }

            string updateStatement = string.Empty;
            StringBuilder setList = new StringBuilder();

            foreach (string item in includeColumns)
            {
                setList.Append(string.Format("[{0}] = {1}{0}, ", item.Trim('[', ']'), PARAMETER_TOKEN));
            }

            updateStatement = string.Format("UPDATE [{0}] SET {1} {2}",
                tableName.Trim('[', ']'), setList.ToString().TrimEnd(new char[] { ' ', ',' }), (whereStr != null ? "WHERE " + whereStr : string.Empty));

            return updateStatement;
        }

        /// <summary>
        /// Creates the delete statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereStr">The sql STR.</param>
        /// <returns>The sql.</returns>
        public override string CreateDeleteStatement(string tableName, string whereStr)
        {
            return string.Format("DELETE FROM [{0}] {1}", tableName.Trim('[', ']'), (whereStr != null ? "WHERE " + whereStr : string.Empty));
        }

        /// <summary>
        /// Creates the select statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereStr">The sql STR.</param>
        /// <param name="orderByStr">The order by STR.</param>
        /// <param name="includeColumns">The include columns.</param>
        /// <returns>The sql.</returns>
        public override string CreateSelectStatement(string tableName, string whereStr, string orderByStr, params string[] includeColumns)
        {
            if (includeColumns == null || includeColumns.Length == 0)
            {
                return null;
            }

            StringBuilder selectStatement = new StringBuilder();

            selectStatement.Append("SELECT ");

            int i = 1;
            foreach (string item in includeColumns)
            {
                if (item[0] == '*')
                {
                    selectStatement.Append('*');
                }
                else if (item[0] == '[')
                {
                    selectStatement.Append(item);
                }
                else
                {
                    selectStatement.Append('[');
                    selectStatement.Append(item);
                    selectStatement.Append(']');
                }
                if (i != includeColumns.Length)
                {
                    selectStatement.Append(',');
                }

                i++;
            }

            selectStatement.Append(" FROM ");
            if (tableName != null && tableName.Length > 0 && tableName[0] == '[')
            {
                selectStatement.Append(tableName);
            }
            else
            {
                selectStatement.Append('[');
                selectStatement.Append(tableName);
                selectStatement.Append(']');
            }
            if (!string.IsNullOrEmpty(whereStr))
            {
                selectStatement.Append(" WHERE ");
                selectStatement.Append(whereStr);
            }
            if (!string.IsNullOrEmpty(orderByStr))
            {
                selectStatement.Append(" ORDER BY ");
                selectStatement.Append(orderByStr);
            }

            return selectStatement.ToString();
        }

        ///// <summary>
        ///// create a 'create table' statement
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <param name="includedColumns"></param>
        ///// <returns></returns>
        //public override string CreateCreateTableStatement(string tableName, params string[] includedColumns)
        //{
        //    //under construction now ...
        //    if (includedColumns.Length == 0)
        //    {
        //        return null;
        //    }

        //    string createTableStatement = string.Empty;
        //    StringBuilder colsList = new StringBuilder();

        //    foreach (string item in includedColumns)
        //    {
        //        colsList.Append(string.Format("[{0}] = {1}{0}, ", item.Trim('[', ']'), PARAMETER_TOKEN));
        //    }

        //    createTableStatement = string.Format("UPDATE [{0}] SET {1} {2}",
        //        tableName.Trim('[', ']'), colsList.ToString().TrimEnd(new char[] { ' ', ',' }), (whereStr != null ? "WHERE " + whereStr : string.Empty));

        //    return createTableStatement;
        //}

        ///// <summary>
        ///// create a 'drop table' statement
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <returns></returns>
        //public override string CreateDropTableStatement(string tableName)
        //{
        //    return null;
        //}

        ///// <summary>
        ///// create a 'alter table' statement
        ///// </summary>
        ///// <param name="tableName">修改的表名</param>
        ///// <param name="addedColumns">增加的字段组</param>
        ///// <param name="droppedColumns">删除的字段组</param>
        ///// <param name="alteredColumns">修改的字段组</param>
        ///// <returns></returns>
        //public override string CreateAlterTableStatement(string tableName, string[] addedColumns, string[] droppedColumns, string[] alteredColumns)
        //{
        //    return null;
        //}

        #endregion
    }
}
