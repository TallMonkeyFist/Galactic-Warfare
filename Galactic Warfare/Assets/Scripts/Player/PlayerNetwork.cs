using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetwork : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    [SerializeField] CharacterController controller;

    private void Update()
    {
        bool _forward = Input.GetKey(KeyCode.W);
        bool _backward = Input.GetKey(KeyCode.S);
        bool _left = Input.GetKey(KeyCode.A);
        bool _right = Input.GetKey(KeyCode.D);
        bool _jump = Input.GetKey(KeyCode.Space);
        bool _sprint = Input.GetKey(KeyCode.LeftShift);

        Move(_left, _right, _forward, _backward, transform.right, transform.forward);
    }

    private void Move(bool left, bool right, bool forward, bool backward, Vector3 rightDir, Vector3 forwardDir)
    {
        Vector3 dir = GetMovementDirection(forward, backward, left, right, rightDir, forwardDir);
        controller.Move(dir * moveSpeed);
    }

    private static Vector3 GetMovementDirection(bool _forward, bool _backward, bool _left, bool _right, Vector3 _rightDir, Vector3 _forwardDir)
    {
        int x = 0;
        int z = 0;

        if (_forward)
        {
            z++;
        }
        if (_backward)
        {
            z--;
        }
        if (_left)
        {
            x--;
        }
        if (_right)
        {
            x++;
        }

        Vector3 direction = (_rightDir * x + _forwardDir * z).normalized;

        return direction;
    }
}
