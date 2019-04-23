using UnityEngine;
/*
public class IKCCD : MonoBehaviour
{
    public int ChainLength = 2;
    public Transform Target;
    protected Quaternion TargetInitialRotation;
    protected Quaternion EndInitialRotation;
    public Transform Pole;
    protected float CompleteLength;

    public int Iterations = 10;
    public float Delta = 0.001f;

    protected Transform[] Bones;
    //protected Quaternion[] InitialRotation;


    // Start is called before the first frame update
    void Awake()
    {
        //initial length
        Bones = new Transform[ChainLength + 1];
        //InitialRotation = new Quaternion[ChainLength + 1];
        TargetInitialRotation = Target.rotation;
        EndInitialRotation = transform.rotation;

        var current = transform;
        CompleteLength = 0;
        for (int i = ChainLength - 1; i >= 0; i--)
        {
            CompleteLength += (current.position - current.parent.position).magnitude;
            Bones[i + 1] = current;
            Bones[i] = current.parent;
            //InitialRotation[i + 1] = current.rotation;
            //InitialRotation[i] = current.parent.rotation;
            current = current.parent;
        }
        if (Bones[0] == null)
            throw new UnityException("The chain value is longer than the ancestor chain!");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //CCD
        var lastBone = Bones[Bones.Length - 1];

        //for (var i = 0; i < Bones.Length; i++)
        //  Bones[i].rotation = InitialRotation[i];

        for (int iteration = 0; iteration < Iterations; iteration++)
        {
            for (var i = Bones.Length - 1; i >= 0; i--)
            {
                //https://www.youtube.com/watch?v=MA1nT9RAF3k

                if (i == Bones.Length - 1)
                {
                    Bones[i].rotation = Target.rotation * Quaternion.Inverse(TargetInitialRotation) * EndInitialRotation;
                }
                else
                {
                    Bones[i].rotation = Quaternion.FromToRotation(lastBone.position - Bones[i].position, Target.position - Bones[i].position) * Bones[i].rotation;

                    //jitter to solve strait line
                    //if (iteration == 5 && i == 0 && (Target.position - lastBone.position).sqrMagnitude > 0.01f && (Target.position - Bones[i].position).sqrMagnitude < CompleteLength * CompleteLength)
                    //    Bones[i].rotation = Quaternion.AngleAxis(10, Vector3.up) * Bones[i].rotation;

                    //move towards pole
                    if (Pole != null && i + 2 <= Bones.Length - 1)
                    {
                        var plane = new Plane(Bones[i + 2].position - Bones[i].position, Bones[i].position);
                        var projectedPole = plane.ClosestPointOnPlane(Pole.position);
                        var projectedBone = plane.ClosestPointOnPlane(Bones[i + 1].position);
                        if ((projectedBone - Bones[i].position).sqrMagnitude > 0.01f)
                        {
                            var angle = Vector3.SignedAngle(projectedBone - Bones[i].position, projectedPole - Bones[i].position, plane.normal);
                            Bones[i].rotation = Quaternion.AngleAxis(angle, plane.normal) * Bones[i].rotation;
                        }
                    }
                }


                //close enough?
                if ((lastBone.position - Target.position).sqrMagnitude < Delta * Delta)
                    break;
            }
        }

    }
    

}
*/