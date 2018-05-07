using UnityEngine;

namespace EnemiesNs
{

    public class FlyAwayMoving : MonoBehaviour
    {
        public float MovingSpeed = 2;

        public float RotatingSpeed = 15;
        public float TimeToWork = 3;
        public Transform someObject;

        [Header("Public for debug purposes only")]
        public bool dbgRestart;
        public bool IsRunning;
        public float WorkedTime = float.PositiveInfinity;
        public Vector3 DefaultDirection = Vector3.one;
        public Vector3 RandomDirection = Vector3.one;
        public PhotonView photonView;

        public void Restart(Transform newObjectToFly)
        {
            WorkedTime = 0;
            someObject = newObjectToFly;
            if (newObjectToFly == null)
                return;
            photonView = PhotonView.Get(someObject);
            RandomDirection = Quaternion.Euler(0, UnityEngine.Random.Range(-179f, 179f), 0) * DefaultDirection;
            Move(0);
        }


        void Move(float TimeDeltaTime)
        {
            if (WorkedTime <= TimeToWork && someObject != null)
                WorkedTime = WorkedTime + TimeDeltaTime;
            else
            {
                WorkedTime = float.PositiveInfinity;
                IsRunning = false;
                return;
            }
       
            IsRunning = true;
            Quaternion targetRotation = Quaternion.LookRotation
                (
                    new Vector3(someObject.forward.x+RandomDirection.x,
                    RandomDirection.y*(1- Mathf.Lerp(0,1, WorkedTime / TimeToWork) ),
                    someObject.forward.z+ RandomDirection.z) 
                );

            targetRotation = Quaternion.RotateTowards(someObject.rotation, targetRotation, RotatingSpeed * TimeDeltaTime);
            someObject.rotation = targetRotation;
            someObject.position = someObject.position + TimeDeltaTime * (targetRotation * someObject.forward);

        }



        bool IsManagedHere()
        {
            return ((photonView != null && photonView.isMine) || PhotonNetwork.offlineMode || !PhotonNetwork.connected);
        }


        private void Update()
        {
            if (dbgRestart)
            {
                dbgRestart = false;
                if (someObject == null)
                    Restart(transform);
                else
                    Restart(someObject);
                return;

            }
            if (IsRunning && IsManagedHere())
            {
                Move(Time.deltaTime);
                
            }
        }
    }
}
