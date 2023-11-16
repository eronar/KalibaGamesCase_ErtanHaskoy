using cakeslice;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class LO_Driver : MonoBehaviour, IColorChangeable , IDriver , ILevelObjects
{
    [SerializeField] private Transform driver_paintable_parts;
    [SerializeField] private cakeslice.Outline highlightOutline;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform drivertransform;
    [SerializeField] private Transform driver_parent_transform;
    [SerializeField] private ParticleSystem happy_particle;
    [SerializeField] private ParticleSystem angry_particle;

    [SerializeField] private GameObject driver_placeholder_parefab;
    private GameObject active_driver_placeholder;
    [field: SerializeField] public int color_index { get; set; }
    [field: SerializeField] public GridCreator gridCreator { get; set; }

    private List<GridTile> pathtiles = new List<GridTile>();
    private float tile_move_timer = 0.15f; //How long does is take to move from one tile to the next one

    private PlayerInput playerInput;
    private bool player_found_matching_car = false;
    private bool is_entering_left_door;
    private CarDoorHitbox door_to_enter;
    private IDriveable car_to_drive;

    // Start is called before the first frame update
    void Start()
    {
        highlightOutline.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeColor(Material new_material, int colorindex)
    {
        color_index = colorindex;
        Debug.Log("cindex"  + colorindex);
        driver_paintable_parts.GetComponent<SkinnedMeshRenderer>().material = new_material;
    }

    public void SelectDriver()
    {
        HighlightColor1();
        highlightOutline.enabled = true;
        animator.SetTrigger("Wave");
    }

    public void UnselectDriver()
    {
        highlightOutline.enabled = false;
        animator.SetTrigger("Idle");
    }

    public void EnterCar()
    {

        //First, face the driver faces the car
        drivertransform.LookAt(car_to_drive.GetDriverSeatPosition().transform.position);
        door_to_enter.DriverArrivedAtDoorLocation();
        //animator.applyRootMotion = true;
        if (is_entering_left_door == true)
        {
            animator.SetTrigger("EnterCarLeft");
        }
        else if (is_entering_left_door == false)
        {
            animator.SetTrigger("EnterCarRight");
        }

        Vector3 get_in_car_position = door_to_enter.GetComponent<CarDoorHitbox>().car_entry_point.transform.position;
        drivertransform.DOMove(get_in_car_position, 0.3f).OnComplete(() =>
        {
            drivertransform.DOMove(car_to_drive.GetDriverSeatPosition().transform.position, 1f).SetEase(Ease.InSine);
            driver_parent_transform.DOScale(0, 1.4f).SetEase(Ease.InSine).OnComplete(() =>
            {
                GetComponent<BoxCollider>().enabled = false;
                gridCreator.ClearGridTile(pathtiles[pathtiles.Count - 1] , gameObject);
                gridCreator.CheckIfTilesAreOccupied();
                car_to_drive.FirstCheckIfCarIsAbleToMove(playerInput);

                Debug.Log("DriverSeated");
                gameObject.SetActive(false);

            });
        });
    }

    public void MoveToTile(List<GridTile> tile_list, PlayerInput playerinput, CarDoorHitbox matching_door)
    {
        //Unblock the tile the driver is standing on
        tile_list[0].tile_is_empty = true;
        gridCreator.ClearGridTile(tile_list[0], gameObject);
        GetComponent<BoxCollider>().enabled = false;

        if ( matching_door != null) 
        {
            Debug.Log("SAME COLOR DOOR");
            player_found_matching_car = true;
            DriverHappy();
            matching_door.DriverMovingTowardsCar();
            door_to_enter = matching_door;
            is_entering_left_door = matching_door.GetDoorSide();
            car_to_drive = matching_door.GetParentCar();


            //Block the tile the character is going to stand on
            //Or we can also turn the tile into occupied only after the character stops moving
            tile_list[tile_list.Count - 1].tile_is_empty = false;
            gridCreator.FillGridTile(tile_list[tile_list.Count - 1], gameObject);

            if (active_driver_placeholder != null) { Destroy(active_driver_placeholder); }
            active_driver_placeholder = Instantiate(driver_placeholder_parefab, null);
            active_driver_placeholder.transform.position = tile_list[tile_list.Count - 1].transform.position;
        }

        gridCreator.CheckIfTilesAreOccupied();

        pathtiles.Clear();
        for (int i = 0; i < tile_list.Count; i = i +1 )
        {
            pathtiles.Add(tile_list[i]);
        }
        playerInput = playerinput;
        highlightOutline.enabled = false;
        DriverMoving();
        StartCharacterMovement();
    }

    public void DriverHappy()
    {
        happy_particle.Play(true);
        //Debug.Log("HAPPYY!");
    }

    public void DriverAngry()
    {
        angry_particle.Play(true);
        //Debug.Log("ANGRYYY!");
    }

    public void DriverMoving()
    {
        animator.SetTrigger("Run");
    }

    public void DriverStopped()
    {
        drivertransform.eulerAngles = new Vector3(0, 180, 0);
        animator.SetTrigger("Idle");
    }



    public void StartCharacterMovement()
    {
        int current_tile = 1;
        GoToNextTile(current_tile);
    }

    public void GoToNextTile(int current_tile_no)
    {
        drivertransform.LookAt(pathtiles[current_tile_no].transform.position);
        drivertransform.DOMove(pathtiles[current_tile_no].transform.position, tile_move_timer).SetEase(Ease.Linear).OnComplete(() =>
        {
            current_tile_no = current_tile_no + 1;
            if (current_tile_no < pathtiles.Count)
            {
                GoToNextTile(current_tile_no);
            }
            else
            {
                //Character movement complete, inform the playerinput so that it can return to default selection state
                if (player_found_matching_car == true)
                {
                    if (active_driver_placeholder != null) { Destroy(active_driver_placeholder); }
                    gridCreator.ClearGridTile(pathtiles[pathtiles.Count - 1], gameObject);
                    gridCreator.CheckIfTilesAreOccupied();
                    EnterCar();
                    Debug.Log("Driver Getting Into The Car!");
                }
                else
                {
                    DriverStopped();
                    //pathtiles[pathtiles.Count - 1].tile_is_empty = false;

                    //playerInput.ReEnablePlayerControlsAfterDriverMovement();  //This is fron the old system where controls are locked until movement is complete
                    gridCreator.FillGridTile(pathtiles[pathtiles.Count - 1], gameObject);
                    GetComponent<BoxCollider>().enabled = true;
                    if (active_driver_placeholder != null) { Destroy(active_driver_placeholder); }
                    gridCreator.CheckIfTilesAreOccupied();
                    Debug.Log("Movement Complete!");
                }
            }
        });
    }



    public void EnterAdjacentCar(List<GridTile> tile_list, PlayerInput playerinput, CarDoorHitbox matching_door)
    {
        player_found_matching_car = true;
        DriverHappy();
        matching_door.DriverMovingTowardsCar();
        door_to_enter = matching_door;
        is_entering_left_door = matching_door.GetDoorSide();
        car_to_drive = matching_door.GetParentCar();

        pathtiles.Clear();
        for (int i = 0; i < tile_list.Count; i = i + 1)
        {
            pathtiles.Add(tile_list[i]);
        }

        EnterCar();
    }

    public void HighlightColor1()
    {
        highlightOutline.color = 0;
    }
    public void HighlightColor2()
    {
        highlightOutline.color = 1;
    }
    public void EnableHighlight()
    {
        highlightOutline.enabled = true;
        Invoke("DisableHighlight", 1);
    }

    public void DisableHighlight()
    {
        highlightOutline.enabled = false;
    }

}
