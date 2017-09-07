using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.DesignerModule.Models
{
    public class DesignMethod : NotificationObject
    {
        private int _methodID;        
        private string _methodName;
        private string _displayName;
        private string _body;
        private string _scriptType;       
        private string _description;
        private bool _isOperationProtocol;
        private string _resultCollectionType;
        private string _resultDataType;
        private string _resultStructName;
        private bool _isChecked;
        private string _state;
        private bool _isMethodChanged;       
        private ObservableCollection<DesignMethodParameter> _parameters = new ObservableCollection<DesignMethodParameter>();       
       
        public int MethodID
        {
            get { return _methodID; }
            set
            {
                if (_methodID != value)
                {
                    _methodID = value;
                    RaisePropertyChanged("MethodID");
                }
            }
        }
        public string MethodName
        {
            get { return _methodName; }
            set
            {
                if (_methodName != value)
                {
                    _methodName = value;
                    _isMethodChanged = true;
                    RaisePropertyChanged("MethodName");
                }
            }
        }
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    _isMethodChanged = true;
                    RaisePropertyChanged("DisplayName");
                }
            }
        }
        public string Body
        {
            get { return _body; }
            set
            {
                if (_body != value)
                {
                    _body = value;
                    _isMethodChanged = true;
                    RaisePropertyChanged("Body");
                }
            }
        }
        public string ScriptType
        {
            get { return _scriptType; }
            set
            {
                if (_scriptType != value)
                {
                    _scriptType = value;
                    _isMethodChanged = true;
                    RaisePropertyChanged("ScriptType");
                }
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    _isMethodChanged = true;
                    RaisePropertyChanged("Description");
                }
            }
        }
        public bool IsOperationProtocol
        {
            get { return _isOperationProtocol; }
            set
            {
                if (_isOperationProtocol != value)
                {
                    _isOperationProtocol = value;
                    _isMethodChanged = true;
                    RaisePropertyChanged("IsOperationProtocol");
                }
            }
        }
        public string ResultCollectionType
        {
            get { return _resultCollectionType; }
            set
            {
                if (_resultCollectionType != value)
                {
                    _resultCollectionType = value;
                    _isMethodChanged = true;
                    RaisePropertyChanged("ResultCollectionType");
                }
            }
        }
        public string ResultDataType
        {
            get { return _resultDataType; }
            set
            {
                if (_resultDataType != value)
                {
                    _resultDataType = value;
                    _isMethodChanged = true;
                    RaisePropertyChanged("ResultDataType");
                }
            }
        }
        public string ResultStructName
        {
            get { return _resultStructName; }
            set
            {
                if (_resultStructName != value)
                {
                    _resultStructName = value;
                    _isMethodChanged = true;
                    RaisePropertyChanged("ResultStructName");
                }
            }
        }
        public ObservableCollection<DesignMethodParameter> Parameters
        {
            get { return _parameters; }
            set
            {
                if (_parameters != value)
                {
                    _parameters = value;
                    RaisePropertyChanged("Parameters");
                }
            }
        }       
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                this.RaisePropertyChanged("IsChecked");
            }
        }
        public string State
        {
            get { return _state; }
            set { _state = value; }
        }
        public bool IsMethodChanged
        {
            get { return _isMethodChanged; }
            set { _isMethodChanged = value; }
        }
    }
}
