using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.Interface;
using Rock.DesignerModule.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.ViewModels
{
    public class WorkflowOpenViewModel : ViewModelBase
    {
        public ISystemService SystemService
        {
            get { return ServiceLocator.Current.GetInstance<ISystemService>(); }
        }
        public WorkflowDesignerViewModel WorkflowDesignerViewModel
        {
            get { return ServiceLocator.Current.GetInstance<WorkflowDesignerViewModel>(); }
        }
        public ObservableCollection<WorkflowModel> WorkflowSource { get; private set; }
        private WorkflowModel _selectedWorkflowModel;

        public WorkflowModel SelectedWorkflowModel
        {
            get { return _selectedWorkflowModel; }
            set
            {
                _selectedWorkflowModel = value;
                this.OnPropertyChanged("_selectedWorkflowModel");
            }
        }
        public ICommand BtnOKCommand { get; private set; }
        public ICommand RowActivatedCommand { get; private set; }

        public WorkflowOpenViewModel()
        {
            BtnOKCommand = new DelegateCommand<object>(BtnOK);
            RowActivatedCommand = new DelegateCommand<object>(RowActivate);
            WorkflowSource = new ObservableCollection<WorkflowModel>();
            LoadWorkflow();
        }

        public void LoadWorkflow()
        {
            WorkflowSource.Clear();
            DataTable dataTable = SystemService.GetDataTable("select WorkflowID, WorkflowName from Workflow");
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                WorkflowModel workflowModel = new WorkflowModel();
                workflowModel.WorkflowID = Convert.ToInt32(dataTable.Rows[i]["WorkflowID"]);
                workflowModel.WorkflowName = dataTable.Rows[i]["WorkflowName"].ToString();
                WorkflowSource.Add(workflowModel);
            }
        }

        public void BtnOK(object parameter)
        {
            if (SelectedWorkflowModel != null)
            {
                WorkflowDesignerViewModel.OpenWorkflow(SelectedWorkflowModel.WorkflowID);               
            }
        }

        public void RowActivate(object parameter)
        {
            if (SelectedWorkflowModel != null)
            {
                WorkflowDesignerViewModel.OpenWorkflow(SelectedWorkflowModel.WorkflowID);               
            }
        }
    }
}
