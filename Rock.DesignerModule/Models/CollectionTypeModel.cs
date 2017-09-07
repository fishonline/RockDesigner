using Microsoft.Practices.Prism.ViewModel;
using Rock.Dyn.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.DesignerModule.Models
{
    public class CollectionTypeModel : NotificationObject
    {
        private CollectionType _collectionType;
        private string _displayName;

        public CollectionType CollectionType
        {
            get { return _collectionType; }
            set
            {
                _collectionType = value;
                RaisePropertyChanged("CollectionType");
            }
        }
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    RaisePropertyChanged("DisplayName");
                }
            }
        }
    }
}
