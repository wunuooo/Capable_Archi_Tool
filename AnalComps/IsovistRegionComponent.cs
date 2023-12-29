using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Geometry.Collections;


using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;

namespace CapableArchiTool.AnalComps
{
    public class IsovistRegionComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the VisualRegionComponent class.
        /// </summary>
        public IsovistRegionComponent()
          : base("Isovist Region", "Isovist",
              "Create the isovist region in 2D or 3D.",
              "Capable Archi Tool", "02 Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Origin", "O", "The visual eye position in space.", GH_ParamAccess.item, Point3d.Origin);
            pManager.AddVectorParameter("Direction", "D", "The direction of sightview line", GH_ParamAccess.item, Vector3d.XAxis);
            pManager.AddNumberParameter("Horizontal Angle", "H", "The horizontal angle of sightview.", GH_ParamAccess.item, Math.PI / 4);
            pManager.AddNumberParameter("Radius", "R", "The radius of the sightview region, same as the length of sightview lines.", GH_ParamAccess.item, 50);
            pManager.AddIntegerParameter("Density", "D", "The sightview line density of sightview region. The higher density it has, the more sightview lines is contains." + Environment.NewLine +
                "The value of density in 3D mode should be a interger above 0, and in 3D mode it should be a double between 0 and 1.", GH_ParamAccess.item, 20);
            pManager.AddGeometryParameter("Context", "C", "The context buildings or terrian in the environment.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Run", "Run", "Run this calculation.", GH_ParamAccess.item, false);

            pManager[5].Optional = true;

            Grasshopper.CentralSettings.CanvasFullNamesChanged += OnCanvasFullNamesChanged;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Isovist", "I", "The isovist region.", GH_ParamAccess.item);
        }

        private void OnCanvasFullNamesChanged() => UpdateIONames();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>

        //Get the SDK version.
        //private readonly Version v = typeof(Curve).Assembly.GetName().Version;

        Line renderArrow = new Line();
        List<Line> brepCornerLines = new List<Line>();
        List<Line> meshCornerLines = new List<Line>();
        bool fieldRun = false;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Declare the variables.
            Point3d origin = new Point3d();
            Vector3d direction = new Vector3d();
            double hAngle = 0;
            double pAngle = 0;
            double radius = 0;
            double density = 0.5;
            int count = 10;
            List<Object> context = new List<object>();
            bool run = false;

            DA.GetData(0, ref origin);
            DA.GetData(1, ref direction);
            if (direction == Vector3d.Zero) direction = Vector3d.XAxis;
            DA.GetData(2, ref hAngle);

            int flag = 0;

            if (isMode2D)
            {
                if (hAngle <= 0 || hAngle > Math.PI * 2)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The horizontal angle should between (0, 2π].");
                    flag++;
                }
                DA.GetData(3, ref radius);
                if (radius <= 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The radius should be positive.");
                    flag++;
                }
                DA.GetData(4, ref count);
                if (count <= 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The density should be positive.");
                    flag++;
                }
                if (flag != 0) return;

                DA.GetData(6, ref run);

                Vector3d v = direction;
                v.Unitize();
                renderArrow = new Line(origin, v * radius);

                //Visualize the view region.
                NurbsCurve nurbs = MoveAndRoatateFrustumArc(CreateArcFrustum(radius, hAngle), origin, direction);
                Brep frustumBrep = CreateFrustumBrep(nurbs, origin, hAngle);

                //Calculate the isovist.
                if (!run)
                    DA.SetData(0, frustumBrep);
                else
                {
                    if (!DA.GetDataList(5, context) || context.Count == 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The context is empty.");
                        return;
                    }

                    List<Point3d> intersectPts = MakeIntersection(FrustumLines(nurbs, origin, count), context, true, origin);
                    DA.SetData(0, IsovistBrep(hAngle, intersectPts, origin));
                }
            }
            else
            {
                if (hAngle <= 0 || hAngle > Math.PI * 2)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The horizontal angle should between (0, 2π].");
                    flag++;
                }
                DA.GetData(3, ref pAngle);
                if (pAngle <= 0 || pAngle > Math.PI)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The pitch angle should between (0, π].");
                    flag++;
                }
                DA.GetData(4, ref radius);
                if (radius <= 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The radius should be positive.");
                    flag++;
                }
                DA.GetData(5, ref density);
                if (density <= 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The density should be positive.");
                    flag++;
                }
                if (flag != 0) return;

                DA.GetData(7, ref run);

                fieldRun = run;

                Vector3d v = direction;
                v.Unitize();
                renderArrow = new Line(origin, v * radius);

                Mesh FrustumMesh = new Mesh();
                List<Line> FrustumLines = new List<Line>();


                //Generate the frustum brep.
                Brep FrustumBrep = MoveAndRoatateFrustumBrep(CreateFrustumBrep(radius, hAngle, pAngle), origin, direction);

                //Visualize the view region.
                //if (versionint >= 7)
                //{
                //    Brep FrustumBrep = TransformFrustumBrep(CreateFrustumBrep(radius, hAngle, pAngle), origin, direction);
                //    List<Line> frustumLine = QuadremeshCreateFrustumLine(FrustumBrep, origin, density, out Mesh frustumMesh);
                //    FrustumLines = frustumLine;
                //    FrustumMesh = frustumMesh;
                //}
                //else
                //{
                //    List<Vector3d> starVectors = CreateStarLikeVectorAndMesh(density, out Mesh sphereMesh);
                //    List<Vector3d> frustumVectors = CreateFrustumVectorAndMesh(sphereMesh, starVectors, hAngle, pAngle, out Mesh frustumMesh);
                //    List<Line> frustumLine = CreateFrustumLineAndMesh(frustumMesh, frustumVectors, direction, origin, radius, out Mesh frustumMesh2);
                //    FrustumLines = frustumLine;
                //    FrustumMesh = frustumMesh2;
                //}

                //Calculate the isovist.
                if (!run)
                {
                    DA.SetData(0, FrustumBrep);
                }
                else
                {
                    if (!DA.GetDataList(6, context) || context.Count == 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The context is empty.");
                        return;
                    };
                    FrustumMesh = SimpleMesh(FrustumBrep, density);
                    FrustumLines = CreateRayLines(FrustumMesh, origin);
                    List<Point3d> intersectPts = MakeIntersection(FrustumLines, context, false, origin);
                    Mesh IsoFrustumMesh = IsovistMesh(FrustumMesh, intersectPts, origin);

                    DA.SetData(0, IsoFrustumMesh);
                }
            }
        }


        //2D Intersection
        //Create region arc and brep according to certain radius and horizontal span.
        private ArcCurve CreateArcFrustum(double radius, double hAngle)
        {
            Arc arc = new Arc(Plane.WorldXY, Point3d.Origin, radius, hAngle);
            ArcCurve arcCrv = new ArcCurve(arc);
            arcCrv.Rotate(-hAngle / 2, Vector3d.ZAxis, Point3d.Origin);
            return arcCrv;
        }

        //Rotate the arc frustum with direction and move it to the origin.
        private NurbsCurve MoveAndRoatateFrustumArc(ArcCurve arc, Point3d Origin, Vector3d Direction)
        {
            double hangle = Vector3d.VectorAngle(Vector3d.XAxis, new Vector3d(Direction.X, Direction.Y, 0));
            if (Direction.Y < 0) hangle = 2 * Math.PI - hangle;

            Transform hRotate = Transform.Rotation(hangle, Vector3d.ZAxis, Point3d.Origin);
            arc.Transform(hRotate);

            Transform move = Transform.Translation(new Vector3d(Origin));
            arc.Transform(move);

            return arc.ToNurbsCurve();
        }

        //Generate the isovist brep.
        private Brep CreateFrustumBrep(NurbsCurve crv, Point3d origin, double hAngle)
        {
            if (hAngle != Math.PI * 2)
            {
                Point3d pt1 = crv.PointAtStart;
                Point3d pt2 = crv.PointAtEnd;
                LineCurve lc1 = new LineCurve(origin, pt1);
                LineCurve lc2 = new LineCurve(origin, pt2);

                List<Curve> edges = new List<Curve> { lc1, lc2, crv };
                Brep[] frustumBrep = Brep.CreatePlanarBreps(edges, 0.01);
                return frustumBrep[0];

            }
            else
            {
                List<Curve> edges = new List<Curve> { crv };
                Brep[] frustumBrep = Brep.CreatePlanarBreps(edges, 0.01);
                return frustumBrep[0];
            }
        }

        #region old version of creating frustum lines
        ////Create star-like vector on World-XY plane, and create the view frustm, determined by the horizontal span.
        //private List<Vector3d> CreateStarLikeVectorOnPlane(int density, double hAngle)
        //{
        //    double segRadius = 2 * Math.PI / density;
        //    List<Vector3d> StarVectors = new List<Vector3d>();
        //    for (int i = 0; i < density; i++)
        //    {
        //        if (i * segRadius <= hAngle / 2 || 2 * Math.PI - i * segRadius <= hAngle / 2)
        //        {
        //            Vector3d V = new Vector3d(Math.Cos(i * segRadius), Math.Sin(i * segRadius), 0);
        //            V.Unitize();
        //            StarVectors.Add(V);
        //        }
        //    }
        //    return StarVectors;
        //}

        ////Generate new frustum lines according to the radius, direction and origin of sightview, and the region plane surface of sightview.
        //private List<Line> CreateFrustumLineAndBrep(List<Vector3d> FrustumVectors, double radius, Vector3d Direction, Point3d Origin, out Brep regionBrep, double hAngle)
        //{
        //    Vector3d Dir = new Vector3d(Direction.X, Direction.Y, 0);
        //    double rAngle = Vector3d.VectorAngle(Vector3d.XAxis, Dir);
        //    List<Line> FrustumLine = new List<Line>();
        //    foreach (Vector3d V in FrustumVectors)
        //    {
        //        V.Rotate(rAngle, Vector3d.ZAxis);
        //        FrustumLine.Add(new Line(Origin, V, radius));
        //    }

        //    //Create the sightview region preview.
        //    List<Point3d> lineEndPts = new List<Point3d>();
        //    for (int i = 0; i < FrustumLine.Count; i++) lineEndPts.Add(FrustumLine[i].To);

        //    List<LineCurve> EdgeLines = new List<LineCurve>();

        //    if (hAngle == 2 * Math.PI)
        //    {
        //        for (int i = 0; i < lineEndPts.Count - 1; i++)
        //        {
        //            EdgeLines.Add(new LineCurve(new Line(lineEndPts[i], lineEndPts[i + 1])));
        //        }
        //        EdgeLines.Add(new LineCurve(new Line(lineEndPts.Last(), lineEndPts.First())));
        //        Brep isovistBrep = Brep.CreateEdgeSurface(EdgeLines);
        //        regionBrep = isovistBrep;
        //    }
        //    else
        //    {
        //        //Sort the intersect points.
        //        List<Point3d> NewPoint3d = new List<Point3d>();
        //        for (int i = 0; i < (FrustumLine.Count - 1) / 2; i++)
        //        {
        //            NewPoint3d.Add(lineEndPts[i]);
        //        }
        //        NewPoint3d.Add(Origin);
        //        for (int i = (FrustumLine.Count - 1) / 2 + 1; i < FrustumLine.Count; i++)
        //        {
        //            NewPoint3d.Add(lineEndPts[i]);
        //        }
        //        //Create edge lines.
        //        for (int i = 0; i < NewPoint3d.Count - 1; i++)
        //        {
        //            EdgeLines.Add(new LineCurve(new Line(NewPoint3d[i], NewPoint3d[i + 1])));
        //        }
        //        EdgeLines.Add(new LineCurve(new Line(NewPoint3d.Last(), NewPoint3d.First())));
        //        Brep[] isovistBrep = Brep.CreatePlanarBreps(EdgeLines, 0.1);
        //        regionBrep = isovistBrep[0];
        //    }

        //    return FrustumLine;
        //}
        #endregion

        //Divide the arc and create frustum lines.
        private List<Line> FrustumLines(NurbsCurve crv, Point3d origin, int density)
        {
            List<Line> ls = new List<Line>();
            crv.DivideByCount(density, true, out Point3d[] pts);
            for(int i = 0; i < pts.Length; i++)
            {
                Line l = new Line(origin, pts[i]);
                ls.Add(l);
            }
            return ls;
        }

        //Calculate the interection.
        private List<Point3d> IntersectLineAndContext2D(List<Line> frustumLine, List<Object> context, Point3d origin)
        {
            List<Point3d> IntersectLinePts = new List<Point3d>();
            List<Point3d> IntersectToPts = new List<Point3d>();

            foreach (Line L in frustumLine)
            {
                List<Point3d> IntersectPts = new List<Point3d>();
                foreach (Object Con in context)
                {
                    if (Con is GH_Surface)
                    {
                        GH_Surface surface = Con as GH_Surface;
                        Surface ConSurface = null;
                        GH_Convert.ToSurface(surface, ref ConSurface, 0);

                        Intersection.CurveBrep(L.ToNurbsCurve(), ConSurface.ToBrep(), 0.1, out _, out Point3d[] intersectpts);
                        if (intersectpts.Length != 0)
                            foreach (Point3d pt in intersectpts)
                            {
                                IntersectPts.Add(pt);
                            }
                    }

                    if (Con is GH_Brep)
                    {
                        GH_Brep brep = Con as GH_Brep;
                        Brep ConBrep = new Brep();
                        GH_Convert.ToBrep(brep, ref ConBrep, 0);

                        Intersection.CurveBrep(L.ToNurbsCurve(), ConBrep, 0.1, out _, out Point3d[] intersectpts);
                        if (intersectpts.Length != 0)
                            foreach (Point3d pt in intersectpts)
                            {
                                IntersectPts.Add(pt);
                            }
                    }

                    if (Con is GH_Mesh)
                    {
                        GH_Mesh mesh = Con as GH_Mesh;
                        Mesh ConMesh = new Mesh();
                        GH_Convert.ToMesh(mesh, ref ConMesh, 0);

                        Point3d[] intersectpts = Intersection.MeshLine(ConMesh, L);
                        if (intersectpts.Length != 0)
                            foreach (Point3d pt in intersectpts)
                            {
                                IntersectPts.Add(pt);
                            }
                    }
                }
                if (IntersectPts.Count != 0)
                {
                    List<double> distance = new List<double>();
                    foreach (Point3d pt in IntersectPts) distance.Add((new Line(origin, pt)).Length);
                    IntersectLinePts.Add(IntersectPts[distance.IndexOf(distance.Min())]);
                }
                else IntersectLinePts.Add(L.To);
            }
            foreach (Point3d pt in IntersectLinePts)
            {
                IntersectToPts.Add(new Point3d(pt.X, pt.Y, origin.Z));
            }

            return IntersectToPts;
        }

        //Create isovist region brep.
        private Brep IsovistBrep(double hAngle, List<Point3d> intersectPts, Point3d origin)
        {
            if (hAngle != 2 * Math.PI)
            {
                LineCurve l1 = new LineCurve(intersectPts.First(), origin);
                LineCurve l2 = new LineCurve(intersectPts.Last(), origin);
                PolylineCurve pl = new PolylineCurve(intersectPts);
                Brep[] frustumBrep = Brep.CreatePlanarBreps(new List<Curve> { l1, l2, pl }, 0.01);
                if (frustumBrep != null) return frustumBrep[0];
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The sighview point is too close to the context surface.");
                    return null;
                }
            }
            else
            {
                intersectPts.Add(intersectPts.First());
                PolylineCurve pl = new PolylineCurve(intersectPts);
                Brep[] frustumBrep = Brep.CreatePlanarBreps(new List<Curve> { pl }, 0.01);
                if (frustumBrep != null) return frustumBrep[0];
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The sighview point is too close to the context surface.");
                    return null;
                }
            }
        }


        //3D Intersection
        //Create frustum according to certain radius sphere and horizontal span and pitch span.
        private Brep CreateFrustumBrep(double radius, double hAngle, double pAngle)
        {
            Sphere sphere = new Sphere(Point3d.Origin, radius);
            sphere.Rotate(Math.PI, Vector3d.ZAxis);
            Brep sphereBrep = Brep.CreateFromSphere(sphere);

            if (hAngle == Math.PI * 2 && pAngle == Math.PI) return sphereBrep;
            if (hAngle == Math.PI * 2 && pAngle != Math.PI)
            {
                Vector3d v = new Vector3d(radius, 0, 0);
                v.Rotate(-pAngle / 2, Vector3d.YAxis);

                Plane upperPlane = new Plane(0, 0, 1, -v.Z);
                Plane lowerPlane = new Plane(0, 0, 1, v.Z);

                Point3d pt1 = new Point3d(2 * radius, 2 * radius, v.Z);
                Point3d pt3 = new Point3d(-2 * radius, -2 * radius, v.Z);

                Rectangle3d uprec = new Rectangle3d(upperPlane, pt1, pt3);
                Rectangle3d lorec = new Rectangle3d(lowerPlane, pt1, pt3);
                PolylineCurve upplc = uprec.ToPolyline().ToPolylineCurve();
                PolylineCurve loplc = lorec.ToPolyline().ToPolylineCurve();
                Brep upperBrep = Brep.CreatePlanarBreps(upplc, 0.1)[0];
                Brep lowerBrep = Brep.CreatePlanarBreps(loplc, 0.1)[0];
                List<Brep> cutterBreps = new List<Brep> { upperBrep, lowerBrep };

                Brep[] splitBrep = sphereBrep.Split(cutterBreps, 0.1);
                return splitBrep[2];

            }
            if (hAngle != Math.PI * 2 && pAngle == Math.PI)
            {
                Vector3d v1 = new Vector3d(radius * 2, 0, 0);
                v1.Rotate(hAngle / 2, Vector3d.ZAxis);
                Vector3d v2 = new Vector3d(radius * 2, 0, 0);
                v2.Rotate(-hAngle / 2, Vector3d.ZAxis);

                Line l1 = new Line(new Point3d(0, 0, radius * 2), v1);
                LineCurve lc1 = new LineCurve(l1);
                Line l2 = new Line(new Point3d(0, 0, radius * 2), v2);
                LineCurve lc2 = new LineCurve(l2);
                LineCurve[] upperlc = new LineCurve[] { lc1, lc2 };

                Line l3 = new Line(new Point3d(0, 0, -radius * 2), v1);
                LineCurve lc3 = new LineCurve(l3);
                Line l4 = new Line(new Point3d(0, 0, -radius * 2), v2);
                LineCurve lc4 = new LineCurve(l4);
                LineCurve[] lowerlc = new LineCurve[] { lc3, lc4 };

                Curve[] joinedUpperCrv = LineCurve.JoinCurves(upperlc, 0.1);
                Curve[] joinedLowerCrv = LineCurve.JoinCurves(lowerlc, 0.1);

                Curve[] loftCrv = new Curve[2] { joinedUpperCrv[0], joinedLowerCrv[0] };

                Brep[] loftBrep = Brep.CreateFromLoft(loftCrv, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);

                Brep[] splitBrep = sphereBrep.Split(loftBrep, 0.1);

                //for (int i = 0; i < splitBrep.Length; i++)
                //{
                //    Line l = new Line(new Point3d(0, 0, 0), new Point3d(radius * 2, 0, 0));
                //    LineCurve lc = new LineCurve(l);
                //    bool isIntersect = Intersection.CurveBrep(lc, splitBrep[i], 0.1, out _, out _);
                //    if (isIntersect) return splitBrep[i];
                //}

                return splitBrep[1];
            }
            if (hAngle != Math.PI * 2 && pAngle != Math.PI)
            {
                Brep preFrustumBrep = new Brep();

                Vector3d v = new Vector3d(radius, 0, 0);
                v.Rotate(-pAngle / 2, Vector3d.YAxis);

                Plane upperPlane = new Plane(0, 0, 1, -v.Z);
                Plane lowerPlane = new Plane(0, 0, 1, v.Z);

                Point3d pt1 = new Point3d(2 * radius, 2 * radius, v.Z);
                Point3d pt3 = new Point3d(-2 * radius, -2 * radius, v.Z);

                Rectangle3d uprec = new Rectangle3d(upperPlane, pt1, pt3);
                Rectangle3d lorec = new Rectangle3d(lowerPlane, pt1, pt3);
                PolylineCurve upplc = uprec.ToPolyline().ToPolylineCurve();
                PolylineCurve loplc = lorec.ToPolyline().ToPolylineCurve();
                Brep upperBrep = Brep.CreatePlanarBreps(upplc, 0.1)[0];
                Brep lowerBrep = Brep.CreatePlanarBreps(loplc, 0.1)[0];
                List<Brep> cutterBreps = new List<Brep> { upperBrep, lowerBrep };

                Brep[] splitBrep = sphereBrep.Split(cutterBreps, 0.1);
                preFrustumBrep = splitBrep[2];

                Vector3d v1 = new Vector3d(radius * 2, 0, 0);
                v1.Rotate(hAngle / 2, Vector3d.ZAxis);
                Vector3d v2 = new Vector3d(radius * 2, 0, 0);
                v2.Rotate(-hAngle / 2, Vector3d.ZAxis);

                Line l1 = new Line(new Point3d(0, 0, radius * 2), v1);
                LineCurve lc1 = new LineCurve(l1);
                Line l2 = new Line(new Point3d(0, 0, radius * 2), v2);
                LineCurve lc2 = new LineCurve(l2);
                LineCurve[] upperlc = new LineCurve[] { lc1, lc2 };

                Line l3 = new Line(new Point3d(0, 0, -radius * 2), v1);
                LineCurve lc3 = new LineCurve(l3);
                Line l4 = new Line(new Point3d(0, 0, -radius * 2), v2);
                LineCurve lc4 = new LineCurve(l4);
                LineCurve[] lowerlc = new LineCurve[] { lc3, lc4 };

                Curve[] joinedUpperCrv = LineCurve.JoinCurves(upperlc, 0.1);
                Curve[] joinedLowerCrv = LineCurve.JoinCurves(lowerlc, 0.1);

                Curve[] loftCrv = new Curve[2] { joinedUpperCrv[0], joinedLowerCrv[0] };

                Brep[] loftBrep = Brep.CreateFromLoft(loftCrv, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);

                Brep[] splitBrep2 = preFrustumBrep.Split(loftBrep, 0.1);
                return splitBrep2[1];

            }
            return null;
        }

        //Rotate the frustum with direction and move it to the origin.
        private Brep MoveAndRoatateFrustumBrep(Brep Frustum, Point3d Origin, Vector3d Direction)
        {
            //Get frust line through ray according to the direction, radius and position of sightview.
            double pangle = Vector3d.VectorAngle(Direction, new Vector3d(Direction.X, Direction.Y, 0));
            if (Direction.X == 0 && Direction.Y == 0) pangle = Direction.Z <= 0 ? 0.5 * Math.PI : 1.5 * Math.PI;
            else pangle = Direction.Z <= 0 ? pangle : 2 * Math.PI - pangle;

            Transform pRotate = Transform.Rotation(pangle, Vector3d.YAxis, Point3d.Origin);

            double hangle = Vector3d.VectorAngle(Vector3d.XAxis, new Vector3d(Direction.X, Direction.Y, 0));
            if (Direction.Y < 0) hangle = 2 * Math.PI - hangle;

            Transform hRotate = Transform.Rotation(hangle, Vector3d.ZAxis, Point3d.Origin);

            Frustum.Transform(pRotate);
            Frustum.Transform(hRotate);

            Transform move = Transform.Translation(new Vector3d(Origin));

            Frustum.Transform(move);

            //List<Brep> bs = new List<Brep>();
            //Brep frustumBrep = new Brep();
            //if (Frustum.Edges != null)
            //{
            //    foreach (BrepEdge edge in Frustum.Edges)
            //    {
            //        Curve crv = edge.DuplicateCurve();
            //        Point3d pt1 = crv.PointAtStart;
            //        Point3d pt2 = crv.PointAtEnd;

            //        LineCurve crv1 = new LineCurve(pt1, Origin);
            //        LineCurve crv2 = new LineCurve(pt2, Origin);

            //        List<Curve> crvs = new List<Curve> { crv, crv1, crv2 };

            //        Brep[] b = Brep.CreatePlanarBreps(crvs, 0.01);
            //        bs.AddRange(b.ToList<Brep>());
            //    }
            //    bs.Add(Frustum);
            //    frustumBrep = Brep.JoinBreps(bs, 0.01)[0];
            //}

            Point3d[] brepPts = Frustum.DuplicateVertices();
            brepCornerLines.Clear();
            meshCornerLines.Clear();
            for (int i = 0; i < brepPts.Length; i++)
            {
                brepCornerLines.Add(new Line(Origin, brepPts[i]));
            }

            return Frustum;
        }

        //Transform the brep to mesh, the default method is simple mesh, if the rhino version is beyond 7, the quadremesh is valid.
        private Mesh SimpleMesh(Brep Frustum, double density)
        {
            MeshingParameters meshParam = new MeshingParameters(density);
            Mesh[] mesh = Mesh.CreateFromBrep(Frustum, meshParam);
            return mesh[0];
        }

        //Get lines which are created from origin to vertices on mesh.
        List<Line> CreateRayLines(Mesh FrustumMesh, Point3d Origin)
        {
            List<Line> FrustumLine = new List<Line>();
            foreach (Point3d pt in FrustumMesh.Vertices)
            {
                FrustumLine.Add(new Line(Origin, pt));
            }
            return FrustumLine;
        }

        #region old version of creating the frustum brep
        ////Quad Remesh the frustum brep to mesh, and get lines which are created from origin to vertices on mesh.
        //private List<Line> QuadremeshCreateFrustumLine(Brep Frustum, Point3d Origin, int density, out Mesh FrustumMesh)
        //{
        //    QuadRemeshParameters para = new QuadRemeshParameters
        //    {
        //        AdaptiveQuadCount = false,
        //        AdaptiveSize = 50,
        //        DetectHardEdges = true,
        //        PreserveMeshArrayEdgesMode = 1,
        //        TargetQuadCount = density
        //    };

        //    Mesh QuadFrustumMesh = Mesh.QuadRemeshBrep(Frustum, para);
        //    QuadFrustumMesh.Vertices.CombineIdentical(true, true);

        //    List<Line> FrustumLine = new List<Line>();
        //    foreach (Point3d pt in QuadFrustumMesh.Vertices)
        //    {
        //        FrustumLine.Add(new Line(Origin, pt));
        //    }

        //    FrustumMesh = QuadFrustumMesh;
        //    return FrustumLine;
        //}


        ////Create star-like vector and the sphere mesh, which is determined by a cube.
        //private List<Vector3d> CreateStarLikeVectorAndMesh(int density, out Mesh SphereMesh)
        //{
        //    List<Vector3d> StarVectors = new List<Vector3d>();

        //    Box VectorBox = new Box(Plane.WorldXY, new Interval(-1.0, 1.0), new Interval(-1.0, 1.0), new Interval(-1.0, 1.0));
        //    Mesh BoxMesh = Mesh.CreateFromBox(VectorBox, density, density, density);
        //    BoxMesh.Vertices.CombineIdentical(true, true);

        //    for (int i = 0; i < BoxMesh.Vertices.Count; i++)
        //    {
        //        Vector3d StarVector = BoxMesh.Vertices[i] - Plane.WorldXY.Origin;
        //        StarVector.Unitize();
        //        StarVectors.Add(StarVector);
        //        BoxMesh.Vertices[i] = new Point3f((float)StarVector.X, (float)StarVector.Y, (float)StarVector.Z);
        //    }

        //    SphereMesh = BoxMesh;
        //    return StarVectors;
        //}

        ////Create the view frustum, which contains the sight vector and frustum mesh, determined by the horizontal span and pitch sapn.
        //private List<Vector3d> CreateFrustumVectorAndMesh(Mesh SphereMesh, List<Vector3d> StarVectors, double hAngle, double pAngle, out Mesh FrustumMesh)
        //{
        //    List<Vector3d> PreFrustumVector = new List<Vector3d>();
        //    List<int> FaceIndex = new List<int>();

        //    for (int i = 0; i < StarVectors.Count; i++)
        //    {
        //        double coshA = new Vector3d(StarVectors[i].X, StarVectors[i].Y, 0) * Vector3d.XAxis;
        //        double sinpA = Math.Abs(StarVectors[i] * Vector3d.ZAxis);

        //        //Get the certain vector in the span.
        //        if (coshA <= 1 && coshA >= Math.Cos(hAngle / 2) && sinpA >= 0 && sinpA <= Math.Sin(pAngle / 2))
        //        {
        //            PreFrustumVector.Add(StarVectors[i]);
        //        }
        //        else foreach (int index in SphereMesh.Vertices.GetVertexFaces(i)) FaceIndex.Add(index);
        //    }

        //    //Delete mesh face.
        //    List<int> FaceIndexNoRepeat = FaceIndex.Distinct().ToList();
        //    SphereMesh.Faces.DeleteFaces(FaceIndexNoRepeat);
        //    FrustumMesh = SphereMesh;

        //    //Get new points order.
        //    Point3d[] points = FrustumMesh.Vertices.ToPoint3dArray();
        //    List<Vector3d> FrustumVector = new List<Vector3d>();
        //    for (int i = 0; i < points.Length; i++) FrustumVector.Add(new Vector3d(points[i].X, points[i].Y, points[i].Z));

        //    return FrustumVector;
        //}

        ////Generate new frustum lines according to the radius, direction and origin of sightview, and the mesh with correct radius, direction and position.
        //private List<Line> CreateFrustumLineAndMesh(Mesh PreFrustumMesh, List<Vector3d> FrustumVectors, Vector3d Direction, Point3d Origin, double radius, out Mesh FrustumMesh)
        //{
        //    //Get frust line through ray according to the direction, radius and position of sightview.
        //    double pangle = Vector3d.VectorAngle(Direction, new Vector3d(Direction.X, Direction.Y, 0));
        //    if (Direction.X == 0 && Direction.Y == 0) pangle = Direction.Z <= 0 ? 0.5 * Math.PI : 1.5 * Math.PI;
        //    else pangle = Direction.Z <= 0 ? pangle : 2 * Math.PI - pangle;

        //    double hangle = Vector3d.VectorAngle(Vector3d.XAxis, new Vector3d(Direction.X, Direction.Y, 0));
        //    if (Direction.Y < 0) hangle = 2 * Math.PI - hangle;
        //    //double radian = Vector3d.VectorAngle(Direction, Vector3d.XAxis);
        //    //Vector3d axis = Vector3d.CrossProduct(Vector3d.XAxis, Direction);

        //    List<Line> FrustumLine = new List<Line>();
        //    foreach (Vector3d V in FrustumVectors)
        //    {
        //        //V.Rotate(radian, axis);

        //        V.Rotate(pangle, Vector3d.YAxis);
        //        V.Rotate(hangle, Vector3d.ZAxis);

        //        Line line = new Line(Origin, V, radius);
        //        FrustumLine.Add(line);
        //    }

        //    //Rotate, expand and move the FrustumMesh.
        //    for (int i = 0; i < PreFrustumMesh.Vertices.Count; i++)
        //    {
        //        PreFrustumMesh.Vertices[i] = (Point3f)FrustumLine[i].To;
        //    }

        //    FrustumMesh = PreFrustumMesh;
        //    return FrustumLine;
        //}
        #endregion

        //Calculate the interection.
        private List<Point3d> IntersectLineAndContext3D(List<Line> FrustumLine, List<Object> Context, Point3d Origin)
        {
            List<Point3d> IntersectLinePts = new List<Point3d>();

            foreach (Line L in FrustumLine)
            {
                List<Point3d> IntersectPts = new List<Point3d>();
                foreach (Object Con in Context)
                {
                    if (Con is GH_Surface)
                    {
                        GH_Surface surface = Con as GH_Surface;
                        Surface ConSurface = null;
                        GH_Convert.ToSurface(surface, ref ConSurface, 0);

                        Intersection.CurveBrep(L.ToNurbsCurve(), ConSurface.ToBrep(), 0.1, out _, out Point3d[] intersectpts);
                        if (intersectpts.Length != 0)
                            foreach (Point3d pt in intersectpts)
                            {
                                IntersectPts.Add(pt);
                            }
                    }

                    if (Con is GH_Brep)
                    {
                        GH_Brep brep = Con as GH_Brep;
                        Brep ConBrep = new Brep();
                        GH_Convert.ToBrep(brep, ref ConBrep, 0);

                        Intersection.CurveBrep(L.ToNurbsCurve(), ConBrep, 0.1, out _, out Point3d[] intersectpts);
                        if (intersectpts.Length != 0)
                            foreach (Point3d pt in intersectpts)
                            {
                                IntersectPts.Add(pt);
                            }
                    }

                    if (Con is GH_Mesh)
                    {
                        GH_Mesh mesh = Con as GH_Mesh;
                        Mesh ConMesh = new Mesh();
                        GH_Convert.ToMesh(mesh, ref ConMesh, 0);

                        Point3d[] intersectpts = Intersection.MeshLine(ConMesh, L);
                        if (intersectpts.Length != 0)
                            foreach (Point3d pt in intersectpts)
                            {
                                IntersectPts.Add(pt);
                            }
                    }
                }
                if (IntersectPts.Count != 0)
                {
                    List<double> distance = new List<double>();
                    foreach (Point3d pt in IntersectPts) distance.Add((new Line(Origin, pt)).Length);
                    IntersectLinePts.Add(IntersectPts[distance.IndexOf(distance.Min())]);
                }
                else IntersectLinePts.Add(L.To);
            }
            return IntersectLinePts;
        }

        //Create isovist isovist mesh.
        private Mesh IsovistMesh(Mesh FrustumMesh, List<Point3d> IntersectPts, Point3d Origin)
        {
            for (int i = 0; i < FrustumMesh.Vertices.Count; i++)
            {
                FrustumMesh.Vertices[i] = (Point3f)IntersectPts[i];
            }

            //Polyline[] pls = FrustumMesh.GetNakedEdges();
            //Mesh sideMesh = new Mesh();
            //if(pls.Length != 0)
            //{
            //    for(int i = 0; i < pls.Length; i++)
            //    {
            //        Point3d pt1 = pls[i].First;
            //        Point3d pt2 = pls[i].Last;
            //        List<Point3d> pts = new List<Point3d> { pt1, pt2, Origin };
            //        Mesh mesh = new Mesh();
            //        mesh.Vertices.AddVertices(pts);
            //        mesh.Faces.AddFace(0, 1, 2);

            //        sideMesh.Append(mesh);
            //    }

            //    FrustumMesh.Append(sideMesh);
            //}

            return FrustumMesh;
        }

        //Convert context to mesh object and make line-ray intersection.
        private List<Point3d> MakeIntersection(List<Line> FrustumLine, List<Object> Context, bool is2D, Point3d Origin)
        {
            List<Point3d> IntersectLinePts = new List<Point3d>();
            MeshingParameters meshParam = new MeshingParameters(1);

            Mesh allContext = new Mesh();

            foreach (Object Con in Context)
            {
                if (Con is GH_Surface)
                {
                    GH_Surface surface = Con as GH_Surface;
                    Surface conSurface = null;
                    GH_Convert.ToSurface(surface, ref conSurface, 0);

                    Brep conBrep = conSurface.ToBrep();

                    Mesh[] mesh = Mesh.CreateFromBrep(conBrep, meshParam);
                    foreach (Mesh m in mesh.ToList<Mesh>())
                    {
                        allContext.Append(m);
                    }
                }

                if (Con is GH_Brep)
                {
                    GH_Brep brep = Con as GH_Brep;
                    Brep conBrep = new Brep();
                    GH_Convert.ToBrep(brep, ref conBrep, 0);

                    Mesh[] mesh = Mesh.CreateFromBrep(conBrep, meshParam);
                    foreach (Mesh m in mesh.ToList<Mesh>())
                    {
                        allContext.Append(m);
                    }
                }

                if (Con is GH_Mesh)
                {
                    GH_Mesh mesh = Con as GH_Mesh;
                    Mesh conMesh = new Mesh();
                    GH_Convert.ToMesh(mesh, ref conMesh, 0);

                    allContext.Append(conMesh);
                }
            }

            if (is2D == true)
            {
                foreach (Line l in FrustumLine)
                {
                    Point3d[] pt = Intersection.MeshLine(allContext, l);
                    if (pt.Length != 0)
                    {
                        IntersectLinePts.Add(new Point3d(pt[0].X, pt[0].Y, Origin.Z));
                    }
                    else
                    {
                        IntersectLinePts.Add(l.To);
                    }
                }
            }
            else
            {
                foreach (Line l in FrustumLine)
                {
                    Point3d[] pt = Intersection.MeshLine(allContext, l);
                    if (pt.Length != 0)
                    {
                        IntersectLinePts.Add(pt[0]);
                    }
                    else
                    {
                        IntersectLinePts.Add(l.To);
                    }
                }

                foreach (Line l in brepCornerLines)
                {
                    Point3d[] pt = Intersection.MeshLine(allContext, l);
                    if (pt.Length == 0)
                        meshCornerLines.Add(new Line(Origin, l.To));
                    else
                        meshCornerLines.Add(new Line(Origin, pt[0]));
                }
            }


            return IntersectLinePts;
        }

        //Calculate the target
        //Area


        //Initialize the component.
        bool isMode2D = true;

        IEnumerator<IGH_Param> ParamEnum;
        readonly List<IGH_Param> paramList = new List<IGH_Param>();

        private void Mode2DParaRegister()
        {
            Param_Number paramRadius = new Param_Number
            {
                Name = "Radius",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Radius" : "R",
                Description = "The radius of the sightview region, same as the length of sightview lines.",
                Access = GH_ParamAccess.item
            };
            Param_Integer paramDensity = new Param_Integer
            {
                Name = "Density",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Density" : "D",
                Description = "The sightview line density of sightview region. The higher density it has, the more sightview lines is contains." + Environment.NewLine +
                "The value of density in 3D mode should be a interger above 0, and in 3D mode it should be a double between 0 and 1.",
                Access = GH_ParamAccess.item
            };
            Param_Geometry paramContext = new Param_Geometry
            {
                Name = "Context",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Context" : "C",
                Description = "The context buildings or terrian in the environment.",
                Access = GH_ParamAccess.list
            };
            Param_Boolean paramRun = new Param_Boolean
            {
                Name = "Run",
                NickName = "Run",
                Description = "Run this calculation.",
                Access = GH_ParamAccess.item
            };
            Param_Geometry paramIsovist = new Param_Geometry
            {
                Name = "Isovist",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Isovist" : "I",
                Description = "The isovist region.",
                Access = GH_ParamAccess.item
            };

            paramRadius.SetPersistentData(50);
            paramDensity.SetPersistentData(20);
            paramRun.SetPersistentData(false);
            paramContext.Optional = true;

            Params.RegisterInputParam(paramRadius);
            Params.RegisterInputParam(paramDensity);
            Params.RegisterInputParam(paramContext);
            Params.RegisterInputParam(paramRun);
            Params.RegisterOutputParam(paramIsovist);
        }

        private void Mode3DParaRegister()
        {
            Param_Number paramPitchAngle = new Param_Number
            {
                Name = "Pitch Angle",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Pitch Angle" : "P",
                Description = "The Pitch angle of sightview.",
                Access = GH_ParamAccess.item
            };
            Param_Number paramRadius = new Param_Number
            {
                Name = "Radius",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Radius" : "R",
                Description = "The radius of the sightview region, same as the length of sightview lines.",
                Access = GH_ParamAccess.item
            };
            Param_Number paramDensity = new Param_Number
            {
                Name = "Density",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Density" : "D",
                Description = "The sightview line density of sightview region. The higher density it has, the more sightview lines is contains." + Environment.NewLine +
                "The value of density in 3D mode should be a interger above 0, and in 3D mode it should be a double between 0 and 1.",
                Access = GH_ParamAccess.item
            };
            Param_Geometry paramContext = new Param_Geometry
            {
                Name = "Context",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Context" : "C",
                Description = "The context buildings or terrian in the environment.",
                Access = GH_ParamAccess.list
            };
            Param_Boolean paramRun = new Param_Boolean
            {
                Name = "Run",
                NickName = "Run",
                Description = "Run this calculation.",
                Access = GH_ParamAccess.item
            };
            Param_Geometry paramIsovist = new Param_Geometry
            {
                Name = "Isovist",
                NickName = Grasshopper.CentralSettings.CanvasFullNames ? "Isovist" : "I",
                Description = "The isovist region.",
                Access = GH_ParamAccess.item
            };

            paramPitchAngle.SetPersistentData(Math.PI / 4);
            paramRadius.SetPersistentData(50);
            paramDensity.SetPersistentData(0.5);
            paramRun.SetPersistentData(false);
            paramContext.Optional = true;

            Params.RegisterInputParam(paramPitchAngle);
            Params.RegisterInputParam(paramRadius);
            Params.RegisterInputParam(paramDensity);
            Params.RegisterInputParam(paramContext);
            Params.RegisterInputParam(paramRun);
            Params.RegisterOutputParam(paramIsovist);
        }

        //Add the label.
        public void UpdateMessage()
        {
            base.Message = (isMode2D ? "2D Mode" : "3D Mode");
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem Mode2DChecked = new ToolStripMenuItem
            {
                Text = "2D Mode",
                Checked = isMode2D
            };
            Mode2DChecked.Click += new EventHandler((o, e) =>
            {
                if (!isMode2D)
                {
                    isMode2D = true;

                    UpdateMessage();

                    ParamEnum = Params.GetEnumerator();
                    while (ParamEnum.MoveNext())
                    {
                        IGH_Param param = ParamEnum.Current;
                        if (param.Name != "Origin" && param.Name != "Direction" && param.Name != "Horizontal Angle")
                        {
                            paramList.Add(param);
                        }
                    }
                    ParamEnum.Reset();

                    foreach (IGH_Param param in paramList)
                    {
                        Params.UnregisterParameter(param);
                    }
                    paramList.Clear();

                    Mode2DParaRegister();
                    Params.OnParametersChanged();
                    ExpireSolution(true);
                }
            });


            ToolStripMenuItem Mode3DChecked = new ToolStripMenuItem
            {
                Text = "3D Mode",
                Checked = !isMode2D
            };
            Mode3DChecked.Click += new EventHandler((o, e) =>
            {
                if (isMode2D)
                {
                    isMode2D = false;

                    UpdateMessage();

                    ParamEnum = Params.GetEnumerator();
                    while (ParamEnum.MoveNext())
                    {
                        IGH_Param param = ParamEnum.Current;
                        if (param.Name != "Origin" && param.Name != "Direction" && param.Name != "Horizontal Angle")
                        {
                            paramList.Add(param);
                        }
                    }
                    ParamEnum.Reset();

                    foreach (IGH_Param param in paramList)
                    {
                        Params.UnregisterParameter(param);
                    }
                    paramList.Clear();

                    Mode3DParaRegister();
                    Params.OnParametersChanged();
                    ExpireSolution(true);
                }
            });

            menu.Items.Add(Mode2DChecked);
            menu.Items.Add(Mode3DChecked);

            return;
        }

        private void UpdateIONames()
        {
            if (isMode2D)
            {
                Params.Input[3].NickName = CentralSettings.CanvasFullNames ? "Radius" : "R";
                Params.Input[4].NickName = CentralSettings.CanvasFullNames ? "Density" : "D";
                Params.Input[5].NickName = CentralSettings.CanvasFullNames ? "Context" : "C";
            }

            else
            {
                Params.Input[3].NickName = CentralSettings.CanvasFullNames ? "Pitch Angle" : "P";
                Params.Input[4].NickName = CentralSettings.CanvasFullNames ? "Radius" : "R";
                Params.Input[5].NickName = CentralSettings.CanvasFullNames ? "Density" : "D";
                Params.Input[6].NickName = CentralSettings.CanvasFullNames ? "Context" : "C";
            }

            Params.Output[0].NickName = CentralSettings.CanvasFullNames ? "Isovist" : "I";

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("Mode", isMode2D);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("Mode"))
            {
                isMode2D = reader.GetBoolean("Mode");

                ParamEnum = Params.GetEnumerator();
                while (ParamEnum.MoveNext())
                {
                    IGH_Param param = ParamEnum.Current;
                    if (param.Name != "Origin" && param.Name != "Direction" && param.Name != "Horizontal Angle")
                    {
                        paramList.Add(param);
                    }
                }
                ParamEnum.Reset();

                foreach (IGH_Param param in paramList)
                {
                    Params.UnregisterParameter(param);
                }
                paramList.Clear();


                if (isMode2D) Mode2DParaRegister();
                else Mode3DParaRegister();
                Params.OnParametersChanged();
                ExpireSolution(true);
            }
            return base.Read(reader);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            UpdateMessage();
            ExpireSolution(true);
        }

        Color color = Color.DarkRed;

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            args.Display.DrawArrow(renderArrow, color);
            if (!isMode2D)
            {
                if (!fieldRun)
                {
                    foreach (Line l in brepCornerLines)
                    args.Display.DrawLine(l, color);
                }
                else
                {
                    foreach (Line l in meshCornerLines)
                    args.Display.DrawLine(l, color);
                }
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Isovist_Region;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("CA5B065B-977A-499F-B101-D4704B956F99"); }
        }
    }
}