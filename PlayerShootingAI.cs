using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySampleAssets.CrossPlatformInput;

namespace CompleteProject
{
    public class PlayerShootingAI : MonoBehaviour
    {
        public int damagePerShot = 20;                  // The damage inflicted by each bullet.
        public float timeBetweenBullets = 0.15f;        // The time between each shot.
        public float range = 100f;                      // The distance the gun can fire.

        //Shooting stuff
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
        bool AbleShooting = false;

        [SerializeField]
        GameObject Player;
        Vector3 TargetEnemy;

        [SerializeField]
        float AISizeDetection = 7.5f;

        [SerializeField]
        float UpdateTick = 0.25f;

        void Awake()
        {
            // Create a layer mask for the Shootable layer.
            shootableMask = LayerMask.GetMask("Shootable");

            // Set up the references.
            gunParticles = GetComponent<ParticleSystem>();
            gunLine = GetComponent<LineRenderer>();
            gunAudio = GetComponent<AudioSource>();
            gunLight = GetComponent<Light>();
            //faceLight = GetComponentInChildren<Light> ();

            StartCoroutine(StartShooting());
        }


        void Update()
        {
            // Add the time since Update was last called to the timer.
            timer += Time.deltaTime;

            if (AbleShooting && timer >= timeBetweenBullets && Time.timeScale != 0)
            {
                // AI look to the Monster
                Player.transform.LookAt(TargetEnemy);

                // ... shoot the gun.
                Shoot();
            }

            // If the timer has exceeded the proportion of timeBetweenBullets that the effects should be displayed for...
            if (timer >= timeBetweenBullets * effectsDisplayTime)
            {
                // ... disable the effects.
                DisableEffects();
            }
        }

        IEnumerator StartShooting()
        {
            while (true)
            {
                List<GameObject> Enemies = EnemiesList();

                if (Enemies.Count == 0)
                    AbleShooting = false;
                else
                {
                    float MinDistance = 10000;
                    foreach (GameObject Enemy in Enemies)
                    {
                        float Distance = Vector3.Distance(this.transform.position, Enemy.transform.position);
                        if (Distance < MinDistance)
                        {
                            MinDistance = Distance;
                            TargetEnemy = Enemy.transform.position;
                        }
                    }
                    AbleShooting = true;
                }
                yield return new WaitForSeconds(UpdateTick);
            }
        }

        List<GameObject> EnemiesList()
        {
            Collider[] EnemiesCollider = Physics.OverlapSphere(this.transform.position, AISizeDetection);
            //we only need the eneimes with Enemy Tag..
            List<GameObject> Enemies = new List<GameObject>();

            foreach (Collider C in EnemiesCollider)
                if (C.tag == "Enemy")
                    Enemies.Add(C.gameObject);

            return Enemies;
        }

        public void DisableEffects()
        {
            // Disable the line renderer and the light.
            gunLine.enabled = false;
            faceLight.enabled = false;
            gunLight.enabled = false;
        }


        void Shoot()
        {
            // Reset the timer.
            timer = 0f;

            // Play the gun shot audioclip.
            gunAudio.Play();

            // Enable the lights.
            gunLight.enabled = true;
            faceLight.enabled = true;

            // Stop the particles from playing if they were, then start the particles.
            gunParticles.Stop();
            gunParticles.Play();

            // Enable the line renderer and set it's first position to be the end of the gun.
            gunLine.enabled = true;
            gunLine.SetPosition(0, transform.position);

            // Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
            shootRay.origin = transform.position;
            shootRay.direction = transform.forward;

            // Perform the raycast against gameobjects on the shootable layer and if it hits something...
            if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
            {
                // Try and find an EnemyHealth script on the gameobject hit.
                EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

                // If the EnemyHealth component exist...
                if (enemyHealth != null)
                {
                    // ... the enemy should take damage.
                    enemyHealth.TakeDamage(damagePerShot, shootHit.point);
                }

                // Set the second position of the line renderer to the point the raycast hit.
                gunLine.SetPosition(1, shootHit.point);
            }
            // If the raycast didn't hit anything on the shootable layer...
            else
            {
                // ... set the second position of the line renderer to the fullest extent of the gun's range.
                gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
            }
        }
    }
}