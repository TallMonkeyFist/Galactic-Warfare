using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToEnable : NetworkBehaviour
{
    [Tooltip("Scripts to enable")]
    [SerializeField] private Behaviour[] toEnable = null;

    #region Client

    public override void OnStartClient()
    {
        if(hasAuthority)
        {
            foreach(Behaviour comp in toEnable)
            {
                comp.enabled = true;
            }
        }

        Destroy(this);
    }

    #endregion
}
