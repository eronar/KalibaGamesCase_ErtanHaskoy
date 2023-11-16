using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDriveable
{
    void DriverArrivedAtDoorLocation(bool is_left_door);
    void EnableGreenHighlight();
    void EnableHighlight();
    void DisableHighlight();
    GameObject GetDriverSeatPosition();
    GameObject GetCarPosition();
    void FirstCheckIfCarIsAbleToMove(PlayerInput playerinput);
    void RoutineCarMovementCheckAfterEachPlayerAction();
    Vector3 GetDoorPosition1();
    Vector3 GetDoorPosition2();
    CarDoorHitbox GetDoorHitbox1();
    CarDoorHitbox GetDoorHitbox2();

}
