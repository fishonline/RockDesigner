using Microsoft.Practices.ServiceLocation;
using Rock.Common;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using Rock.Dyn.Core;
using Rock.Orm.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    public class ClassViewModel : ViewModelBase
    {
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }
        public DesignerViewModel DesignerViewModel
        {
            get { return ServiceLocator.Current.GetInstance<DesignerViewModel>(); }
        }
        private DesignClass _designClass;
        private ObservableCollection<Namespace> _namespaceSource;
        private Namespace _selectedNameSpace;
        private ObservableCollection<string> _structSource;
        private ObservableCollection<string> _relationPropertyAStructSource;
        private ObservableCollection<string> _relationPropertyBStructSource;
        private string _editState;
        private string _mainTypeName;
        private DesignProperty designProperty;
        private bool _isEnableBaseClass;
        private bool _isClassNameReadonly = false;


        public DesignClass DesignClass
        {
            get { return _designClass; }
            set
            {
                _designClass = value;
                this.OnPropertyChanged("DesignClass");
            }
        }
        public ObservableCollection<Namespace> NamespaceSource
        {
            get { return _namespaceSource; }
            set
            {
                _namespaceSource = value;
                this.OnPropertyChanged("NamespaceSource");
            }
        }
        public Namespace SelectedNameSpace
        {
            get { return _selectedNameSpace; }
            set
            {
                _selectedNameSpace = value;
                this.OnPropertyChanged("SelectedNameSpace");
            }
        }
        public ObservableCollection<string> StructSource
        {
            get { return _structSource; }
            set
            {
                _structSource = value;
                this.OnPropertyChanged("StructSource");
            }
        }
        public string EditState
        {
            get { return _editState; }
            set { _editState = value; }
        }
        public string MainTypeName
        {
            get { return _mainTypeName; }
            set
            {
                if (_mainTypeName != value)
                {
                    _mainTypeName = value;
                    this.OnPropertyChanged("MainTypeName");
                }
            }
        }
        public bool IsEnableBaseClass
        {
            get { return _isEnableBaseClass; }
            set
            {
                if (_isEnableBaseClass != value)
                {
                    _isEnableBaseClass = value;
                    this.OnPropertyChanged("IsEnableBaseClass");
                }
            }
        }
        public bool IsClassNameReadonly
        {
            get { return _isClassNameReadonly; }
            set
            {
                if (_isClassNameReadonly != value)
                {
                    _isClassNameReadonly = value;
                    this.OnPropertyChanged("IsClassNameReadonly");
                }
            }
        }
        public ObservableCollection<string> RelationPropertyAStructSource
        {
            get { return _relationPropertyAStructSource; }
            set
            {
                _relationPropertyAStructSource = value;
                this.OnPropertyChanged("RelationPropertyAStructSource");
            }
        }
        public ObservableCollection<string> RelationPropertyBStructSource
        {
            get { return _relationPropertyBStructSource; }
            set
            {
                _relationPropertyBStructSource = value;
                this.OnPropertyChanged("RelationPropertyBStructSource");
            }
        }
        public ICommand RelationPropertyAStructChangedCommand { get; private set; }
        public ICommand RelationPropertyBStructChangedCommand { get; private set; }
        //
        public ClassViewModel()
        {
            RelationPropertyAStructChangedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(RelationPropertyAStructChanged);
            RelationPropertyBStructChangedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(RelationPropertyBStructChanged);
            DesignClass = new DesignClass();
            DesignClass.ClassID = -1;
            DesignClass.State = "added";
            ListCollectionView listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(ApplicationDesignCache.NamespaceSource);
            listCollectionView.SortDescriptions.Add(new SortDescription("NamespaceName", ListSortDirection.Ascending));
            NamespaceSource = ApplicationDesignCache.NamespaceSource;
            SelectedNameSpace = ApplicationDesignCache.NamespaceSource.Where(item => item.NamespaceID == 55).FirstOrDefault();
            listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(ApplicationDesignCache.StructSource);
            listCollectionView.SortDescriptions.Add(new SortDescription());
            StructSource = ApplicationDesignCache.StructSource;
            RelationPropertyAStructSource = ApplicationDesignCache.StructSource;
            RelationPropertyBStructSource = ApplicationDesignCache.StructSource;
            IsEnableBaseClass = false;
        }
        public void EntityClassInit()
        {
            DesignClass.BaseClassName = "Entity";
            DesignClass.IsPersistable = true;
        }
        public void RelationClassInit()
        {
            DesignClass.BaseClassName = "Entity";
            DesignClass.IsPersistable = true;
            DesignClass.RelationPropertyA = new DesignProperty();
            DesignClass.RelationPropertyA.IsPrimarykey = true;
            DesignClass.RelationPropertyA.CollectionType = "None";
            DesignClass.RelationPropertyA.DataType = "I32";
            DesignClass.RelationPropertyA.IsNullable = false;
            DesignClass.RelationPropertyA.IsPersistable = true;
            DesignClass.RelationPropertyA.PropertyID = -1;
            DesignClass.RelationPropertyA.RelationType = "多对多";
            DesignClass.RelationPropertyA.SqlType = "int";
            DesignClass.RelationPropertyA.IsQueryProperty = false;
            DesignClass.RelationPropertyA.IsArray = false;
            DesignClass.RelationPropertyA.State = "added";


            DesignClass.RelationPropertyB = new DesignProperty();
            DesignClass.RelationPropertyB.IsPrimarykey = true;
            DesignClass.RelationPropertyB.CollectionType = "None";
            DesignClass.RelationPropertyB.DataType = "I32";
            DesignClass.RelationPropertyB.IsNullable = false;
            DesignClass.RelationPropertyB.IsPersistable = true;
            DesignClass.RelationPropertyB.PropertyID = -1;
            DesignClass.RelationPropertyB.RelationType = "多对多";
            DesignClass.RelationPropertyB.SqlType = "int";
            DesignClass.RelationPropertyB.IsQueryProperty = false;
            DesignClass.RelationPropertyB.IsArray = false;
            DesignClass.RelationPropertyB.State = "added";

        }
        public void ControlClassInit()
        {
            DesignClass.IsPersistable = false;
            DesignClass.IsServiceProtocol = true;
        }
        public void RelationPropertyAStructChanged()
        {
            if (DesignClass.RelationPropertyA != null)
            {
                DesignClass.RelationPropertyA.DbFieldName = DesignClass.RelationPropertyA.StructName + "ID";
                DesignClass.RelationPropertyA.PropertyName = DesignClass.RelationPropertyA.StructName + "ID";
            }
        }
        public void RelationPropertyBStructChanged()
        {
            if (DesignClass.RelationPropertyB != null)
            {
                DesignClass.RelationPropertyB.DbFieldName = DesignClass.RelationPropertyB.StructName + "ID";
                DesignClass.RelationPropertyB.PropertyName = DesignClass.RelationPropertyB.StructName + "ID";
            }
        }
        public bool CreateDesignClass()
        {
            if (SelectedNameSpace == null)
            {
                MessageBox.Show("请选择命名空间!", "提示");
                return false;
            }
            switch (MainTypeName)
            {
                case "EntityClass":
                    DesignClass.MainType = 0;
                    DesignClass.MainTypeImage = ApplicationDesignCache.EntityClassImage;
                    //添加主键属性
                    designProperty = new DesignProperty();
                    designProperty.PropertyID = -1;
                    designProperty.PropertyName = DesignClass.ClassName + "ID";
                    if (!string.IsNullOrEmpty(DesignClass.DisplayName))
                    {
                        designProperty.DisplayName = DesignClass.DisplayName + "ID";
                    }
                    designProperty.Description = DesignClass.DisplayName + "主键";
                    designProperty.IsNullable = false;
                    designProperty.IsPersistable = true;
                    designProperty.IsPrimarykey = true;
                    designProperty.DataType = "I32";
                    designProperty.SqlType = "int";
                    designProperty.CollectionType = "None";
                    designProperty.RelationType = "无";
                    designProperty.State = "added";
                    DesignClass.Properties.Add(designProperty);

                    //添加名称属性
                    designProperty = new DesignProperty();
                    designProperty.PropertyID = -1;
                    designProperty.PropertyName = DesignClass.ClassName + "Name";
                    if (!string.IsNullOrEmpty(DesignClass.DisplayName))
                    {
                        designProperty.DisplayName = DesignClass.DisplayName + "名称";
                    }
                    designProperty.Description = DesignClass.DisplayName + "名称";
                    designProperty.IsNullable = true;
                    designProperty.IsPersistable = true;
                    designProperty.DataType = "String";
                    designProperty.SqlType = "nvarchar";
                    designProperty.DbFieldLength = 50;
                    designProperty.CollectionType = "None";
                    designProperty.RelationType = "无";
                    designProperty.State = "added";

                    DesignInfo UIDesignInfo = new DesignInfo();
                    UIDesignInfo.GridColAlign = "left";
                    UIDesignInfo.GridColSorting = "str";
                    UIDesignInfo.GridColType = "ro";
                    UIDesignInfo.GridWidth = 0;
                    UIDesignInfo.InputType = "TextBox";
                    UIDesignInfo.ValidateType = "None";
                    UIDesignInfo.QueryForm = "Fuzzy";
                    UIDesignInfo.ReferType = "";
                    designProperty.UIDesignInfo = UIDesignInfo;
                    DesignClass.Properties.Add(designProperty);
                    break;
                case "RelationClass":
                    DesignClass.MainType = 2;
                    DesignClass.MainTypeImage = ApplicationDesignCache.RelationClassImage;
                    //添加关联属性
                    DesignClass.Properties.Add(DesignClass.RelationPropertyA);
                    DesignClass.Properties.Add(DesignClass.RelationPropertyB);
                    if (string.IsNullOrEmpty(DesignClass.RelationPropertyA.StructName))
                    {
                        MessageBox.Show("关联属性一没有选择关联类型请检查!", "提示");
                        return false;
                    }
                    if (string.IsNullOrEmpty(DesignClass.RelationPropertyB.StructName))
                    {
                        MessageBox.Show("关联属性二没有选择关联类型请检查!", "提示");
                        return false;
                    }
                    break;
                case "ControlClass":
                    DesignClass.MainType = 1;
                    DesignClass.MainTypeImage = ApplicationDesignCache.ControlClassImage;
                    DesignClass.BaseClassName = null;
                    DesignClass.IsPersistable = false;
                    break;
                default:
                    break;
            }
            DesignClass.MainTypeName = MainTypeName;
            DesignClass.NamespaceID = SelectedNameSpace.NamespaceID;
            DesignClass.ModuleID = DesignerViewModel.SelectedTreeNode.TreeNodeID;
            DesignClass.ClassID = -1;
            DesignerViewModel.CurrentDesignClass = DesignClass;
            DesignerViewModel.AddNewDesignClass();
            DesignerViewModel.SaveClassCommand.RaiseCanExecuteChanged();
            return true;
        }
        public bool EditDesignClass()
        {
            DesignClass.NamespaceID = SelectedNameSpace.NamespaceID;
            if (DesignClass.State != "added")
            {
                ApplicationDesignService.EditDesignClass(DesignClass);
                DesignClass.State = "normal";
                DesignerViewModel.SaveClassCommand.RaiseCanExecuteChanged();
                MessageBox.Show("保存成功!");
            }
            return true;
        }
        public void Cancel()
        {
            if (DesignClass.ClassID > 0)
            {
                DynEntity classEntity = ApplicationDesignService.GetClassEntity(DesignClass.ClassID);
                string dynAtrributeStr = classEntity["Attributes"] as string;
                List<DynObject> dynAttributes = DynObjectTransverter.JsonToDynObjectList(dynAtrributeStr);
                foreach (var item in dynAttributes)
                {
                    switch (item.DynClass.Name)
                    {
                        case "Persistable":
                            DesignClass.IsPersistable = true;
                            break;
                    }
                }
                DesignClass.DisplayName = classEntity["Description"] as string;
                DesignClass.Description = classEntity["DisplayName"] as string;
                DesignClass.NamespaceID = Convert.ToInt32(classEntity["NamespaceID"]);
                DesignerViewModel.CurrentDesignClass = DesignClass;
            }
        }
    }
}
