using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("First Person Camera Controller")]
    [SerializeField] private FirstPersonCamera fps = null;
    [Tooltip("Third Person Camera Controller")]
    [SerializeField] private ThirdPersonCamera tps = null;

    private bool fpsEnabled;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            if(fpsEnabled)
            {
                fps.enabled = false;
                tps.enabled = true;
            }
            else
            {
                fps.enabled = true;
                tps.enabled = false;
            }
            fpsEnabled = !fpsEnabled;
        }
    }
}
