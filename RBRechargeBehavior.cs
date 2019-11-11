using System;
using System.Collections.Generic;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Geometry;

namespace ABS
{
    public class RBRechargeBehavior : GH_Component
    {
        private RechargeBehavior rechargeBehav = new RechargeBehavior();
        public RBRechargeBehavior()
          : base("RechargeBehavior", "RB-Recharge",
              "Makes sure agents keep track over their battery life and recharge when needed.",
              "Thesis", "ABS")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Recharge Behavior", "Recharge", "Recharge Behavior", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, rechargeBehav);
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
