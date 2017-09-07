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

namespace Rock.DesignerModule.Views
{
    /// <summary>
    /// RelationClassInfoView.xaml 的交互逻辑
    /// </summary>
    public partial class RelationClassInfoView : UserControl
    {
        public RelationClassInfoViewModel ViewModel = new RelationClassInfoViewModel();
        public RelationClassInfoView()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}
