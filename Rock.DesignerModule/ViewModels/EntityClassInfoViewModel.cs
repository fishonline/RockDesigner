using Rock.DesignerModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    public class EntityClassInfoViewModel : ViewModelBase
    {
        private DesignClass _designClass;
        private string _namespaceName;
      
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
    }
}
