using Microsoft.Practices.ServiceLocation;
using Rock.DesignerModule.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    /// UserControl1.xaml 的交互逻辑
    /// </summary>   
    public partial class ApplicationManagerView : UserControl
    {
       public ApplicationManagerViewModel ViewModel
       {
           get { return ServiceLocator.Current.GetInstance<ApplicationManagerViewModel>(); }
       }
        public ApplicationManagerView()
        {
            InitializeComponent();
            this.DataContext = ViewModel; 
        }
        //private void GridView_RowActivated(object sender, Telerik.Windows.Controls.GridView.RowEventArgs e)
        //{
        //    GridViewRowItem row = e.Row;
        //    ViewModel.ActivatedRowItem = row.Item;
        //}
    }
}
