using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldPlayerMovement : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Character Controller for the player")]
    [SerializeField] private CharacterController controller = null;
    [Tooltip("Head Transform for the player")]
    [SerializeField] private Transform headTransform = null;
    [Tooltip("Left/Right Rotation for the player")]
    [SerializeField] private Transform parentTransform = null;
    [Tooltip("Up/Down Rotation for the player")]
    [SerializeField] private Transform weaponTransform = null;

    [Header("Movement")]
    [Tooltip("Player move speed")]
    [SerializeField] public float moveSpeed = 6.0f;
    [Tooltip("Player jump height")]
    [SerializeField] private float jumpHeight = 3.0f;
    [Tooltip("Gravity that is applied to player")]
    [SerializeField] private float gravity = 20.0f;

    [Header("Sprint")]
    [Tooltip("Sprint speed multiplier")]
    [SerializeField] private float sprintMultiplier = 1.3f;
    [Tooltip("Seconds of stamina (time to sprint)")]
    [SerializeField] private float sprintTime = 6.0f;
    [Tooltip("Time for stamina to recharge")]
    [SerializeField] private float sprintChargeTime = 3.0f;
    [Tooltip("Stamina activation charge")]
    [Range(0, 1)]
    [SerializeField] private float sprintStartCharge = 0.10f;

    [Header("Rotation")]
    [Tooltip("Player mouse speed")]
    [SerializeField] private float mouseSensitivity = 250.0f;

    [Header("Force")]
    [Tooltip("Player mass")]
    [SerializeField] private float mass = 3.0f;

    public bool InputEnabled;

    private Vector3 downForce;
    private Vector3 lastDirection;
    private Vector3 force;

    private float stamina;
    private float minStamina;
    private float sprintCharge;
    private bool sprinting;

    public event Action<float, float> OnStaminaChanged;

    public float xAxis { get; private set; }
    public Transform LookTransform
    {
        get
        {
            return headTransform;
        }
    }

    private void OnEnable()
    {
        xAxis = 0;
        Cursor.lockState = CursorLockMode.Locked;
        stamina = sprintTime;
        minStamina = sprintTime * sprintStartCharge;
        sprintCharge = sprintTime / sprintChargeTime;
        OnStaminaChanged?.Invoke(stamina, stamina);
        InputEnabled = true;
    }

    private void Update()
    {

        ForceMove();

        if (!InputEnabled)
        {
            Move(0, 0, false, false);
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        bool jumpKeyDown = Input.GetKeyDown(KeyCode.Space);
        bool sprintKeyDown = Input.GetKey(KeyCode.LeftShift);

        if (force.sqrMagnitude < 4.0f)
        {
            Move(horizontal, vertical, jumpKeyDown, sprintKeyDown);
            Rotate(mouseX, mouseY);
        }
    }

    private void Move(float horizontal, float vertical, bool wantsToJump, bool wantsToSprint)
    {
        Vector3 direction = (transform.right * horizontal + transform.forward * vertical).normalized;

        float speed = moveSpeed;

        if (controller.isGrounded)
        {
            downForce.y = -2.0f;

            if (wantsToJump)
            {
                downForce.y = Mathf.Sqrt(jumpHeight * 2.0f * gravity);
                Debug.Log(downForce.y);
            }
            lastDirection = direction;

            if (wantsToSprint)
            {
                if (sprinting)
                {
                    if (stamina > 0)
                    {
                        speed *= sprintMultiplier;
                    }
                    else
                    {
                        sprinting = false;
                    }
                }
                else
                {
                    if (stamina > minStamina)
                    {
                        speed *= sprintMultiplier;
                        sprinting = true;
                    }
                }
            }
            else
            {
                sprinting = false;
            }
        }
        else
        {
            direction = Vector3.ClampMagnitude((direction * .25f + lastDirection), 1.0f);
        }

        if (sprinting)
        {
            stamina = Mathf.Clamp(stamina - Time.deltaTime, 0, sprintTime);
            OnStaminaChanged?.Invoke(stamina, sprintTime);
        }
        else if (stamina < sprintTime)
        {
            stamina = Mathf.Clamp(stamina + sprintCharge * Time.deltaTime, 0, sprintTime);
            OnStaminaChanged?.Invoke(stamina, sprintTime);
        }

        downForce.y -= gravity * Time.deltaTime;

        controller.Move(direction * speed * Time.deltaTime + downForce * Time.deltaTime);
    }

    private void Rotate(float x, float y)
    {
        parentTransform.Rotate(Vector3.up, x * mouseSensitivity * Time.deltaTime);

        xAxis = Mathf.Clamp(xAxis - (y * mouseSensitivity * Time.deltaTime), -80.0f, 40.0f);

        weaponTransform.localRotation = Quaternion.Euler(xAxis, 0.0f, 0.0f);
        headTransform.localRotation = weaponTransform.localRotation;
    }

    private void ForceMove()
    {
        if (force.sqrMagnitude > 0.2f)
        {
            controller.Move(force * Time.deltaTime);
            force = Vector3.Lerp(force, Vector3.zero, 5 * Time.deltaTime);
        }
    }

    public void ApplyForce(Vector3 dir, float impactForce)
    {
        dir.Normalize();
        if (dir.y < 0)
        {
            dir.y = -dir.y;
        }
        force += dir * impactForce / mass;
    }

    public float GetStamina()
    {
        return stamina;
    }

    public float GetMaxStamina()
    {
        return sprintTime;
    }
}