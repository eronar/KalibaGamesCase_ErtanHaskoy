using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDriveable
{
    void DriverArrivedAtDoorLocation(bool is_left_door);
    void EnableHighlight();
    void DisableHighlight();
    GameObject GetDriverSeatPosition();
    GameObject GetCarPosition();
    void FirstCheckIfCarIsAbleToMove(PlayerInput playerinput);
    void RoutineCarMovementCheckAfterEachPlayerAction();

}
