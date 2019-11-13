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
                if (builderAgent.BatteryLife > 100) builderAgent.BatteryLife = 100;
            }
            else
            {
                //  Get neighbor faces
                int[] neighbors = env.Mesh.Faces.AdjacentFaces(builderAgent.FaceId);
                //  Cull un-built faces
                HashSet<int> builtNeighborFaces = new HashSet<int>();
                foreach (int x in neighbors)
                {
                    if (env.ConstructedFaces.Contains(x) && !env.OccupiedFaces.Contains(x))
                    {
                        builtNeighborFaces.Add(x);
                    }
                }
                //  Check which one has highest pheromone

                int faceWithHighestPheromone = -1;
                double pheromoneLevel = 0;
                foreach (int y in builtNeighborFaces)
                {
                    if (env.ChargingPheromones[y] > pheromoneLevel)
                    {
                        pheromoneLevel = env.ChargingPheromones[y];
                        faceWithHighestPheromone = y;
                    }
                }

                //  If no faces have pheromones choose one randomly
                int randoBeh = builderAgent.rndGenerator.Next(0, 3);
                if (faceWithHighestPheromone == -1 || randoBeh == 1)
                {
                    int random = builderAgent.rndGenerator.Next(0, builtNeighborFaces.Count);

                        builderAgent.Position = env.Mesh.Faces.GetFaceCenter(builtNeighborFaces.ElementAt(random));
                        builderAgent.FaceId = builtNeighborFaces.ElementAt(random);
                }
                else
                {

                        builderAgent.Position = env.Mesh.Faces.GetFaceCenter(faceWithHighestPheromone);
                        builderAgent.FaceId = faceWithHighestPheromone;
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


            if (builderAgent.Goal != BuilderAgent.GoalState.ACQUISITION)
            {
                return;
            }
            

            // If current mesh face has resource then change goal state
            if (env.ResourceLocations.Contains(builderAgent.FaceId))
            {
                builderAgent.hasResource = true;                
                builderAgent.Goal = BuilderAgent.GoalState.DELIVERY;
                env.ResourcePheromones[builderAgent.FaceId] += builderAgent.PheromoneCount * builderAgent.dropProportion;
                Debug.WriteLine("Just dropped: " + env.ResourcePheromones[builderAgent.FaceId]);
            }
            else
            {

                //  Get neighbor faces
                int[] neighbors = env.Mesh.Faces.AdjacentFaces(builderAgent.FaceId);
                //  Cull un-built faces
                HashSet<int> builtNeighborFaces = new HashSet<int>();
                foreach (int x in neighbors)
                {
                    if (env.ConstructedFaces.Contains(x) /*&& !env.OccupiedFaces.Contains(x)*/)
                    {
                        builtNeighborFaces.Add(x);
                    }
                }
                // Remove last position from builtNeighbors
                if (builtNeighborFaces.Count > 1)
                {
                    builtNeighborFaces.Remove(builderAgent.LastFaceId);
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
                int randoBeh = builderAgent.rndGenerator.Next(0, 3);
                if (faceWithHighestPheromone == -1 || randoBeh == 1)
                {
                    if (builtNeighborFaces.Count > 0)
                    {
                        int random = builderAgent.rndGenerator.Next(0, builtNeighborFaces.Count);
                        builderAgent.Position = env.Mesh.Faces.GetFaceCenter(builtNeighborFaces.ElementAt(random));
                        builderAgent.FaceId = builtNeighborFaces.ElementAt(random);
                    }
                        
                }
                else
                {
                        builderAgent.Position = env.Mesh.Faces.GetFaceCenter(faceWithHighestPheromone);
                        builderAgent.FaceId = faceWithHighestPheromone;
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
                if (env.ConstructedFaces.Contains(x) /*&& !env.OccupiedFaces.Contains(x)*/)
                {
                    builtNeighborFaces.Add(x);
                }
                else
                {
                    unbuiltNeighborFaces.Add(x);
                }
            }
            // Remove last position from builtNeighbors
            if (builtNeighborFaces.Count > 1)
            {
                builtNeighborFaces.Remove(builderAgent.LastFaceId);
            }

            if (unbuiltNeighborFaces.Count > 0)
            {
                // choose one of them and build it (add face id to built faces)
                // prefer faces with smaller z value!!!! Still gotta implement this
                int x = builderAgent.rndGenerator.Next(0, unbuiltNeighborFaces.Count);
                env.ConstructedFaces.Add(unbuiltNeighborFaces.ElementAt(x));
                builderAgent.Position = env.Mesh.Faces.GetFaceCenter(unbuiltNeighborFaces.ElementAt(x));
                builderAgent.FaceId = unbuiltNeighborFaces.ElementAt(x);
                
                // tell agent to look for resource again
                builderAgent.Goal = BuilderAgent.GoalState.ACQUISITION;
                env.DeliveryPheromones[builderAgent.FaceId] += builderAgent.PheromoneCount * builderAgent.dropProportion;
            }
            else
            {
                // Look for neighbor with highest pheromone
                int faceWithHighestPheromone = -1;
                double pheromoneLevel = 0;

                foreach (int y in builtNeighborFaces)
                {
                    if (env.DeliveryPheromones[y] > pheromoneLevel)
                    {
                        pheromoneLevel = env.ResourcePheromones[y];
                        faceWithHighestPheromone = y;
                    }
                }
                // if no neighbors have pheromone or random returns true
                int randoBeh = builderAgent.rndGenerator.Next(0, 3);
                if (faceWithHighestPheromone == -1 || randoBeh == 1)
                {
                    int random = builderAgent.rndGenerator.Next(0, builtNeighborFaces.Count);
                    builderAgent.Position = env.Mesh.Faces.GetFaceCenter(builtNeighborFaces.ElementAt(random));
                    builderAgent.FaceId = builtNeighborFaces.ElementAt(random);                
                }
                else
                {
                // Move to face with highest pheromone
                    builderAgent.Position = env.Mesh.Faces.GetFaceCenter(faceWithHighestPheromone);
                    builderAgent.FaceId = faceWithHighestPheromone;
                }
            }
            builderAgent.DropPheromones();

        }
    }

    
}
