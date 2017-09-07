using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Rock.Common;
using Rock.DesignerModule.Interface;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using Rock.DesignerModule.Views;
using Rock.Dyn.Core;
using Rock.Orm.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    [Export]
    public class DesignerViewModel : ViewModelBase
    {       
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }
        private ObservableCollection<TreeNode> _applicationSystemTreeDataSource = new ObservableCollection<TreeNode>();
        private ObservableCollection<DesignClass> _designClassDataSource = new ObservableCollection<DesignClass>();
        private Dictionary<int, ObservableCollection<DesignClass>> designClassCache = new Dictionary<int, ObservableCollection<DesignClass>>();
        private TreeNode rootNode;
        private TreeNode _selectedTreeNode;
        private DesignClass _currentDesignClass;
        private DesignProperty _currentdesignProperty;
        private DesignMethod _currentDesignMethod;
        private DesignMethod _selectedDesignMethod;
      
        private DesignMethodParameter _currentDesignMethodParameter;              
        private int _tabSelectedIndex;
        private Control _classInfoView = null;        
       
        public ObservableCollection<TreeNode> ApplicationSystemTreeDataSource
        {
            get
            {
                return _applicationSystemTreeDataSource;
            }
            set
            {
                _applicationSystemTreeDataSource = value;
                this.OnPropertyChanged("ApplicationSystemTreeDataSource");
            }
        }
        public ObservableCollection<DesignClass> DesignClassDataSource
        {
            get { return _designClassDataSource; }
            set { _designClassDataSource = value; this.OnPropertyChanged("DesignClassDataSource"); }
        }
        public TreeNode SelectedTreeNode
        {
            get { return _selectedTreeNode; }
            set { _selectedTreeNode = value; this.OnPropertyChanged("SelectedTreeNode"); }
        }
        public DesignProperty CurrentdesignProperty
        {
            get { return _currentdesignProperty; }
            set { _currentdesignProperty = value; this.OnPropertyChanged("CurrentdesignProperty"); }
        }
        public DesignClass CurrentDesignClass
        {
            get { return _currentDesignClass; }
            set { _currentDesignClass = value; this.OnPropertyChanged("CurrentDesignClass"); }
        }
       
        public DesignMethod CurrentDesignMethod
        {
            get { return _currentDesignMethod; }
            set { _currentDesignMethod = value; this.OnPropertyChanged("CurrentDesignMethod"); }
        }
        public DesignMethod SelectedDesignMethod
        {
            get { return _selectedDesignMethod; }
            set { _selectedDesignMethod = value; this.OnPropertyChanged("SelectedDesignMethod"); }
        }
        public DesignMethodParameter CurrentDesignMethodParameter
        {
            get { return _currentDesignMethodParameter; }
            set { _currentDesignMethodParameter = value; this.OnPropertyChanged("CurrentDesignMethodParameter"); }
        }       
        public int TabSelectedIndex
        {
            get { return _tabSelectedIndex; }
            set
            {
                if (_tabSelectedIndex != value)
                {
                    _tabSelectedIndex = value;
                    this.OnPropertyChanged("TabSelectedIndex");
                }
            }
        }
        public Control ClassInfoView
        {
            get { return _classInfoView; }
            set
            {
                _classInfoView = value;
                this.OnPropertyChanged("ClassInfoView");
            }
        }
        public ICommand MouseUpCommand { get; private set; }
        public DelegateCommand<object> SaveClassCommand { get; private set; }
        public DelegateCommand<string> AddClassCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand EditClassCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand DeleteClassCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand ClassCheckedCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand ClassUncheckedCommand { get; private set; }
        public ICommand ClassRowActivatedCommand { get; private set; }
        public ICommand SelectAllClassCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand GenerateStaticEntityCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand AddPropertyCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand EditPropertyCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand DeletePropertyCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand UIInfoEditCommand { get; private set; }
        public DelegateCommand<string> PropertyCheckedCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand PropertyUncheckedCommand { get; private set; }
        public ICommand PropertyRowActivatedCommand { get; private set; }

        public Microsoft.Practices.Prism.Commands.DelegateCommand AddMethodCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand EditMethodCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand DeleteMethodCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand MethodCheckedCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand MethodUncheckedCommand { get; private set; }
        public ICommand MethodRowActivatedCommand { get; private set; }

        public Microsoft.Practices.Prism.Commands.DelegateCommand AddParameterCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand EditParameterCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand DeleteParameterCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand ParameterCheckedCommand { get; private set; }
        public Microsoft.Practices.Prism.Commands.DelegateCommand ParameterUncheckedCommand { get; private set; }
        public ICommand ParameterRowActivatedCommand { get; private set; }
        //
        public DesignerViewModel()
        {
            //类型部分
            MouseUpCommand = new DelegateCommand<object>(TreeNode_Click);
            SaveClassCommand = new DelegateCommand<object>(SaveClass, CanSaveClassExecute);
            AddClassCommand = new DelegateCommand<string>(AddClass, CanAddClassExecute);
            EditClassCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(EditClass, CanEditClassExecute);
            DeleteClassCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(DeleteClass, CanDeleteClassExecute);
            ClassCheckedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(ClassChecked);
            ClassUncheckedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(ClassUnchecked);
            ClassRowActivatedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(ClassRowActivate);
            SelectAllClassCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(SelectAllClass);
            GenerateStaticEntityCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(GenerateStaticEntity, CanGenerateStaticEntityExecute);
            //属性部分
            AddPropertyCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(AddProperty, CanAddPropertyExecute);
            EditPropertyCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(EditProperty, CanEditPropertyExecute);
            DeletePropertyCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(DeleteProperty, CanDeletePropertyExecute);
            UIInfoEditCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(EditUIInfo, CanEditUIInfoExecute);
            PropertyCheckedCommand = new DelegateCommand<string>(PropertyChecked);
            PropertyUncheckedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(PropertyUnchecked);
            PropertyRowActivatedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(PropertyRowActivate);
            //方法部分
            AddMethodCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(AddMethod, CanAddMethodExecute);
            EditMethodCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(EditMethod, CanEditMethodExecute);
            DeleteMethodCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(DeleteMethod, CanDeleteMethodExecute);
            MethodCheckedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(MethodChecked);
            MethodUncheckedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(MethodUnchecked);
            MethodRowActivatedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(MethodRowActivate);
            //参数部分
            AddParameterCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(AddParameter, CanAddParameterExecute);
            EditParameterCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(EditParameter, CanEditParameterExecute);
            DeleteParameterCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(DeleteParameter, CanDeleteParameterExecute);
            ParameterCheckedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(ParameterChecked);
            ParameterUncheckedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(ParameterUnchecked);
            ParameterRowActivatedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(ParameterRowActivate);

        }
        #region 类型部分
        private bool CanSaveClassExecute(object arg)
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.State == "added")
                {
                    return true;
                }
                foreach (var item in CurrentDesignClass.Properties)
                {
                    if (item.State == "added" || item.State == "modified")
                    {
                        return true;
                    }
                }
                foreach (var methode in CurrentDesignClass.Methodes)
                {
                    if (methode.State == "added" || methode.State == "modified")
                    {
                        return true;
                    }
                    foreach (var parameter in methode.Parameters)
                    {
                        if (parameter.State == "added" || parameter.State == "modified")
                        {
                            return true;
                        }
                    }
                }
                if (CurrentDesignClass.DeletedProperties.Count > 0)
                {
                    return true;
                }
                if (CurrentDesignClass.DeletedMethodes.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CanAddClassExecute(object arg)
        {
            if (SelectedTreeNode != null)
            {
                if (SelectedTreeNode.Type == "Module")
                {
                    return true;
                }
            }
            return false;
        }
        private bool CanEditClassExecute()
        {
            if (DesignClassDataSource != null)
            {
                if (DesignClassDataSource.Where(item => item.IsChecked).ToList().Count == 1)
                {
                    DesignClass designClass = DesignClassDataSource.Where(item => item.IsChecked).FirstOrDefault();
                    if (designClass.MainType == 2)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        private bool CanDeleteClassExecute()
        {
            if (DesignClassDataSource != null)
            {
                if (DesignClassDataSource.Where(item => item.IsChecked).ToList().Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CanGenerateStaticEntityExecute()
        {
            if (DesignClassDataSource != null)
            {
                if (DesignClassDataSource.Where(item => item.IsChecked && (item.MainType == 0 || item.MainType == 2)).ToList().Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
        public void TreeNode_Click(Object arg)
        {
            if (SelectedTreeNode == null)
            {
                return;
            }
            SelectedTreeNode.IsExpanded = true;
            switch (SelectedTreeNode.Type)
            {
                case "Application":
                    DesignClassDataSource = null;
                    break;
                case "Module":
                    int moduleID = designClassCache.Keys.Where(key => key == SelectedTreeNode.TreeNodeID).FirstOrDefault();
                    if (moduleID > 0)
                    {
                        DesignClassDataSource = designClassCache[moduleID];
                    }
                    else
                    {
                        designClassCache[SelectedTreeNode.TreeNodeID] = ApplicationDesignService.GetModuleClassCollection(SelectedTreeNode.TreeNodeID);
                        DesignClassDataSource = null;
                    }
                    TabSelectedIndex = 0;
                    //判断下级有没有未保存的类如果有就删除
                    List<TreeNode> treeNodelist = SelectedTreeNode.Children.Where(item => item.TreeNodeID == -1).ToList();
                    foreach (var item in treeNodelist)
                    {
                        DesignClass designClass = item.Tag as DesignClass;
                        if (designClass.State == "added")
                        {
                            SelectedTreeNode.Children.Remove(item);
                            DesignClassDataSource.Remove(designClass);
                            designClassCache.Remove(designClass.ClassID);
                        }
                    }
                    //处理其他模块
                    foreach (var moduleTreeNode in rootNode.Children)
                    {
                        if (moduleTreeNode.TreeNodeID != SelectedTreeNode.TreeNodeID)
                        {
                            List<TreeNode> otherTreeNodelist = moduleTreeNode.Children.Where(otherTreeNode => otherTreeNode.TreeNodeID == -1).ToList();
                            foreach (var otherTreeNode in otherTreeNodelist)
                            {
                                DesignClass designClass = otherTreeNode.Tag as DesignClass;
                                if (designClass.State == "added")
                                {
                                    moduleTreeNode.Children.Remove(otherTreeNode);
                                    DesignClassDataSource.Remove(designClass);
                                    designClassCache[moduleTreeNode.TreeNodeID].Remove(designClass);
                                }
                            }
                        }
                    }
                    CurrentDesignClass = null;
                    CurrentDesignMethod = null;
                    CurrentdesignProperty = null;
                    CurrentDesignMethodParameter = null;
                    SaveClassCommand.RaiseCanExecuteChanged();
                    AddClassCommand.RaiseCanExecuteChanged();
                    EditClassCommand.RaiseCanExecuteChanged();
                    DeleteClassCommand.RaiseCanExecuteChanged();
                    GenerateStaticEntityCommand.RaiseCanExecuteChanged();
                    break;
                default:
                    DesignClassDataSource = null;
                    CurrentDesignClass = SelectedTreeNode.Tag as DesignClass;
                    //构造属性和方法集合
                    if (CurrentDesignClass.State != "added")
                    {
                        CurrentDesignClass.Properties = ApplicationDesignService.GetClassDesignPropertyCollection(CurrentDesignClass.ClassID);
                        CurrentDesignClass.Methodes = ApplicationDesignService.GetClassDesignMethodCollection(CurrentDesignClass.ClassID);
                    }                   
                    switch (CurrentDesignClass.MainType)
                    {
                        case 0:
                            EntityClassInfoView entityClassInfoView = new EntityClassInfoView();
                            entityClassInfoView.ViewModel.DesignClass = CurrentDesignClass;
                            entityClassInfoView.ViewModel.NamespaceName = ApplicationDesignCache.NamespaceSource.Where(item => item.NamespaceID == CurrentDesignClass.NamespaceID).FirstOrDefault().NamespaceName;
                            ClassInfoView = entityClassInfoView;
                            TabSelectedIndex = 1;
                            break;
                        case 1:
                            ControlClassInfoView controlClassInfoView = new ControlClassInfoView();
                            controlClassInfoView.ViewModel.DesignClass = CurrentDesignClass;
                            controlClassInfoView.ViewModel.NamespaceName = ApplicationDesignCache.NamespaceSource.Where(item => item.NamespaceID == CurrentDesignClass.NamespaceID).FirstOrDefault().NamespaceName;
                            ClassInfoView = controlClassInfoView;
                            TabSelectedIndex = 2;
                            break;
                        case 2:
                            RelationClassInfoView relationClassInfoView = new RelationClassInfoView();
                            CurrentDesignClass.RelationPropertyA = CurrentDesignClass.Properties[0];
                            CurrentDesignClass.RelationPropertyB = CurrentDesignClass.Properties[1];
                            relationClassInfoView.ViewModel.DesignClass = CurrentDesignClass;
                            relationClassInfoView.ViewModel.NamespaceName = ApplicationDesignCache.NamespaceSource.Where(item => item.NamespaceID == CurrentDesignClass.NamespaceID).FirstOrDefault().NamespaceName;
                            ClassInfoView = relationClassInfoView;
                            TabSelectedIndex = 1;
                            break;
                        default:
                            break;
                    }
                    break;
            }
            SaveClassCommand.RaiseCanExecuteChanged();
            AddClassCommand.RaiseCanExecuteChanged();
            AddPropertyCommand.RaiseCanExecuteChanged();
            AddMethodCommand.RaiseCanExecuteChanged();
        }
        public void SaveClass(object parameter)
        {
            //判断是否可以保存
            switch (CurrentDesignClass.MainType)
            {
                case 0:
                    if (CurrentDesignClass.Properties.Count == 0)
                    {
                        MessageBox.Show("实体类至少要有一个属性!", "提示");
                        return;
                    }
                    break;
                case 1:
                    if (CurrentDesignClass.Methodes.Count == 0)
                    {
                        MessageBox.Show("控制类至少要有一个方法!", "提示");
                        return;
                    }
                    break;
            }
            if (CurrentDesignClass.State == "added")
            {
                ApplicationDesignService.AddDesignClass(CurrentDesignClass);
                CurrentDesignClass.State = "normal";
                //更新TreeNo的节点ID
                SelectedTreeNode.TreeNodeID = CurrentDesignClass.ClassID;               
            }
            else
            {
                ApplicationDesignService.ModifyDesignClass(CurrentDesignClass);
            }
            SaveClassCommand.RaiseCanExecuteChanged();
            //更新StructSource
            if(!ApplicationDesignCache.StructSource.Contains(CurrentDesignClass.ClassName))
            { 
                ApplicationDesignCache.StructSource.Add(CurrentDesignClass.ClassName); 
            }
            
            MessageBox.Show("保存成功");
        }
        public void AddClass(string classType)
        {
            switch (classType)
            {
                case "Entity":
                    EntityClassView entityClassView = new EntityClassView();
                    entityClassView.ViewModel.EditState = "add";
                    entityClassView.ViewModel.MainTypeName = "EntityClass";
                    entityClassView.ViewModel.EntityClassInit();
                    entityClassView.btnOK.Content = "确 定";
                    entityClassView.Title = "添加实体类";
                    entityClassView.ShowDialog();
                    break;
                case "Control":
                    ControlClassView controlClassView = new ControlClassView();
                    controlClassView.ViewModel.EditState = "add";
                    controlClassView.ViewModel.MainTypeName = "ControlClass";
                    controlClassView.ViewModel.ControlClassInit();
                    controlClassView.Title = "添加控制类";
                    controlClassView.ShowDialog();
                    break;
                case "Relation":
                    RelationClassView relationClassView = new RelationClassView();
                    relationClassView.ViewModel.EditState = "add";
                    relationClassView.ViewModel.MainTypeName = "RelationClass";
                    relationClassView.ViewModel.RelationClassInit();
                    relationClassView.Title = "添加关联类";
                    relationClassView.ShowDialog();
                    break;
                default:
                    break;
            }
        }
        public void AddNewDesignClass()
        {
            //添加到designClassCache
            designClassCache[SelectedTreeNode.TreeNodeID].Add(CurrentDesignClass);
            TreeNode treeNode = new TreeNode() { TreeNodeID = CurrentDesignClass.ClassID, Type = CurrentDesignClass.MainTypeName, Tag = CurrentDesignClass, Name = CurrentDesignClass.ClassName, Parent = SelectedTreeNode, Image = CurrentDesignClass.MainTypeImage };
            SelectedTreeNode.Children.Add(treeNode);
            SelectedTreeNode = treeNode;
            TreeNode_Click(null);
        }
        public void EditClass()
        {
            CurrentDesignClass = DesignClassDataSource.Where(item => item.IsChecked).FirstOrDefault();
            switch (CurrentDesignClass.MainTypeName)
            {
                case "EntityClass":
                    EntityClassView entityClassView = new EntityClassView();
                    entityClassView.ViewModel.EditState = "modify";
                    entityClassView.ViewModel.DesignClass = CurrentDesignClass;
                    entityClassView.ViewModel.SelectedNameSpace = ApplicationDesignCache.NamespaceSource.Where(item => item.NamespaceID == CurrentDesignClass.NamespaceID).FirstOrDefault();
                    if (CurrentDesignClass.State == "added")
                    {
                        entityClassView.ViewModel.IsClassNameReadonly = false;
                        entityClassView.btnOK.Content = "确 定";
                    }
                    else
                    {
                        entityClassView.ViewModel.IsClassNameReadonly = true;
                        entityClassView.btnOK.Content = "保 存";
                    }
                    entityClassView.Title = "编辑实体类";
                    entityClassView.ShowDialog();
                    break;
                case "ControlClass":
                    break;
                case "RelationClass":
                    break;
                default:
                    break;
            }
        }
        private void DeleteClass()
        {
            MessageBoxResult result = MessageBox.Show("您确定要删除所选类型吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                List<DesignClass> checkedClasses = DesignClassDataSource.Where(item => item.IsChecked).ToList();
                //判断是否可以删除
                foreach (var item in checkedClasses)
                {
                    if (!ApplicationDesignService.CanDeleteClass(item.ClassName))
                    {
                        MessageBox.Show("您选择的Class:[" + item.ClassName + "]是其他对象的外键,不允许删除,请检查!", "提示");
                        return;
                    }
                }

                foreach (var checkedClass in checkedClasses)
                {
                    ApplicationDesignService.DeleteClass(checkedClass);
                    TreeNode node = SelectedTreeNode.Children.Where(item => item.TreeNodeID == checkedClass.ClassID).FirstOrDefault();
                    SelectedTreeNode.Children.Remove(node);
                    DesignClassDataSource.Remove(checkedClass);
                    //清空缓存
                    designClassCache.Remove(checkedClass.ClassID);
                }
                MessageBox.Show("删除成功！");
            }
        }
        private void ClassChecked()
        {
            EditClassCommand.RaiseCanExecuteChanged();
            DeleteClassCommand.RaiseCanExecuteChanged();
            GenerateStaticEntityCommand.RaiseCanExecuteChanged();
        }
        private void ClassUnchecked()
        {
            EditClassCommand.RaiseCanExecuteChanged();
            DeleteClassCommand.RaiseCanExecuteChanged();
            GenerateStaticEntityCommand.RaiseCanExecuteChanged();
        }
        public void ClassRowActivate()
        {
            if (CurrentDesignClass != null)
            {
                if (!CurrentDesignClass.IsChecked)
                {
                    CurrentDesignClass.IsChecked = true;
                    EditClassCommand.RaiseCanExecuteChanged();
                }
                switch (CurrentDesignClass.MainTypeName)
                {
                    case "EntityClass":
                        EntityClassView entityClassView = new EntityClassView();
                        entityClassView.ViewModel.EditState = "modify";
                        entityClassView.ViewModel.DesignClass = CurrentDesignClass;
                        entityClassView.ViewModel.SelectedNameSpace = ApplicationDesignCache.NamespaceSource.Where(item => item.NamespaceID == CurrentDesignClass.NamespaceID).FirstOrDefault();
                        if (CurrentDesignClass.State == "added")
                        {
                            entityClassView.ViewModel.IsClassNameReadonly = false;
                            entityClassView.btnOK.Content = "确 定";
                        }
                        else
                        {
                            entityClassView.ViewModel.IsClassNameReadonly = true;
                            entityClassView.btnOK.Content = "保 存";
                        }
                        entityClassView.Title = "编辑实体类";
                        entityClassView.ShowDialog();
                        break;
                    case "ControlClass":
                        ControlClassView controlClassView = new ControlClassView();
                        controlClassView.ViewModel.EditState = "modify";
                        controlClassView.ViewModel.DesignClass = CurrentDesignClass;
                        controlClassView.ViewModel.SelectedNameSpace = ApplicationDesignCache.NamespaceSource.Where(item => item.NamespaceID == CurrentDesignClass.NamespaceID).FirstOrDefault();
                        if (CurrentDesignClass.State == "added")
                        {
                            controlClassView.ViewModel.IsClassNameReadonly = false;
                            controlClassView.btnOK.Content = "确 定";
                        }
                        else
                        {
                            controlClassView.ViewModel.IsClassNameReadonly = true;
                            controlClassView.btnOK.Content = "保 存";
                        }
                        controlClassView.Title = "编辑控制类";
                        controlClassView.ShowDialog();
                        break;
                    case "RelationClass":
                        break;
                    default:
                        break;
                }
            }
        }
        public void SelectAllClass()
        {
            if (DesignClassDataSource != null)
            {
                bool allChecked = true;
                foreach (var item in DesignClassDataSource)
                {
                    if (!item.IsChecked)
                    {
                        allChecked = false;
                        break;
                    }
                }
                if (allChecked)
                {
                    foreach (var item in DesignClassDataSource)
                    {
                        item.IsChecked = false;
                    }
                }
                else
                {
                    foreach (var item in DesignClassDataSource)
                    {
                        item.IsChecked = true;
                    }
                }
            }            
        }
        public void GenerateStaticEntity()
        {
            CodeGenWindow codeGenWindow = new CodeGenWindow();
            List<DesignClass> entityClassList = DesignClassDataSource.Where(item => item.IsChecked && (item.MainType == 0 || item.MainType == 2)).ToList();
            foreach (var item in entityClassList)
            {
                codeGenWindow.ViewModel.ClassDataSource.Add(item);
            }
            codeGenWindow.ShowDialog();
        }
        #endregion 类型部分
        #region 属性部分
        private bool CanAddPropertyExecute()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.MainType == 0)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CanEditPropertyExecute()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.Properties.Where(item => item.IsChecked).ToList().Count == 1)
                {
                    DesignProperty designProperty = CurrentDesignClass.Properties.Where(item => item.IsChecked).FirstOrDefault();
                    bool canEdit = true;
                    if (designProperty.IsPrimarykey)
                    {
                        canEdit = false;
                    }
                    if (designProperty.IsPrimarykey && CurrentdesignProperty.State == "added")
                    {
                        canEdit = false;
                    }
                    if (designProperty.DataType == "Struct")
                    {
                        canEdit = false;
                    }
                    if (canEdit)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        private bool CanDeletePropertyExecute()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.Properties.Where(item => item.IsChecked).ToList().Count > 0)
                {

                    return true;
                }
            }
            return false;
        }
        private bool CanEditUIInfoExecute()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.Properties.Where(item => item.IsChecked).ToList().Count == 1)
                {
                    DesignProperty designProperty = CurrentDesignClass.Properties.Where(item => item.IsChecked).FirstOrDefault();
                    bool canEdit = true;
                    if (designProperty.IsPrimarykey)
                    {
                        canEdit = false;
                    }
                    if (designProperty.IsPrimarykey && CurrentdesignProperty.State == "added")
                    {
                        canEdit = false;
                    }
                    if (!designProperty.IsPersistable)
                    {
                        canEdit = false;
                    }
                    if (canEdit)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        public void AddProperty()
        {
            PropertyView propertyView = new PropertyView();
            propertyView.ViewModel.EditState = "add";
            propertyView.ViewModel.CreateNewProperty();
            propertyView.Title = "添加属性";
            propertyView.ShowDialog();
        }
        public void EditProperty()
        {
            CurrentdesignProperty = CurrentDesignClass.Properties.Where(item => item.IsChecked).FirstOrDefault();
            if (CurrentdesignProperty != null)
            {
                bool canEdit = true;
                if (CurrentdesignProperty.IsPrimarykey)
                {
                    canEdit = false;
                }
                if (CurrentdesignProperty.IsPrimarykey && CurrentdesignProperty.State == "added")
                {
                    canEdit = false;
                }
                if (CurrentdesignProperty.IsQueryProperty)
                {
                    canEdit = false;
                }
                if (canEdit)
                {
                    for (int i = 0; i < CurrentDesignClass.Properties.Count; i++)
                    {
                        CurrentDesignClass.Properties[i].IsChecked = false;
                    }

                    CurrentdesignProperty.IsChecked = true;
                    EditPropertyCommand.RaiseCanExecuteChanged();

                    PropertyView propertyView = new PropertyView();
                    if (CurrentdesignProperty.State == "normal" || CurrentdesignProperty.State == "modified")
                    {
                        propertyView.ViewModel.EditState = "modify";
                    }
                    else
                    {
                        propertyView.ViewModel.EditState = "add";
                    }
                    propertyView.ViewModel.DesignProperty = CurrentdesignProperty;
                    propertyView.ViewModel.UIDesignInfo = CurrentdesignProperty.UIDesignInfo;
                    propertyView.ViewModel.InitReferSourceState();
                    propertyView.Title = "编辑属性";
                    propertyView.ShowDialog();
                }
            }
        }
        public void EditUIInfo()
        {
            UIInfoView uIInfoView = new UIInfoView();
            CurrentdesignProperty = CurrentDesignClass.Properties.Where(item => item.IsChecked).FirstOrDefault();
            uIInfoView.ViewModel.DesignProperty = CurrentdesignProperty;
            if (CurrentdesignProperty.UIDesignInfo != null)
            {
                uIInfoView.ViewModel.UIDesignInfo = CurrentdesignProperty.UIDesignInfo;               
            }
            else
            {
                uIInfoView.ViewModel.InitDesignInfo();                
            }            
            uIInfoView.Title = "编辑界面信息";
            uIInfoView.ShowDialog();
        }
       
        private void DeleteProperty()
        {
            MessageBoxResult result = MessageBox.Show("您确定要删除所选属性吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                List<DesignProperty> designProperties = CurrentDesignClass.Properties.Where(item => item.IsChecked).ToList();
                foreach (var item in designProperties)
                {
                    switch (item.State)
                    {
                        case "added":
                            CurrentDesignClass.Properties.Remove(item);
                            break;
                        default:
                            CurrentDesignClass.Properties.Remove(item);
                            CurrentDesignClass.DeletedProperties.Add(item);
                            SaveClassCommand.RaiseCanExecuteChanged();
                            break;
                    }
                }
                MessageBox.Show("属性从内存中删除成功,请点击保存按钮更新数据库！", "提示");
            }
        }
        private void PropertyChecked(string propertyName)
        {
            //TODO:这个判断是处理一个bug,具体bug原因不清楚,什么情况下propertyName是空值
            if (propertyName == null)
            {
                return;
            }
            CurrentdesignProperty = CurrentDesignClass.Properties.Where(item => item.PropertyName == propertyName).FirstOrDefault();
            if (CurrentdesignProperty.IsPrimarykey)
            {
                CurrentdesignProperty.IsChecked = false;
            }
            else
            {
                EditPropertyCommand.RaiseCanExecuteChanged();
                DeletePropertyCommand.RaiseCanExecuteChanged();
                UIInfoEditCommand.RaiseCanExecuteChanged();
            }
        }
        private void PropertyUnchecked()
        {
            EditPropertyCommand.RaiseCanExecuteChanged();
            DeletePropertyCommand.RaiseCanExecuteChanged();
            UIInfoEditCommand.RaiseCanExecuteChanged();
        }        
        public void PropertyRowActivate()
        {
            if (CurrentdesignProperty != null)
            {
                bool canEdit = true;
                if (CurrentdesignProperty.IsPrimarykey)
                {
                    canEdit = false;
                }
                if (CurrentdesignProperty.IsPrimarykey && CurrentdesignProperty.State == "added")
                {
                    canEdit = false;
                }
                if (CurrentdesignProperty.IsQueryProperty)
                {
                    canEdit = false;
                }
                if (canEdit)
                {
                    for (int i = 0; i < CurrentDesignClass.Properties.Count; i++)
                    {
                        CurrentDesignClass.Properties[i].IsChecked = false;
                    }

                    CurrentdesignProperty.IsChecked = true;
                    EditPropertyCommand.RaiseCanExecuteChanged();
               
                    PropertyView propertyView = new PropertyView();
                    if (CurrentdesignProperty.State == "normal" || CurrentdesignProperty.State == "modified")
                    {
                        propertyView.ViewModel.EditState = "modify";
                    }
                    else
                    {
                        propertyView.ViewModel.EditState = "add";
                    }
                    propertyView.ViewModel.DesignProperty = CurrentdesignProperty;
                    propertyView.ViewModel.UIDesignInfo = CurrentdesignProperty.UIDesignInfo;
                    propertyView.ViewModel.InitReferSourceState();
                    propertyView.Title = "编辑属性";
                    propertyView.ShowDialog();
                }
            }
        }
        #endregion 属性部分
        #region 方法部分
        private bool CanAddMethodExecute()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.MainType == 1)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CanEditMethodExecute()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.Methodes.Where(item => item.IsChecked).ToList().Count == 1)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CanDeleteMethodExecute()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.Methodes.Where(item => item.IsChecked).ToList().Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
        public void AddMethod()
        {
            MethodView methodView = new MethodView();
            methodView.ViewModel.EditState = "add";
            methodView.Title = "添加方法";
            methodView.ShowDialog();
        }
        public void EditMethod()
        {
            MethodView methodView = new MethodView();
            methodView.ViewModel.EditState = "modify";
            if (CurrentDesignMethod.State == "normal")
            {
                CurrentDesignMethod.IsMethodChanged = false;
            }
            methodView.ViewModel.DesignMethod = CurrentDesignMethod;
            methodView.Title = "编辑方法";
            methodView.ShowDialog();
        }
        private void DeleteMethod()
        {
            MessageBoxResult result = MessageBox.Show("您确定要删除所选方法吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                List<DesignMethod> designMethodes = CurrentDesignClass.Methodes.Where(item => item.IsChecked).ToList();
                foreach (var item in designMethodes)
                {
                    switch (item.State)
                    {
                        case "added":
                            CurrentDesignClass.Methodes.Remove(item);
                            break;
                        default:
                            CurrentDesignClass.Methodes.Remove(item);
                            CurrentDesignClass.DeletedMethodes.Add(item);
                            SaveClassCommand.RaiseCanExecuteChanged();
                            break;
                    }
                }
                MessageBox.Show("方法从内存中删除成功,请点击保存按钮更新数据库！", "提示");
            }
        }       
        private void MethodChecked()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.Methodes.Where(item => item.IsChecked).ToList().Count == 1)
                {
                    CurrentDesignMethod = CurrentDesignClass.Methodes.Where(item => item.IsChecked).FirstOrDefault();
                }
                else
                {
                    CurrentDesignMethod = null;
                }
            }
            EditMethodCommand.RaiseCanExecuteChanged();
            DeleteMethodCommand.RaiseCanExecuteChanged();
            AddParameterCommand.RaiseCanExecuteChanged();
        }
        private void MethodUnchecked()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.Methodes.Where(item => item.IsChecked).ToList().Count == 1)
                {
                    CurrentDesignMethod = CurrentDesignClass.Methodes.Where(item => item.IsChecked).FirstOrDefault();
                }
                else
                {
                    CurrentDesignMethod = null;
                }
            }
            EditMethodCommand.RaiseCanExecuteChanged();
            DeleteMethodCommand.RaiseCanExecuteChanged();
            AddParameterCommand.RaiseCanExecuteChanged();
        }
        public void MethodRowActivate()
        {
            if (SelectedDesignMethod != null)
            {
                foreach (var item in CurrentDesignClass.Methodes)
                {
                    if (item.MethodName == SelectedDesignMethod.MethodName)
                    {
                        item.IsChecked = true;
                    }
                    else
                    {
                        item.IsChecked = false;
                    }
                }

                MethodView methodView = new MethodView();
                methodView.ViewModel.EditState = "modify";
                if (CurrentDesignMethod.State == "normal")
                {
                    CurrentDesignMethod.IsMethodChanged = false;
                }
                methodView.ViewModel.DesignMethod = CurrentDesignMethod;
                methodView.Title = "编辑方法";
                methodView.ShowDialog();
            }
        }
        #endregion 方法部分
        #region 参数部分
        private bool CanAddParameterExecute()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.Methodes.Where(item => item.IsChecked).ToList().Count == 1)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CanEditParameterExecute()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.Methodes.Where(item => item.IsChecked).ToList().Count == 1)
                {
                    DesignMethod designMethod = CurrentDesignClass.Methodes.Where(item => item.IsChecked).FirstOrDefault();
                    if (designMethod.Parameters.Where(item => item.IsChecked).ToList().Count == 1)
                    {
                        return true;
                    }                    
                }
            }
            return false;
        }
        private bool CanDeleteParameterExecute()
        {
            if (CurrentDesignClass != null)
            {
                if (CurrentDesignClass.Methodes.Where(item => item.IsChecked).ToList().Count == 1)
                {
                    DesignMethod designMethod = CurrentDesignClass.Methodes.Where(item => item.IsChecked).FirstOrDefault();
                    if (designMethod.Parameters.Where(item => item.IsChecked).ToList().Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void AddParameter()
        {
            ParameterView parameterView = new ParameterView();
            parameterView.ViewModel.EditState = "add";
            parameterView.ViewModel.CreateNewParameter();
            parameterView.Title = "添加参数";
            parameterView.ShowDialog();
        }
        public void EditParameter()
        {
            ParameterView parameterView = new ParameterView();
            parameterView.ViewModel.EditState = "modify";
            CurrentDesignMethodParameter =  CurrentDesignMethod.Parameters.Where(item => item.IsChecked).FirstOrDefault();
            if(CurrentDesignMethodParameter.State == "normal")
            {
                CurrentDesignMethodParameter.IsParameterChanged = false;
            }
            parameterView.ViewModel.DesignMethodParameter = CurrentDesignMethodParameter;
            parameterView.Title = "编辑参数";
            parameterView.ShowDialog();
        }
        private void DeleteParameter()
        {
            MessageBoxResult result = MessageBox.Show("您确定要删除所选参数吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                List<DesignMethodParameter> designMethodParameters = CurrentDesignMethod.Parameters.Where(item => item.IsChecked).ToList();
                foreach (var item in designMethodParameters)
                {
                    switch (item.State)
                    {
                        case "added":
                            CurrentDesignMethod.Parameters.Remove(item);
                            break;
                        default:
                            CurrentDesignMethod.Parameters.Remove(item);
                            CurrentDesignMethod.State = "modified";
                            CurrentDesignMethod.IsMethodChanged = true;
                            SaveClassCommand.RaiseCanExecuteChanged();
                            break;
                    }
                }
                MessageBox.Show("参数从内存中删除成功,请点击保存按钮更新数据库！", "提示");
            }
        }
        private void ParameterChecked()
        {
            EditParameterCommand.RaiseCanExecuteChanged();
            DeleteParameterCommand.RaiseCanExecuteChanged();
        }
        private void ParameterUnchecked()
        {
            EditParameterCommand.RaiseCanExecuteChanged();
            DeleteParameterCommand.RaiseCanExecuteChanged();
        }
        public void ParameterRowActivate()
        {
            if (CurrentDesignMethodParameter != null)
            {
                if (!CurrentDesignMethodParameter.IsChecked)
                {
                    CurrentDesignMethodParameter.IsChecked = true;
                }
                ParameterView parameterView = new ParameterView();
                parameterView.ViewModel.EditState = "modify";
                if (CurrentDesignMethodParameter.State == "normal")
                {
                    CurrentDesignMethodParameter.IsParameterChanged = false;
                }
                parameterView.ViewModel.DesignMethodParameter = CurrentDesignMethodParameter;
                parameterView.Title = "编辑参数";
                parameterView.ShowDialog();
            }
        }
        #endregion 参数部分               
        public void LoadApplicationSystem()
        {
            ApplicationSystemTreeDataSource.Clear();
            designClassCache.Clear();
            //构造根节点
            rootNode = new TreeNode()
            {
                Type = "Application",
                TreeNodeID = ApplicationDesignCache.ApplicationID,
                Name = ApplicationDesignCache.ApplicationName,
                Image = ApplicationDesignCache.ApplicationImage
            };

            //加载模块节点
            List<DynEntity> moduleCollection = ApplicationDesignService.GetAplicationModulesByAplicationID(ApplicationDesignCache.ApplicationID);
            if (moduleCollection != null)
            {
                foreach (var moduleEntity in moduleCollection)
                {
                    TreeNode moduleTreeNode = new TreeNode()
                    {
                        Type = "Module",
                        TreeNodeID = Convert.ToInt32(moduleEntity["ModuleID"]),
                        Name = moduleEntity["ModuleName"] as string,
                        Parent = rootNode,
                        Image = ApplicationDesignCache.ModuleClassImage
                    };
                    rootNode.Children.Add(moduleTreeNode);
                    LoadModuleClassEntities(moduleTreeNode);
                }
            }
            ApplicationSystemTreeDataSource.Add(rootNode);
        }
        private void LoadModuleClassEntities(TreeNode moduleTreeNode)
        {
            int moduleID = (int)moduleTreeNode.TreeNodeID;
            designClassCache.Add(moduleID, new ObservableCollection<DesignClass>());
            List<DynEntity> classEntityCollection = ApplicationDesignService.GetModuleClassEntityCollection(moduleID);

            if (classEntityCollection != null)
            {
                foreach (var classEntity in classEntityCollection)
                {
                    DesignClass designClass = new DesignClass();
                    designClass.ClassID = Convert.ToInt32(classEntity["DynClassID"]);
                    designClass.ClassName = classEntity["DynClassName"] as string;
                    designClass.DisplayName = classEntity["DisplayName"] as string;
                    designClass.BaseClassName = classEntity["BaseClassName"] as string;
                    designClass.MainType = Convert.ToInt32(classEntity["MainType"]);
                    designClass.NamespaceID = Convert.ToInt32(classEntity["NamespaceID"]);
                    designClass.ModuleID = Convert.ToInt32(classEntity["ModuleID"]);
                    designClass.Description = classEntity["Description"] as string;
                    designClass.State = "normal";
                    switch (designClass.MainType)
                    {
                        case 0:
                            designClass.MainTypeName = "EntityClass";
                            designClass.MainTypeImage = ApplicationDesignCache.EntityClassImage;
                            break;
                        case 1:
                            designClass.MainTypeName = "ControlClass";
                            designClass.MainTypeImage = ApplicationDesignCache.ControlClassImage;
                            break;
                        case 2:
                            designClass.MainTypeName = "RelationClass";
                            designClass.MainTypeImage = ApplicationDesignCache.RelationClassImage;
                            break;
                        case 4:
                            designClass.MainTypeName = "FuncationClass";
                            designClass.MainTypeImage = ApplicationDesignCache.FuncationClassImage;
                            break;                        
                        default:
                            break;
                    }
                    //构造Attribute
                    string attributeStr = classEntity["Attributes"] as string;
                    if (!string.IsNullOrEmpty(attributeStr))
                    {
                        designClass.Attributes = DynObjectTransverter.JsonToDynObjectList(attributeStr);
                    }
                    foreach (var item in designClass.Attributes)
                    {
                        switch (item.DynClass.Name)
                        {
                            case "Persistable":
                                designClass.IsPersistable = true;
                                break;
                            case "ServiceContract":
                                designClass.IsServiceProtocol = true;
                                break;                            
                        }                        
                    }
                    designClassCache[moduleID].Add(designClass);
                    moduleTreeNode.Children.Add(new TreeNode() { TreeNodeID = designClass.ClassID, Type = designClass.MainTypeName, Tag = designClass, Name = classEntity["DynClassName"] as string, Parent = moduleTreeNode, Image = designClass.MainTypeImage });
                }
            }
        }       
    }
}
