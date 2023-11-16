using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LO_ObstacleSize2 : MonoBehaviour, IRotatable, ILevelObjects
{
    [SerializeField] private GameObject object_model;
    [field: SerializeField] public GridCreator gridCreator { get; set; }
    public int rotation_index { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        object_model.GetComponent<cakeslice.Outline>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RotateGameObject(int rotation_type)
    {
        rotation_index = rotation_type;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotation_type * 90, transform.eulerAngles.z);
    }

    public void HighlightColor1()
    {
        object_model.GetComponent<cakeslice.Outline>().color = 0;
    }
    public void HighlightColor2()
    {
        object_model.GetComponent<cakeslice.Outline>().color = 1;
    }

    public void EnableHighlight()
    {
        object_model.GetComponent<cakeslice.Outline>().enabled = true;
        Invoke("DisableHighlight", 1);
    }

    public void DisableHighlight()
    {
        object_model.GetComponent<cakeslice.Outline>().enabled = false;
    }
}
