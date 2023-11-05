using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDriver 
{
    void SelectDriver();
    void UnselectDriver();
    void EnterCar();
    void MoveToTile(List<GridTile> tile_list , PlayerInput playerinput, CarDoorHitbox matching_door);
    void DriverHappy();
    void DriverAngry();
    void DriverMoving();
    void DriverStopped();



}
