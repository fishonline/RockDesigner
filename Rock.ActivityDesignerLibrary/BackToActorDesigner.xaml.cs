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
    // BackToActorDesigner.xaml 的交互逻辑
    public partial class BackToActorDesigner
    {
        public BackToActorDesigner()
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
            int wfActivityID = ((BackToActor)(this.ModelItem.Source.ComputedValue)).WorkflowActivityID;

            BackToActorGuide backToActor = new BackToActorGuide();
            backToActor.Type = type;
            backToActor.Item = item;
            backToActor.Position = position;
            backToActor.Command = command;
            backToActor.Expression = expression;
            backToActor.Description = description;
            backToActor.WorkflowActivityID = wfActivityID;

            bool? result = backToActor.ShowDialog();

            if (result == true)
            {
                System.Activities.InArgument<int> typeArg = new System.Activities.InArgument<int>();
                typeArg = Convert.ToInt32(backToActor.cbxTypes.SelectedValue);
                this.ModelItem.Properties["Type"].SetValue(typeArg);

                System.Activities.InArgument<int> itemArg = new System.Activities.InArgument<int>();
                itemArg = Convert.ToInt32(backToActor.cbxItems.SelectedValue);
                this.ModelItem.Properties["Item"].SetValue(itemArg);

                System.Activities.InArgument<int> positionArg = new System.Activities.InArgument<int>();
                positionArg = Convert.ToInt32(backToActor.cbxPosition.SelectedValue);
                this.ModelItem.Properties["Position"].SetValue(positionArg);

                System.Activities.InArgument<string> commandArg = new System.Activities.InArgument<string>();
                commandArg = backToActor.txtCmd.Text;
                this.ModelItem.Properties["Command"].SetValue(commandArg);

                System.Activities.InArgument<string> expressionArg = new System.Activities.InArgument<string>();
                expressionArg = backToActor.Expression;
                this.ModelItem.Properties["Expression"].SetValue(expressionArg);

                System.Activities.InArgument<string> descriptionArg = new System.Activities.InArgument<string>();
                descriptionArg = backToActor.txtDescription.Text;
                this.ModelItem.Properties["Description"].SetValue(descriptionArg);

                this.ModelItem.Properties["WorkflowActivityID"].SetValue(backToActor.WorkflowActivityID);
            }
        }
    }
}
