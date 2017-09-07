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
    public class MethodViewModel : ViewModelBase
    {
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }
        public DesignerViewModel DesignerViewModel
        {
            get { return ServiceLocator.Current.GetInstance<DesignerViewModel>(); }
        }
        private DesignMethod _designMethod;

       
        private ObservableCollection<string> _collectionTypeSource;
        private ObservableCollection<string> _resultTypeSource;
        private ObservableCollection<string> _structSource;
        private ObservableCollection<string> _scriptTypeSource;

       
        


        private bool _collectionTypeEnabled;
        private bool _dynTypeEnabled;
        private bool _structEnabled;       
        private string _editState;

        public DesignMethod DesignMethod
        {
            get { return _designMethod; }
            set
            {
                _designMethod = value;
                this.OnPropertyChanged("DesignMethod");
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
        public ObservableCollection<string> ResultTypeSource
        {
            get { return _resultTypeSource; }
            set
            {
                _resultTypeSource = value;
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
        public ObservableCollection<string> ScriptTypeSource
        {
            get { return _scriptTypeSource; }
            set
            {
                _scriptTypeSource = value;
                this.OnPropertyChanged("ScriptTypeSource");
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

        public string EditState
        {
            get { return _editState; }
            set { _editState = value; }
        }
        public ICommand ResultTypeSelectionChangedCommand { get; private set; }

        public MethodViewModel()
        {
            ResultTypeSelectionChangedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(ResultTypeSelectionChanged);

            CollectionTypeSource = ApplicationDesignCache.CollectionTypeSource;
            ResultTypeSource = ApplicationDesignCache.ResultTypeSource;
            ListCollectionView listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(ApplicationDesignCache.StructSource);
            listCollectionView.SortDescriptions.Add(new SortDescription());
            StructSource = ApplicationDesignCache.StructSource;
            ScriptTypeSource = ApplicationDesignCache.ScriptTypeSource;

            DesignMethod = new DesignMethod();
            DesignMethod.MethodID = -1;
            DesignMethod.IsOperationProtocol = true;
            DesignMethod.Parameters = new ObservableCollection<DesignMethodParameter>();
            DesignMethod.ResultCollectionType = "None";
            DesignMethod.ResultDataType = "Void";
            DesignMethod.ScriptType = "Dll";
            DesignMethod.IsMethodChanged = false;
            DesignMethod.State = "added";
        }
       
        public void ResultTypeSelectionChanged()
        {
            if (DesignMethod.ResultDataType == "Struct")
            {
                StructEnabled = true;
            }
            else
            {
                DesignMethod.ResultStructName = string.Empty;
                StructEnabled = false;
            }
        }
        public bool AddMethod()
        {
            if (MethodCheck())
            {
                DesignerViewModel.CurrentDesignClass.Methodes.Add(DesignMethod);
                DesignerViewModel.SaveClassCommand.RaiseCanExecuteChanged();
                return true;
            }
            return false;

        }
        public bool EditMethod()
        {
            if (MethodCheck())
            {
                if (DesignMethod.State == "normal")
                {
                    if (DesignMethod.IsMethodChanged)
                    {
                        DesignMethod.State = "modified";
                    }
                }
                DesignerViewModel.SaveClassCommand.RaiseCanExecuteChanged();
                return true;
            }
            return false;
        }
        public void Cancel()
        {
            if (DesignMethod.MethodID > 0)
            {
                DesignMethod originalDesignMethod = ApplicationDesignService.GetDesignMethod(DesignMethod.MethodID);
                DesignMethod.MethodName = originalDesignMethod.MethodName;
                DesignMethod.Body = originalDesignMethod.Body;
                DesignMethod.Description = originalDesignMethod.Description;
                DesignMethod.IsChecked = true;
                DesignMethod.IsMethodChanged = originalDesignMethod.IsMethodChanged;
                DesignMethod.Parameters = originalDesignMethod.Parameters;
                DesignMethod.ResultCollectionType = originalDesignMethod.ResultCollectionType;
                DesignMethod.ResultDataType = originalDesignMethod.ResultDataType;
                DesignMethod.ResultStructName = originalDesignMethod.ResultStructName;
                DesignMethod.ScriptType = originalDesignMethod.ScriptType;
                DesignMethod.State = originalDesignMethod.State;
            }
        }
        //检查方法名称和参数是否重名
        private bool MethodCheck()
        {
            if (DesignMethod.State == "added" && EditState == "add")
            {
                if (DesignerViewModel.CurrentDesignClass.Methodes.Where(item => item.MethodName == DesignMethod.MethodName).ToList().Count > 0)
                {
                    MessageBox.Show("方法名重复,请检查!", "提示");
                    return false;
                }
               
            }
            else
            {
                List<string> methodNames = new List<string>();
                foreach (var method in DesignerViewModel.CurrentDesignClass.Methodes)
                {
                    if (methodNames.Contains(method.MethodName))
                    {
                        MessageBox.Show("方法名重复,请检查!", "提示");
                        return false;
                    }
                    else
                    {
                        methodNames.Add(method.MethodName);
                    }
                }                
            }
            List<string> parameterNames = new List<string>();
            foreach (var Parameter in DesignMethod.Parameters)
            {
                if (parameterNames.Contains(Parameter.ParameterName))
                {
                    MessageBox.Show("参数名重复,请检查!", "提示");
                    return false;
                }
                else
                {
                    parameterNames.Add(Parameter.ParameterName);
                }
            }
            return true;
        }
    }
}
