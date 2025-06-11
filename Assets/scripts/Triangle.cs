using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Triangle : MonoBehaviour
{
    public List<Triangle> Hexagons = new List<Triangle>();
    public List<GameObject> Plitkas = new List<GameObject>();
    public List<GameObject> Walls = new List<GameObject>();
    public bool canAddPlitka = true;

    private bool isProcessing = false;
    private bool isUpdatingPositions = false;
    private bool isTransferInProgress = false; // Флаг для отслеживания переноса

    private void Start()
    {
        FindNeighbors();
    }

    private void Update()
    {
        if (Plitkas.Count > 0 && canAddPlitka)
        {
            StartCoroutine(UpdateTilePositionsCoroutine());
        }
    }
    private void FindNeighbors()
    {
        float rayDistance = 1.5f;
        Vector3[] directions = new Vector3[6]
        {
            new Vector3(0, 0, 1),
            new Vector3(0.866f, 0, 0.5f),
            new Vector3(0.866f, 0, -0.5f),
            new Vector3(0, 0, -1),
            new Vector3(-0.866f, 0, -0.5f),
            new Vector3(-0.866f, 0, 0.5f)
        };

        for (int i = 0; i < 6; i++)
        {
            Ray ray = new Ray(transform.position, directions[i]);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                Triangle triangle = hit.collider.GetComponentInParent<Triangle>();
                if (triangle != null && !Hexagons.Contains(triangle))
                {
                    Hexagons.Add(triangle);
                }
            }
        }
    }

    IEnumerator UpdateTilePositionsCoroutine()
    {
        isUpdatingPositions = true;
        UpdateTilePositions();
        yield return new WaitForSeconds(0.1f);
        isUpdatingPositions = false;
    }

    void UpdateTilePositions()
    {
        for (int i = 0; i < Plitkas.Count; i++)
        {
            if (Plitkas[i] == null || Plitkas[i].transform.parent != null)
                continue;

            var dragComp = Plitkas[i].GetComponent<DragObject>();
            if (dragComp != null && (dragComp.isDragging || !dragComp.canDrag))
                continue;

            Vector3 basePosition = (i == 0) ? transform.position : Plitkas[i-1].transform.position;
            Vector3 targetPosition = basePosition + Vector3.up * 0.2f * Walls.Count;
            
            // Проверяем, нужно ли вообще перемещать
            if (Vector3.Distance(Plitkas[i].transform.position, targetPosition) > 0.01f)
            {
                StartCoroutine(SmoothMove(Plitkas[i], targetPosition));
                dragComp.OffDrag = true;
                canAddPlitka = false;
            }
            StartCoroutine(MakeNullParent());
        }
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Plitka") && canAddPlitka)
        {
            Walls.Add(other.gameObject);
            if (other.gameObject.transform.parent == null)
            {
                DragObject dragObject = other.gameObject.GetComponent<DragObject>();
                if (dragObject != null && !dragObject.OffDrag)
                {
                    
                    Plitkas.Add(other.gameObject);
                    
                }
            }
           
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Plitka") && canAddPlitka) 
        {
            if (Plitkas.Contains(other.gameObject))
            {
                Plitkas.Remove(other.gameObject);
            }

            if (Walls.Contains(other.gameObject))
            {
                Walls.Remove(other.gameObject);
            }
        }
    }

    IEnumerator SmoothMove(GameObject obj, Vector3 targetPosition)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startPosition = obj.transform.position;
    
        while (elapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed/duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    
        obj.transform.position = targetPosition;
    }

    

    IEnumerator MakeNullParent()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (var wal in Walls)
        {
            if (wal != null && wal.transform.parent != null)
            {
                wal.transform.parent = null;
            }
        }
        
        // Проверяем соседей только если не в процессе переноса
        if (!isTransferInProgress)
        {
            CheckNeighborTopColor();
        }
    }
    public void CheckNeighborTopColor()
    {
        if (Hexagons.Count == 0 || Plitkas.Count == 0 || isProcessing)
            return;

        isProcessing = true;
        try
        {
            foreach (var neighbor in Hexagons)
            {
                if (neighbor == null || neighbor.Plitkas.Count == 0) 
                    continue;
            
                string myColor = GetColorKey(Plitkas[0]);
                string neighborColor = GetColorKey(neighbor.Plitkas[0]);
            
                if (myColor == neighborColor)
                {
                    Debug.Log($"Color match found: {myColor}");
                    StartCoroutine(ProcessColorMatch(neighbor));
                    break;
                }
            }
        }
        finally
        {
            isProcessing = false;
        }
    }
    private string GetColorKey(GameObject tileObject)
    {
        if (tileObject == null) return string.Empty;
        
        Renderer renderer = tileObject.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            return renderer.material.name.Split(' ')[0];
        }
        
        return tileObject.name;
    }
    IEnumerator ProcessColorMatch(Triangle neighbor)
    {
        if (isTransferInProgress) yield break;
        
        isTransferInProgress = true;
        yield return new WaitForSeconds(0.2f);
        
        // Дополнительные проверки
        if (this == null || neighbor == null || 
            Plitkas.Count == 0 || neighbor.Plitkas.Count == 0 || 
            GetColorKey(Plitkas[0]) != GetColorKey(neighbor.Plitkas[0]))
        {
            isTransferInProgress = false;
            yield break;
        }

        // Определяем направление переноса
        if (Walls.Count < neighbor.Walls.Count)
        {
            yield return StartCoroutine(TransferWallsSafely(neighbor, this));
        }
        else if (Walls.Count > neighbor.Walls.Count)
        {
            yield return StartCoroutine(TransferWallsSafely(this, neighbor));
        }
        
        isTransferInProgress = false;
    }

    IEnumerator TransferWallsSafely(Triangle from, Triangle to)
    {
        string targetColor = GetColorKey(from.Plitkas[0]);
        List<GameObject> wallsToTransfer = new List<GameObject>();
    
        // Собираем стены для переноса
        foreach (var wall in from.Walls.ToArray()) // Используем ToArray для безопасного удаления
        {
            if (wall != null && GetColorKey(wall) == targetColor)
            {
                wallsToTransfer.Add(wall);
            }
        }
    
        // Переносим с проверками
        foreach (var wall in wallsToTransfer)
        {
            if (wall == null) continue;
            
            from.Walls.Remove(wall);
            to.Walls.Add(wall);
            
            // Плавное перемещение
            Vector3 targetPos = to.GetTopPosition() + Vector3.up * 0.2f;
            yield return StartCoroutine(SmoothMove(wall, targetPos));
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    Vector3 GetTopPosition()
    {
        if (Plitkas.Count > 0) 
            return Plitkas[Plitkas.Count - 1].transform.position;
        if (Walls.Count > 0) 
            return Walls[Walls.Count - 1].transform.position;
        return transform.position;
    }
    
}