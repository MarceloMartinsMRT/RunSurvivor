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
    public int maxActiveEnemies = 150; // Limite máximo de inimigos ativos simultâneos para manter 60 FPS

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

            // Só gera novos inimigos se não tiver atingido o teto de capacidade
            if (Enemy.activeEnemyCount < maxActiveEnemies)
            {
                int remainingAllowed = maxActiveEnemies - Enemy.activeEnemyCount;
                int count = Mathf.Min(Random.Range(minSpawnAmount, maxSpawnAmount + 1), remainingAllowed);
                if (count > 0)
                {
                    SpawnEnemies(count, 0);
                }
            }
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

    public EnemyBoss SpawnBoss(string title, int health, int damage, float speed, float scale, int chestUpgrades)
    {
        if (player == null || enemies == null || enemies.Count == 0) return null;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        if (randomDir == Vector2.zero) randomDir = Vector2.up;
        Vector2 bossPos = (Vector2)player.transform.position + randomDir * 14f;

        GameObject prefab = enemies[Random.Range(0, enemies.Count)];
        GameObject bossObj = Instantiate(prefab, bossPos, Quaternion.identity);

        Enemy oldEnemy = bossObj.GetComponent<Enemy>();
        if (oldEnemy != null)
        {
            oldEnemy.enabled = false;
            Destroy(oldEnemy);
        }

        EnemyBoss boss = bossObj.AddComponent<EnemyBoss>();
        boss.bossTitle = title;
        boss.Health = health;
        boss.Damage = damage;
        boss.Speed = speed;
        boss.XpReward = 40;
        boss.chestUpgrades = chestUpgrades;
        bossObj.transform.localScale = Vector3.one * scale;

        return boss;
    }

    GameObject RandomEnemy()
    {
        if (enemies == null || enemies.Count == 0) return null;
        return enemies[Random.Range(0, enemies.Count)];
    }
}
