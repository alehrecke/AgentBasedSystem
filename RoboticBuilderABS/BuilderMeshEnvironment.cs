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
<<<<<<< HEAD
        public Brep BrepForAStar;
=======

>>>>>>> origin/master
        public List<int> ResourceLocations = new List<int>();
        public List<int> ConstructionLocations = new List<int>();
        public List<int> ChargingLocations = new List<int>();
        public List<int> ConstructedFaces = new List<int>();




        public BuilderMeshEnvironment(Mesh mesh)
        {
            this.Mesh = mesh;

            DetermineBaseFaces(10);
        }

        private void DetermineBaseFaces(int baseFaceCount)
        {
            for (int i = 0; i < baseFaceCount; i++)
            {
                ConstructedFaces.Add(i);
            }
        }

        public void SetMesh(Mesh mesh)
        {
            this.Mesh = mesh;
        }

        public override List<object> GetDisplayGeometry()
        {
            List<object> meshEdges = GetMeshEdges();
            List<object> constructedMeshFaces = GetConstructedFaces(meshEdges);
            List<object> returnList = new List<object>();
            returnList.AddRange(meshEdges);
            returnList.AddRange(constructedMeshFaces);
            return returnList;
        }

        private List<object> GetMeshEdges()
        {
            List<object> meshEdges = new List<object>();
            foreach(MeshFace mF in Mesh.Faces)
            {
                List<Point3d> meshVertices = new List<Point3d>();        
                meshVertices.Add(Mesh.Vertices[mF[0]]);
                meshVertices.Add(Mesh.Vertices[mF[1]]);
                meshVertices.Add(Mesh.Vertices[mF[2]]);
                if (mF.IsQuad) meshVertices.Add(Mesh.Vertices[mF[3]]);
                meshVertices.Add(Mesh.Vertices[mF[0]]);
                Polyline meshOutline = new Polyline(meshVertices);
                meshEdges.Add((object)meshOutline);
            }

            return meshEdges;
        }

        private List<object> GetConstructedFaces(List<object> meshEdges)
        {
            List<object> constructedFaces = new List<object>();
            foreach (int x in ConstructedFaces)
            {
                Mesh meshSrf = Mesh.CreateFromClosedPolyline((Polyline) meshEdges[x]);
                constructedFaces.Add(meshSrf);
            }

            return constructedFaces;
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
