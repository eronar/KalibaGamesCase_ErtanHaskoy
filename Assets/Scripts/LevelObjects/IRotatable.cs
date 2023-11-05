using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRotatable
{
    int rotation_index { get; set; }
    void RotateGameObject(int rotation_type);
}
