using Rock.Orm.Common;
using Rock.Orm.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ActivityDesignerLibrary
{
    public class DesignService
    {
        public int GetNextID(string typeName)
        {
            DynEntity dynEntity = GatewayFactory.Default.Find("ObjType", _.P("ObjType", "Name") == typeName.Trim());
            int nextID = (int)dynEntity["NextID"];
            dynEntity["NextID"] = nextID + 1;
            GatewayFactory.Default.Save(dynEntity);
            return nextID;
        }

        public DataTable GetDataTable(string cmdText)
        {
            return GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, cmdText).Tables[0];
        }

        public DynEntity GetDynEntityByID(string structName, params object[] pkValues)
        {
            return GatewayFactory.Default.Find(structName, pkValues);
        }

        public void AddDynEntity(DynEntity dynEntity)
        {
            Check.Require(dynEntity != null, "添加的实体不允许为空!");
            GatewayFactory.Default.Save(dynEntity, null);
        }
        public void ModifyDynEntity(DynEntity dynEntity)
        {
            Check.Require(dynEntity != null, "修改的实体不允许为空!");
            GatewayFactory.Default.Save(dynEntity, null);
        }

        public void TerminateWorkflow(string tableName, string tableKey)
        {
            DynEntity dynEntity = null;
            dynEntity = GatewayFactory.Default.Find(tableName, Convert.ToInt32(tableKey));
            dynEntity["Comment"] = "已通过";
            GatewayFactory.Default.Save(dynEntity);
        }

        public void ExcuteNoneReturnQuery(string sqlString)
        {
            Check.Require(sqlString != null, "执行的sql语句不允许为空!");
            GatewayFactory.Default.Db.ExecuteNonQuery(CommandType.Text, sqlString);
        }
    }
}
