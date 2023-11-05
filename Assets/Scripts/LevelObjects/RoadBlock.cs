using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RoadBlock : MonoBehaviour
{
    public Transform barrier_pivot;
    private bool barrier_already_raised = false;
    private float barrier_raised_time = 0.4f;
    private float current_barrier_raised_time;
    private Tween barrier_lower_tween;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (barrier_already_raised == true)
        {
            current_barrier_raised_time = current_barrier_raised_time - Time.deltaTime;
            if (current_barrier_raised_time <= 0)
            {
                current_barrier_raised_time = 0;
                LowerBarrier();
            }

        }

    }
    public void RaiseBarier()
    {
        if (barrier_already_raised == false)
        {
            barrier_already_raised = true;
            current_barrier_raised_time = barrier_raised_time;
            barrier_pivot.DOLocalRotate(new Vector3(0, 0, 60), 0.15f);
        }
        else if (barrier_already_raised == true) //Extend the duration
        {
            if (barrier_lower_tween != null)
            {
                barrier_lower_tween.Kill();
            }
            current_barrier_raised_time = current_barrier_raised_time + barrier_raised_time * 0.5f;
            barrier_pivot.DOLocalRotate(new Vector3(0, 0, 60), 0.15f);

        }
    }

    public void LowerBarrier()
    {
        barrier_lower_tween = barrier_lower_tween = barrier_pivot.DOLocalRotate(new Vector3(0, 0, 0), 0.15f).OnComplete(() =>
        {
            barrier_already_raised = false;
        });
    }
}
