using System;
using System.Collections.Generic;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Geometry;

namespace ABS
{
    public class RBGoalBehavior : GH_Component
    {
        private LookForGoalBehavior goalBehavior = new LookForGoalBehavior();
        public RBGoalBehavior()
          : base("RB-Goal-Behavavior", "RB-Goal",
              "Makes sure agents pick goals as they finish old ones",
              "Thesis", "ABS")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Goal Behavior", "Goal", "Goal Behavior", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData("Goal Behavior", (object)this.goalBehavior);
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
            get { return new Guid("3DBB44A3-21B6-45C0-814C-201306E71339"); }
        }
    }
}
