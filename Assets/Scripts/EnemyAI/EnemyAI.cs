using BNG;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

[Serializable]
public class Enemies
{
    public string EnemyName;
    public int EnemyHealth;
    public GameObject EnemyObject;
    public float SpawnChance = 1f; // 0 to 1 chance
    public Animator EnemyAnim;
}


public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, MoveToPoint, ChasePlayer, Attack, Death, Despawn }
    public EnemyState currentState = EnemyState.Idle;

    [SerializeField]
    public List<Enemies> Enemies;
#if UNITY_EDITOR
    [SerializeField, EnemyTypeDropdown]
#endif
    public string selectedEnemyName;

    public Transform player;
    public Transform pointToMove;
    public float attackRange = 2f; // Melee attack range
    // Define a threshold distance for detecting enemies behind the player
    public float behindDistanceThreshold = 2f;

    [SerializeField]private EnemyLaserShooter SniperScript;
    [SerializeField] private NavMeshAgent navMeshAgent;
    private GameObject EnemyVisuals;
    public float health = 100f; // Enemy's health

    // Variables to control chase update intervals
    private float chaseUpdateTimer = 0f;
    public float minChaseUpdateInterval = 0.2f; // Minimum time between chase updates
    public float maxChaseUpdateInterval = 0.5f; // Maximum time between chase updates
    private float chaseUpdateInterval;
    private PlayerHealth PlayerHealth;
    private bool isDead = false;
    public UnityEvent OnDeath;
    public Animator selectedAnimator;
    private bool isDespawning = false;
    private bool hasDamaged = false;
    // Property to get current Enemy object and name
    public Enemies CurrentEnemy
    {
        get
        {
            return Enemies.Find(e => e.EnemyName == selectedEnemyName);
        }
    }

    void OnEnable()
    {
        UpdateEnemyVisuals();
        EnemyTracker.Instance?.RegisterEnemy(gameObject);
        navMeshAgent = GetComponent<NavMeshAgent>();
        SetRandomChaseUpdateInterval(); // Set a random initial update interval
        PlayerHealth = PlayerHealth.Instance;
        InitializeHealth();
        if (currentState == EnemyState.Attack)
        {
            navMeshAgent.enabled = false;
        }

        ApplyInitialAnimationState();

    }

    public void UpdateEnemyVisuals()
    {
        EnemyVisuals = null;

        var selectedEnemy = Enemies.Find(e => e.EnemyName.ToLower() == selectedEnemyName.ToLower());

        // Fallback to first enemy if selectedEnemy is null
        if (selectedEnemy == null && Enemies.Count > 0)
        {
            selectedEnemy = Enemies[0];
            selectedEnemyName = selectedEnemy.EnemyName;
        }

        foreach (var enemy in Enemies)
        {
            if (enemy.EnemyObject != null)
            {
                enemy.EnemyObject.SetActive(enemy == selectedEnemy);
            }
        }

        if (selectedEnemy != null && selectedEnemy.EnemyObject != null)
        {
            EnemyVisuals = selectedEnemy.EnemyObject;
            ApplyHealthToDamageables(EnemyVisuals);
            health = selectedEnemy.EnemyHealth;
        }

        selectedAnimator = selectedEnemy.EnemyAnim;
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
            case EnemyState.Despawn:
                HandleDespawn();
                break;
        }

        if (health <= 0f)
        {
            TransitionToState(EnemyState.Death);
        }

        if (!EnemyVisuals)
        {
            TransitionToState(EnemyState.Death);
        }
    }

    void InitializeHealth()
    {
        var enemyData = CurrentEnemy;
        if (enemyData != null)
        {
            health = enemyData.EnemyHealth;
        }
    }

    // Transition to a new state
    public void TransitionToState(EnemyState newState)
    {
        if (currentState == newState) return; // Avoid redundant transitions
        currentState = newState;

        // Reset all animation triggers before setting the new one
        ResetAnimationTriggers();

        switch (newState)
        {
            case EnemyState.Idle:
                if (navMeshAgent.isOnNavMesh)
                    navMeshAgent.isStopped = true;
                SetAnimationTrigger("Idle");
                break;
            case EnemyState.MoveToPoint:
                if (navMeshAgent.isOnNavMesh)
                    navMeshAgent.isStopped = false;
                MoveToPoint();
                SetAnimationTrigger("WalkToPoint");
                break;
            case EnemyState.ChasePlayer:
                if (navMeshAgent.isOnNavMesh)
                    navMeshAgent.isStopped = false;
                SetAnimationTrigger("FollowPlayer");
                break;
            case EnemyState.Attack:
                if (navMeshAgent.isOnNavMesh)
                    navMeshAgent.isStopped = true;
                SetAnimationTrigger("Attack");
                break;
            case EnemyState.Death:
                if (navMeshAgent.isOnNavMesh)
                    navMeshAgent.isStopped = true;
                SetAnimationTrigger("Death");
                break;
            case EnemyState.Despawn: // <--- ADD THIS NEW CASE
                if (navMeshAgent.isOnNavMesh)
                    navMeshAgent.isStopped = true; // Stop movement
                // No specific animation trigger for despawn, as it's typically quick
                HandleDespawn(); // Call the despawn logic
                break;
        }
    }

    // Sets the animation trigger on the selected Animator
    private void SetAnimationTrigger(string triggerName)
    {
        if (selectedAnimator != null)
        {
            selectedAnimator.SetTrigger(triggerName);
        }
    }

    // Resets all animation triggers to prevent conflicting animations
    private void ResetAnimationTriggers()
    {
        if (selectedAnimator != null)
        {
            selectedAnimator.ResetTrigger("Attack");
            selectedAnimator.ResetTrigger("FollowPlayer");
            selectedAnimator.ResetTrigger("WalkToPoint");
            selectedAnimator.ResetTrigger("Idle");
            selectedAnimator.ResetTrigger("Damage");
            selectedAnimator.ResetTrigger("Death");
        }
    }

    // Handle Idle State
    void HandleIdle()
    {
        // Idle behavior
        if (IsEnemyBehindPlayer())
        {
            switch (selectedEnemyName.ToLower())
            {
                case "melee":
                    if (selectedAnimator == null)
                    {
                        DamagePlayer();
                    }
                    TransitionToState(EnemyState.Despawn);
                    break;
                case "sniper":
                    TransitionToState(EnemyState.Despawn);
                    break;
                default:
                    
                    break;
            }
            
        }
    }

    // Handle MoveToPoint State
    void HandleMoveToPoint()
    {
        if (pointToMove != null)
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

        if (distanceToPlayer <= attackRange)
        {
            TransitionToState(EnemyState.Attack); // Reached destination, switch to attack
        }
    }

    // Handle Attack State
    void HandleAttack()
    {
        Vector3 targetPosition = new Vector3(
        player.transform.position.x,
        transform.position.y,
        player.transform.position.z
        );

        transform.LookAt(targetPosition);
        PerformAttack();
        

        // After melee attack, return to idle
        switch (selectedEnemyName.ToLower())
        {
                default:
                TransitionToState(EnemyState.Idle);
                break;
        }
    }
    void HandleDeath()
    {
        if (isDead) return;

        

        Debug.Log("Enemy has died");

        if (PlayerHealth)
        {
            PlayerHealth.KillEnemy(this.transform.position);
        }

        isDead = true;

        if (selectedAnimator == null)
        {
            GameObject _enemyObject = CurrentEnemy.EnemyObject;
            if(_enemyObject)
            _enemyObject.SetActive(false);
        }
        EnemyTracker.Instance?.UnregisterEnemyKilled(gameObject);
        TransitionToState(EnemyState.Despawn);
    }

    public void HandleDespawn()
    {
        if (isDespawning) return;

        isDespawning = true;

        if (!isDead)
        {
            EnemyTracker.Instance?.UnregisterEnemyDespawned(gameObject);
        }

        Destroy(gameObject, 4f);
    }

    // Enemy takes damage and checks for death
    public void TakeDamage(float damage)
    {
        health -= damage;
        SetAnimationTrigger("Damage"); // Play damage animation
        //Debug.LogError("Enemy Damaged! " +  damage);
        if (health <= 0f)
        {
            TransitionToState(EnemyState.Death);
        }
    }


    // Enemy moves to a designated point
    void MoveToPoint()
    {
        if (pointToMove != null)
            navMeshAgent.SetDestination(pointToMove.position);
    }

    public void MoveToPoint(Transform targetPoint)
    {
        pointToMove = targetPoint;
        if (pointToMove != null)
            navMeshAgent.SetDestination(pointToMove.position);
    }

    public void DamagePlayer()
    {
        if (!hasDamaged) 
        {
            PlayerHealth.Instance.TakeDamage();
            hasDamaged = true;
        }
    }

    // Perform attack logic based on the enemy type determined by name
    void PerformAttack()
    {

        switch (selectedEnemyName.ToLower())
        {
            case "melee":
                Debug.Log("Enemy performs a melee attack!");
                TransitionToState(EnemyState.Idle);
                break;
            case "sniper":
                Debug.Log("Enemy performs a sniper attack!");
                if (SniperScript != null)
                    SniperScript.StartShooting();
                break;
            default:
                Debug.Log("Unknown enemy performs a melee attack!");
                PlayerHealth.Instance.TakeDamage();
                break;
        }
    }

    bool IsEnemyBehindPlayer()
    {
        Transform baseTransform = GameManager.Instance.BaseTransform;
        Vector3 directionToEnemy = (transform.position - baseTransform.position).normalized;
        float distanceToEnemy = Vector3.Distance(transform.position, baseTransform.position);
        float dotProduct = Vector3.Dot(baseTransform.forward, directionToEnemy);
        return dotProduct < 0 && distanceToEnemy > behindDistanceThreshold;
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

    public void ReShowVisuals()
    {
        UpdateEnemyVisuals();
    }
    public void PickRandomEnemyType()
    {
        if (Enemies == null || Enemies.Count == 0)
            return;

        float totalChance = 0f;
        foreach (var enemy in Enemies)
        {
            totalChance += enemy.SpawnChance;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalChance);
        float cumulative = 0f;

        foreach (var enemy in Enemies)
        {
            cumulative += enemy.SpawnChance;
            if (randomValue <= cumulative)
            {
                selectedEnemyName = enemy.EnemyName;
                UpdateEnemyVisuals();
                return;
            }
        }

        // Fallback to first enemy if something goes wrong
        selectedEnemyName = Enemies[0].EnemyName;
        UpdateEnemyVisuals();
    }

    private void ApplyHealthToDamageables(GameObject root)
    {
        var damageables = root.GetComponentsInChildren<Damageable>(true);
        foreach (var dmg in damageables)
        {
            dmg.Health = health; // Or use a custom method or property
        }
    }

    private void ApplyInitialAnimationState()
    {
        ResetAnimationTriggers();

        switch (currentState)
        {
            case EnemyState.Idle:
                SetAnimationTrigger("Idle");
                break;
            case EnemyState.MoveToPoint:
                SetAnimationTrigger("WalkToPoint");
                break;
            case EnemyState.ChasePlayer:
                SetAnimationTrigger("FollowPlayer");
                break;
            case EnemyState.Attack:
                SetAnimationTrigger("Attack");
                break;
            case EnemyState.Death:
                SetAnimationTrigger("Death");
                break;
            case EnemyState.Despawn:
                // Optional: Set a despawn animation if you have one
                break;
        }
    }

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnemyTypeDropdownAttribute))]
public class EnemyTypeDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        UnityEngine.Object target = property.serializedObject.targetObject;
        List<Enemies> enemyList = null;
        EnemyAI enemyAI = null;
        EnemyAIManager manager = null;

        if (target is EnemyAI ai)
        {
            enemyAI = ai;
            enemyList = ai.Enemies;
        }
        else if (target is EnemyAIManager mgr)
        {
            manager = mgr;
            enemyAI = mgr.GetComponentInChildren<EnemyAI>();
            if (enemyAI != null)
                enemyList = enemyAI.Enemies;
        }

        if (enemyList == null || enemyList.Count == 0)
        {
            EditorGUI.LabelField(position, label.text, "No enemies defined");
            return;
        }

        List<string> names = new List<string>();
        int currentIndex = -1;

        for (int i = 0; i < enemyList.Count; i++)
        {
            names.Add(enemyList[i].EnemyName);
            if (enemyList[i].EnemyName == property.stringValue)
                currentIndex = i;
        }

        if (currentIndex < 0) currentIndex = 0;

        int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, names.ToArray());
        string newSelection = names[selectedIndex];

        if (selectedIndex != currentIndex && property.stringValue != newSelection)
        {
            property.stringValue = newSelection;
            property.serializedObject.ApplyModifiedProperties();

            if (enemyAI != null)
            {
                enemyAI.selectedEnemyName = newSelection;
                enemyAI.UpdateEnemyVisuals();
                EditorUtility.SetDirty(enemyAI);
            }

            if (manager != null)
            {
                manager.selectedEnemyName = newSelection;
                manager.ApplyChangesToEnemy(); // Ensure visual + stats update
                EditorUtility.SetDirty(manager);
            }
        }
    }
}

public class EnemyTypeDropdownAttribute : PropertyAttribute { }
#endif

