using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    public class ParameterViewModel : ViewModelBase
    {
        public DesignerViewModel DesignerViewModel
        {
            get { return ServiceLocator.Current.GetInstance<DesignerViewModel>(); }
        }
        DesignMethodParameter _designMethodParameter;

        
        private ObservableCollection<string> _collectionTypeSource;
        private ObservableCollection<string> _dynTypeSource;
        private ObservableCollection<string> _structSource;
        private bool _collectionTypeEnabled;
        private bool _dynTypeEnabled;
        private bool _structEnabled;
        private string _editState;

        public DesignMethodParameter DesignMethodParameter
        {
            get { return _designMethodParameter; }
            set
            {
                _designMethodParameter = value;
                this.OnPropertyChanged("DesignMethodParameter");
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
        //public bool CollectionTypeEnabled
        //{
        //    get { return _collectionTypeEnabled; }
        //    set
        //    {
        //        if (_collectionTypeEnabled != value)
        //        {
        //            _collectionTypeEnabled = value;
        //            this.OnPropertyChanged("CollectionTypeEnabled");
        //        }
        //    }
        //}
        //public bool DynTypeEnabled
        //{
        //    get { return _dynTypeEnabled; }
        //    set
        //    {
        //        if (_dynTypeEnabled != value)
        //        {
        //            _dynTypeEnabled = value;
        //            this.OnPropertyChanged("DynTypeEnabled");
        //        }
        //    }
        //}
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
        public string EditState
        {
            get { return _editState; }
            set { _editState = value; }
        }
        public ICommand DynTypeSelectionChangedCommand { get; private set; }
        public ParameterViewModel()
        {
            DynTypeSelectionChangedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(DynTypeSelectionChanged);

            CollectionTypeSource = ApplicationDesignCache.CollectionTypeSource;
            DynTypeSource = ApplicationDesignCache.DynTypeSource;
            ListCollectionView listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(ApplicationDesignCache.StructSource);
            listCollectionView.SortDescriptions.Add(new SortDescription());
            StructSource = ApplicationDesignCache.StructSource;
        }
        public void DynTypeSelectionChanged()
        {
            if (DesignMethodParameter.DataType == "Struct")
            {
                StructEnabled = true;
            }
            else
            {
                DesignMethodParameter.StructName = string.Empty;
                StructEnabled = false;
            }
        }
        public void CreateNewParameter()
        {
            DesignMethodParameter = new Models.DesignMethodParameter();
            DesignMethodParameter.CollectionType = "None";
            DesignMethodParameter.DataType = "String";
            DesignMethodParameter.State = "added";
            DesignMethodParameter.IsParameterChanged = true;
        }
        public bool AddParameter()
        {
            DesignMethod designMethod = DesignerViewModel.CurrentDesignClass.Methodes.Where(item => item.IsChecked).FirstOrDefault();
            designMethod.Parameters.Add(DesignMethodParameter);
            DesignerViewModel.SaveClassCommand.RaiseCanExecuteChanged();
            return true;
        }
        public bool EditParameter()
        {
            if (DesignMethodParameter.State == "normal")
            {
                if (DesignMethodParameter.IsParameterChanged)
                {
                    DesignMethodParameter.State = "modified";
                }
            }
            DesignerViewModel.SaveClassCommand.RaiseCanExecuteChanged();
            return true;
        }       
    }
}
