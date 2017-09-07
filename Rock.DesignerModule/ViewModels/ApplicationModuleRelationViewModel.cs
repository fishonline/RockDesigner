using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Rock.Common;
using Rock.DesignerModule.Interface;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using Rock.Dyn.Core;
using Rock.Orm.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    public class ApplicationModuleRelationViewModel : ViewModelBase
    {
          public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }       
         private ObservableCollection<DesignApplication> _applicationSource = new ObservableCollection<DesignApplication>();
        private DesignApplication _selectedApplication;
        private ObservableCollection<ApplicationModule> _addedModuleList = new ObservableCollection<ApplicationModule>();
        private ObservableCollection<ApplicationModule> _moduleList = new ObservableCollection<ApplicationModule>();

        public ObservableCollection<DesignApplication> ApplicationSource
        {
            get { return _applicationSource; }
            set { _applicationSource = value; this.OnPropertyChanged("ApplicationSource"); }
        }
        public ObservableCollection<ApplicationModule> AddedModuleList
        {
            get { return _addedModuleList; }
            set { _addedModuleList = value; this.OnPropertyChanged("AddedModuleList"); }
        }
        public ObservableCollection<ApplicationModule> ModuleList
        {
            get { return _moduleList; }
            set { _moduleList = value; this.OnPropertyChanged("ModuleList"); }
        }
        public DesignApplication SelectedApplication
        {
            get { return _selectedApplication; }
            set
            {
                _selectedApplication = value;
                this.OnPropertyChanged("SelectedApplication");
            }
        }      

        public ICommand SelectionChangedCommand { get; private set; }

        public ApplicationModuleRelationViewModel()
        {
            SelectionChangedCommand = new DelegateCommand<object>(SelectionChanged);

            List<DynEntity> applications = ApplicationDesignService.GetAllApplicationCollection();
            ApplicationSource.Clear();
            foreach (var application in applications)
            {
                DesignApplication designApplication = new DesignApplication(application);
                ApplicationSource.Add(designApplication);
                SelectedApplication = ApplicationSource[0];
            }
            InitModules();
        }

        public void AppRelationModule(int moduleID)
        {
            DynEntity applicationModule = new DynEntity("ApplicationModule");
            applicationModule["ApplicationID"] = SelectedApplication.ApplicationID;
            applicationModule["ModuleID"] = moduleID;
            SystemService.AddDynEntity(applicationModule);
        }

        public void AppUnRelationModule(int moduleID)
        {
            SystemService.DeleteObjectByID("ApplicationModule", SelectedApplication.ApplicationID, moduleID);
        }

        private void InitModules()
        {
            ModuleList.Clear();
            AddedModuleList.Clear();
            if (SelectedApplication != null)
            {
                List<DynEntity> allModules = ApplicationDesignService.GetAllApplictionModuleCollection();
                List<DynEntity> currentApplicationModules = ApplicationDesignService.GetAplicationModulesByAplicationID(SelectedApplication.ApplicationID);

                foreach (var module in allModules)
                {
                    bool isHave = false;
                    foreach (var addedModule in currentApplicationModules)
                    {
                        if (Convert.Equals(addedModule["ModuleID"], module["ModuleID"]))
                        {
                            AddedModuleList.Add(new ApplicationModule() { ModuleName = module["ModuleName"] as string, ModuleID = (int)module["ModuleID"] });
                            isHave = true;
                            break;
                        }
                    }
                    if (!isHave)
                    {
                        ModuleList.Add(new ApplicationModule() { ModuleName = module["ModuleName"] as string, ModuleID = (int)module["ModuleID"] });
                    }
                }
            } 
        }

        private void SelectionChanged(object arg)
        {
            InitModules();
        }
    }
}
