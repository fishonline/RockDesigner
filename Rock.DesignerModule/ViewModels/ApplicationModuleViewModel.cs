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
    public class ApplicationModuleViewModel : ViewModelBase
    {
        public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }    
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }
        private ApplicationModule _module;
        private string _editState;
        public ApplicationModuleManagerViewModel ModuleManagerViewModel
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationModuleManagerViewModel>(); }
        }

        public ApplicationModule Module
        {
            get { return _module; }
            set { _module = value; }
        }
        public string EditState
        {
            get { return _editState; }
            set { _editState = value; }
        }
        public ApplicationModuleViewModel()
        {
            this.Module = new ApplicationModule();
        }
        public bool AddModule()
        {
            DynEntity moduleDynEntity = new DynEntity("Module");
            Module.ModuleID = SystemService.GetNextID("Module");
            moduleDynEntity["ModuleID"] = Module.ModuleID;
            moduleDynEntity["ModuleName"] = Module.ModuleName;
            moduleDynEntity["Description"] = Module.Description;
            //moduleDynEntity["LocalVersion"] = 1;
            //moduleDynEntity["CascadeVersion"] = 1;
            //moduleDynEntity["ReleaseVersion"] = "1";

            try
            {
                SystemService.AddDynEntity(moduleDynEntity);
                ModuleManagerViewModel.ModuleSouce.Add(Module);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool EditModule()
        {
            DynEntity moduleDynEntity = SystemService.GetDynEntityByID("Module", Module.ModuleID); 
            moduleDynEntity["ModuleID"] = Module.ModuleID;
            moduleDynEntity["ModuleName"] = Module.ModuleName;
            moduleDynEntity["Description"] = Module.Description;

            try
            {
                SystemService.ModifyDynEntity(moduleDynEntity);
                return true;
            }
            catch (Exception ex)
            {
                throw (ex);
            } 
        }
    }
}