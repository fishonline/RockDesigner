using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.Models;
using Rock.DesignerModule.ViewModels;
using System;
using System.Activities.Presentation.Toolbox;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rock.DesignerModule.Views
{
    /// <summary>
    /// WorkflowDesignerView.xaml 的交互逻辑
    /// </summary>
    public partial class WorkflowDesignerView : UserControl
    {
        public WorkflowDesignerViewModel ViewModel
        {
            get { return ServiceLocator.Current.GetInstance<WorkflowDesignerViewModel>(); }
        }
        public WorkflowDesignerView()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}
