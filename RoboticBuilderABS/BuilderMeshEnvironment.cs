using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICD.AbmFramework.Core;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using ICD.AbmFramework.Core.Environments;
using Rhino.Geometry.Intersect;


namespace ABS.RoboticBuilderABS
{
    public class BuilderMeshEnvironment : EnvironmentBase
    {
        public Mesh Mesh;
        // public List<Vector3d> VectorField;
        public List<int> ResourceLocations;
        public List<int> ConstructionLocations;
        public List<int> ChargingLocations;
        public List<int> ConstructedFaces;


        public BuilderMeshEnvironment(Mesh mesh)
        {
            this.Mesh = mesh;
        }

        public void SetMesh(Mesh mesh)
        {
            this.Mesh = mesh;
        }

        public override List<object> GetDisplayGeometry()
        {
            return new List<object>() { (object)this.Mesh };
        }

        public Point3d ClosestPoint(Point3d position)
        {
            return this.Mesh.ClosestPoint(position);
        }

        public Plane SurfacePlane(Point3d position)
        {
            Point3d pointOnMesh;
            Vector3d normalAtPoint;
            if (this.Mesh.ClosestPoint(position, out pointOnMesh, out normalAtPoint, 0.0) != -1)
                return new Plane(pointOnMesh, normalAtPoint);
            return Plane.Unset;
        }

        public Vector3d GetNormal(Point3d position)
        {
            return this.SurfacePlane(position).Normal;
        }

        public Point3d IntersectionPoint(Line line)
        {
            Ray3d ray = new Ray3d(line.From, line.Direction);
            List<GeometryBase> geometryBaseList = new List<GeometryBase>();
            geometryBaseList.Add((GeometryBase)this.Mesh);
            int maxReflections = 1;
            Point3d[] point3dArray = Intersection.RayShoot(ray, (IEnumerable<GeometryBase>)geometryBaseList, maxReflections);
            if (point3dArray == null || point3dArray.Length == 0)
                return Point3d.Unset;
            return point3dArray[0];
        }

        public List<Curve> BoundaryCurves3D()
        {
            List<Curve> curveList = new List<Curve>();
            foreach (Polyline nakedEdge in this.Mesh.GetNakedEdges())
                curveList.Add((Curve)nakedEdge.ToNurbsCurve());
            return curveList;
        }

       
    }
}
