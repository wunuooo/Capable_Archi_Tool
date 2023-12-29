using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CapableArchiTool.InfoComps
{
    public class PluginVersionComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ShowVersionComponent class.
        /// </summary>
        public PluginVersionComponent()
          : base("Plugin Version", "Version",
              "Show the plugin version",
              "Capable Archi Tool", "00 Information")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Version", "V", "Plugin version (semantic).", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("Major", "Ma", "", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("Minor", "Mi", "", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("Patch", "P", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, CapableArchiTool.Utils.CustomPluginProperties.Version);
            //DA.SetData(1, Utils.CustomPluginProperties.MAJOR_VERSION);
            //DA.SetData(2, Utils.CustomPluginProperties.MINOR_VERSION);
            //DA.SetData(3, Utils.CustomPluginProperties.PATCH_VERSION);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Plugin_Version;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("205227B0-867E-4375-B0CC-A5EEAD0D26D5"); }
        }
    }
}