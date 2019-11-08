using System;
using System.Collections.Generic;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using Rhino.Geometry;


namespace ABS
{
    public class RBEnvironment : GH_Component
    {
        public RBEnvironment()
          : base("RoboticBuilderEnvironment", "RB-Env",
              "Environment for RB-Agents",
              "Thesis", "ABS")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh environment for agents", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Brep", "Brep", "Brep representation of environment for agents trajectory calculation", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Environment", "Environment", "Mesh environment object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh inputMesh = new Mesh();   
            Surface inputBrep = null;
            if (!DA.GetData("Mesh", ref inputMesh)) return;
            if (!DA.GetData("Brep", ref inputBrep)) return;

            BuilderMeshEnvironment bme = new BuilderMeshEnvironment(inputMesh, inputBrep);
            
            DA.SetData("Environment", bme);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("a38943df-e362-4707-a5a8-48d517255925"); }
        }
    }
}
