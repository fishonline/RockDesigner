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
    /// MethodView.xaml 的交互逻辑
    /// </summary>
    public partial class MethodView : Window
    {
        public MethodViewModel ViewModel = new MethodViewModel();
        public MethodView()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //数据验证
            if (this.txtMethodName.Text.Trim() == "")
            {
                MessageBox.Show("方法名称不能为空!", "提示");
                return;
            }


            if (ViewModel.EditState == "add")
            {
                if (ViewModel.AddMethod())
                {
                    this.Close();
                }
            }

            if (ViewModel.EditState == "modify")
            {
                if (ViewModel.EditMethod())
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
