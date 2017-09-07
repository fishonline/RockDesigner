using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Common
{
    [ModuleExport(typeof(CommonModule))]
    public class CommonModule : IModule
    {
        public CommonModule()
        {

        }

        public void Initialize()
        {

        }
    }
}
