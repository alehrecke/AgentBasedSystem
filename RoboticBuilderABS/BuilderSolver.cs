
using ICD.AbmFramework.Core.AgentSystem;
using System.Collections.Generic;

namespace ABS.RoboticBuilderABS
{
    public class BuilderSolver
    {
        public List<AgentSystemBase> AgentSystems;
        public int IterationCount;

        public BuilderSolver()
        {
            this.AgentSystems = new List<AgentSystemBase>();
        }

        public BuilderSolver(List<AgentSystemBase> agentSystems)
        {
            this.AgentSystems = agentSystems;
        }

        public void ExecuteSingleStep()
        {
            ++this.IterationCount;
            foreach (AgentSystemBase agentSystem in this.AgentSystems)
            {
                if (!agentSystem.IsFinished())
                    agentSystem.PreExecute();
            }
            foreach (AgentSystemBase agentSystem in this.AgentSystems)
            {
                if (!agentSystem.IsFinished())
                    agentSystem.Execute();
            }
            foreach (AgentSystemBase agentSystem in this.AgentSystems)
            {
                if (!agentSystem.IsFinished())
                    agentSystem.PostExecute();
            }
        }

        public void Execute(int maxIterationCount)
        {
            for (int index = 0; index < maxIterationCount; ++index)
            {
                this.ExecuteSingleStep();
                bool flag = true;
                foreach (AgentSystemBase agentSystem in this.AgentSystems)
                {
                    if (!agentSystem.IsFinished())
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    break;
            }
        }

        public List<object> GetDisplayGeometries()
        {
            List<object> objectList = new List<object>();
            foreach (AgentSystemBase agentSystem in this.AgentSystems)
            {
                objectList.AddRange((IEnumerable<object>)agentSystem.GetDisplayGeometries());
                objectList.AddRange(((BuilderAgentSystem)agentSystem).BuilderEnvironment.GetDisplayGeometry());
            }
            return objectList;
        }
    }
}
