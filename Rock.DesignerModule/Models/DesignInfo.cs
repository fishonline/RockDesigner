using Microsoft.Practices.Prism.ViewModel;
using Rock.Dyn.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.DesignerModule.Models
{
    public class DesignInfo : NotificationObject
    {
        private string _validateType;
        private string _inputType;
        private string _gridHeader;
        private int _gridWidth;
        private string _gridColAlign;
        private string _gridColSorting;
        private string _gridColType;
        private string _referType;
        private bool _isRequired;
        private bool _isReadOnly;
        private string _queryForm;
        private string _state;
        private bool _isPropertyChanged;
        
        public string ValidateType
        {
            get { return _validateType; }
            set
            {
                if (_validateType != value)
                {
                    _validateType = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("ValidateType");
                }
            }
        }
        public string InputType
        {
            get { return _inputType; }
            set
            {
                if (_inputType != value)
                {
                    _inputType = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("InputType");
                }
            }
        }
       
        public string GridHeader
        {
            get { return _gridHeader; }
            set
            {
                if (_gridHeader != value)
                {
                    _gridHeader = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("GridHeader");
                }
            }
        }

        public int GridWidth
        {
            get { return _gridWidth; }
            set
            {
                if (_gridWidth != value)
                {
                    _gridWidth = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("GridWidth");
                }
            }
        }
        public string GridColAlign
        {
            get { return _gridColAlign; }
            set
            {
                if (_gridColAlign != value)
                {
                    _gridColAlign = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("GridColAlign");
                }
            }
        }
        public string GridColSorting
        {
            get { return _gridColSorting; }
            set
            {
                if (_gridColSorting != value)
                {
                    _gridColSorting = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("GridColSorting");
                }
            }
        }

        public string GridColType
        {
            get { return _gridColType; }
            set
            {
                if (_gridColType != value)
                {
                    _gridColType = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("GridColType");
                }
            }
        }       
        public string ReferType
        {
            get { return _referType; }
            set
            {
                if (_referType != value)
                {
                    _referType = value;
                    if (State != "added")
                    {
                        _isPropertyChanged = true;
                    }
                    RaisePropertyChanged("ReferType");
                }
            }
        }

        public bool IsRequired
        {
            get { return _isRequired; }
            set
            {
                _isRequired = value;
                if (State != "added")
                {
                    _isPropertyChanged = true;
                }
                this.RaisePropertyChanged("IsRequired");
            }
        }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                if (State != "added")
                {
                    _isPropertyChanged = true;
                }
                this.RaisePropertyChanged("IsReadOnly");
            }
        }
        public string QueryForm
        {
            get { return _queryForm; }
            set
            {
                _queryForm = value;
                if (State != "added")
                {
                    _isPropertyChanged = true;
                }
                this.RaisePropertyChanged("QueryForm");
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
        public DesignInfo()
        {

        }
    }
}
