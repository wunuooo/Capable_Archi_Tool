using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CapableArchiTool.ManaComps
{
    public class DeletePlaceholderComponent : GH_Component
    {
        GH_Document GrasshopperDocument;

        /// <summary>
        /// Initializes a new instance of the DeletePlacehloderComponent class.
        /// </summary>
        public DeletePlaceholderComponent()
          : base("Delete Placeholder", "Delete",
              "Delete the placeholder components in this grasshopper file.",
              "Capable Archi Tool", "01 Management")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Delete", "D", "Delete the placeholder components.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool delete = false;

            if (!DA.GetData(0, ref delete)) return;
            
            GrasshopperDocument = Instances.ActiveCanvas.Document;

            if (GrasshopperDocument == null) return;

            if(delete)
            {
                IList<IGH_DocumentObject> comps = GrasshopperDocument.Objects;
                IList<IGH_DocumentObject> delcomps = comps.Where(o => o.GetType().ToString() == "Grasshopper.Kernel.Components.GH_PlaceholderComponent").ToList();
                if (delcomps == null || delcomps.Count == 0) return;
                GrasshopperDocument.ScheduleSolution(0, d => GrasshopperDocument.RemoveObjects(delcomps, false));
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Delete_Placeholder;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3265D645-96B0-49BB-A233-534720FBB1A2"); }
        }
    }
}