using UnityEngine;

public class DragObject : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private float zCoord;
    [SerializeField] private float smoothSpeed = 20f;

    private void OnMouseDown()
    {
        zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPos();
        isDragging = true;
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    private void Update()
    {
        if (isDragging)
        {
            Vector3 targetPosition = GetMouseWorldPos() + offset;
            // Сохраняем текущую координату Y
            targetPosition.y = transform.position.y;
            // Применяем сглаживание только к осям X и Z
            Vector3 newPosition = transform.position;
            newPosition.x = Mathf.Lerp(newPosition.x, targetPosition.x, smoothSpeed * Time.deltaTime);
            newPosition.z = Mathf.Lerp(newPosition.z, targetPosition.z, smoothSpeed * Time.deltaTime);
            transform.position = newPosition;
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePoint);
        // Возвращаем позицию мыши в мировых координатах, но сохраняем текущую координату Y
        mouseWorldPos.y = transform.position.y;
        return mouseWorldPos;
    }
}