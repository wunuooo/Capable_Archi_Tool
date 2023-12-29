using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper;
using Grasshopper.Kernel;

namespace CapableArchiTool.Utils
{
    public class TabProperties : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("Capable Archi Tool", Properties.Resources.Plugin_Icon);
            Instances.ComponentServer.AddCategoryShortName("Capable Archi Tool", "CAT");
            Instances.ComponentServer.AddCategorySymbolName("Capable Archi Tool", 'C');
            return GH_LoadingInstruction.Proceed;
        }

    }
}