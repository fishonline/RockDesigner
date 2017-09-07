using Rock.Common;
using Rock.Dyn.Core;
using Rock.Orm.Common;
using Rock.Orm.Data;
using System;
using System.Activities;
using System.Activities.DurableInstancing;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Configuration;
namespace Rock.SystemService
{
    public class SystemService
    {
        public int GetNextID(string typeName)
        {
            Check.Require(!string.IsNullOrEmpty(typeName), "获取NextID的对象名称不允许为空!");
            DynEntity dbEntity = GatewayFactory.Default.Find("ObjType", _.P("ObjType", "Name") == typeName.Trim());
            int nextID = (int)dbEntity["NextID"];
            dbEntity["NextID"] = nextID + 1;
            GatewayFactory.Default.Save(dbEntity);
            return nextID;
        }
        public string ExecuteScalar(string sqlString)
        {
            Check.Require(!string.IsNullOrEmpty(sqlString), "获取Scalar要执行的sql语句不允许为空!");
            object result = GatewayFactory.Default.Db.ExecuteScalar(CommandType.Text, sqlString);
            if (result != null && result != DBNull.Value)
            {
                return result.ToString();
            }
            else
            {
                return "";
            }
        }
        public DynObject ExecQuery(string sqlString)
        {
            Check.Require(sqlString != null, "执行的sql语句不允许为空!");
            DataTable dataTable = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, sqlString).Tables[0];
            return DynObjectTransverter.ToTransDataTable(dataTable);
        }
        public void ExcuteNoneReturnQuery(string sqlString)
        {
            Check.Require(sqlString != null, "执行的sql语句不允许为空!");
            GatewayFactory.Default.Db.ExecuteNonQuery(CommandType.Text, sqlString);
        }
        public void ExcuteNoneReturnQueryWithTrans(string sqlString)
        {
            Check.Require(sqlString != null, "执行的sql语句不允许为空!");
            DbTransaction tran = GatewayFactory.Default.Db.BeginTransaction();

            try
            {
                GatewayFactory.Default.Db.ExecuteNonQuery(tran, CommandType.Text, sqlString);
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw ex;
            }
        }

        public DynObject GetDynObjectByID(string structName, int dynObjectID)
        {
            DynEntity dynEntity = GatewayFactory.Default.Find(structName, dynObjectID);
            return DynObjectTransverter.DynEntity2DynObject(dynEntity);
        }
        public DynObject GetDynObjectByIDs(string structName, int firstID, int secondID)
        {
            DynEntity dynEntity = GatewayFactory.Default.Find(structName, firstID, secondID);
            return DynObjectTransverter.DynEntity2DynObject(dynEntity);
        }
        public void AddDynObject(DynObject dynObject)
        {
            Check.Require(dynObject != null, "添加的对象不允许为空!");
            GatewayFactory.Default.Save(DynObjectTransverter.DynObject2DynEntity(dynObject), null);
        }

        public void AddDynObjectsByTransaction(List<DynObject> objects)
        {
            Check.Require(objects != null && objects.Count > 0, "添加的对象列表不允许为空!");
            DbTransaction tran = GatewayFactory.Default.Db.BeginTransaction();
            try
            {
                foreach (var obj in objects)
                {
                    GatewayFactory.Default.Save(DynObjectTransverter.DynObject2DynEntity(obj), tran);
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw ex;
            }
        }

        public void ModifyDynObject(DynObject dynObject)
        {
            Check.Require(dynObject != null, "修改的对象不允许为空!");
            GatewayFactory.Default.Save(DynObjectTransverter.DynObject2DynEntity(dynObject), null);
        }

        public void DeleteDynObjectByID(string structName, int dynObjectID)
        {
            Check.Require(structName != null, "要删除的对象类型不允许为空!");
            GatewayFactory.Default.Delete(structName, dynObjectID);
        }
        public void DeleteDynObjectByIDs(string structName, int firstID, int secondID)
        {
            Check.Require(structName != null, "要删除的对象类型不允许为空!");
            GatewayFactory.Default.Delete(structName, firstID, secondID);
        }

        public string ExecuteBaseScalar(string sqlString)
        {
            Check.Require(!string.IsNullOrEmpty(sqlString), "获取Scalar要执行的sql语句不允许为空!");

            object result = GatewayFactory.Base.Db.ExecuteScalar(CommandType.Text, sqlString);
            if (result != null && result != DBNull.Value)
            {
                return result.ToString();
            }
            else
            {
                return "";
            }
        }

        public DynObject ExecBaseQuery(string sqlString)
        {
            Check.Require(sqlString != null, "执行的sql语句不允许为空!");
            DataTable dataTable = GatewayFactory.Base.Db.ExecuteDataSet(CommandType.Text, sqlString).Tables[0];
            return DynObjectTransverter.ToTransDataTable(dataTable);
        }

        public string GetBillNO(string billType)
        {
            Check.Require(!string.IsNullOrEmpty(billType), "获取编码的单据类型不允许为空!");
            DynEntity dbEntity = GatewayFactory.Default.Find("Bill", _.P("Bill", "BillType") == billType.Trim());

            string prefix = dbEntity["Prefix"].ToString();
            string availableBillNO = dbEntity["AvailableBillNO"].ToString();
            DateTime lastDate = (DateTime)dbEntity["LastDate"];
            if (lastDate.Date == DateTime.Now.Date)
            {
                availableBillNO = availableBillNO == "" ? "1" : availableBillNO;
            }
            else
            {
                dbEntity["LastDate"] = DateTime.Now.Date;
                availableBillNO = "1";
            }
            dbEntity["AvailableBillNO"] = int.Parse(availableBillNO) + 1;
            GatewayFactory.Default.Save(dbEntity);
            return String.Concat(prefix, DateTime.Now.ToString("yyyyMMdd"), availableBillNO.PadLeft(4, '0'));
        }

        public string GetObjectProperty(string objName, string property,int ojbID)
        {
            Check.Require(!string.IsNullOrEmpty(objName), "获取对象属性对象名不允许为空!");
            Check.Require(!string.IsNullOrEmpty(property), "获取对象属性属性名不允许为空!");
            string sqlString = "SELECT [" + property + "] from [" + objName + "] WHERE " + objName + "ID = " + ojbID;
            return ExecuteScalar(sqlString);           
        }

        //删除已经上传的文档
        public void DeleteUploadFile(int uploadFileID)
        {
            DynEntity uploadFile = GatewayFactory.Default.Find("UploadFile", uploadFileID);
            if (uploadFile != null)
            {
                string sqlString = "select UploadFileID from [UploadFile] where UploadFile.ServerFileName = '" + uploadFile["ServerFileName"] + "'";
                DataTable dataTable = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, sqlString).Tables[0];
                if (dataTable.Rows.Count == 1)
                {
                    GatewayFactory.Default.Delete("UploadFile", uploadFileID);
                    File.Delete(uploadFile["ServerFilePath"] + "\\" + uploadFile["ServerFileName"]);
                }
                else
                {
                    GatewayFactory.Default.Delete("UploadFile", uploadFileID);
                }
            }
        }

        //获取服务器日期
        public string GetServerDate()
        {
            return DateTime.Now.ToShortDateString();
        }

        public string GetDiffDateString(int dateDiff)
        {
            return DateTime.Now.AddDays(dateDiff).ToShortDateString();
        }

        public string GetServerDateTime()
        {
            return DateTime.Now.ToString();
        }

        public string GetConfigValue(string key) 
        {
            return Rock.Common.Utility.ConfigManager.GetConfigValue(key);
        }

        //获取用户所在角色集合
        public DynObject GetUserRoles(int userID)
        {
            Check.Require(userID > 0, "用户ID必需大于0!");
            string sqlString = "select r.RoleID from [UserRole] as u inner join [Role] as r on u.RoleID=r.RoleID where  u.UserID=" + userID;
            DataTable dataTable = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, sqlString).Tables[0];
            return DynObjectTransverter.ToTransDataTable(dataTable);
        }

        //获取用户角色所有功能集合
        public DynObject GetUserRoleActions(int roleID)
        {
            Check.Require(roleID > 0, "角色ID必需大于0!");
            string sqlString = "select ActionID from [RoleAction] where RoleID=" + roleID;
            DataTable dataTable = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, sqlString).Tables[0];
            return DynObjectTransverter.ToTransDataTable(dataTable);
        }

        //获取所有菜单集合
        public DynObject GetMenus()
        {
            string sqlString = "select MenuID,MenuName, CommandText,ActionID,DisplayOrder,ParentID FROM Menu order by ParentID, DisplayOrder";
            DataTable dataTable = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, sqlString).Tables[0];
            return DynObjectTransverter.ToTransDataTable(dataTable);
        }

        //获取用户菜单集合
        public DynObject GetUserMenus(int userID)
        {
            //获取用户有权的3级菜单
            string sqlString = "select MenuID,MenuName, CommandText,ActionID,DisplayOrder,ParentID FROM Menu where Grades = 3 and ActionID in (select ActionID from RoleAction where RoleID in (select RoleID from UserRole where UserID = " + userID + ")) order by ParentID, DisplayOrder";
            DataTable dtGrade3 = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, sqlString).Tables[0];
            if (dtGrade3.Rows.Count == 0)
            {
                return DynObjectTransverter.ToTransDataTable(dtGrade3);
            }
            //获取二级菜单的ID
            DataView dataView = dtGrade3.DefaultView;
            DataTable dtGrade2IDs = dataView.ToTable(true, "ParentID");
            string IDs = "";
            for (int i = 0; i < dtGrade2IDs.Rows.Count; i++)
            {
                if (i < dtGrade2IDs.Rows.Count - 1)
                {
                    IDs += dtGrade2IDs.Rows[i][0].ToString() + ",";
                }
                else
                {
                    IDs += dtGrade2IDs.Rows[i][0].ToString() ;
                }               
            }
            //获取二级菜单集合
            sqlString = "select MenuID,MenuName, CommandText,ActionID,DisplayOrder,ParentID FROM Menu where Grades = 2 and MenuID in (" + IDs + ") order by ParentID, DisplayOrder";
            DataTable dtGrade2 = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, sqlString).Tables[0];

            //获取一级菜单的ID
            dataView = dtGrade2.DefaultView;
            DataTable dtGrade1IDs = dataView.ToTable(true, "ParentID");
            IDs = "";
            for (int i = 0; i < dtGrade1IDs.Rows.Count; i++)
            {
                if (i < dtGrade1IDs.Rows.Count - 1)
                {
                    IDs += dtGrade1IDs.Rows[i][0].ToString() + ",";
                }
                else
                {
                    IDs += dtGrade1IDs.Rows[i][0].ToString();
                }
            }
            //获取一级菜单集合
            sqlString = "select MenuID,MenuName, CommandText,ActionID,DisplayOrder,ParentID FROM Menu where Grades = 1 and MenuID in (" + IDs + ") order by ParentID, DisplayOrder";
            DataTable dtGrade1 = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, sqlString).Tables[0];

            dtGrade1.Merge(dtGrade2);
            dtGrade1.Merge(dtGrade3);

            return DynObjectTransverter.ToTransDataTable(dtGrade1);
        }

        public void StartWorkflow(int workflowID, int userID, List<string> exchangeParamLists)
        {
            Dictionary<string, object> exchangeParams = new Dictionary<string, object>();
            Dictionary<string, object> workflowParams = new Dictionary<string, object>();
            foreach (string item in exchangeParamLists)
            {
                string[] x = item.Split('^');
                exchangeParams.Add(x[0], x[1]);
            }

            WorkflowApplication workflowApplication = null;
            SqlWorkflowInstanceStore instanceStore = null;
            InstanceView view;

            //获取工作流实例ID
            int workflowInstanceID = GetNextID("WorkflowInstance");
            string definition = ExecuteScalar("select RuntimeDefinition from Workflow where WorkflowID = " + workflowID);// workflowEntity["RuntimeDefinition"].ToString();

            //加载并运行工作流
            byte[] bs = Encoding.UTF8.GetBytes(definition);
            Activity activity = null;

            using (MemoryStream memoryStream = new MemoryStream(bs))
            {
                activity = ActivityXamlServices.Load(memoryStream);
            }

            //workflowParams = new Dictionary<string, object>();
            workflowParams.Add("WorkflowInstanceID", workflowInstanceID);
            workflowParams.Add("ExchangeParams", exchangeParams);

            workflowApplication = new WorkflowApplication(activity, workflowParams);

            if (instanceStore == null)
            {
                string connectionString = GatewayFactory.DefaultConnectionString;
                instanceStore = new SqlWorkflowInstanceStore(connectionString);
                view = instanceStore.Execute(instanceStore.CreateInstanceHandle(), new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(30));
                instanceStore.DefaultInstanceOwner = view.InstanceOwner;
            }
            string objType = "";
            int objID = 0;
            string command = "";
            if (exchangeParams.ContainsKey("ObjType"))
            {
                objType = exchangeParams["ObjType"].ToString();
            }
            if (exchangeParams.ContainsKey("ObjID"))
            {
                objID = Convert.ToInt32( exchangeParams["ObjID"]);
            }
            if (exchangeParams.ContainsKey("Command"))
            {
                command = exchangeParams["Command"].ToString();
            }

            //添加工作流实例
            DynEntity workflowInstance = new DynEntity("WorkflowInstance");
            workflowInstance["WorkflowInstanceID"] = workflowInstanceID;
            workflowInstance["WorkflowID"] = workflowID;
            workflowInstance["StartTime"] = DateTime.Now;
            workflowInstance["InstanceGUID"] = workflowApplication.Id.ToString();
            workflowInstance["ObjType"] = objType;
            workflowInstance["ObjID"] = objID;
            workflowInstance["Command"] = command;
            workflowInstance["Actor"] = userID;
            workflowInstance["State"] = "已保存";           

            //添加启动工作流活动实例节点,便于流程历史查看
            int workflowActivityInstanceID = GetNextID("WorkflowActivityInstance");
            DynEntity workflowActivityInstance = new DynEntity("WorkflowActivityInstance");
            workflowActivityInstance["WorkflowActivityInstanceID"] = workflowActivityInstanceID;
            workflowActivityInstance["WorkflowActivityInstanceName"] = "流程启动";
            workflowActivityInstance["WorkflowActivityID"] = -1;
            workflowActivityInstance["WorkflowInstanceID"] = workflowInstanceID;            
            workflowActivityInstance["StartTime"] = DateTime.Now;
            workflowActivityInstance["EndTime"] = DateTime.Now;
            workflowActivityInstance["Actor"] = userID;
            workflowActivityInstance["State"] = "已执行";
            workflowActivityInstance["ObjType"] = objType;
            workflowActivityInstance["ObjID"] = objID;
            workflowActivityInstance["Handler"] = ExecuteScalar("select TrueName from [User] where UserID = " + userID);            
            
            using (TransactionScope trans = new TransactionScope())
            {
                GatewayFactory.Default.Save(workflowInstance);
                GatewayFactory.Default.Save(workflowActivityInstance);

                //更新状态字段
                if (exchangeParams.ContainsKey("ObjType") && exchangeParams.ContainsKey("ObjID") && exchangeParams.ContainsKey("StateField") && exchangeParams.ContainsKey("StateValue"))
                {
                    string sqlString = "UPDATE [" + exchangeParams["ObjType"] + "] SET [" + exchangeParams["StateField"] + "] = '" + exchangeParams["StateValue"] + "' WHERE " + exchangeParams["ObjType"] + "ID = " + exchangeParams["ObjID"];
                    ExcuteNoneReturnQuery(sqlString);
                }

                //执行附加SQL
                if (exchangeParams.ContainsKey("AdditionalSql"))
                {
                    ExcuteNoneReturnQuery(exchangeParams["AdditionalSql"].ToString());
                }

                workflowApplication.InstanceStore = instanceStore;
                workflowApplication.PersistableIdle = workflowPersistableIdle;
                workflowApplication.Idle = workflowIdle;
                workflowApplication.Unloaded = unload;
                workflowApplication.Aborted = workflowAborted;
                workflowApplication.Completed = workflowCompleted;
                workflowApplication.OnUnhandledException = workflowUnhandledException;
                workflowApplication.Run();

                trans.Complete();
            }           
        }

        public void ResumeBookmark(int workflowToDoListID, int userID, List<string> exchangeParamLists)
        {
            Dictionary<string, object> exchangeParams = new Dictionary<string, object>();

            foreach (string item in exchangeParamLists)
            {
                string[] x = item.Split('^');
                exchangeParams.Add(x[0], x[1]);
            }

            WorkflowApplication workflowApplication = null;
            SqlWorkflowInstanceStore instanceStore = null;
            InstanceView view;

            DynEntity workflowToDoEntity = GatewayFactory.Default.Find("WorkflowToDoList", workflowToDoListID);
            DynEntity workflowInstance = null;
            if (workflowToDoEntity != null)
            {
                workflowInstance = GatewayFactory.Default.Find("WorkflowInstance", Convert.ToInt32(workflowToDoEntity["WorkflowInstanceID"]));
                string bookmarkName = workflowToDoEntity["BookmarkName"].ToString();
                if (workflowInstance != null)
                {
                    string definition = ExecuteScalar("select RuntimeDefinition from Workflow where WorkflowID = " + workflowToDoEntity["WorkflowID"]);
                    //加载并运行工作流
                    byte[] bs = Encoding.UTF8.GetBytes(definition);
                    Activity activity = null;
                    using (MemoryStream memoryStream = new MemoryStream(bs))
                    {
                        activity = ActivityXamlServices.Load(memoryStream);
                    }

                    workflowApplication = new WorkflowApplication(activity);

                    if (instanceStore == null)
                    {
                        string connectionString = GatewayFactory.DefaultConnectionString;
                        instanceStore = new SqlWorkflowInstanceStore(connectionString);
                        view = instanceStore.Execute(instanceStore.CreateInstanceHandle(), new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(30));
                        instanceStore.DefaultInstanceOwner = view.InstanceOwner;
                    }

                    workflowApplication.InstanceStore = instanceStore;
                    workflowApplication.PersistableIdle = workflowPersistableIdle;
                    workflowApplication.Idle = workflowIdle;
                    workflowApplication.Unloaded = unload;
                    workflowApplication.Aborted = workflowAborted;
                    workflowApplication.Completed = workflowCompleted;
                    workflowApplication.OnUnhandledException = workflowUnhandledException;
                    Guid guid = new Guid(workflowInstance["InstanceGUID"].ToString());

                    try
                    {
                        workflowApplication.Load(guid);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (workflowApplication != null)
                    {
                        if (workflowApplication.GetBookmarks().Count(p => p.BookmarkName == bookmarkName) == 1)
                        {
                            DynEntity workflowActivityInstance = GatewayFactory.Default.Find("WorkflowActivityInstance", Convert.ToInt32(workflowToDoEntity["WorkflowActivityInstanceID"]));
                            if (workflowActivityInstance != null)
                            {
                                foreach (string item in exchangeParamLists)
                                {
                                    string[] arg = item.Split('^');

                                    switch (arg[0])
                                    {
                                        case "Handler":
                                            workflowActivityInstance["Handler"] = arg[1];
                                            break;
                                        case "ObjType":
                                            workflowActivityInstance["ObjType"] = arg[1];
                                            break;
                                        case "ObjID":
                                            workflowActivityInstance["ObjID"] = arg[1];
                                            break;
                                        case "DetailID":
                                            workflowActivityInstance["DetailID"] = arg[1];
                                            break;
                                        case "Opinion":
                                            workflowActivityInstance["Opinion"] = arg[1];
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                exchangeParams["Actor"] = userID;

                                workflowActivityInstance["State"] = "已执行";
                                workflowActivityInstance["EndTime"] = DateTime.Now;

                                using (TransactionScope trans = new TransactionScope())
                                {
                                    GatewayFactory.Default.Save(workflowActivityInstance);
                                    GatewayFactory.Default.Delete("WorkflowToDoList", workflowToDoListID);

                                    //更新状态字段
                                    if (exchangeParams.ContainsKey("ObjType") && exchangeParams.ContainsKey("ObjID") && exchangeParams.ContainsKey("StateField") && exchangeParams.ContainsKey("StateValue"))
                                    {
                                        string sqlString = "UPDATE [" + exchangeParams["ObjType"] + "] SET [" + exchangeParams["StateField"] + "] = '" + exchangeParams["StateValue"] + "' WHERE " + exchangeParams["ObjType"] + "ID = " + exchangeParams["ObjID"];
                                        ExcuteNoneReturnQuery(sqlString);
                                    }

                                    //执行附加SQL
                                    if (exchangeParams.ContainsKey("AdditionalSql"))
                                    {
                                        ExcuteNoneReturnQuery(exchangeParams["AdditionalSql"].ToString());
                                    }

                                    workflowApplication.ResumeBookmark(bookmarkName, exchangeParams);
                                    trans.Complete();
                                }
                            }
                            else
                            {
                                throw new ApplicationException(string.Format("在工作流活动:{0} 不存在无法执行,请检查", workflowActivityInstance["WorkflowActivityInstanceName"], bookmarkName));
                            }
                        }
                        else
                        {
                            throw new ApplicationException(string.Format("在工作流实例{0}中找不到书签名为{1}的活动", workflowToDoEntity["WorkflowInstanceID"], bookmarkName));
                        }
                    }
                    else
                    {
                        throw new ApplicationException("工作流创建实例失败!");
                    }
                }
            }
            else
            {
                throw new ApplicationException("当前待做任务在数据库中不存在,请检查!");
            }
        }

        public void TerminateWorkflow(int workflowID, int workflowInstanceID, int userID, List<string> exchangeParamLists)
        {
            Dictionary<string, object> exchangeParams = new Dictionary<string, object>();

            foreach (string item in exchangeParamLists)
            {
                string[] arg = item.Split('^');
                exchangeParams.Add(arg[0], arg[1]);
            }

            WorkflowApplication workflowApplication = null;
            SqlWorkflowInstanceStore instanceStore = null;
            InstanceView view;

            DynEntity workflowInstance = null;
            workflowInstance = GatewayFactory.Default.Find("WorkflowInstance", workflowInstanceID);
            if (workflowInstance != null)
            {
                string definition = ExecuteScalar("select RuntimeDefinition from Workflow where WorkflowID = " + workflowID);
                //加载并运行工作流
                byte[] bs = Encoding.UTF8.GetBytes(definition);
                Activity activity = null;
                using (MemoryStream memoryStream = new MemoryStream(bs))
                {
                    activity = ActivityXamlServices.Load(memoryStream);
                }

                workflowApplication = new WorkflowApplication(activity);

                if (instanceStore == null)
                {
                    string connectionString = GatewayFactory.DefaultConnectionString;
                    instanceStore = new SqlWorkflowInstanceStore(connectionString);
                    view = instanceStore.Execute(instanceStore.CreateInstanceHandle(), new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(30));
                    instanceStore.DefaultInstanceOwner = view.InstanceOwner;
                }

                workflowApplication.InstanceStore = instanceStore;
                workflowApplication.PersistableIdle = workflowPersistableIdle;
                workflowApplication.Idle = workflowIdle;
                workflowApplication.Unloaded = unload;
                workflowApplication.Aborted = workflowAborted;
                workflowApplication.Completed = workflowCompleted;
                workflowApplication.OnUnhandledException = workflowUnhandledException;
                Guid guid = new Guid(workflowInstance["InstanceGUID"].ToString());

                try
                {
                    workflowApplication.Load(guid);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                workflowApplication.Terminate("终止工作流实例!");

                //后续流程处理
                using (TransactionScope trans = new TransactionScope())
                {
                    //删除待做任务
                    ExcuteNoneReturnQuery("Delete WorkflowToDoList where WorkflowInstanceID = " + workflowInstanceID + " and WorkflowID = " + workflowID);
                    //更新WorkflowActivityInstance
                    string sqlString = "UPDATE [WorkflowActivityInstance] SET [EndTime] = '" + DateTime.Now + "', [Actor] = " + userID + ", [State] = '已取消' WHERE WorkflowInstanceID = " + workflowInstanceID;
                    ExcuteNoneReturnQuery(sqlString);

                    //更新状态字段
                    if (exchangeParams.ContainsKey("ObjType") && exchangeParams.ContainsKey("ObjID") && exchangeParams.ContainsKey("StateField") && exchangeParams.ContainsKey("StateValue"))
                    {
                        sqlString = "UPDATE [" + exchangeParams["ObjType"] + "] SET [" + exchangeParams["StateField"] + "] = '" + exchangeParams["StateValue"] + "' WHERE " + exchangeParams["ObjType"] + "ID = " + exchangeParams["ObjID"];
                        ExcuteNoneReturnQuery(sqlString);
                    }

                    //执行附加SQL
                    if (exchangeParams.ContainsKey("AdditionalSql"))
                    {
                        ExcuteNoneReturnQuery(exchangeParams["AdditionalSql"].ToString());
                    }

                    trans.Complete();
                }
            }
            else
            {
                throw new ApplicationException("数据库中工作流实例不存在,请检查!");
            }
        }

        public void ResetWorkflow(int workflowID, int workflowInstanceID, int userID, List<string> exchangeParamLists)
        {
            Dictionary<string, object> exchangeParams = new Dictionary<string, object>();
            Dictionary<string, object> workflowParams = new Dictionary<string, object>();
            foreach (string item in exchangeParamLists)
            {
                string[] arg = item.Split('^');
                exchangeParams.Add(arg[0], arg[1]);
            }

            WorkflowApplication workflowApplication = null;
            SqlWorkflowInstanceStore instanceStore = null;
            InstanceView view;

            DynEntity workflowInstance = null;
            workflowInstance = GatewayFactory.Default.Find("WorkflowInstance", workflowInstanceID);
            if (workflowInstance != null)
            {
                string definition = ExecuteScalar("select RuntimeDefinition from Workflow where WorkflowID = " + workflowID);
                //加载并运行工作流
                byte[] bs = Encoding.UTF8.GetBytes(definition);
                Activity activity = null;
                using (MemoryStream memoryStream = new MemoryStream(bs))
                {
                    activity = ActivityXamlServices.Load(memoryStream);
                }

                workflowApplication = new WorkflowApplication(activity);

                if (instanceStore == null)
                {
                    string connectionString = GatewayFactory.DefaultConnectionString;
                    instanceStore = new SqlWorkflowInstanceStore(connectionString);
                    view = instanceStore.Execute(instanceStore.CreateInstanceHandle(), new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(30));
                    instanceStore.DefaultInstanceOwner = view.InstanceOwner;
                }

                workflowApplication.InstanceStore = instanceStore;
                workflowApplication.PersistableIdle = workflowPersistableIdle;
                workflowApplication.Idle = workflowIdle;
                workflowApplication.Unloaded = unload;
                workflowApplication.Aborted = workflowAborted;
                workflowApplication.Completed = workflowCompleted;
                workflowApplication.OnUnhandledException = workflowUnhandledException;
                Guid guid = new Guid(workflowInstance["InstanceGUID"].ToString());

                try
                {
                    workflowApplication.Load(guid);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                //当用[TerminateWorkflow]结束流程时,流程正常结束.会调用实例的[completed],但状态为[faulted]
                workflowApplication.Terminate("重启工作流实例!");

                //重新启动工作流实例
                workflowApplication = null;
                //获取新工作流实例ID
                int newWorkflowInstanceID = GetNextID("WorkflowInstance");

                workflowParams.Add("WorkflowInstanceID", newWorkflowInstanceID);
                workflowParams.Add("ExchangeParams", exchangeParams);

                workflowApplication = new WorkflowApplication(activity, workflowParams);               
                string objType = "";
                int objID = 0;
                string command = "";
                if (exchangeParams.ContainsKey("ObjType"))
                {
                    objType = exchangeParams["ObjType"].ToString();
                }
                if (exchangeParams.ContainsKey("ObjID"))
                {
                    objID = Convert.ToInt32(exchangeParams["ObjID"]);
                }
                if (exchangeParams.ContainsKey("Command"))
                {
                    command = exchangeParams["Command"].ToString();
                }

                //添加工作流实例
                workflowInstance = new DynEntity("WorkflowInstance");
                workflowInstance["WorkflowInstanceID"] = newWorkflowInstanceID;
                workflowInstance["WorkflowID"] = workflowID;
                workflowInstance["StartTime"] = DateTime.Now;
                workflowInstance["InstanceGUID"] = workflowApplication.Id.ToString();
                workflowInstance["ObjType"] = objType;
                workflowInstance["ObjID"] = objID;
                workflowInstance["Command"] = command;
                workflowInstance["Actor"] = userID;
                workflowInstance["State"] = "已保存";

                //添加启动工作流活动实例节点,便于流程历史查看
                int workflowActivityInstanceID = GetNextID("WorkflowActivityInstance");
                DynEntity workflowActivityInstance = new DynEntity("WorkflowActivityInstance");
                workflowActivityInstance["WorkflowActivityInstanceID"] = workflowActivityInstanceID;
                workflowActivityInstance["WorkflowActivityInstanceName"] = "流程重新启动";
                workflowActivityInstance["WorkflowActivityID"] = -1;
                workflowActivityInstance["WorkflowInstanceID"] = newWorkflowInstanceID;
                workflowActivityInstance["StartTime"] = DateTime.Now;
                workflowActivityInstance["EndTime"] = DateTime.Now;
                workflowActivityInstance["Actor"] = userID;
                workflowActivityInstance["State"] = "已执行";
                workflowActivityInstance["ObjType"] = objType;
                workflowActivityInstance["ObjID"] = objID;
                workflowActivityInstance["Handler"] = ExecuteScalar("select TrueName from [User] where UserID = " + userID);               

                //后续流程处理
                using (TransactionScope trans = new TransactionScope())
                {

                    GatewayFactory.Default.Save(workflowInstance);
                    GatewayFactory.Default.Save(workflowActivityInstance);

                    //更新状态字段
                    string sqlString = "";
                    if (exchangeParams.ContainsKey("ObjType") && exchangeParams.ContainsKey("ObjID") && exchangeParams.ContainsKey("StateField") && exchangeParams.ContainsKey("StateValue"))
                    {
                        sqlString = "UPDATE [" + objType + "] SET [" + exchangeParams["StateField"] + "] = '" + exchangeParams["StateValue"] + "' WHERE " + exchangeParams["ObjType"] + "ID = " + objID;
                        ExcuteNoneReturnQuery(sqlString);
                    }                  

                    //执行附加SQL
                    if (exchangeParams.ContainsKey("AdditionalSql"))
                    {
                        ExcuteNoneReturnQuery(exchangeParams["AdditionalSql"].ToString());
                    }
                    //删除原工作流实例待做任务
                    ExcuteNoneReturnQuery("Delete WorkflowToDoList where WorkflowInstanceID = " + workflowInstanceID + " and WorkflowID = " + workflowID);
                    //更新原WorkflowActivityInstance
                    sqlString = "UPDATE [WorkflowActivityInstance] SET [EndTime] = '" + DateTime.Now + "', [Actor] = " + userID + ", [State] = '已取消' WHERE WorkflowInstanceID = " + workflowInstanceID;
                    ExcuteNoneReturnQuery(sqlString); 

                    workflowApplication.InstanceStore = instanceStore;
                    workflowApplication.PersistableIdle = workflowPersistableIdle;
                    workflowApplication.Idle = workflowIdle;
                    workflowApplication.Unloaded = unload;
                    workflowApplication.Aborted = workflowAborted;
                    workflowApplication.Completed = workflowCompleted;
                    workflowApplication.OnUnhandledException = workflowUnhandledException;
                    workflowApplication.Run();
                    trans.Complete();
                }
            }
            else
            {
                throw new ApplicationException("数据库中工作流实例不存在,请检查!");
            }
           
        }
        public DynObject GetToDoListByUserID(int userID, string tableName)
        {
            //用户类型待做任务
            string sqlString = "select WorkflowToDoListID, WorkflowToDoListName, Expression, BookmarkName, WorkflowInstanceID, WorkflowActivityInstanceID, ObjID from WorkflowToDoList where State = '待处理' and Type = '1'";
            DataTable dataTable = GatewayFactory.Default.Db.ExecuteDataSet(CommandType.Text, sqlString).Tables[0];
            return DynObjectTransverter.ToTransDataTable(dataTable);
        }

        private PersistableIdleAction workflowPersistableIdle(WorkflowApplicationIdleEventArgs e)
        {
            return PersistableIdleAction.Unload;
        }

        private void workflowIdle(WorkflowApplicationIdleEventArgs e)
        {
        }

        private void unload(WorkflowApplicationEventArgs e)
        {
        }

        private void workflowAborted(WorkflowApplicationAbortedEventArgs e)
        {
            string objType = ExecuteScalar("select [ObjType] from [WorkflowInstance] where [InstanceGUID] = '" + e.InstanceId + "'");
            string objID = ExecuteScalar("select [ObjID] from [WorkflowInstance] where [InstanceGUID] = '" + e.InstanceId + "'");
            DynEntity log = new DynEntity("Log");
            log["LogID"] = GetNextID("Log");
            log["LogType"] = "工作流失败";
            log["ObjType"] = objType;
            log["ObjID"] = objID;
            log["Comment"] = e.Reason.Message + "实例为:" + e.InstanceId; 
            log["OperateTime"] = DateTime.Now;
            log["OperaterName"] = "system";
            log["OperaterID"] = 0;
            GatewayFactory.Default.Save(log);
        }

        private void workflowCompleted(WorkflowApplicationCompletedEventArgs e)
        {            
            DynEntity workflowInstance = GatewayFactory.Default.Find("WorkflowInstance", _.P("WorkflowInstance", "InstanceGUID") == e.InstanceId);
            DynEntity log = new DynEntity("Log");
            log["LogID"] = GetNextID("Log");
            log["LogType"] = "工作流完成";
            log["ObjType"] = workflowInstance["ObjType"];
            log["ObjID"] = workflowInstance["ObjID"];
            log["Comment"] = "工作流执行完成实例为:" + e.InstanceId + "完成状态为:" + e.CompletionState.ToString();
            //if (e.TerminationException != null)
            //{
            //    log["Comment"] = "工作流执行完成实例为:" + e.InstanceId + "完成状态为:" + e.CompletionState.ToString() + "消息:" + e.TerminationException.Message;
            //}
            //else
            //{
            //    log["Comment"] = "工作流执行完成实例为:" + e.InstanceId + "完成状态为:" + e.CompletionState.ToString();
            //}
            log["OperateTime"] = DateTime.Now;
            log["OperaterName"] = "system";
            log["OperaterID"] = 0;
            if (e.CompletionState == ActivityInstanceState.Faulted)
            {
                workflowInstance["State"] = "已终止";
            }
            else
            {
                workflowInstance["State"] = "已完成";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                GatewayFactory.Default.Save(log);
                GatewayFactory.Default.Save(workflowInstance);
                trans.Complete();
            }          
        }

        private UnhandledExceptionAction workflowUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
        {
            string objType = ExecuteScalar("select [ObjType] from [WorkflowInstance] where [InstanceGUID] = '" + e.InstanceId + "'");
            string objID = ExecuteScalar("select [ObjID] from [WorkflowInstance] where [InstanceGUID] = '" + e.InstanceId + "'");
            
            DynEntity log = new DynEntity("Log");
            log["LogID"] = GetNextID("Log");
            log["LogName"] = e.ExceptionSource.DisplayName;
            log["LogType"] = "工作流异常";
            log["ObjType"] = objType;
            log["ObjID"] = objID;
            log["Comment"] = e.UnhandledException.Message + "实例为:" + e.InstanceId; 
            log["OperateTime"] = DateTime.Now;
            log["OperaterName"] = e.ExceptionSource.Id;
            log["OperaterID"] = 0;
            ////将工作流对应的对象状态置为异常终止,可以重新启动
            //string updateSql = "update " + objType + "set State = '异常终止' where " + objType + "ID = " + objID;
            //ExcuteNoneReturnQuery(updateSql);
            GatewayFactory.Default.Save(log);
            return UnhandledExceptionAction.Cancel;
        }
    }
}
