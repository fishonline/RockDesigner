using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using Rock.Orm.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    public class ApplicationOpenViewModel : ViewModelBase
    {
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }
        public DocumentControlViewModel DocumentControlViewModel
        {
            get { return ServiceLocator.Current.GetInstance<DocumentControlViewModel>(); }
        }

        private DesignApplication _selectedApplication;
        public ObservableCollection<DesignApplication> ApplicationSource { get; private set; }
        public DesignApplication SelectedApplication
        {
            get { return _selectedApplication; }
            set
            {
                _selectedApplication = value;
                this.OnPropertyChanged("SelectedApplication");
            }
        }

        public ICommand BtnOKCommand { get; private set; }
        public ICommand RowActivatedCommand { get; private set; }
        public ApplicationOpenViewModel()
        {
            BtnOKCommand = new DelegateCommand<object>(BtnOK);
            ApplicationSource = new ObservableCollection<DesignApplication>();
            RowActivatedCommand = new DelegateCommand<object>(RowActivate);
            List<DynEntity> applications = ApplicationDesignService.GetAllApplicationCollection();
            foreach (var application in applications)
            {
                DesignApplication applicationViewModel = new DesignApplication(application);
                ApplicationSource.Add(applicationViewModel);
            }   
        }

        public void BtnOK(object parameter)
        {
            if (SelectedApplication != null)
            {
                if (SelectedApplication.ApplicationID != ApplicationDesignCache.ApplicationID)
                {
                    ApplicationDesignCache.ApplicationID = SelectedApplication.ApplicationID;
                    ApplicationDesignCache.ApplicationName = SelectedApplication.ApplicationName;
                    DocumentControlViewModel.OpenDesignerView();
                }
                else
                {
                    DocumentControlViewModel.OpenDesignerView();
                }
            }
        }

        public void RowActivate(object parameter)
        {
            if (SelectedApplication != null)
            {
                if (SelectedApplication.ApplicationID != ApplicationDesignCache.ApplicationID)
                {
                    ApplicationDesignCache.ApplicationID = SelectedApplication.ApplicationID;
                    ApplicationDesignCache.ApplicationName = SelectedApplication.ApplicationName;
                    DocumentControlViewModel.OpenDesignerView();
                }
                else
                {
                    DocumentControlViewModel.OpenDesignerView();
                }
            }
        }
    }
}
