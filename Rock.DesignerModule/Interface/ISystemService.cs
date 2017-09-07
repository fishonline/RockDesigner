using Rock.Dyn.Core;
using Rock.Orm.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.DesignerModule.Interface
{
    public interface ISystemService
    {
        int GetNextID(string typeName);
        string ExecuteScalar(string sqlString);
        DataTable GetDataTable(string cmdText);
        DynEntity GetDynEntityByID(string structName, params object[] pkValues);
        DynObject GetDynObjectByID(string structName, params object[] pkValues);
        void AddDynEntity(DynEntity dynEntity);
        void AddDynObject(DynObject dynObject);
        void ModifyDynEntity(DynEntity dynEntity);
        void ModifyDynObject(DynObject dynObject);
        void DeleteObjectByID(string structName, params object[] pkValues);
        void AddStaticEntity(Entity entity);
        void ModifyStaticEntity(Entity entity);
    }
}
