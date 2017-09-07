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
    /// DesignApplicationView.xaml 的交互逻辑
    /// </summary>
    public partial class DesignApplicationView : Window
    {
        public DesignApplicationViewModel ViewModel = new DesignApplicationViewModel();
        public DesignApplicationView()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //数据验证
            if (this.txtApplicationName.Text.Trim() == "")
            {
                MessageBox.Show("应用程序名称不能为空!", "提示");
                return;
            }

            if (ViewModel.EditState == "add")
            {
                if (ViewModel.AddDesignApplication())
                {
                    MessageBox.Show("添加成功!");
                    this.Close();
                }
            }

            if (ViewModel.EditState == "modify")
            {
                if (ViewModel.EditDesignApplication())
                {
                    MessageBox.Show("编辑成功!");
                    this.Close();
                }
            }
            this.Close();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
