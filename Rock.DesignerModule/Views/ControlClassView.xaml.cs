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
using System.Windows.Shapes;

namespace Rock.DesignerModule.Views
{
    /// <summary>
    /// ControlClassView.xaml 的交互逻辑
    /// </summary>
    public partial class ControlClassView : Window
    {
        public ClassViewModel ViewModel = new ClassViewModel();
        public ControlClassView()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //数据验证
            if (this.txtClassName.Text.Trim() == "")
            {
                MessageBox.Show("类型名称不能为空!", "提示");
                return;
            }

            if (ViewModel.EditState == "add")
            {
                if (ViewModel.CreateDesignClass())
                {
                    this.Close();
                }
            }

            if (ViewModel.EditState == "modify")
            {
                if (ViewModel.EditDesignClass())
                {
                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Cancel();
            this.Close();
        }
    }
}
