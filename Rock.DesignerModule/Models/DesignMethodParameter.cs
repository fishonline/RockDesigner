using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.DesignerModule.Models
{
    public class DesignMethodParameter : NotificationObject
    {
        private string _parameterName;
        private string _collectionType;
        private string _dataType;
        private string _structName;
        private string _description;
        private bool _isChecked;
        private string _state;
        private bool _isParameterChanged;
       
        public string ParameterName
        {
            get { return _parameterName; }
            set
            {
                if (_parameterName != value)
                {
                    _parameterName = value;
                    _isParameterChanged = true;
                    RaisePropertyChanged("ParameterName");
                }
            }
        }       
        public string CollectionType
        {
            get { return _collectionType; }
            set
            {
                if (_collectionType != value)
                {
                    _collectionType = value;
                    _isParameterChanged = true;
                    RaisePropertyChanged("CollectionType");
                }
            }
        }
        public string DataType
        {
            get { return _dataType; }
            set
            {
                if (_dataType != value)
                {
                    _dataType = value;
                    _isParameterChanged = true;
                    RaisePropertyChanged("DataType");
                }
            }
        }
        public string StructName
        {
            get { return _structName; }
            set
            {
                if (_structName != value)
                {
                    _structName = value;
                    _isParameterChanged = true;
                    RaisePropertyChanged("StructName");
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
                    _isParameterChanged = true;
                    RaisePropertyChanged("Description");
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
        public bool IsParameterChanged
        {
            get { return _isParameterChanged; }
            set { _isParameterChanged = value; }
        }   
    }
}
