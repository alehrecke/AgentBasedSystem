using System;
using System.Collections.Generic;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Geometry;

namespace ABS
{
    public class RBSystem : GH_Component
    {
        private List<BuilderAgent> localAgents = new List<BuilderAgent>();
        private BuilderAgentSystem agentSystem;
        private bool firstRun = true;

        public RBSystem()
          : base("RoboticBuilderSystem", "RB-Sys",
              "System taking care of agents, behaviors, and environment",
              "Thesis", "ABS")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("RB-Agents", "RB-Agents", "RB-Agents", GH_ParamAccess.list);
            pManager.AddGenericParameter("RB-Env", "RB-Env", "RB-Env", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("RB-Sys", "RB-Sys", "RB-Sys", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BuilderMeshEnvironment builderMeshEnv = (BuilderMeshEnvironment)null;
            List<BuilderAgent> tempAgents = new List<BuilderAgent>();

            DA.GetData<BuilderMeshEnvironment>("RB-Env", ref builderMeshEnv);
            DA.GetDataList<BuilderAgent>("RB-Agents", tempAgents);

            bool reload = false;
            if (localAgents.Count != tempAgents.Count)
            {
                reload = true;
            }
            else
            {
                for (int index = 0; index < localAgents.Count; ++index)
                {
                    if (this.localAgents[index] != tempAgents[index])
                    {
                        reload = true;
                        break;
                    }
                }
            }
            localAgents = tempAgents;
            if (reload)
            {
                agentSystem = new BuilderAgentSystem(localAgents, builderMeshEnv);
            }
            agentSystem.BuilderEnvironment = builderMeshEnv;

            DA.SetData("RB-Sys", (object)this.agentSystem);
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
            get { return new Guid("a38943df-e362-4707-a5a8-48d517255923"); }
        }
    }
}
