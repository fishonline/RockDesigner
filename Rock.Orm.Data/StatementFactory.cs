using System;
using System.Collections.Generic;
using System.Text;

namespace Rock.Orm.Data
{
    /// <summary>
    /// Base statement factory.
    /// </summary>
    public abstract class StatementFactory : IStatementFactory
    {
        #region Rock's IStatementFactory Members

        /// <summary>
        /// Creates the insert statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="includeColumns">The include columns.</param>
        /// <returns>The sql.</returns>
        public abstract string CreateInsertStatement(string tableName, params string[] includeColumns);

        /// <summary>
        /// Creates the update statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereStr">The where STR.</param>
        /// <param name="includeColumns">The include columns.</param>
        /// <returns>The sql.</returns>
        public abstract string CreateUpdateStatement(string tableName, string whereStr, params string[] includeColumns);

        /// <summary>
        /// Creates the delete statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereStr">The where STR.</param>
        /// <returns>The sql.</returns>
        public abstract string CreateDeleteStatement(string tableName, string whereStr);

        /// <summary>
        /// Creates the select statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereStr">The where STR.</param>
        /// <param name="orderByStr">The order by STR.</param>
        /// <param name="includeColumns">The include columns.</param>
        /// <returns>The sql.</returns>
        public abstract string CreateSelectStatement(string tableName, string whereStr, string orderByStr, params string[] includeColumns);

        #endregion

        //#region table's create/drop/alter operations
        ///// <summary>
        ///// create a 'create table' statement
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <param name="includedColumns"></param>
        ///// <returns></returns>
        //public abstract string CreateCreateTableStatement(string tableName, params string[] includedColumns);

        ///// <summary>
        ///// create a 'drop table' statement
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <returns></returns>
        //public abstract string CreateDropTableStatement(string tableName);

        ///// <summary>
        ///// create a 'alter table' statement
        ///// </summary>
        ///// <param name="tableName">修改的表名</param>
        ///// <param name="addedColumns">增加的字段组</param>
        ///// <param name="droppedColumns">删除的字段组</param>
        ///// <param name="alteredColumns">修改的字段组</param>
        ///// <returns></returns>
        //public abstract string CreateAlterTableStatement(string tableName, string[] addedColumns, string[] droppedColumns, string[] alteredColumns);
        //#endregion 
    }
}
