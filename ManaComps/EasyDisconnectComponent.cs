using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas.Interaction;

namespace CapableArchiTool.ManaComps
{   
    public class EasyDisconnectComponent : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the EasyWirecutComponent class.
        /// </summary>
        public EasyDisconnectComponent()
          : base("Easy Disconnect", "Disconnect",
              "Cut the wire with one click.",
              "Capable Archi Tool", "01 Management")
        {
        }

        // Double click to switch the component.
        public class EasyDisconnectAttributes : GH_ComponentAttributes
        {
            public EasyDisconnectAttributes(EasyDisconnectComponent owner)
                : base(owner)
            {
            }
            public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (base.Owner is EasyDisconnectComponent easyWirecutComponent)
                {
                    easyWirecutComponent.CompActive();
                    return GH_ObjectResponse.Handled;
                }
                return base.RespondToMouseDoubleClick(sender, e);
            }
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
            pManager.AddTextParameter("Message", "M", "Running message", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            canvas = Instances.ActiveCanvas;

            Instances.ActiveCanvas.MouseDown -= MouseDown;
            Instances.ActiveCanvas.MouseDown += MouseDown;
            Instances.ActiveCanvas.MouseUp -= MouseUp;
            Instances.ActiveCanvas.MouseUp += MouseUp;

            DA.SetData(0, Text());
        }

        GH_Canvas canvas;

        public PointF mousePosition;

        public bool isActive = true;

        public bool isCapableDelete = false;

        public bool isLargerThanDelay = false;

        private int delaySpanll = 200;
        private int delaySpanul = 600;

        private int mouseHold = 0;

        private bool isWireInteraction = false;

        public DateTime beforeDT;
        public DateTime afterDT;

        private void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (canvas.ActiveInteraction != null &&
               (canvas.ActiveInteraction is Grasshopper.GUI.Canvas.Interaction.GH_WireInteraction ||
               canvas.ActiveInteraction is Grasshopper.GUI.Canvas.Interaction.GH_RewireInteraction))
            {
                isWireInteraction = true;
                return;
            }
            
            beforeDT = System.DateTime.Now;

            if (isActive && e.Button == MouseButtons.Left)
            {
                IGH_Param source = null;
                IGH_Param target = null;
                mousePosition = Instances.ActiveCanvas.CursorCanvasPosition;
                bool isNearWire = Instances.ActiveCanvas.Document.FindWireAt(mousePosition, 8f, ref source, ref target);

                if (isNearWire) isCapableDelete = true;
            }
        }

        private void MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (isWireInteraction)
            {
                isWireInteraction = false;
                return;
            }

            afterDT = System.DateTime.Now;

            TimeSpan ts = afterDT.Subtract(beforeDT);
            mouseHold = (int)ts.TotalMilliseconds;

            if (isActive && isCapableDelete)
            {
                IGH_Param source = null;
                IGH_Param target = null;
                mousePosition = Instances.ActiveCanvas.CursorCanvasPosition;
                bool isNearWire = Instances.ActiveCanvas.Document.FindWireAt(mousePosition, 8f, ref source, ref target);

                if (mouseHold > delaySpanll && mouseHold < delaySpanul && isNearWire)
                {
                    target.RecordUndoEvent("Removed");
                    target.RemoveSource(source);
                    target.Attributes.GetTopLevel.DocObject.ExpireSolution(true);
                    ExpireSolution(true);
                    isCapableDelete = false;
                }
            }
            ExpireSolution(true);
        }


        public void CompActive()
        {
            isActive = !isActive;
            UpdateMessage();
            ExpireSolution(true);
        }

        public override void CreateAttributes()
        {
            m_attributes = new EasyDisconnectAttributes(this);
        }

        public void UpdateMessage()
        {
            base.Message = (isActive ? "Active" : "Inactive");
        }

        public string Text()
        {
            if (isActive)
            {
                return $"The mousedown span is between {delaySpanll} and {delaySpanul} milliseconds.\n" +
                    $"The recent valid left mousedown event lasts {mouseHold} milliseconds.";
            }
            return "Component is inactive.";
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            UpdateMessage();
            ExpireSolution(true);
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            isActive = false;
            base.RemovedFromDocument(document);
            ExpireSolution(true);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Span");
            Menu_AppendTextItem(menu, "", KeyDown, null, true);
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextChanged(sender, ((Grasshopper.GUI.GH_MenuTextBox)sender).Text);
            }
            ExpireSolution(true);
        }

        private void TextChanged(object sender, string text)
        {
            string[] span = text.Split(',', '，', ' ');
            bool isParseLower = int.TryParse(span[0], out int lowerLimit);
            bool isParseUpper = int.TryParse(span[1], out int upperLimit);

            if (isParseLower && isParseUpper && lowerLimit > 0 && upperLimit > 0 && upperLimit > lowerLimit)
            {
                delaySpanll = lowerLimit;
                delaySpanul = upperLimit;
            }
            ExpireSolution(true);
        }


        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // add field.
            writer.SetInt32("Delayll", delaySpanll);
            writer.SetInt32("Delayul", delaySpanul);
            writer.SetBoolean("isActive", isActive);
            // call the base class implementation.
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // read field.
            if (reader.ItemExists("Delayll"))
            {
                delaySpanll = reader.GetInt32("Delayll");
            }
            if (reader.ItemExists("Delayul"))
            {
                delaySpanul = reader.GetInt32("Delayul");
            }
            if (reader.ItemExists("isActive"))
            {
                isActive = reader.GetBoolean("isActive");
            }
            // call the base class implementation.
            return base.Read(reader);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Easy_WireCut;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("586309E3-CAE9-425C-8B40-C88F4466ACF3"); }
        }
    }
}