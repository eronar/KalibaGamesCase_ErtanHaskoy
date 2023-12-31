using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILevelObjects
{
    GridCreator gridCreator { get; set; }
    void HighlightColor1();
    void HighlightColor2();
    void EnableHighlight();
    void DisableHighlight();
}
