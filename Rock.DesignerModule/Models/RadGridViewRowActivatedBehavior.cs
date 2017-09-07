using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Rock.DesignerModule.Models
{
    public class RadGridViewRowActivatedBehavior
    {
        private ICommand _command;
        private object _commandParameter;

        public ICommand Command
        {
            get { return _command; }
            set { _command = value; }
        }

        public object CommandParameter
        {
            get { return _commandParameter; }
            set { _commandParameter = value; }
        }

        protected void ExecuteCommand()
        {
            if (this.Command != null)
            {
                this.Command.Execute(null);
            }
        }

        public RadGridViewRowActivatedBehavior(RadGridView gridView)
        {
            gridView.RowActivated += gridView_RowActivated;
        }

        void gridView_RowActivated(object sender, Telerik.Windows.Controls.GridView.RowEventArgs e)
        {
            ExecuteCommand();
        }
    }

    //AttachedBehavior由两部分组成：一个attached 属性和一个behavior对象。
    public static class RowActivated
    {
        private static readonly DependencyProperty RowActivatedCommandBehaviorProperty = DependencyProperty.RegisterAttached(
            "RowActivatedCommandBehavior",
            typeof(RadGridViewRowActivatedBehavior),
            typeof(RowActivated),
            null);

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(RowActivated),
            new PropertyMetadata(OnSetCommandCallback));

        public static void SetCommand(RadGridView gridView, ICommand command)
        {
            gridView.SetValue(CommandProperty, command);
        }

        public static ICommand GetCommand(RadGridView gridView)
        {
            return gridView.GetValue(CommandProperty) as ICommand;
        }

        //attached 属性建立目标控件和behavior对象之间的关系
        private static void OnSetCommandCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            RadGridView gridView = dependencyObject as RadGridView;
            if (gridView != null)
            {
                RadGridViewRowActivatedBehavior behavior = GetOrCreateBehavior(gridView);
                behavior.Command = e.NewValue as ICommand;
            }
        }

        private static RadGridViewRowActivatedBehavior GetOrCreateBehavior(RadGridView gridView)
        {
            RadGridViewRowActivatedBehavior behavior = gridView.GetValue(RowActivatedCommandBehaviorProperty) as RadGridViewRowActivatedBehavior;
            if (behavior == null)
            {
                behavior = new RadGridViewRowActivatedBehavior(gridView);
                gridView.SetValue(RowActivatedCommandBehaviorProperty, behavior);
            }
            return behavior;
        }
    }
}
