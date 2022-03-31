using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltController : MonoBehaviour
{
    SurfaceEffector2D effector;
    ConveyorRail[] rails;
    Animator[] animators;

    public float Speed
    {
        get => effector.speed;
        set
        {
            effector.speed = value;
            SetRailsSpeed(value);
            SetAnimationSpeed(Mathf.Clamp01(value / 4f));
        }
    }


    private void Awake()
    {
        effector = GetComponent<SurfaceEffector2D>();
        rails = GetComponentsInChildren<ConveyorRail>();
        animators = GetComponentsInChildren<Animator>();
        SetRailsSpeed(Speed);
        SetAnimationSpeed(Mathf.Clamp01(Speed / 4f));
    }

#if UNITY_EDITOR
    private void FixedUpdate()
    {
        SetRailsSpeed(effector.speed);
        SetAnimationSpeed(Mathf.Clamp01(Speed / 4f));
    }
#endif

    void SetRailsSpeed(float speed)
    {
        for (int i = rails.GetLength(0) - 1; i >= 0; i--)
        {
            rails[i].Speed = speed;
        }
    }

    void SetAnimationSpeed(float speed)
    {
        for (int i = animators.GetLength(0) - 1; i >= 0; i--)
        {
            animators[i].speed = speed;
        }
    }
}
