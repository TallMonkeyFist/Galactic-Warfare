using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class Projectile : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Rigidbody for moving the projectile")]
    [SerializeField] private Rigidbody rb = null;

    [Header("Projectile Settings")]
    [Tooltip("How fast the projectile will travel")]
    [SerializeField] public float speed = 50.0f;
    [Tooltip("The projectiles life span (seconds)")]
    [SerializeField] public float lifeSpan = 5.0f;
    [Tooltip("The damage that the projectile deals")]
    [SerializeField] public float projetileDamage = 50.0f;
    [Tooltip("Raycast layers to prevent clipping through object")]
    [SerializeField] public LayerMask shootMask = new LayerMask();

    [Header("Explosive")]
    [Tooltip("Does the projectile deal splash damage")]
    [SerializeField] public bool isExplosive = false;
    [Tooltip("Prefab to spawn during explosion")]
    [SerializeField] public GameObject explosionPrefab = null;
    [Tooltip("Outer falloff radius of the explosion")]
    [SerializeField] public float outerFalloffRange = 10.0f;
    [Tooltip("Inner falloff radius of the explosion")]
    [SerializeField] public float innerFalloffRange = 0.0f;
    [Tooltip("Maximum damage dealt")]
    [SerializeField] public float maxDamage = 100.0f;
    [Tooltip("Mimum damage dealt")]
    [SerializeField] public float minDamage = 50.0f;

    [HideInInspector] public int team = -1;
    [HideInInspector] public uint spawnedFromPlayer;

    private float damageDiff;
    private float inverseFalloff;

    #region Server

    public override void OnStartServer()
    {
        damageDiff = maxDamage - minDamage;
        inverseFalloff = 1 / outerFalloffRange;
        StartCoroutine(DestoryAfterSeconds());
    }

    [Server]
    private IEnumerator DestoryAfterSeconds()
    {
        yield return new WaitForSeconds(lifeSpan);

        RpcSetTransform(transform.position, transform.rotation);

        NetworkServer.Destroy(gameObject);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if(!other.TryGetComponent<Health>(out Health health) && other.isTrigger) { return;  }

        if (health != null)
        {
            health.DealDamage(projetileDamage, team, spawnedFromPlayer);
        }

        RpcSetTransform(transform.position, transform.rotation);

        NetworkServer.Destroy(gameObject);
    }

    [ServerCallback]
    private void OnDestroy()
    {
        AISoundManager.RegisterSoundAtLocation(transform.position, team);

        if (isExplosive)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, outerFalloffRange);
            List<Health> damagedHealth = new List<Health>();
            foreach (Collider c in hits)
            {
                if (Physics.Raycast(transform.position, c.transform.position - transform.position, out RaycastHit hit, outerFalloffRange))
                {
                    if (hit.collider.TryGetComponent<Health>(out Health healthComp))
                    {
                        if (damagedHealth.Contains(healthComp))
                            continue;
                        damagedHealth.Add(healthComp);
                        float damage = maxDamage;
                        float distance = hit.distance;
                        if (distance > innerFalloffRange && distance <= outerFalloffRange)
                        {
                            damage = minDamage + ((outerFalloffRange - distance) * inverseFalloff) * damageDiff;
                        }
                        healthComp.DealDamage(damage, team, spawnedFromPlayer);
                    }
                }
            }
        }
    }

    #endregion


    #region Client

    public override void OnStopClient()
    {
        if (explosionPrefab != null && isExplosive)
        {
            GameObject explosionInstance = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
    }

    #endregion

    private void FixedUpdate()
    {
        Vector3 newPosition = transform.forward * speed * Time.fixedDeltaTime + transform.position;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, speed * Time.fixedDeltaTime, shootMask))
        {
            newPosition = hit.point;
        }
        rb.MovePosition(newPosition);
    }

    [ClientRpc]
    private void RpcSetTransform(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
