using System;
using Rock.Orm.Common;
namespace Rock.Orm.Data
{
    interface IQuery
    {
        QuerySection GroupBy(Rock.Orm.Common.GroupByClip groupBy);
        QuerySection GroupBy(params Rock.Orm.Common.PropertyItem[] properties);
        QuerySection OrderBy(Rock.Orm.Common.OrderByClip orderBy);
        QuerySection Select(params Rock.Orm.Common.ExpressionClip[] properties);
        Rock.Orm.Common.EntityArrayList<EntityType> ToArrayList<EntityType>(int topItemCount) where EntityType : Rock.Orm.Common.Entity, new();
        Rock.Orm.Common.EntityArrayList<EntityType> ToArrayList<EntityType>(int topItemCount, int skipItemCount) where EntityType : Rock.Orm.Common.Entity, new();
        Rock.Orm.Common.EntityArrayList<EntityType> ToArrayList<EntityType>() where EntityType : Rock.Orm.Common.Entity, new();
        EntityType[] ToArray<EntityType>(int topItemCount) where EntityType : Rock.Orm.Common.Entity, new();
        EntityType[] ToArray<EntityType>(int topItemCount, int skipItemCount) where EntityType : Rock.Orm.Common.Entity, new();
        EntityType[] ToArray<EntityType>() where EntityType : Rock.Orm.Common.Entity, new();
        System.Data.IDataReader ToDataReader(int topItemCount);
        System.Data.IDataReader ToDataReader(int topItemCount, int skipItemCount);
        System.Data.IDataReader ToDataReader();
        System.Data.DataSet ToDataSet(int topItemCount);
        System.Data.DataSet ToDataSet(int topItemCount, int skipItemCount);
        System.Data.DataSet ToDataSet();
        object[] ToSinglePropertyArray();
        object[] ToSinglePropertyArray(int topItemCount);
        object[] ToSinglePropertyArray(int topItemCount, int skipItemCount);
        System.Data.Common.DbCommand ToDbCommand();
        System.Data.Common.DbCommand ToDbCommand(int topItemCount);
        System.Data.Common.DbCommand ToDbCommand(int topItemCount, int skipItemCount);
        EntityType ToFirst<EntityType>() where EntityType : Rock.Orm.Common.Entity, new();
        object ToScalar();
        QuerySection Where(WhereClip where);
    }
}
