using Rock.DesignerModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    public class ControlClassInfoViewModel : ViewModelBase
    {
        private DesignClass _designClass;
        private string _namespaceName;
        private bool _isServiceProtocol;

        public DesignClass DesignClass
        {
            get { return _designClass; }
            set
            {
                _designClass = value;
                this.OnPropertyChanged("DesignClass");
            }
        }
        public string NamespaceName
        {
            get { return _namespaceName; }
            set
            {
                _namespaceName = value;
                this.OnPropertyChanged("NamespaceName");
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
                    this.OnPropertyChanged("IsServiceProtocol");
                }
            }
        }
    }
}
