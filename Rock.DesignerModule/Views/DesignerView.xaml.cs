using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.ViewModels;
using System;
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
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.Views
{
    /// <summary>
    /// DesignerView.xaml 的交互逻辑
    /// </summary>
    public partial class DesignerView : UserControl
    {
        public DesignerViewModel ViewModel
        {
            get { return ServiceLocator.Current.GetInstance<DesignerViewModel>(); }
        }
        public DesignerView()
        {
            InitializeComponent();
            ViewModel.LoadApplicationSystem();
            this.DataContext = ViewModel;
        }       
    }
}
