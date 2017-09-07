using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.Interface;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using Rock.DesignerModule.Views;
using Rock.Dyn.Core;
using System;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.DurableInstancing;
using System.Activities.Presentation;
using System.Activities.Presentation.Toolbox;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xaml;
using System.Xml;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    [Export]
    public class WorkflowDesignerViewModel : ViewModelBase
    {
        public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }
        private string _workflowName;
        private ToolboxControl _workflowToolboxControl;
        private UIElement _designerView;
        private UIElement _propertyContent;
        private UndoEngine _undoEngine;
        private bool _canUndo;
        private bool _canRedo;
        private string _editState;
        private DynObject Workflow;

        private WorkflowDesigner _designer;

        public string WorkflowName
        {
            get { return _workflowName; }
            set
            {
                _workflowName = value;
                this.OnPropertyChanged("WorkflowName");
            }
        }
        public ToolboxControl WorkflowToolboxControl
        {
            get { return _workflowToolboxControl; }
        }

        public UIElement DesignerView
        {
            get { return _designerView; }
            set
            {
                _designerView = value;
                this.OnPropertyChanged("DesignerView");
            }
        }
        public UIElement PropertyContent
        {
            get { return _propertyContent; }
            set
            {
                _propertyContent = value;
                this.OnPropertyChanged("PropertyContent");
            }
        }

        public bool CanUndo
        {
            get { return _canUndo; }
            set
            {
                _canUndo = value;
                this.OnPropertyChanged("CanUndo");
            }
        }
        public bool CanRedo
        {
            get { return _canRedo; }
            set
            {
                _canRedo = value;
                this.OnPropertyChanged("CanRedo");
            }
        }
        public string EditState
        {
            get { return _editState; }
            set { _editState = value; }
        }
        public ICommand OpenCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public DelegateCommand<Object> DeleteCommand { get; private set; }
        public DelegateCommand<Object> SaveCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand DebugCommand { get; private set; }
        public WorkflowDesignerViewModel()
        {
            (new DesignerMetadata()).Register();
            _workflowToolboxControl = new ToolboxControl() { Categories = WorkflowToolbox.LoadToolbox() };

            _designer = new WorkflowDesigner();
            _undoEngine = _designer.Context.Services.GetService<UndoEngine>();
            _undoEngine.UndoUnitAdded += delegate(object ss, UndoUnitEventArgs ee)
            {
                _designer.Flush();
                CanUndo = true;
            };
            DesignerView = _designer.View;
            PropertyContent = _designer.PropertyInspectorView;
            _designer.Text = "<Activity mc:Ignorable=\"sap\" x:Class=\"WorkflowInvokerSample.流程图\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\" xmlns:mv=\"clr-namespace:Microsoft.VisualBasic;assembly=System\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:s1=\"clr-namespace:System;assembly=System\" xmlns:s2=\"clr-namespace:System;assembly=System.Xml\" xmlns:s3=\"clr-namespace:System;assembly=System.Core\" xmlns:s4=\"clr-namespace:System;assembly=System.ServiceModel\" xmlns:sad=\"clr-namespace:System.Activities.Debugger;assembly=System.Activities\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:scg1=\"clr-namespace:System.Collections.Generic;assembly=System\" xmlns:scg2=\"clr-namespace:System.Collections.Generic;assembly=System.ServiceModel\" xmlns:scg3=\"clr-namespace:System.Collections.Generic;assembly=System.Core\" xmlns:sd=\"clr-namespace:System.Data;assembly=System.Data\" xmlns:sl=\"clr-namespace:System.Linq;assembly=System.Core\" xmlns:st=\"clr-namespace:System.Text;assembly=mscorlib\" xmlns:w=\"clr-namespace:WorkflowInvokerSample\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">\r\n  <x:Members>\r\n    <x:Property Name=\"WorkflowInstanceID\" Type=\"InArgument(x:Int32)\" />\r\n    <x:Property Name=\"ExchangeParams\" Type=\"InOutArgument(scg:Dictionary(x:String, x:Object))\" />\r\n    <x:Property Name=\"FirstActor\" Type=\"InArgument(x:String)\" />\r\n    <x:Property Name=\"LastActor\" Type=\"InOutArgument(x:String)\" />\r\n  </x:Members>\r\n  <sap:VirtualizedContainerService.HintSize>654,676</sap:VirtualizedContainerService.HintSize>\r\n  <mva:VisualBasic.Settings>Assembly references and imported namespaces for internal implementation</mva:VisualBasic.Settings>\r\n  <Flowchart DisplayName=\"流程图\" sad:XamlDebuggerXmlReader.FileName=\"C:\\Users\\Administrator\\Downloads\\WorkflowInvokerSample\\WorkflowInvokerSample\\流程图.xaml\" sap:VirtualizedContainerService.HintSize=\"614,636\">\r\n    <sap:WorkflowViewStateService.ViewState>\r\n      <scg:Dictionary x:TypeArguments=\"x:String, x:Object\">\r\n        <x:Boolean x:Key=\"IsExpanded\">False</x:Boolean>\r\n        <av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point>\r\n        <av:Size x:Key=\"ShapeSize\">60,75</av:Size>\r\n        <av:PointCollection x:Key=\"ConnectorLocation\">300,77.5 300,107.5 300,189</av:PointCollection>\r\n      </scg:Dictionary>\r\n    </sap:WorkflowViewStateService.ViewState>\r\n    <Flowchart.StartNode>\r\n      <x:Null />\r\n    </Flowchart.StartNode>\r\n  </Flowchart>\r\n</Activity>";
            _designer.Load();
            WorkflowName = "新建工作流";
            CanUndo = false;
            CanRedo = false;
            EditState = "add";

            OpenCommand = new DelegateCommand<object>(OpenWorkflowListWindow);
            AddCommand = new DelegateCommand<object>(Add);
            DeleteCommand = new DelegateCommand<object>(Delete, CanDeleteWorkflowExecute);
            SaveCommand = new DelegateCommand<object>(Save);
            UndoCommand = new DelegateCommand<object>(Undo);
            RedoCommand = new DelegateCommand<object>(Redo);
            DebugCommand = new DelegateCommand<object>(Debug);
        }

        private bool CanDeleteWorkflowExecute(object arg)
        {
            if (EditState == "modify")
            {
                return true;
            }
            return false;
        }

        public void OpenWorkflow(int workflowID)
        {
            Workflow = SystemService.GetDynObjectByID("Workflow", workflowID);
            if (Workflow != null)
            {
                _designer = new WorkflowDesigner();
                _undoEngine = _designer.Context.Services.GetService<UndoEngine>();
                _undoEngine.UndoUnitAdded += delegate(object ss, UndoUnitEventArgs ee)
                {
                    _designer.Flush();
                    CanUndo = true;
                };

                DesignerView = _designer.View;
                PropertyContent = _designer.PropertyInspectorView;
                _designer.Text = Workflow["Definition"].ToString();
                _designer.Load();
                WorkflowName = Workflow["WorkflowName"].ToString();
                EditState = "modify";
                CanUndo = false;
                CanRedo = false;
                DeleteCommand.RaiseCanExecuteChanged();
            }
            else
            {
                MessageBox.Show("所选的工作流在数据库中不存在,请检查!");
            }
        }
        private void OpenWorkflowListWindow(object arg)
        {
            WorkflowOpenWindow workflowOpenWindow = new WorkflowOpenWindow();
            workflowOpenWindow.ViewModel.LoadWorkflow();
            workflowOpenWindow.ShowDialog();
        }
        private void Add(object arg)
        {
            _designer = new WorkflowDesigner();
            _undoEngine = _designer.Context.Services.GetService<UndoEngine>();
            _undoEngine.UndoUnitAdded += delegate(object ss, UndoUnitEventArgs ee)
            {
                _designer.Flush();
                CanUndo = true;
            };

            DesignerView = _designer.View;
            PropertyContent = _designer.PropertyInspectorView;
            _designer.Text = "<Activity mc:Ignorable=\"sap\" x:Class=\"WorkflowInvokerSample.流程图\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\" xmlns:mv=\"clr-namespace:Microsoft.VisualBasic;assembly=System\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:s1=\"clr-namespace:System;assembly=System\" xmlns:s2=\"clr-namespace:System;assembly=System.Xml\" xmlns:s3=\"clr-namespace:System;assembly=System.Core\" xmlns:s4=\"clr-namespace:System;assembly=System.ServiceModel\" xmlns:sad=\"clr-namespace:System.Activities.Debugger;assembly=System.Activities\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:scg1=\"clr-namespace:System.Collections.Generic;assembly=System\" xmlns:scg2=\"clr-namespace:System.Collections.Generic;assembly=System.ServiceModel\" xmlns:scg3=\"clr-namespace:System.Collections.Generic;assembly=System.Core\" xmlns:sd=\"clr-namespace:System.Data;assembly=System.Data\" xmlns:sl=\"clr-namespace:System.Linq;assembly=System.Core\" xmlns:st=\"clr-namespace:System.Text;assembly=mscorlib\" xmlns:w=\"clr-namespace:WorkflowInvokerSample\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">\r\n  <x:Members>\r\n    <x:Property Name=\"WorkflowInstanceID\" Type=\"InArgument(x:Int32)\" />\r\n    <x:Property Name=\"ExchangeParams\" Type=\"InOutArgument(scg:Dictionary(x:String, x:Object))\" />\r\n    <x:Property Name=\"FirstActor\" Type=\"InArgument(x:String)\" />\r\n    <x:Property Name=\"LastActor\" Type=\"InOutArgument(x:String)\" />\r\n  </x:Members>\r\n  <sap:VirtualizedContainerService.HintSize>654,676</sap:VirtualizedContainerService.HintSize>\r\n  <mva:VisualBasic.Settings>Assembly references and imported namespaces for internal implementation</mva:VisualBasic.Settings>\r\n  <Flowchart DisplayName=\"流程图\" sad:XamlDebuggerXmlReader.FileName=\"C:\\Users\\Administrator\\Downloads\\WorkflowInvokerSample\\WorkflowInvokerSample\\流程图.xaml\" sap:VirtualizedContainerService.HintSize=\"614,636\">\r\n    <sap:WorkflowViewStateService.ViewState>\r\n      <scg:Dictionary x:TypeArguments=\"x:String, x:Object\">\r\n        <x:Boolean x:Key=\"IsExpanded\">False</x:Boolean>\r\n        <av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point>\r\n        <av:Size x:Key=\"ShapeSize\">60,75</av:Size>\r\n        <av:PointCollection x:Key=\"ConnectorLocation\">300,77.5 300,107.5 300,189</av:PointCollection>\r\n      </scg:Dictionary>\r\n    </sap:WorkflowViewStateService.ViewState>\r\n    <Flowchart.StartNode>\r\n      <x:Null />\r\n    </Flowchart.StartNode>\r\n  </Flowchart>\r\n</Activity>";
            _designer.Load();
            WorkflowName = "新建工作流";
            EditState = "add";
            CanUndo = false;
            CanRedo = false;
            DeleteCommand.RaiseCanExecuteChanged();
        }
        private void Delete(object arg)
        {
            if (Workflow != null)
            {
                SystemService.DeleteObjectByID("Workflow", Convert.ToInt32(Workflow["WorkflowID"]));
                MessageBox.Show("当前工作流已经从数据库中删除!");
                Add(null);
            }
            else
            {
                MessageBox.Show("当前工作流对象为空,不能删除!");
            }
        }
        private void Save(object arg)
        {
            switch (EditState)
            {
                case "add":
                    string count = SystemService.ExecuteScalar("select count(*) from Workflow where WorkflowName = '" + WorkflowName + "'");
                    if (Convert.ToInt32(count) == 0)
                    {
                        Workflow = new DynObject("Workflow");
                        Workflow["WorkflowID"] = SystemService.GetNextID("Workflow");
                        Workflow["WorkflowName"] = WorkflowName;
                        Workflow["Definition"] = _designer.Text;
                        Workflow["RuntimeDefinition"] = RemoveViewState(_designer.Text);
                        Workflow["State"] = "已保存";
                        SystemService.AddDynObject(Workflow);
                        MessageBox.Show("当前工作流已经成功新增到数据库!");
                        EditState = "modify";
                    }
                    else
                    {
                        MessageBox.Show("工作流名称:" + WorkflowName + "  已经存在,请检查!");
                        return;
                    }
                    break;
                case "modify":
                    Workflow["WorkflowName"] = WorkflowName;
                    Workflow["Definition"] = _designer.Text;
                    Workflow["RuntimeDefinition"] = RemoveViewState(_designer.Text);
                    SystemService.ModifyDynObject(Workflow);
                    MessageBox.Show("当前工作流已经成功更新到数据库!");
                    break;
                default:
                    break;
            }
            DeleteCommand.RaiseCanExecuteChanged();
        }
        private void Undo(object arg)
        {
            CanUndo = this._designer.Context.Services.GetService<UndoEngine>().Undo();
            if (CanUndo)
            {
                CanRedo = true;
            }
        }

        private void Redo(object arg)
        {
            CanRedo = this._designer.Context.Services.GetService<UndoEngine>().Redo();
            if (CanRedo)
            {
                CanUndo = true;
            }
        }

        private string RemoveViewState(string xaml)
        {
            string xamlString = "";
            try
            {
                StringReader stringReader = new StringReader(xaml);
                XamlXmlReader xamlXmlReader = new XamlXmlReader(stringReader);
                XamlReader xamlReader = ActivityXamlServices.CreateBuilderReader(xamlXmlReader);
                ActivityBuilder ab = XamlServices.Load(ActivityXamlServices.CreateBuilderReader(xamlReader)) as ActivityBuilder;

                XmlWriterSettings writerSettings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };

                using (MemoryStream ms = new MemoryStream())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(ms, writerSettings))
                    {
                        XamlXmlWriter xamlWriter = new XamlXmlWriter(xmlWriter, new XamlSchemaContext());
                        XamlServices.Save(new ViewStateCleaningWriter(ActivityXamlServices.CreateBuilderWriter(xamlWriter)), ab);

                        xamlString = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
                return xamlString;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("去除ViewState异常:{0}", ex.Message));
            }
        }


        private void Debug(object arg)
        {
            StartWorkFlow();
            
        }
        private void StartWorkFlow()
        {
            //准备参数
            Dictionary<string, object> exchangeParams = new Dictionary<string, object>();
            exchangeParams.Add("ObjType", "MotorMaintenanceRecord");
            exchangeParams.Add("ObjID", 198);
            exchangeParams.Add("Actor", 1);
            int workflowInstanceID = SystemService.GetNextID("WorkflowInstance");
            WorkflowApplication workflowApplication = null;
            SqlWorkflowInstanceStore instanceStore = null;
            InstanceView view;

            //加载并运行工作流
            byte[] bs = Encoding.UTF8.GetBytes(RemoveViewState(_designer.Text));
            Activity activity = null;

            using (MemoryStream memoryStream = new MemoryStream(bs))
            {
                activity = ActivityXamlServices.Load(memoryStream);
            }
            Dictionary<string, object> workflowParams = new Dictionary<string, object>();
            workflowParams = new Dictionary<string, object>();
            workflowParams.Add("WorkflowInstanceID", workflowInstanceID);
            workflowParams.Add("ExchangeParams", exchangeParams);

            workflowApplication = new WorkflowApplication(activity, workflowParams);

            if (instanceStore == null)
            {
                string connectionString = "Data Source=.\\MSSQL2008R2;Initial Catalog=MotorMaintenance;Integrated Security=True";
                instanceStore = new SqlWorkflowInstanceStore(connectionString);
                view = instanceStore.Execute(instanceStore.CreateInstanceHandle(), new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(30));
                instanceStore.DefaultInstanceOwner = view.InstanceOwner;
            }

            //添加工作流实例
            Rock.Orm.Common.DynEntity workflowInstance = new Rock.Orm.Common.DynEntity("WorkflowInstance");
            workflowInstance["WorkflowInstanceID"] = workflowInstanceID;
            workflowInstance["WorkflowID"] = 2;
            workflowInstance["StartTime"] = DateTime.Now;
            workflowInstance["InstanceGUID"] = workflowApplication.Id.ToString();
            workflowInstance["State"] = "已保存";
            if (exchangeParams.ContainsKey("ObjType"))
            {
                workflowInstance["ObjType"] = exchangeParams["ObjType"].ToString();
            }
            if (exchangeParams.ContainsKey("ObjID"))
            {
                workflowInstance["ObjID"] = exchangeParams["ObjID"].ToString();
            }
            if (exchangeParams.ContainsKey("Actor"))
            {
                workflowInstance["Actor"] = Convert.ToInt32(exchangeParams["Actor"]);
            }
            if (exchangeParams.ContainsKey("Command"))
            {
                workflowInstance["Command"] = exchangeParams["Command"].ToString();
            }

            Rock.Orm.Data.GatewayFactory.Default.Save(workflowInstance);

            workflowApplication.InstanceStore = instanceStore;
            workflowApplication.PersistableIdle = workflowPersistableIdle;
            workflowApplication.Idle = workflowIdle;
            workflowApplication.Unloaded = unload;
            workflowApplication.Aborted = workflowAborted;
            workflowApplication.Completed = workflowCompleted;
            workflowApplication.OnUnhandledException = workflowUnhandledException;
            workflowApplication.Run();
        }

        private void ResumeBookmark()
        {
            ////准备参数
            //Dictionary<string, object> exchangeParams = new Dictionary<string, object>();
            //exchangeParams.Add("ObjType", "MotorMaintenanceRecord");
            //exchangeParams.Add("ObjID", 198);
            //exchangeParams.Add("Actor", 1);

            //WorkflowApplication workflowApplication = null;
            //SqlWorkflowInstanceStore instanceStore = null;
            //InstanceView view;


            //Rock.Orm.Common.DynEntity workflowToDoEntity = Rock.Orm.Data.GatewayFactory.Default.Find("WorkflowToDoList", 1);
            //Rock.Orm.Common.DynEntity workflowInstance = null;
            //if (workflowToDoEntity != null)
            //{
            //    workflowInstance = Rock.Orm.Data.GatewayFactory.Default.Find("WorkflowInstance", Convert.ToInt32(workflowToDoEntity["WorkflowInstanceID"]));
            //    string bookmarkName = workflowToDoEntity["BookmarkName"].ToString();
            //    if (workflowInstance != null)
            //    {
            //        string definition = ExecuteScalar("select RuntimeDefinition from Workflow where WorkflowID = " + workflowToDoEntity["WorkflowID"]);
            //        //加载并运行工作流
            //        byte[] bs = Encoding.UTF8.GetBytes(definition);
            //        Activity activity = null;
            //        using (MemoryStream memoryStream = new MemoryStream(bs))
            //        {
            //            activity = ActivityXamlServices.Load(memoryStream);
            //        }

            //        workflowApplication = new WorkflowApplication(activity);

            //        if (instanceStore == null)
            //        {
            //            string connectionString = GatewayFactory.DefaultConnectionString;
            //            instanceStore = new SqlWorkflowInstanceStore(connectionString);
            //            view = instanceStore.Execute(instanceStore.CreateInstanceHandle(), new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(30));
            //            instanceStore.DefaultInstanceOwner = view.InstanceOwner;
            //        }

            //        workflowApplication.InstanceStore = instanceStore;
            //        workflowApplication.PersistableIdle = workflowPersistableIdle;
            //        workflowApplication.Idle = workflowIdle;
            //        workflowApplication.Unloaded = unload;
            //        workflowApplication.Aborted = workflowAborted;
            //        workflowApplication.Completed = workflowCompleted;
            //        workflowApplication.OnUnhandledException = workflowUnhandledException;
            //        Guid guid = new Guid(workflowInstance["InstanceGUID"].ToString());

            //        try
            //        {
            //            workflowApplication.Load(guid);
            //        }
            //        catch (Exception ex)
            //        {
            //            throw ex;
            //        }

            //        if (workflowApplication != null)
            //        {
            //            if (workflowApplication.GetBookmarks().Count(p => p.BookmarkName == bookmarkName) == 1)
            //            {
            //                DynEntity workflowActivityInstance = GatewayFactory.Default.Find("WorkflowActivityInstance", Convert.ToInt32(workflowToDoEntity["WorkflowActivityInstanceID"]));
            //                if (workflowActivityInstance != null)
            //                {
            //                    foreach (string item in exchangeParamLists)
            //                    {
            //                        string[] arg = item.Split('^');

            //                        switch (arg[0])
            //                        {
            //                            case "Handler":
            //                                workflowActivityInstance["Handler"] = arg[1];
            //                                break;
            //                            case "ObjType":
            //                                workflowActivityInstance["ObjType"] = arg[1];
            //                                break;
            //                            case "ObjID":
            //                                workflowActivityInstance["ObjID"] = arg[1];
            //                                break;
            //                            case "DetailID":
            //                                workflowActivityInstance["DetailID"] = arg[1];
            //                                break;
            //                            case "Opinion":
            //                                workflowActivityInstance["Opinion"] = arg[1];
            //                                break;
            //                            default:
            //                                exchangeParams.Add(arg[0], arg[1]);
            //                                break;
            //                        }
            //                    }
            //                    exchangeParams["Actor"] = userID;
            //                    workflowApplication.ResumeBookmark(bookmarkName, exchangeParams);

            //                    workflowActivityInstance["State"] = "已执行";
            //                    workflowActivityInstance["EndTime"] = DateTime.Now;

            //                    using (TransactionScope trans = new TransactionScope())
            //                    {
            //                        GatewayFactory.Default.Save(workflowActivityInstance);
            //                        GatewayFactory.Default.Delete("WorkflowToDoList", workflowToDoListID);

            //                        //更新状态字段
            //                        if (exchangeParams.ContainsKey("ObjType") && exchangeParams.ContainsKey("ObjID") && exchangeParams.ContainsKey("StateField") && exchangeParams.ContainsKey("StateValue"))
            //                        {
            //                            string sqlString = "UPDATE [" + exchangeParams["ObjType"] + "] SET [" + exchangeParams["StateField"] + "] = " + exchangeParams["StateValue"] + " WHERE " + exchangeParams["ObjType"] + "ID = " + exchangeParams["ObjID"];
            //                            ExcuteNoneReturnQuery("sqlString");
            //                        }
            //                        trans.Complete();
            //                    }
            //                }
            //                else
            //                {
            //                    throw new ApplicationException(string.Format("在工作流活动:{0} 不存在无法执行,请检查", workflowActivityInstance["WorkflowActivityInstanceName"], bookmarkName));
            //                }
            //            }
            //            else
            //            {
            //                throw new ApplicationException(string.Format("在工作流实例{0}中找不到书签名为{1}的活动", workflowToDoEntity["WorkflowInstanceID"], bookmarkName));
            //            }
            //        }
            //        else
            //        {
            //            throw new ApplicationException("没有创建实例");
            //        }
            //    }
            //}
            //else
            //{
            //    throw new ApplicationException("当前待做任务在数据库中不存在,请检查!");
            //}

        }

        private PersistableIdleAction workflowPersistableIdle(WorkflowApplicationIdleEventArgs e)
        {
            return PersistableIdleAction.Unload;
        }

        private void workflowIdle(WorkflowApplicationIdleEventArgs e)
        {
            var xx = 8;
        }

        private void unload(WorkflowApplicationEventArgs e)
        {
            var xx = 8;
        }

        private void workflowAborted(WorkflowApplicationAbortedEventArgs e)
        {
            var xx = 8;
        }

        private void workflowCompleted(WorkflowApplicationCompletedEventArgs e)
        {
            var xx = 8;
        }

        private UnhandledExceptionAction workflowUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
        {
            return UnhandledExceptionAction.Cancel;
        }
    }
}
