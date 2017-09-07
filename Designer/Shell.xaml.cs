using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Logging;
using System.Windows;

namespace Rock.Designer
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
   [Export]
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();
         }
    }
}
