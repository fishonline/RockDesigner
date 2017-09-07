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
    /// RelationClassView.xaml 的交互逻辑
    /// </summary>
    public partial class RelationClassView : Window
    {
        public ClassViewModel ViewModel = new ClassViewModel();
        public RelationClassView()
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
            if (this.txtRelationPropertyAName.Text.Trim() == "")
            {
                MessageBox.Show("关联属性一的名称不能为空!", "提示");
                return;
            }
            if (this.txtRelationPropertyADbFieldName.Text.Trim() == "")
            {
                MessageBox.Show("关联属性一的存储名称不能为空!", "提示");
                return;
            }
            
            if (this.txtRelationPropertyBName.Text.Trim() == "")
            {
                MessageBox.Show("关联属性二的名称不能为空!", "提示");
                return;
            }
            if (this.txtRelationPropertyBDbFieldName.Text.Trim() == "")
            {
                MessageBox.Show("关联属性二的存储名称不能为空!", "提示");
                return;
            }
            if (ViewModel.EditState == "add")
            {
                if (ViewModel.CreateDesignClass())
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
