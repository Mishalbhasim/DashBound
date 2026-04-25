using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "PixelAdventure/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 12f;
    public float deceleration = 16f;
    public float airAcceleration = 8f;

    [Header("Jumping")]
    public float jumpForce = 10f;
    public float doubleJumpForce = 8f;
    public float jumpCutMultiplier = 0.4f;   // release jump early = lower peak
    public float fallGravityMultiplier = 5f;
    public float maxFallSpeed = 25f;
    public float coyoteTime = 0.12f;          // forgiveness window at platform edge
    public float jumpBufferTime = 0.15f;      // buffer jump input before landing

    [Header("Health")]
    public int maxHealth = 5;
    public float invincibilityDuration = 1.5f;
    public float knockbackForce = 8f;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
}