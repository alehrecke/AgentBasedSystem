using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public class RandomDirectionBehaviour : BehaviorBase
    { 
        public RandomDirectionBehaviour(double weight)
        {
            this.Weight = weight;
        }
        public override void Execute(AgentBase agent)
        {
            BuilderAgent agent1 = (BuilderAgent)agent;
            
            Random random = new Random(DateTime.Now.Millisecond + agent1.Id);
            double x = random.Next(-10,10);
            double y = random.Next(-10,10);
            double z = random.Next(-10,10);

            Vector3d finalVector = new Vector3d(x/10, y/10, z/10) * this.Weight;
            agent1.AddForce(finalVector);
        }
    }

    public class RechargeBehavior : BehaviorBase
    {
        public RechargeBehavior()
        {

        }

        public override void Execute(AgentBase agent)
        {
            BuilderAgent builderAgent = (BuilderAgent)agent;
            BuilderAgentSystem builderAgentSystem = agent.AgentSystem as BuilderAgentSystem;
            BuilderMeshEnvironment env = builderAgentSystem.BuilderEnvironment;

            List<int> otherAgentsPosition = new List<int>();
            foreach (BuilderAgent a in builderAgentSystem.Agents)
            {
                otherAgentsPosition.Add(a.FaceId);
            }

            if (builderAgent.BatteryLife <= 20)
            {
                builderAgent.Goal = BuilderAgent.GoalState.CHARGING;
            }

            if (builderAgent.Goal != BuilderAgent.GoalState.CHARGING)
                return;
            
            if (env.ChargingLocations.Contains(builderAgent.FaceId))
            {
                if (builderAgent.BatteryLife >= 100)
                {
                    builderAgent.Goal = BuilderAgent.GoalState.ACQUISITION;
                    return;
                } 
                builderAgent.BatteryLife += 10;
            }
            else
            {
                //  Get neighbor faces
                int[] neighbors = env.Mesh.Faces.AdjacentFaces(builderAgent.FaceId);
                //  Cull un-built faces
                HashSet<int> builtNeighborFaces = new HashSet<int>();
                foreach (int x in neighbors)
                {
                    if (env.ConstructedFaces.Contains(x))
                    {
                        builtNeighborFaces.Add(x);
                    }
                }
                //  Check which one has highest pheromone

                int faceWithHighestPheromone = -1;
                double pheromoneLevel = 0;
                foreach (int y in builtNeighborFaces)
                {
                    if (env.ChargingLocationPheromones[y] > pheromoneLevel)
                    {
                        pheromoneLevel = env.ChargingLocationPheromones[y];
                        faceWithHighestPheromone = y;
                    }
                }

                //  If no faces have pheromones choose one randomly
                if (faceWithHighestPheromone == -1)
                {
                    int random = builderAgent.rndGenerator.Next(0, builtNeighborFaces.Count);
                    if (otherAgentsPosition.Contains(builtNeighborFaces.ElementAt(random)))
                        return;
                    else
                    {
                        builderAgent.Position = env.Mesh.Faces.GetFaceCenter(builtNeighborFaces.ElementAt(random));
                        builderAgent.FaceId = builtNeighborFaces.ElementAt(random);
                    }
                }
                else
                {
                    if (otherAgentsPosition.Contains(faceWithHighestPheromone))
                        return;
                    else
                    {
                        builderAgent.Position = env.Mesh.Faces.GetFaceCenter(faceWithHighestPheromone);
                        builderAgent.FaceId = faceWithHighestPheromone;
                    }
                }
            }
            builderAgent.DropPheromones();

        }
    }
        
    public class ResourceAcquisitionBehavior : BehaviorBase
    {
        public ResourceAcquisitionBehavior()
        {

        }
        public override void Execute(AgentBase agent)
        {
            BuilderAgent builderAgent = (BuilderAgent)agent;
            BuilderAgentSystem builderAgentSystem = agent.AgentSystem as BuilderAgentSystem;
            BuilderMeshEnvironment env = builderAgentSystem.BuilderEnvironment;

            List<int> otherAgentsPosition = new List<int>();
            foreach (BuilderAgent a in builderAgentSystem.Agents)
            {
                otherAgentsPosition.Add(a.FaceId);
            }

            if (builderAgent.Goal != BuilderAgent.GoalState.ACQUISITION)
            {
                return;
            }
            

            // If current mesh face has resource then change goal state
            if (env.ResourceLocations.Contains(builderAgent.FaceId))
            {
                builderAgent.hasResource = true;
                builderAgent.Goal = BuilderAgent.GoalState.DELIVERY;
            }
            else
            {

                //  Get neighbor faces
                int[] neighbors = env.Mesh.Faces.AdjacentFaces(builderAgent.FaceId);
                //  Cull un-built faces
                HashSet<int> builtNeighborFaces = new HashSet<int>();
                foreach (int x in neighbors)
                {
                    if (env.ConstructedFaces.Contains(x))
                    {
                        builtNeighborFaces.Add(x);
                    }
                }
                //  Check which one has highest pheromone

                int faceWithHighestPheromone = -1;
                double pheromoneLevel = 0;
                foreach (int y in builtNeighborFaces)
                {
                    if (env.ResourcePheromones[y] > pheromoneLevel)
                    {
                        pheromoneLevel = env.ResourcePheromones[y];
                        faceWithHighestPheromone = y;
                    }
                }

                //  If no faces have pheromones choose one randomly
                if (faceWithHighestPheromone == -1)
                {
                    int random = builderAgent.rndGenerator.Next(0, builtNeighborFaces.Count);
                    if (otherAgentsPosition.Contains(builtNeighborFaces.ElementAt(random)))
                        return;
                    else
                    {
                        builderAgent.Position = env.Mesh.Faces.GetFaceCenter(builtNeighborFaces.ElementAt(random));
                        builderAgent.FaceId = builtNeighborFaces.ElementAt(random);
                    }
                }
                else
                {
                    if (otherAgentsPosition.Contains(faceWithHighestPheromone))
                        return;
                    else
                    {
                        builderAgent.Position = env.Mesh.Faces.GetFaceCenter(faceWithHighestPheromone);
                        builderAgent.FaceId = faceWithHighestPheromone;
                    }
                }
            }
            builderAgent.DropPheromones();
        }
    }

    public class ResourceDeliveryBehavior : BehaviorBase
    {
        public ResourceDeliveryBehavior()
        {

        }
        public override void Execute(AgentBase agent)
        {
            BuilderAgent builderAgent = (BuilderAgent)agent;
            BuilderAgentSystem builderAgentSystem = agent.AgentSystem as BuilderAgentSystem;
            BuilderMeshEnvironment env = builderAgentSystem.BuilderEnvironment;

            List<int> otherAgentsPosition = new List<int>();
            foreach (BuilderAgent a in builderAgentSystem.Agents)
            {
                otherAgentsPosition.Add(a.FaceId);
            }

            if (builderAgent.Goal != BuilderAgent.GoalState.DELIVERY)
            {
                return;
            }

            //  Get neighbor faces
            int[] neighbors = env.Mesh.Faces.AdjacentFaces(builderAgent.FaceId);
            //  Find unbuilt faces
            HashSet<int> builtNeighborFaces = new HashSet<int>();
            HashSet<int> unbuiltNeighborFaces = new HashSet<int>();
            foreach (int x in neighbors)
            {
                if (env.ConstructedFaces.Contains(x))
                {
                    builtNeighborFaces.Add(x);
                }
                else
                {
                    unbuiltNeighborFaces.Add(x);
                }
            }

            if (unbuiltNeighborFaces.Count > 0)
            {
                
                // choose one of them and build it (add face id to built faces)
                // prefer faces with smaller z value!!!! Still gotta implement this
                env.ConstructedFaces.Add(unbuiltNeighborFaces.First());
                builderAgent.Position = env.Mesh.Faces.GetFaceCenter(unbuiltNeighborFaces.First());
                builderAgent.FaceId = unbuiltNeighborFaces.First();
                
                // tell agent to look for resource again
                builderAgent.Goal = BuilderAgent.GoalState.ACQUISITION;

            }
            else
            {
                int faceWithHighestPheromone = -1;
                double pheromoneLevel = 0;
                foreach (int y in builtNeighborFaces)
                {
                    if (env.BuildLocationPheromones[y] > pheromoneLevel)
                    {
                        pheromoneLevel = env.ResourcePheromones[y];
                        faceWithHighestPheromone = y;
                    }
                }

                if (faceWithHighestPheromone == -1)
                {
                    int random = builderAgent.rndGenerator.Next(0, builtNeighborFaces.Count);
                    if (otherAgentsPosition.Contains(builtNeighborFaces.ElementAt(random)))
                        return;
                    else
                    {
                        builderAgent.Position = env.Mesh.Faces.GetFaceCenter(builtNeighborFaces.ElementAt(random));
                        builderAgent.FaceId = builtNeighborFaces.ElementAt(random);
                    }                    
                }
                else
                {
                    if (otherAgentsPosition.Contains(faceWithHighestPheromone))
                        return;
                    else
                    {
                        builderAgent.Position = env.Mesh.Faces.GetFaceCenter(faceWithHighestPheromone);
                        builderAgent.FaceId = faceWithHighestPheromone;
                    }
                }
            }
            builderAgent.DropPheromones();

        }
    }

    
}
