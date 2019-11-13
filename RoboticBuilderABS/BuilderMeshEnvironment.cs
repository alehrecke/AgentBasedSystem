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

        public HashSet<int> ResourceLocations = new HashSet<int>();
        public HashSet<int> ChargingLocations = new HashSet<int>();
        public HashSet<int> ConstructedFaces = new HashSet<int>();
        public HashSet<int> OccupiedFaces = new HashSet<int>();
        public double[] ResourcePheromones;
        public double[] DeliveryPheromones;
        public double[] ChargingPheromones;
        private double fadeRate = 0.01;
        public int lastUpdated = 0;

        public BuilderMeshEnvironment()
        {

        }
        public BuilderMeshEnvironment(Mesh mesh)
        {
            this.Mesh = mesh;
            DetermineBaseFaces(100);
            DetermineResourceLocations();
            DetermineChargingLocations();
            ResourcePheromones = new double[Mesh.Faces.Count];
            DeliveryPheromones = new double[Mesh.Faces.Count];
            ChargingPheromones = new double[Mesh.Faces.Count];
        }

        public void Reset()
        {
            ConstructedFaces.Clear();
            DetermineBaseFaces(60);
            DetermineResourceLocations();
            DetermineChargingLocations();
            ResourcePheromones = new double[Mesh.Faces.Count];
            DeliveryPheromones = new double[Mesh.Faces.Count];
            ChargingPheromones = new double[Mesh.Faces.Count];
        }
        
        
        public void DetermineChargingLocations()
        {
            ChargingLocations.Add(ConstructedFaces.ElementAt(45));
            

        }
        public void DetermineResourceLocations()
        {
            
            ResourceLocations.Add(ConstructedFaces.ElementAt(0));


        }
        public void FadePheromonesArrays()
        {
            for (int i = 0; i < this.ResourcePheromones.Length; i++)
            {
                if (this.ResourcePheromones[i] > 0)
                    this.ResourcePheromones[i] -= fadeRate; 
            }
            for (int i = 0; i < this.DeliveryPheromones.Length; i++)
            {
                if (this.DeliveryPheromones[i] > 0)
                    this.DeliveryPheromones[i] -= fadeRate;
            }
            for (int i = 0; i < this.ChargingPheromones.Length; i++)
            {
                if (this.ChargingPheromones[i] > 0)
                    this.ChargingPheromones[i] -= fadeRate;
            }

        }

        private void DetermineBaseFaces(int baseFaceCount)
        {
            
            List<Point3d> MeshFaceCentres = new List<Point3d>();
            List<int> FaceIndexes = new List<int>();

            for (int i = 0; i< this.Mesh.Faces.Count; i++)
            { 
                MeshFaceCentres.Add(this.Mesh.Faces.GetFaceCenter(i));
                FaceIndexes.Add(i);
            }

            for (int i = 0; i < baseFaceCount; i++)
            {
                double smallestZ = double.MaxValue;
                Point3d lowerPoint = new Point3d(0, 0, 0);
                int lowerIndex = int.MaxValue;

                for (int j = 0; j < MeshFaceCentres.Count; j++)
                {
                    if (MeshFaceCentres[j].Z < smallestZ)
                    {
                        smallestZ = MeshFaceCentres[j].Z;
                        lowerPoint = MeshFaceCentres[j];
                        lowerIndex = FaceIndexes[j];
                    }

                }
                ConstructedFaces.Add(lowerIndex);

                MeshFaceCentres.Remove(lowerPoint);
                FaceIndexes.Remove(lowerIndex);
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

        public List<object> GetMeshEdges()
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

        public List<object> GetResourceLocations()
        {
            List<object> resourceFaces = new List<object>();
            foreach (int x in ResourceLocations)
            {
                resourceFaces.Add(Mesh.Faces.GetFaceCenter(x));
            }

            return resourceFaces;
        }

        public List<object> GetConstructedFaces(List<object> meshEdges)
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

