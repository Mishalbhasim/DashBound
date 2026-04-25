using UnityEngine;

public enum PowerUpType { DoubleJump, SpeedBoost, Invincibility, ExtraLife, Shield }

[CreateAssetMenu(fileName = "PowerUpData", menuName = "PixelAdventure/PowerUpData")]
public class PowerUpData : ScriptableObject
{
    public string powerUpName;
    public PowerUpType type;
    public float duration = 10f;
    public float multiplier = 1.5f;   // For speed: 1.5x, etc.
    public Sprite icon;
    public AudioClip collectSound;
    public Color glowColor = Color.yellow;
}