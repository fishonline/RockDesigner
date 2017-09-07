using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rock.ActivityDesignerLibrary
{
    // ArtificialDesigner.xaml 的交互逻辑
    public partial class ArtificialDesigner
    {
        public ArtificialDesigner()
        {
            InitializeComponent();
        }

        private void ActivityDesigner_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string description = ((System.Activities.Expressions.Literal<string>)(((this.ModelItem.Properties["Description"].Value).Content).ComputedValue)).Value;
            int workflowActivityID = (int)this.ModelItem.Properties["WorkflowActivityID"].Value.Source.ComputedValue;
            //int workflowActivityID = ((Artificial)(this.ModelItem.Source.ComputedValue)).WorkflowActivityID;

            ArtificialGuide artificialGuide = new ArtificialGuide();
            artificialGuide.Description = description;
            artificialGuide.WorkflowActivityID = workflowActivityID;

            bool? result = artificialGuide.ShowDialog();

            if (result == true)
            {
                System.Activities.InArgument<string> descriptionArg = new System.Activities.InArgument<string>();
                descriptionArg = artificialGuide.txtDescription.Text;
                this.ModelItem.Properties["Description"].SetValue(descriptionArg);

                this.ModelItem.Properties["WorkflowActivityID"].SetValue(artificialGuide.WorkflowActivityID);
            }
        }
    }
}
