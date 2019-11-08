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

        public Random rndGenerator;
        public int Id;
        public int FaceId;
        private Point3d position;
        public Point3d LastPosition;
        protected Point3d startPosition;
        protected Vector3d Force;

        public Vector3d Velocity;
        public GoalState Goal;
        public bool hasResource;
        public int Reach;
        public int PerceptionRange;
        public int ResourceId;
        public NurbsCurve Trajectory;
        public double BatteryLife;

        private const double EnergyExpenditure = 0.1;

        public Point3d Position { get => position;
            set
            {
                LastPosition = Position;
                position = value;
            }
        }

        public BuilderAgent(int _id, int _reach, int _perceptionRange, List<BehaviorBase> _behaviors)
        {
            // get faceID from position
            // AgentSystem property gets filled by agent system, leave alone for now
            
           //Position = startPosition;
            Id = _id;
            rndGenerator = new Random(Id);
            Reach = _reach;
            PerceptionRange = _perceptionRange;
            Behaviors = _behaviors;
            Goal = GoalState.ACQUISITION;
            BatteryLife = 100;
        }

        public override void Reset()
        {
            Position = this.startPosition;
            LastPosition = Position;
            Force = Vector3d.Zero;
            Goal = GoalState.ACQUISITION;
            ResourceId = -1;
            BatteryLife = 100;
            Behaviors.Clear();
        }

        public override void PreExecute()
        {

        }

        public override void Execute()
        {
            foreach (BehaviorBase behavior in this.Behaviors)
            {
                behavior.Execute((AgentBase)this);
            }
            //CalculateNextPosition(this.Force);
        }

        public override void PostExecute()
        {
            BatteryLife -= EnergyExpenditure;
            this.Force.Unitize();
        }

        public override List<object> GetDisplayGeometries()
        {
            return new List<object>()
            {
                (object) this.Position,
                (object) this.LastPosition,
                (object) new Line(Position, LastPosition),
                (object) this.Trajectory
            };
        }

        public void AddForce(Vector3d force)
        {
            this.Force += force;
        }

        public void FindStartingPosition()
        {
            //choose one of the preconstructed faces
            BuilderMeshEnvironment env = ((BuilderAgentSystem)this.AgentSystem).BuilderEnvironment;
            Mesh meshRef = env.Mesh;
            this.FaceId = env.ConstructedFaces.First();
            Point3d tempPos  = meshRef.Faces.GetFaceCenter(this.FaceId);
            this.startPosition = tempPos;
            this.Position = tempPos;
        }

        
        public void CalculateNextPosition(Vector3d finalVector)
        {   
            // later change mesh to only constructed faces
            Mesh meshRef = ((BuilderAgentSystem)this.AgentSystem).BuilderEnvironment.Mesh;
            meshRef.Faces.GetFace(FaceId);
            // change to all adjacent faces or better all faces in reach
            int[] adjacentFacesIndexes = meshRef.Faces.AdjacentFaces(this.FaceId);

            LastPosition = Position;
            Point3d currentPosition = this.Position;
            double minAngle = double.MaxValue;
            int newFaceId = 0; 
            for (int i = 0; i < adjacentFacesIndexes.Length; i ++)
            {
                int id = adjacentFacesIndexes[i];
                Point3d tempNewPosition = meshRef.Faces.GetFaceCenter(id);
                Vector3d tempVector = new Vector3d( tempNewPosition.X - currentPosition.X, tempNewPosition.Y - currentPosition.Y, tempNewPosition.Z - currentPosition.Z);
                double tempAngle = Vector3d.VectorAngle(finalVector, tempVector);
                if ( tempAngle < minAngle )
                {
                    minAngle = tempAngle;
                    newFaceId = id;
                }
                this.FaceId = newFaceId;
                this.Position = meshRef.Faces.GetFaceCenter(newFaceId);
            }
            
        }


    }

}
