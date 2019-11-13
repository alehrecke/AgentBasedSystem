using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core.Environments;
using Rhino.Display;
using Rhino.Geometry;


namespace ABS
{
    public class RBVisualizer : GH_Component
    {
        
        private Mesh mesh = new Mesh();
        private BuilderMeshEnvironment env = new BuilderMeshEnvironment();
        System.Drawing.Color resourceColor = new Color();
        System.Drawing.Color deliveryColor = new Color();
        System.Drawing.Color chargingColor = new Color();
        System.Drawing.Color baseColor = new Color();

        public RBVisualizer()
          : base("RoboticBuilderVisualizer", "RB-Vis",
              "Visualizer for RB-Agent-System",
              "Thesis", "ABS")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Agent System", "RB-Sys", "Agent system from solver", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Pheromone Selected", "Pheromone", "Pheromone To Display", GH_ParamAccess.item);
            pManager.AddColourParameter("Base Color", "Base Color", "Base Color", GH_ParamAccess.item);
            pManager.AddColourParameter("Delivery Color", "Delivery Color", "Delivery Color", GH_ParamAccess.item);
            pManager.AddColourParameter("Resource Color", "Resource Color", "Resource Color", GH_ParamAccess.item);
            pManager.AddColourParameter("Charging Color", "Charging Color", "Charging Color", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Agents", "Agents", "Agent Geometry", GH_ParamAccess.list);
            pManager.AddGenericParameter("Pheromone Faces", "Pheromones", "Pheromone Faces", GH_ParamAccess.list);
            pManager.AddGenericParameter("Mesh Edges", "Mesh Edges", "Mesh Edges", GH_ParamAccess.list);
            pManager.AddGenericParameter("Resources", "Resources", "Resources", GH_ParamAccess.list);
            pManager.AddGenericParameter("Charging", "Charging", "Charging", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BuilderAgentSystem agentSystem = new BuilderAgentSystem();
            DA.GetData<BuilderAgentSystem>("Agent System", ref agentSystem);

            int pheromone = 0;
            DA.GetData("Pheromone Selected", ref pheromone);

            DA.GetData("Delivery Color", ref deliveryColor);
            DA.GetData("Resource Color", ref resourceColor);
            DA.GetData("Charging Color", ref chargingColor);
            DA.GetData("Base Color", ref baseColor);


            env = agentSystem.BuilderEnvironment;
            this.mesh = agentSystem.BuilderEnvironment.Mesh;
            
            List<object> agentGeos = new List<object>();
            foreach(AgentBase agent in agentSystem.Agents)
            {
                agentGeos.AddRange((IEnumerable<object>)agent.GetDisplayGeometries());
            }

            List<Mesh> coloredFaces = ColorMeshPheromones(pheromone);

            List<object> meshEdges = new List<object>();
            meshEdges = env.GetMeshEdges();

            List<Point3d> resourceLocations = new List<Point3d>();
            foreach (int x in env.ResourceLocations)
            {
                resourceLocations.Add(mesh.Faces.GetFaceCenter(x));
            }
            List<Point3d> chargingLocations = new List<Point3d>();
            foreach (int x in env.ChargingLocations)
            {
                chargingLocations.Add(mesh.Faces.GetFaceCenter(x));
            }


            DA.SetDataList("Agents", agentGeos);
            DA.SetDataList("Pheromone Faces", coloredFaces);
            //DA.SetDataList("Mesh Edges", meshEdges);
            DA.SetDataList("Resources", resourceLocations);
            DA.SetDataList("Charging", chargingLocations);


        }

        private List<Mesh> ColorMeshPheromones(int pheromone)
        {
            List<Mesh> coloredFaces = new List<Mesh>();

            for (int i = 0; i < env.ConstructedFaces.Count; i++)
            {
                Mesh displayMesh = new Mesh();
                MeshFace face = mesh.Faces[env.ConstructedFaces.ElementAt(i)];

                displayMesh.Vertices.Add(mesh.Vertices[face.A]);
                displayMesh.Vertices.Add(mesh.Vertices[face.B]);
                displayMesh.Vertices.Add(mesh.Vertices[face.C]);
                if (face.IsQuad) displayMesh.Vertices.Add(mesh.Vertices[face.D]);

                int n = displayMesh.Vertices.Count;

                if (face.IsQuad)
                {
                    displayMesh.Faces.AddFace(new MeshFace(n - 4, n - 3, n - 2, n - 1));
                }
                else
                {
                    displayMesh.Faces.AddFace(new MeshFace(n - 3, n - 2, n - 1));
                }
                double maxValueResource = env.ResourcePheromones.Max();
                double maxValueDelivery = env.DeliveryPheromones.Max();
                double maxValueCharging = env.ChargingPheromones.Max();
                double preRed = 0;
                double preBlue = 0;
                int red;
                int blue;
                int r;
                int b;
                int g;
                var finalColor = baseColor;
                switch (pheromone)
                {
                    case 0: // Resource
                         
                        preRed = (env.ResourcePheromones[env.ConstructedFaces.ElementAt(i)])*4;
                        if (preRed > 255) preRed = 255;
                        red = Convert.ToInt32(preRed);
                        b = 255 - red;
                        if (b < 0) b = 0;
                        g = 255 - red;
                        if (g < 0) g = 0;
                        finalColor = Color.FromArgb(255, 255, g, b);
                        break;
                    case 1: // Delivery
                            
                        preBlue = (env.DeliveryPheromones[env.ConstructedFaces.ElementAt(i)]) * 4;
                        if (preBlue > 255) preBlue = 255;
                        blue = Convert.ToInt32(preBlue);
                        r = 255 - blue;
                        if (r < 0) r = 0;
                        g = 255 - blue;
                        if (g < 0) g = 0;

                        finalColor = Color.FromArgb(255, r, g, 255);
                        break;
                    case 2: // Charging
                        
                        break;
                    case 3: // All
                        preRed = (env.ResourcePheromones[env.ConstructedFaces.ElementAt(i)]) * 4;
                        if (preRed > 255) preRed = 255;
                        red = Convert.ToInt32(preRed);
                        preBlue = (env.DeliveryPheromones[env.ConstructedFaces.ElementAt(i)]) * 4;
                        if (preBlue > 255) preBlue = 255;
                        blue = Convert.ToInt32(preBlue);
                        r = 255 - blue;
                        if (r < 0) r = 0;
                        g = (255 - blue)-red;
                        if (g < 0) g = 0;
                        b = 255 - red;
                        if (b < 0) b = 0;
                        finalColor = Color.FromArgb(255, r, g, b);
                        break;
                }


                //var color = new System.Drawing.Color(255, 255*env.ResourcePheromones[env.ConstructedFaces.ElementAt(i)], );

                displayMesh.VertexColors.Add(finalColor);
                displayMesh.VertexColors.Add(finalColor);
                displayMesh.VertexColors.Add(finalColor);
                if (face.IsQuad) displayMesh.VertexColors.Add(finalColor);

                coloredFaces.Add(displayMesh);
            }

            return coloredFaces;
        }

        public static Color Blend(Color color, Color backColor, double amount)
        {
            byte r = (byte)((color.R) + backColor.R * (amount));
            byte g = (byte)((color.G) + backColor.G * (amount));
            byte b = (byte)((color.B) + backColor.B * (amount));
            return Color.FromArgb(175, r, g, b);
        }
        //public static Color Blend2(Color fg, Color bg, double amount)
        //{
            
        //    // The result

        //    int r = fg.R   + bg.R * bg.A * (1 - fg.A) / r.A; // 0.67
        //    int g = fg.G * fg.A / r.A + bg.G * bg.A * (1 - fg.A) / r.A; // 0.33
        //    int b = fg.B * fg.A / r.A + bg.B * bg.A * (1 - fg.A) / r.A; // 0.00
        //    //int r = (color.R + backColor.R) / 2;
        //    //int g = (color.G + backColor.G) / 2;
        //    //int b = (color.B + backColor.B) / 2;
        //    //byte r = (byte)((color.R) + backColor.R * (amount));
        //    //byte g = (byte)((color.G) + backColor.G * (amount));
        //    //byte b = (byte)((color.B) + backColor.B * (amount));
        //    return Color.FromArgb(255, r, g, b);
        //}
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("49D0C150-0BE2-4012-915D-691F8236D0FE"); }
        }
        
    }
}
