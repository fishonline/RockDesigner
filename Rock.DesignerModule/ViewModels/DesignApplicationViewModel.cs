using Microsoft.Practices.ServiceLocation;
using Rock.Common;
using Rock.DesignerModule.Interface;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using Rock.Dyn.Core;
using Rock.Orm.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    public class DesignApplicationViewModel : ViewModelBase
    {
        public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }
        private ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }

        private DesignApplication _designApplication;
        private string _editState;

        public ApplicationManagerViewModel ApplicationManagerViewModel
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationManagerViewModel>(); }
        }        

        public DesignApplication DesignApplication
        {
            get { return _designApplication; }
            set { _designApplication = value; }
        }

        public string EditState
        {
            get { return _editState; }
            set { _editState = value; }
        }

        public DesignApplicationViewModel()
        {
            this.DesignApplication = new DesignApplication();
        }

        public bool AddDesignApplication()
        {
            DynEntity applicationDynEntity = new DynEntity("Application");
            DesignApplication.ApplicationID = SystemService.GetNextID("Application");
            applicationDynEntity["ApplicationID"] = DesignApplication.ApplicationID;
            applicationDynEntity["ApplicationName"] = DesignApplication.ApplicationName;
            applicationDynEntity["Description"] = DesignApplication.Description;
            //applicationDynEntity["LocalVersion"] = 1;
            //applicationDynEntity["CascadeVersion"] = 1;
            //applicationDynEntity["ReleaseVersion"] = "1";

            try
            {
                SystemService.AddDynEntity(applicationDynEntity);
                ApplicationManagerViewModel.ApplicationSource.Add(DesignApplication);
                return true;        
            }
            catch (Exception ex)
            {
                throw(ex);
            }       
        }

        public bool EditDesignApplication()
        {
            DynEntity applicationDynEntity = SystemService.GetDynEntityByID("Application", DesignApplication.ApplicationID); 
            applicationDynEntity["ApplicationID"] = DesignApplication.ApplicationID;
            applicationDynEntity["ApplicationName"] = DesignApplication.ApplicationName;
            applicationDynEntity["Description"] = DesignApplication.Description;

            try
            {
                SystemService.ModifyDynEntity(applicationDynEntity);              
                return true;
            }
            catch (Exception)
            {
                return false;
            }       
        }
    }
}