using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableMouse : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
