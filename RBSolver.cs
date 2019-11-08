using System;
using System.Collections;
using System.Collections.Generic;
using ABS.RoboticBuilderABS;
using Grasshopper.Kernel;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Geometry;
using System;
using System.Diagnostics;
using System.Timers;

namespace ABS
{
    public class RBSolver : GH_Component
    {
        private List<AgentSystemBase> iAgentSystems = new List<AgentSystemBase>();
        private BuilderSolver solver;
        private bool executeStep = false;
        private int TimeStep = 2000;

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
            pManager.AddIntegerParameter("Timestep", "T", "Iteration Rate", GH_ParamAccess.item, 2000);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Display Geometries", "G", "Display Geometries", GH_ParamAccess.list);
            pManager.AddGenericParameter("All Agent Systems", "AS", "All Agent Systems", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Iteration Count", "I", "Iteration Count", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool _reset = false;
            DA.GetData<bool>("Reset", ref _reset);
            this.iAgentSystems = new List<AgentSystemBase>();
            DA.GetDataList<AgentSystemBase>("Agent Systems", this.iAgentSystems);

            int _timestep = 2000;
            DA.GetData<int>("Timestep", ref _timestep);
            TimeStep = _timestep;

            
            if (_reset || this.solver == null)
            {
                foreach (AgentSystemBase iAgentSystem in this.iAgentSystems)
                    iAgentSystem.Reset();
                this.solver = new BuilderSolver(this.iAgentSystems);
            }
            else
            {
                this.solver.AgentSystems = this.iAgentSystems;
                bool _execute = false;
                DA.GetData<bool>("Execute", ref _execute);
                if (_execute)
                {
                    this.solver.ExecuteSingleStep();
                    GH_Document doc = OnPingDocument();
                    if (doc != null) doc.ScheduleSolution(TimeStep, ScheduleCallback);
                }
            }

            DA.SetDataList("Display Geometries", (IEnumerable)this.solver.GetDisplayGeometries());
            DA.SetDataList("All Agent Systems", (IEnumerable)this.solver.AgentSystems);
            DA.SetData("Iteration Count", (object)this.solver.IterationCount);
        }

        private void ScheduleCallback(GH_Document document)
        {
            ExpireSolution(false);
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
