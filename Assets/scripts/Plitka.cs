using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Plitka : MonoBehaviour
{
    private DragObject dragObject;

    void Start()
    {
        dragObject = GetComponent<DragObject>();
        if (gameObject.transform.parent != null)
        {
            dragObject.OffDrag = true;
        }
    }
}
