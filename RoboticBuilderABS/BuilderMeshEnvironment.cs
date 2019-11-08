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

        public List<int> ResourceLocations = new List<int>();
        public List<int> ClaimedResources = new List<int>();
        public List<int> ConstructionLocations = new List<int>();
        public List<int> ChargingLocations = new List<int>();
        public List<int> ConstructedFaces = new List<int>();




        public BuilderMeshEnvironment(Mesh mesh, Surface brep)
        {
            this.Mesh = mesh;
            DetermineBaseFaces(80);
            DetermineResourceLocations();
        }

        public Point3d GetNextResource()
        {
            if (ResourceLocations.Count <= 0)
            {
                return new Point3d();
            }
            // pop from resource locations
            int acquiredResourceId = ResourceLocations[0];
            // add to claimed resources
            ClaimedResources.Add(acquiredResourceId);
            ResourceLocations.Remove(0);
            // return position of resource
            return Mesh.Faces.GetFaceCenter(acquiredResourceId);
        }

        public void DetermineResourceLocations()
        {
            Random rnd = new Random();
            int randomLocation = rnd.Next(0, ConstructedFaces.Count);
            ResourceLocations.Add(randomLocation);
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
            List<object> resourceLocations = GetResourceLocations();
            List<object> returnList = new List<object>();
            returnList.AddRange(meshEdges);
            returnList.AddRange(constructedMeshFaces);
            returnList.AddRange(resourceLocations);
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

        private List<object> GetResourceLocations()
        {
            List<object> resourceFaces = new List<object>();
            foreach (int x in ResourceLocations)
            {
                resourceFaces.Add(Mesh.Faces.GetFaceCenter(x));
            }

            return resourceFaces;
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
    }
}
