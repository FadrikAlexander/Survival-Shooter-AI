using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CompleteProject
{
    public class PlayerMovementAI : MonoBehaviour
    {
        [SerializeField]
        float UpdateTick = 0.25f;

        [SerializeField]
        float AISizeDetection = 7.5f;


        float[,] AIPositionCosts;
        Vector3[,] AIPositions;

        NavMeshAgent navAgent;
        Animator anim;   

        void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();

            AIPositionCosts = new float[5, 5];
            AIPositions = new Vector3[5, 5];

            GameStarted = true ;
            StartCoroutine(startMovement());
        }

        void Update()
        {
            Animating(navAgent.isStopped);
        }

        bool ShouldMove;

        IEnumerator startMovement()
        {
            while (true)
            {
                ShouldMove = false;

                setPositions();

                calculateCosts();

                if (ShouldMove)
                    navAgent.SetDestination(GetBestPosition());
                else
                    navAgent.SetDestination(Vector3.zero); //go to the middle of the level

                yield return new WaitForSeconds(UpdateTick);
            }
        }

        Vector3 GetBestPosition()
        {
            float Min = AIPositionCosts[0, 0];
            Vector3 MovePos =  AIPositions[0, 0];

            for(int i=0 ; i <5 ; i++)
                for(int j=0 ; j<5 ; j++)
                    if (AIPositionCosts[i, j] < Min)
                    {
                        Min = AIPositionCosts[i, j];
                        MovePos = AIPositions[i, j];
                    }

            return MovePos;
        }

        void calculateCosts()
        {
            //Find all Enemies or the cost modifiers
            Collider[] Enemies = Physics.OverlapSphere(this.transform.position, AISizeDetection);

            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    foreach (Collider Enemy in Enemies)
                        if (Enemy.tag == "Enemy")
                        {
                            ShouldMove = true;
                            AIPositionCosts[i, j] -= Vector3.Distance(AIPositions[i, j], Enemy.transform.position);
                        }
        }

        void setPositions()
        {
            //reset the costs
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    AIPositionCosts[i, j] = 0 + Vector3.Distance(Vector3.zero, this.transform.position);

            //reset the Positions around the player
            for (int i = 0 , x =-4 ; i < 5 && x<5; i++ , x+=2)
                for (int j = 0 , z = -4; j < 5 && z<5; j++, z+=2)
                {
                    AIPositions[i, j] = new Vector3(this.transform.position.x + x, this.transform.position.y, this.transform.position.z + z);
                }
        }

        void Animating(bool walking)
        {
            anim.SetBool("IsWalking", walking);
        }


        bool GameStarted = false;

        void OnDrawGizmos()
        {
            if (GameStarted)
            {
                Gizmos.DrawWireSphere(this.transform.position, AISizeDetection);

                float penaltyMin = float.MaxValue;
                float penaltyMax = float.MinValue;
                for (int i = 0; i < 5; i++)
                    for (int j = 0; j < 5; j++)
                    {
                        if (AIPositionCosts[i, j] > penaltyMax)
                        {
                            penaltyMax = AIPositionCosts[i, j];
                        }
                        if (AIPositionCosts[i, j] < penaltyMin)
                        {
                            penaltyMin = AIPositionCosts[i, j];
                        }
                    }
                for (int i = 0; i < 5; i++)
                    for (int j = 0; j < 5; j++)
                    {
                        Gizmos.color = Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(penaltyMin, penaltyMax, AIPositionCosts[i, j]));
                        Gizmos.DrawCube(AIPositions[i, j], Vector3.one);
                    }
            }
        }
    }
}
