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

namespace Rock.ActivityDesignerLibrary
{
    /// <summary>
    /// ArtificialGuide.xaml 的交互逻辑
    /// </summary>
    public partial class ArtificialGuide : Window
    {
        DesignService designService = new DesignService();      
        private string _description = "";
        private int _workflowActivityID = 0;
     
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public ArtificialGuide()
        {
            InitializeComponent();
        }
        public int WorkflowActivityID
        {
            get { return _workflowActivityID; }
            set { _workflowActivityID = value; }
        }

        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {  

            if (_workflowActivityID == 0)
            {
                //获取工作流活动的ID
                _workflowActivityID = designService.GetNextID("WorkflowActivity");
            }
            _description = this.txtDescription.Text;
            //设置ShowDialog的返回值
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //设置ShowDialog的返回值
            this.DialogResult = false;
            this.Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {           
            this.txtDescription.Text = Description;            
        }
    }
}
