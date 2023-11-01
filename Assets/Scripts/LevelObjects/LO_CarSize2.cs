using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LO_CarSize2 : MonoBehaviour, IColorChangeable , IRotatable
{
    [SerializeField] private Transform car_paintable_parts_parent;

    private List<int> car_occupied_grid = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeColor(Material new_material)
    {
        foreach (Transform child in car_paintable_parts_parent)
        {
            child.gameObject.GetComponent<Renderer>().material = new_material;
        }
    }

    public void RotateGameObject(int rotation_type)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotation_type * 90, transform.eulerAngles.z);
    }
}
