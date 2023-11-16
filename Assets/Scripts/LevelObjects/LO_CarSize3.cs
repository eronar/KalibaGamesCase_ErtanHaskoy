using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LO_CarSize3 : MonoBehaviour, IColorChangeable, IRotatable , IDriveable , ILevelObjects
{
    [SerializeField] private Transform car_paintable_parts_parent;
    [SerializeField] private Transform car_model_parent;
    [SerializeField] private List<cakeslice.Outline> highlightoutline_list;
    [SerializeField] private CarDoorHitbox car_left_door_hitbox;
    [SerializeField] private CarDoorHitbox car_right_door_hitbox;
    [SerializeField] private ParticleSystem smoke_particle;

    [SerializeField] private Transform car_left_door_pivot;
    [SerializeField] private Transform car_right_door_pivot;
    [SerializeField] private GameObject driver_seat;
    [field: SerializeField] public int rotation_index { get; set; }
    [field: SerializeField] public int color_index { get; set; }
    [field: SerializeField] public GridCreator gridCreator { get; set; }
    private int car_departure_state = 0; //0 >> default state, 1 >> has driver, 2 >> departing

    private List<int> car_occupied_grid = new List<int>();
    private PlayerInput playerInput;
    [SerializeField] private LayerMask movement_blockers_layermask;
    [SerializeField] private LayerMask road_layermask;
    private List<GameObject> exit_route_list;
    private float tile_movement_time = 0.15f;

    // Start is called before the first frame update
    void Start()
    {
        DisableHighlight();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeColor(Material new_material, int colorindex)
    {
        color_index = colorindex;
        car_left_door_hitbox.color_index = colorindex;
        car_right_door_hitbox.color_index = colorindex;
        foreach (Transform child in car_paintable_parts_parent)
        {
            child.gameObject.GetComponent<Renderer>().material = new_material;
        }
    }

    public void RotateGameObject(int rotation_type)
    {
        rotation_index = rotation_type;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, rotation_type * 90, transform.eulerAngles.z);
    }

    public void DisableDoorHitboxes() //Disables car door hitboxes once a driver gets inside the car
    {
        car_left_door_hitbox.gameObject.SetActive(false);
        car_right_door_hitbox.gameObject.SetActive(false);
    }

    public void DriverArrivedAtDoorLocation(bool is_left_door)
    {
        //car_departure_state = 1;
        DisableDoorHitboxes();
        if (is_left_door == true)
        {
            Invoke("LeanLeft", 0.8f);
            DOTween.Sequence().AppendInterval(0.4f).Append(car_left_door_pivot.DOLocalRotate(new Vector3(0, 90, 0), 0.3f).SetEase(Ease.InSine)).AppendInterval(0.6f).Append(car_left_door_pivot.DOLocalRotate(new Vector3(0, 0, 0), 0.3f)).AppendInterval(0.2f).OnComplete(() =>
            {

            }).Play();
        }
        else if (is_left_door == false)
        {
            Invoke("LeanRight", 0.8f);
            DOTween.Sequence().AppendInterval(0.4f).Append(car_right_door_pivot.DOLocalRotate(new Vector3(0, -90, 0), 0.3f).SetEase(Ease.InSine)).AppendInterval(0.6f).Append(car_right_door_pivot.DOLocalRotate(new Vector3(0, 0, 0), 0.3f)).AppendInterval(0.2f).OnComplete(() =>
            {

            }).Play();
        }
    }

    public void EnableGreenHighlight()
    {
        HighlightColor1();
        for (int i = 0; i < highlightoutline_list.Count; i = i + 1)
        {
            highlightoutline_list[i].enabled = true;
        }
        Invoke("DisableHighlight", 1);
    }

    public void EnableHighlight()
    {
        for (int i = 0; i < highlightoutline_list.Count; i = i + 1)
        {
            highlightoutline_list[i].enabled = true;
        }
        Invoke("DisableHighlight", 1);
    }

    public void DisableHighlight()
    {
        for (int i = 0; i < highlightoutline_list.Count; i = i + 1)
        {
            highlightoutline_list[i].enabled = false;
        }
    }

    public GameObject GetDriverSeatPosition()
    {
        return driver_seat;
    }

    public GameObject GetCarPosition()
    {
        return gameObject;
    }

    public void CarReadyToDrive()
    {
        car_departure_state = 1;
    }

    public void FirstCheckIfCarIsAbleToMove(PlayerInput playerinput)
    {
        playerInput = playerinput;
        CheckCheckIfCarIsAbleToMove();
    }

    public bool CheckCheckIfCarIsAbleToMove()
    {
        //Check forward and backward to see if there are any obstacles on the way

        if (Physics.Raycast(transform.position, -transform.forward, out RaycastHit hitf, 100, movement_blockers_layermask))
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitb, 100, movement_blockers_layermask))
            {
                //Car is unable to move forward or backward, reactivate player controls
                playerInput.ReEnablePlayerControls();
                if (car_departure_state == 0)
                {
                    car_departure_state = 1;
                    gridCreator.AddCarToWaiitingList(gameObject);
                }
                return false;
            }
            else
            {
                Debug.Log("Backward open");
                MoveBackwardToRoad();
                return true;
            }
        }
        else
        {
            Debug.Log("Forward open");
            MoveForwardToRoad();
            return true;
        }

    }

    public void MoveForwardToRoad()
    {
        ActivateExhaustParticleAndRaiseFront();
        car_departure_state = 2;
        if (Physics.Raycast(transform.position, -transform.forward, out RaycastHit hit, 100, road_layermask))
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            transform.DOMove(transform.position, 0.2f).OnComplete(() => //This is just a delay to make the movement more impactful
            {
                transform.DOMove(hit.transform.position, distance * tile_movement_time).SetEase(Ease.Linear).OnComplete(() =>
                {
                    MoveToExit(hit.collider.GetComponent<RoadTile>().road_side);
                });
            });

        }

    }

    public void MoveBackwardToRoad()
    {
        ActivateExhaustParticleAndRaiseFront();
        car_departure_state = 2;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100, road_layermask))
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            transform.DOMove(hit.transform.position, distance * tile_movement_time).SetEase(Ease.Linear).OnComplete(() =>
            {
                MoveToExit(hit.collider.GetComponent<RoadTile>().road_side);
            });
        }
    }

    public void MoveToExit(int road_side)
    {
        GetComponent<BoxCollider>().enabled = false;
        exit_route_list = gridCreator.CarExitRouteWaypoints(road_side);
        if (exit_route_list.Count == 1) { UnlockPlayerControls(); gridCreator.CheckIfTilesAreOccupied(); }
        MoveToNextExitWaypoint(0);
    }

    public void MoveToNextExitWaypoint(int current_waypoint_index)
    {
        Vector3 next_waypoint_position = exit_route_list[current_waypoint_index].transform.position;
        float distance = Vector3.Distance(transform.position, next_waypoint_position);
        transform.DOLookAt((2 * transform.position - next_waypoint_position), 0.15f);
        transform.DOMove(next_waypoint_position, distance * tile_movement_time).SetEase(Ease.Linear).OnComplete(() =>
        {
            current_waypoint_index = current_waypoint_index + 1;
            if (exit_route_list.Count - current_waypoint_index == 1) { UnlockPlayerControls(); gridCreator.CheckIfTilesAreOccupied(); }
            if (current_waypoint_index >= exit_route_list.Count)
            {
                Debug.Log("Checkpoint reached");
                //Open the barrier and leave the game area 
                gridCreator.RaiseBarrier();
                next_waypoint_position = transform.position + new Vector3(0, 0, 25);
                transform.DOLookAt((2 * transform.position - next_waypoint_position), 0.15f);
                transform.DOMove(next_waypoint_position, 25 * tile_movement_time * 0.6f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    Debug.Log("car outside game field");
                });
            }
            else
            {
                MoveToNextExitWaypoint(current_waypoint_index);
            }
        });
    }

    public void UnlockPlayerControls()
    {
        playerInput.ReEnablePlayerControls();
    }

    public void ActivateExhaustParticleAndRaiseFront()
    {
        car_model_parent.transform.DOLocalMoveY(car_model_parent.transform.localPosition.y + 0.1f, 0.2f);
        car_model_parent.DOLocalRotate(new Vector3(-12, car_model_parent.localEulerAngles.y, car_model_parent.localEulerAngles.z), 0.1f).OnComplete(() =>
        {
            car_model_parent.DOLocalRotate(new Vector3(-8, car_model_parent.localEulerAngles.y, car_model_parent.localEulerAngles.z), 0.2f).SetEase(Ease.OutBack);
        });
        smoke_particle.Play(true);
    }

    public void LeanLeft()
    {
        transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, -10), 0.4f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0), 0.5f).SetEase(Ease.OutElastic);
        });
    }

    public void LeanRight()
    {
        transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 10), 0.4f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            transform.DOLocalRotate(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0), 0.5f).SetEase(Ease.OutElastic);
        });
    }

    public void RoutineCarMovementCheckAfterEachPlayerAction()
    {
        if (car_departure_state == 1 && playerInput.character_selection_state != 2)
        {
            if (CheckCheckIfCarIsAbleToMove() == true)
            {
                gridCreator.RemoveCarFromWaitingList(gameObject);
                playerInput.LockPlayerControls();
            }
        }
    }
    public void HighlightColor1()
    {
        for (int i = 0; i < highlightoutline_list.Count; i = i + 1)
        {
            highlightoutline_list[i].color = 0;
        }
    }
    public void HighlightColor2()
    {
        for (int i = 0; i < highlightoutline_list.Count; i = i + 1)
        {
            highlightoutline_list[i].color = 1;
        }
    }

    public Vector3 GetDoorPosition1()
    {
        return car_left_door_hitbox.gameObject.transform.position;
    }

    public Vector3 GetDoorPosition2()
    {
        return car_right_door_hitbox.gameObject.transform.position;
    }

    public CarDoorHitbox GetDoorHitbox1()
    {
        return car_left_door_hitbox;
    }

    public CarDoorHitbox GetDoorHitbox2()
    {
        return car_right_door_hitbox;
    }


}
