using Microsoft.Practices.Prism.ViewModel;
using Rock.Dyn.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.DesignerModule.Models
{
    public class DesignProperty : NotificationObject
    {
        private int _propertyID;        
        private string _propertyName;
        private string _displayName;        
        private string _defaultValue;
        private bool _isNullable = true;
        private bool _isPersistable = true;
        private string _description;
        private string _dataType;       
        private string _dbFieldName;
        private int? _dbFieldLength;
        private string _sqlType;
        private string _relationType;
        private bool _isChecked;
        private string _collectionType;
        private string _structName;
        private bool _isPrimarykey;
        private bool _isQueryProperty;
        private bool _isArray;
        private int? _decimalDigits;
        private DesignInfo _designInfo;
        private string _state;
        private string _originalDataType;
        private bool _isPropertyChanged;
      
        public int PropertyID
        {
            get { return _propertyID; }
            set { _propertyID = value; }
        }
        public string PropertyName
        {
            get { return _propertyName; }
            set
            {
                if (_propertyName != value)
                {
                    _propertyName = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("PropertyName");
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
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("DisplayName");
                }
            }
        }
        public string CollectionType
        {
            get { return _collectionType; }
            set
            {
                _collectionType = value;
                if (State != "added")
                {
                    _isPropertyChanged = true;
                }
                RaisePropertyChanged("CollectionType");
            }
        }
        public string DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                if (_defaultValue != value)
                {
                    _defaultValue = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("DefaultValue");
                }
            }
        }
        public bool IsNullable
        {
            get { return _isNullable; }
            set
            {
                if (_isNullable != value)
                {
                    _isNullable = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("IsNullable");
                }
            }
        }
        public bool IsPersistable
        {
            get { return _isPersistable; }
            set
            {
                if (_isPersistable != value)
                {
                    _isPersistable = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("IsPersistable");
                }
            }
        }
        public bool IsPrimarykey
        {
            get { return _isPrimarykey; }
            set
            {
                if (_isPrimarykey != value)
                {
                    _isPrimarykey = value;
                    RaisePropertyChanged("IsPrimarykey");
                }
            }
        }       
        public bool IsQueryProperty
        {
            get { return _isQueryProperty; }
            set
            {
                if (_isQueryProperty != value)
                {
                    _isQueryProperty = value;
                    RaisePropertyChanged("IsQueryProperty");
                }
            }
        }
        public bool IsArray
        {
            get { return _isArray; }
            set
            {
                if (_isArray != value)
                {
                    _isArray = value;
                    RaisePropertyChanged("IsArray");
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
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("Description");
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
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("DataType");
                }
            }
        }
        public string DbFieldName
        {
            get { return _dbFieldName; }
            set
            {
                if (_dbFieldName != value)
                {
                    _dbFieldName = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("DbFieldName");
                }
            }
        }
        public int? DbFieldLength
        {
            get { return _dbFieldLength; }
            set
            {
                if (_dbFieldLength != value)
                {
                    _dbFieldLength = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("DbFieldLength");
                }
            }
        }
        public string SqlType
        {
            get { return _sqlType; }
            set
            {
                if (_sqlType != value)
                {
                    _sqlType = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("SqlType");
                }
            }
        }
        public string RelationType
        {
            get { return _relationType; }
            set
            {
                _relationType = value;
                if (State != "added")
                {
                    _isPropertyChanged = true;
                }
                RaisePropertyChanged("RelationType");
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
        public string StructName
        {
            get { return _structName; }
            set
            {
                if (_structName != value)
                {
                    _structName = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("StructName");
                }
            }
        }
        public int? DecimalDigits
        {
            get { return _decimalDigits; }
            set
            {
                if (_decimalDigits != value)
                {
                    _decimalDigits = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }                   
                    RaisePropertyChanged("DecimalDigits");
                }
            }
        }
        public string OriginalDataType
        {
            get { return _originalDataType; }
            set { _originalDataType = value; }
        }

        public DesignInfo UIDesignInfo
        {
            get { return _designInfo; }
            set
            {
                if (_designInfo != value)
                {
                    _designInfo = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("UIDesignInfo");
                }
            }
        }
        public string State
        {
            get { return _state; }
            set { _state = value; }
        }
        public bool IsPropertyChanged
        {
            get { return _isPropertyChanged; }
            set { _isPropertyChanged = value; }
        }
        public DesignProperty()
        {

        } 
    }
}
