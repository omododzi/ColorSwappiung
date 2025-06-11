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
    
    private int isMixing = 0;
    private int DownPlitkasCount = 0;
    private int UpPlitkasCount = 0;
    private int DownPlitkasColor = 0;
    private int UpPlitkasColor = 0;

    private int monoliteColor;

    void Start()
    {
        SpawnTiles();
    }

  void SpawnTiles()
{
    GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnTarg");
    
    foreach (GameObject spawnPoint in spawnPoints)
    {
        int tilesCount = Random.Range(2, maxStackHeight + 1);
        isMixing = Random.Range(0, 2); // Исправлено на (0, 2)
        
        Vector3 currentPosition = spawnPoint.transform.position;
        GameObject currentParent = null;

        if (isMixing == 0) 
        {
            // Монотонная стопка
            monoliteColor = Random.Range(0, plitkiPrefabs.Count);
            for (int i = 0; i < tilesCount; i++)
            {
                GameObject newTile = CreateMonoliteTileAtPosition(currentPosition);
                currentPosition.y += verticalOffset;
                
                if (currentParent != null)
                    currentParent.transform.SetParent(newTile.transform);
                
                currentParent = newTile;
            }
        }
        else
        {
            // Смешанная стопка
            DownPlitkasCount = Random.Range(1, tilesCount); // Минимум 1 нижняя плитка
            UpPlitkasCount = tilesCount - DownPlitkasCount; // Остальные - верхние
            
            DownPlitkasColor = Random.Range(0, plitkiPrefabs.Count);
            UpPlitkasColor = Random.Range(0, plitkiPrefabs.Count);
            
            // Создаем нижние плитки
            for (int i = 0; i < DownPlitkasCount; i++)
            {
                GameObject newTile = CreateMixTileAtPosition(currentPosition, DownPlitkasColor);
                currentPosition.y += verticalOffset;
                
                if (currentParent != null)
                    currentParent.transform.SetParent(newTile.transform);
                
                currentParent = newTile;
            }
            
            // Создаем верхние плитки
            for (int i = 0; i < UpPlitkasCount; i++)
            {
                GameObject newTile = CreateMixTileAtPosition(currentPosition, UpPlitkasColor);
                currentPosition.y += verticalOffset;
                
                if (currentParent != null)
                    currentParent.transform.SetParent(newTile.transform);
                
                currentParent = newTile;
            }
        }
    }
}

    GameObject CreateMonoliteTileAtPosition(Vector3 position)
    {
        if (plitkiPrefabs.Count == 0)
        {
            Debug.LogError("Нет префабов плиток для генерации!");
            return null;
        }

        // Выбираем случайный префаб
        GameObject tilePrefab = plitkiPrefabs[monoliteColor];
        
        // Создаем экземпляр плитки
        GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity);
        
        // Можно добавить здесь дополнительные настройки плитки
        newTile.name = $"Tile_{monoliteColor}_{Time.time}";
        
        return newTile;
    }

    GameObject CreateMixTileAtPosition(Vector3 position, int tilesColor)
    {
        GameObject tilePrefab = plitkiPrefabs[tilesColor];
        GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity);
        newTile.name = $"Tile_{tilesColor}_{Time.time}"; // Исправлено на tilesColor
        return newTile;
    }
}