using System;
using System.Collections.Generic;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Geometry;

namespace ABS
{
    public class RBRandVectBehavior : GH_Component
    {
        public RBRandVectBehavior()
          : base("RB-Rand-Vect-Behavior", "RB-RandVector",
              "Causes agents to go in random direction",
              "Thesis", "ABS")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight", "Weight", "Weight", GH_ParamAccess.item, 0.5);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("RandDirBehavior", "Behav", "RandDirBehavior", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            double _weight = 1;
            DA.GetData(0, ref _weight);


            RandomDirectionBehaviour ranDirBehavior = new RandomDirectionBehaviour(_weight);
            DA.SetData("RandDirBehavior", (object)ranDirBehavior);

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
            get { return new Guid("0D52D831-623B-4772-8E38-FD11B6B79F7B"); }
        }
    }
}
