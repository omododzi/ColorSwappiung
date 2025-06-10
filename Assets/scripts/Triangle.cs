using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Triangle : MonoBehaviour
{
    public List<Triangle> Hexagons = new List<Triangle>();
    public List<GameObject> Plitkas = new List<GameObject>();
    public List<GameObject> Walls = new List<GameObject>();
    public bool canAddPlitka = true;

    private void Start()
    {
        FindNeighbors();
    }

    private void Update()
    {
        if (Plitkas.Count > 0 & canAddPlitka)
        {
            UpdateTilePositions();
        }
    }
    void UpdateTilePositions()
    {
        for (int i = 0; i < Plitkas.Count; i++)
        {
            // Пропускаем плитки, которые перемещаются или имеют родителя
            if (Plitkas[i] == null || Plitkas[i].transform.parent != null)
                continue;

            var dragComp = Plitkas[i].GetComponent<DragObject>();
            if (dragComp != null && (dragComp.isDragging || !dragComp.canDrag))
                continue;

            // Устанавливаем позицию плитки
            Vector3 basePosition = (i == 0) ? transform.position : Plitkas[i-1].transform.position;
            Plitkas[i].transform.position = basePosition + Vector3.up * 0.2f *Walls.Count;
            dragComp.OffDrag = true;
            canAddPlitka = false;
            StartCoroutine(MakeNullParent());
        }
    }

    void AddNewWalls(GameObject wall)
    {
        if (wall == null) return;

        // Определяем базовую позицию (последняя плитка или стена, или сам треугольник)
        Vector3 basePosition;
    
        if (Plitkas.Count > 0)
            basePosition = Plitkas[Plitkas.Count - 1].transform.position;
        else if (Walls.Count > 0)
            basePosition = Walls[Walls.Count - 1].transform.position;
        else
            basePosition = transform.position;

        // Позиционируем новую стену с небольшим смещением вверх
        wall.transform.position = basePosition + Vector3.up * 0.2f;
    
        // Добавляем в список стен
        Walls.Add(wall);
    
        // Разрешаем добавлять новые элементы
        canAddPlitka = true;
    
        // Опционально: плавное перемещение вместо телепортации
        StartCoroutine(SmoothMove(wall, basePosition + Vector3.up * 0.2f));
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
        yield return new WaitForSeconds(0.1f);
        foreach (var wal in Walls)
        {
            if (wal.transform.parent != null)
            {
                wal.transform.parent = null;
            }
        }
        CheckNeighborTopColor();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Plitka")&& canAddPlitka)
        {
            Walls.Add(other.gameObject);
            if (other.gameObject.transform.parent == null)
            {
                DragObject dragObject = other.gameObject.GetComponent<DragObject>();
                if (!dragObject.OffDrag)
                {
                    Plitkas.Add(other.gameObject);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Plitka")&& canAddPlitka) 
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
   
    private void FindNeighbors()
    {
        float rayDistance = 1.5f; // Зависит от размера гекса
        Vector3[] directions = new Vector3[6]
        {
            new Vector3(0, 0, 1),        // Up
            new Vector3(0.866f, 0, 0.5f), // UpRight (cos(30°), sin(30°))
            new Vector3(0.866f, 0, -0.5f), // DownRight
            new Vector3(0, 0, -1),       // Down
            new Vector3(-0.866f, 0, -0.5f), // DownLeft
            new Vector3(-0.866f, 0, 0.5f)  // UpLeft
        };

        for (int i = 0; i < 6; i++)
        {
            Ray ray = new Ray(transform.position, directions[i]);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                Hexagons.Add(hit.collider.gameObject.transform.parent.gameObject.GetComponent<Triangle>());
            }
        }
    }
    
    
    public void CheckNeighborTopColor()
    {
        if (Hexagons.Count == 0 || Plitkas.Count == 0)
            return; // Нет соседей или плиток
        for (int i = 0; i < Hexagons.Count; i++)
        {
            Triangle upperNeighbor = Hexagons[i]; // Up-сосед (индекс зависит от порядка в directions)

            if (upperNeighbor != null && upperNeighbor.Plitkas.Count > 0)
            {
                Debug.Log("StartCheck");
                GameObject myTopTile = Plitkas[0]; // Последняя плитка = верхняя
                GameObject neighborTopTile = upperNeighbor.Plitkas[0];

                if (myTopTile.name == neighborTopTile.name)
                {
                    Debug.Log("Цвет верхней плитки совпадает с соседом!");
                    RemuvePlitkas(Hexagons[i].gameObject,gameObject,myTopTile.name);
                }
            }
           
        }
    }

    void RemuvePlitkas(GameObject me, GameObject other,string name)
    {
        Triangle myTriangle = gameObject.GetComponent<Triangle>();
        Triangle otherTriangle = other.gameObject.GetComponent<Triangle>();
        if (myTriangle.Walls.Count != otherTriangle.Walls.Count 
            && myTriangle.Walls.Count < otherTriangle.Walls.Count)
        {
            for (int i = 0; i < myTriangle.Walls.Count; i++)
            {
                if (myTriangle.Walls[i].name == name)
                {
                    AddNewWalls(otherTriangle.Walls[i]);
                    Walls.Remove(Walls[i]);
                }
            }
        }else if (myTriangle.Walls.Count > otherTriangle.Walls.Count)
        {
            for (int i = 0; i < otherTriangle.Walls.Count; i++)
            {
                if (otherTriangle.Walls[i].name == name)
                {
                    AddNewWalls(myTriangle.Walls[i]);
                    otherTriangle.Walls.Remove(otherTriangle.Walls[i]);
                }
            }
        }else if (myTriangle.Walls.Count == otherTriangle.Walls.Count)
        {
            for (int i = 0; i < otherTriangle.Walls.Count; i++)
            {
                if (otherTriangle.Walls[i].name == name)
                {
                    AddNewWalls(myTriangle.Walls[i]);
                    otherTriangle.Walls.Remove(otherTriangle.Walls[i]);
                }
            }
        }
        
    }
}
