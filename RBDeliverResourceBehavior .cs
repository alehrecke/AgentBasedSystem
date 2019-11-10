using System;
using System.Collections.Generic;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Geometry;

namespace ABS
{
    public class RBDeliverResourceBehavior : GH_Component
    {
        ResourceDeliveryBehavior resourceDeliveryBehavior = new ResourceDeliveryBehavior();
        public RBDeliverResourceBehavior()
          : base("RB-Delivery-Behavior", "RB-Delivery",
              "Causes agents to deposit resources",
              "Thesis", "ABS")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("DeliveryBehavior", "Delivery-Behav", "Delivery-Behav", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData("DeliveryBehavior", resourceDeliveryBehavior);
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
            get { return new Guid("62BDFC8C-A93A-4396-A647-96AC080C44EF"); }
        }
    }
}
