using System;
using System.Collections.Generic;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Geometry;

namespace ABS
{
    public class RBAcquireResourceBehavior : GH_Component
    {
        ResourceAcquisitionBehavior resourceBehavior = new ResourceAcquisitionBehavior();
        public RBAcquireResourceBehavior()
          : base("RB-Resource-Behavior", "RB-Resource",
              "Causes agents to look for resources",
              "Thesis", "ABS")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ResourceBehavior", "Resource-Behav", "Resource-Behav", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData("ResourceBehavior", resourceBehavior);
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
            get { return new Guid("7045F644-2DEA-4F6E-848C-D8F252D649E3"); }
        }
        
    }
}
