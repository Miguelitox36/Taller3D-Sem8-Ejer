using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyFormationManager : MonoBehaviour
{
    public GameObject enemyPrefab; 
    public GameObject enemy2Prefab; 
    public float baseFollowSpeed = 10f;
    public float descentStep = 2f; 
    public float spacing = 3f; 
    public float minSpawnZOffset = 1f; 
    public float maxSpawnZOffset = 3f;

    private float direction = 1f; 
    private float mapHalfWidth;
    private List<Enemy> enemies = new List<Enemy>();
    private BoxCollider formationCollider; 

    private float currentFollowSpeed; 
        
    private List<Vector3[]> waveFormations = new List<Vector3[]>
    {
        // Ola 0: Cuadrado (3x3)
        new Vector3[] {
            new Vector3(-1, 1, 20), new Vector3(0, 1, 20), new Vector3(1, 1, 20),
            new Vector3(-1, 1, 19), new Vector3(0, 1, 19), new Vector3(1, 1, 19),
            new Vector3(-1, 1, 18), new Vector3(0, 1, 18), new Vector3(1, 1, 18)
        },
        // Ola 1: Línea horizontal más larga (5 enemigos)
        new Vector3[] {
            new Vector3(-2, 1, 20), new Vector3(-1, 1, 20), new Vector3(0, 1, 20), new Vector3(1, 1, 20), new Vector3(2, 1, 20)
        },
        // Ola 2: Triángulo (5 enemigos)
        new Vector3[] {
            new Vector3(0, 1, 20),
            new Vector3(-1, 1, 19), new Vector3(1, 1, 19),
            new Vector3(-2, 1, 18), new Vector3(2, 1, 18)
        },        
        // Ola 3: Linea vertical (5 enemigos)
        new Vector3[] {
            new Vector3(0, 1, 20), new Vector3(0, 1, 19), new Vector3(0, 1, 18), new Vector3(0, 1, -19), new Vector3(0, 1, -20)
        },
        // Ola 4: Forma de 'V' invertida
        new Vector3[] {
            new Vector3(-2, 1, 20), new Vector3(2, 1, 20),
            new Vector3(-1, 1, 19), new Vector3(1, 1, 19),
            new Vector3(0, 1, 18)
        }
    };

    void Awake()
    {       
        formationCollider = gameObject.AddComponent<BoxCollider>();
        formationCollider.isTrigger = false; 
        formationCollider.size = Vector3.one;
        formationCollider.center = Vector3.zero; 
        formationCollider.enabled = false; 
    }

    void Update()
    {
        MoveFormation();
    }
        
    public void UpdateFormationSettings(int mapUnitsSize, float tileUnitSize, float speedMultiplier)
    {        
        mapHalfWidth = mapUnitsSize * tileUnitSize;
        currentFollowSpeed = baseFollowSpeed * speedMultiplier;
        Debug.Log($"EFM: mapHalfWidth actualizado a {mapHalfWidth}, velocidad a {currentFollowSpeed}");
    }

    void MoveFormation()
    {        
        if (enemies.Count == 0)
        {
            formationCollider.enabled = false;
            return;
        }

        formationCollider.enabled = true; 
                
        UpdateFormationColliderSize();

        // Obtén los límites globales del BoxCollider de la formación después de la actualización
        Bounds currentBounds = formationCollider.bounds;

        // Calcula el movimiento horizontal
        Vector3 horizontalMove = Vector3.right * direction * currentFollowSpeed * Time.deltaTime;

        // Aplica el movimiento tentativo
        transform.Translate(horizontalMove, Space.World);

        currentBounds = formationCollider.bounds; 

        bool bounced = false;
        if (currentBounds.min.x < -mapHalfWidth)
        {            
            transform.position = new Vector3(-mapHalfWidth + currentBounds.extents.x, transform.position.y, transform.position.z);
            direction *= -1f; 
            DescentFormation(); 
            bounced = true;
        }
        else if (currentBounds.max.x > mapHalfWidth)
        {           
            transform.position = new Vector3(mapHalfWidth - currentBounds.extents.x, transform.position.y, transform.position.z);
            direction *= -1f; 
            DescentFormation(); 
            bounced = true;
        }

        if (bounced)
        {
            
        }
    }
        
    void UpdateFormationColliderSize()
    {
        CleanupNullEnemies();

        if (enemies.Count == 0)
        {
            formationCollider.size = Vector3.zero;
            formationCollider.center = Vector3.zero;
            return;
        }
                
        Bounds bounds = new Bounds(enemies[0].transform.position, Vector3.zero);
        for (int i = 1; i < enemies.Count; i++) 
        {
            Enemy enemy = enemies[i];
            if (enemy != null && enemy.gameObject != null)
            {                
                bounds.Encapsulate(enemy.transform.position);
            }
        }
                
        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
        Vector3 localSize = bounds.size;
               
        localCenter.y = 0;
        localSize.y = 1f; 

        formationCollider.center = localCenter;
        formationCollider.size = localSize;
    }

    void DescentFormation()
    {
        transform.position -= new Vector3(0, 0, descentStep);
        Debug.Log($"Formación descendiendo a: {transform.position}");
    }
        
    public void SpawnWave(int waveIndex, float enemySpeedMultiplier, int additionalEnemies)
    {
        ClearEnemies(); 

        if (waveIndex < 0 || waveIndex >= waveFormations.Count)
        {
            Debug.LogWarning($"No hay formación definida para la oleada {waveIndex}. Usando la oleada 0.");
            waveIndex = 0;
        }

        Vector3[] formationPositions = waveFormations[waveIndex];
        List<Enemy> currentWaveEnemies = new List<Enemy>();

        foreach (Vector3 posUnit in formationPositions)
        {
            GameObject prefabToUse = GetEnemyPrefabForWave(waveIndex, currentWaveEnemies.Count);
            if (prefabToUse != null)
            {
                // Multiplica la posición de la unidad por el espaciado para obtener la posición local real
                Vector3 enemyLocalPos = new Vector3(posUnit.x * spacing, posUnit.y, posUnit.z * spacing);

                // Instancia el enemigo como hijo del EnemyFormationManager, con su posición local.
                GameObject enemyGO = Instantiate(prefabToUse, transform.position + enemyLocalPos, Quaternion.identity, transform);
                Enemy enemy = enemyGO.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.SetFormationManager(this, enemyLocalPos); // Guarda la posición LOCAL de este enemigo
                    // Asegura que la velocidad del enemigo individual se actualice
                    enemy.SetEnemySpeed(enemySpeedMultiplier);
                    currentWaveEnemies.Add(enemy);
                }
            }
        }
                
        Bounds currentFormationBounds = CalculateCurrentFormationBounds(formationPositions);

        for (int i = 0; i < additionalEnemies; i++)
        {
            GameObject prefabToUse = GetEnemyPrefabForWave(waveIndex, enemies.Count);
            if (prefabToUse != null)
            {                
                float randomX = Random.Range(currentFormationBounds.min.x - spacing, currentFormationBounds.max.x + spacing);
                float randomZ = Random.Range(currentFormationBounds.min.z - minSpawnZOffset, currentFormationBounds.max.z + maxSpawnZOffset);

                Vector3 randomLocalPos = new Vector3(randomX, 0, randomZ); 
                               
                GameObject enemyGO = Instantiate(prefabToUse, transform.position + randomLocalPos, Quaternion.identity, transform);
                Enemy enemy = enemyGO.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.SetFormationManager(this, randomLocalPos);
                    enemy.SetEnemySpeed(enemySpeedMultiplier);
                    currentWaveEnemies.Add(enemy);
                }
            }
        }

        enemies.AddRange(currentWaveEnemies);
        UpdateFormationColliderSize(); 
        Debug.Log($"Aparecieron {enemies.Count} enemigos para la oleada {waveIndex}.");
                
        GameManager.instance.RegisterEnemies(enemies);
    }
        
    private Bounds CalculateCurrentFormationBounds(Vector3[] formationPositions)
    {
        if (formationPositions == null || formationPositions.Length == 0)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        Bounds bounds = new Bounds(formationPositions[0] * spacing, Vector3.zero);
        for (int i = 1; i < formationPositions.Length; i++)
        {
            bounds.Encapsulate(formationPositions[i] * spacing);
        }
        return bounds;
    }

    private GameObject GetEnemyPrefabForWave(int waveIndex, int enemyIndex)
    {       
        if (waveIndex == 1 && enemyIndex % 2 == 0) 
            return enemy2Prefab;
        else if (waveIndex == 2 && enemyIndex > 2)
            return enemy2Prefab;
        else if (waveIndex == 3 && enemyIndex % 3 == 0) 
            return enemy2Prefab;
        else
            return enemyPrefab;
    }

    public void ClearEnemies()
    {
        foreach (Enemy enemy in enemies.ToList())
        {
            if (enemy != null && enemy.gameObject != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        enemies.Clear();
        formationCollider.enabled = false;
    }

    public void RemoveEnemy(Enemy enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
        CleanupNullEnemies();
        UpdateFormationColliderSize(); 
    }

    private void CleanupNullEnemies()
    {
        enemies.RemoveAll(enemy => enemy == null || enemy.gameObject == null);
    }

    public bool HasEnemiesLeft()
    {
        CleanupNullEnemies();
        return enemies.Count > 0;
    }
}
