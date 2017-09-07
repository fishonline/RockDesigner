using Rock.Common;
using Rock.DesignerModule.Interface;
using Rock.Dyn.Core;
using Rock.Orm.Common;
using Rock.Orm.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.DesignerModule.Service
{
    [Export(typeof(ISystemService))]
    public class SystemService : ISystemService
    {
        public int GetNextID(string typeName)
        {
            DynEntity dbEntity = GatewayFactory.Default.Find("ObjType", _.P("ObjType", "Name") == typeName.Trim());
            int nextID = (int)dbEntity["NextID"];
            dbEntity["NextID"] = nextID + 1;
            GatewayFactory.Default.Save(dbEntity);
            return nextID;
        }
        public string ExecuteScalar(string sqlString)
        {
            string value = "";
            if (GatewayFactory.Default.Db.ExecuteScalar(CommandType.Text, sqlString) == null)
            {
                return value;
            }
            else
            {
                return GatewayFactory.Default.Db.ExecuteScalar(CommandType.Text, sqlString).ToString();
            }
        }

        public DataTable GetDataTable(string cmdText)
        {
            return GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, cmdText).Tables[0];
        }

        public DynEntity GetDynEntityByID(string structName, params object[] pkValues)
        {
            return GatewayFactory.Default.Find(structName, pkValues);
        }

        public DynObject GetDynObjectByID(string structName, params object[] pkValues)
        {
            DynEntity dynEntity = GatewayFactory.Default.Find(structName, pkValues);
            return DynObjectTransverter.DynEntity2DynObject(dynEntity);
        }

        public void AddDynEntity(DynEntity dynEntity)
        {
            Check.Require(dynEntity != null, "添加的实体不允许为空!");
            GatewayFactory.Default.Save(dynEntity, null);
        }

        public void AddDynObject(DynObject dynObject)
        {
            Check.Require(dynObject != null, "添加的对象不允许为空!");
            GatewayFactory.Default.Save(DynObjectTransverter.DynObject2DynEntity(dynObject), null);
        }

        public void ModifyDynEntity(DynEntity dynEntity)
        {
            Check.Require(dynEntity != null, "修改的实体不允许为空!");
            GatewayFactory.Default.Save(dynEntity, null);
        }

        public void ModifyDynObject(DynObject dynObject)
        {
            Check.Require(dynObject != null, "修改的对象不允许为空!");
            GatewayFactory.Default.Save(DynObjectTransverter.DynObject2DynEntity(dynObject), null);
        }

        public void DeleteObjectByID(string structName, params object[] pkValues)
        {
            Check.Require(structName != null, "要删除的对象类型不允许为空!");
            Check.Require(pkValues != null, "要删除的对象主键不允许为空!");
            GatewayFactory.Default.Delete(structName, pkValues);
        }

        public void AddStaticEntity(Entity entity)
        {
            Check.Require(entity != null, "添加的静态实体不允许为空!");
            GatewayFactory.Default.Save(entity);
        }
        public void ModifyStaticEntity(Entity entity)
        {
            Check.Require(entity != null, "修改的静态实体不允许为空!");
            GatewayFactory.Default.Save(entity);
        }

        //public void TerminateWorkflow(string tableName, string tableKey, string state)
        //{
        //    DynEntity dynEntity = null;
        //    switch (state)
        //    {
        //        case "Exception":
        //            dynEntity = GatewayFactory.Default.Find(tableName, Convert.ToInt32(tableKey));
        //            dynEntity["AuditState"] = "已退回";
        //            GatewayFactory.Default.Save(dynEntity);
        //            break;
        //        default:
        //            dynEntity = GatewayFactory.Default.Find(tableName, Convert.ToInt32(tableKey));
        //            dynEntity["AuditState"] = "已审核";
        //            GatewayFactory.Default.Save(dynEntity);
        //            break;
        //    }
        //}
    }
}
