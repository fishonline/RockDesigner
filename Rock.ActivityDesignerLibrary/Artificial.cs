using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Activities.Presentation;
using System.Windows;
using Microsoft.VisualBasic.Activities;
using System.Transactions;
using Rock.Orm.Common;


namespace Rock.ActivityDesignerLibrary
{
    [Designer(typeof(ArtificialDesigner))]
    public sealed class Artificial : NativeActivity<string>, IActivityTemplateFactory
    {
        [Description("工作流实例ID")]
        [Browsable(true)]
        public InArgument<int> WorkflowInstanceID { get; set; }

        //[Description("操作人")]
        //[RequiredArgument]
        //[Browsable(false)]
        //public InArgument<string> Actor { get; set; }      
       

        [Description("参数")]
        [RequiredArgument]
        [Browsable(true)]
        public InOutArgument<Dictionary<string, object>> ExchangeParams { get; set; }

        [Description("描述")]
        [RequiredArgument]
        public InArgument<string> Description { get; set; }


        [Description("工作流活动ID")]
        [RequiredArgument]
        [Browsable(true)]
        public int WorkflowActivityID { get; set; }

        /// <summary>
        /// 对于 NativeActivity 派生的活动，若其调用 
        /// System.Activities.NativeActivityContext 中定义的某个 CreateBookmark 重载，
        /// 以便执行异步操作，则必须覆盖 CanInduceIdle 属性且返回 true
        /// 指示活动是否会使工作流进入空闲状态。必须返回true，因为我们的持久化工作将会在时工作流处于PersistentIdle状态进行
        /// </summary>
        protected override bool CanInduceIdle
        {
            get
            { return true; }
        }

        public Activity Create(DependencyObject target)
        {
            ArtificialGuide artificialGuide = new ArtificialGuide();
            bool? result = artificialGuide.ShowDialog();

            if (result == true)
            {
                Artificial artificial = new Artificial();

                artificial.DisplayName = artificialGuide.txtDescription.Text;

                artificial.Description = artificialGuide.txtDescription.Text;
                artificial.WorkflowActivityID = artificialGuide.WorkflowActivityID;

                VisualBasicValue<int> workflowfInstanceIDParams = new VisualBasicValue<int>();
                workflowfInstanceIDParams.ExpressionText = "WorkflowInstanceID";
                artificial.WorkflowInstanceID = workflowfInstanceIDParams;

                artificial.ExchangeParams = new VisualBasicReference<Dictionary<string, object>> { ExpressionText = "ExchangeParams" };
                //artificial.Actor = new VisualBasicValue<string> { ExpressionText = "Actor" };

                return artificial;
            }
            return null;
        }

        protected override void Execute(NativeActivityContext context)
        {
            DesignService designService = new DesignService();
            //启动事务
            using (TransactionScope trans = new TransactionScope())
            {
                //获取传入的参数集合
                Dictionary<string, object> inParams = this.ExchangeParams.Get(context) as Dictionary<string, object>;    
                //添加工作流活动实例
                DynEntity workflowfActivityInstance = new DynEntity("WorkflowActivityInstance");
                workflowfActivityInstance["WorkflowActivityInstanceID"] = designService.GetNextID("WorkflowActivityInstance");
                workflowfActivityInstance["WorkflowInstanceID"] = context.GetValue(WorkflowInstanceID);
                workflowfActivityInstance["WorkflowActivityID"] = WorkflowActivityID;
                workflowfActivityInstance["WorkflowActivityInstanceName"] = DisplayName;
                if (inParams.ContainsKey("ObjType"))
                {
                    workflowfActivityInstance["ObjType"] = inParams["ObjType"].ToString();
                }
                if (inParams.ContainsKey("ObjID"))
                {
                    workflowfActivityInstance["ObjID"] = inParams["ObjID"].ToString();
                }
                workflowfActivityInstance["State"] = "正在执行";
                workflowfActivityInstance["StartTime"] = DateTime.Now;
                designService.AddDynEntity(workflowfActivityInstance);

                //获取工作流实例
                DynEntity workflowInstance = designService.GetDynEntityByID("WorkflowInstance", context.GetValue(WorkflowInstanceID));
                int workflowID;
                if (workflowInstance != null)
                {
                    workflowID = Convert.ToInt32(workflowInstance["WorkflowID"]);
                }
                else
                {
                    throw new ApplicationException(string.Format("{0}实例下的活动{1}，根据工作流实例ID获取工作流实例不正常", context.GetValue(WorkflowInstanceID), DisplayName));
                }

                //添加待做任务清单
                DynEntity task = new DynEntity("WorkflowToDoList");
                task["WorkflowToDoListID"] = designService.GetNextID("WorkflowToDoList");
                //task["Actor"] = context.GetValue(Actor);
                
                        

                task["WorkflowToDoListName"] = DisplayName;
                task["WorkflowID"] = workflowID;
                task["WorkflowInstanceID"] = context.GetValue(WorkflowInstanceID);
                task["WorkflowActivityInstanceID"] = workflowfActivityInstance["WorkflowActivityInstanceID"];                
                task["BookmarkName"] = workflowfActivityInstance["WorkflowActivityInstanceID"].ToString();              
                task["State"] = "待处理";

                if (inParams.ContainsKey("ObjType"))
                {
                    task["ObjType"] = inParams["ObjType"].ToString();
                }
                if (inParams.ContainsKey("ObjID"))
                {
                    task["ObjID"] = inParams["ObjID"].ToString();
                }
                designService.AddDynEntity(task);
                context.CreateBookmark(workflowfActivityInstance["WorkflowActivityInstanceID"].ToString(), new BookmarkCallback(BookmarkCallbackMethod));

                trans.Complete();
            }
        }

        void BookmarkCallbackMethod(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            if (obj != null && obj is Dictionary<string, object>)
            {
                Dictionary<string, object> outParams = obj as Dictionary<string, object>;

                //if (outParams.ContainsKey("Actor"))
                //{
                //    this.Actor.Set(context, outParams["Actor"].ToString());
                //    outParams.Remove("Actor");
                //}

                Dictionary<string, object> inParams = this.ExchangeParams.Get(context) as Dictionary<string, object>;

                foreach (string key in outParams.Keys)
                {
                    inParams[key] = outParams[key];
                }
                this.ExchangeParams.Set(context, inParams);
              
            }
        }
    }
}
