using Microsoft.Practices.Prism.ViewModel;
using Rock.Dyn.Core;
using Rock.Orm.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Rock.DesignerModule.Models
{
    public class DesignClass : NotificationObject
    {
        private int _classID;
        private string _className;       
        private string _displayName;
        private int _mainType;
        private ImageSource _mainTypeImage;
        private string _mainTypeName;       
        private string _baseClassName;
        private string _description;        
        private bool _isPersistable;
        private int _namespaceID;
        private int _moduleID;
        private bool _isChecked;
        private string _state;
        DesignProperty _relationPropertyA;
        DesignProperty _relationPropertyB;
        private bool _isServiceProtocol;
        private string _interfaceName;        
       

        private ObservableCollection<DesignProperty> _properties = new ObservableCollection<DesignProperty>();
        private List<DesignProperty> _deletedProperties = new List<DesignProperty>();
        private ObservableCollection<DesignMethod> _methodes = new ObservableCollection<DesignMethod>();
        private ObservableCollection<DesignMethod> _deletedMethodes = new ObservableCollection<DesignMethod>();

       
        private List<DynObject> _attributes = new List<DynObject>();
        public int ClassID
        {
            get { return _classID; }
            set
            {
                if (_classID != value)
                {
                    _classID = value;
                    RaisePropertyChanged("ClassID");
                }
            }
        }
        public string ClassName
        {
            get { return _className; }
            set
            {
                if (_className != value)
                {                    
                    _className = value;
                    RaisePropertyChanged("ClassName");
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
                    RaisePropertyChanged("DisplayName");
                }
            }
        }
        public int MainType
        {
            get { return _mainType; }
            set
            {
                if (_mainType != value)
                {
                    _mainType = value;
                    RaisePropertyChanged("MainType");
                }
            }
        }
        public ImageSource MainTypeImage
        {
            get { return _mainTypeImage; }
            set
            {
                if (_mainTypeImage != value)
                {
                    _mainTypeImage = value;
                    RaisePropertyChanged("MainTypeImage");
                }
            }
        }
        public string MainTypeName
        {
            get { return _mainTypeName; }
            set
            {
                if (_mainTypeName != value)
                {
                    _mainTypeName = value;
                    RaisePropertyChanged("MainTypeName");
                }
            }
        }
        
        public string BaseClassName
        {
            get { return _baseClassName; }
            set
            {
                if (_baseClassName != value)
                {
                    _baseClassName = value;
                    RaisePropertyChanged("BaseClassName");
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
                    RaisePropertyChanged("Description");
                }
            }
        }
        public int NamespaceID
        {
            get { return _namespaceID; }
            set { _namespaceID = value; RaisePropertyChanged("NamespaceID"); }
        }
        public int ModuleID
        {
            get { return _moduleID; }
            set { _moduleID = value; RaisePropertyChanged("ModuleID"); }
        }
        
        public ObservableCollection<DesignProperty> Properties
        {
            get
            {
                if (_properties != null)
                {
                    ListCollectionView myAllMethod = (ListCollectionView)CollectionViewSource.GetDefaultView(_properties);
                    myAllMethod.SortDescriptions.Add(new SortDescription("PropertyName", ListSortDirection.Ascending));
                }
                return _properties;
            }
            set { _properties = value; RaisePropertyChanged("Properties"); }
        }
        public List<DesignProperty> DeletedProperties
        {
            get { return _deletedProperties; }
            set { _deletedProperties = value; }
        }
        public ObservableCollection<DesignMethod> Methodes
        {
            get { return _methodes; }
            set { _methodes = value; RaisePropertyChanged("Methodes"); }
        }
        public ObservableCollection<DesignMethod> DeletedMethodes
        {
            get { return _deletedMethodes; }
            set { _deletedMethodes = value; }
        } 
        public List<DynObject> Attributes
        {
            get { return _attributes; }
            set { _attributes = value; RaisePropertyChanged("Attributes"); }
        }
       
        public bool IsPersistable
        {
            get { return _isPersistable; }
            set
            {
                if (_isPersistable != value)
                {
                    _isPersistable = value;
                    RaisePropertyChanged("IsPersistable");
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
        public DesignProperty RelationPropertyA
        {
            get { return _relationPropertyA; }
            set
            {
                _relationPropertyA = value;
                this.RaisePropertyChanged("RelationPropertyA");
            }
        }
        public DesignProperty RelationPropertyB
        {
            get { return _relationPropertyB; }
            set
            {
                _relationPropertyB = value;
                this.RaisePropertyChanged("RelationPropertyB");
            }
        }
        public bool IsServiceProtocol
        {
            get { return _isServiceProtocol; }
            set
            {
                if (_isServiceProtocol != value)
                {
                    _isServiceProtocol = value;
                    RaisePropertyChanged("IsServiceProtocol");
                }
            }
        }
        public string InterfaceName
        {
            get { return _interfaceName; }
            set { _interfaceName = value; }
        }
        public DesignClass()
        {           
        }
    }
}
