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
    /// UIInfoView.xaml 的交互逻辑
    /// </summary>
    public partial class UIInfoView : Window
    {
        public UIInfoViewModel ViewModel = new UIInfoViewModel();
        public UIInfoView()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            ////数据验证
            //if (this.txtPropertyName.Text.Trim() == "")
            //{
            //    MessageBox.Show("属性名称不能为空!", "提示");
            //    return;
            //}
            if (ViewModel.UIDesignInfo.IsPropertyChanged)
            {
                if (ViewModel.EditUIDesignInfo())
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
