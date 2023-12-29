using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace CapableArchiTool
{
    public class CapableArchiToolInfo : GH_AssemblyInfo
    {
        public override string Name => "CapableArchiTool";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.Plugin_Icon;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "A tiny toolbox.";

        public override Guid Id => new Guid("67E060C9-E40F-48D8-AE70-3229B29CF709");

        //Return a string identifying you or your company.
        public override string AuthorName => "wunuooo";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "Contact me:xuwunuo@foxmail.com.";
    }
}