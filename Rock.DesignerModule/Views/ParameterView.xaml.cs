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
    /// ParameterView.xaml 的交互逻辑
    /// </summary>
    public partial class ParameterView : Window
    {
        public ParameterViewModel ViewModel = new ParameterViewModel();
        public ParameterView()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //数据验证
            if (this.txtParaName.Text.Trim() == "")
            {
                MessageBox.Show("参数名称不能为空!", "提示");
                return;
            }


            if (ViewModel.EditState == "add")
            {
                if (ViewModel.AddParameter())
                {
                    this.Close();
                }
            }

            if (ViewModel.EditState == "modify")
            {
                if (ViewModel.EditParameter())
                {
                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
