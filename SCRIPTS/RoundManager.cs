using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Manages the round-based gameplay:
/// - Spawns enemies
/// - Controls round timer
/// - Scales difficulty
/// - Detects win and lose conditions


public class RoundManager : MonoBehaviour
{
    // Singleton instance
    public static RoundManager Instance { get; private set; }

    // Round Configuration
    [Header("Round Settings")]
    public int totalRounds = 3;                 // Total number of rounds
    public int[] enemiesPerRound = { 5, 8, 12 }; // Enemies spawned per round
    public float timePerRound = 60f;            // Time limit per round (seconds)

    // Difficulty Scaling
    [Header("Difficulty Scaling")]
    public float[] healthMultipliers = { 1.0f, 1.25f, 1.5f };
    public float[] damageMultipliers = { 1.0f, 1.15f, 1.3f };
    public float[] speedMultipliers  = { 1.0f, 1.1f, 1.2f };

    // Enemy Spawning
    [Header("Spawning")]
    public Transform[] spawnPoints;      // Possible enemy spawn locations
    public GameObject enemyPrefab;       // Enemy prefab to spawn

    // Internal State
    int currentRound = 0;                // Current round index
    int aliveEnemies = 0;                // Enemies currently alive
    bool isRoundActive = false;           // Is the round currently running?
    float roundTimer = 0f;               // Remaining time for the round
    bool gameEnded = false;              // Has the game ended?

    // Initialization
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Start the first round
        StartCoroutine(StartNextRound());
    }

    // Update Loop
    void Update()
    {
        // Do nothing if the game is over or the round is inactive
        if (gameEnded || !isRoundActive) return;

        // Countdown timer
        roundTimer -= Time.deltaTime;

        // Update timer UI
        HUDManager.Instance.UpdateTimer(Mathf.Max(0, roundTimer));

        // Lose condition: time ran out
        if (roundTimer <= 0f)
        {
            GameOver();
        }
    }

    // Round Flow
    IEnumerator StartNextRound()
    {
        // Win condition: all rounds completed
        if (currentRound >= totalRounds)
        {
            WinGame();
            yield break;
        }

        currentRound++;
        isRoundActive = true;
        roundTimer = timePerRound;

        // Determine number of enemies to spawn this round
        int toSpawn = enemiesPerRound[Mathf.Clamp(currentRound - 1, 0, enemiesPerRound.Length - 1)];
        aliveEnemies = 0;

        // Update UI at the start of the round
        HUDManager.Instance.UpdateRoundInfo(currentRound, totalRounds, aliveEnemies);

        // Spawn enemies progressively
        for (int i = 0; i < toSpawn; i++)
        {
            SpawnEnemyForCurrentRound();
            yield return new WaitForSeconds(0.5f);
        }
    }

    void SpawnEnemyForCurrentRound()
    {
        if (gameEnded) return;

        // Choose a random spawn point
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyPrefab, spawn.position, spawn.rotation);

        // Apply difficulty scaling
        float healthMultiplier = GetMultiplier(healthMultipliers);
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.health *= healthMultiplier;

        float damageMultiplier = GetMultiplier(damageMultipliers);
        float speedMultiplier  = GetMultiplier(speedMultipliers);
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.baseDamage *= damageMultiplier;
            enemyController.moveSpeed  *= speedMultiplier;
        }

        // Increase alive enemy count
        aliveEnemies++;
        HUDManager.Instance.UpdateRoundInfo(currentRound, totalRounds, aliveEnemies);

        // Attach relay to detect when this enemy dies
        EnemyLifecycleRelay relay = enemy.AddComponent<EnemyLifecycleRelay>();
        relay.onDeath = () => EnemyKilled();
    }

    // Enemy Death Handling
    void EnemyKilled()
    {
        if (gameEnded) return;

        aliveEnemies--;
        HUDManager.Instance.UpdateRoundInfo(currentRound, totalRounds, aliveEnemies);

        if (aliveEnemies <= 0)
        {
            isRoundActive = false;

            if (this != null && isActiveAndEnabled)
                StartCoroutine(StartNextRound());
        }
    }

    // Helpers
    float GetMultiplier(float[] array)
    {
        int index = Mathf.Clamp(currentRound - 1, 0, array.Length - 1);
        return array[index];
    }

    // Game End States
    public void GameOver()
    {
        if (gameEnded) return;

        gameEnded = true;
        isRoundActive = false;

        Debug.Log("GAME OVER");
        HUDManager.Instance.ShowGameOver();
    }

    void WinGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        isRoundActive = false;

        Debug.Log("YOU WIN");
        HUDManager.Instance.ShowWin();
    }

    // UI Button Helper
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
