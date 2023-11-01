using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    public string grid_identifier;
    public Vector2 grid_coordinate; //The coordinate of the grid unit inside the grid

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGridSpecifications(string identifier, Vector2 coordinate)
    {
        grid_identifier = identifier;
        grid_coordinate = coordinate;
    }

}
