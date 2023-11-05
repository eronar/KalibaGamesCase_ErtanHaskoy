using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IColorChangeable
{
    int color_index { get; set; }
    void ChangeColor(Material new_material , int colorindex);
}
