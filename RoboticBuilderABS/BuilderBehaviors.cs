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
using Rhino.Geometry.Collections;


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
                //Look for goal and set goal using builderMeshEnvironment List of Resources and choosing the closest one
                // Generate trajectory

                GenerateTrajectory(builderAgent, new Point3d(0,0,0), new Point3d(0,0,0));
            }
        }

        class Node
        {
            public Point3d Location;
            public double HCost;
            public double GCost;
            public Node Parent;
            public int FaceId;
            public bool isValidForLocomotion;

            public Node(Point3d location, int faceId)
            {
                Location = location;
                FaceId = faceId;
                isValidForLocomotion = true;
            }
            public double FCost => GCost + HCost;
        }

        private Node NodeFromWorldPoint(Point3d worldPoint, int faceId)
        {
            // Actually implement a way to find closest mesh face
            Node tempNode = new Node(worldPoint, faceId);
            return tempNode;
        }

        private List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>();
            
            return neighbors;
        }

        private double GetDistance(Node nodeA, Node nodeB)
        {
            return 0;
        }

        private void RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
        }

        private void GenerateTrajectory(BuilderAgent agent, Point3d startPosition, Point3d targetPosition)
        {
           

            BuilderAgentSystem builderAgentSystem = agent.AgentSystem as BuilderAgentSystem;
            BuilderMeshEnvironment env = builderAgentSystem.BuilderEnvironment;
            Mesh refMesh = env.Mesh;


            // A* Algorithm

           
            // Temporary!
            List<Point3d> meshVertices = new List<Point3d>();
            MeshFace mF = refMesh.Faces[10];
            meshVertices.Add(refMesh.Vertices[mF[0]]);
            meshVertices.Add(refMesh.Vertices[mF[1]]);
            meshVertices.Add(refMesh.Vertices[mF[2]]);
            if (mF.IsQuad) meshVertices.Add(refMesh.Vertices[mF[3]]);
            Point3d centroid = new Point3d();
            foreach (Point3d pt in meshVertices)
            {
                centroid += pt;
            }
            
            Node targetNode = NodeFromWorldPoint(centroid/meshVertices.Count, 10);
            Node startNode = NodeFromWorldPoint(agent.Position, agent.FaceId);

            List<Node> OpenSet = new List<Node>();
            HashSet<Node> ClosedSet = new HashSet<Node>();

            OpenSet.Add(startNode);

            while (OpenSet.Count > 0)
            {
                Node currentNode = OpenSet[0];
                for (int i = 1; i < OpenSet.Count; i++)
                {
                    if (OpenSet[i].FCost < currentNode.FCost || OpenSet[i].FCost == currentNode.FCost && OpenSet[i].HCost < currentNode.HCost)
                    {
                        currentNode = OpenSet[i];
                    }
                }

                OpenSet.Remove(currentNode);
                ClosedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return;
                }

                foreach (Node neighborNode in GetNeighbors(currentNode))
                {
                    if (!neighborNode.isValidForLocomotion || ClosedSet.Contains(neighborNode))
                    {
                        continue;
                    }

                    double newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighborNode);
                    if (newMovementCostToNeighbor < neighborNode.GCost || !OpenSet.Contains(neighborNode))
                    {
                        neighborNode.GCost = newMovementCostToNeighbor;
                        neighborNode.HCost = GetDistance(neighborNode, targetNode);
                        neighborNode.Parent = currentNode;

                        if (!OpenSet.Contains(neighborNode))
                        {
                            OpenSet.Add(neighborNode);
                        }
                    }
                }
            }
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
