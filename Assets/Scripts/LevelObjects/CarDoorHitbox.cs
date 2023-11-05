using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDoorHitbox : MonoBehaviour
{
    [SerializeField] private IDriveable parent_car;
    [SerializeField] bool is_left_door = true;
    [SerializeField] public GameObject car_entry_point;

    public int color_index;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DriverMovingTowardsCar()
    {
        parent_car = transform.parent.GetComponent<IDriveable>();
        parent_car.EnableHighlight();
    }

    public void DriverArrivedAtDoorLocation()
    {
        parent_car = transform.parent.GetComponent<IDriveable>();
        parent_car.DriverArrivedAtDoorLocation(is_left_door);
    }

    public IDriveable GetParentCar()
    {
        return parent_car;
    }

    public bool GetDoorSide()
    {
        return is_left_door;
    }
}
