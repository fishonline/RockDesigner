using Rock.Orm.Common;
using Rock.Orm.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.DesignerModule.Service
{
    public class DataTransactionContext
    {
        public List<DynEntity> SavedDynEntities = new List<DynEntity>();
        public Dictionary<int, string> DeletedDynPropertyEntities = new Dictionary<int, string>();
        public Dictionary<int, string> DeletedObjTypes = new Dictionary<int, string>();
        public List<string> SqlCommandStrings = new List<string>();

        public void Commit()
        {
            //删除实体对象
            foreach (var item in DeletedDynPropertyEntities)
            {
                GatewayFactory.Default.Delete(item.Value, item.Key);
            }
            //新增修改实体对象
            foreach (var savedDynEntity in SavedDynEntities)
            {
                if (savedDynEntity != null)
                {
                    GatewayFactory.Default.Save(savedDynEntity);
                }
            }
            //删除ObjType对象
            foreach (var item in DeletedObjTypes)
            {
                GatewayFactory.Default.Delete(item.Value, item.Key);
            }
            //数据库脚本的执行
            foreach (var sqlCommandString in SqlCommandStrings)
            { 
                GatewayFactory.Default.Db.ExecuteNonQuery(System.Data.CommandType.Text, sqlCommandString);
            }
        }
    }
}
