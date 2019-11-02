using System;
using System.Collections;
using System.Collections.Generic;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Geometry;

namespace ABS
{
    public class RBSolver : GH_Component
    {
        private List<AgentSystemBase> iAgentSystems = new List<AgentSystemBase>();
        private BuilderSolver solver;

        public RBSolver()
          : base("RoboticBuilderSolver", "RB-Solver",
              "Does the magic ;)",
              "Thesis", "ABS")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "R", "Reset", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Execute", "E", "Execute", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Agent Systems", "AS", "Agent Systems", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Display Geometries", "G", "Display Geometries", GH_ParamAccess.list);
            pManager.AddGenericParameter("All Agent Systems", "AS", "All Agent Systems", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Iteration Count", "I", "Iteration Count", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool destination1 = false;
            DA.GetData<bool>("Reset", ref destination1);

            this.iAgentSystems = new List<AgentSystemBase>();
            DA.GetDataList<AgentSystemBase>("Agent Systems", this.iAgentSystems);

            if (destination1 || this.solver == null)
            {
                foreach (AgentSystemBase iAgentSystem in this.iAgentSystems)
                    iAgentSystem.Reset();
                this.solver = new BuilderSolver(this.iAgentSystems);
            }
            else
            {
                this.solver.AgentSystems = this.iAgentSystems;
                bool destination2 = false;
                DA.GetData<bool>("Execute", ref destination2);
                if (destination2 && !destination1)
                {
                    this.solver.ExecuteSingleStep();
                    this.ExpireSolution(true);
                }
            }

            DA.SetDataList("Display Geometries", (IEnumerable)this.solver.GetDisplayGeometries());
            DA.SetDataList("All Agent Systems", (IEnumerable)this.solver.AgentSystems);
            DA.SetData("Iteration Count", (object)this.solver.IterationCount);
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
            get { return new Guid("a38943df-e362-4707-a5a8-48d517255928"); }
        }
    }
}
