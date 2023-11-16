using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GridTile : MonoBehaviour
{
    public string grid_identifier;
    public Vector2Int grid_coordinate; //The coordinate of the grid unit inside the grid
    public TextMeshProUGUI index_text;
    [SerializeField] private Transform tiletransform;
    [SerializeField] LayerMask movement_blockers_layermask;

    [SerializeField] private SpriteRenderer tilehighlight_renderer;


    public int g_cost = int.MaxValue;
    public int h_cost = 0;
    public int f_cost = 0;
    public bool tile_is_empty = true;
    public GameObject occupying_car = null;

    public GridTile pathfinding_parent;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetGridSpecifications(string identifier, Vector2Int coordinate)
    {
        grid_identifier = identifier;
        grid_coordinate = coordinate;
    }

    public void ResetPathfindingVariables()
    {
        g_cost = int.MaxValue;
        h_cost = 0;
        f_cost = 0;
        pathfinding_parent = null;
        CalculateFCost();

    }

    public void CalculateFCost()
    {
        f_cost = g_cost + h_cost;
    }

    public void ShowIndexText(int index)
    {
        index_text.enabled = true;
        index_text.text = index.ToString();
    }

    public void CheckIfTileIsOccupied()
    {
        Collider[] hitColliders = Physics.OverlapBox(tiletransform.position, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, movement_blockers_layermask);
        if (hitColliders.Length > 0)  //Lets check the first element only for now, there shouldn't be overlapping objects normally
        {
            if (hitColliders[0].GetComponent<IDriveable>() != null)  //Tile is not empty but there is a car, we should check the car color and car door tile accessibility
            {
                tile_is_empty = false;
                occupying_car = hitColliders[0].gameObject;
            }
            else  //Tile is not empty and there is not a car there, 
            {
                tile_is_empty = false;
                occupying_car = null;
                //tiletransform.GetChild(0).gameObject.SetActive(false);
            }
        }
        else
        {
            tile_is_empty = true;
            occupying_car = null;
            //tiletransform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void TileIsAccessible()
    {
        tilehighlight_renderer.enabled = true;
        tilehighlight_renderer.color = new Color(0, 1, 0, 0);
        DOTween.Sequence().Append(tilehighlight_renderer.DOFade(1, 0.3f).SetEase(Ease.InSine)).AppendInterval(0.2f).Append(tilehighlight_renderer.DOFade(0, 0.3f).SetEase(Ease.InSine).OnComplete(() =>
        {
            tilehighlight_renderer.enabled = false;
        })).Play();
    }

    public void TileIsInaccessible()
    {
        tilehighlight_renderer.enabled = true;
        tilehighlight_renderer.color = new Color(1, 0, 0, 0);
        DOTween.Sequence().Append(tilehighlight_renderer.DOFade(1, 0.3f).SetEase(Ease.InSine)).AppendInterval(0.2f).Append(tilehighlight_renderer.DOFade(0, 0.3f).SetEase(Ease.InSine).OnComplete(() =>
        {
            tilehighlight_renderer.enabled = false;
        })).Play();
    }

    public GameObject ReturnObjectOnTop()
    {
        Collider[] hitColliders = Physics.OverlapBox(tiletransform.position, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, movement_blockers_layermask);
        if (hitColliders.Length > 0)
        {
            return hitColliders[0].gameObject;
        }
        return null;
    }

}
