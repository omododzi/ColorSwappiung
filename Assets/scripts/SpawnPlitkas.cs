using System.Collections.Generic;
using UnityEngine;

public class SpawnPlitkas : MonoBehaviour
{
    [Header("Настройки генерации")]
    [Tooltip("Префабы плиток для генерации")]
    public List<GameObject> plitkiPrefabs = new List<GameObject>();
    [Tooltip("Максимальное количество плиток в стопке")]
    public int maxStackHeight = 5;
    [Tooltip("Расстояние между плитками по вертикали")]
    public float verticalOffset = 0.2f;

    void Start()
    {
        SpawnTiles();
    }

    void SpawnTiles()
    {
        // Находим все точки спавна
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnTarg");
        
        foreach (GameObject spawnPoint in spawnPoints)
        {
            // Определяем случайное количество плиток для этой точки
            int tilesCount = Random.Range(2, maxStackHeight + 1);
            Vector3 currentPosition = spawnPoint.transform.position;
            
            // Создаем первую (нижнюю) плитку
            GameObject bottomTile = CreateTileAtPosition(currentPosition);
            GameObject currentParent = bottomTile;

            // Создаем остальные плитки в стопке
            for (int i = 1; i < tilesCount; i++)
            {
                currentPosition.y += verticalOffset;
                GameObject newTile = CreateTileAtPosition(currentPosition);
                
                // Делаем предыдущую плитку дочерней к новой
                currentParent.transform.SetParent(newTile.transform);
                currentParent = newTile;
            }
        }
    }

    GameObject CreateTileAtPosition(Vector3 position)
    {
        if (plitkiPrefabs.Count == 0)
        {
            Debug.LogError("Нет префабов плиток для генерации!");
            return null;
        }

        // Выбираем случайный префаб
        int randomIndex = Random.Range(0, plitkiPrefabs.Count);
        GameObject tilePrefab = plitkiPrefabs[randomIndex];
        
        // Создаем экземпляр плитки
        GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity);
        
        // Можно добавить здесь дополнительные настройки плитки
        newTile.name = $"Tile_{randomIndex}_{Time.time}";
        
        return newTile;
    }
}