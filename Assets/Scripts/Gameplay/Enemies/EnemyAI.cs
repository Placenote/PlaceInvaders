using GameplayNs;
using System.Collections;
using UnityEngine;
using PlayerNs;

namespace EnemiesNs
{
    [RequireComponent (typeof (EnemyState))]
    public class EnemyAI : Photon.PunBehaviour
    {
        public enum StageId
        {
            JustCreated,
            FlyAway,
            TargetSelection,
            Attack
        }

        public float FullHealth = 1f;
        public StageId CurrentStage = StageId.JustCreated;

        [Header ("Setup Attack")]
        public float MeleeDamageDistance = 0.5f;

        [Range (0, 1)]
        public float AttackPower = 0.1f;
        public float AttackTimeLimit = 10f;

        [Header ("Setup Rotating to player")]
        [Tooltip ("Degrees per second")]
        public float RotationSpeed;

        [Header ("Setup approach to player")]
        public float ClosestDist = 0.5f;
        public float StartDecelerationDist = 1.5f;

        public AnimationCurve NormalizedDecelerationCurve;

        [Tooltip ("Usual speed of enemy")]
        public float FarFromPlayerSpeed = 0.75f;

        [Tooltip ("Speed of enemy at deceleration point and closer")]
        public float CloseToPlayerSpeed = 1f;

        // External class reference
        public FlyAwayMoving FlyAway;
        public EnemyState EnemyData;

        PlayerController _targetPlayer = null;
        public PlayerController TargetPlayer
        {
            get
            {
                if (_targetPlayer == null)
                    _targetPlayer = GameController.GetRandomPlayer ();
                return _targetPlayer;
            }
        }

        void Start ()
        {
            EnemyData = GetComponent<EnemyState> ();
            EnemyData.CurrentHealth = FullHealth;

            FlyAway = GetComponent<FlyAwayMoving> ();
            if (FlyAway == null)
                FlyAway = gameObject.AddComponent<FlyAwayMoving> ();

            if (transform.parent == null)
                transform.parent = GameController.WorldRootObject.transform;

            CurrentStage = StageId.JustCreated;
        }



        [Header ("Public for debug only")]
        public float MovingSpeed;
        public float Distance;
        public Quaternion NewRotation;
        public Vector3 Delta;

        void DoActivity (float deltaTime)
        {
            if (GameController.Data.GameState != GameStateId.GamePlaying)
                return;

            switch (CurrentStage)
            {
                case StageId.JustCreated:
                    SwitchStage (StageId.FlyAway);
                    break;

                case StageId.FlyAway:
                    if (!FlyAway.IsRunning)
                        SwitchStage (StageId.TargetSelection);
                    break;

                case StageId.Attack:
                    if (TargetPlayer != null)
                    {
                        if (TargetPlayer.IsDead)
                        {
                            SwitchStage (StageId.FlyAway);
                            break;
                        }

                        Delta = TargetPlayer.transform.position - transform.position;
                        Distance = Delta.magnitude;

                        // Rotation calculation.
                        Quaternion desiredRotation = Quaternion.LookRotation (Delta);
                        NewRotation = Quaternion.RotateTowards (transform.rotation, desiredRotation, RotationSpeed * Time.deltaTime);

                        // Movement speed calculation.
                        MovingSpeed = FarFromPlayerSpeed;
                        if (Distance < ClosestDist)
                            MovingSpeed = 0;
                        else if (Distance < StartDecelerationDist)
                            MovingSpeed = CloseToPlayerSpeed * NormalizedDecelerationCurve.Evaluate (Mathf.Abs ((Distance - ClosestDist) / (StartDecelerationDist - ClosestDist)));

                        // Move and rotate towards player.
                        transform.position = transform.position + deltaTime * MovingSpeed * Delta;
                        transform.rotation = NewRotation;

                        // Damage player if within damage distance.
                        if (Distance < MeleeDamageDistance)
                        {
                            TargetPlayer.Damage (AttackPower);
                        }
                    }
                    break;
            }
        }

        void Update ()
        {
            DoActivity (Time.deltaTime);
        }

        void SwitchStage (StageId newstage)
        {
            switch (newstage)
            {
                case StageId.FlyAway:
                    CurrentStage = newstage;
                    FlyAway.Restart (transform);
                    break;

                case StageId.TargetSelection:
                    CurrentStage = newstage;
                    StartCoroutine (FindPlayerToAttack ());
                    break;

                case StageId.Attack:
                    CurrentStage = newstage;
                    StartCoroutine (AttackTimer ());
                    break;
            }
        }

        IEnumerator FindPlayerToAttack ()
        {
            yield return null;
            PlayerController player = null;
            int timeout = 0;
            do
            {
                yield return new WaitForSeconds (0.25f);
                player = GameController.GetRandomPlayer ();
                timeout++;
            }
            while ((player == null || player.IsDead) && timeout < 15);

            if (timeout >= 15)
            {
                Debug.Log ("============ Player for attack was NOT found. ============");
                Debug.Log ("============ Operation Timed Out... ============");
                SwitchStage (StageId.FlyAway);
            }
            else
            {
                Debug.Log ("============ Player for attack was found ============");
                _targetPlayer = player;
                SwitchStage (StageId.Attack);
            }
        }

        /// <summary>
        /// Allows the attacking state to run for AttackTimeLimit seconds.
        /// Then it sets the state to FlyAway.
        /// </summary>
        bool IsAttackTimeoutRunning;
        IEnumerator AttackTimer ()
        {
            if (IsAttackTimeoutRunning)
                yield break;
            IsAttackTimeoutRunning = true;
            yield return new WaitForSeconds (AttackTimeLimit);
            if (CurrentStage == StageId.Attack)
                SwitchStage (StageId.FlyAway);

            IsAttackTimeoutRunning = false;
        }
    }
}