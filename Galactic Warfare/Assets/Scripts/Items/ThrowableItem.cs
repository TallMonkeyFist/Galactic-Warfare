using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableItem : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Rigidbody for the item")]
    [SerializeField] private Rigidbody rb = null;
    [Tooltip("Collider to destroy after movement is locked")]
    [SerializeField] private Collider lockedCollider = null;

    [Header("Throwable Settings")]
    [Tooltip("The initial velocity direction of the item")]
    [SerializeField] private Vector3 initialDirection = new Vector3(0, 1, 1);
    [Tooltip("The initial speed of the item")]
    [SerializeField] private float speed = 5.0f;
    [Tooltip("Minimum time before item can be locked")]
    [SerializeField] private float timeToLock = 5.0f;
    [Tooltip("Does the item stick to walls")]
    [SerializeField] private bool isSticky = false;

    private float k_GroundCheckDistance = 0.2f;
    private float m_TimeBeforeLock;
    private bool locked;
    private Vector3 lastDirection;

    #region Server

    /*[Command (ignoreAuthority = true)]
    private void CmdSyncTransform(NetworkConnectionToClient conn = null)
    {
        TargetSetTransform(conn, transform.position, transform.rotation, locked, transform.parent);
    }*/

    #endregion

    #region Client

    public override void OnStartClient()
    {
        //CmdSyncTransform();
    }

    [TargetRpc]
    private void TargetSetTransform(NetworkConnection conn, Vector3 position, Quaternion rotation, bool locked, Transform parent)
    {
        transform.position = position;
        transform.rotation = rotation;
        if(lockedCollider != null && locked)
        {
            this.locked = true;
            Destroy(lockedCollider);
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        if(parent != null)
        {
            transform.parent = parent;
        }
    }

    [ClientRpc]
    private void RpcSetTransform( Vector3 position, Quaternion rotation, bool locked, Transform parent)
    {
        transform.position = position;
        transform.rotation = rotation;
        if (lockedCollider != null && locked)
        {
            this.locked = true;
            Destroy(lockedCollider);
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        if (parent != null)
        {
            transform.parent = parent;
        }
    }

    #endregion

    private void Start()
    {
        locked = false;
        Vector3 direction = transform.rotation * initialDirection;
        rb.velocity = direction * speed;
        Vector3 forward = transform.forward;
        transform.up = Vector3.up;
        transform.forward = Vector3.ProjectOnPlane(forward, Vector3.up);
        m_TimeBeforeLock = Time.time + timeToLock;
    }

    private void FixedUpdate()
    {
        if(rb.velocity.sqrMagnitude < 0.5f && !locked && Time.time > m_TimeBeforeLock)
        {
            if(isSticky)
            {
                if(Physics.Raycast(transform.position, lastDirection, out RaycastHit hit))
                {
                    transform.forward = Vector3.ProjectOnPlane(lastDirection, hit.normal);
                    transform.up = hit.normal;
                    transform.position = hit.point + hit.normal * transform.localScale.y / 2.0f;
                    SetLock(hit);
                }
            }
            else if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100.0f))
            {
                if(hit.distance < k_GroundCheckDistance)
                {
                    SetLock(hit);
                }
            }
        }
        else if(!locked && isSticky)
        {
            lastDirection = rb.velocity;
        }
    }

    private void SetLock(RaycastHit hit)
    {
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        locked = true;
        Destroy(lockedCollider);
        if (isServer)
        {
            Transform toSend = hit.transform;
            if (!hit.transform.TryGetComponent<NetworkIdentity>(out NetworkIdentity id))
            {
                toSend = null;
            }
            RpcSetTransform(transform.position, transform.rotation, true, toSend);
        }
    }
}
