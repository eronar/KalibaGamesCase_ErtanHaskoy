using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PathfindingController : MonoBehaviour
{
    public Camera maincamera;
    public LayerMask grid_tile_layer;
    public GridCreator gridCreator;

    private float grid_tile_size;

    public GridTile start_tile;
    public GridTile end_tile;

    public List<GridTile> all_tiles_list = new List<GridTile>();
    public int grid_x;
    public int grid_y;


    public List<GridTile> open_list = new List<GridTile>();
    public List<GridTile> closed_list = new List<GridTile>();
    public List<GridTile> grid_tile_path = new List<GridTile>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))  //Start
        //{
        //    maincamera = gridCreator.active_camera;
        //    Ray ray = maincamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, maincamera.nearClipPlane));
        //    if (Physics.Raycast(ray, out RaycastHit hit, 100, grid_tile_layer))
        //    {
        //        start_tile = hit.collider.gameObject.GetComponent<GridTile>();
        //        start_point_indicator.transform.position = start_tile.transform.position + new Vector3(0, 1, 0);
        //        character.transform.position = start_tile.transform.position + new Vector3(0, 1, 0);
        //    }

        //}

        //if (Input.GetMouseButtonDown(1))  //End
        //{
        //    maincamera = gridCreator.active_camera;
        //    Ray ray = maincamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, maincamera.nearClipPlane));
        //    if (Physics.Raycast(ray, out RaycastHit hit, 100, grid_tile_layer))
        //    {
        //        end_tile = hit.collider.gameObject.GetComponent<GridTile>();
        //        end_point_indicator.transform.position = end_tile.transform.position + new Vector3(0, 1, 0);
        //    }
        //}

        //if (Input.GetKeyDown(KeyCode.Space)) //Calculate path
        //{
        //    CalculateShortestPath();
        //}

        //if (Input.GetKeyDown(KeyCode.Return)) //Start movement
        //{
        //    StartCharacterMovement();
        //}

        //if (Input.GetKeyDown(KeyCode.Backspace)) //ResetGridPathfindingValues
        //{
        //    ResetGridValues();
        //}
    }

    public void ResetGridValues()
    {
        //all_tiles_list.Clear();
        for (int i = 0; i < gridCreator.grid_objects.Count; i = i + 1)
        {
            gridCreator.grid_objects[i].GetComponent<GridTile>().ResetPathfindingVariables(); ;
        }
    }

    public List<GridTile> CalculateShortestPath()
    {
        //First copy the grid tile list here
        all_tiles_list.Clear();
        for ( int i = 0; i  < gridCreator.grid_objects.Count; i = i + 1)
        {
            all_tiles_list.Add(gridCreator.grid_objects[i].GetComponent<GridTile>());
        }
        grid_x = gridCreator.grid_x;
        grid_y = gridCreator.grid_y;
        grid_tile_size = gridCreator.grid_unit_size;

        open_list.Clear();
        closed_list.Clear();
        grid_tile_path.Clear();

        open_list.Add(start_tile);

        for (int i = 0; i < all_tiles_list.Count; i = i + 1)
        {

            all_tiles_list[i].ResetPathfindingVariables();
        }

        start_tile.g_cost = 0;
        start_tile.h_cost = CalculateDistanceBetweenTwoTiles(start_tile, end_tile);
        start_tile.CalculateFCost();

        while (open_list.Count > 0 )
        {
            GridTile currently_checked_tile = ReturnLowestFCostGridTile(open_list);
            if (currently_checked_tile == end_tile)
            {
                Debug.Log("Found a path!");
                grid_tile_path = ReturnGridTilePath(end_tile);
                //ShowTileIndexes();
                return grid_tile_path;
            }

            open_list.Remove(currently_checked_tile);
            closed_list.Add(currently_checked_tile);
            List<GridTile> neighbours =  ReturnNeighboringGridTiles(currently_checked_tile);
            for (int i = 0; i <  neighbours.Count; i = i + 1) 
            {
                if (closed_list.Contains(neighbours[i]) == false && neighbours[i].tile_is_empty == true)
                {
                    int total_gcost = currently_checked_tile.g_cost + CalculateDistanceBetweenTwoTiles(neighbours[i], currently_checked_tile);
                    if (total_gcost < neighbours[i].g_cost)
                    {
                        neighbours[i].g_cost = total_gcost;
                        neighbours[i].h_cost = CalculateDistanceBetweenTwoTiles(neighbours[i], end_tile);
                        neighbours[i].CalculateFCost();
                        neighbours[i].pathfinding_parent = currently_checked_tile;
                    }
                    if (open_list.Contains(neighbours[i]) == false)
                    {
                        open_list.Add(neighbours[i]);
                    }
                }
            }
            //Debug.Log("Finished " + currently_checked_tile.grid_coordinate.x + " , " + currently_checked_tile.grid_coordinate.y);
        }

        Debug.Log("All tiles checked, there is no path available");
        return null;
    }

    private List<GridTile> ReturnGridTilePath(GridTile end_tile)
    {
        List<GridTile> grid_tile_path_list = new List<GridTile>();
        grid_tile_path_list.Add(end_tile);
        GridTile current_tile = end_tile;
        while (current_tile.pathfinding_parent != null)
        {
            grid_tile_path_list.Add(current_tile.pathfinding_parent);
            current_tile = current_tile.pathfinding_parent;
        }

        grid_tile_path_list.Reverse();
        return grid_tile_path_list;
    }

    private int CalculateDistanceBetweenTwoTiles(GridTile tile1, GridTile tile2)
    {
        int total_distance = Mathf.Abs(tile2.grid_coordinate.x - tile1.grid_coordinate.x) + Mathf.Abs(tile2.grid_coordinate.y - tile1.grid_coordinate.y); 
        return total_distance;
    }

    private GridTile ReturnLowestFCostGridTile(List<GridTile> grid_tile_list)
    {

        GridTile lowestfcosttile = grid_tile_list[0];
        for ( int i = 0; i < grid_tile_list.Count; i = i + 1)
        {
            if (grid_tile_list[i].f_cost < lowestfcosttile.f_cost)
            {
                lowestfcosttile = grid_tile_list[i];
            }
        }

        return lowestfcosttile;
    }

    private List<GridTile> ReturnNeighboringGridTiles(GridTile current_tile)
    {
        List<GridTile> neighbouring_tiles = new List<GridTile>();
        int current_tile_x = current_tile.grid_coordinate.x;
        int current_tile_y = current_tile.grid_coordinate.y;

        //Left
        if (current_tile_x - 1 >= 0)
        {
            neighbouring_tiles.Add(all_tiles_list[grid_x * current_tile_y + current_tile_x - 1]);
        }

        //Right
        if (current_tile_x + 1 < grid_x)
        {
            neighbouring_tiles.Add(all_tiles_list[grid_x * current_tile_y + current_tile_x + 1]);
        }

        //Down
        if (current_tile_y - 1 >= 0)
        {
            neighbouring_tiles.Add(all_tiles_list[grid_x * (current_tile_y - 1) + current_tile_x]);
        }


        //Up
        if (current_tile_y + 1 < grid_y)
        {
            neighbouring_tiles.Add(all_tiles_list[grid_x * (current_tile_y +1 )+ current_tile_x]);
        }

        return neighbouring_tiles;
    }
    /// <summary>
    /// Shows the order of the tiles over them after a a path is calculated for debug purposes
    /// </summary>
    public void ShowTileIndexes()  
    {
        for (int i = 0; i < grid_tile_path.Count; i = i + 1) 
        {
            grid_tile_path[i].ShowIndexText(i);
        }
    }

    public void SaveNewDriverPositionToLists(GameObject driver)
    {
        gridCreator.SaveNewDriverPositionToLists(start_tile, end_tile, driver);
    }

    public void DeleteDriverPositionFromLists(GameObject driver)
    {
        gridCreator.DeleteDriverPositionFromLists(start_tile, driver);
    }

}
