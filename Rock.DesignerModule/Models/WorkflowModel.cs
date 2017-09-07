using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.DesignerModule.Models
{
    public class WorkflowModel : NotificationObject
    {
        private int _workflowID;
        private string _workflowName;     

        public int WorkflowID
        {
            get { return _workflowID; }
            set
            {
                _workflowID = value;
                this.RaisePropertyChanged("WorkflowID");
            }
        }
        public string WorkflowName
        {
            get { return _workflowName; }
            set
            {
                _workflowName = value;
                this.RaisePropertyChanged("WorkflowName");
            }
        }
    }
}
