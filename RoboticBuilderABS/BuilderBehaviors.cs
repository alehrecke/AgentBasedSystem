using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICD.AbmFramework.Core;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.Behavior;


namespace ABS.RoboticBuilderABS
{

    public class SeparationBehavior : BehaviorBase
    {
        public double Distance;
        public double Power;
        public bool AffectSelf;
        public SeparationBehavior(double weight, double distance, double power, bool affectSelf)
        {
            this.Weight = weight;
            this.Distance = distance;
            this.Power = power;
            this.AffectSelf = affectSelf;
        }
        public override void Execute(AgentBase agent)
        {
            BuilderAgent agent1 = (BuilderAgent)agent;
            
            Random random = new Random();
            double x = random.Next(-1, 1);
            double y = random.Next(-1, 1);
            double z = random.Next(-1, 1);

            Vector3d finalVector = new Vector3d(x, y, z);

            // change in position is applied here, Please calculate correct finalVector
            agent1.AddForce(finalVector);

            //agent1.Position += new Vector3d( x, y, z);

            //List<BuilderAgent> neighbors = (agent1.AgentSystem as BuilderAgentSystem).FindNeighbors(agent1, this.Distance);
            //Vector3d zero = Vector3d.Zero;
            //if (neighbors.Count == 0)
            //    return;
            //foreach (BuilderAgent builderAgent in neighbors)
            //{
            //    Vector3d vector3d1 = agent1.Position - builderAgent.Position;
            //    double length = vector3d1.Length;
            //    if (vector3d1.IsZero)
            //        vector3d1 = Vector3d.XAxis;
            //    if (length < this.Distance)
            //    {
            //        vector3d1.Unitize();
            //        Vector3d vector3d2 = this.Weight * vector3d1 * Math.Pow((this.Distance - length) / this.Distance, this.Power) * this.Distance;
            //        if (this.AffectSelf)
            //            zero += vector3d2;
            //        else
            //            builderAgent.AddForce(-vector3d2);
            //    }
            //}
            //agent1.AddForce(zero * this.Weight);
        }
    }

    public class CheckBatteryBehavior : BehaviorBase
    {
        public override void Execute(AgentBase agent)
        {
            BuilderAgent builderAgent = (BuilderAgent)agent;
            if (builderAgent.BatteryLife <= 10)
            {
                builderAgent.Goal = BuilderAgent.GoalState.CHARGING;
                // interrupt! drop resource and run to the closest battery charging station
            }
        }
    }

    public class LookForGoalBehavior : BehaviorBase
    {
        public LookForGoalBehavior()
        {
        }
        public override void Execute(AgentBase agent)
        {
            BuilderAgent builderAgent = (BuilderAgent)agent;
            if (builderAgent.Goal == BuilderAgent.GoalState.NOT_SET)
            {
                BuilderAgentSystem builderAgentSystem = builderAgent.AgentSystem as BuilderAgentSystem;
                BuilderMeshEnvironment builderMeshEnvironment = builderAgentSystem.BuilderEnvironment;
                //Look for goal and set goal using builderMeshEnvironment List of Resources and choosing the closest one
                // Generate trajectory
                GenerateTrajectory();
            }
        }
        private void GenerateTrajectory()
        {

        }
    }

    public class TrajectoryBehavior : BehaviorBase
    {
        public double Power;
        public TrajectoryBehavior(double power)
        {
            this.Power = power;
        }

        public override void Execute(AgentBase agent)
        {
            BuilderAgent builderAgent = (BuilderAgent)agent;
            // look for closest param on trajectory
            // look for param a certain distance away from closest param
            // calculate vector from one to the other
            // add vector + power to the Force
            // agent.AddForce(calculatedVector);
        }
    }

    public class ObstacleAvoidanceBehavior : BehaviorBase
    {
        public ObstacleAvoidanceBehavior()
        {

        }
        public override void Execute(AgentBase agent)
        {
        }
    }

    // Pick up resource if position is equal to resource position and switch Goal State
    public class ResourceAcquisitionBehavior : BehaviorBase
    {
        public ResourceAcquisitionBehavior()
        {

        }
        public override void Execute(AgentBase agent)
        {
        }
    }

    // Deposit resource if position is equal to build site position and switch goal state
    public class ResourceDeliveryBehavior : BehaviorBase
    {
        public ResourceDeliveryBehavior()
        {

        }
        public override void Execute(AgentBase agent)
        {
        }
    }

    public class MappingBehavior : BehaviorBase
    {
        public MappingBehavior()
        {

        }
        public override void Execute(AgentBase agent)
        {
        }
    }

}
