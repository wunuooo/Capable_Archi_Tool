using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.GUI.Canvas;
using Rhino.Geometry;
using System.Windows.Forms;
using GH_IO.Serialization;

namespace CapableArchiTool.ManaComps
{
    public class CustomGUIComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CustomCanvasComponent class.
        /// </summary>
        public CustomGUIComponent()
          : base("Custom GUI", "GUI",
              "Change the GHI of canvas",
              "Capable Archi Tool", "01 Management")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Custom", "C", "Set true to custom canvas.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Run", "R", "Set true to make the change come into effect.", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("QuickPattern", "QuickPattern", "Set the canvas appearance to the pattern" + Environment.NewLine + "0 - Default, 1 - DayMode, 2 - NightMode", GH_ParamAccess.item, 0);
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
            bool isCustomed = false;
            bool isRun = false;
            int quickPattern = 0;

            if (!DA.GetData(0, ref isCustomed)) return;
            if (!DA.GetData(1, ref isRun)) return;

            if (isCustomed && isRun)
            {
                if (part == "Quick Pattern")
                {
                    if (!DA.GetData(2, ref quickPattern)) return;
                    if (quickPattern == 0) UseDefaultPattern();
                    if (quickPattern == 1) UseDayModePattern();
                    if (quickPattern == 2) UseNightModePattern();
                }
                else if (part == "Normal Component")
                {
                    Color unselectedFillColor = default(Color);
                    Color unselectedEdgeColor = default(Color);
                    Color unselectedTextColor = default(Color);
                    Color selectedFillColor = default(Color);
                    Color selectedEdgeColor = default(Color);
                    Color selectedTextColor = default(Color);
                    DA.GetData(2, ref unselectedFillColor);
                    DA.GetData(3, ref unselectedEdgeColor);
                    DA.GetData(4, ref unselectedTextColor);
                    DA.GetData(5, ref selectedFillColor);
                    DA.GetData(6, ref selectedEdgeColor);
                    DA.GetData(7, ref selectedTextColor);
                    GH_Skin.palette_normal_standard = new GH_PaletteStyle(unselectedFillColor, unselectedEdgeColor, unselectedTextColor);
                    GH_Skin.palette_normal_selected = new GH_PaletteStyle(selectedFillColor, selectedEdgeColor, selectedTextColor);
                }
                else if (part == "Hidden Component")
                {
                    Color unselectedFillColor = default(Color);
                    Color unselectedEdgeColor = default(Color);
                    Color unselectedTextColor = default(Color);
                    Color selectedFillColor = default(Color);
                    Color selectedEdgeColor = default(Color);
                    Color selectedTextColor = default(Color);
                    DA.GetData(2, ref unselectedFillColor);
                    DA.GetData(3, ref unselectedEdgeColor);
                    DA.GetData(4, ref unselectedTextColor);
                    DA.GetData(5, ref selectedFillColor);
                    DA.GetData(6, ref selectedEdgeColor);
                    DA.GetData(7, ref selectedTextColor);
                    GH_Skin.palette_hidden_standard = new GH_PaletteStyle(unselectedFillColor, unselectedEdgeColor, unselectedTextColor);
                    GH_Skin.palette_hidden_selected = new GH_PaletteStyle(selectedFillColor, selectedEdgeColor, selectedTextColor);
                }
                else if (part == "Disabled Component")
                {
                    Color unselectedFillColor = default(Color);
                    Color unselectedEdgeColor = default(Color);
                    Color unselectedTextColor = default(Color);
                    Color selectedFillColor = default(Color);
                    Color selectedEdgeColor = default(Color);
                    Color selectedTextColor = default(Color);
                    DA.GetData(2, ref unselectedFillColor);
                    DA.GetData(3, ref unselectedEdgeColor);
                    DA.GetData(4, ref unselectedTextColor);
                    DA.GetData(5, ref selectedFillColor);
                    DA.GetData(6, ref selectedEdgeColor);
                    DA.GetData(7, ref selectedTextColor);
                    GH_Skin.palette_locked_standard = new GH_PaletteStyle(unselectedFillColor, unselectedEdgeColor, unselectedTextColor);
                    GH_Skin.palette_locked_selected = new GH_PaletteStyle(selectedFillColor, selectedEdgeColor, selectedTextColor);
                }
                else if (part == "Warning Component")
                {
                    Color unselectedFillColor = default(Color);
                    Color unselectedEdgeColor = default(Color);
                    Color unselectedTextColor = default(Color);
                    Color selectedFillColor = default(Color);
                    Color selectedEdgeColor = default(Color);
                    Color selectedTextColor = default(Color);
                    DA.GetData(2, ref unselectedFillColor);
                    DA.GetData(3, ref unselectedEdgeColor);
                    DA.GetData(4, ref unselectedTextColor);
                    DA.GetData(5, ref selectedFillColor);
                    DA.GetData(6, ref selectedEdgeColor);
                    DA.GetData(7, ref selectedTextColor);
                    GH_Skin.palette_warning_standard = new GH_PaletteStyle(unselectedFillColor, unselectedEdgeColor, unselectedTextColor);
                    GH_Skin.palette_warning_selected = new GH_PaletteStyle(selectedFillColor, selectedEdgeColor, selectedTextColor);
                }
                else if (part == "Error Component")
                {
                    Color unselectedFillColor = default(Color);
                    Color unselectedEdgeColor = default(Color);
                    Color unselectedTextColor = default(Color);
                    Color selectedFillColor = default(Color);
                    Color selectedEdgeColor = default(Color);
                    Color selectedTextColor = default(Color);
                    DA.GetData(2, ref unselectedFillColor);
                    DA.GetData(3, ref unselectedEdgeColor);
                    DA.GetData(4, ref unselectedTextColor);
                    DA.GetData(5, ref selectedFillColor);
                    DA.GetData(6, ref selectedEdgeColor);
                    DA.GetData(7, ref selectedTextColor);
                    GH_Skin.palette_error_standard = new GH_PaletteStyle(unselectedFillColor, unselectedEdgeColor, unselectedTextColor);
                    GH_Skin.palette_error_selected = new GH_PaletteStyle(selectedFillColor, selectedEdgeColor, selectedTextColor);
                }
                else if (part == "Component Label")
                {
                    Color unselectedFillColor = default(Color);
                    Color unselectedEdgeColor = default(Color);
                    Color unselectedTextColor = default(Color);
                    Color selectedFillColor = default(Color);
                    Color selectedEdgeColor = default(Color);
                    Color selectedTextColor = default(Color);
                    DA.GetData(2, ref unselectedFillColor);
                    DA.GetData(3, ref unselectedEdgeColor);
                    DA.GetData(4, ref unselectedTextColor);
                    DA.GetData(5, ref selectedFillColor);
                    DA.GetData(6, ref selectedEdgeColor);
                    DA.GetData(7, ref selectedTextColor);

                    GH_Skin.palette_black_standard.Fill = unselectedFillColor;
                    GH_Skin.palette_black_standard.Edge = unselectedEdgeColor;
                    GH_Skin.palette_black_standard.Text = unselectedTextColor;
                    GH_Skin.palette_black_selected.Fill = selectedFillColor;
                    GH_Skin.palette_black_selected.Edge = selectedEdgeColor;
                    GH_Skin.palette_black_selected.Text = selectedTextColor;
                }
                else if (part == "Galapagos")
                {
                    Color unselectedFillColor = default(Color);
                    Color unselectedEdgeColor = default(Color);
                    Color selectedFillColor = default(Color);
                    Color selectedEdgeColor = default(Color);
                    DA.GetData(2, ref unselectedFillColor);
                    DA.GetData(3, ref unselectedEdgeColor);
                    DA.GetData(4, ref selectedFillColor);
                    DA.GetData(5, ref selectedEdgeColor);

                    GH_Skin.palette_pink_standard.Fill = unselectedFillColor;
                    GH_Skin.palette_pink_standard.Edge = unselectedEdgeColor;
                    GH_Skin.palette_pink_selected.Fill = selectedFillColor;
                    GH_Skin.palette_pink_selected.Edge = selectedEdgeColor;
                }
                else if (part == "Wire")
                {
                    Color normalColor = default(Color);
                    Color emptyeColor = default(Color);
                    Color selectedStartColor = default(Color);
                    Color selectedEndColor = default(Color);
                    DA.GetData(2, ref normalColor);
                    DA.GetData(3, ref emptyeColor);
                    DA.GetData(4, ref selectedStartColor);
                    DA.GetData(5, ref selectedEndColor);
                    GH_Skin.wire_default = normalColor;
                    GH_Skin.wire_empty = emptyeColor;
                    GH_Skin.wire_selected_a = selectedStartColor;
                    GH_Skin.wire_selected_b = selectedEndColor;
                }
                else if (part == "Panel")
                {
                    Color panelColor = default(Color);
                    DA.GetData(2, ref panelColor);
                    GH_Skin.panel_back = panelColor;
                }
                else if (part == "Group")
                {
                    Color groupColor = default(Color);
                    DA.GetData(2, ref groupColor);
                    GH_Skin.group_back = groupColor;
                }
                else if (part == "Canvas")
                {
                    bool monochromatic = false;
                    DA.GetData(2, ref monochromatic);

                    if (!monochromatic)
                    {
                        Color canvasBack = default(Color);
                        Color canvasGrid = default(Color);
                        int canvasGridCol = 0;
                        int canvasGridRow = 0;
                        Color canvasEdge = default(Color);
                        Color canvasShade = default(Color);
                        int canvasShadeSize = 0;
                        if (!DA.GetData(3, ref canvasBack)) return;
                        if (!DA.GetData(4, ref canvasGrid)) return;
                        if (!DA.GetData(5, ref canvasGridCol)) return;
                        if (!DA.GetData(6, ref canvasGridRow)) return;
                        if (!DA.GetData(7, ref canvasEdge)) return;
                        if (!DA.GetData(8, ref canvasShade)) return;
                        if (!DA.GetData(9, ref canvasShadeSize)) return;
                        GH_Skin.canvas_mono = false;
                        GH_Skin.canvas_back = canvasBack;
                        GH_Skin.canvas_grid = canvasGrid;
                        GH_Skin.canvas_grid_col = canvasGridCol;
                        GH_Skin.canvas_grid_row = canvasGridRow;
                        GH_Skin.canvas_edge = canvasEdge;
                        GH_Skin.canvas_shade = canvasShade;
                        GH_Skin.canvas_shade_size = canvasShadeSize;
                    }

                    else if (monochromatic)
                    {
                        Color monochromaticColor = default(Color);
                        if (!DA.GetData(10, ref monochromaticColor)) return;

                        GH_Skin.canvas_mono = true;
                        GH_Skin.canvas_mono_color = monochromaticColor;
                    }

                }
                else if (part == "Load&Save")
                {
                    bool load = false;
                    bool save = false;
                    DA.GetData(2, ref load);
                    DA.GetData(3, ref save);

                    if (load) GH_Skin.LoadSkin();
                    if (save) GH_Skin.SaveSkin();
                }
            }
        }


        private ToolStripComboBox partComboBox;

        private string part = "Quick Pattern";

        IEnumerator<IGH_Param> ParamEnum;

        List<IGH_Param> paramList = new List<IGH_Param>();


        private void InputParaRegister(string text)
        {
            if (text == "Quick Pattern")
            {
                Param_Integer DefaultPattern = new Param_Integer
                {
                    Name = "QuickPattern",
                    NickName = "QuickPattern",
                    Description = "Set the canvas appearance to the pattern, 0 - Default, 1 - DayMode, 2 - NightMode",
                    Access = GH_ParamAccess.item
                };

                DefaultPattern.SetPersistentData(0);
                Params.RegisterInputParam(DefaultPattern);
            }

            if (text == "Normal Component")
            {
                Param_Colour UnselectedFillColor = new Param_Colour
                {
                    Name = "Unselected Fill Color",
                    NickName = "Unselected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedEdgeColor = new Param_Colour
                {
                    Name = "Unselected Edge Color",
                    NickName = "Unselected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedTextColor = new Param_Colour
                {
                    Name = "Unselected Text Color",
                    NickName = "Unselected Text Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedFillColor = new Param_Colour
                {
                    Name = "Selected Fill Color",
                    NickName = "Selected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedEdgeColor = new Param_Colour
                {
                    Name = "Selected Edge Color",
                    NickName = "Selected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedTextColor = new Param_Colour
                {
                    Name = "Selected Text Color",
                    NickName = "Selected Text Color",
                    Access = GH_ParamAccess.item
                };

                UnselectedFillColor.SetPersistentData(GH_Skin.palette_normal_standard.Fill);
                UnselectedEdgeColor.SetPersistentData(GH_Skin.palette_normal_standard.Edge);
                UnselectedTextColor.SetPersistentData(GH_Skin.palette_normal_standard.Text);
                SelectedFillColor.SetPersistentData(GH_Skin.palette_normal_selected.Fill);
                SelectedEdgeColor.SetPersistentData(GH_Skin.palette_normal_selected.Edge);
                SelectedTextColor.SetPersistentData(GH_Skin.palette_normal_selected.Text);

                Params.RegisterInputParam(UnselectedFillColor);
                Params.RegisterInputParam(UnselectedEdgeColor);
                Params.RegisterInputParam(UnselectedTextColor);
                Params.RegisterInputParam(SelectedFillColor);
                Params.RegisterInputParam(SelectedEdgeColor);
                Params.RegisterInputParam(SelectedTextColor);
            }

            if (text == "Hidden Component")
            {
                Param_Colour UnselectedFillColor = new Param_Colour
                {
                    Name = "Unselected Fill Color",
                    NickName = "Unselected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedEdgeColor = new Param_Colour
                {
                    Name = "Unselected Edge Color",
                    NickName = "Unselected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedTextColor = new Param_Colour
                {
                    Name = "Unselected Text Color",
                    NickName = "Unselected Text Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedFillColor = new Param_Colour
                {
                    Name = "Selected Fill Color",
                    NickName = "Selected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedEdgeColor = new Param_Colour
                {
                    Name = "Selected Edge Color",
                    NickName = "Selected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedTextColor = new Param_Colour
                {
                    Name = "Selected Text Color",
                    NickName = "Selected Text Color",
                    Access = GH_ParamAccess.item
                };

                UnselectedFillColor.SetPersistentData(GH_Skin.palette_hidden_standard.Fill);
                UnselectedEdgeColor.SetPersistentData(GH_Skin.palette_hidden_standard.Edge);
                UnselectedTextColor.SetPersistentData(GH_Skin.palette_hidden_standard.Text);
                SelectedFillColor.SetPersistentData(GH_Skin.palette_hidden_selected.Fill);
                SelectedEdgeColor.SetPersistentData(GH_Skin.palette_hidden_selected.Edge);
                SelectedTextColor.SetPersistentData(GH_Skin.palette_hidden_selected.Text);

                Params.RegisterInputParam(UnselectedFillColor);
                Params.RegisterInputParam(UnselectedEdgeColor);
                Params.RegisterInputParam(UnselectedTextColor);
                Params.RegisterInputParam(SelectedFillColor);
                Params.RegisterInputParam(SelectedEdgeColor);
                Params.RegisterInputParam(SelectedTextColor);
            }

            if (text == "Disabled Component")
            {
                Param_Colour UnselectedFillColor = new Param_Colour
                {
                    Name = "Unselected Fill Color",
                    NickName = "Unselected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedEdgeColor = new Param_Colour
                {
                    Name = "Unselected Edge Color",
                    NickName = "Unselected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedTextColor = new Param_Colour
                {
                    Name = "Unselected Text Color",
                    NickName = "Unselected Text Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedFillColor = new Param_Colour
                {
                    Name = "Selected Fill Color",
                    NickName = "Selected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedEdgeColor = new Param_Colour
                {
                    Name = "Selected Edge Color",
                    NickName = "Selected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedTextColor = new Param_Colour
                {
                    Name = "Selected Text Color",
                    NickName = "Selected Text Color",
                    Access = GH_ParamAccess.item
                };

                UnselectedFillColor.SetPersistentData(GH_Skin.palette_locked_standard.Fill);
                UnselectedEdgeColor.SetPersistentData(GH_Skin.palette_locked_standard.Edge);
                UnselectedTextColor.SetPersistentData(GH_Skin.palette_locked_standard.Text);
                SelectedFillColor.SetPersistentData(GH_Skin.palette_locked_selected.Fill);
                SelectedEdgeColor.SetPersistentData(GH_Skin.palette_locked_selected.Edge);
                SelectedTextColor.SetPersistentData(GH_Skin.palette_locked_selected.Text);

                Params.RegisterInputParam(UnselectedFillColor);
                Params.RegisterInputParam(UnselectedEdgeColor);
                Params.RegisterInputParam(UnselectedTextColor);
                Params.RegisterInputParam(SelectedFillColor);
                Params.RegisterInputParam(SelectedEdgeColor);
                Params.RegisterInputParam(SelectedTextColor);
            }

            if (text == "Warning Component")
            {
                Param_Colour UnselectedFillColor = new Param_Colour
                {
                    Name = "Unselected Fill Color",
                    NickName = "Unselected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedEdgeColor = new Param_Colour
                {
                    Name = "Unselected Edge Color",
                    NickName = "Unselected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedTextColor = new Param_Colour
                {
                    Name = "Unselected Text Color",
                    NickName = "Unselected Text Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedFillColor = new Param_Colour
                {
                    Name = "Selected Fill Color",
                    NickName = "Selected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedEdgeColor = new Param_Colour
                {
                    Name = "Selected Edge Color",
                    NickName = "Selected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedTextColor = new Param_Colour
                {
                    Name = "Selected Text Color",
                    NickName = "Selected Text Color",
                    Access = GH_ParamAccess.item
                };

                UnselectedFillColor.SetPersistentData(GH_Skin.palette_warning_standard.Fill);
                UnselectedEdgeColor.SetPersistentData(GH_Skin.palette_warning_standard.Edge);
                UnselectedTextColor.SetPersistentData(GH_Skin.palette_warning_standard.Text);
                SelectedFillColor.SetPersistentData(GH_Skin.palette_warning_selected.Fill);
                SelectedEdgeColor.SetPersistentData(GH_Skin.palette_warning_selected.Edge);
                SelectedTextColor.SetPersistentData(GH_Skin.palette_warning_selected.Text);

                Params.RegisterInputParam(UnselectedFillColor);
                Params.RegisterInputParam(UnselectedEdgeColor);
                Params.RegisterInputParam(UnselectedTextColor);
                Params.RegisterInputParam(SelectedFillColor);
                Params.RegisterInputParam(SelectedEdgeColor);
                Params.RegisterInputParam(SelectedTextColor);
            }

            if (text == "Error Component")
            {
                Param_Colour UnselectedFillColor = new Param_Colour
                {
                    Name = "Unselected Fill Color",
                    NickName = "Unselected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedEdgeColor = new Param_Colour
                {
                    Name = "Unselected Edge Color",
                    NickName = "Unselected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedTextColor = new Param_Colour
                {
                    Name = "Unselected Text Color",
                    NickName = "Unselected Text Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedFillColor = new Param_Colour
                {
                    Name = "Selected Fill Color",
                    NickName = "Selected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedEdgeColor = new Param_Colour
                {
                    Name = "Selected Edge Color",
                    NickName = "Selected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedTextColor = new Param_Colour
                {
                    Name = "Selected Text Color",
                    NickName = "Selected Text Color",
                    Access = GH_ParamAccess.item
                };

                UnselectedFillColor.SetPersistentData(GH_Skin.palette_error_standard.Fill);
                UnselectedEdgeColor.SetPersistentData(GH_Skin.palette_error_standard.Edge);
                UnselectedTextColor.SetPersistentData(GH_Skin.palette_error_standard.Text);
                SelectedFillColor.SetPersistentData(GH_Skin.palette_error_selected.Fill);
                SelectedEdgeColor.SetPersistentData(GH_Skin.palette_error_selected.Edge);
                SelectedTextColor.SetPersistentData(GH_Skin.palette_error_selected.Text);

                Params.RegisterInputParam(UnselectedFillColor);
                Params.RegisterInputParam(UnselectedEdgeColor);
                Params.RegisterInputParam(UnselectedTextColor);
                Params.RegisterInputParam(SelectedFillColor);
                Params.RegisterInputParam(SelectedEdgeColor);
                Params.RegisterInputParam(SelectedTextColor);
            }

            if (text == "Component Label")
            {
                Param_Colour UnselectedFillColor = new Param_Colour
                {
                    Name = "Unselected Fill Color",
                    NickName = "Unselected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedEdgeColor = new Param_Colour
                {
                    Name = "Unselected Edge Color",
                    NickName = "Unselected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedTextColor = new Param_Colour
                {
                    Name = "Unselected Text Color",
                    NickName = "Unselected Text Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedFillColor = new Param_Colour
                {
                    Name = "Selected Fill Color",
                    NickName = "Selected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedEdgeColor = new Param_Colour
                {
                    Name = "Selected Edge Color",
                    NickName = "Selected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedTextColor = new Param_Colour
                {
                    Name = "Selected Text Color",
                    NickName = "Selected Text Color",
                    Access = GH_ParamAccess.item
                };

                UnselectedFillColor.SetPersistentData(GH_Skin.palette_black_standard.Fill);
                UnselectedEdgeColor.SetPersistentData(GH_Skin.palette_black_standard.Edge);
                UnselectedTextColor.SetPersistentData(GH_Skin.palette_black_standard.Text);
                SelectedFillColor.SetPersistentData(GH_Skin.palette_black_selected.Fill);
                SelectedEdgeColor.SetPersistentData(GH_Skin.palette_black_selected.Edge);
                SelectedTextColor.SetPersistentData(GH_Skin.palette_black_selected.Text);

                Params.RegisterInputParam(UnselectedFillColor);
                Params.RegisterInputParam(UnselectedEdgeColor);
                Params.RegisterInputParam(UnselectedTextColor);
                Params.RegisterInputParam(SelectedFillColor);
                Params.RegisterInputParam(SelectedEdgeColor);
                Params.RegisterInputParam(SelectedTextColor);
            }

            if (text == "Galapagos")
            {
                Param_Colour UnselectedFillColor = new Param_Colour
                {
                    Name = "Unselected Fill Color",
                    NickName = "Unselected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour UnselectedEdgeColor = new Param_Colour
                {
                    Name = "Unselected Edge Color",
                    NickName = "Unselected Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedFillColor = new Param_Colour
                {
                    Name = "Selected Fill Color",
                    NickName = "Selected Fill Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedEdgeColor = new Param_Colour
                {
                    Name = "Selected Edge Color",
                    NickName = "Selected Edge Color",
                    Access = GH_ParamAccess.item
                };

                UnselectedFillColor.SetPersistentData(GH_Skin.palette_pink_standard.Fill);
                UnselectedEdgeColor.SetPersistentData(GH_Skin.palette_pink_standard.Edge);
                SelectedFillColor.SetPersistentData(GH_Skin.palette_pink_selected.Fill);
                SelectedEdgeColor.SetPersistentData(GH_Skin.palette_pink_selected.Edge);

                Params.RegisterInputParam(UnselectedFillColor);
                Params.RegisterInputParam(UnselectedEdgeColor);
                Params.RegisterInputParam(SelectedFillColor);
                Params.RegisterInputParam(SelectedEdgeColor);
            }

            if (text == "Wire")
            {
                Param_Colour NormalColor = new Param_Colour
                {
                    Name = "Normal Color",
                    NickName = "Normal Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour EmptyeColor = new Param_Colour
                {
                    Name = "Emptye Color",
                    NickName = "Emptye Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedStartColor = new Param_Colour
                {
                    Name = "Selected Start Color",
                    NickName = "Selected Start Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour SelectedEndColor = new Param_Colour
                {
                    Name = "Selected End Color",
                    NickName = "Selected End Color",
                    Access = GH_ParamAccess.item
                };

                NormalColor.SetPersistentData(GH_Skin.wire_default);
                EmptyeColor.SetPersistentData(GH_Skin.wire_empty);
                SelectedStartColor.SetPersistentData(GH_Skin.wire_selected_a);
                SelectedEndColor.SetPersistentData(GH_Skin.wire_selected_b);

                Params.RegisterInputParam(NormalColor);
                Params.RegisterInputParam(EmptyeColor);
                Params.RegisterInputParam(SelectedStartColor);
                Params.RegisterInputParam(SelectedEndColor);
            }

            if (text == "Panel")
            {
                Param_Colour PanelColor = new Param_Colour
                {
                    Name = "Panel Color",
                    NickName = "Panel Color",
                    Access = GH_ParamAccess.item
                };

                PanelColor.SetPersistentData(GH_Skin.panel_back);

                Params.RegisterInputParam(PanelColor);
            }

            if (text == "Group")
            {
                Param_Colour GroupColor = new Param_Colour
                {
                    Name = "Group Color",
                    NickName = "Group Color",
                    Access = GH_ParamAccess.item
                };

                GroupColor.SetPersistentData(GH_Skin.group_back);

                Params.RegisterInputParam(GroupColor);
            }

            if (text == "Canvas")
            {
                Param_Boolean MonochromaticOnOff = new Param_Boolean
                {
                    Name = "Monochromatic On/Off",
                    NickName = "Monochromatic On/Off",
                    Access = GH_ParamAccess.item
                };
                Param_Colour BackgroundColor = new Param_Colour
                {
                    Name = "Background Color",
                    NickName = "Background Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour GridlineColor = new Param_Colour
                {
                    Name = "Gridline Color",
                    NickName = "Gridline Color",
                    Access = GH_ParamAccess.item
                };
                Param_Integer GridlineWidth = new Param_Integer
                {
                    Name = "Gridline Width",
                    NickName = "Gridline Width",
                    Access = GH_ParamAccess.item
                };
                Param_Integer GridlineHeight = new Param_Integer
                {
                    Name = "Gridline Height",
                    NickName = "Gridline Height",
                    Access = GH_ParamAccess.item
                };
                Param_Colour EdgeColor = new Param_Colour
                {
                    Name = "Edge Color",
                    NickName = "Edge Color",
                    Access = GH_ParamAccess.item
                };
                Param_Colour ShadowColor = new Param_Colour
                {
                    Name = "Shadow Color",
                    NickName = "Shadow Color",
                    Access = GH_ParamAccess.item
                };
                Param_Integer ShadowSize = new Param_Integer
                {
                    Name = "Shadow Size",
                    NickName = "Shadow Size",
                    Access = GH_ParamAccess.item
                };
                Param_Colour MonochromaticColor = new Param_Colour
                {
                    Name = "Monochromatic Color",
                    NickName = "Monochromatic Color",
                    Access = GH_ParamAccess.item
                };

                MonochromaticOnOff.SetPersistentData(false);
                BackgroundColor.SetPersistentData(GH_Skin.canvas_back);
                GridlineColor.SetPersistentData(GH_Skin.canvas_grid);
                GridlineWidth.SetPersistentData(GH_Skin.canvas_grid_col);
                GridlineHeight.SetPersistentData(GH_Skin.canvas_grid_row);
                EdgeColor.SetPersistentData(GH_Skin.canvas_edge);
                ShadowColor.SetPersistentData(GH_Skin.canvas_shade);
                ShadowSize.SetPersistentData(GH_Skin.canvas_shade_size);
                MonochromaticColor.SetPersistentData(GH_Skin.canvas_mono_color);

                Params.RegisterInputParam(MonochromaticOnOff);
                Params.RegisterInputParam(BackgroundColor);
                Params.RegisterInputParam(GridlineColor);
                Params.RegisterInputParam(GridlineWidth);
                Params.RegisterInputParam(GridlineHeight);
                Params.RegisterInputParam(EdgeColor);
                Params.RegisterInputParam(ShadowColor);
                Params.RegisterInputParam(ShadowSize);
                Params.RegisterInputParam(MonochromaticColor);
            }

            if (text == "Load&Save")
            {
                Param_Boolean LoadXml = new Param_Boolean
                {
                    Name = "Load gui.xml",
                    NickName = "Load gui.xml",
                    Access = GH_ParamAccess.item
                };
                Param_Boolean SaveXml = new Param_Boolean
                {
                    Name = "Save gui.xml",
                    NickName = "Save gui.xml",
                    Access = GH_ParamAccess.item
                };

                LoadXml.SetPersistentData(false);
                SaveXml.SetPersistentData(false);

                Params.RegisterInputParam(LoadXml);
                Params.RegisterInputParam(SaveXml);
            }

            base.Message = text;
            ExpireSolution(true);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Settings:");
            partComboBox = new ToolStripComboBox();
            partComboBox.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            partComboBox.ComboBox.BindingContext = new BindingContext();
            partComboBox.ComboBox.Width = 200;
            partComboBox.ComboBox.DataSource = GUIfeature;
            partComboBox.ComboBox.SelectedIndexChanged += Feature_SelectedIndexChanged;
            partComboBox.ComboBox.SelectedIndex = GUIfeature.IndexOf(part);
            menu.Items.Add(partComboBox);
        }

        private void Feature_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = partComboBox.SelectedItem as string;
            if (text != part && text != null)
            {
                part = partComboBox.SelectedItem as string;
                ParamEnum = Params.GetEnumerator();

                while (ParamEnum.MoveNext())
                {
                    IGH_Param param = ParamEnum.Current;
                    if (param.Name != "Custom" && param.Name != "Run")
                    {
                        paramList.Add(param);
                    }
                }
                ParamEnum.Reset();

                foreach (IGH_Param param in paramList)
                {
                    Params.UnregisterInputParameter(param);
                }
                paramList.Clear();

                Instances.RedrawCanvas();
                Params.OnParametersChanged();
                InputParaRegister(text);
                ExpireSolution(true);
            }
        }

        private List<string> GUIfeature = new List<string> { "Quick Pattern", "Normal Component", "Hidden Component", "Disabled Component",
            "Warning Component", "Error Component", "Component Label", "Galapagos", "Wire", "Panel", "Group", "Canvas", "Load&Save" };

        private void UseDefaultPattern()
        {
            GH_Skin.palette_normal_standard = new GH_PaletteStyle(Color.FromArgb(255, 200, 200, 200), Color.FromArgb(255, 50, 50, 50), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_normal_selected = new GH_PaletteStyle(Color.FromArgb(255, 130, 215, 50), Color.FromArgb(255, 0, 50, 0), Color.FromArgb(255, 0, 25, 0));

            GH_Skin.palette_hidden_standard = new GH_PaletteStyle(Color.FromArgb(255, 140, 140, 155), Color.FromArgb(255, 35, 35, 35), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_hidden_selected = new GH_PaletteStyle(Color.FromArgb(255, 80, 180, 10), Color.FromArgb(255, 0, 35, 0), Color.FromArgb(255, 0, 25, 0));

            GH_Skin.palette_locked_standard = new GH_PaletteStyle(Color.FromArgb(255, 120, 120, 120), Color.FromArgb(255, 90, 90, 90), Color.FromArgb(255, 70, 70, 70));
            GH_Skin.palette_locked_selected = new GH_PaletteStyle(Color.FromArgb(255, 110, 150, 115), Color.FromArgb(255, 40, 70, 40), Color.FromArgb(255, 75, 100, 65));

            GH_Skin.palette_warning_standard = new GH_PaletteStyle(Color.FromArgb(255, 255, 140, 20), Color.FromArgb(255, 80, 10, 0), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_warning_selected = new GH_PaletteStyle(Color.FromArgb(255, 185, 205, 0), Color.FromArgb(255, 0, 50, 0), Color.FromArgb(255, 0, 0, 0));

            GH_Skin.palette_error_standard = new GH_PaletteStyle(Color.FromArgb(255, 200, 0, 0), Color.FromArgb(255, 60, 0, 0), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_error_selected = new GH_PaletteStyle(Color.FromArgb(255, 125, 155, 0), Color.FromArgb(255, 0, 50, 0), Color.FromArgb(255, 255, 255, 255));

            GH_Skin.palette_black_standard.Fill = Color.FromArgb(255, 50, 50, 50);
            GH_Skin.palette_black_standard.Edge = Color.FromArgb(255, 0, 0, 0);
            GH_Skin.palette_black_standard.Text = Color.FromArgb(255, 255, 255, 255);
            GH_Skin.palette_black_selected.Fill = Color.FromArgb(255, 25, 60, 25);
            GH_Skin.palette_black_selected.Edge = Color.FromArgb(255, 0, 35, 0);
            GH_Skin.palette_black_selected.Text = Color.FromArgb(255, 190, 250, 180);

            GH_Skin.palette_pink_standard.Fill = Color.FromArgb(255, 255, 0, 125);
            GH_Skin.palette_pink_standard.Edge = Color.FromArgb(255, 100, 0, 50);
            GH_Skin.palette_pink_selected.Fill = Color.FromArgb(255, 160, 210, 30);
            GH_Skin.palette_pink_selected.Edge = Color.FromArgb(255, 0, 50, 0);

            GH_Skin.wire_default = Color.FromArgb(150, 0, 0, 0);
            GH_Skin.wire_empty = Color.FromArgb(180, 255, 60, 0);
            GH_Skin.wire_selected_a = Color.FromArgb(255, 125, 210, 40);
            GH_Skin.wire_selected_b = Color.FromArgb(50, 0, 0, 0);

            GH_Skin.panel_back = Color.FromArgb(255, 255, 250, 90);
            GH_Skin.group_back = Color.FromArgb(150, 170, 135, 225);

            GH_Skin.canvas_back = Color.FromArgb(255, 212, 208, 200);
            GH_Skin.canvas_grid = Color.FromArgb(30, 0, 0, 0);
            GH_Skin.canvas_grid_col = 150;
            GH_Skin.canvas_grid_row = 50;
            GH_Skin.canvas_edge = Color.FromArgb(255, 0, 0, 0);
            GH_Skin.canvas_shade = Color.FromArgb(80, 0, 0, 0);
            GH_Skin.canvas_shade_size = 30;
            GH_Skin.canvas_mono = false;
            GH_Skin.canvas_mono_color = Color.FromArgb(255, 212, 208, 200);
        }

        private void UseDayModePattern()
        {
            GH_Skin.palette_normal_standard = new GH_PaletteStyle(Color.FromArgb(255, 200, 200, 200), Color.FromArgb(255, 50, 50, 50), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_normal_selected = new GH_PaletteStyle(Color.FromArgb(255, 130, 215, 50), Color.FromArgb(255, 0, 50, 0), Color.FromArgb(255, 0, 25, 0));

            GH_Skin.palette_hidden_standard = new GH_PaletteStyle(Color.FromArgb(255, 140, 140, 155), Color.FromArgb(255, 35, 35, 35), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_hidden_selected = new GH_PaletteStyle(Color.FromArgb(255, 80, 180, 10), Color.FromArgb(255, 0, 35, 0), Color.FromArgb(255, 0, 25, 0));

            GH_Skin.palette_locked_standard = new GH_PaletteStyle(Color.FromArgb(255, 120, 120, 120), Color.FromArgb(255, 90, 90, 90), Color.FromArgb(255, 70, 70, 70));
            GH_Skin.palette_locked_selected = new GH_PaletteStyle(Color.FromArgb(255, 110, 150, 115), Color.FromArgb(255, 40, 70, 40), Color.FromArgb(255, 75, 100, 65));

            GH_Skin.palette_warning_standard = new GH_PaletteStyle(Color.FromArgb(255, 255, 140, 20), Color.FromArgb(255, 80, 10, 0), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_warning_selected = new GH_PaletteStyle(Color.FromArgb(255, 185, 205, 0), Color.FromArgb(255, 0, 50, 0), Color.FromArgb(255, 0, 0, 0));

            GH_Skin.palette_error_standard = new GH_PaletteStyle(Color.FromArgb(255, 200, 0, 0), Color.FromArgb(255, 60, 0, 0), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_error_selected = new GH_PaletteStyle(Color.FromArgb(255, 125, 155, 0), Color.FromArgb(255, 0, 50, 0), Color.FromArgb(255, 255, 255, 255));

            GH_Skin.palette_black_standard.Fill = Color.FromArgb(255, 50, 50, 50);
            GH_Skin.palette_black_standard.Edge = Color.FromArgb(255, 0, 0, 0);
            GH_Skin.palette_black_standard.Text = Color.FromArgb(255, 255, 255, 255);
            GH_Skin.palette_black_selected.Fill = Color.FromArgb(255, 25, 60, 25);
            GH_Skin.palette_black_selected.Edge = Color.FromArgb(255, 0, 35, 0);
            GH_Skin.palette_black_selected.Text = Color.FromArgb(255, 190, 250, 180);

            GH_Skin.palette_pink_standard.Fill = Color.FromArgb(255, 255, 0, 125);
            GH_Skin.palette_pink_standard.Edge = Color.FromArgb(255, 100, 0, 50);
            GH_Skin.palette_pink_selected.Fill = Color.FromArgb(255, 160, 210, 30);
            GH_Skin.palette_pink_selected.Edge = Color.FromArgb(255, 0, 50, 0);

            GH_Skin.wire_default = Color.FromArgb(150, 0, 0, 0);
            GH_Skin.wire_empty = Color.FromArgb(180, 255, 60, 0);
            GH_Skin.wire_selected_a = Color.FromArgb(255, 125, 210, 40);
            GH_Skin.wire_selected_b = Color.FromArgb(50, 0, 0, 0);

            GH_Skin.panel_back = Color.FromArgb(255, 255, 255, 255);
            GH_Skin.group_back = Color.FromArgb(150, 255, 255, 255);

            GH_Skin.canvas_back = Color.FromArgb(255, 255, 255, 255);
            GH_Skin.canvas_grid = Color.FromArgb(30, 0, 0, 0);
            GH_Skin.canvas_grid_col = 150;
            GH_Skin.canvas_grid_row = 50;
            GH_Skin.canvas_edge = Color.FromArgb(255, 0, 0, 0);
            GH_Skin.canvas_shade = Color.FromArgb(80, 0, 0, 0);
            GH_Skin.canvas_shade_size = 30;
        }

        private void UseNightModePattern()
        {
            GH_Skin.palette_normal_standard = new GH_PaletteStyle(Color.FromArgb(255, 120, 120, 120), Color.FromArgb(255, 50, 50, 50), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_normal_selected = new GH_PaletteStyle(Color.FromArgb(255, 130, 215, 50), Color.FromArgb(255, 0, 50, 0), Color.FromArgb(255, 0, 25, 0));
            GH_Skin.palette_hidden_standard = new GH_PaletteStyle(Color.FromArgb(255, 60, 60, 60), Color.FromArgb(255, 35, 35, 35), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_hidden_selected = new GH_PaletteStyle(Color.FromArgb(255, 80, 180, 10), Color.FromArgb(255, 0, 35, 0), Color.FromArgb(255, 0, 25, 0));
            GH_Skin.palette_locked_standard = new GH_PaletteStyle(Color.FromArgb(255, 40, 40, 40), Color.FromArgb(255, 90, 90, 90), Color.FromArgb(255, 70, 70, 70));
            GH_Skin.palette_locked_selected = new GH_PaletteStyle(Color.FromArgb(255, 110, 150, 115), Color.FromArgb(255, 40, 70, 40), Color.FromArgb(255, 75, 100, 65));
            GH_Skin.palette_warning_standard = new GH_PaletteStyle(Color.FromArgb(255, 255, 140, 20), Color.FromArgb(255, 80, 10, 0), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_warning_selected = new GH_PaletteStyle(Color.FromArgb(255, 185, 205, 0), Color.FromArgb(255, 0, 50, 0), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_error_standard = new GH_PaletteStyle(Color.FromArgb(255, 200, 0, 0), Color.FromArgb(255, 60, 0, 0), Color.FromArgb(255, 0, 0, 0));
            GH_Skin.palette_error_selected = new GH_PaletteStyle(Color.FromArgb(255, 125, 155, 0), Color.FromArgb(255, 0, 50, 0), Color.FromArgb(255, 255, 255, 255));

            GH_Skin.palette_black_standard.Fill = Color.FromArgb(255, 50, 50, 50);
            GH_Skin.palette_black_standard.Edge = Color.FromArgb(255, 0, 0, 0);
            GH_Skin.palette_black_standard.Text = Color.FromArgb(255, 255, 255, 255);
            GH_Skin.palette_black_selected.Fill = Color.FromArgb(255, 25, 60, 25);
            GH_Skin.palette_black_selected.Edge = Color.FromArgb(255, 0, 35, 0);
            GH_Skin.palette_black_selected.Text = Color.FromArgb(255, 190, 250, 180);
            GH_Skin.palette_pink_standard.Fill = Color.FromArgb(255, 255, 0, 125);
            GH_Skin.palette_pink_standard.Edge = Color.FromArgb(255, 100, 0, 50);
            GH_Skin.palette_pink_selected.Fill = Color.FromArgb(255, 160, 210, 30);
            GH_Skin.palette_pink_selected.Edge = Color.FromArgb(255, 0, 50, 0);

            GH_Skin.wire_default = Color.FromArgb(150, 100, 240, 220);
            GH_Skin.wire_empty = Color.FromArgb(180, 255, 60, 0);
            GH_Skin.wire_selected_a = Color.FromArgb(255, 125, 210, 40);
            GH_Skin.wire_selected_b = Color.FromArgb(50, 0, 0, 0);

            GH_Skin.panel_back = Color.FromArgb(255, 150, 150, 150);
            GH_Skin.group_back = Color.FromArgb(150, 30, 30, 30);

            GH_Skin.canvas_back = Color.FromArgb(255, 50, 50, 50);
            GH_Skin.canvas_grid = Color.FromArgb(150, 200, 200, 200);
            GH_Skin.canvas_grid_col = 150;
            GH_Skin.canvas_grid_row = 50;
            GH_Skin.canvas_edge = Color.FromArgb(255, 0, 0, 0);
            GH_Skin.canvas_shade = Color.FromArgb(80, 150, 150, 150);
            GH_Skin.canvas_shade_size = 30;
        }


        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("part", part);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("part"))
            {
                part = reader.GetString("part");
                ParamEnum = Params.GetEnumerator();

                while (ParamEnum.MoveNext())
                {
                    IGH_Param param = ParamEnum.Current;
                    if (param.Name != "Custom" && param.Name != "Run")
                    {
                        paramList.Add(param);
                    }
                }
                ParamEnum.Reset();

                foreach (IGH_Param param in paramList)
                {
                    Params.UnregisterInputParameter(param);
                }
                paramList.Clear();

                Instances.RedrawCanvas();
                Params.OnParametersChanged();
                InputParaRegister(part);
                ExpireSolution(true);
            }
            return base.Read(reader);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            base.Message = part;
            ExpireSolution(true);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Custom_GUI;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9B3E40D9-0C03-4E57-9D3A-B397C53D9934"); }
        }
    }
}