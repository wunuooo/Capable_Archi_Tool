using System;
using System.Collections.Generic;

using Rhino;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;

using Rhino.Geometry.Intersect;
using System.Windows.Forms;

using Grasshopper.GUI.Canvas;


namespace CapableArchiTool.AnalComps
{
    public class WaterRunoffComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WaterRunoffComponent class.
        /// </summary>
        public WaterRunoffComponent()
          : base("Water Runoff", "WaterRunoff",
              "Simulate rainwater runoff in a mountainous region",
              "Capable Archi Tool", "02 Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Terrain", "T", "Terrain that will be calculated water runoff.", GH_ParamAccess.item);
            pManager.AddRectangleParameter("Region", "R", "Rectangle region that raindrop will be generated.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Density", "D", "Waterdrop density in rectangle region.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Seed", "S", "Seed of the random points.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "L", "The length of step every time the waterdrop will take in resultant force direction.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Iteration", "I", "The iteration that the calculation proceeds on.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Run this calculation.", GH_ParamAccess.item, false);

            Grasshopper.CentralSettings.CanvasFullNamesChanged += OnCanvasFullNamesChanged;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Every waterdrop point on the water runoff path.", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Path", "P", "The water runoff path.", GH_ParamAccess.list);
            //pManager.AddMeshParameter("Area", "A", "Different water runoff area.", GH_ParamAccess.item);
        }

        private void OnCanvasFullNamesChanged() => UpdateIONames();



        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object terrian = new object();
            Rectangle3d region = new Rectangle3d();
            int density = 0;
            int seed = 0;
            double length = 0;
            int iteration = 0;
            bool run = false;

            int xCount = 0;
            int yCount = 0;

            List<Point3d> sourcePoints = new List<Point3d>();

            switch (Mode)
            {
                case 0:
                    if (!DA.GetData(0, ref terrian)) return;
                    if (!DA.GetData(1, ref region)) return;
                    if (!DA.GetData(2, ref density)) return;
                    if (!DA.GetData(3, ref seed)) return;
                    if (!DA.GetData(4, ref length)) return;
                    if (!DA.GetData(5, ref iteration)) return;
                    if (!DA.GetData(6, ref run)) return;
                    break;

                case 1:
                    if (!DA.GetData(0, ref terrian)) return;
                    if (!DA.GetData(1, ref region)) return;
                    if (!DA.GetData(2, ref xCount)) return;
                    if (!DA.GetData(3, ref yCount)) return;
                    if (!DA.GetData(4, ref length)) return;
                    if (!DA.GetData(5, ref iteration)) return;
                    if (!DA.GetData(6, ref run)) return;
                    break;

                case 2:
                    if (!DA.GetData(0, ref terrian)) return;
                    if (!DA.GetData(1, ref region)) return;
                    if (!DA.GetDataList(2, sourcePoints)) return;
                    if (!DA.GetData(3, ref length)) return;
                    if (!DA.GetData(4, ref iteration)) return;
                    if (!DA.GetData(5, ref run)) return;
                    break;
            }

            if (!run) return;

            //Detect whether the object is among surface, Brep and mesh, if true, detect whether the object is plane.
            bool isplane = IsPlane(terrian, out int type);
            if (type == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The input object is not among surface, Brep and mesh!");
                return;
            }
            if (isplane)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The input object is plane!");
                return;
            }

            //Generate points.
            List<Point3d> sourcePts = new List<Point3d>();

            switch (Mode)
            {
                case 0:
                    sourcePts = Random2DPoints(terrian, region, bb, density, seed, type);
                    break;
                case 1:
                    sourcePts = Serial2DPoints(terrian, region, bb, xCount, yCount, type);
                    break;
                case 2:
                    sourcePts = Custom3DPoints(terrian, region, bb, sourcePoints, type);
                    break;
            }


            GH_Structure<GH_Point> pts = new GH_Structure<GH_Point>();
            List<Curve> pathCrvs = new List<Curve>();
            int pathID = 0;

            //Loop every point in the pointlist and move it.
            foreach (Point3d pt in sourcePts)
            {
                List<Point3d> crvPts = new List<Point3d>();
                GH_Path path = new GH_Path(pathID);
                pts.Append(new GH_Point(pt), path);
                crvPts.Add(pt);

                pathID++;
                int i = 0;
                Point3d cpt1 = FindClosestPoint(terrian, pt, type);
                Point3d mpt, cpt2 = new Point3d();
                Vector3d moveDir = new Vector3d();

                double cpt1X = 0;
                double cpt2X = 0;

                //Begin the loop
                do
                {
                    moveDir = GeteResultantForce(terrian, cpt1, type);
                    moveDir.Unitize();
                    mpt = MovePoint(cpt1, moveDir, length);
                    cpt2 = FindClosestPoint(terrian, mpt, type);
                    GH_Point ghp = new GH_Point(cpt2);
                    pts.Append(ghp, path);
                    crvPts.Add(cpt2);

                    cpt1X = cpt1.X;
                    cpt2X = cpt2.X;

                    cpt1 = cpt2;
                    i++;
                }
                while (!DetectOutOfRegion(region, mpt) && i < iteration && cpt1X != cpt2X);

                PolylineCurve pathCrv = new PolylineCurve(crvPts);
                pathCrv.ToNurbsCurve();
                if(pathCrv.IsValid) pathCrvs.Add(pathCrv);
            }

            //Output
            DA.SetDataTree(0, pts);
            DA.SetDataList(1, pathCrvs);
        }

        BoundingBox bb = new BoundingBox();

        //Check the input terrian type.
        //public int CheckType(

        //Detect whether the object is among surface, Brep and mesh, if true, detect whether the object is plane,
        //when the object is surface, type = 1, brep type = 2, mesh type = 3, otherwise type = 0,
        //and if object is plane, return true, or return false.
        public bool IsPlane(object terrian, out int type)
        {
            if (terrian is GH_Surface)
            {
                GH_Surface ter = terrian as GH_Surface;
                bb = ter.Boundingbox;
                type = 1;
            }
            else if (terrian is GH_Brep)
            {
                GH_Brep ter = terrian as GH_Brep;
                bb = ter.Boundingbox;
                type = 2;
            }
            else if (terrian is GH_Mesh)
            {
                GH_Mesh ter = terrian as GH_Mesh;
                bb = ter.Boundingbox;
                type = 3;
            }
            else type = 0;

            if ((bb.Max.Z - bb.Min.Z) == 0) return true;
            else return false;
        }

        //Randomly generate points in certain rectangular.
        public List<Point3d> Random2DPoints(object ter, Rectangle3d rec, BoundingBox bb, int density, int seed, int type)
        {
            List<Point3d> ptlist = new List<Point3d>();
            Random rand = new Random(seed);
            for (int i = 0; i < density; i++)
            {
                double x = rec.Corner(0).X + rand.NextDouble() * rec.Width;
                double y = rec.Corner(0).Y + rand.NextDouble() * rec.Height;
                double z = bb.Center.Z + bb.Max.Z - bb.Min.Z;
                Point3d pt = new Point3d(x, y, z);
                double proZ = ProZ(ter, pt, type, bb, out bool isIntersect);
                if (isIntersect) ptlist.Add(new Point3d(x, y, proZ));
            }
            return ptlist;
        }

        //Serially generate points in certain rectangular.
        public List<Point3d> Serial2DPoints(object ter, Rectangle3d rec, BoundingBox bb, int xcount, int ycount, int type)
        {
            List<Point3d> ptlist = new List<Point3d>();
            double xstep = rec.Width / (xcount - 1);
            double ystep = rec.Height / (xcount - 1);
            double z = bb.Center.Z + bb.Max.Z - bb.Min.Z;
            for (int i = 0; i < xcount; i++)
            {
                double x = rec.Corner(0).X + i * xstep;
                for (int j = 0; j < ycount; j++)
                {
                    double y = rec.Corner(0).Y + j * ystep;
                    Point3d pt = new Point3d(x, y, z);
                    double proZ = ProZ(ter, pt, type, bb, out bool isIntersect);
                    if (isIntersect) ptlist.Add(new Point3d(x, y, proZ));
                }
            }
            return ptlist;
        }

        //Adjust points position that user input.
        public List<Point3d> Custom3DPoints(object ter, Rectangle3d rec, BoundingBox bb, List<Point3d> pts, int type)
        {
            double xmin = rec.Corner(0).X;
            double xmax = rec.Corner(2).X;
            double ymin = rec.Corner(0).Y;
            double ymax = rec.Corner(2).Y;
            List<Point3d> ptlist = new List<Point3d>();
            foreach (Point3d inputPt in pts)
            {
                if (inputPt.X >= xmin && inputPt.X <= xmax && inputPt.Y >= ymin && inputPt.Y <= ymax)
                {
                    Point3d pt = new Point3d(inputPt.X, inputPt.Y, bb.Center.Z + bb.Max.Z - bb.Min.Z);
                    double proZ = ProZ(ter, pt, type, bb, out bool isIntersect);
                    if (isIntersect) ptlist.Add(new Point3d(inputPt.X, inputPt.Y, proZ));
                }
            }
            return ptlist;
        }

        //Project point to the terrian and get the Z coordinate.
        public double ProZ(object obj, Point3d pt, int type, BoundingBox bb, out bool isIntersect)
        {
            double len = bb.Max.Z - bb.Min.Z;
            Line l = new Line(pt, -Vector3d.ZAxis, 2 * len);
            switch (type)
            {
                case 1:
                    GH_Surface srf = obj as GH_Surface;
                    Surface rhSrf = null;
                    GH_Convert.ToSurface(srf, ref rhSrf, 0);
                    Intersection.CurveBrep(l.ToNurbsCurve(), rhSrf.ToBrep(), 0.1, out _, out Point3d[] srfpt);
                    isIntersect = !(srfpt.Length == 0);
                    if (isIntersect == false)
                    {
                        return 0;
                    }
                    else return srfpt[0].Z;
                case 2:
                    GH_Brep brep = obj as GH_Brep;
                    Brep rhBrep = new Brep();
                    GH_Convert.ToBrep(brep, ref rhBrep, 0);
                    Intersection.CurveBrep(l.ToNurbsCurve(), rhBrep, 0.1, out _, out Point3d[] breppt);
                    isIntersect = !(breppt.Length == 0);
                    if (isIntersect == false)
                    {
                        return 0;
                    }
                    else return breppt[0].Z;
                case 3:
                    GH_Mesh mesh = obj as GH_Mesh;
                    Mesh rhMesh = new Mesh();
                    GH_Convert.ToMesh(mesh, ref rhMesh, 0);
                    Point3d[] intersectpt = Intersection.MeshLine(rhMesh, l);
                    if (intersectpt.Length == 0)
                    {
                        isIntersect = false;
                        return 0;
                    }
                    else
                    {
                        isIntersect = true;
                        return intersectpt[0].Z;
                    }
                default:
                    {
                        isIntersect = false;
                        return 0;
                    }
            }
        }


        //Get the resultant force direction.
        public Vector3d GeteResultantForce(object obj, Point3d pt, int type)
        {
            switch (type)
            {
                //Input object is surface.
                case 1:
                    {
                        GH_Surface srf = obj as GH_Surface;
                        Surface rhSrf = null;
                        GH_Convert.ToSurface(srf, ref rhSrf, 0);
                        rhSrf.ClosestPoint(bb.Center, out double u, out double v);
                        Vector3d centernor = rhSrf.NormalAt(u, v);
                        Brep srfBrep = rhSrf.ToBrep();
                        if (centernor.Z < 0) srfBrep.Flip();

                        //Surface normal at certain point.
                        srfBrep.ClosestPoint(pt, out Point3d cpt, out ComponentIndex ci, out double s, out double t, double.MaxValue, out Vector3d normal);
                        normal.Unitize();
                        Vector3d resultant = (-Vector3d.ZAxis) + normal * (Vector3d.Multiply(Vector3d.ZAxis, normal) / normal.Length);
                        return resultant;
                    }

                //Input object is brep.
                case 2:
                    {
                        GH_Brep brep = obj as GH_Brep;
                        Brep rhBrep = new Brep();
                        GH_Convert.ToBrep(brep, ref rhBrep, 0);
                        rhBrep.ClosestPoint(bb.Center, out Point3d ccpt, out ComponentIndex ci, out double s, out double t, double.MaxValue, out Vector3d centernor);
                        if (centernor.Z < 0) rhBrep.Flip();

                        //Brep normal at certain point.
                        rhBrep.ClosestPoint(pt, out Point3d cpt, out ComponentIndex ci2, out double s2, out double t2, double.MaxValue, out Vector3d normal);
                        normal.Unitize();
                        Vector3d resultant = (-Vector3d.ZAxis) + normal * (Vector3d.Multiply(Vector3d.ZAxis, normal) / normal.Length);
                        return resultant;
                    }

                //Input object is mesh.
                case 3:
                    {
                        GH_Mesh mesh = obj as GH_Mesh;
                        Mesh rhMesh = new Mesh();
                        GH_Convert.ToMesh(mesh, ref rhMesh, 0);
                        rhMesh.ClosestPoint(bb.Center, out Point3d centerPointOnMesh, out Vector3d centernor, double.MaxValue);
                        if (centernor.Z < 0) rhMesh.Flip(true, true, true);

                        //Mesh normal at certain point.
                        rhMesh.ClosestPoint(pt, out Point3d pointOnMesh, out Vector3d normal, double.MaxValue);
                        normal.Unitize();
                        Vector3d resultant = (-Vector3d.ZAxis) + normal * (Vector3d.Multiply(Vector3d.ZAxis, normal) / normal.Length);
                        return resultant;
                    }

                default: return Vector3d.Zero;
            }
        }

        //Find the closest point the terrian
        public Point3d FindClosestPoint(object obj, Point3d pt, int type)
        {
            switch (type)
            {
                case 1:
                    {
                        GH_Surface srf = obj as GH_Surface;
                        Surface rhSrf = null;
                        GH_Convert.ToSurface(srf, ref rhSrf, 0);
                        rhSrf.ClosestPoint(pt, out double u, out double v);
                        return rhSrf.PointAt(u, v);
                    }
                case 2:
                    {
                        GH_Brep brep = obj as GH_Brep;
                        Brep rhBrep = new Brep();
                        GH_Convert.ToBrep(brep, ref rhBrep, 0);
                        return rhBrep.ClosestPoint(pt);
                    }
                case 3:
                    {
                        GH_Mesh mesh = obj as GH_Mesh;
                        Mesh rhMesh = new Mesh();
                        GH_Convert.ToMesh(mesh, ref rhMesh, 0);
                        return rhMesh.ClosestPoint(pt);
                    }
                default: return Point3d.Origin;
            }
        }

        //Move the point to the next position.
        public Point3d MovePoint(Point3d pt, Vector3d dir, double step)
        {
            Point3d npt = pt + dir * step;
            return npt;
        }

        //Dectect whether the point is out of region.
        public bool DetectOutOfRegion(Rectangle3d rec, Point3d pt)
        {
            double xmin = rec.Corner(0).X;
            double xmax = rec.Corner(2).X;
            double ymin = rec.Corner(0).Y;
            double ymax = rec.Corner(2).Y;
            if (pt.X <= xmin || pt.X >= xmax || pt.Y <= ymin || pt.Y >= ymax)
            {
                return true;
            }
            return false;
        }




        //Detect whether the angle between velocity and the gravity is smaller than threshold, which means that the waterdrop is sinking into a hollow.
        //public bool AngleSmallerThanThreshold(Vector3d v, double threshold)
        //{
        //    double angle = Vector3d.VectorAngle(v, -Vector3d.ZAxis);
        //    if (angle < threshold)
        //        return true;
        //    else return false;
        //}


        //Get the Gaussian curvature at certain point
        //public double GaussianCurvature(object obj, Point3d pt, int type)
        //{
        //    switch (type)
        //    {
        //        case 1:
        //            {
        //                GH_Surface srf = obj as GH_Surface;
        //                Surface rhSrf = null;
        //                GH_Convert.ToSurface(srf, ref rhSrf, 0);
        //                rhSrf.ClosestPoint(pt, out double u, out double v);
        //                SurfaceCurvature sc = rhSrf.CurvatureAt(u, v);
        //                return sc.Gaussian;
        //            }
        //        case 2:
        //            {
        //                GH_Brep brep = obj as GH_Brep;
        //                Brep rhBrep = new Brep();
        //                GH_Convert.ToBrep(brep, ref rhBrep, 0);
        //                rhBrep.ClosestPoint(pt, out Point3d cpt, out ComponentIndex ci, out double s, out double t, 0.1, out Vector3d normal);
        //                if(ci.ComponentIndexType == ComponentIndexType.BrepFace)
        //                {
        //                    Surface face = rhBrep.Faces[ci.Index];
        //                    SurfaceCurvature sc = face.CurvatureAt(s, t);
        //                    return sc.Gaussian;
        //                }
        //                return 1;
        //            }
        //        case 3:
        //            {
        //                GH_Mesh mesh = obj as GH_Mesh;
        //                Mesh rhMesh = new Mesh();
        //                GH_Convert.ToMesh(mesh, ref rhMesh, 0);
        //            }
        //        default: return 1;
        //    }
        //}

        private bool randomChecked = true;
        private bool orderedChecked = false;
        private bool customChecked = false;

        private int modeType;
        public int Mode
        {
            get { return modeType; }
            set
            {
                modeType = value;
                if (modeType == 0)
                {
                    Message = "Random";
                    randomChecked = true;
                    orderedChecked = false;
                    customChecked = false;
                }
                else if (modeType == 1)
                {
                    Message = "Ordered";
                    randomChecked = false;
                    orderedChecked = true;
                    customChecked = false;
                }
                else if (modeType == 2)
                {
                    Message = "Custom";
                    randomChecked = false;
                    orderedChecked = false;
                    customChecked = true;
                }
            }
        }


        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem RandomChecked = new ToolStripMenuItem();
            RandomChecked.Text = "Random Points";
            RandomChecked.Checked = randomChecked;
            RandomChecked.Click += new EventHandler((o, e) =>
            {
                if (orderedChecked || customChecked)
                {
                    Params.Clear();
                    OutputRegister();
                    RandomCheckedParaRegister();
                    Params.OnParametersChanged();
                    ExpireSolution(true);
                }
            });


            ToolStripMenuItem OrderedChecked = new ToolStripMenuItem();
            OrderedChecked.Text = "Ordered Points";
            OrderedChecked.Checked = orderedChecked;
            OrderedChecked.Click += new EventHandler((o, e) =>
            {
                if (randomChecked || customChecked)
                {
                    Params.Clear();
                    OutputRegister();
                    OrderedCheckedParaRegister();
                    Params.OnParametersChanged();
                    ExpireSolution(true);
                }
            });

            ToolStripMenuItem CustomChecked = new ToolStripMenuItem();
            CustomChecked.Text = "Custom Points";
            CustomChecked.Checked = customChecked;
            CustomChecked.Click += new EventHandler((o, e) =>
            {
                if (randomChecked || orderedChecked)
                {
                    Params.Clear();
                    OutputRegister();
                    CustomCheckedParaRegister();
                    Params.OnParametersChanged();
                    ExpireSolution(true);
                }

            });

            menu.Items.Add(RandomChecked);
            menu.Items.Add(OrderedChecked);
            menu.Items.Add(CustomChecked);

            return;
        }


        public void RandomCheckedParaRegister()
        {
            Param_Geometry paramTerrain = new Param_Geometry
            {
                Name = "Terrain",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Terrain" : "T",
                Description = "Terrain that will be calculated water runoff.",
                Access = GH_ParamAccess.item
            };

            Param_Rectangle paramRegion = new Param_Rectangle
            {
                Name = "Region",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Region" : "R",
                Description = "Rectangle region that raindrop will be generated.",
                Access = GH_ParamAccess.item
            };

            Param_Integer paramDensity = new Param_Integer
            {
                Name = "Density",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Density" : "D",
                Description = "Waterdrop density in rectangle region.",
                Access = GH_ParamAccess.item
            };

            Param_Integer paramSeed = new Param_Integer
            {
                Name = "Seed",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Seed" : "S",
                Description = "Seed of the random points.",
                Access = GH_ParamAccess.item
            };

            Param_Number paramLength = new Param_Number
            {
                Name = "Length",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Length" : "L",
                Description = "The length of step every time the waterdrop will take in resultant force direction.",
                Access = GH_ParamAccess.item
            };

            Param_Integer paramIteration = new Param_Integer
            {
                Name = "Iteration",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Iteration" : "I",
                Description = "The iteration that the calculation proceeds on.",
                Access = GH_ParamAccess.item
            };

            Param_Boolean paramRun = new Param_Boolean
            {
                Name = "Run",
                NickName = "Run",
                Description = "Run calculation.",
                Access = GH_ParamAccess.item
            };

            paramRun.SetPersistentData(false);

            Params.RegisterInputParam(paramTerrain);
            Params.RegisterInputParam(paramRegion);
            Params.RegisterInputParam(paramDensity);
            Params.RegisterInputParam(paramSeed);
            Params.RegisterInputParam(paramLength);
            Params.RegisterInputParam(paramIteration);
            Params.RegisterInputParam(paramRun);

            Mode = 0;
        }

        public void OrderedCheckedParaRegister()
        {
            Param_Geometry paramTerrain = new Param_Geometry
            {
                Name = "Terrain",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Terrain" : "T",
                Description = "Terrain that will be calculated water runoff.",
                Access = GH_ParamAccess.item
            };

            Param_Rectangle paramRegion = new Param_Rectangle
            {
                Name = "Region",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Region" : "R",
                Description = "Rectangle region that raindrop will be generated.",
                Access = GH_ParamAccess.item
            };

            Param_Integer paramXCount = new Param_Integer
            {
                Name = "Xcount",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Xcount" : "X",
                Description = "The points amount in X direction.",
                Access = GH_ParamAccess.item
            };

            Param_Integer paramYCount = new Param_Integer
            {
                Name = "Ycount",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Ycount" : "Y",
                Description = "The points amount in Y direction.",
                Access = GH_ParamAccess.item
            };

            Param_Number paramLength = new Param_Number
            {
                Name = "Length",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Length" : "L",
                Description = "The length of step every time the waterdrop will take in resultant force direction.",
                Access = GH_ParamAccess.item
            };

            Param_Integer paramIteration = new Param_Integer
            {
                Name = "Iteration",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Iteration" : "I",
                Description = "The iteration that the calculation proceeds on.",
                Access = GH_ParamAccess.item
            };

            Param_Boolean paramRun = new Param_Boolean
            {
                Name = "Run",
                NickName = "Run",
                Description = "Run calculation.",
                Access = GH_ParamAccess.item
            };

            paramRun.SetPersistentData(false);

            Params.RegisterInputParam(paramTerrain);
            Params.RegisterInputParam(paramRegion);
            Params.RegisterInputParam(paramXCount);
            Params.RegisterInputParam(paramYCount);
            Params.RegisterInputParam(paramLength);
            Params.RegisterInputParam(paramIteration);
            Params.RegisterInputParam(paramRun);

            Mode = 1;
        }

        public void CustomCheckedParaRegister()
        {
            Param_Geometry paramTerrain = new Param_Geometry
            {
                Name = "Terrain",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Terrain" : "T",
                Description = "Terrain that will be calculated water runoff.",
                Access = GH_ParamAccess.item
            };

            Param_Rectangle paramRegion = new Param_Rectangle
            {
                Name = "Region",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Region" : "R",
                Description = "Rectangle region that raindrop will be generated.",
                Access = GH_ParamAccess.item
            };

            Param_Point paramCustomPoints = new Param_Point
            {
                Name = "Points",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Points" : "P",
                Description = "The waterdrop points that user defines.",
                Access = GH_ParamAccess.list
            };

            Param_Number paramLength = new Param_Number
            {
                Name = "Length",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Length" : "L",
                Description = "The length of step every time the waterdrop will take in resultant force direction.",
                Access = GH_ParamAccess.item
            };

            Param_Integer paramIteration = new Param_Integer
            {
                Name = "Iteration",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Iteration" : "I",
                Description = "The iteration that the calculation proceeds on.",
                Access = GH_ParamAccess.item
            };

            Param_Boolean paramRun = new Param_Boolean
            {
                Name = "Run",
                NickName = "Run",
                Description = "Run calculation.",
                Access = GH_ParamAccess.item
            };

            paramRun.SetPersistentData(false);

            Params.RegisterInputParam(paramTerrain);
            Params.RegisterInputParam(paramRegion);
            Params.RegisterInputParam(paramCustomPoints);
            Params.RegisterInputParam(paramLength);
            Params.RegisterInputParam(paramIteration);
            Params.RegisterInputParam(paramRun);

            Mode = 2;
        }

        public void OutputRegister()
        {
            Param_Point paramOutPoints = new Param_Point
            {
                Name = "Points",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Points" : "Pts",
                Description = "Every waterdrop point on the water runoff path.",
                Access = GH_ParamAccess.tree
            };

            Param_Curve paramOutPath = new Param_Curve
            {
                Name = "Path",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Path" : "P",
                Description = "The water runoff path.",
                Access = GH_ParamAccess.list
            };

            Params.RegisterOutputParam(paramOutPoints);
            Params.RegisterOutputParam(paramOutPath);
        }

        private void UpdateIONames()
        {
            if (Mode == 0)
            {
                Params.Input[0].NickName = CentralSettings.CanvasFullNames ? "Terrain" : "T";
                Params.Input[1].NickName = CentralSettings.CanvasFullNames ? "Region" : "R";
                Params.Input[2].NickName = CentralSettings.CanvasFullNames ? "Density" : "D";
                Params.Input[3].NickName = CentralSettings.CanvasFullNames ? "Seed" : "S";
                Params.Input[4].NickName = CentralSettings.CanvasFullNames ? "Length" : "L";
                Params.Input[5].NickName = CentralSettings.CanvasFullNames ? "Iteration" : "I";
                Params.Input[6].NickName = CentralSettings.CanvasFullNames ? "Run" : "Run";

                Params.Output[0].NickName = CentralSettings.CanvasFullNames ? "Points" : "Pts";
                Params.Output[1].NickName = CentralSettings.CanvasFullNames ? "Path" : "P";
            }
      
            else if (Mode == 1)
            {
                Params.Input[0].NickName = CentralSettings.CanvasFullNames ? "Terrain" : "T";
                Params.Input[1].NickName = CentralSettings.CanvasFullNames ? "Region" : "R";
                Params.Input[2].NickName = CentralSettings.CanvasFullNames ? "Xcount" : "X";
                Params.Input[3].NickName = CentralSettings.CanvasFullNames ? "Ycount" : "Y";
                Params.Input[4].NickName = CentralSettings.CanvasFullNames ? "Length" : "L";
                Params.Input[5].NickName = CentralSettings.CanvasFullNames ? "Iteration" : "I";
                Params.Input[6].NickName = CentralSettings.CanvasFullNames ? "Run" : "Run";

                Params.Output[0].NickName = CentralSettings.CanvasFullNames ? "Points" : "Pts";
                Params.Output[1].NickName = CentralSettings.CanvasFullNames ? "Path" : "P";
            }
            else if (Mode == 2)
            {
                Params.Input[0].NickName = CentralSettings.CanvasFullNames ? "Terrain" : "T";
                Params.Input[1].NickName = CentralSettings.CanvasFullNames ? "Region" : "R";
                Params.Input[2].NickName = CentralSettings.CanvasFullNames ? "Points" : "P";
                Params.Input[3].NickName = CentralSettings.CanvasFullNames ? "Length" : "L";
                Params.Input[4].NickName = CentralSettings.CanvasFullNames ? "Iteration" : "I";
                Params.Input[5].NickName = CentralSettings.CanvasFullNames ? "Run" : "Run";

                Params.Output[0].NickName = CentralSettings.CanvasFullNames ? "Points" : "Pts";
                Params.Output[1].NickName = CentralSettings.CanvasFullNames ? "Path" : "P";
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        public override void AddedToDocument(GH_Document document)
        {
            Func<int, string> myFunction = delegate (int x)
            {
                if (x == 0) return "Random";
                else if (x == 1) return "Ordered";
                else return "Custom";
            };

            base.AddedToDocument(document);
            base.Message = myFunction(Mode);
            ExpireSolution(true);
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // First add our own field.
            writer.SetInt32("Mode", Mode);
            // Then call the base class implementation.
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("Mode"))
            {
                Params.Clear();
                OutputRegister();

                // First read our own field.
                Mode = reader.GetInt32("Mode");

                if (Mode == 0)
                    RandomCheckedParaRegister();
                else if (Mode == 1)
                    OrderedCheckedParaRegister();
                else if (Mode == 2)
                    CustomCheckedParaRegister();

                Instances.RedrawCanvas();
                Params.OnParametersChanged();
                ExpireSolution(true);

            }
            // Then call the base class implementation.
            return base.Read(reader);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Water_Runoff;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4CDE9051-2B00-4F41-874F-2CEE76945282"); }
        }
    }
}