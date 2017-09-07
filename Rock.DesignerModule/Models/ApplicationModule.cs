using Microsoft.Practices.Prism.ViewModel;
using Rock.Dyn.Core;
using Rock.Orm.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.DesignerModule.Models
{
    public class ApplicationModule : NotificationObject
    {
        private int _moduleID;
        private bool _isChecked;
        private string _moduleName;
        private string _description;
        public int ModuleID
        {
            get { return _moduleID; }
            set { _moduleID = value; RaisePropertyChanged("ModuleID"); }
        }

        public string ModuleName
        {
            get { return _moduleName; }
            set { _moduleName = value; RaisePropertyChanged("ModuleName"); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged("Description"); }
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

        public ApplicationModule()
        {

        }

        public ApplicationModule(DynEntity module)
        {
            if (module != null && module.EntityType.Name == "Module")
            {
                this.ModuleID = (int)module["ModuleID"];
                this.ModuleName = module["ModuleName"] as string;
                this.Description = module["Description"] as string;
            }
        }
    }
}