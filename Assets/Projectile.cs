using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Projectile : MonoBehaviour
{
    public float speed = 5;
    public float damage = 5;
    public float knockBackTime = 0.1f, knockBackVelocity = 50;
    public float maxLifetime = 1, curLifetime = 0;
    public bool realProj = false;
    public bool followOwner = false;
    public bool breaksOnContact = true;

    public Transform ownerTransform;

    public PlayerController pc;


    private List<DefaultEnemy> hitEnemies = new List<DefaultEnemy>();

    private List<ResourceEntity> hitResources = new List<ResourceEntity>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        curLifetime += Time.deltaTime;

        if (curLifetime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        //if (!IsOwner) return;
        if (followOwner)
        {
            transform.position = ownerTransform.position;
        }
        else
        {
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
        }


        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 7;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer


        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, (transform.forward * speed * Time.fixedDeltaTime).magnitude, layerMask) && !followOwner)
        {
            //Debug.DrawRay(transform.position + Vector3.up, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Debug.Log("Proj hit");

            //transform.position = hit.point;

            if (realProj)
            {
                //Do Damage

                if (hit.collider.gameObject.layer == 9)
                {
                    ResourceEntity re =  hit.collider.gameObject.GetComponent<ResourceEntity>();

                    if (!hitResources.Contains(re))
                    {
                        re.TakeDamage(damage);
                        hitResources.Add(re);
                    }
                }

                if (hit.collider.gameObject.layer == 8)
                {
                    DefaultEnemy de = hit.collider.gameObject.GetComponent<DefaultEnemy>();

                    if (!hitEnemies.Contains(de))
                    {
                        var rot = Quaternion.Euler(0, 0, 0);

                        var forward = transform.forward;  // fairly common

                        var result = rot * forward;

                        de.targetPlayer = pc;
                        de.TakeDamage(damage);

                        if (!de.isKnockedBack)
                        {
                            de.TakeKnockBack(result, knockBackTime, knockBackVelocity);
                        }


                        hitEnemies.Add(de);
                    }
                }
            }
            //else
            //{
            //    if (hit.collider.gameObject.layer == 8)
            //    {
            //        DefaultEnemy de = hit.collider.gameObject.GetComponent<DefaultEnemy>();

            //        if (!hitEnemies.Contains(de))
            //        {
            //            var rot = Quaternion.Euler(0, 0, 0);

            //            var forward = transform.forward;  // fairly common

            //            var result = rot * forward;

            //            de.TakeKnockBack(result, knockBackTime, knockBackVelocity);

            //        }
            //    }
            //}

            if (breaksOnContact)
            {
                Destroy(gameObject);
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            DefaultEnemy de = collision.gameObject.gameObject.GetComponent<DefaultEnemy>();

            if (!hitEnemies.Contains(de))
            {
                var rot = Quaternion.Euler(0, 0, 0);

                var forward = transform.forward;  // fairly common

                var result = de.transform.position - (new Vector3(transform.position.x, 0, transform.position.z));

                if (de)
                {
                    if (realProj)
                    {
                        de.TakeDamage(damage);
                        hitEnemies.Add(de);

                        if (!de.isKnockedBack)
                        {
                            de.TakeKnockBack(result.normalized, knockBackTime, knockBackVelocity);
                        }

                        // de.TakeKnockBack(transform.forward, knockBackTime, knockBackVelocity);
                    }


                    de.targetPlayer = pc;
                }

                if (breaksOnContact)
                {
                    Destroy(gameObject);
                }
            }
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            ResourceEntity re = other.gameObject.gameObject.GetComponent<ResourceEntity>();

            if (realProj)
            {
                if (!hitResources.Contains(re))
                {
                    re.TakeDamage(damage);
                    hitResources.Add(re);
                }
            }



            if (breaksOnContact)
            {
                Destroy(gameObject);
            }
        }

        if (other.gameObject.layer == 8)
        {
            DefaultEnemy de = other.gameObject.gameObject.GetComponent<DefaultEnemy>();

            if (!hitEnemies.Contains(de))
            {
                var rot = Quaternion.Euler(0, 0, 0);

                var forward = transform.forward;  // fairly common

                var result =  de.transform.position - (new Vector3(transform.position.x, 0 , transform.position.z)) ;

                if (de)
                {
                    if (realProj)
                    {
                        de.TakeDamage(damage);
                        hitEnemies.Add(de);

                        if (!de.isKnockedBack)
                        {
                            de.TakeKnockBack(result.normalized, knockBackTime, knockBackVelocity);
                        }

                       // de.TakeKnockBack(transform.forward, knockBackTime, knockBackVelocity);
                    }


                    de.targetPlayer = pc;
                }

                if (breaksOnContact)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
