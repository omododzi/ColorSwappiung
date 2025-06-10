using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Plitka : MonoBehaviour
{
    private DragObject dragObject;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        dragObject = GetComponent<DragObject>();
        if (gameObject.transform.parent != null)
        {
            dragObject.OffDrag = true;
        }
    }

    void Update()
    {
        if (!dragObject.isDragging && !dragObject.OffDrag)
        {
            transform.position = startPosition;
        }
    }
}
