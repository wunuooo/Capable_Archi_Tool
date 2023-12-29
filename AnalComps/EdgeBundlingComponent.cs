using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;

using System.Linq;


namespace CapableArchiTool.AnalComps
{
    public class EdgeBundlingComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EdgeBundlingComponent class.
        /// </summary>
        public EdgeBundlingComponent()
          : base("Edge Bundling", "Bundling",
              "A static edge bundling method based on kernel density estimation",
              "Capable Archi Tool", "02 Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "Lines to be calculated.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Iteration", "I", "Number of iterations.The calculation will be long if the " +
                "number is large than 10.", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("Smoothness", "S", "Smoothness of the output curves.", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("KernelSize", "K", "Radius of kernel in pixels.", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("Value", "V", "Value of density remap", GH_ParamAccess.item, 2);
            pManager.AddBooleanParameter("Run", "R", "Run calculation.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Information", "Info", "Information of the algorithm", GH_ParamAccess.item);
            pManager.AddCurveParameter("Polylines", "P", "Polylines that are generated.", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Mesh of density map.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<Line> lines = new List<Line>();
            int iteration = 0;
            int smoothness = 0;
            int kernelsize = 0;
            double value = 0;
            bool run = false;

            DA.SetData(0, info);

            if (!DA.GetDataList(0, lines)) return;
            if (!DA.GetData(1, ref iteration)) return;
            if (!DA.GetData(2, ref smoothness)) return;
            if (!DA.GetData(3, ref kernelsize)) return;
            if (!DA.GetData(4, ref value)) return;
            if (!DA.GetData(5, ref run)) return;

            if(!run) return;

            List<Point3d> pt1 = new List<Point3d>();
            List<Point3d> pt2 = new List<Point3d>();
            List<Point3d> allpoints = new List<Point3d>();

            //Extraction of beginning and end of the lines
            //also used for bounding box generation
            for (int i = 0; i < lines.Count; i++)
            {
                pt1.Add(lines[i].From);
                pt2.Add(lines[i].To);
                allpoints.Add(lines[i].From);
                allpoints.Add(lines[i].To);
            }
            BoundingBox BB = new BoundingBox(allpoints);
            double width = BB.Max.X - BB.Min.X;
            double height = BB.Max.Y - BB.Min.Y;

            // Sample Parameters
            double splitDistance = 0.005 * Math.Min(width, height);
            double removeDistance = splitDistance / 2.0;

            // Splatting & Accumulation Parameters
            int AccResolution_x = 300;
            int AccResolution_y = Convert.ToInt32((double)AccResolution_x * height / width);

            double[,] Kernel = GetKernel((kernelsize * 2 + 1));
            double[,] AccMap = new double[2, 2];

            // Gradient Application Parameters
            int GradientW = (kernelsize * 3 + 1);
            double attractionFactor = 1.0;

            //Lines are sampled to the minimum resolution
            //It differs from previous algorithm which split by 2 at each step
            Point3d[][] linespt = Resample_points(splitDistance, pt1, pt2);

            for (int i = 0; i < iteration; i++)
            {
                // Resampling
                Resample(splitDistance, removeDistance, ref linespt);

                // Splatting
                AccMap = ComputeSplatting(AccResolution_x, AccResolution_y, Kernel, (kernelsize * 2 + 1), linespt, BB);

                // Apply Gradient
                ApplyGradient(ref linespt, GradientW, attractionFactor, AccMap, AccResolution_x, AccResolution_y, BB);

                // Smooth trajectories
                for (int j = 0; j < smoothness; j++)
                {
                    SmoothTrajectories(ref linespt);
                }
            }

            List<Polyline> listCurves = new List<Polyline>();
            for (int r = 0; r < linespt.Length; r++)
            {
                List<Point3d> listPoint = new List<Point3d>();
                for (int s = 0; s < linespt[r].Length; s++)
                {
                    listPoint.Add(linespt[r][s]);
                }
                listCurves.Add(new Polyline(listPoint));
            }

            //List of heights for mesh generation
            List<double> listHeights = new List<double>();
            for (int j = 0; j < AccResolution_y; j++)
            {
                for (int i = 0; i < AccResolution_x; i++)
                {
                    listHeights.Add(AccMap[i, j]);
                }
            }

            Mesh mesh = DensityMap(BB, AccResolution_x - 1, AccResolution_y - 1, listHeights, value);

            DA.SetDataList(1, listCurves);
            DA.SetData(2, mesh);

            //DA.SetData(2, BB);
            //DA.SetData(3, AccResolution_x - 1);
            //DA.SetData(4, AccResolution_y - 1);
            //DA.SetDataList(5, listHeights);
            
            
        }

        //Information
        private string info =
            "This C# code instance algorithm of Kernel Density Estimation-based Edge Bundling is created by Antoine LHUILLIER, and is based on " +
            "work from Christophe Hurter, Alexandru Telea, and Ozan Ersoy, Graph Bundling by Kernel Density Estimation. " +
            "EuroVis 2012.Computer Graphics Forum journal.\r\n" +
            "Extremely appreciate for their contribution.\r\n" +
            "More details on the website:\r\n" +
            "https://www.grasshopper3d.com/forum/topics/kernel-density-estimation-based-edge-bundling\r\n" +
            "http://recherche.enac.fr/~hurter/KDEEB/KDEEB.html";

        //Method
        public double[,] GetKernel(int KernSize)
        {
            // Creates the kernel Structure
            double[,] Kernel = new double[KernSize, KernSize];
            double id;
            double jd;
            double n = ((double)KernSize - 1.0) / 2.0;
            double centerDist;

            // Fill the structure for a circular gradiant Kernel
            for (int i = 0; i < KernSize; i++)
            {
                for (int j = 0; j < KernSize; j++)
                {
                    id = (double)i;
                    jd = (double)j;
                    centerDist = Math.Sqrt((jd - n) * (jd - n) + (id - n) * (id - n));
                    //Epanechnikov Kernel (1-distance²)
                    Kernel[i, j] = Math.Max(0.0, 1.0 - (centerDist / n) * (centerDist / n));
                }
            }
            return Kernel;
        }
        public double[,] ComputeSplatting(int AccResolution_x, int AccResolution_y, double[,] Kernel, int KernelSize, Point3d[][] lines, BoundingBox BB)
        {
            double[,] AccMap = new double[AccResolution_x, AccResolution_y];
            int ix;
            int iy;
            int itabx;
            int itaby;

            for (int i_l = 0; i_l < lines.Length; i_l++)
            {
                for (int i_p = 0; i_p < lines[i_l].Length; i_p++)
                {
                    ix = Convert.ToInt32((lines[i_l][i_p].X - BB.Min.X) / (BB.Max.X - BB.Min.X) * ((double)AccResolution_x - 1.0));
                    iy = Convert.ToInt32((lines[i_l][i_p].Y - BB.Min.Y) / (BB.Max.Y - BB.Min.Y) * ((double)AccResolution_y - 1.0));
                    for (int i = 0; i < KernelSize; i++)
                    {
                        for (int j = 0; j < KernelSize; j++)
                        {
                            itabx = (ix + i - (KernelSize - 1) / 2);
                            itaby = (iy + j - (KernelSize - 1) / 2);
                            if ((itabx >= 0) && (itabx < AccResolution_x) && (itaby >= 0) && (itaby < AccResolution_y))
                            {
                                AccMap[itabx, itaby] += Kernel[i, j];
                            }
                        }
                    }
                }
            }
            return AccMap;
        }

        public Vector3d GetLocalGradient(int gradientW, double[,] AccMap, int AccResolution_x, int AccResolution_y, Point3d pt, BoundingBox BB)
        {
            int i_x;
            int i_y;

            Vector3d localGradient = new Vector3d();
            for (int i = 0; i < gradientW; i++)
            {
                for (int j = 0; j < gradientW; j++)
                {
                    i_x = Convert.ToInt32((pt.X - BB.Min.X) / (BB.Max.X - BB.Min.X) * ((double)AccResolution_x - 1.0));
                    i_y = Convert.ToInt32((pt.Y - BB.Min.Y) / (BB.Max.Y - BB.Min.Y) * ((double)AccResolution_y - 1.0));

                    if ((i_x + i - gradientW / 2) >= 0
                      && (i_x + i - gradientW / 2) < AccResolution_x
                      && (i_y + j - gradientW / 2) >= 0
                      && (i_y + j - gradientW / 2) < AccResolution_y)
                    {
                        double localDensity = AccMap[i_x + i - gradientW / 2, i_y + j - gradientW / 2];
                        double dX = ((double)i - (double)gradientW / 2.0) * (BB.Max.X - BB.Min.X) / ((double)AccResolution_x - 1.0);
                        double dY = ((double)j - (double)gradientW / 2.0) * (BB.Max.Y - BB.Min.Y) / ((double)AccResolution_y - 1.0);
                        localGradient.X += localDensity * dX;
                        localGradient.Y += localDensity * dY;
                    }
                }
            }
            return localGradient;
        }

        private void ApplyGradient(ref Point3d[][] lines, int gradientW, double attracFactor, double[,] AccMap, int AccResolution_x, int AccResolution_y, BoundingBox BB)
        {

            double dx = (BB.Max.X - BB.Min.X) / ((double)AccResolution_x - 1.0);
            double dy = (BB.Max.Y - BB.Min.Y) / ((double)AccResolution_y - 1.0);
            // Apply the Gradient for each points
            for (int i_l = 0; i_l < lines.Length; i_l++)
            {
                for (int i_p = 1; i_p < lines[i_l].Length - 1; i_p++)
                {
                    // Get the gradient value at the current point
                    Vector3d localGradient = GetLocalGradient(gradientW, AccMap, AccResolution_x, AccResolution_y, lines[i_l][i_p], BB);

                    // Normalizing the localGradient.
                    if (localGradient.X != 0 || localGradient.Y != 0)
                    {
                        localGradient.Unitize();
                    }
                    // Move each point by the gradient vector associated within
                    lines[i_l][i_p].X = lines[i_l][i_p].X + attracFactor * localGradient.X * dx;
                    lines[i_l][i_p].Y = lines[i_l][i_p].Y + attracFactor * localGradient.Y * dy;
                }
            }
        }
        /// Sampling extrapolate a new point from two point if thoose are too far from each other.
        /// But also can suppress a point if he is to close to another.
        /// Sampling is done only one vertices from the same trajectory.
        public Point3d[][] Resample_points(double splitDist, List<Point3d> pt1, List<Point3d> pt2)
        {
            Point3d[][] tempPoint3d = new Point3d[pt1.Count][];

            for (int i = 0; i < pt1.Count; i++)
            {
                List<Point3d> tmpPoint3dList = new List<Point3d>();
                //tmpPoint3dList.Add(pt1[i]);
                double distance = pt1[i].DistanceTo(pt2[i]);
                int n_points = Math.Max(3, Convert.ToInt32(distance / splitDist));
                // Sampling on each lines (= trajectory)
                for (int j = 0; j <= n_points; j++)
                {
                    tmpPoint3dList.Add(pt1[i] + (pt2[i] - pt1[i]) * j / n_points);
                }
                tempPoint3d[i] = tmpPoint3dList.ToArray();
            }
            return tempPoint3d;
        }
        /// Sampling extrapolate a new point from two point if thoose are too far from each other.
        /// But also can suppress a point if he is to close to another.
        /// Sampling is done only one vertices from the same trajectory.
        public void Resample(double splitDist, double removeDist, ref Point3d[][] tempPoint3d)
        {
            for (int i = 0; i < tempPoint3d.Length; i++)
            {
                List<Point3d> tmpPoint3dList = new List<Point3d>
                {
                    tempPoint3d[i][0]
                };
                // Sampling on each lines (= trajectory)
                for (int j = 0; j < tempPoint3d[i].Length - 1; j++)
                {
                    Point3d currentPoint = tempPoint3d[i][j];
                    Point3d nextPoint = tempPoint3d[i][j + 1];
                    double dist = currentPoint.DistanceTo(nextPoint);
                    // Test if the next point is too far or too close
                    if (dist > splitDist)
                    {
                        tmpPoint3dList.Add(new Point3d((currentPoint + nextPoint) / 2));
                    }
                    if (!(dist < removeDist) || j == tempPoint3d[i].Length - 2)
                    {
                        tmpPoint3dList.Add(nextPoint);
                    }
                }
                tempPoint3d[i] = tmpPoint3dList.ToArray();
            }
        }
        //Laplacian smoothing
        public void SmoothTrajectories(ref Point3d[][] CurrentTraj)
        {
            for (int i = 0; i < CurrentTraj.Length; i++)
            {
                for (int j = 1; j < CurrentTraj[i].Length - 1; j++)
                {
                    CurrentTraj[i][j] = (CurrentTraj[i][j - 1] + CurrentTraj[i][j] + CurrentTraj[i][j + 1]) / 3;
                }
            }
        }

        //Create density map mesh
        public Mesh DensityMap(BoundingBox b, int x, int y, List<Double> h, double v)
        {
            //Get the mesh region.
            Interval width = new Interval(0, b.Max.X - b.Min.X);
            Interval heigth = new Interval(0, b.Max.Y - b.Min.Y);
            //Get the origin of map and create a mesh based on it.
            Point3d origin = new Point3d(b.Min.X, b.Min.Y, 0);
            Plane meshplane = new Plane(origin, Plane.WorldXY.Normal);
            Mesh planemesh = Mesh.CreateFromPlane(meshplane, width, heigth, x, y);
            //Calculate the maximum of density.
            double hmax = h.Max();
            //Replace the mesh points
            for (int i = 0; i < planemesh.Vertices.Count; i++)
            {
                //Remap the density.
                float vdensity = (float) (v * h[i] / hmax);
                //Replace points.
                Point3f pt = new Point3f(planemesh.Vertices[i].X, planemesh.Vertices[i].Y, vdensity);
                planemesh.Vertices[i] = pt;
            }
            return planemesh;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Edge_Bundling;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9A6C1904-CC01-453C-8F9A-A240EB419B0D"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
    }
}