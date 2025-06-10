using System;
using UnityEngine;
using System.Collections.Generic;

public class Triangle : MonoBehaviour
{
    public List<Triangle> Hexagons = new List<Triangle>();
    public List<GameObject> Plitkas = new List<GameObject>();
    public List<GameObject> Walls = new List<GameObject>();

    private void Start()
    {
        FindNeighbors();
    }

    private void Update()
    {
        if (Plitkas.Count > 0)
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
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Plitka"))
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
        if (other.CompareTag("Plitka")) 
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

        Triangle upperNeighbor = Hexagons[0]; // Up-сосед (индекс зависит от порядка в directions)
        
        if (upperNeighbor != null && upperNeighbor.Plitkas.Count > 0)
        {
            GameObject myTopTile = Plitkas[Plitkas.Count - 1]; // Последняя плитка = верхняя
            GameObject neighborTopTile = upperNeighbor.Plitkas[upperNeighbor.Plitkas.Count - 1];

            // Сравниваем цвета (предполагаем, что у плиток есть компонент SpriteRenderer)
            Material myRenderer = myTopTile.GetComponent<Material>();
            Material neighborRenderer = neighborTopTile.GetComponent<Material>();

            if (myRenderer != null && neighborRenderer != null && 
                myRenderer == neighborRenderer)
            {
                Debug.Log("Цвет верхней плитки совпадает с соседом!");
                // Здесь можно добавить логику слияния или другие действия
            }
        }
    }
}
