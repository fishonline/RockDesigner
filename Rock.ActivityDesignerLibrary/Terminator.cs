using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Activities.Presentation;
using System.Windows;
using Microsoft.CSharp.Activities;
using System.Transactions;
using Rock.Orm.Common;
using Microsoft.VisualBasic.Activities;

namespace Rock.ActivityDesignerLibrary
{
    [Designer(typeof(TerminatorDesigner))]
    public sealed class Terminator : NativeActivity<string>, IActivityTemplateFactory
    {
        [Description("工作流实例ID")]
        [Browsable(true)]
        public InArgument<int> WorkflowInstanceID { get; set; }

        [Description("参数")]
        [RequiredArgument]
        [Browsable(true)]
        public InOutArgument<Dictionary<string, object>> ExchangeParams { get; set; }

        public Activity Create(DependencyObject target)
        {
            Terminator terminator = new Terminator();
            terminator.DisplayName = "流程结束";
            terminator.WorkflowInstanceID = new VisualBasicValue<int> { ExpressionText = "WorkflowInstanceID" };
            terminator.ExchangeParams = new VisualBasicReference<Dictionary<string, object>> { ExpressionText = "ExchangeParams" };
            return terminator;
        }
        protected override void Execute(NativeActivityContext context)
        {
            //DesignService designService = new DesignService();
            ////启动事务
            //using (TransactionScope trans = new TransactionScope())
            //{
            //    Dictionary<string, object> inParams = this.ExchangeParams.Get(context) as Dictionary<string, object>;
            //    if (inParams.ContainsKey("TableName") && inParams.ContainsKey("TableKey"))
            //    {
            //        designService.TerminateWorkflow(inParams["TableName"].ToString(), inParams["TableKey"].ToString());
            //    }

            //    //获取工作流实例
            //    DynEntity workflowInstance = designService.GetDynEntityByID("WorkflowInstance", context.GetValue(WorkflowInstanceID));
            //    int workflowID;
            //    if (workflowInstance != null)
            //    {
            //        workflowID = Convert.ToInt32(workflowInstance["WorkflowID"]);
            //    }
            //    else
            //    {
            //        throw new ApplicationException(string.Format("{0}实例下的活动{1}，根据工作流实例ID获取工作流实例不正常", context.GetValue(WorkflowInstanceID), DisplayName));
            //    }

            //    workflowInstance["EndTime"] = DateTime.Now;
            //    workflowInstance["State"] = "已结束";
            //    designService.ModifyDynEntity(workflowInstance);

            //    trans.Complete();
            //}
        }
    }
}
