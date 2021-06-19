using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTest : NetworkBehaviour
{
    [System.Serializable]
    public class Package
    {
        public bool forward;
        public bool backward;
        public bool left;
        public bool right;
        public bool jump;
        public bool sprint;
        public Vector3 rightDirection;
        public Vector3 forwardDirection;
        public float Timestamp;
    }

    [System.Serializable]
    public class ReceivePackage
    {
        public float X;
        public float Y;
        public float Z;
        public float Timestamp;
    }
}
