using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private PathfindingController pathfindingController;
    [SerializeField] private GridCreator gridCreator;
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
                                //Highlight the clicked tile green and lock controls until character movement is complete
                                //Also check if the clicked tile has a car door next to it
                                //If it has a door with the same color as the driver, show happy emoji, highlight the car etc.
                                //If there are multiple same color car doors next to the tile, driver enters the first one
                                hit.collider.gameObject.GetComponent<GridTile>().TileIsAccessible();

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
                            }
                        }
                        else//Tile is occupied, do nothing (player is unable to click this location)
                        {
                            Debug.Log("Invalid Destination!");
                        }

                    }
                    else
                    {
                        currently_selected_character.GetComponent<IDriver>().UnselectDriver();
                        currently_selected_character = driver_over_clicked_gridtile;
                        currently_selected_character.GetComponent<IDriver>().SelectDriver();
                        pathfindingController.start_tile = hit.collider.gameObject.GetComponent<GridTile>();
                        character_selection_state = 1;
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

    public void DeleteDriverPositionFromLists()
    {
        pathfindingController.DeleteDriverPositionFromLists(currently_selected_character);
        gridCreator.CheckIfTilesAreOccupied();
    }


    public void CheckCarsInWaitingList()
    {

    }

}
