using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CapableArchiTool.AnalComps
{
    public class PolygonPOIComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the POIHeatMapMesh class.
        /// </summary>
        public PolygonPOIComponent()
          : base("Polygon POI", "POI",
              "Grab POI data within a polygon plot, use the Amap API.",
              "Capable Archi Tool", "02 Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Polygon", "P", "Set the corner points of the polygon, " +
                "should use the format like 'longitude1,latitude1|longitude2,latitude2|longitude3,latitude3|...'", 
                GH_ParamAccess.item);
            pManager.AddTextParameter("Keywords", "K", "The Keywords of data that are being searched, " +
                "should use the format like 'keyword1|keyword2|keyword3|...'", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Type", "T", "The types of data that are being searched, " +
                "should use the format like 'type1|type2|type3|...'", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddIntegerParameter("Offset", "O", "The amount of data that will be recorded on every page, " +
                "larger than 25 will probably cause error.", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager.AddIntegerParameter("Page", "P", "The current page.", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager.AddTextParameter("API Key", "A", "The key that applied from Amap to use API.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Run this component.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Json", "J", "The json file that return from website.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string polygon = null;
            string keywords = null;
            string type = null;
            int offset = 20;
            int page = 1;
            string key = null;
            bool run = false;

            string url = "https://restapi.amap.com/v3/place/polygon?";

            if (!DA.GetData(0, ref polygon)) return;
            else
                url += "polygon=" + polygon;
            if (DA.GetData(1, ref keywords))
                url += "&keywords=" + keywords;
            if (DA.GetData(2, ref type))
                url += "&types=" + type;
            if (DA.GetData(3, ref offset))
                url += "&offset=" + offset;
            if (DA.GetData(4, ref page))
                url += "&page=" + page;
            if (!DA.GetData(5, ref key)) return;
            else 
                url += "&output=json&key=" + key;
            if (!DA.GetData(6, ref run)) return;

            if(run)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string json = reader.ReadToEnd();

                DA.SetData(0, json);
            }
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.POI_Heat_Map;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4080B609-6818-4DC0-96A4-61132731106A"); }
        }
    }
}