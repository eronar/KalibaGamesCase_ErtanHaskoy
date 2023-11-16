using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private PathfindingController pathfindingController;
    [SerializeField] public GridCreator gridCreator;
    private Camera maincamera;
    [SerializeField] LayerMask grid_tile_layer;
    [SerializeField] LayerMask car_door_hitboxes_layer;

    public int character_selection_state = 0; //0 >> no character actively selected, 1 >> character selected, 2 >> selected character moving???
    private GameObject currently_selected_character;
    private List<IDriveable> cars_in_waiting_list;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))  //Start player touch input
        {
            maincamera = gridCreator.active_camera;
            Ray ray = maincamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, maincamera.nearClipPlane));
            if (Physics.Raycast(ray, out RaycastHit hit, 100, grid_tile_layer))
            {

                if (character_selection_state == 0) //No character was was currently selected, can only select a driver in this state
                {
                    //Debug.Log(hit.collider.gameObject.GetComponent<GridTile>().grid_coordinate);
                    GameObject driver_over_clicked_gridtile = GetDriverOnClickedTile(hit.collider.gameObject.GetComponent<GridTile>());
                    if (driver_over_clicked_gridtile == null) //There was no driver on the clicked tile, do nothing
                    {
                        Debug.Log("No Driver Over Clicked Tile");
                    }
                    else
                    {
                        currently_selected_character = driver_over_clicked_gridtile;
                        currently_selected_character.GetComponent<IDriver>().SelectDriver();
                        pathfindingController.start_tile = hit.collider.gameObject.GetComponent<GridTile>();
                        character_selection_state = 1;

                    }
                }
                else if (character_selection_state == 1)  //A character was already selected, if we click on another character, select the new one, if we click on an unocupied tile, calculate path and move if possible
                {
                    GameObject driver_over_clicked_gridtile = GetDriverOnClickedTile(hit.collider.gameObject.GetComponent<GridTile>());
                    if (driver_over_clicked_gridtile == null) //There was no driver on the clicked tile, check if it is occupied and calculate a path if it is not
                    {
                        if (hit.collider.gameObject.GetComponent<GridTile>().tile_is_empty == true)  //Tile is empty, check if the selected driver is able to go there 
                        {

                            pathfindingController.end_tile = hit.collider.gameObject.GetComponent<GridTile>();
                            List<GridTile> path_gridtile_list = pathfindingController.CalculateShortestPath();

                            if (path_gridtile_list == null) //There is no path available to target tile
                            {
                                //Highlight the clicked tile red and lock controls for  short time
                                hit.collider.gameObject.GetComponent<GridTile>().TileIsInaccessible();
                                currently_selected_character.GetComponent<IDriver>().DriverAngry();
                                character_selection_state = 2;
                                Invoke("ReEnablePlayerControls", 0.5f);
                            }
                            else //Shortest path calculated, start the movement of the currently selected character
                            {

                                hit.collider.gameObject.GetComponent<GridTile>().TileIsAccessible();
                                currently_selected_character.GetComponent<IDriver>().MoveToTile(path_gridtile_list, this, null);
                                character_selection_state = 2;
                                Invoke("ReEnablePlayerControls", 0.1f);

                                //THIS PART IS NOT USED ANYMORE
                                //THE DRIVER GETS ON THE CAR BY CLICKING ON ADJACENT ENTRY POINTS INSTEAD OF THE CAR ITSELF
                                //Highlight the clicked tile green and lock controls until character movement is complete
                                //Also check if the clicked tile has a car door next to it
                                //If it has a door with the same color as the driver, show happy emoji, highlight the car etc.
                                //If there are multiple same color car doors next to the tile, driver enters the first one
                                /*
                                CarDoorHitbox matching_color_car_door = null;
                                Collider[] hitColliders = Physics.OverlapBox(hit.collider.transform.position, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, car_door_hitboxes_layer);
                                if (hitColliders.Length > 0)
                                {
                                    Debug.Log(hitColliders[0]);
                                    int current_character_color_index = currently_selected_character.GetComponent<IColorChangeable>().color_index;

                                    for ( int i = 0; i < hitColliders.Length; i = i + 1)
                                    {
                                        if (hitColliders[i].GetComponent<CarDoorHitbox>().color_index == current_character_color_index)
                                        {
                                            matching_color_car_door = hitColliders[i].GetComponent<CarDoorHitbox>();
                                            break;
                                        }
                                    }
                                }

                                currently_selected_character.GetComponent<IDriver>().MoveToTile(path_gridtile_list, this , matching_color_car_door);
                                character_selection_state = 2;
                                */
                            }
                        }
                        else//Tile is occupied, but if it is a car, we need to check its color and door positions
                        {
                            if (hit.collider.gameObject.GetComponent<GridTile>().occupying_car == null) //The occupying object is not a car
                            {
                                Debug.Log("Invalid Destination!");
                            }
                            else
                            {
                                GameObject clicked_car = hit.collider.gameObject.GetComponent<GridTile>().occupying_car;
                                //Instead of checking if the tile is empty, we also need to check if there is a car on the clicked tile
                                //If there is a car, check if its the correct color (if it is not, angry emoji)
                                //If it is the correct color, check if its doors are accessible or not (if not accessible, angry emoji?, red highlight the blocking obstacles)

                                //Driver and car are same color, check for pathfinding
                                if ( currently_selected_character.GetComponent<IColorChangeable>().color_index == clicked_car.GetComponent<IColorChangeable>().color_index)
                                {
                                    //We use the positions of the  CarDoorHitbox of the clicked car to determine the GridTile they are over,
                                    GridTile grid_at_first_door_position =  GetGridTileAtPosition(clicked_car.GetComponent<IDriveable>().GetDoorPosition1());
                                    GridTile grid_at_second_door_position = GetGridTileAtPosition(clicked_car.GetComponent<IDriveable>().GetDoorPosition2());
                                    CarDoorHitbox first_door = clicked_car.GetComponent<IDriveable>().GetDoorHitbox1();
                                    CarDoorHitbox second_door = clicked_car.GetComponent<IDriveable>().GetDoorHitbox2();

                                    //First, check if the driver is already over one of the door tiles,
                                    //If it is, immediately swith to drive sequence
                                    if (grid_at_first_door_position != null && Vector3.Distance(currently_selected_character.transform.position , grid_at_first_door_position.transform.position) < 0.1f)
                                    {
                                        List<GridTile> templist = new List<GridTile>();
                                        templist.Add(grid_at_first_door_position);
                                        EnterAdjacentCar(grid_at_first_door_position, first_door, templist);
                                    }
                                    else if (grid_at_second_door_position != null && Vector3.Distance(currently_selected_character.transform.position, grid_at_second_door_position.transform.position) < 0.1f)
                                    {
                                        List<GridTile> templist = new List<GridTile>();
                                        templist.Add(grid_at_second_door_position);
                                        EnterAdjacentCar(grid_at_second_door_position, second_door, templist);
                                    }
                                    else //Driver is not adjacent to car, check for paths
                                    {
                                        //Check if these GridTiles are occupied, if not check if there is a path available
                                        List<GridTile> first_path_list = new List<GridTile>();
                                        List<GridTile> second_path_list = new List<GridTile>();
                                        if (grid_at_first_door_position != null && grid_at_first_door_position.tile_is_empty == true) //FIRST DOOR
                                        {
                                            List<GridTile> temp_list = CheckPathToGridTile(grid_at_first_door_position);
                                            if (temp_list != null)
                                            {
                                                for (int i = 0; i < temp_list.Count; i = i + 1)
                                                {
                                                    first_path_list.Add(temp_list[i]);
                                                }
                                            }
                                        }

                                        if (grid_at_second_door_position != null && grid_at_second_door_position.tile_is_empty == true) //SECOND DOOR
                                        {
                                            List<GridTile> temp_list = CheckPathToGridTile(grid_at_second_door_position);
                                            if (temp_list != null)
                                            {
                                                for (int i = 0; i < temp_list.Count; i = i + 1)
                                                {
                                                    second_path_list.Add(temp_list[i]);
                                                }
                                            }
                                        }

                                        Debug.Log("P1Length:" + first_path_list.Count);
                                        Debug.Log("P2Length:" + second_path_list.Count);

                                        if (first_path_list.Count == 0 && second_path_list.Count == 0)
                                        {
                                            //Both doors are unaccessible, unselect driver, play angry emoji, red highlight blockers
                                            Debug.Log("Both Doors Are Blocked!");
                                            currently_selected_character.GetComponent<IDriver>().UnselectDriver();
                                            currently_selected_character.GetComponent<IDriver>().DriverAngry();
                                            currently_selected_character = null;
                                            character_selection_state = 2;
                                            Invoke("ReEnablePlayerControls", 0.1f);

                                            //RED HIGHLIGHT OBSTACLES!
                                            if (grid_at_first_door_position != null && grid_at_first_door_position.ReturnObjectOnTop() != null)
                                            {
                                                GameObject object_over_first_door = grid_at_first_door_position.ReturnObjectOnTop();
                                                if (object_over_first_door.GetComponent<ILevelObjects>() != null)
                                                {
                                                    object_over_first_door.GetComponent<ILevelObjects>().HighlightColor2();
                                                    object_over_first_door.GetComponent<ILevelObjects>().EnableHighlight();
                                                }
                                            }

                                            if (grid_at_second_door_position != null && grid_at_second_door_position.ReturnObjectOnTop() != null)
                                            {
                                                GameObject object_over_second_door = grid_at_second_door_position.ReturnObjectOnTop();
                                                if (object_over_second_door.GetComponent<ILevelObjects>() != null)
                                                {
                                                    object_over_second_door.GetComponent<ILevelObjects>().HighlightColor2();
                                                    object_over_second_door.GetComponent<ILevelObjects>().EnableHighlight();
                                                }
                                            }
                                        }
                                        else if (first_path_list.Count != 0 && second_path_list.Count != 0)
                                        {
                                            //Both doors are accessiable, choose shortest path
                                            if (first_path_list.Count <= second_path_list.Count)
                                            {
                                                Debug.Log("First Door Closer");
                                                MoveToTheDoorOfTheClickedCar(grid_at_first_door_position, first_door, first_path_list);
                                            }
                                            else
                                            {
                                                Debug.Log("Second Door Closer");
                                                MoveToTheDoorOfTheClickedCar(grid_at_second_door_position, second_door, second_path_list);
                                            }
                                        }
                                        else //One or both doors are accessible, choose the shortest route
                                        {
                                            if (first_path_list.Count == 0)
                                            {
                                                Debug.Log("Only Second Door is Accessible");
                                                MoveToTheDoorOfTheClickedCar(grid_at_second_door_position, second_door, second_path_list);
                                            }
                                            else
                                            {
                                                Debug.Log("Only First Door is Accessible");
                                                MoveToTheDoorOfTheClickedCar(grid_at_first_door_position, first_door, first_path_list);
                                            }
                                        }
                                    }
                                }
                                else //Driver and car are different colors, play angry emoji and unselect current driver
                                {

                                    //Also red highlight the car?
                                    currently_selected_character.GetComponent<IDriver>().UnselectDriver();
                                    currently_selected_character.GetComponent<IDriver>().DriverAngry();
                                    currently_selected_character = null;
                                    character_selection_state = 2;
                                    Invoke("ReEnablePlayerControls", 0.1f);

                                    //RED HIGHLIGHT THE CAR
                                    clicked_car.GetComponent<ILevelObjects>().HighlightColor2();
                                    clicked_car.GetComponent<IDriveable>().EnableHighlight();
                                }
                            }
                        }

                    }
                    else
                    {
                        //If we click on the previously selected character, unselect it
                        if (driver_over_clicked_gridtile == currently_selected_character)
                        {
                            currently_selected_character.GetComponent<IDriver>().UnselectDriver();
                            currently_selected_character = null;
                            character_selection_state = 0;
                        }
                        else
                        {
                            currently_selected_character.GetComponent<IDriver>().UnselectDriver();
                            currently_selected_character = null;
                            currently_selected_character = driver_over_clicked_gridtile;
                            currently_selected_character.GetComponent<IDriver>().SelectDriver();
                            pathfindingController.start_tile = hit.collider.gameObject.GetComponent<GridTile>();
                            character_selection_state = 1;
                        }

                    }
                }
                else if (character_selection_state == 2) //A character is already moving, controls are locked
                {
                    Debug.Log("Controls Are Locked!");
                }
            }
        }
    }

    public GameObject GetDriverOnClickedTile(GridTile clickedgridtile)
    {
        int grid_x = gridCreator.grid_x;
        int clickedgridtileindex = clickedgridtile.grid_coordinate.y * grid_x + clickedgridtile.grid_coordinate.x;

        return gridCreator.ReturnDriverByGridTileIndex(clickedgridtileindex);
    }

    public void ReEnablePlayerControls()
    {
        Invoke("AllowNewInputs", 0.05f);
    }

    public void ReEnablePlayerControlsAfterDriverMovement()
    {
        Invoke("CheckTileOccupationStateAfterDelay", 0.05f);
        Invoke("AllowNewInputs", 0.05f);
    }

    public void LockPlayerControls()
    {
        character_selection_state = 2;
    }

    public void AllowNewInputs()
    {
        if (currently_selected_character != null)
        {
            currently_selected_character.GetComponent<IDriver>().UnselectDriver();
        }
        character_selection_state = 0;
        currently_selected_character = null;
    }

    public void CheckTileOccupationStateAfterDelay()
    {
        pathfindingController.SaveNewDriverPositionToLists(currently_selected_character);
        gridCreator.CheckIfTilesAreOccupied();
    }

    public void SaveDriverNewPosition(GameObject driver)
    {
        pathfindingController.SaveNewDriverPositionToLists(driver);
        //gridCreator.CheckIfTilesAreOccupied();
    }

    public void DeleteDriverPositionFromLists()
    {
        pathfindingController.DeleteDriverPositionFromLists(currently_selected_character);
        gridCreator.CheckIfTilesAreOccupied();
    }


    public void CheckCarsInWaitingList()
    {

    }

    public GridTile GetGridTileAtPosition(Vector3 position_to_check)
    {
        if (Physics.Raycast(position_to_check + new Vector3(0,1,0) ,Vector3.down, out RaycastHit hit, 100, grid_tile_layer))
        {
            return hit.collider.GetComponent<GridTile>();
        }
        return null;
    }

    public List<GridTile> CheckPathToGridTile(GridTile gridtile_to_check)
    {
        pathfindingController.end_tile = gridtile_to_check;
        List<GridTile> path_gridtile_list = pathfindingController.CalculateShortestPath();
        return path_gridtile_list;
    }

    public void MoveToTheDoorOfTheClickedCar(GridTile door_grid_tile, CarDoorHitbox door_hitbox, List<GridTile> path_list)
    {
        {
            //Highlight the clicked tile green and lock controls until character movement is complete
            door_grid_tile.TileIsAccessible();
            currently_selected_character.GetComponent<IDriver>().MoveToTile(path_list, this, door_hitbox);
            character_selection_state = 2;

            ReEnablePlayerControls();
        }
    }

    public void EnterAdjacentCar(GridTile door_grid_tile , CarDoorHitbox door_hitbox, List<GridTile> path_list)
    {
        door_grid_tile.TileIsAccessible();
        currently_selected_character.GetComponent<IDriver>().EnterAdjacentCar(path_list, this, door_hitbox);
        character_selection_state = 2;
        ReEnablePlayerControls();
    }

}
