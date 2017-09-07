using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using CN.Rock.DesignByContract;

namespace Rock.Orm.Common.Sqlite
{
    public class SqliteQueryFactory : SqlQueryFactory
    {
        public SqliteQueryFactory() : base('[', ']', '?', '%', '_', SQLiteFactory.Instance)
        {
        }

        protected override void PrepareCommand(DbCommand cmd)
        {
            base.PrepareCommand(cmd);

            foreach (DbParameter p in cmd.Parameters)
            {
                cmd.CommandText = cmd.CommandText.Replace(p.ParameterName, "?");
                p.ParameterName = "?";

                if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue)
                {
                    continue;
                }

                object value = p.Value;
                if (value == DBNull.Value)
                {
                    continue;
                }
                Type type = value.GetType();
                SQLiteParameter sqliteParam = (SQLiteParameter)p;

                if (type == typeof(Guid))
                {
                    sqliteParam.DbType = DbType.String;
                    sqliteParam.Size = 32;
                    continue;
                }

                if ((p.DbType == DbType.Time || p.DbType == DbType.DateTime) && type == typeof(TimeSpan))
                {
                    sqliteParam.DbType = DbType.Double;
                    sqliteParam.Value = ((TimeSpan)value).TotalDays;
                    continue;
                }

                switch (p.DbType)
                {
                    case DbType.Time:
                        sqliteParam.DbType = DbType.DateTime;
                        p.Value = value.ToString();
                        break;
                    case DbType.Object:
                        sqliteParam.DbType = DbType.String;
                        p.Value = CN.Rock.Common.SerializationManager.Instance.Serialize(value);
                        break;
                }
            }

            //replace sqlite specific function names in cmd.CommandText
            cmd.CommandText = cmd.CommandText
                .Replace("SUBSTRING(", "substr(")
                .Replace("LEN(", "length(")
                .Replace("GETDATE()", "datetime('now')");

            if (cmd.CommandText.Contains("TRIM("))
            {
                throw new NotSupportedException("Sqlite provider does not support Trim() function.");
            }
            if (cmd.CommandText.Contains("CHARINDEX("))
            {
                throw new NotSupportedException("Sqlite provider does not support IndexOf() function.");
            }
            if (cmd.CommandText.Contains("REPLACE("))
            {
                throw new NotSupportedException("Sqlite provider does not support Replace() function.");
            }
            if (cmd.CommandText.Contains("DATEPART("))
            {
                throw new NotSupportedException("Sqlite provider does not support GetYear()/GetMonth()/GetDay() functions.");
            }
        }

        public override DbCommand CreateSelectRangeCommand(WhereClip where, string[] columns, int topCount, int skipCount, string identyColumn, bool identyColumnIsNumber)
        {
            Check.Require(((object)where) != null && where.From != null, "expr and expr.From could not be null!");
            Check.Require(columns != null && columns.Length > 0, "columns could not be null or empty!");
            Check.Require(topCount > 0, "topCount must > 0!");

            if (string.IsNullOrEmpty(where.OrderBy) && identyColumn != null)
            {
                where.SetOrderBy(new KeyValuePair<string,bool>[] { new KeyValuePair<string,bool>(identyColumn, false) });
            }

            if (topCount == int.MaxValue && skipCount == 0)
            {
                return CreateSelectCommand(where, columns);
            }
            else
            {
                DbCommand cmd = CreateSelectCommand(where, columns);
                if (skipCount == 0)
                {
                    cmd.CommandText += " LIMIT " + topCount;
                }
                else
                {
                    cmd.CommandText +=  " LIMIT " + skipCount;
                    cmd.CommandText +=  "," + topCount;
                }
                return cmd;
            }
        }
    }
}
