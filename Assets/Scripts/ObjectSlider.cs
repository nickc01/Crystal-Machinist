using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSlider : MonoBehaviour
{
    //[field: SerializeField]
    //public float Force { get; private set; } = 10.0f;

    //[SerializeField]
    //List<string> objectsToSlide;

    SurfaceEffector2D effector;

    HashSet<Rigidbody2D> collidedObjects = new HashSet<Rigidbody2D>();

    private void Awake()
    {
        effector = GetComponentInParent<SurfaceEffector2D>();
    }

    // Update is called once per frame
    /*void LateUpdate()
    {
        foreach (var obj in collidedObjects)
        {
            obj.MovePosition(obj.position + new Vector2(effector.speed * Time.deltaTime,0f));
            //obj.AddForce(new Vector2(effector.speed * 40.0f, 0f), ForceMode2D.Force);
        }
    }*/

    /*private void OnTriggerEnter2D(Collision2D collision)
    {
        if (objectsToSlide.Contains(collision.gameObject.name))
        {
            collidedObjects.Add(collision.rigidbody);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collidedObjects.Contains(collision.rigidbody))
        {
            collidedObjects.Remove(collision.rigidbody);
        }
    }*/

    /*bool physicsApplied = false;
    float velocityApplied = 0;

    private void FixedUpdate()
    {
        velocityApplied = Force * Time.deltaTime;
        foreach (var obj in collidedObjects)
        {
            obj.velocity += new Vector2(velocityApplied,0f);
        }
        physicsApplied = true;
    }

    private void LateUpdate()
    {
        if (physicsApplied)
        {
            foreach (var obj in collidedObjects)
            {
                obj.velocity -= new Vector2(velocityApplied, 0f);
            }
            physicsApplied = false;
        }
    }*/

    /*private void LateUpdate()
    {
        float velocityApplied = effector.speed * Time.deltaTime;
        foreach (var obj in collidedObjects)
        {
            //obj.AddForce(new Vector2(velocityApplied, 0f),ForceMode2D.Force);
            //obj.AddFo += new Vector2(velocityApplied, 0f);
            obj.velocity += new Vector2(velocityApplied,0f);
        }
    }*/

    HashSet<HeroController> players = new HashSet<HeroController>();

    /*private float SpeedCorrection(float originalSpeed)
    {
        return originalSpeed * (float)(1.05 + (17.13617 * Math.Pow(Math.E,-3.439027 * originalSpeed)));
    }*/

    private float AdjustSpeed(float originalSpeed)
    {
        if (originalSpeed >= 0f)
        {
            return Mathf.Clamp(originalSpeed + 0.35f,0f,2000);
        }
        else
        {
            return Mathf.Clamp(originalSpeed - 0.35f, -2000, 0);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //collidedObjects.Add(collision.rigidbody);

        if (collision.gameObject.TryGetComponent<HeroController>(out var controller) && !players.Contains(controller))
        {
            if (enabled)
            {
                controller.SetConveyorSpeed(effector.speed);
                controller.cState.onConveyor = true;
            }
            players.Add(controller);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<HeroController>(out var controller) && players.Contains(controller))
        {
            if (enabled)
            {
                controller.cState.onConveyor = false;
                controller.cState.onConveyorV = false;
            }
            players.Remove(controller);
        }
    }

    private void FixedUpdate()
    {
        foreach (var player in players)
        {
            if (player.cState.hazardRespawning || player.cState.hazardDeath)
            {
                player.SetConveyorSpeed(0f);
                player.cState.onConveyor = false;
                player.cState.onConveyorV = false;
                player.transform.position += new Vector3(effector.speed * Time.fixedDeltaTime, 0f);
            }
            else
            {
                player.cState.onConveyor = true;
                player.cState.onConveyorV = true;
                player.SetConveyorSpeed(effector.speed);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var player in players)
        {
            player.cState.onConveyor = false;
            player.cState.onConveyorV = false;
        }
    }

    /*private void OnCollisionExit2D(Collision2D collision)
    {
        collidedObjects.Remove(collision.rigidbody);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collidedObjects.Add(collision.attachedRigidbody);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        collidedObjects.Remove(collision.attachedRigidbody);
    }*/

    /*private void OnCollisionStay2D(Collision2D collision)
    {
        collision.rigidbody.AddForce(new Vector2(Force, 0f));
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        collision.attachedRigidbody.AddForce(new Vector2(Force, 0f));
        collision.attachedRigidbody.
        Debug.Log("COlliding = " + collision.gameObject);
    }*/
}
