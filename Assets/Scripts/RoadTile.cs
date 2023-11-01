using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadTile : MonoBehaviour
{
    [SerializeField] private GameObject road_type_1; //Linear
    [SerializeField] private GameObject road_type_2; //Corner
    [SerializeField] private GameObject road_type_3; //Crossroad

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRoadType(int road_type, float road_rotation)
    {
        road_type_1.SetActive(false);
        road_type_2.SetActive(false);
        road_type_3.SetActive(false);

        if (road_type == 1)
        {
            road_type_1.SetActive(true);
        }
        else if (road_type == 2)
        {
            road_type_2.SetActive(true);
        }
        else if (road_type == 3)
        {
            road_type_3.SetActive(true);
        }

        transform.eulerAngles = new Vector3(0, road_rotation, 0);
    }
}
