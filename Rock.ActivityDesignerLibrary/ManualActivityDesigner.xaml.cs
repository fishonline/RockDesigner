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
    // ManualActivityDesigner.xaml 的交互逻辑
    public partial class ManualActivityDesigner
    {
        public ManualActivityDesigner()
        {
            InitializeComponent();
        }
        private void ActivityDesigner_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int type = ((System.Activities.Expressions.Literal<int>)(((this.ModelItem.Properties["Type"].Value).Content).ComputedValue)).Value;
            int item = ((System.Activities.Expressions.Literal<int>)(((this.ModelItem.Properties["Item"].Value).Content).ComputedValue)).Value;
            int position = ((System.Activities.Expressions.Literal<int>)(((this.ModelItem.Properties["Position"].Value).Content).ComputedValue)).Value;
            string command = ((System.Activities.Expressions.Literal<string>)(((this.ModelItem.Properties["Command"].Value).Content).ComputedValue)).Value;
            string expression = ((System.Activities.Expressions.Literal<string>)(((this.ModelItem.Properties["Expression"].Value).Content).ComputedValue)).Value;
            string description = ((System.Activities.Expressions.Literal<string>)(((this.ModelItem.Properties["Description"].Value).Content).ComputedValue)).Value;
            int workflowActivityID = ((ManualActivity)(this.ModelItem.Source.ComputedValue)).WorkflowActivityID;

            ManualActivityGuide manualActivity = new ManualActivityGuide();
            manualActivity.Type = type;
            manualActivity.Item = item;
            manualActivity.Position = position;
            manualActivity.Command = command;
            manualActivity.Expression = expression;
            manualActivity.Description = description;
            manualActivity.WorkflowActivityID = workflowActivityID;

            bool? result = manualActivity.ShowDialog();

            if (result == true)
            {
                System.Activities.InArgument<int> typeArg = new System.Activities.InArgument<int>();
                typeArg = Convert.ToInt32(manualActivity.cbxTypes.SelectedValue);
                this.ModelItem.Properties["Type"].SetValue(typeArg);

                System.Activities.InArgument<int> itemArg = new System.Activities.InArgument<int>();
                itemArg = Convert.ToInt32(manualActivity.cbxItems.SelectedValue);
                this.ModelItem.Properties["Item"].SetValue(itemArg);

                System.Activities.InArgument<int> positionArg = new System.Activities.InArgument<int>();
                positionArg = Convert.ToInt32(manualActivity.cbxPosition.SelectedValue);
                this.ModelItem.Properties["Position"].SetValue(positionArg);

                System.Activities.InArgument<string> commandArg = new System.Activities.InArgument<string>();
                commandArg = manualActivity.txtCmd.Text;
                this.ModelItem.Properties["Command"].SetValue(commandArg);

                System.Activities.InArgument<string> expressionArg = new System.Activities.InArgument<string>();
                expressionArg = manualActivity.Expression;
                this.ModelItem.Properties["Expression"].SetValue(expressionArg);

                System.Activities.InArgument<string> descriptionArg = new System.Activities.InArgument<string>();
                descriptionArg = manualActivity.txtDescription.Text;
                this.ModelItem.Properties["Description"].SetValue(descriptionArg);

                this.ModelItem.Properties["WorkflowActivityID"].SetValue(manualActivity.WorkflowActivityID);
            }
        }
    }
}
