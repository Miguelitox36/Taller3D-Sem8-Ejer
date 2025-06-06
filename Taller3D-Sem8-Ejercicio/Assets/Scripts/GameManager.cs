using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Necesario para .ToList()

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject enemy2Prefab;
    public GameObject tilePrefab;
    public GameObject wallPrefab;

    public int level = 1;
    private int enemiesRemaining;

    private int mapUnitsSize;

    public float tileUnitSize = 8f;

    private Transform player;
    private GameObject currentFloorTile;

    private GameObject leftWall, rightWall, topWall, bottomWall;

    [System.NonSerialized]
    public int currentWaveIndex = 0;
    [System.NonSerialized]
    public int wavesPerLevel = 3;

    private EnemyFormationManager efm;

    private HashSet<Enemy> aliveEnemies = new HashSet<Enemy>();

    private bool gameOver = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetupGame();
    }

    void SetupGame()
    {
        if (efm == null)
        {
            GameObject formationManagerGO = new GameObject("EnemyFormationManager");
            efm = formationManagerGO.AddComponent<EnemyFormationManager>();
            efm.enemyPrefab = enemyPrefab;
            efm.enemy2Prefab = enemy2Prefab;
        }

        gameOver = false;

        SpawnNextWave();
    }

    public int GetMapUnitsSize() => mapUnitsSize;
    public float GetTileUnitSize() => tileUnitSize;

    void GenerateOrUpdateMap()
    {

        float worldSize = (mapUnitsSize * 2) * tileUnitSize;
        Debug.Log($"Calculado worldSize (dimensión total del mapa): {worldSize}");

        if (currentFloorTile == null)
        {
            currentFloorTile = Instantiate(tilePrefab, new Vector3(0, -0.1f, 0), Quaternion.identity);
            currentFloorTile.tag = "Tile";
            currentFloorTile.name = "FloorTile";
        }

        currentFloorTile.transform.localScale = new Vector3(worldSize / tileUnitSize, 1f, worldSize / tileUnitSize);
        Debug.Log($"Escala del Tile del Suelo ajustada a: {currentFloorTile.transform.localScale}");

        float wallHeight = 5f;
        float wallPositionOffset = worldSize / 2f;
        Debug.Log($"Desplazamiento de posición del muro: {wallPositionOffset}");

        if (leftWall == null) { leftWall = Instantiate(wallPrefab); leftWall.name = "LeftWall"; leftWall.tag = "Wall"; }
        if (rightWall == null) { rightWall = Instantiate(wallPrefab); rightWall.name = "RightWall"; rightWall.tag = "Wall"; }
        if (topWall == null) { topWall = Instantiate(wallPrefab); topWall.name = "TopWall"; topWall.tag = "Wall"; }
        if (bottomWall == null) { bottomWall = Instantiate(wallPrefab); bottomWall.name = "BottomWall"; bottomWall.tag = "BottomWall"; }

        BoxCollider bc = bottomWall.GetComponent<BoxCollider>();
        if (bc == null) bc = bottomWall.AddComponent<BoxCollider>();
        bc.isTrigger = true;

        leftWall.transform.position = new Vector3(-wallPositionOffset, wallHeight / 2f, 0);
        leftWall.transform.localScale = new Vector3(1f, wallHeight, worldSize);
        Debug.Log($"Muro Izquierdo Pos: {leftWall.transform.position}, Escala: {leftWall.transform.localScale}");

        rightWall.transform.position = new Vector3(wallPositionOffset, wallHeight / 2f, 0);
        rightWall.transform.localScale = new Vector3(1f, wallHeight, worldSize);
        Debug.Log($"Muro Derecho Pos: {rightWall.transform.position}, Escala: {rightWall.transform.localScale}");

        topWall.transform.position = new Vector3(0, wallHeight / 2f, wallPositionOffset);
        topWall.transform.localScale = new Vector3(worldSize, wallHeight, 1f);
        Debug.Log($"Muro Superior Pos: {topWall.transform.position}, Escala: {topWall.transform.localScale}");

        bottomWall.transform.position = new Vector3(0, wallHeight / 2f, -wallPositionOffset);
        bottomWall.transform.localScale = new Vector3(worldSize, wallHeight, 1f);
        Debug.Log($"Muro Inferior Pos: {bottomWall.transform.position}, Escala: {bottomWall.transform.localScale}");

        if (player != null && player.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            pc.UpdateLimits(mapUnitsSize, tileUnitSize);
        }
    }

    void SpawnPlayer()
    {
        Vector3 startPos = new Vector3(0, 0.6f, -(mapUnitsSize * tileUnitSize) + (2f * tileUnitSize)); // 2 tiles de margen desde el borde

        if (player == null)
        {
            GameObject newPlayer = Instantiate(playerPrefab, startPos, Quaternion.identity);
            player = newPlayer.transform;
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(player);
            }
        }
        else
        {
            player.position = startPos;
        }

        if (player.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            pc.UpdateMoveSpeed(level);
            pc.UpdateLimits(mapUnitsSize, tileUnitSize);
        }
        Debug.Log($"Jugador spawn/reposicionado en: {player.position}");
    }

    public void SpawnNextWave()
    {
        if (gameOver) return;

        if (currentWaveIndex >= wavesPerLevel)
        {
            level++;
            currentWaveIndex = 0;
            Debug.Log($"--- Avanzando a Nivel general {level} ---");
        }

        mapUnitsSize = 4 + (level - 1);
        Debug.Log($"Calculado mapUnitsSize para oleada {currentWaveIndex} (nivel {level}): {mapUnitsSize}");

        GenerateOrUpdateMap();
        SpawnPlayer();
        float spawnZPos = (mapUnitsSize * tileUnitSize) - (4f * tileUnitSize);
        efm.transform.position = new Vector3(0, 1f, spawnZPos);
        Debug.Log($"Formación de enemigos reposicionada en: {efm.transform.position}");

        float enemySpeedMultiplier = 1f + (level - 1) * 0.15f + (currentWaveIndex) * 0.05f;
        int additionalEnemies = (level - 1) * 3 + (currentWaveIndex);
        additionalEnemies = Mathf.Min(additionalEnemies, 25);

        efm.UpdateFormationSettings(mapUnitsSize, tileUnitSize, enemySpeedMultiplier);

        int waveId = currentWaveIndex % 5;
        if (level > 2)
        {
            waveId = (currentWaveIndex + level) % 5;
        }

        efm.SpawnWave(waveId, enemySpeedMultiplier, additionalEnemies);

        Debug.Log($"Lanzando oleada {currentWaveIndex + 1}/{wavesPerLevel} (del nivel general {level}) con {additionalEnemies} enemigos adicionales. Velocidad de enemigo: {enemySpeedMultiplier}");
        currentWaveIndex++;

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateUI();
        }
    }

    public void RegisterEnemies(List<Enemy> enemyList)
    {
        aliveEnemies.Clear();
        foreach (Enemy e in enemyList)
        {
            if (e != null && e.gameObject != null)
            {
                aliveEnemies.Add(e);
            }
        }
        enemiesRemaining = aliveEnemies.Count;
        Debug.Log($"Oleada iniciada con {enemiesRemaining} enemigos");

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateEnemiesRemaining(enemiesRemaining);
        }
    }

    public void EnemyKilled(Enemy enemy)
    {
        if (enemy == null || gameOver) return;
                
        if (aliveEnemies.Remove(enemy)) // Esto devuelve true si el elemento se encontró y se eliminó
        {
            enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);

            if (UIManager.instance != null)
            {
                UIManager.instance.AddScore(10 * level);
                UIManager.instance.UpdateEnemiesRemaining(enemiesRemaining);
            }

            Debug.Log($"Enemigo eliminado. Quedan: {enemiesRemaining}");

            if (enemiesRemaining <= 0)
            {
                Debug.Log("Oleada completada. Spawning next wave in 2 seconds...");
                Invoke(nameof(SpawnNextWave), 2f);
            }
        }
    }

    public void EnemyReachedBottom(Enemy enemy)
    {
        if (gameOver) return;

        Debug.Log($"Enemigo {enemy.name} alcanzó la pared inferior.");
               
        gameOver = true;

        if (UIManager.instance != null)
        {
            UIManager.instance.ShowGameOver("Game Over!\nEnemigos alcanzaron el fondo!");
        }

        Debug.Log("Game Over: Los enemigos llegaron al fondo");
    }

    public void RestartGame()
    {
        if (efm != null)
        {
            efm.ClearEnemies();
        }

        level = 1;
        currentWaveIndex = 0;
        gameOver = false;
        aliveEnemies.Clear();
        enemiesRemaining = 0;

        if (player != null)
        {
            SpawnPlayer();
        }
        SpawnNextWave();

        Debug.Log("Juego reiniciado");
    }
}