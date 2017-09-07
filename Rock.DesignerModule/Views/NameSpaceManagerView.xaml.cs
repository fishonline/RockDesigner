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
using Telerik.Windows.Controls.GridView;

namespace Rock.DesignerModule.Views
{
    /// <summary>
    /// NameSpaceManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class NameSpaceManagerView : UserControl
    {
        public NamespaceManagerViewModel ViewModel
        {
            get { return ServiceLocator.Current.GetInstance<NamespaceManagerViewModel>(); }
        }
        public NameSpaceManagerView()
        {
            InitializeComponent();
            this.DataContext = ViewModel; 
        }        
    }
}
