using System.Collections.Generic;
using UnityEngine;

public class SpawnPlitkas : MonoBehaviour
{
   
    public List<GameObject> plitki = new List<GameObject>();
    private GameObject[] positions;

    void Start()
    {
        positions = GameObject.FindGameObjectsWithTag("SpawnTarg");
        
        for (int i = 0; i < positions.Length; i++)
        {
            int a = Random.Range(0, 5);
            
            Vector3 newPos = new Vector3(positions[i].transform.position.x,
                positions[i].transform.position.y+0.2f,
                positions[i].transform.position.z);
            int index = Random.Range(0, plitki.Count);
            Instantiate(plitki[index], newPos, Quaternion.identity);
            for (int j = 0; j < a; j++)
            {
                index = Random.Range(0, plitki.Count);
                newPos.y += 0.2f;
                Instantiate(plitki[index], newPos, Quaternion.identity);
            }
           
        }
    }
}
