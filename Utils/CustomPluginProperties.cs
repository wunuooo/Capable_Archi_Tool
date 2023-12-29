using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapableArchiTool.Utils
{
    internal class CustomPluginProperties
    {
        public static string Version => MAJOR_VERSION + "." + MINOR_VERSION + "." + PATCH_VERSION;
        public static int MAJOR_VERSION = 0;
        public static int MINOR_VERSION = 1;
        public static int PATCH_VERSION = 0;
    }
}
