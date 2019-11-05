using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using ICD.AbmFramework.Core.Behavior;
using System.Linq;
using ABS.RoboticBuilderABS;

namespace ABS
{
    public class RBAgent : GH_Component
    {

        public RBAgent()
          : base("RoboticBuilderAgent", "RB-Agent",
              "Robotic Builder Agents",
              "Thesis", "ABS")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("iAgentCount", "", "", GH_ParamAccess.item, 1 );
            pManager.AddIntegerParameter("iReach", "", "", GH_ParamAccess.item, 5);
            pManager.AddGenericParameter("iBehaviors", "", "", GH_ParamAccess.list);
            pManager[2].Optional = true;
        }
            
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("oAgents", "oAgents", "Here comes the agents", GH_ParamAccess.list);
            
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int _agentCount = 1;
            int _reach = 1;
            List<BehaviorBase> _behaviors = new List<BehaviorBase>();
            List<BuilderAgent> Agents = new List<BuilderAgent>();
            


            if (!DA.GetData("iAgentCount", ref _agentCount)) return;
            if (!DA.GetDataList<BehaviorBase>("iBehaviors", _behaviors)) return;
            if (!DA.GetData("iReach", ref _reach)) return;

            for ( int i = 0; i < _agentCount; i++)
            {
                BuilderAgent agent = new BuilderAgent(_reach, 3 * _reach, _behaviors);
                Agents.Add(agent);
            }

            DA.SetDataList("oAgents", Agents);
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
            get { return new Guid("a38943df-e362-4707-a5a8-48d517255924"); }
        }
    }
}
