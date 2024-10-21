using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static Enemies;

[Serializable]
public class Enemies
{
    public string EnemyName;
    public enum EnemyType { Melee, Ranged, Sniper }
    public EnemyType enemyType;
    public GameObject EnemyObject;
}

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, MoveToPoint, ChasePlayer, Attack, Death }
    public EnemyState currentState = EnemyState.Idle;
    public EnemyType enemyAIType = EnemyType.Melee; // Default type is melee
    [SerializeField]
    private List<Enemies> Enemies;
    public Transform player;
    public Transform pointToMove;
    public float chaseRange = 10f;
    public float attackRange = 2f; // Melee attack range
    public float rangedAttackRange = 15f; // Ranged or sniper attack range
    public float attackCooldown = 1.5f;
    // Define a threshold distance for detecting enemies behind the player
    public float behindDistanceThreshold = 2f;
    public EnemyLaserShooter SniperScript;

    [SerializeField] private NavMeshAgent navMeshAgent;
    private float lastAttackTime = 0f;
    private GameObject EnemyVisuals;
    public float health = 100f; // Enemy's health

    // Variables to control chase update intervals
    private float chaseUpdateTimer = 0f;
    public float minChaseUpdateInterval = 0.2f; // Minimum time between chase updates
    public float maxChaseUpdateInterval = 0.5f; // Maximum time between chase updates
    private float chaseUpdateInterval;
    private PlayerHealth PlayerHealth;

    private bool isDead = false;

    void OnEnable()
    {
        foreach (var enemy in Enemies)
        {
            if (enemy.enemyType == this.enemyAIType)
            {
                enemy.EnemyObject.SetActive(true);
                EnemyVisuals = enemy.EnemyObject;
            }
            else
            {
                enemy.EnemyObject.SetActive(false);
            }
        }
        navMeshAgent = GetComponent<NavMeshAgent>();
        SetRandomChaseUpdateInterval(); // Set a random initial update interval
        PlayerHealth = PlayerHealth.Instance;

        if(enemyAIType == EnemyType.Sniper && currentState == EnemyState.Attack)
        {
            navMeshAgent.enabled = false;
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.MoveToPoint:
                HandleMoveToPoint();
                break;
            case EnemyState.ChasePlayer:
                HandleChasePlayer();
                break;
            case EnemyState.Attack:
                HandleAttack();
                break;
            case EnemyState.Death:
                HandleDeath();
                break;
        }

        if (!EnemyVisuals)
        {
            TransitionToState(EnemyState.Death);
        }
    }

    // Transition to a new state
    void TransitionToState(EnemyState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case EnemyState.Idle:
                navMeshAgent.isStopped = true;
                break;
            case EnemyState.MoveToPoint:
                navMeshAgent.isStopped = false;
                MoveToPoint();
                break;
            case EnemyState.ChasePlayer:
                navMeshAgent.isStopped = false;
                break;
            case EnemyState.Attack:
                navMeshAgent.isStopped = true;
                break;
            case EnemyState.Death:
                navMeshAgent.isStopped = true;
                HandleDeath();
                break;
        }
    }

    // Handle Idle State
    void HandleIdle()
    {
        // Idle behavior
        Despawn();
    }

    // Handle MoveToPoint State
    void HandleMoveToPoint()
    {
        float distanceToDestination = Vector3.Distance(transform.position, pointToMove.position);
        chaseUpdateTimer += Time.deltaTime;
        if (chaseUpdateTimer >= chaseUpdateInterval)
        {
            chaseUpdateTimer = 0f; // Reset the timer
            SetRandomChaseUpdateInterval(); // Randomize the next interval
            MoveToPoint(); // Update destination
        }
        if (distanceToDestination <= 1f)
        {
            TransitionToState(EnemyState.Attack); // Reached destination, switch to attack
        }
    }

    // Handle ChasePlayer State
    void HandleChasePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Only update the destination at intervals to optimize performance
        chaseUpdateTimer += Time.deltaTime;
        if (chaseUpdateTimer >= chaseUpdateInterval)
        {
            chaseUpdateTimer = 0f; // Reset the timer
            SetRandomChaseUpdateInterval(); // Randomize the next interval
            MoveToPoint(player); // Update destination
        }
        if (IsEnemyBehindPlayer())
        {
            switch (enemyAIType)
            {
                case EnemyType.Melee:
                    TransitionToState(EnemyState.Attack); // Melee attack when close
                    break;

                case EnemyType.Ranged:
                case EnemyType.Sniper:
                    TransitionToState(EnemyState.Attack); // Ranged attack at distance or Sniper attack at long range
                    break;
            }
        }
    }

    // Handle Attack State
    void HandleAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            PerformAttack();
        }

        switch (enemyAIType)
        {
            case EnemyType.Melee:
            case EnemyType.Sniper:
                TransitionToState(EnemyState.Idle);
                break;
        }
    }

    // Handle Death State
    void HandleDeath()
    {
        Debug.Log("Enemy has died");
        if (PlayerHealth)
        {
            if (!isDead)
            {
                PlayerHealth.KillEnemy();
                isDead = true;
            }
            
        }
        // Optionally, destroy the enemy object after a delay
        Destroy(gameObject, 0.5f); // Adjust the delay as needed
    }

    void Despawn()
    {
        Destroy(gameObject, 4f);
    }

    // Enemy takes damage and checks for death
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            TransitionToState(EnemyState.Death);
        }
    }

    // Enemy moves to a designated point
    void MoveToPoint()
    {
        navMeshAgent.SetDestination(pointToMove.position);
    }

    public void MoveToPoint(Transform targetPoint)
    {
        pointToMove = targetPoint;
        navMeshAgent.SetDestination(pointToMove.position);
    }

    // Perform attack logic based on the enemy type
    void PerformAttack()
    {
        switch (enemyAIType)
        {
            case EnemyType.Melee:
                Debug.Log("Enemy performs a melee attack!");
                PlayerHealth.Instance.TakeDamage(); // Example of damaging the player
                break;
            case EnemyType.Ranged:
                Debug.Log("Enemy performs a ranged attack!");
                ShootProjectile();
                break;
            case EnemyType.Sniper:
                Debug.Log("Enemy performs a sniper attack!");
                SniperScript.StartShooting();
                break;
        }
    }

    bool IsEnemyBehindPlayer()
    {
        Transform baseTransform = GameManager.Instance.BaseTransform;

        // Get the direction from the player to the enemy
        Vector3 directionToEnemy = (transform.position - baseTransform.position).normalized;

        // Get the distance from the player to the enemy
        float distanceToEnemy = Vector3.Distance(transform.position, baseTransform.position);

        // Check the dot product of the player's forward direction and the direction to the enemy
        float dotProduct = Vector3.Dot(baseTransform.forward, directionToEnemy);

         // Adjust this value as needed

        // If the dotProduct is less than 0 and the distance is greater than the threshold, the enemy is behind and far enough
        return dotProduct < 0 && distanceToEnemy > behindDistanceThreshold;
    }

    // Simulate shooting a projectile (for ranged and sniper enemies)
    void ShootProjectile()
    {
        Debug.Log("Enemy shoots a projectile at the player!");
    }

    // Public function to make the enemy idle
    public void Idle()
    {
        TransitionToState(EnemyState.Idle);
    }

    // Randomize the chase update interval to avoid synchronized destination updates
    void SetRandomChaseUpdateInterval()
    {
        chaseUpdateInterval = UnityEngine.Random.Range(minChaseUpdateInterval, maxChaseUpdateInterval);
    }
}
