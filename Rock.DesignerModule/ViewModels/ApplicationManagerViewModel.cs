using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Rock.Common;
using Rock.DesignerModule.Interface;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using Rock.DesignerModule.Views;
using Rock.Dyn.Core;
using Rock.Orm.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    [Export]
    public class ApplicationManagerViewModel : ViewModelBase
    {
        public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }       

        private ObservableCollection<DesignApplication> _applicationSource;
        private DesignApplication _selectedApplication;

        public ObservableCollection<DesignApplication> ApplicationSource
        {
            get { return _applicationSource; }
            set { _applicationSource = value; this.OnPropertyChanged("ApplicationSource"); }
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

        public DelegateCommand<Object> AddDesignApplicationCommand { get; private set; }
        public DelegateCommand<Object> EditDesignApplicationCommand { get; private set; }
        public DelegateCommand<Object> DeleteDesignApplicationCommand { get; private set; }

        public DelegateCommand<Object> CheckedCommand { get; private set; }
        public DelegateCommand<Object> UncheckedCommand { get; private set; }
        public ICommand RowActivatedCommand { get; private set; }

        public ApplicationManagerViewModel()
        {
            AddDesignApplicationCommand = new DelegateCommand<object>(AddDesignApplication);
            EditDesignApplicationCommand = new DelegateCommand<object>(EditDesignApplication, CanEditDesignApplicationExecute);
            DeleteDesignApplicationCommand = new DelegateCommand<object>(DeleteDesignApplication, CanDeleteDesignApplicationExecute);
            RowActivatedCommand = new DelegateCommand<object>(RowActivate);

            CheckedCommand = new DelegateCommand<object>(Checked);
            UncheckedCommand = new DelegateCommand<object>(UnChecked);

            _applicationSource = new ObservableCollection<DesignApplication>();

            List<DynEntity> applications = ApplicationDesignService.GetAllApplicationCollection();
            ApplicationSource.Clear();
            foreach (var application in applications)
            {
                DesignApplication applicationViewModel = new DesignApplication(application);
                ApplicationSource.Add(applicationViewModel);
            }
        }

        private bool CanEditDesignApplicationExecute(object arg)
        {
            if (ApplicationSource.Where(item => item.IsChecked).ToList().Count == 1)
            {
                return true;
            }
            return false;
        }

        private bool CanDeleteDesignApplicationExecute(object arg)
        {
            if (ApplicationSource.Where(item => item.IsChecked).ToList().Count > 0)
            {
                return true;
            }
            return false;
        }

        //选中选择框
        private void Checked(object parameter)
        {
            EditDesignApplicationCommand.RaiseCanExecuteChanged();
            DeleteDesignApplicationCommand.RaiseCanExecuteChanged();
        }
        //取消选中选择框
        private void UnChecked(object parameter)
        {
            EditDesignApplicationCommand.RaiseCanExecuteChanged();
            DeleteDesignApplicationCommand.RaiseCanExecuteChanged();
        }

        private void AddDesignApplication(object arg)
        {
            DesignApplicationView designApplicationView = new DesignApplicationView();
            designApplicationView.ViewModel.EditState = "add";
            designApplicationView.Title = "新增应用程序";
            designApplicationView.ShowDialog();
        }

        public void EditDesignApplication(object parameter)
        {
            DesignApplicationView designApplicationView = new DesignApplicationView();
            designApplicationView.ViewModel.DesignApplication = ApplicationSource.Where(item => item.IsChecked == true).First();
            designApplicationView.Title = "编辑应用程序";
            designApplicationView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            designApplicationView.ViewModel.EditState = "modify";
            designApplicationView.ShowDialog();     
        }
        public void RowActivate(object parameter)
        {
            if (SelectedApplication != null)
            {
                DesignApplicationView designApplicationView = new DesignApplicationView();
                designApplicationView.ViewModel.DesignApplication = SelectedApplication;
                designApplicationView.Title = "编辑应用程序";
                designApplicationView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                designApplicationView.ViewModel.EditState = "modify";
                designApplicationView.ShowDialog();
            }
        }

        private void DeleteDesignApplication(object parameter)
        {
            MessageBoxResult result = MessageBox.Show("您确定要删除所选应用程序吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                List<DesignApplication> deletedDesignApplications = new List<DesignApplication>();
                foreach (var item in ApplicationSource)
                {
                    if (item.IsChecked)
                    {
                        if (!ApplicationDesignService.CanDeleteApplication(item.ApplicationID))
                        {
                            MessageBox.Show("应用程序: " + item.ApplicationName + " 已经存在模块关联不可以删除！", "提示");
                            deletedDesignApplications.Clear();
                            return;
                        }
                        else
                        {
                            deletedDesignApplications.Add(item);
                        }
                    }
                }

                foreach (var designApplication in deletedDesignApplications)
                {
                    SystemService.DeleteObjectByID("Application", designApplication.ApplicationID);
                    ApplicationSource.Remove(designApplication);
                }
                MessageBox.Show("删除成功！");
            }
        }        
    }
}
