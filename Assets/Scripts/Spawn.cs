using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public List<GameObject> enemies = new List<GameObject>();
    private GameObject player;
    public float spawnInterval = 3f;
    public int minSpawnAmount = 5;
    public int maxSpawnAmount = 15;
    public float radius = 15f;

    private float timeCount;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused || player == null) return;

        timeCount += Time.deltaTime;
        if (timeCount >= spawnInterval)
        {
            timeCount = 0f;
            int count = Random.Range(minSpawnAmount, maxSpawnAmount + 1);
            SpawnEnemies(count, 0);
        }
    }

    public void SpawnEnemies(int quantity, int percentageToIncreaseHealth)
    {
        if (player == null || enemies == null || enemies.Count == 0) return;

        float minDistance = Mathf.Max(12f, radius * 0.8f);
        float maxDistance = Mathf.Max(15f, radius * 1.2f);

        for (int i = 0; i < quantity; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            if (randomDir == Vector2.zero) randomDir = Vector2.right;

            float distance = Random.Range(minDistance, maxDistance);
            Vector2 enemyPos = (Vector2)player.transform.position + randomDir * distance;

            GameObject enemyPrefab = RandomEnemy();
            if (enemyPrefab != null)
            {
                GameObject spawnedEnemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);

                Enemy enemyComponent = spawnedEnemy.GetComponent<Enemy>();
                if (enemyComponent != null && percentageToIncreaseHealth > 0)
                {
                    int extraHealth = Mathf.RoundToInt(enemyComponent.OriginalHealth * (percentageToIncreaseHealth / 100f));
                    enemyComponent.Health = enemyComponent.OriginalHealth + extraHealth;
                }
            }
        }
    }

    GameObject RandomEnemy()
    {
        if (enemies == null || enemies.Count == 0) return null;
        return enemies[Random.Range(0, enemies.Count)];
    }
}
