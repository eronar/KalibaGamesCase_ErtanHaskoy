using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LO_Obstacle : MonoBehaviour , IRotatable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RotateGameObject(int rotation_type)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotation_type * 90, transform.eulerAngles.z);
    }
}
