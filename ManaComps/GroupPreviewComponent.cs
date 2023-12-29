using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;

using Grasshopper.Kernel.Special;

namespace CapableArchiTool.ManaComps
{
    public class GroupPreviewComponent : GH_Component
    {
        GH_Component GrasshopperComponent;
        GH_Document GrasshopperDocument;

        /// <summary>
        /// Initializes a new instance of the MassPreviewComponent class.
        /// </summary>
        public GroupPreviewComponent()
          : base("Group Preview", "Preview",
              "Control whether to preview all the components in the group that the MassPreviewComponent is in, " +
                "except the MassPreviewComponent itself.",
              "Capable Archi Tool", "01 Management")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Preview", "P", "Preview all the components in this group except MassPreviewComponent.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "R", "Run this component.", GH_ParamAccess.item);
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
            bool preview = false;
            bool run = false;

            if (!DA.GetData(0, ref preview)) return;
            if (!DA.GetData(1, ref run)) return;
            if (!run) return;

            GrasshopperComponent = this;
            GrasshopperDocument = Instances.ActiveCanvas.Document;

            Guid groupGuid = GroupGuid(GroupsGuid(), out bool succeed);
            if (!succeed) return;
            PreviewGroupComponents(groupGuid, preview);
        }

        //Return all the group guids.
        public List<Guid> GroupsGuid()
        {
            List<Guid> groupguids = new List<Guid>();
            foreach (var obj in GrasshopperDocument.Objects)
            {
                if (obj.GetType().ToString() == "Grasshopper.Kernel.Special.GH_Group")
                {
                    groupguids.Add(obj.InstanceGuid);
                }
            }
            return groupguids;
        }

        //Find the group where this component is in and return its guid.
        public Guid GroupGuid(List<Guid> guids, out bool succeed)
        {
            foreach (var guid in guids)
            {
                IGH_DocumentObject obj = GrasshopperDocument.FindObject(guid, false);
                GH_Group group = obj as GH_Group;
                List<IGH_DocumentObject> comps = group.ObjectsRecursive();
                foreach (IGH_DocumentObject comp in comps)
                {
                    if (comp.InstanceGuid == GrasshopperComponent.InstanceGuid)
                    {
                        succeed = true;
                        return guid;
                    }
                }
            }
            succeed = false;
            return Guid.Empty;
        }

        //Change the preview property.
        public void PreviewGroupComponents(Guid guid, bool preview)
        {
            IGH_DocumentObject obj = GrasshopperDocument.FindObject(guid, false);
            GH_Group group = obj as GH_Group;
            foreach (IGH_DocumentObject groupobj in group.ObjectsRecursive())
            {
                if(groupobj is IGH_PreviewObject)
                {
                    if(groupobj.InstanceGuid != GrasshopperComponent.InstanceGuid)
                    {
                        IGH_PreviewObject precomp = groupobj as IGH_PreviewObject;
                        precomp.Hidden = !preview;
                    }
                }     
            }
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Group_Preview;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2CB4543B-C142-4CF5-9A85-DED6CBFF3A81"); }
        }
    }
}