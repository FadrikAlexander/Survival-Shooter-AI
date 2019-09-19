using UnityEngine;
using System.Collections;
using System;
using UnityEngine.AI;
using UnitySampleAssets.CrossPlatformInput;

namespace CompleteProject
{
    public class PlayerMovement : MonoBehaviour
    {
        public float speed = 6f;            // The speed that the player will move at.

        Vector3 movement;                   // The vector to store the direction of the player's movement.
        Animator anim;                      // Reference to the animator component.
        Rigidbody playerRigidbody;          // Reference to the player's rigidbody.
        int floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
        float camRayLength = 100f;          // The length of the ray from the camera into the scene.


        //AI Stuff
        NavMeshAgent nav;
        float[,] PlacesCost;
        Vector3[,] PosCost;

       
        void Awake ()
        {
            // Create a layer mask for the floor layer.
            floorMask = LayerMask.GetMask ("Floor");
            // Set up references.
            anim = GetComponent <Animator> ();
            playerRigidbody = GetComponent <Rigidbody> ();
            nav = GetComponent<NavMeshAgent>();
            nav.updateRotation = false;
            PlacesCost = new float[5, 5];
            PosCost = new Vector3[5, 5];
            clearCosts();
            StartCoroutine(startMovement());

        }

        void clearCosts()
        {
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    PlacesCost[i, j] = 0 + Vector3.Distance(new Vector3(0, 0, 0), this.transform.position);
                }
            for (int ii = 0, i = -4;ii < 5 && i < 5; ii++, i +=2)
                for (int jj = 0, j = -4; jj < 5 && j < 5; jj++, j +=2)
                {
                    PosCost[ii, jj] = new Vector3(this.transform.position.x + i, this.transform.position.y, this.transform.position.z + j);
                }
        }

        void PrintCosts()
        {
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    Debug.Log(PlacesCost[i, j]);
                }
        }

        void SetCosts()
        {
            Collider[] Col = Physics.OverlapSphere(this.transform.position, 7.5f);
            bool AbleMove = false ;

            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    foreach (Collider c in Col)
                    {
                        switch (c.tag)
                        {
                            case "EnemyMove": AbleMove = true;
                                PlacesCost[i, j] -= Vector3.Distance(PosCost[i, j], c.transform.position);
                            break;
                           /* case "Border": PlacesCost[i, j] += 2*Vector3.Distance(PosCost[i, j], c.transform.position);
                            break;
                            case "Cover": PlacesCost[i, j] += Vector3.Distance(PosCost[i, j], c.transform.position);
                            break;*/
                        }
                    }
                }
            if (AbleMove)
            {
                float num = PlacesCost[0, 0];
                Vector3 MovePos = Vector3.one;

                for (int i = 0; i < 5; i++)
                    for (int j = 0; j < 5; j++)
                    {
                        if (PlacesCost[i, j] < num)
                        {
                            num = PlacesCost[i, j];
                            MovePos = PosCost[i, j];
                        }
                    }
                Debug.Log(MovePos + " " + num);
                nav.SetDestination(MovePos);
            }
            else
                nav.SetDestination(new Vector3(0, 0, 0));
        }

        IEnumerator startMovement()
        {
            while (true)
            {
                clearCosts();
                SetCosts();
                yield return new WaitForSeconds(0.25f);
            }
        }


        void FixedUpdate()
        {
            /*float h = CrossPlatformInputManager.GetAxisRaw("Horizontal");
            float v = CrossPlatformInputManager.GetAxisRaw("Vertical");
            Move (h, v);*/

            Animating (nav.isStopped);
        }
        


        void Move (float h, float v)
        {
            // Set the movement vector based on the axis input.
            movement.Set (h, 0f, v);
            
            // Normalise the movement vector and make it proportional to the speed per second.
            movement = movement.normalized * speed * Time.deltaTime;

            // Move the player to it's current position plus the movement.
            playerRigidbody.MovePosition (transform.position + movement);
        }
        void Animating (bool walking)
        {
            anim.SetBool ("IsWalking", walking);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(this.transform.position, 7.5f);
            float penaltyMin = float.MaxValue;
            float penaltyMax = float.MinValue;
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    if (PlacesCost[i, j] > penaltyMax)
                    {
                        penaltyMax = PlacesCost[i, j];
                    }
                    if (PlacesCost[i, j] < penaltyMin)
                    {
                        penaltyMin = PlacesCost[i, j];
                    }
                }
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    Gizmos.color = Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(penaltyMin, penaltyMax, PlacesCost[i, j]));
                    Gizmos.DrawCube(PosCost[i, j], Vector3.one);
                }
        }
    }
}