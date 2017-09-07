using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
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
    public class UIInfoViewModel : ViewModelBase
    {
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }
        public DesignerViewModel DesignerViewModel
        {
            get { return ServiceLocator.Current.GetInstance<DesignerViewModel>(); }
        }
        private ObservableCollection<string> _validateTypeSource;
        private ObservableCollection<string> _inputTypeSource;
        private ObservableCollection<string> _gridColAlignSource;
        private ObservableCollection<string> _gridColSortingSource;
        private ObservableCollection<string> _gridColTypeSource;
        private ObservableCollection<string> _queryFormSource;    

        private DesignProperty _designProperty;
        private DesignInfo _designInfo;

        public DesignProperty DesignProperty
        {
            get { return _designProperty; }
            set { _designProperty = value; }
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
        public ObservableCollection<string> QueryFormSource
        {
            get { return _queryFormSource; }
            set
            {
                _queryFormSource = value;
                this.OnPropertyChanged("QueryFormSource");
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

        //
        public UIInfoViewModel()
        {
            ListCollectionView listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(ApplicationDesignCache.StructSource);
            listCollectionView.SortDescriptions.Add(new SortDescription());
            ValidateTypeSource = ApplicationDesignCache.ValidateTypeSource;
            InputTypeSource = ApplicationDesignCache.InputTypeSource;
            GridColSortingSource = ApplicationDesignCache.GridColSortingSource;
            GridColTypeSource = ApplicationDesignCache.GridColTypeSource;
            GridColAlignSource = ApplicationDesignCache.GridColAlignSource;
            QueryFormSource = ApplicationDesignCache.QueryFormSource;
        }

        public void InitDesignInfo()
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
        public bool EditUIDesignInfo()
        {
            if (DesignProperty.DataType != "String" && DesignProperty.DataType != "Struct")
            {
                if (UIDesignInfo.QueryForm == "Fuzzy")
                {
                    MessageBox.Show("非字符型的属性不支持模糊查询,请检查!", "提示");
                    return false;
                }
            }

            if (UIDesignInfo.IsPropertyChanged)
            {
                ApplicationDesignService.ModifyUIDesignInfo(DesignProperty.PropertyID, UIDesignInfo);
                UIDesignInfo.IsPropertyChanged = false;
                MessageBox.Show("界面信息已经成功保存到数据库!", "提示");
            }
            return true;
        }

        public void Cancel()
        {
            DesignInfo DesignInfo = ApplicationDesignService.GetUIDesignInfo(DesignProperty.PropertyID);
            if (DesignInfo != null)
            {
                UIDesignInfo = DesignInfo;
            }
            else
            {
                InitDesignInfo();
            }
        }
    }
}
