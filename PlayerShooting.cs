using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnitySampleAssets.CrossPlatformInput;

namespace CompleteProject
{
    public class PlayerShooting : MonoBehaviour
    {
        public int damagePerShot = 20;                  // The damage inflicted by each bullet.
        public float timeBetweenBullets = 0.15f;        // The time between each shot.
        public float range = 100f;                      // The distance the gun can fire.


        float timer;                                    // A timer to determine when to fire.
        Ray shootRay = new Ray();                       // A ray from the gun end forwards.
        RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
        int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.
        ParticleSystem gunParticles;                    // Reference to the particle system.
        LineRenderer gunLine;                           // Reference to the line renderer.
        AudioSource gunAudio;                           // Reference to the audio source.
        Light gunLight;                                 // Reference to the light component.
		public Light faceLight;								// Duh
        float effectsDisplayTime = 0.2f;                // The proportion of the timeBetweenBullets that the effects will display for.

        //AI Stuff
        public GameObject Player;
        GameObject mon;
        Vector3 MonsterPos;
        bool AbleShooting= false;
        List<GameObject> Monsters = new List<GameObject>();

        void Awake ()
        {
            shootableMask = LayerMask.GetMask ("Shootable");

            gunParticles = GetComponent<ParticleSystem> ();
            gunLine = GetComponent <LineRenderer> ();
            gunAudio = GetComponent<AudioSource> ();
            gunLight = GetComponent<Light> ();

            StartCoroutine(StartShooting());
        }

        void Update ()
        {
            timer += Time.deltaTime;

            if (AbleShooting && timer >= timeBetweenBullets && Time.timeScale != 0)
            {
                Player.transform.LookAt(MonsterPos);
                Shoot ();
            }
       
            if(timer >= timeBetweenBullets * effectsDisplayTime)
            {
                DisableEffects ();
            }
        }

        IEnumerator StartShooting()
        {
            while (true)
            {
                if (Monsters.Count == 0)
                {
                    AbleShooting = false;
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    float min = 100000;
                    foreach (GameObject g in Monsters)
                    {
                        if(g!=null)
                        {
                            float minn = Vector3.Distance(this.transform.position, g.transform.position);
                            if (minn < min)
                            {
                                min = minn;
                                mon = g;
                                MonsterPos = g.transform.position;
                            }
                        }
                    }
                    AbleShooting = true;
                    yield return new WaitForSeconds(1f);
                }

            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Enemy")
                Monsters.Add(other.gameObject);

            StopAllCoroutines();
            StartCoroutine(StartShooting());
        }

        void OnTriggerExit(Collider other)
        {
            if (other.tag == "Enemy")
                Monsters.Remove(other.gameObject);

            if (Monsters.Count == 0)
            {
                AbleShooting = false;
                StopAllCoroutines();
            }
        }
            


        public void DisableEffects ()
        {
            gunLine.enabled = false;
			faceLight.enabled = false;
            gunLight.enabled = false;
        }
        void Shoot ()
        {
            // Reset the timer.
            timer = 0f;

            // Play the gun shot audioclip.
            gunAudio.Play ();

            // Enable the lights.
            gunLight.enabled = true;
			faceLight.enabled = true;

            // Stop the particles from playing if they were, then start the particles.
            gunParticles.Stop ();
            gunParticles.Play ();

            // Enable the line renderer and set it's first position to be the end of the gun.
            gunLine.enabled = true;
            gunLine.SetPosition (0, transform.position);

            // Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
            shootRay.origin = transform.position;
            shootRay.direction = transform.forward;

            // Perform the raycast against gameobjects on the shootable layer and if it hits something...
            if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
            {
                // Try and find an EnemyHealth script on the gameobject hit.
                EnemyHealth enemyHealth = shootHit.collider.GetComponent <EnemyHealth> ();

                // If the EnemyHealth component exist...
                if(enemyHealth != null)
                {
                    // ... the enemy should take damage.
                    if (enemyHealth.TakeDamage(damagePerShot, shootHit.point))
                    {
                        AbleShooting = false;
                        Monsters.Remove(mon.gameObject);
                        MonsterPos.y = 0;
                        StopAllCoroutines();
                        StartCoroutine(StartShooting());
                    }
                }

                // Set the second position of the line renderer to the point the raycast hit.
                gunLine.SetPosition (1, shootHit.point);
            }
            // If the raycast didn't hit anything on the shootable layer...
            else
            {
                // ... set the second position of the line renderer to the fullest extent of the gun's range.
                gunLine.SetPosition (1, shootRay.origin + shootRay.direction * range);
            }
        }
    }
}