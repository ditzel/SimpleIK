using UnityEditor;
using UnityEngine;

namespace DitzelGames.FastIK
{
    /// <summary>
    /// Fabrik IK Solver
    /// </summary>
    public class FastIKFabric : MonoBehaviour
    {
        /// <summary>
        /// Chain length of bones
        /// </summary>
        public int ChainLength = 2;

        /// <summary>
        /// Target the chain should bent to
        /// </summary>
        public Transform Target;
        public Transform Pole;

        /// <summary>
        /// Solver iterations per update
        /// </summary>
        [Header("Solver Parameters")]
        public int Iterations = 10;

        /// <summary>
        /// Distance when the solver stops
        /// </summary>
        public float Delta = 0.001f;

        /// <summary>
        /// Strength of going back to the start position.
        /// </summary>
        [Range(0,1)]
        public float SnapBackStrength = 1f;


        protected float[] BonesLength; //Target to Origin
        protected float CompleteLength;
        protected Transform[] Bones;
        protected Vector3[] Positions;
        protected Vector3[] StartDirectionSucc;
        protected Quaternion[] StartRotationBone;
        protected Quaternion StartRotationTarget;
        protected Quaternion StartRotationRoot;


        // Start is called before the first frame update
        void Awake()
        {
            Init();
        }

        void Init()
        {
            //initial array
            Bones = new Transform[ChainLength + 1];
            Positions = new Vector3[ChainLength + 1];
            BonesLength = new float[ChainLength];
            StartDirectionSucc = new Vector3[ChainLength + 1];
            StartRotationBone = new Quaternion[ChainLength + 1];

            //init fields
            if (Target == null)
            {
                Target = new GameObject(gameObject.name + " Target").transform;
                Target.position = transform.position;
            }
            StartRotationTarget = Target.rotation;
            CompleteLength = 0;

            //init data
            var current = transform;
            for (var i = Bones.Length - 1; i >= 0; i--)
            {
                Bones[i] = current;
                StartRotationBone[i] = current.rotation;

                if (i == Bones.Length - 1)
                {
                    //leaf
                    StartDirectionSucc[i] = Target.position - current.position;
                }
                else
                {
                    //mid bone
                    StartDirectionSucc[i] = Bones[i + 1].position - current.position;
                    BonesLength[i] = StartDirectionSucc[i].magnitude;
                    CompleteLength += BonesLength[i];
                }

                current = current.parent;
            }

            if (Bones[0] == null)
                throw new UnityException("The chain value is longer than the ancestor chain!");

            StartRotationRoot = (Bones[0].parent != null) ? Bones[0].parent.rotation : Quaternion.identity;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            ResolveIK();
        }

        private void ResolveIK()
        {
            if (Target == null)
                return;

            if (BonesLength.Length != ChainLength)
                Init();

            //Fabric

            //  (bone0) (bonelen 0) (bone1) (bonelen 1) (bone2)...
            //   x--------------------x--------------------x---...

            //get position
            for (int i = 0; i < Bones.Length; i++)
                Positions[i] = Bones[i].position;

            var RootRot = (Bones[0].parent != null) ? Bones[0].parent.rotation : Quaternion.identity;
            var RootRotDiff = RootRot * Quaternion.Inverse(StartRotationRoot);

            //1st is possible to reach?
            if ((Target.position - Bones[0].position).sqrMagnitude >= CompleteLength * CompleteLength)
            {
                //just strech it
                var direction = (Target.position - Positions[0]).normalized;
                //set everything after root
                for (int i = 1; i < Positions.Length; i++)
                    Positions[i] = Positions[i - 1] + direction * BonesLength[i - 1];
            }
            else
            {
                for (int i = 0; i < Positions.Length - 1; i++)
                    Positions[i + 1] = Vector3.Lerp(Positions[i + 1], Positions[i] + RootRotDiff * StartDirectionSucc[i], SnapBackStrength);

                for (int iteration = 0; iteration < Iterations; iteration++)
                {
                    //https://www.youtube.com/watch?v=UNoX65PRehA
                    //back
                    for (int i = Positions.Length - 1; i > 0; i--)
                    {
                        if (i == Positions.Length - 1)
                            Positions[i] = Target.position; //set it to target
                        else
                            Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * BonesLength[i]; //set in line on distance
                    }

                    //forward
                    for (int i = 1; i < Positions.Length; i++)
                        Positions[i] = Positions[i - 1] + (Positions[i] - Positions[i - 1]).normalized * BonesLength[i - 1];

                    //close enough?
                    if ((Positions[Positions.Length - 1] - Target.position).sqrMagnitude < Delta * Delta)
                        break;
                }
            }

            //move towards pole
            if (Pole != null)
            {
                for (int i = 1; i < Positions.Length - 1; i++)
                {

                    var plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);
                    var projectedPole = plane.ClosestPointOnPlane(Pole.position);
                    var projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                    var angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1], plane.normal);
                    Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) + Positions[i - 1];
                }
            }

            //set position & rotation
            for (int i = 0; i < Positions.Length; i++)
            {
                if (i == Positions.Length - 1)
                    Bones[i].rotation = Target.rotation * Quaternion.Inverse(StartRotationTarget) * StartRotationBone[i];
                else
                    Bones[i].rotation = Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i + 1] - Positions[i]) * StartRotationBone[i];
                Bones[i].position = Positions[i];
            }
        }

        void OnDrawGizmos()
        {
            var current = this.transform;
            for (int i = 0; i < ChainLength && current != null && current.parent != null; i++)
            {
                var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
                Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
                Handles.color = Color.green;
                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                current = current.parent;
            }
        }


    }
}