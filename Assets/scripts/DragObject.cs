using UnityEngine;
using System.Collections.Generic;
public class DragObject : MonoBehaviour
{
    public bool isDragging = false;
    public bool canDrag = true;
    public bool OffDrag = false;
    private Vector3 offset;
    private float zCoord;
    private float baseSmoothSpeed = 10f;
    private float distanceSpeedMultiplier = 5f;
    
    [HideInInspector]
    public Triangle currentHex; // Текущий гекс, на котором находится плитка

    private void OnMouseDown()
    {
        if(!OffDrag)
        {
            if (transform.parent != null || !canDrag)
                return;

            zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
            offset = transform.position - GetMouseWorldPos();
            isDragging = true;

            // Захватываем всю стопку плиток
            if (currentHex != null)
            {
                int index = currentHex.Plitkas.IndexOf(gameObject);
                if (index != -1)
                {
                    // Создаем временный список плиток для перемещения
                    List<GameObject> tilesToDrag = new List<GameObject>();
                    for (int i = index; i < currentHex.Plitkas.Count; i++)
                    {
                        tilesToDrag.Add(currentHex.Plitkas[i]);
                    }

                    // Устанавливаем родителя для плиток стопки
                    foreach (var tile in tilesToDrag)
                    {
                        tile.transform.SetParent(transform);
                    }
                }
            }
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging&& !OffDrag) 
            return;
        
        isDragging = false;
        
        // Сбрасываем родителя для всех дочерних плиток
       
    }

    private void Update()
    {
        if (isDragging && transform.parent == null&& !OffDrag)
        {
            Vector3 targetPosition = GetMouseWorldPos() + offset;
            targetPosition.y = 2f;

            float distanceToCamera = Vector3.Distance(Camera.main.transform.position, transform.position);
            float dynamicSpeed = baseSmoothSpeed * (1f + distanceToCamera * distanceSpeedMultiplier);

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                dynamicSpeed * Time.deltaTime
            );
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    // Обработка триггеров для отслеживания текущего гекса
    private void OnTriggerEnter(Collider other)
    {
        Triangle hex = other.GetComponent<Triangle>();
        if (hex != null)
        {
            currentHex = hex;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Triangle hex = other.GetComponent<Triangle>();
        if (hex == currentHex)
        {
            currentHex = null;
        }
    }
}