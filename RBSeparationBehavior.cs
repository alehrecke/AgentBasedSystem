using System;
using System.Collections.Generic;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Geometry;

namespace ABS
{
    public class RBSeparationBehavior : GH_Component
    {
        
        public RBSeparationBehavior()
          : base("RB-Separation-Behavior", "RB-Sep",
              "Causes agents to avoid each other",
              "Thesis", "ABS")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight", "Weight", "Weight", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Distance", "Distance", "Distance", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Power", "Power", "Power", GH_ParamAccess.item, 1);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SepBehavior", "Behav", "RandDirBehavior", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            double weight = 1;
            double distance = 1;
            double power = 1;

            DA.GetData("Weight", ref weight);
            DA.GetData("Distance", ref distance);
            DA.GetData("Power", ref power);

            SeparationBehavior sepBehavior = new SeparationBehavior(weight, distance, power, false);

            DA.SetData("SepBehavior", (object)sepBehavior);
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
            get { return new Guid("BC782F93-6CF4-46A9-99AC-7B821D38EC32"); }
        }
    }
}
