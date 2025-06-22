using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIManager : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private NavMeshAgent navAgent;

    [Header("Enemy Stats")]
#if UNITY_EDITOR
    [SerializeField, EnemyTypeDropdown]
#endif
    public string selectedEnemyName;

    public EnemyAI.EnemyState currentState;
    public float health;
    public float attackRange;
    public float moveSpeed;

    private void Awake()
    {
        if (enemyAI == null)
            enemyAI = GetComponentInChildren<EnemyAI>();

        if (enemyAI == null)
            Debug.LogError("EnemyAIManager: No EnemyAI found in children.");

        if (navAgent == null && enemyAI != null)
            navAgent = enemyAI.GetComponent<NavMeshAgent>();

        if (navAgent == null)
            Debug.LogWarning("EnemyAIManager: No NavMeshAgent found on EnemyAI.");
    }

    public void SyncFromEnemyAI()
    {
        if (enemyAI == null) return;

        selectedEnemyName = enemyAI.selectedEnemyName;
        health = enemyAI.health;
        attackRange = enemyAI.attackRange;
        currentState = enemyAI.currentState;

        if (navAgent != null)
            moveSpeed = navAgent.speed;
    }

    public void ApplyChangesToEnemy()
    {
        if (enemyAI == null) return;

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(enemyAI, "Apply Enemy Changes");
#endif

        if (enemyAI.selectedEnemyName != selectedEnemyName)
        {
            enemyAI.selectedEnemyName = selectedEnemyName;
            enemyAI.UpdateEnemyVisuals();
        }

        enemyAI.health = health;
        enemyAI.attackRange = attackRange;
        enemyAI.currentState = currentState;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(enemyAI);
#endif

        if (navAgent != null)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(navAgent, "Apply NavMeshAgent Changes");
#endif
            navAgent.speed = moveSpeed;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(navAgent);
#endif
        }
    }


    public List<string> GetAllEnemyNames()
    {
        List<string> names = new List<string>();
        foreach (var e in enemyAI.Enemies)
        {
            names.Add(e.EnemyName);
        }
        return names;
    }

    public Enemies GetCurrentEnemyData()
    {
        return enemyAI.CurrentEnemy;
    }

    public void KillEnemy()
    {
        if (enemyAI == null) return;
        enemyAI.health = 0;
        enemyAI.OnDeath?.Invoke();
    }
}
