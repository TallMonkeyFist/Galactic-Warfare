using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "AI/Stats")]
public class EnemyStats : ScriptableObject
{
    private const float DEGREE_MULT = 1.0f / 180.0f;

    [Header("Movement")]
    public float MaxTravelDistance = 1000.0f;
    public float MinMoveSpeed = 5.0f;
    public float MaxMoveSpeed = 6.5f;
    public float MinStrafeTime = 1.5f;
    public float MaxStrafeTime = 9.0f;
    public float MinDistanceForStrafe = 2.0f;

    [Header("Patrolling")]
    public float MinPatrolTime = 10.0f;
    public float MaxPatrolTime = 45.0f;

    [Header("Sight")]
    public float FOV = 130.0f;
    public float MinEngagementRange = 20.0f;
    public float MaxEngagementRange = 60.0f;
    public float AutoEngagementRange = 20.0f;
    public float EnemyLookRadius = 20.0f;
    public float ChaseTime = 5.0f;

    [Header("Hearing")]
    public float MinHearingRange = 20.0f;
    public float MaxHearingRange = 50.0f;
    public float TimeToIgnoreSound = 1.0f;

    [Header("Rotation")]
    public float RotationSpeed = 360.0f;

    [Header("Combat")]
    public float FireFOV = 10.0f;
    public float WeaponRotationSpeed = Mathf.PI;
    [Tooltip("How accurate the AI is (0 = perfect accuracy)")]
    [Range (0, 180)]
    [SerializeField] private float accuracy = 5.0f;

    [HideInInspector] public float LookAccuracy
    {
        get
        {
            float degrees = accuracy * DEGREE_MULT;
            return degrees;
        }
    }
    [HideInInspector] public float FireAccuracy { get { return accuracy; } }
}
