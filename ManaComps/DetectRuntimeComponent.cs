using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;

using System.Drawing;
using System.Windows.Forms;

using Grasshopper.Kernel.Special;
using System.Linq.Expressions;

namespace CapableArchiTool.ManaComps
{
    public class DetectRuntimeComponent : GH_Component
    {
        GH_Document GrasshopperDocument;

        /// <summary>
        /// Initializes a new instance of the RuntimeDetectionComponent class.
        /// </summary>
        public DetectRuntimeComponent()
          : base("Detect Runtime", "Runtime",
              "Dectect the component whose runtime is over the threshold and group it.",
              "Capable Archi Tool", "01 Management")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Group", "G", "Group the component whose runtime is over threshold.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("UnGroup", "UG", "UnGroup the component whose is grouped.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Threshold", "T", "The threshold of runtime(milliseconds).", GH_ParamAccess.item);
            pManager.AddColourParameter("Color", "C", "The color of group.", GH_ParamAccess.item);
            pManager.AddTextParameter("Label", "L", "The label of group.", GH_ParamAccess.item);
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
            Boolean g = false;
            Boolean ug = false;
            int threshold = 0;
            Color color = new Color();
            String label = null;

            if (!DA.GetData(0, ref g)) return;
            if (!DA.GetData(1, ref ug)) return;
            if (!DA.GetData(2, ref threshold)) return;
            if (!DA.GetData(3, ref color)) return;
            if (!DA.GetData(4, ref label)) return;

            //It is possible for the OnPingDocument() to return null.
            //GrasshopperDocument = this.OnPingDocument();
            GrasshopperDocument = Instances.ActiveCanvas.Document;

            // Group
            if (g)
            {
                // Ungroup previously made groups
                UngroupghGroups(ghGroups);
                UngroupghGroup(ghGroup);

                // Get slow active objects
                List<Guid> guids = GetSlowActiveObjectsGuid
                  (GrasshopperDocument.ActiveObjects(), threshold);

                // Group slow active objects
                if(Together)
                {
                    ghGroup = GroupActiveObjectsTogether(guids, color, label);
                }
                else
                {
                    ghGroups = GroupActiveObjects(guids, color, label);
                }

            }

            // Ungroup
            if (ug)
            {
                // Ungroup previously made groups
                if (Together)
                {
                    UngroupghGroup(ghGroup);
                }
                else
                {
                    UngroupghGroups(ghGroups);
                }

            }

        }

        private bool istogether = false;
        public bool Together
        {
            get { return istogether; }
            set
            {
                istogether = value;
                if (istogether)
                {
                    Message = "Multiple";
                }
                else
                {
                    Message = "Single";
                }
            }
        }


        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            // Append the item to the menu, making sure it's always enabled and checked if Absolute is True.
            ToolStripMenuItem item = Menu_AppendItem(menu, "Multiple", Menu_TogetherClicked, true, Together);
            // Specifically assign a tooltip text to the menu item.
            item.ToolTipText = "The long-runtime components will be grouped together when checked.";
        }

        private void Menu_TogetherClicked(object sender, EventArgs e)
        {
            RecordUndoEvent("Multiple");
            Together = !Together;
            ExpireSolution(true);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            base.Message = (istogether) ? "Multiple" : "Single" ;
            ExpireSolution(true);
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // First add our own field.
            writer.SetBoolean("Multiple", Together);
            // Then call the base class implementation.
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("Multiple"))
            {
                // First read our own field.
                Together = reader.GetBoolean("Multiple");
            }
            // Then call the base class implementation.
            return base.Read(reader);
        }

        // Persistent variables
        GH_Group ghGroup = new GH_Group();
        List<GH_Group> ghGroups = new List<GH_Group>();

        // Get all objects that run slower than a given threshold in milliseconds
        List<Guid> GetSlowActiveObjectsGuid(List<IGH_ActiveObject> activeObjects, int threshold)
        {
            List<Guid> guids = new List<Guid>();

            foreach (IGH_ActiveObject activeObject in activeObjects)
            {
                if (activeObject.ProcessorTime.TotalMilliseconds > threshold)
                {
                    guids.Add(activeObject.InstanceGuid);
                }
            }

            return guids;
        }


        // Individually group each active object with a given colour and label
        List<GH_Group> GroupActiveObjects(List<Guid> guids, Color colour, string text)
        {
            List<GH_Group> ghGroups = new List<GH_Group>();

            foreach (Guid guid in guids)
            {
                GH_Group ghGroup = new GH_Group
                {
                    Colour = colour,
                    NickName = text
                };
                GrasshopperDocument.AddObject(ghGroup, false, GrasshopperDocument.ObjectCount);
                ghGroup.AddObject(guid);
                ghGroup.ExpireCaches();
                ghGroups.Add(ghGroup);
            }
            return ghGroups;
        }

        // Group every active object in a group with a given colour and label
        GH_Group GroupActiveObjectsTogether(List<Guid> guids, Color colour, string text)
        {
            GH_Group ghGroup = new GH_Group
            {
                Colour = colour,
                NickName = text
            };
            ghGroup.ExpireCaches();

            GrasshopperDocument.AddObject(ghGroup, false, GrasshopperDocument.ObjectCount);
            foreach (Guid guid in guids)
            {
                ghGroup.AddObject(guid);
                ghGroup.ExpireCaches();
            }
            return ghGroup;
        }

        // Ungroup all groups previously made
        void UngroupghGroups(List<GH_Group> ghGroups)
        {
            GrasshopperDocument.ScheduleSolution(0, callback =>
            {
                for (int i = 0; i < ghGroups.Count; i++)
                {
                    GrasshopperDocument.RemoveObject(ghGroups[i], false);
                    if (i >= ghGroups.Count)
                    {
                        ghGroups.Clear();
                    }
                }
            });
        }
        // Ungroup the group previously made
        void UngroupghGroup(GH_Group ghGroup)
        {
            GrasshopperDocument.ScheduleSolution(0, callback =>
            {
                GrasshopperDocument.RemoveObject(ghGroup, false);
                ghGroup = null;
            });
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Detect_Runtime;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("22B03768-5DBF-4EB5-AD92-278B84936D34"); }
        }
    }
}