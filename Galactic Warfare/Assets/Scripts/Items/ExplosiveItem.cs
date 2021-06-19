using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveItem : NetworkBehaviour
{
    [Header("Explosive")]
    [Tooltip("Time it takes for the item to explode (0 if not timed)")]
    [SerializeField] private float explosionTimer = 0;
    [Tooltip("Should the timer start after first collision or on start")]
    [SerializeField] private bool startTimerAfterBounce = true;
    [Tooltip("Prefab to spawn during explosion")]
    [SerializeField] private GameObject explosionPrefab = null;
    [Tooltip("Outer falloff radius of the explosion")]
    [SerializeField] private float outerFalloffRange = 10.0f;
    [Tooltip("Inner falloff radius of the explosion")]
    [SerializeField] private float innerFalloffRange = 0.0f;
    [Tooltip("Maximum damage dealt")]
    [SerializeField] private float maxDamage = 100.0f;
    [Tooltip("Mimum damage dealt")]
    [SerializeField] private float minDamage = 50.0f;
    [Tooltip("Layers that don't block the explosion")]
    [SerializeField] private LayerMask explosionMask = new LayerMask();

    private bool timerStarted;
    private float damageDiff;
    private float inverseFalloff;

    [HideInInspector] public int team = -1;
    [HideInInspector] public uint spawnedFromPlayer;

    #region Server

    public override void OnStartServer()
    {
        damageDiff = maxDamage - minDamage;
        inverseFalloff = 1 / outerFalloffRange;
        if(explosionTimer > 0 && !startTimerAfterBounce)
        {
            StartCoroutine(DestoryAfterSeconds());
            timerStarted = true;
        }
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (explosionTimer > 0 && startTimerAfterBounce && !timerStarted)
        {
            timerStarted = true;
            StartCoroutine(DestoryAfterSeconds());
        }
    }

    [Server]
    private IEnumerator DestoryAfterSeconds()
    {
        yield return new WaitForSeconds(explosionTimer);

        NetworkServer.Destroy(gameObject);
    }

    [ServerCallback]
    private void OnDestroy()
    {
        AISoundManager.RegisterSoundAtLocation(transform.position);

        Collider[] hits = Physics.OverlapSphere(transform.position, outerFalloffRange);

        List<Health> damagedHealth = new List<Health>();

        Vector3 startPosition = transform.position + transform.up * transform.localScale.y / 2.0f; 

        foreach (Collider c in hits)
        {
            if (Physics.Raycast(startPosition, c.transform.position - startPosition, out RaycastHit hit, outerFalloffRange, explosionMask))
            {
                if (hit.collider == c && hit.collider.TryGetComponent<Health>(out Health healthComp))
                {
                    if (damagedHealth.Contains(healthComp))
                        continue;
                    damagedHealth.Add(healthComp);
                    float damage = maxDamage;
                    float distance = hit.distance;
                    if (distance >= innerFalloffRange && distance <= outerFalloffRange)
                    {
                        damage = minDamage + ((outerFalloffRange - distance) * inverseFalloff) * damageDiff;
                    }
                    healthComp.DealDamage(damage, team, spawnedFromPlayer);
                }
                else if(c.TryGetComponent(out Health health))
                {
                    if((hit.point - startPosition).sqrMagnitude > (c.transform.position - startPosition).sqrMagnitude)
                    {
                        if (damagedHealth.Contains(health))
                            continue;
                        damagedHealth.Add(health);
                        float damage = maxDamage;
                        float distance = (c.transform.position - startPosition).magnitude;
                        if (distance >= innerFalloffRange && distance <= outerFalloffRange)
                        {
                            damage = minDamage + ((outerFalloffRange - distance) * inverseFalloff) * damageDiff;
                        }
                        health.DealDamage(damage, team, spawnedFromPlayer);
                    }
                }
            }
            else
            {
                if (c.TryGetComponent<Health>(out Health healthComp))
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

    #endregion

    #region Client

    public override void OnStopClient()
    {
        if(explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
    }

    #endregion
}
