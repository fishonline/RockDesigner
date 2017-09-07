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
    public class DesignApplication : NotificationObject
    {
        private int _applicationID;
        private string _applicationName;
        private string _description;
        private int version;
        private bool _isChecked;
        public int Version
        {
            get { return version; }
            set
            {
                version = value;
                this.RaisePropertyChanged("Version");
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

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                this.RaisePropertyChanged("Description");
            }
        }

        public string ApplicationName
        {
            get { return _applicationName; }
            set
            {
                _applicationName = value;
                this.RaisePropertyChanged("ApplicationName");
            }
        }

        public int ApplicationID
        {
            get { return _applicationID; }
            set
            {
                _applicationID = value;
                this.RaisePropertyChanged("ApplicationID");
            }
        }

        public DesignApplication()
        {

        }

        public DesignApplication(DynEntity application)
        {
            if (application != null && application.EntityType.Name == "Application")
            {
                this.ApplicationID = (int)application["ApplicationID"];
                this.ApplicationName = application["ApplicationName"] as string;
                this.Description = application["Description"] as string;               
            }
        }
    }
}
