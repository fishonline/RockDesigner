using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rock.Orm.Data
{
    /// <summary>
    /// Interface of all statement factory
    /// </summary>
    public interface IStatementFactory
    {
        #region Rock's original interface methods
        /// <summary>
        /// Creates the insert statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="includeColumns">The include columns.</param>
        /// <returns>The sql.</returns>
        string CreateInsertStatement(string tableName, params string[] includeColumns);

        /// <summary>
        /// Creates the update statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereStr">The where STR.</param>
        /// <param name="includeColumns">The include columns.</param>
        /// <returns>The sql.</returns>
        string CreateUpdateStatement(string tableName, string whereStr, params string[] includeColumns);

        /// <summary>
        /// Creates the delete statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereStr">The where STR.</param>
        /// <returns>The sql.</returns>
        string CreateDeleteStatement(string tableName, string whereStr);

        /// <summary>
        /// Creates the select statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereStr">The where STR.</param>
        /// <param name="orderByStr">The order by STR.</param>
        /// <param name="includeColumns">The include columns.</param>
        /// <returns>The sql.</returns>
        string CreateSelectStatement(string tableName, string whereStr, string orderByStr, params string[] includeColumns);
        #endregion 

        #region table create/alter/drop operation added 
        /// <summary>
        /// create a 'create table' statement
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="includedColumns"></param>
        /// <returns></returns>
        //string CreateCreateTableStatement(string tableName, params string[] includedColumns);

        /// <summary>
        /// create a 'drop table' statement
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        //string CreateDropTableStatement(string tableName);

        /// <summary>
        /// create a 'alter table' statement
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="addedColumns">添加的字段数组</param>
        /// <param name="droppedColumns">删除的字段数组</param>
        /// <param name="alteredColumns">修改的字段数组</param>
        /// <returns></returns>
        //string CreateAlterTableStatement(string tableName, params string [] addedColumns,params string [] droppedColumns,params string [] alteredColumns);
        #endregion
    }
}
