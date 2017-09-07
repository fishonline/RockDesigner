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
    public class ApplicationModuleManagerViewModel : ViewModelBase
    {
        public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }   
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }

        private ObservableCollection<ApplicationModule> _moduleSouce;
        private ApplicationModule _selectedModule;
        //private object _activatedRowItem;

        public ObservableCollection<ApplicationModule> ModuleSouce
        {
            get { return _moduleSouce; }
            set { _moduleSouce = value; this.OnPropertyChanged("ModuleSouce"); }
        }

        public ApplicationModule SelectedModule
        {
            get { return _selectedModule; }
            set
            {
                _selectedModule = value;
                this.OnPropertyChanged("SelectedModule");
            }
        }
        //public object ActivatedRowItem
        //{
        //    get { return _activatedRowItem; }
        //    set { _activatedRowItem = value; }
        //}
        public DelegateCommand<Object> AddModuleCommand { get; private set; }
        public DelegateCommand<Object> EditModuleCommand { get; private set; }
        public DelegateCommand<Object> DeleteModuleCommand { get; private set; }

        public DelegateCommand<Object> CheckedCommand { get; private set; }
        public DelegateCommand<Object> UncheckedCommand { get; private set; }
        public ICommand RowActivatedCommand { get; private set; }

        public ApplicationModuleManagerViewModel()
        {
            AddModuleCommand = new DelegateCommand<object>(AddModule);
            EditModuleCommand = new DelegateCommand<object>(EditModule, CanEditModuleExecute);
            DeleteModuleCommand = new DelegateCommand<object>(DeleteModule, CanDeleteModuleExecute);

            CheckedCommand = new DelegateCommand<object>(Checked);
            UncheckedCommand = new DelegateCommand<object>(UnChecked);
            RowActivatedCommand = new DelegateCommand<object>(RowActivate);

            _moduleSouce = new ObservableCollection<ApplicationModule>();
            List<DynEntity> modules = ApplicationDesignService.GetAllApplictionModuleCollection();
            ModuleSouce.Clear();
            foreach (var module in modules)
            {
                ApplicationModule moduleModel = new ApplicationModule(module);
                ModuleSouce.Add(moduleModel);
            }
        }

        private bool CanEditModuleExecute(object arg)
        {
            if (ModuleSouce.Where(module => module.IsChecked).ToList().Count == 1)
            {
                return true;
            }
            return false;
        }

        private bool CanDeleteModuleExecute(object arg)
        {
            if (ModuleSouce.Where(module => module.IsChecked).ToList().Count > 0)
            {
                return true;
            }
            return false;
        }

        //选中选择框
        private void Checked(object parameter)
        {
            EditModuleCommand.RaiseCanExecuteChanged();
            DeleteModuleCommand.RaiseCanExecuteChanged();
        }
        //取消选中选择框
        private void UnChecked(object parameter)
        {
            EditModuleCommand.RaiseCanExecuteChanged();
            DeleteModuleCommand.RaiseCanExecuteChanged();
        }

        private void AddModule(object arg)
        {
            ApplicationModuleView aplicationModuleView = new ApplicationModuleView();
            aplicationModuleView.ViewModel.EditState = "add";
            aplicationModuleView.Title = "新增应用模块";
            aplicationModuleView.ShowDialog();
        }

        public void EditModule(object parameter)
        {
            ApplicationModuleView aplicationModuleView = new ApplicationModuleView();
            aplicationModuleView.ViewModel.Module = ModuleSouce.Where(item => item.IsChecked == true).First();
            aplicationModuleView.Title = "编辑应用模块";
            aplicationModuleView.ViewModel.EditState = "modify";
            aplicationModuleView.ShowDialog();
        }
        public void RowActivate(object parameter)
        {
            if (SelectedModule != null)
            {
                ApplicationModuleView aplicationModuleView = new ApplicationModuleView();
                aplicationModuleView.ViewModel.Module = SelectedModule;
                aplicationModuleView.Title = "编辑应用模块";
                aplicationModuleView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                aplicationModuleView.ViewModel.EditState = "modify";
                aplicationModuleView.ShowDialog();
            }
        }
        private void DeleteModule(object parameter)
        {
            MessageBoxResult result = MessageBox.Show("您确定要删除所选应用模块吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                List<ApplicationModule> deletedModules = new List<ApplicationModule>();
                foreach (var aplicationModule in ModuleSouce)
                {
                    if (aplicationModule.IsChecked)
                    {
                        if (!ApplicationDesignService.CanDeleteAplicationModule(aplicationModule.ModuleID))
                        {
                            MessageBox.Show("应用模块: " + aplicationModule.ModuleName + " 已经存在关联不可以删除！", "提示");
                            deletedModules.Clear();
                            return;
                        }
                        else
                        {
                            deletedModules.Add(aplicationModule);
                        }
                    }
                }

                foreach (var aplicationModule in deletedModules)
                {
                    SystemService.DeleteObjectByID("Module", aplicationModule.ModuleID);
                    ModuleSouce.Remove(aplicationModule);
                }
                MessageBox.Show("删除成功！");
            }
        }
    }
}
