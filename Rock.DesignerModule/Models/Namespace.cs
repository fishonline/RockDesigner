using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.DesignerModule.Models
{
    public class Namespace : NotificationObject
    {
        private int _namespaceID;
        private string _namespaceName;
        private string _description;
        private bool _isChecked;
        public int NamespaceID
        {
            get
            {
                return _namespaceID;
            }
            set
            {
                if (_namespaceID != value)
                {
                    _namespaceID = value;
                    RaisePropertyChanged("NamespaceID");
                }
            }
        }
        public string NamespaceName
        {
            get
            {
                return _namespaceName;
            }
            set
            {
                if (_namespaceName != value)
                {
                    _namespaceName = value;
                    RaisePropertyChanged("NamespaceName");
                }
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description != value)
                {
                    _description = value;
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
    }
}
