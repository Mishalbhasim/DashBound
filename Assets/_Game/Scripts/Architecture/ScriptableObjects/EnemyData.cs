using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "PixelAdventure/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public string enemyName;
    public int maxHealth = 1;
    public int damageToPlayer = 1;
    public float moveSpeed = 2f;

    [Header("Patrol")]
    public float patrolDistance = 4f;
    public float waitTimeAtTurn = 0.5f;

    [Header("Detection")]
    public float detectionRange = 6f;
    public bool canFly = false;

    [Header("Death")]
    public int scoreValue = 100;
    public GameObject deathParticle;
    public AudioClip deathSound;
}