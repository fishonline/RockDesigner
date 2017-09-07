using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using Rock.Dyn.Core;
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
    public class PropertyViewModel : ViewModelBase
    {
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }
        public DesignerViewModel DesignerViewModel
        {
            get { return ServiceLocator.Current.GetInstance<DesignerViewModel>(); }
        }
        private ObservableCollection<string> _collectionTypeSource;
        private ObservableCollection<string> _dynTypeSource;
        private ObservableCollection<string> _structSource;
        private ObservableCollection<string> _relationTypeSource;
        private ObservableCollection<string> _sqlTypeSource;
        private ObservableCollection<string> _validateTypeSource;
        private ObservableCollection<string> _inputTypeSource;
        private ObservableCollection<string> _gridColAlignSource;
        private ObservableCollection<string> _gridColSortingSource;
        private ObservableCollection<string> _gridColTypeSource;
        private ObservableCollection<string> _queryFormSource;

        private DesignProperty _designProperty;
        private bool _collectionTypeEnabled;
        private bool _dynTypeEnabled;
        private bool _structEnabled;
        private bool _sqlTypeSourceEnabled;
        private bool _dbFieldLengthReadOnly;
        private bool _decimalDigitsReadOnly;
        private string _editState;
        private DesignInfo _designInfo;

        public DesignProperty DesignProperty
        {
            get { return _designProperty; }
            set
            {
                _designProperty = value;
                this.OnPropertyChanged("DesignProperty");
            }
        }
        public bool CollectionTypeEnabled
        {
            get { return _collectionTypeEnabled; }
            set
            {
                if (_collectionTypeEnabled != value)
                {
                    _collectionTypeEnabled = value;
                    this.OnPropertyChanged("CollectionTypeEnabled");
                }
            }
        }
        public bool DynTypeEnabled
        {
            get { return _dynTypeEnabled; }
            set
            {
                if (_dynTypeEnabled != value)
                {
                    _dynTypeEnabled = value;
                    this.OnPropertyChanged("DynTypeEnabled");
                }
            }
        }
        public bool StructEnabled
        {
            get { return _structEnabled; }
            set
            {
                if (_structEnabled != value)
                {
                    _structEnabled = value;
                    this.OnPropertyChanged("StructEnabled");
                }
            }
        }

        public ObservableCollection<string> CollectionTypeSource
        {
            get { return _collectionTypeSource; }
            set
            {
                _collectionTypeSource = value;
                this.OnPropertyChanged("CollectionTypeSource");
            }
        }
        public ObservableCollection<string> DynTypeSource
        {
            get { return _dynTypeSource; }
            set
            {
                _dynTypeSource = value;
                this.OnPropertyChanged("DynTypeSource");
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
        public ObservableCollection<string> QueryFormSource
        {
            get { return _queryFormSource; }
            set
            {
                _queryFormSource = value;
                this.OnPropertyChanged("QueryFormSource");
            }
        }
        public bool SqlTypeSourceEnabled
        {
            get { return _sqlTypeSourceEnabled; }
            set
            {
                if (_sqlTypeSourceEnabled != value)
                {
                    _sqlTypeSourceEnabled = value;
                    this.OnPropertyChanged("SqlTypeSourceEnabled");
                }
            }
        }
        public bool DbFieldLengthReadOnly
        {
            get { return _dbFieldLengthReadOnly; }
            set
            {
                if (_dbFieldLengthReadOnly != value)
                {
                    _dbFieldLengthReadOnly = value;
                    this.OnPropertyChanged("DbFieldLengthReadOnly");
                }
            }
        }
        public bool DecimalDigitsReadOnly
        {
            get { return _decimalDigitsReadOnly; }
            set
            {
                if (_decimalDigitsReadOnly != value)
                {
                    _decimalDigitsReadOnly = value;
                    this.OnPropertyChanged("DecimalDigitsReadOnly");
                }
            }
        }
        public string EditState
        {
            get { return _editState; }
            set { _editState = value; }
        }

        public ObservableCollection<string> RelationTypeSource
        {
            get { return _relationTypeSource; }
            set
            {
                _relationTypeSource = value;
                this.OnPropertyChanged("RelationTypeSource");
            }
        }
        public ObservableCollection<string> SqlTypeSource
        {
            get { return _sqlTypeSource; }
            set
            {
                _sqlTypeSource = value;
                this.OnPropertyChanged("SqlTypeSource");
            }
        }
        public ObservableCollection<string> ValidateTypeSource
        {
            get { return _validateTypeSource; }
            set
            {
                _validateTypeSource = value;
                this.OnPropertyChanged("ValidateTypeSource");
            }
        }
        public ObservableCollection<string> InputTypeSource
        {
            get { return _inputTypeSource; }
            set
            {
                _inputTypeSource = value;
                this.OnPropertyChanged("InputTypeSource");
            }
        }
        public ObservableCollection<string> GridColSortingSource
        {
            get { return _gridColSortingSource; }
            set
            {
                _gridColSortingSource = value;
                this.OnPropertyChanged("GridColSortingSource");
            }
        }
        public ObservableCollection<string> GridColTypeSource
        {
            get { return _gridColTypeSource; }
            set
            {
                _gridColTypeSource = value;
                this.OnPropertyChanged("GridColTypeSource");
            }
        }
        public ObservableCollection<string> GridColAlignSource
        {
            get { return _gridColAlignSource; }
            set
            {
                _gridColAlignSource = value;
                this.OnPropertyChanged("GridColAlignSource");
            }
        }
        public DesignInfo UIDesignInfo
        {
            get { return _designInfo; }
            set
            {
                _designInfo = value;
                this.OnPropertyChanged("UIDesignInfo");
            }
        }
        public ICommand CollectionTypeSelectionChangedCommand { get; private set; }
        public ICommand DynTypeSelectionChangedCommand { get; private set; }
        public ICommand RelationTypeSelectionChangedCommand { get; private set; }
        public ICommand SqlTypeSelectionChangedCommand { get; private set; }
        public ICommand PersistableCheckedCommand { get; private set; }
        public ICommand PersistableUncheckedCommand { get; private set; }
        public DelegateCommand<object> TabControlSelectionChangedCommand { get; private set; }

        //
        public PropertyViewModel()
        {
            CollectionTypeSelectionChangedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CollectionTypeSelectionChanged);
            DynTypeSelectionChangedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(DynTypeSelectionChanged);
            RelationTypeSelectionChangedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(RelationTypeSelectionChanged);
            SqlTypeSelectionChangedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(SqlTypeSelectionChanged);
            PersistableCheckedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(PersistableChecked);
            PersistableUncheckedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(PersistableUnChecked);
            TabControlSelectionChangedCommand = new DelegateCommand<object>(TabControlSelectionChanged);

            CollectionTypeSource = ApplicationDesignCache.CollectionTypeSource;
            DynTypeSource = ApplicationDesignCache.DynTypeSource;
            ListCollectionView listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(ApplicationDesignCache.StructSource);
            listCollectionView.SortDescriptions.Add(new SortDescription());
            StructSource = ApplicationDesignCache.StructSource;
            RelationTypeSource = ApplicationDesignCache.RelationTypeSource;
            SqlTypeSource = ApplicationDesignCache.SqlTypeSource;
            ValidateTypeSource = ApplicationDesignCache.ValidateTypeSource;
            InputTypeSource = ApplicationDesignCache.InputTypeSource;
            GridColSortingSource = ApplicationDesignCache.GridColSortingSource;
            GridColTypeSource = ApplicationDesignCache.GridColTypeSource;
            GridColAlignSource = ApplicationDesignCache.GridColAlignSource;
            QueryFormSource = ApplicationDesignCache.QueryFormSource;
            DecimalDigitsReadOnly = true;
        }
        public void DynTypeSelectionChanged()
        {
            if (DesignProperty.DataType == "Struct")
            {
                StructEnabled = true;
            }
            else
            {
                DesignProperty.StructName = string.Empty;
                StructEnabled = false;
                switch (DesignProperty.DataType)
                {
                    case "I32":
                        DesignProperty.SqlType = "int";
                        UIDesignInfo.QueryForm = "Value";
                        break;
                    case "String":
                        DesignProperty.SqlType = "nvarchar";
                        UIDesignInfo.QueryForm = "Fuzzy";
                        break;
                    case "Decimal":
                        DesignProperty.SqlType = "decimal";
                        UIDesignInfo.QueryForm = "Value";
                        break;
                    case "DateTime":
                        DesignProperty.SqlType = "date";
                        UIDesignInfo.QueryForm = "Date";
                        break;
                    case "Bool":
                        DesignProperty.SqlType = "bit";
                        UIDesignInfo.QueryForm = "Value";
                        break;
                }
            }
        }
        public void PersistableChecked()
        {
            DesignProperty.CollectionType = "None";
        }
        public void PersistableUnChecked()
        {
            DesignProperty.SqlType = null;
            DesignProperty.DbFieldLength = null;
            DesignProperty.DbFieldName = null;
        }
        public void TabControlSelectionChanged(object selectedIndex)
        {
            if (selectedIndex != null)
            {
                if ((int)selectedIndex == 1)
                {
                    if (UIDesignInfo == null)
                    {
                        UIDesignInfo = new DesignInfo();
                        UIDesignInfo.GridColAlign = "left";
                        UIDesignInfo.GridColSorting = "str";
                        UIDesignInfo.GridColType = "ro";
                        UIDesignInfo.GridWidth = 0;
                        UIDesignInfo.InputType = "TextBox";
                        UIDesignInfo.ValidateType = "None";
                        UIDesignInfo.QueryForm = "Fuzzy";
                        UIDesignInfo.ReferType = "";
                    }
                    if (_editState == "add")
                    {
                        UIDesignInfo.IsRequired = !DesignProperty.IsNullable;
                        UIDesignInfo.GridHeader = DesignProperty.DisplayName;
                    }
                }
            }
        }
        public void CollectionTypeSelectionChanged()
        {
            switch (DesignProperty.CollectionType)
            {
                case "None":
                    DesignProperty.IsPersistable = true;
                    DesignProperty.IsArray = false;
                    break;
                case "List":
                    DesignProperty.IsPersistable = false;
                    DesignProperty.SqlType = null;
                    DesignProperty.DbFieldLength = null;
                    DesignProperty.IsArray = true;
                    break;
            }
        }
        public void RelationTypeSelectionChanged()
        {
            switch (DesignProperty.RelationType)
            {
                case "无":
                    DesignProperty.DbFieldName = null;
                    DesignProperty.DbFieldLength = null;
                    CollectionTypeEnabled = true;
                    DynTypeEnabled = true;
                    StructEnabled = false;
                    DbFieldLengthReadOnly = false;
                    DesignProperty.IsArray = false;
                    DesignProperty.IsQueryProperty = false;
                    break;
                case "一对多":
                    if (string.IsNullOrEmpty(DesignProperty.PropertyName))
                    {
                        DesignProperty.RelationType = "无";
                        DesignProperty.DataType = "String";
                        MessageBox.Show("请先输入属性名称后再选择一对多关联!", "提示");
                    }
                    else
                    {
                        DesignProperty.CollectionType = "None";
                        DesignProperty.DataType = "Struct";
                        DesignProperty.DbFieldName = DesignProperty.PropertyName + "ID";
                        DesignProperty.SqlType = "int";
                        DesignProperty.DbFieldLength = null;
                        DesignProperty.IsNullable = false;
                        CollectionTypeEnabled = false;
                        DynTypeEnabled = false;
                        StructEnabled = true;
                        DbFieldLengthReadOnly = true;
                        DesignProperty.IsArray = false;
                        DesignProperty.IsQueryProperty = true;
                    }
                    break;
                case "多对一":
                    if (string.IsNullOrEmpty(DesignProperty.PropertyName))
                    {
                        DesignProperty.RelationType = "无";
                        MessageBox.Show("请先输入属性名称后再选择多对一关联!", "提示");
                    }
                    else
                    {
                        DesignProperty.CollectionType = "List";
                        DesignProperty.DataType = "Struct";
                        DesignProperty.IsPersistable = false;
                        DesignProperty.SqlType = null;
                        DesignProperty.DbFieldLength = null;
                        CollectionTypeEnabled = false;
                        DynTypeEnabled = false;
                        StructEnabled = true;
                        DbFieldLengthReadOnly = true;
                        DesignProperty.IsArray = true;
                        DesignProperty.IsQueryProperty = true;
                    }
                    break;
                default:
                    break;
            }
        }
        public void SqlTypeSelectionChanged()
        {
            switch (DesignProperty.SqlType)
            {
                case "char":
                case "nchar":
                case "nvarchar":
                case "varchar":
                    DesignProperty.DbFieldLength = 50;
                    DbFieldLengthReadOnly = false;
                    DesignProperty.DecimalDigits = null;
                    DecimalDigitsReadOnly = true;
                    break;
                case "decimal":
                    DesignProperty.DbFieldLength = 18;
                    DbFieldLengthReadOnly = false;
                    DesignProperty.DecimalDigits = 0;
                    DecimalDigitsReadOnly = false;
                    break;
                default:
                    DesignProperty.DbFieldLength = null;
                    DbFieldLengthReadOnly = true;
                    DesignProperty.DecimalDigits = null;
                    DecimalDigitsReadOnly = true;
                    break;
            }
        }

        public void CreateNewProperty()
        {           
            InitDesignInfo();
            UIDesignInfo.State = "added";

            DesignProperty = new DesignProperty();

            DesignProperty.CollectionType = "None";
            DesignProperty.DataType = "String";
            DesignProperty.IsNullable = true;
            DesignProperty.SqlType = "nvarchar";
            DesignProperty.DbFieldLength = 50;
            DesignProperty.RelationType = "无";
            DesignProperty.State = "added";

            CollectionTypeEnabled = true;
            DynTypeEnabled = true;
            StructEnabled = false;
            SqlTypeSourceEnabled = true;
            DbFieldLengthReadOnly = false;
            DesignProperty.IsPropertyChanged = false;
            DesignProperty.IsQueryProperty = false;

        }
        private void InitDesignInfo()
        {
            UIDesignInfo = new DesignInfo();
            UIDesignInfo.GridColAlign = "left";
            UIDesignInfo.GridColSorting = "str";
            UIDesignInfo.GridColType = "ro";
            UIDesignInfo.GridHeader = "";
            UIDesignInfo.GridWidth = 0;
            UIDesignInfo.InputType = "TextBox";
            UIDesignInfo.ReferType = "";
            UIDesignInfo.QueryForm = "Fuzzy";
            UIDesignInfo.IsRequired = false;
            UIDesignInfo.IsReadOnly = false;
            UIDesignInfo.ValidateType = "None";
            UIDesignInfo.IsPropertyChanged = true;
        }
        public void InitReferSourceState()
        {
            switch (DesignProperty.DataType)
            {
                case "Struct":
                    CollectionTypeEnabled = true;
                    DynTypeEnabled = true;
                    StructEnabled = true;
                    SqlTypeSourceEnabled = true;
                    DbFieldLengthReadOnly = true;
                    break;
                default:
                    CollectionTypeEnabled = true;
                    DynTypeEnabled = true;
                    StructEnabled = false;
                    SqlTypeSourceEnabled = true;
                    DbFieldLengthReadOnly = false;
                    if (DesignProperty.SqlType == "decimal")
                    {
                        DecimalDigitsReadOnly = false;
                    }
                    else
                    {
                        DesignProperty.DecimalDigits = null;
                        DecimalDigitsReadOnly = true;
                    }
                    if (DesignProperty.CollectionType == "List")
                    {
                        DesignProperty.SqlType = null;
                        DesignProperty.DbFieldLength = null;
                    }
                    break;
            }
            DesignProperty.IsPropertyChanged = false;
        }
        public bool AddProperty()
        {
            if (DesignProperty.RelationType == "多对一")
            {
                DesignProperty.UIDesignInfo = null;
            }
            else
            {
                if (DesignProperty.IsPersistable)
                {
                    DesignProperty.UIDesignInfo = UIDesignInfo;
                    if (UIDesignInfo.IsPropertyChanged)
                    {
                        DesignProperty.IsPropertyChanged = true;
                    }
                }
                else
                {
                    DesignProperty.UIDesignInfo = null;
                }
            }
            //if (DesignProperty.DataType == "Struct")
            //{
            //    DesignProperty.IsQueryProperty = true;
            //}
            //else
            //{
            //    DesignProperty.IsQueryProperty = false;
            //}

            if (PropertyCheck())
            {
                //检查属性是否存在
                if (!DesignerViewModel.CurrentDesignClass.Properties.Contains(DesignProperty))
                {
                    DesignerViewModel.CurrentDesignClass.Properties.Add(DesignProperty);
                }
                DesignerViewModel.SaveClassCommand.RaiseCanExecuteChanged();
                return true;
            }
            return false;
        }
        public bool EditProperty()
        {
            if (DesignProperty.RelationType == "多对一")
            {
                DesignProperty.UIDesignInfo = null;
            }
            else
            {
                if (DesignProperty.IsPersistable)
                {
                    if (DesignProperty.UIDesignInfo == null)
                    {
                        InitDesignInfo();
                    }
                    DesignProperty.UIDesignInfo = UIDesignInfo;
                    if (UIDesignInfo.IsPropertyChanged)
                    {
                        DesignProperty.IsPropertyChanged = true;
                    }
                }
                else
                {
                    DesignProperty.UIDesignInfo = null;
                }
            }
            if (PropertyCheck())
            {
                if (DesignProperty.State == "normal")
                {
                    if (DesignProperty.IsPropertyChanged)
                    {
                        DesignProperty.State = "modified";
                    }
                }
                DesignerViewModel.SaveClassCommand.RaiseCanExecuteChanged();
                return true;
            }
            return false;
        }
        public void Cancel()
        {
            if (DesignProperty.PropertyID > 0)
            {
                DesignProperty originalDesignProperty = ApplicationDesignService.GetDesignProperty(DesignProperty.PropertyID);
                DesignProperty = originalDesignProperty;
                DesignerViewModel.CurrentdesignProperty = originalDesignProperty;
            }
        }

        private bool PropertyCheck()
        {
            if (string.IsNullOrEmpty(DesignProperty.DisplayName))
            {
                MessageBox.Show("显示名称不能为空,请检查!", "提示");
                return false;
            }
            if (!DataTypeCheck(DesignProperty.DataType, DesignProperty.SqlType))
            {
                MessageBox.Show("C#数据类型和数据库数据类型不匹配,或者Struct类型未选择,请检查!", "提示");
                return false;
            }

            if (DesignProperty.DataType != "String" && DesignProperty.DataType != "Struct")
            {
                if (UIDesignInfo.QueryForm == "Fuzzy")
                {
                    MessageBox.Show("非字符型的属性不支持模糊查询,请检查!", "提示");
                    return false;
                }
            }

            if (DesignProperty.DataType == "Struct")
            {
                if (DesignProperty.StructName == string.Empty)
                {
                    MessageBox.Show("对象属性必须选择Struct对象!", "提示");
                    return false;
                }
            }
            if (DesignProperty.CollectionType == "None" && DesignProperty.DataType == "Struct" && DesignProperty.IsPersistable)
            {
                if (DesignProperty.RelationType == "无")
                {
                    MessageBox.Show("存盘对象属性必须选择关联类型为:一对多!", "提示");
                    return false;
                }
            }
            if (DesignProperty.RelationType == "多对一")
            {
                if (!DesignProperty.IsNullable)
                {
                    MessageBox.Show("对象列表属性必须可空!", "提示");
                    return false;
                }
                if (DesignProperty.IsPersistable)
                {
                    MessageBox.Show("对象列表属性不能存盘!", "提示");
                    return false;
                }
            }
            return true;
        }
        private bool DataTypeCheck(string dataType, string sqlType)
        {           
            if (dataType == "Struct")
            {
                if (!string.IsNullOrEmpty(DesignProperty.StructName))
                {
                    return true;
                }
            }
            else
            {
                if (DesignProperty.IsPersistable)
                {
                    switch (dataType)
                    {
                        case "String":
                            string[] stringArrar = { "nvarchar", "nchar", "varchar", "char", "uniqueidentifier" };
                            if (stringArrar.Contains(sqlType))
                            {
                                return true;
                            }
                            break;
                        case "I16":
                            if (sqlType == "smallint")
                            {
                                return true;
                            }
                            break;
                        case "I32":
                            if (sqlType == "int")
                            {
                                return true;
                            }
                            break;
                        case "I64":
                            if (sqlType == "bigint")
                            {
                                return true;
                            }
                            break;
                        case "Bool":
                            if (sqlType == "bit")
                            {
                                return true;
                            }
                            break;
                        case "Byte":
                            if (sqlType == "tinyint")
                            {
                                return true;
                            }
                            break;
                        case "Decimal":
                            if (sqlType == "decimal")
                            {
                                return true;
                            }
                            break;
                        case "DateTime":
                            string[] timeArrar = { "datetime", "date", "time" };
                            if (timeArrar.Contains(sqlType))
                            {
                                return true;
                            }
                            break;
                        case "Double":
                            if (sqlType == "float")
                            {
                                return true;
                            }
                            break;
                        case "Binary":
                            string[] binaryArrar = { "binary", "varbinary" };
                            if (binaryArrar.Contains(sqlType))
                            {
                                return true;
                            }
                            break;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}
