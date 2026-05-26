
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "PixelAdventure/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Horizontal move speed in units/second")]
    [Range(1f, 20f)] public float moveSpeed = 8f;

    [Tooltip("How fast player reaches full speed on ground")]
    [Range(1f, 50f)] public float acceleration = 12f;

    [Tooltip("How fast player stops on ground")]
    [Range(1f, 50f)] public float deceleration = 16f;

    [Tooltip("How fast player changes direction in air")]
    [Range(1f, 30f)] public float airAcceleration = 8f;

    [Header("Jumping")]
    [Tooltip("Initial jump velocity")]
    [Range(5f, 30f)] public float jumpForce = 10f;

    [Tooltip("Double jump velocity")]
    [Range(5f, 25f)] public float doubleJumpForce = 8f;

    [Tooltip("Velocity multiplier when jump button released early (0=no cut, 1=full)")]
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.4f;

    [Tooltip("Extra gravity multiplier when falling (makes fall feel snappier)")]
    [Range(1f, 10f)] public float fallGravityMultiplier = 5f;

    [Tooltip("Maximum fall speed cap")]
    [Range(5f, 50f)] public float maxFallSpeed = 25f;

    [Tooltip("Time after walking off ledge where jump still works")]
    [Range(0f, 0.3f)] public float coyoteTime = 0.12f;

    [Tooltip("Time before landing where jump input is buffered")]
    [Range(0f, 0.3f)] public float jumpBufferTime = 0.15f;

    [Header("Health")]
    [Tooltip("Maximum hearts/hit points")]
    [Range(1, 10)] public int maxHealth = 5;

    [Tooltip("Duration of invincibility frames after being hit")]
    [Range(0.5f, 5f)] public float invincibilityDuration = 1.5f;

    [Tooltip("Force applied away from damage source")]
    [Range(1f, 20f)] public float knockbackForce = 8f;

    [Header("Ground Check")]
    [Tooltip("Radius of ground detection circle")]
    [Range(0.05f, 0.5f)] public float groundCheckRadius = 0.1f;

    [Tooltip("Layers considered as ground")]
    public LayerMask groundLayer;
}