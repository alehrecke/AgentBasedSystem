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
    public class BuilderAgent : AgentBase
    {
        public enum GoalState
        {
            NOT_SET = 0,
            ACQUISITION = 1,
            DELIVERY = 2,
            CHARGING = 3
        }
        public int FaceId;
        public Point3d Position;
        protected Point3d startPosition;
        protected Vector3d Force;

        public Vector3d Velocity;
        public GoalState Goal;
        public Point3d GoalPosition;
        public int Reach;
        public int PerceptionRange;
        public int ResourceId;
        public Curve Trajectory;
        public double BatteryLife;

        private const double EnergyExpenditure = 0.1;

        public BuilderAgent(Point3d _position, int _reach, int _perceptionRange, List<BehaviorBase> behaviors)
        {
            // get faceID from position
            // AgentSystem property gets filled by agent system, leave alone for now
            Position = _position;
            startPosition = Position;
            Reach = _reach;
            PerceptionRange = _perceptionRange;
            Behaviors = behaviors;
            Goal = GoalState.NOT_SET;
            BatteryLife = 100;
        }

        public override void Reset()
        {
            Position = this.startPosition;
            Force = Vector3d.Zero;
            Goal = GoalState.NOT_SET;
            ResourceId = -1;
            GoalPosition = Point3d.Unset;
            BatteryLife = 100;
        }

        public override void PreExecute()
        {
            // Conduct movement based on Force Vector
        }

        public override void Execute()
        {
            foreach (BehaviorBase behavior in this.Behaviors)
                behavior.Execute((AgentBase)this);
        }

        public override void PostExecute()
        {
            BatteryLife -= EnergyExpenditure;
        }

        public override List<object> GetDisplayGeometries()
        {
            return new List<object>()
            {
                (object) this.Position
            };
        }

        public void AddForce(Vector3d force)
        {
            this.Force += force;
        }
    }

}
