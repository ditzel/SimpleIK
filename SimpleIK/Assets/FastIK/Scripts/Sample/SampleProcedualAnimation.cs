using UnityEngine;

namespace DitzelGames.FastIK
{
    class SampleProcedualAnimation :  MonoBehaviour
    {
        public Transform[] FootTarget;
        public Transform LookTarget;
        public Transform HandTarget;
        public Transform HandPole;
        public Transform Step;
        public Transform Attraction;

        public void LateUpdate()
        {
            //move step & attraction
            Step.Translate(Vector3.forward * Time.deltaTime * 0.7f);
            if (Step.position.z > 1f)
                Step.position = Step.position + Vector3.forward * -2f;
            Attraction.Translate(Vector3.forward * Time.deltaTime * 0.5f);
            if (Attraction.position.z > 1f)
                Attraction.position = Attraction.position + Vector3.forward * -2f;

            //footsteps
            for(int i = 0; i < FootTarget.Length; i++)
            {
                var foot = FootTarget[i];
                var ray = new Ray(foot.transform.position + Vector3.up * 0.5f, Vector3.down);
                var hitInfo = new RaycastHit();
                if(Physics.SphereCast(ray, 0.05f, out hitInfo, 0.50f))
                    foot.position = hitInfo.point + Vector3.up * 0.05f;
            }

            //hand and look
            var normDist = Mathf.Clamp((Vector3.Distance(LookTarget.position, Attraction.position) - 0.3f) / 1f, 0, 1);
            HandTarget.rotation = Quaternion.Lerp(Quaternion.Euler(90, 0, 0), HandTarget.rotation, normDist);
            HandTarget.position = Vector3.Lerp(Attraction.position, HandTarget.position, normDist);
            HandPole.position = Vector3.Lerp(HandTarget.position + Vector3.down * 2, HandTarget.position + Vector3.forward * 2f, normDist);
            LookTarget.position = Vector3.Lerp(Attraction.position, LookTarget.position, normDist);


        }

    }
}
