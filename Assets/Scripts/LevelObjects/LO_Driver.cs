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
    [field: SerializeField] public int color_index { get; set; }
    [field: SerializeField] public GridCreator gridCreator { get; set; }

    private List<GridTile> pathtiles;
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
                playerInput.DeleteDriverPositionFromLists();
                car_to_drive.FirstCheckIfCarIsAbleToMove(playerInput);

                Debug.Log("DriverSeated");
                gameObject.SetActive(false);

            });
        });
    }

    public void MoveToTile(List<GridTile> tile_list, PlayerInput playerinput, CarDoorHitbox matching_door)
    {
        if ( matching_door != null) 
        {
            Debug.Log("SAME COLOR DOOR");
            player_found_matching_car = true;
            DriverHappy();
            matching_door.DriverMovingTowardsCar();
            door_to_enter = matching_door;
            is_entering_left_door = matching_door.GetDoorSide();
            car_to_drive = matching_door.GetParentCar();
        }

        pathtiles = tile_list;
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
                    EnterCar();
                    Debug.Log("Driver Getting Into The Car!");
                }
                else
                {
                    DriverStopped();
                    playerInput.ReEnablePlayerControlsAfterDriverMovement();
                    Debug.Log("Movement Complete!");
                }
            }
        });
    }

}
