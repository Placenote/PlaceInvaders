using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerNs;
using System;

namespace EnemiesNs
{



    [RequireComponent(typeof(EnemyState))]
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

        [Header("Setup Attack")]
        public float MeleeDamageDistance = 2;

        [Range(0, 1)]
        public float AttackPower = 0.1f;
        public float AttackTimeLimit = 10f;

        [Header("Setup Rotating to player")]
        [Tooltip("Degrees per second")]
        public float RotationSpeed;

        [Header("Setup approach to player")]
        public float ClosestDist = 0.5f;
        public float StartDecelerationDist = 1.5f;

        public AnimationCurve NormalizedDecelerationCurve;

        [Tooltip("Usual speed of enemy")]
        public float FarFromPlayerSpeed = 2f;

        [Tooltip("Speed of enemy at deceleration point and closer")]
        public float CloseToPlayerSpeed = 0.75f;

        public FlyAwayMoving FlyAway;

        public EnemyState EnemyData;

        // Use this for initialization
        void Start()
        {
            EnemyData = GetComponent<EnemyState>();
            EnemyData.CurrentHealth = FullHealth;
            CurrentStage = StageId.JustCreated;

            if (transform.parent == null)
                transform.parent = GameController.WorldRootObject.transform;

            FlyAway = GetComponent<FlyAwayMoving>();
            if (FlyAway == null)
                FlyAway = gameObject.AddComponent<FlyAwayMoving>();
            

        }




        bool DoOnce = true;
        public Vector3 velocity;
        public float timer;
        // Update is called once per frame


        PlayerController _targetPlayer = null;
        public PlayerController TargetPlayer
        {
            get
            {
                if (_targetPlayer == null)
                    _targetPlayer = GameController.GetRandomPlayer();
                return _targetPlayer;
            }
        }

        bool IsAlreadyWaitingPlayer = false;
        IEnumerator WaitPlayer()
        {
            if (IsAlreadyWaitingPlayer)
            {
                //SwitchStage(StageId.Spreading);
                yield break;
            }
            yield return null;
            PlayerController player = null;
            do
            {
                // Debug.Log("2");

                yield return new WaitForSeconds(1);
                // Debug.Log("3");

                player = GameController.GetRandomPlayer();
            }
            while (player == null || player.IsDead);
            Debug.Log("============ Player for attcak is  found  ============");


            _targetPlayer = player;
            IsAlreadyWaitingPlayer = false;
            SwitchStage(StageId.Attack);

        }


        void SwitchStage(StageId newstage)
        {
            switch (newstage)
            {
                case StageId.FlyAway:
                    CurrentStage = newstage;
                    FlyAway.Restart(transform);

                    break;

                case StageId.TargetSelection:
                    // Debug.Log("1");

                    CurrentStage = newstage;
                    StartCoroutine(WaitPlayer());
                    break;


                case StageId.Attack:
                    CurrentStage = newstage;
                    StartCoroutine(AttackTimer());
                    break;



            }
        }





        bool IsAttackTimeoutRunning;
        IEnumerator AttackTimer()
        {
            if (IsAttackTimeoutRunning)
                yield break;
            IsAttackTimeoutRunning = true;
            yield return new WaitForSeconds(AttackTimeLimit);
            if (CurrentStage == StageId.Attack)
                SwitchStage(StageId.FlyAway);

            IsAttackTimeoutRunning = false;
        }

        [Header("Public for debug only")]
        public float MovingSpeed;
        public float Distance;
        public Quaternion NewRotation;
        public Vector3 Delta;

        void DoActivity(float deltaTime)
        {
            if (GameController.Data.GameState != GameStateId.GamePlaying)
                return;

            switch (CurrentStage)
            {
                case StageId.JustCreated:
                    SwitchStage(StageId.FlyAway);
                    break;

                case StageId.FlyAway:
                    if(!FlyAway.IsRunning)
                        SwitchStage(StageId.TargetSelection);
                    break;

                case StageId.Attack:
                    if (TargetPlayer != null)
                    {

                        if (TargetPlayer.IsDead)
                        {
                            SwitchStage(StageId.FlyAway);
                            break;
                        }
                       
                        Delta = TargetPlayer.transform.position - transform.position;
                       
                        Distance = Delta.magnitude;
                        Quaternion desiredRotation = Quaternion.LookRotation(Delta);
                         NewRotation =   Quaternion.RotateTowards(transform.rotation,desiredRotation,  RotationSpeed * Time.deltaTime);

                        float NewForwardMagnitude = Quaternion.Dot(NewRotation , transform.rotation);
                        if (NewForwardMagnitude < 0)
                            NewForwardMagnitude = 0;

                        MovingSpeed = FarFromPlayerSpeed;
                        if (Distance < ClosestDist)
                            MovingSpeed = 0;
                        else if (Distance < StartDecelerationDist)
                            MovingSpeed = CloseToPlayerSpeed * NormalizedDecelerationCurve.Evaluate( Mathf.Abs((Distance - ClosestDist) / (StartDecelerationDist - ClosestDist)) );

                        transform.position = transform.position +   deltaTime * MovingSpeed * Quaternion.Dot(NewRotation, transform.rotation)* Delta;
                        transform.rotation = NewRotation;


                        if (Distance < MeleeDamageDistance)
                        {
                            
                            TargetPlayer.Damage(AttackPower);
                        }

                    }

                    break;

            }

            
        }


        void Update()
        {
            if(photonView.isMine || !PhotonNetwork.connected)
                DoActivity(Time.deltaTime);
        }


    }
}
