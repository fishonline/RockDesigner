﻿using Rock.DesignerModule.ViewModels;
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
    /// WorkflowOpenWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WorkflowOpenWindow : Window
    {
        public WorkflowOpenViewModel ViewModel = new WorkflowOpenViewModel();
        public WorkflowOpenWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        private void btnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ViewModel.SelectedWorkflowModel == null)
            {
                MessageBox.Show("请先选择要打开的工作流!", "提示");
            }
            else
            {

                this.Close();
            }
        }

        private void RadGridView_RowActivated(object sender, Telerik.Windows.Controls.GridView.RowEventArgs e)
        {
            if (ViewModel.SelectedWorkflowModel == null)
            {
                MessageBox.Show("请先选择要打开的工作流!", "提示");
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


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
