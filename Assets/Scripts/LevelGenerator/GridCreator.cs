using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


public class GridCreator : MonoBehaviour
{
    [Header("Dependencies")]

    public GameObject grid_prefab;
    public GameObject road_prefab;

    public GameObject perspective_camera;
    public GameObject orthographic_camera;
    public Camera active_camera;

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private LevelCompleteUI levelCompleteUI;
    [SerializeField] private GameObject grid_parent;
    [SerializeField] private GameObject road_parent;
    [SerializeField] private GameObject concrete_pivot;
    [SerializeField] private GameObject road_barrier_prefab;

    [SerializeField] public List<GameObject> object_prefabs_list = new List<GameObject>(); //Lists all prefabs that can place to the level (not roads or grids)

    [Header("Level Customization")]
    [SerializeField] public int grid_x = 5; //How many units are there in the grid hoizontally
    [SerializeField] public int grid_y = 6; //How many units are there in the grid vertically
    [SerializeField] public float grid_unit_size = 1; //The width=height of a single unit of grid
    [SerializeField] private int current_number_of_cars = 0;

    [Header("View Only")]
    public bool lists_initialized = false;
    private Vector3 grid_parent_position;
    private Vector3 grid_origin_position; //Position of the bottom left corner of the grid (including roads)

    [SerializeField] public List<GameObject> grid_objects; //A list of grid objects
    [SerializeField] private List<GameObject> road_objects; //A list of all road objects
    [SerializeField] private List<int> grid_info_list; //Holds integers which denote what is occupying the grid coordinate and if it is occupied
    [SerializeField] private List<int> grid_pathfinding_list; //Only shows if a grid position is empty(0) or occupied(1)

    [SerializeField] private List<int> grid_object_type_list = new List<int>();  //-1 >> empty , 0 >> obstacle1x, 1 >> obstacle2x , 2 >> driver , 3 >> car2x front, 4 >> car3x front
    [SerializeField] private List<GameObject> grid_object_type_prefabs_list = new List<GameObject>();  //-1 >> empty , 0 >> obstacle1x, 1 >> obstacle2x , 2 >> driver , 3 >> car2x front, 4 >> car3x front
    [SerializeField] private List<int> grid_object_color_list = new List<int>();
    [SerializeField] private List<int> grid_object_rotations_list = new List<int>();
    [SerializeField] private List<GameObject> grid_car_doors_list = new List<GameObject>();

    [SerializeField] private List<Color> object_colors_list; //We have 4 colors max for now
    [SerializeField] private List<Material> object_materials_list; //We have 4 materials max for now

    private int current_object_type = -5;
    private int current_color_type = -5;
    private int current_rotation_type = -5;

    [SerializeField] private GameObject bottom_left_road;
    [SerializeField] private GameObject bottom_right_road;
    [SerializeField] private GameObject top_left_road;
    [SerializeField] private GameObject top_right_road;
    [SerializeField] private GameObject road_barrier;


    public List<GameObject> cars_in_waiting_list = new List<GameObject>();
    private void Awake()
    {
        //CreateDefaultGridLists();
    }

    // Start is called before the first frame update
    void Start()
    {
        CheckIfTilesAreOccupied();
    }
    private void OnEnable()
    {
        //CreateDefaultGridLists();
    }

    // Update is called once per frame
    void Update()
    {
 
    }
    /// <summary>
    /// Creates a grid and surrounding roads using the given dimensions
    /// </summary>
    public void CreateGrid()
    {
        //First, make sure that dimensions are valid
        grid_x = Mathf.Clamp(grid_x, 1, 5);
        grid_y = Mathf.Clamp(grid_y, 1, 12);

        concrete_pivot.transform.localScale = new Vector3(grid_x, 1, grid_y);

        //Clear filled lists and Create default lists for object identifiers
        ClearLevel();

        //Calculate the grid origin x (0, 0) from the grid width 
        float grid_total_width = (float)grid_x* grid_unit_size;
        float grid_origin_offset_x = (float)grid_total_width / 2;

        grid_parent_position = grid_parent.transform.position;
        grid_origin_position = new Vector3(-grid_origin_offset_x + grid_unit_size/2, 0, grid_unit_size / 2);

        //Create the grid and road by loooping through width and height of the grid
        for ( int j = -1; j <= grid_y; j = j + 1)
        {
            for (int i = -1; i <= grid_x; i = i + 1)
            {

                //Roads
                if (( i == -1)|| (j == -1) || ( i == grid_x) || (j == grid_y))
                {
                    //Bottom left corner
                    if ((i == -1) && (j == -1))
                    {
                        GameObject spawned_grid_unit = Instantiate(road_prefab, road_parent.transform);
                        spawned_grid_unit.transform.localPosition = grid_origin_position + new Vector3(i * grid_unit_size, 0, j * grid_unit_size);
                        spawned_grid_unit.GetComponent<RoadTile>().SetRoadType(2, 0, new Vector2(i, j) , -1);
                        road_objects.Add(spawned_grid_unit);
                        bottom_left_road = spawned_grid_unit;
                    }

                    //Bottom right corner
                    else if ((i == grid_x) && (j == -1))
                    {
                        GameObject spawned_grid_unit = Instantiate(road_prefab, road_parent.transform);
                        spawned_grid_unit.transform.localPosition = grid_origin_position + new Vector3(i * grid_unit_size, 0, j * grid_unit_size);
                        spawned_grid_unit.GetComponent<RoadTile>().SetRoadType(2, -90, new Vector2(i, j) , -1);
                        road_objects.Add(spawned_grid_unit);
                        bottom_right_road = spawned_grid_unit;
                    }
                    //Top left corner
                    else if ((i == -1) && (j == grid_y))
                    {
                        GameObject spawned_grid_unit = Instantiate(road_prefab, road_parent.transform);
                        spawned_grid_unit.transform.localPosition = grid_origin_position + new Vector3(i * grid_unit_size, 0, j * grid_unit_size);
                        spawned_grid_unit.GetComponent<RoadTile>().SetRoadType(3, 0, new Vector2(i, j) , -1);
                        road_objects.Add(spawned_grid_unit);
                        top_left_road = spawned_grid_unit;

                        //We also create the road tiles that leads to out of screen
                        //Also create the road barrier at the location of the second road

                        for (int k = 1; k < 25; k = k + 1)
                        {
                            GameObject spawned_leaving_road = Instantiate(road_prefab, road_parent.transform);
                            spawned_leaving_road.transform.localPosition = spawned_grid_unit.transform.localPosition + new Vector3(0, 0, k * grid_unit_size);
                            spawned_leaving_road.GetComponent<RoadTile>().SetRoadType(1, 90, new Vector2(i, j), -1);
                            road_objects.Add(spawned_leaving_road);

                            if ( k == 1)
                            {
                                road_barrier = Instantiate(road_barrier_prefab, spawned_leaving_road.transform);
                                road_barrier.transform.localPosition = Vector3.zero;

                            }
                        }

                    }

                    //Top right corner
                    else if ((i == grid_x) && (j == grid_y))
                    {
                        GameObject spawned_grid_unit = Instantiate(road_prefab, road_parent.transform);
                        spawned_grid_unit.transform.localPosition = grid_origin_position + new Vector3(i * grid_unit_size, 0, j * grid_unit_size);
                        spawned_grid_unit.GetComponent<RoadTile>().SetRoadType(2, 180, new Vector2(i, j), -1);
                        road_objects.Add(spawned_grid_unit);
                        top_right_road = spawned_grid_unit;
                    }

                    //Left/right edge
                    else if ((j != -1) && (j != grid_y))
                    {
                        int road_side = -1;
                        if ( i < 0) { road_side = 3; }
                        else if (i >= 0) { road_side = 0; }
                        GameObject spawned_grid_unit = Instantiate(road_prefab, road_parent.transform);
                        spawned_grid_unit.transform.localPosition = grid_origin_position + new Vector3(i * grid_unit_size, 0, j * grid_unit_size);
                        spawned_grid_unit.GetComponent<RoadTile>().SetRoadType(1, 90, new Vector2(i, j), road_side);
                        road_objects.Add(spawned_grid_unit);
                    }

                    //Bottom/top edge
                    else if ((i != -1) && (i != grid_x))
                    {
                        int road_side = -1;
                        if (j < 0) { road_side = 1; }
                        else if (j >= 0) { road_side = 2; }
                        GameObject spawned_grid_unit = Instantiate(road_prefab, road_parent.transform);
                        spawned_grid_unit.transform.localPosition = grid_origin_position + new Vector3(i * grid_unit_size, 0, j * grid_unit_size);
                        spawned_grid_unit.GetComponent<RoadTile>().SetRoadType(1, 0, new Vector2(i, j) , road_side);
                        road_objects.Add(spawned_grid_unit);
                    }

                }
                //Grid
                else
                {
                    GameObject spawned_grid_unit = Instantiate(grid_prefab, grid_parent.transform);
                    spawned_grid_unit.transform.localPosition = grid_origin_position + new Vector3(i * grid_unit_size, 0, j * grid_unit_size);
                    spawned_grid_unit.GetComponent<GridTile>().SetGridSpecifications("grid", new Vector2Int(i, j));
                    grid_objects.Add(spawned_grid_unit);

                    //int randomchance = Random.Range(0, 10);
                    //if ( randomchance <= 2 )
                    //{
                    //    spawned_grid_unit.GetComponent<GridTile>().tile_is_accessible = false;
                    //    spawned_grid_unit.transform.GetChild(0).gameObject.SetActive(false);
                    //}
                }


            }
        }

        SetCorrectCamera();

    }

    public void ClearLevel()
    {
        for (int i = 0; i < grid_object_type_prefabs_list.Count; i = i + 1)
        {
            DestroyImmediate(grid_object_type_prefabs_list[i]);
        }

        for (int i = 0; i < grid_objects.Count; i = i + 1)
        {
            DestroyImmediate(grid_objects[i]);
        }

        for (int i = 0; i < road_objects.Count; i = i + 1)
        {
            DestroyImmediate(road_objects[i]);
        }

        grid_objects.Clear();
        road_objects.Clear();
        grid_object_type_list.Clear();
        grid_car_doors_list.Clear();

        lists_initialized = false;
        CreateDefaultGridLists();
    }

    public void SetCorrectCamera()
    {
        if ( grid_y >= 6)
        {
            perspective_camera.SetActive(false);
            orthographic_camera.SetActive(true);
            active_camera = orthographic_camera.GetComponent<Camera>();
        }
        else
        {
            perspective_camera.SetActive(true);
            orthographic_camera.SetActive(false);
            active_camera = perspective_camera.GetComponent<Camera>();
        }
    }

    public void UIGridButtonClicked(int gridx, int gridy )
    {

        CreateDefaultGridLists();
        //First, check what was in the clicked slot previously
        int current_button_list_index = grid_x * gridy + gridx;
        int previous_object_type = grid_object_type_list[current_button_list_index];
        //int previous_object_color = grid_object_color_list[current_button_list_index];
        //int previous_object_rotation = grid_object_rotations_list[current_button_list_index];

        if (previous_object_type > -1) //The slot is not empty, delete the previous object first then create the new one
        {

            //Delete previous object and delete its data from the lists
            DestroyImmediate(grid_object_type_prefabs_list[current_button_list_index]);
            grid_object_type_prefabs_list[current_button_list_index] = null;
            grid_object_type_list[current_button_list_index] = -1;
            grid_object_rotations_list[current_button_list_index] = -1;
            grid_object_color_list[current_button_list_index] = -1;

        }

        //Then Create the new object if it is not "EMPTY" (If it is empty, we just update the lists and grid button on the editor)
        //If the object to be placed is an obstacle, color is always 1(black)
        if (current_object_type == 0 || current_object_type == 1)
        {
            current_color_type = 1;
        }

        if (current_object_type >= 0)
        {
            GameObject spawned_object = Instantiate(object_prefabs_list[current_object_type]);
            spawned_object.transform.parent = null;
            spawned_object.transform.position = grid_objects[current_button_list_index].transform.position;
            grid_object_type_prefabs_list[current_button_list_index] = spawned_object;
            if (spawned_object.GetComponent<IColorChangeable>() != null)
            {
                if (current_color_type < 2) { current_color_type = 2; }  //We default to the first color(blue) in case the level editor does not change the color setting to one of the valid colors
                spawned_object.GetComponent<IColorChangeable>().ChangeColor(object_materials_list[current_color_type - 2] , current_color_type);
            }
            if (spawned_object.GetComponent<IRotatable>() != null)
            {
                spawned_object.GetComponent<IRotatable>().RotateGameObject(current_rotation_type);
            }

            spawned_object.GetComponent<ILevelObjects>().gridCreator = this;
        }

        grid_object_type_list[current_button_list_index] = current_object_type;
        grid_object_rotations_list[current_button_list_index] = current_rotation_type;
        grid_object_color_list[current_button_list_index] = current_color_type;

        Invoke("CheckIfTilesAreOccupied", 0.05f);
    }


    public void GetCurrentSelectedObjectOptions(int objecttype, int objectcolor, int objectrotation)
    {
        current_object_type = objecttype;
        current_color_type = objectcolor;
        current_rotation_type = objectrotation;
    }

    public void CreateDefaultGridLists()
    {
        if (lists_initialized == false)
        {
            grid_object_type_list.Clear();
            for (int i = 0; i < grid_x * grid_y; i = i + 1)
            {
                grid_object_type_list.Add(-1);
            }

            grid_object_color_list.Clear();
            for (int i = 0; i < grid_x * grid_y; i = i + 1)
            {
                grid_object_color_list.Add(-1);
            }

            grid_object_rotations_list.Clear();
            for (int i = 0; i < grid_x * grid_y; i = i + 1)
            {
                grid_object_rotations_list.Add(-1);
            }

            grid_object_type_prefabs_list.Clear();
            for (int i = 0; i < grid_x * grid_y; i = i + 1)
            {
                grid_object_type_prefabs_list.Add(null);
            }

            lists_initialized = true;
        }
    }

    public Color GetCurrentGridButtonColor(int gridx, int gridy)
    {
        int current_button_list_index = grid_x * gridy + gridx;

        if (grid_object_color_list.Count > current_button_list_index)
        {
            if (grid_object_color_list[current_button_list_index] < 0)  //No color, retrun white
            {
                return Color.white;
            }
            else
            {
                return object_colors_list[grid_object_color_list[current_button_list_index]];
            }
        }
        else
        {
            return Color.white;
        }

    }

    public string GetCurrentGridButtonObjectType(int gridx, int gridy) 
    {
        int current_button_list_index = grid_x * gridy + gridx;

        if (grid_object_type_list.Count > current_button_list_index)
        {
            if (grid_object_type_list[current_button_list_index] <= -1) //Empty
            {
                return "O";
            }
            else if (grid_object_type_list[current_button_list_index] == 0 || grid_object_type_list[current_button_list_index] == 1) //Obstacle
            {
                return "X";
            }
            else if (grid_object_type_list[current_button_list_index] == 2) //Driver
            {
                return "D";
            }
            else if (grid_object_type_list[current_button_list_index] == 3) //Car Size2
            {
                return "C2";
            }
            else if (grid_object_type_list[current_button_list_index] == 4) //Car Size3
            {
                return "C3";
            }

            return "NULL";
        }
        else
        {
            return "NULL";
        }


    }

    public void DelayedTileOccupationCheck()
    {
        Invoke("CheckIfTilesAreOccupied", 0.05f);
    }

    public void CheckIfTilesAreOccupied()
    {
        for (int i = 0; i < grid_objects.Count; i = i + 1)
        {
            grid_objects[i].GetComponent<GridTile>().CheckIfTileIsOccupied();
        }
        Invoke("CheckCarsInWaitingList" , 0.1f);
    }

    public int ReturnGridTileType(int gridtile_index)
    {
        return grid_object_type_list[gridtile_index];
    }

    public GameObject ReturnDriverByGridTileIndex(int gridtileindex)
    {

        //Debug.Log(gridtileindex);
        //Debug.Log(ReturnGridTileType(gridtileindex));
        //Debug.Log(grid_object_type_prefabs_list[gridtileindex]);

        if (ReturnGridTileType(gridtileindex) == 2)  //2 >> driver
        {
            return grid_object_type_prefabs_list[gridtileindex];
        }
        return null;
    }

    public void SaveNewDriverPositionToLists(GridTile old_gridtile, GridTile new_gridtile, GameObject driver)
    {

        int old_tile_index = old_gridtile.grid_coordinate.y * grid_x + old_gridtile.grid_coordinate.x;
        int new_tile_index = new_gridtile.grid_coordinate.y * grid_x + new_gridtile.grid_coordinate.x;

        grid_object_type_prefabs_list[old_tile_index] = null;
        grid_object_type_prefabs_list[new_tile_index] = driver;

        grid_object_type_list[old_tile_index] = -1;
        grid_object_type_list[new_tile_index] = 2;
    }

    public void ClearGridTile(GridTile gridtile, GameObject levelobject)
    {
        int gridtile_index = gridtile.grid_coordinate.y * grid_x + gridtile.grid_coordinate.x;
        grid_object_type_prefabs_list[gridtile_index] = null;
        grid_object_type_list[gridtile_index] = -1;

    }

    public void FillGridTile(GridTile gridtile, GameObject levelobject) //Fill with driver object
    {
        int gridtile_index = gridtile.grid_coordinate.y * grid_x + gridtile.grid_coordinate.x;
        grid_object_type_prefabs_list[gridtile_index] = levelobject;
        grid_object_type_list[gridtile_index] = 2;
    }

    public void DeleteDriverPositionFromLists(GridTile old_gridtile, GameObject driver)
    {
        int old_tile_index = old_gridtile.grid_coordinate.y * grid_x + old_gridtile.grid_coordinate.x;
        grid_object_type_prefabs_list[old_tile_index] = null;
        grid_object_type_list[old_tile_index] = -1;

    }

    public List<GameObject> CarExitRouteWaypoints(int road_side)
    {
        List<GameObject> exit_route_list = new List<GameObject>();
        if (road_side == 0)  //Right side (>>top rigt > top left)
        {
            exit_route_list.Add(top_right_road);
            exit_route_list.Add(top_left_road);
        }
        else if (road_side == 1) //Bottom side (>>bottom left > top left)
        {
            exit_route_list.Add(bottom_left_road);
            exit_route_list.Add(top_left_road);
        }
        else if (road_side == 2) //Top side (>>top left)
        {
            exit_route_list.Add(top_left_road);
        }
        else if (road_side == 3)  //Left side (>>top left)
        {
            exit_route_list.Add(top_left_road);
        }

        return exit_route_list;
    }

    public void RaiseBarrier()
    {
        road_barrier.GetComponent<RoadBlock>().RaiseBarier();
        current_number_of_cars = current_number_of_cars - 1;
        if (current_number_of_cars <= 0)
        {
            playerInput.LockPlayerControls();
            levelCompleteUI.PopupLevelCompleteUI();
            Debug.Log("LEVEL COMPLETE!");

        }

    }

    public void AddCarToWaiitingList(GameObject car)
    {
        cars_in_waiting_list.Add(car);
    }

    public void RemoveCarFromWaitingList(GameObject car)
    {
        cars_in_waiting_list.Remove(car);
    }

    public void CheckCarsInWaitingList()
    {
        Debug.Log("waitingcar:" + cars_in_waiting_list.Count);
        for (int i = 0; i < cars_in_waiting_list.Count; i = i + 1)
        {
            cars_in_waiting_list[i].GetComponent<IDriveable>().RoutineCarMovementCheckAfterEachPlayerAction();
        }
    }
}

