using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Rock.Common;
using Rock.DesignerModule.Interface;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.Service;
using Rock.DesignerModule.Views;
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
    public class NamespaceManagerViewModel : ViewModelBase
    {
        public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }
        public ApplicationDesignService ApplicationDesignService
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationDesignService>(); }
        }       
        private ObservableCollection<Namespace> _namespaceSource;
        private Namespace _selectedNamespace;       
       
        public ObservableCollection<Namespace> NamespaceSource
        {
            get { return _namespaceSource; }
            set { _namespaceSource = value; this.OnPropertyChanged("NamespaceSource"); }
        }

        public Namespace SelectedNamespace
        {
            get { return _selectedNamespace; }
            set
            {
                _selectedNamespace = value;
                this.OnPropertyChanged("SelectedNamespace");
            }
        }
       
        public DelegateCommand<Object> AddNameSpaceCommand { get; private set; }
        public DelegateCommand<Object> EditNamespaceCommand { get; private set; }
        public DelegateCommand<Object> DeleteNamespaceCommand { get; private set; }

        public DelegateCommand<Object> CheckedCommand { get; private set; }
        public DelegateCommand<Object> UncheckedCommand { get; private set; }
        public ICommand RowActivatedCommand { get; private set; }

        public NamespaceManagerViewModel()
        {
            AddNameSpaceCommand = new DelegateCommand<object>(AddNamespace);
            EditNamespaceCommand = new DelegateCommand<object>(EditNamespace, CanEditNamespaceExecute);
            DeleteNamespaceCommand = new DelegateCommand<object>(DeleteNamespace, CanDeleteNamespaceExecute);

            CheckedCommand = new DelegateCommand<object>(Checked);
            UncheckedCommand = new DelegateCommand<object>(UnChecked);
            RowActivatedCommand = new DelegateCommand<object>(RowActivate);

            NamespaceSource = ApplicationDesignCache.NamespaceSource;       
        }

        private bool CanEditNamespaceExecute(object arg)
        {
            if (NamespaceSource.Where(item => item.IsChecked).ToList().Count == 1)
            {
                return true;
            }
            return false;
        }

        private bool CanDeleteNamespaceExecute(object arg)
        {
            if (NamespaceSource.Where(item => item.IsChecked).ToList().Count > 0)
            {
                return true;
            }
            return false;
        }

        //选中选择框
        private void Checked(object parameter)
        {
            EditNamespaceCommand.RaiseCanExecuteChanged();
            DeleteNamespaceCommand.RaiseCanExecuteChanged();
        }
        //取消选中选择框
        private void UnChecked(object parameter)
        {
            EditNamespaceCommand.RaiseCanExecuteChanged();
            DeleteNamespaceCommand.RaiseCanExecuteChanged();
        }

        private void AddNamespace(object arg)
        {
            NameSpaceView nameSpaceView = new NameSpaceView();
            nameSpaceView.ViewModel.EditState = "add";
            nameSpaceView.Title = "新增命名空间";
            nameSpaceView.ShowDialog();
        }

        public void EditNamespace(object parameter)
        {
            Namespace nameSpace = NamespaceSource.Where(item => item.IsChecked == true).First();
            if (ApplicationDesignService.CanDeleteNamespace(nameSpace.NamespaceID))
            {
                NameSpaceView nameSpaceView = new NameSpaceView();
                nameSpaceView.ViewModel.Namespace = nameSpace;
                nameSpaceView.Title = "编辑命名空间";
                nameSpaceView.ViewModel.EditState = "modify";
                nameSpaceView.ShowDialog();
            }
            else
            {
                MessageBox.Show("命名空间: " + nameSpace.NamespaceName + " 已经存在 Class关联不可以编辑！", "提示");
            }
        }
        public void RowActivate(object parameter)
        {
            if (SelectedNamespace != null)
            {
                NameSpaceView nameSpaceView = new NameSpaceView();
                nameSpaceView.ViewModel.Namespace = SelectedNamespace;
                nameSpaceView.Title = "编辑命名空间";
                nameSpaceView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                nameSpaceView.ViewModel.EditState = "modify";
                nameSpaceView.ShowDialog();
            }
        }

        private void DeleteNamespace(object parameter)
        {
            MessageBoxResult result = MessageBox.Show("您确定要删除所选命名空间吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                List<Namespace> deletedNamespace = new List<Namespace>();
                foreach (var item in NamespaceSource)
                {
                    if (item.IsChecked)
                    {
                        if (!ApplicationDesignService.CanDeleteNamespace(item.NamespaceID))
                        {
                            MessageBox.Show("命名空间: " + item.NamespaceName + " 已经存在 Class关联不可以删除！", "提示");
                            deletedNamespace.Clear();
                            return;
                        }
                        else
                        {
                            deletedNamespace.Add(item);
                        }
                    }
                }

                foreach (var item in deletedNamespace)
                {
                    SystemService.DeleteObjectByID("Namespace", item.NamespaceID);
                    NamespaceSource.Remove(item);
                    Namespace namespaceModel = ApplicationDesignCache.NamespaceSource.Where(namespaceItem => namespaceItem.NamespaceID == item.NamespaceID).FirstOrDefault();
                    ApplicationDesignCache.NamespaceSource.Remove(namespaceModel);
                }
                MessageBox.Show("删除成功！");
            }
        }
    }
}
