using System;
using System.Collections;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

// <Custom "using" statements>

using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;

using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using ICD.AbmFramework.Core;
using ICD.AbmFramework.Core.Utilities;
using ICD.AbmFramework.Core.Environments;
using ICD.AbmFramework.Core.Behavior;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using Rhino.Geometry.Intersect;


// </Custom "using" statements>


#region padding (this ensures the line number of this file match with those in the code editor of the C# Script component
















#endregion

public partial class CustomAgent : GH_ScriptInstance
{
    #region Do_not_modify_this_region

    private void Print(string text)
    {
    }

    private void Print(string format, params object[] args)
    {
    }

    private void Reflect(object obj)
    {
    }

    private void Reflect(object obj, string methodName)
    {
    }

    public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs,
        IGH_DataAccess DA)
    {
    }

    public RhinoDoc RhinoDocument;
    public GH_Document GrasshopperDocument;
    public IGH_Component Component;
    public int Iteration;

    #endregion


    private void RunScript(Point3d iStartPosition, int iFaceIndex, List<object> iBehaviors, ref object oAgents)
    {
        // <Custom code>
        List<BehaviorBase> behaviors = new List<BehaviorBase>();
        foreach (BehaviorBase behavior in iBehaviors)
        {
            behaviors.Add(behavior);
        }

        //oAgents = new BuilderAgent();

        // </Custom code>
    }

    // <Custom additional code>
    
    // Notes
    // Make sure that behaviors are not interdependent, so we can turn them on/off as needed for debugging and optimization

    //  Agent (NKK)
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

        public GoalState Goal;
        public Point3d GoalPosition;
        public int Reach;
        public int PerceptionRange;
        public int ResourceId;
        public Curve Trajectory;
        public double BatteryLife;

        private const double EnergyExpenditure = 0.1;

        // super usefull comment
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

    // Agent System
    public class BuilderAgentSystem : AgentSystemBase
    {
        public double TimeStep = 0.02;
        public double MaxSpeed = 2.0;
        public double MaxForce = 3.0;
        public double DisplacementThreshold = -1.0;
        public double TotalDisplacement = double.MaxValue;
        public BuilderMeshEnvironment BuilderEnvironment;

        protected BuilderAgentSystem()
        {
            this.Agents = new List<AgentBase>();
        }

        public override void Reset()
        {
            base.Reset();
            this.TotalDisplacement = double.MaxValue;
        }

        public BuilderAgentSystem(List<LocomotionAgent> agents)
        {
            this.Agents = new List<AgentBase>();
            foreach (AgentBase agent in agents)
            {
                this.Agents.Add(agent);
                agent.AgentSystem = (AgentSystemBase)this;
            }
        }

        public override void Execute()
        {
            this.TotalDisplacement = 0.0;
            foreach (AgentBase agent in this.Agents)
                agent.Execute();
        }

        public override List<object> GetDisplayGeometries()
        {
            List<object> objectList = new List<object>();
            foreach (LocomotionAgent agent in this.Agents)
                objectList.AddRange((IEnumerable<object>)agent.GetDisplayGeometries());
            return objectList;
        }

        public List<GH_Point> GetAgentPositionsAsGhPoints()
        {
            List<GH_Point> ghPointList = new List<GH_Point>();
            foreach (LocomotionAgent agent in this.Agents)
                ghPointList.Add(new GH_Point(agent.Position));
            return ghPointList;
        }

        public List<GH_Vector> GetAgentVelocitiesAsGhVectors()
        {
            List<GH_Vector> ghVectorList = new List<GH_Vector>();
            foreach (LocomotionAgent agent in this.Agents)
                ghVectorList.Add(new GH_Vector(agent.Velocity));
            return ghVectorList;
        }

        public List<BuilderAgent> FindNeighbors(BuilderAgent agent, double distance)
        {
            List<BuilderAgent> locomotionAgentList = new List<BuilderAgent>();
            foreach (BuilderAgent agent1 in this.Agents)
            {
                if (agent != agent1 && agent.Position.DistanceTo(agent1.Position) < distance)
                    locomotionAgentList.Add(agent1);
            }
            return locomotionAgentList;
        }

        public override bool IsFinished()
        {
            return this.TotalDisplacement < this.DisplacementThreshold;
        }
    }

    // Environment
    public class BuilderMeshEnvironment : SurfaceEnvironment
    {
        public Mesh Mesh;
        public List<Vector3d> VectorField;
        public List<Point3d> ResourceLocations;
        public List<Point3d> ConstructionLocations;
        public List<Point3d> ChargingLocations;


        public BuilderMeshEnvironment(Mesh mesh)
        {
            this.Mesh = mesh;
        }

        public void SetMesh(Mesh mesh)
        {
            this.Mesh = mesh;
        }

        public override List<object> GetDisplayGeometry()
        {
            return new List<object>() { (object)this.Mesh };
        }

        public override Point3d ClosestPoint(Point3d position)
        {
            return this.Mesh.ClosestPoint(position);
        }

        public override Plane SurfacePlane(Point3d position)
        {
            Point3d pointOnMesh;
            Vector3d normalAtPoint;
            if (this.Mesh.ClosestPoint(position, out pointOnMesh, out normalAtPoint, 0.0) != -1)
                return new Plane(pointOnMesh, normalAtPoint);
            return Plane.Unset;
        }

        public override Vector3d GetNormal(Point3d position)
        {
            return this.SurfacePlane(position).Normal;
        }

        public override Point3d IntersectionPoint(Line line)
        {
            Ray3d ray = new Ray3d(line.From, line.Direction);
            List<GeometryBase> geometryBaseList = new List<GeometryBase>();
            geometryBaseList.Add((GeometryBase)this.Mesh);
            int maxReflections = 1;
            Point3d[] point3dArray = Intersection.RayShoot(ray, (IEnumerable<GeometryBase>)geometryBaseList, maxReflections);
            if (point3dArray == null || point3dArray.Length == 0)
                return Point3d.Unset;
            return point3dArray[0];
        }

        public override List<Curve> BoundaryCurves3D()
        {
            List<Curve> curveList = new List<Curve>();
            foreach (Polyline nakedEdge in this.Mesh.GetNakedEdges())
                curveList.Add((Curve)nakedEdge.ToNurbsCurve());
            return curveList;
        }
    }

    // Behaviors
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
            List<BuilderAgent> neighbors = (agent1.AgentSystem as BuilderAgentSystem).FindNeighbors(agent1, this.Distance);
            Vector3d zero = Vector3d.Zero;
            if (neighbors.Count == 0)
                return;
            foreach (BuilderAgent builderAgent in neighbors)
            {
                Vector3d vector3d1 = agent1.Position - builderAgent.Position;
                double length = vector3d1.Length;
                if (vector3d1.IsZero)
                    vector3d1 = Vector3d.XAxis;
                if (length < this.Distance)
                {
                    vector3d1.Unitize();
                    Vector3d vector3d2 = this.Weight * vector3d1 * Math.Pow((this.Distance - length) / this.Distance, this.Power) * this.Distance;
                    if (this.AffectSelf)
                        zero += vector3d2;
                    else
                        builderAgent.AddForce(-vector3d2);
                }
            }
            agent1.AddForce(zero * this.Weight);
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
            BuilderAgent builderAgent = (BuilderAgent) agent;
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


    
    // </Custom additional code>
}
