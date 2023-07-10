using GlobalEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;

[ExecuteAlways]
public class Stomper : MonoBehaviour
{


    public enum State
    {
        Idle,
        PreSmashing,
        Smashing,
        Retracting
    }

    [SerializeField]
    float smashAcceleration = 50f;

    [SerializeField]
    float smashSpeed = 10f;

    [SerializeField]
    float retractSpeed = 8f;

    [SerializeField]
    float prepareAcceleration = 2f;

    [SerializeField]
    float prepareHeight = 0.5f;


    float retractedHeight = 0f;

    [SerializeField]
    GameObject PlayerAttackObject;

    [SerializeField]
    GameObject EnemyAttackObject;

    [SerializeField]
    LayerMask detectionMask;

    [SerializeField]
    float bodyColliderOffset;

    [SerializeField]
    float smashIdleTime = 0.2f;

    [field: SerializeField]
    public float DamageHeightThreshold { get; set; } = 14;

    [SerializeField]
    AudioClip smashSound;

    //[SerializeField]
    //Sprite slammingFrame;

    public State CurrentState { get; private set; } = State.Idle;

    [NonSerialized]
    SpriteRenderer mainRenderer;

    SpriteRenderer body;

    BoxCollider2D bodyCollider;

    SpriteRenderer bottom;

    Rigidbody2D rb;

    Coroutine smashRoutine;

    float? heightToReach;

    //[NonSerialized]
    //Collider2D lastCollision;

    //[NonSerialized]
    //HashSet<Collision2D> collidedObjects = new HashSet<Collision2D>();

    [NonSerialized]
    float previousSize = 0f;

    //[NonSerialized]
    //Sprite defaultFrame;

    MaterialPropertyBlock blurBlock;

    Collider2D heroCollider;

    [SerializeField]
    ParticleSystem smashParticles;

    [SerializeField]
    GameObject crystalDestroyer;

    [SerializeField]
    CollisionCounter hitDetector;

    float maximumHeight;
    float maximumSpriteHeight;

    public float RetractTime { get; private set; } = 0f;

    private bool Blurry
    {
        get
        {
            if (blurBlock == null)
            {
                blurBlock = new MaterialPropertyBlock();
            }

            body.GetPropertyBlock(blurBlock);

            return blurBlock.GetInt("_BlurSamples") > 0;
        }

        set
        {
            if (blurBlock == null)
            {
                blurBlock = new MaterialPropertyBlock();
            }
            body.GetPropertyBlock(blurBlock);

            blurBlock.SetInt("_BlurSamples", value ? 3 : 0);

            body.SetPropertyBlock(blurBlock);

            bottom.GetPropertyBlock(blurBlock);

            blurBlock.SetInt("_BlurSamples", value ? 3 : 0);

            bottom.SetPropertyBlock(blurBlock);
        }
    }

    private void Awake()
    {

        //mainRenderer.GetPropertyBlock(blurBlock);

        maximumHeight = transform.position.y;
        rb = GetComponent<Rigidbody2D>();
        mainRenderer = GetComponent<SpriteRenderer>();
        body = transform.Find("Body").GetComponent<SpriteRenderer>();
        bottom = transform.Find("Bottom").GetComponent<SpriteRenderer>();
        bodyCollider = body.GetComponent<BoxCollider2D>();

        maximumSpriteHeight = maximumHeight + mainRenderer.size.y;

        rb.isKinematic = true;

        //defaultFrame = mainRenderer.sprite;
        if (Application.isPlaying)
        {
            StartCoroutine(SmashTest());
        }
    }

    IEnumerator SmashTest()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            //Smash();
        }
    }


    private void LateUpdate()
    {
        if (mainRenderer == null)
        {
            Awake();
        }

        float newSize = mainRenderer.size.y - body.transform.localPosition.y;
        float colliderSize = newSize - bodyColliderOffset;

        if (!Application.isPlaying || newSize != previousSize)
        {
            previousSize = newSize;

            body.size = new Vector2(body.size.x, newSize);
            bodyCollider.size = new Vector2(bodyCollider.size.x, colliderSize);
            bodyCollider.offset = new Vector2(bodyCollider.offset.x, (colliderSize / 2f) + bodyColliderOffset);
        }
    }

    /*public void Smash()
    {
        Smash();
    }*/

    public bool Smash()
    {
        if (CurrentState == State.Smashing || CurrentState == State.PreSmashing)
        {
            return false;
        }

        if (smashRoutine != null)
        {
            StopCoroutine(smashRoutine);
        }
        rb.velocity = default;
        rb.isKinematic = false;
        crystalDestroyer.SetActive(true);
        CurrentState = State.PreSmashing;
        smashRoutine = StartCoroutine(SmashRoutine());

        return true;
    }

    IEnumerator SmashRoutine()
    {
        Blurry = false;
        //mainRenderer.sprite = defaultFrame;
        //Debug.Log("STARTING SMASH");
        if (heightToReach == null)
        {
            heightToReach = transform.position.y;

            if (heightToReach.Value > maximumHeight)
            {
                heightToReach = maximumHeight;
            }
        }

        var prepareDestHeight = Mathf.Min(heightToReach.Value, transform.position.y) + prepareHeight;

        //var bodyRendererHeight = heightToReach.Value + mainRenderer.size.y;
        //var bodyRendererHeight = heightToReach.Value - transform.position.y;

        float upVelocity = 0f;

        rb.isKinematic = true;
        rb.velocity = default;

        //Debug.Log("RISING");
        while (transform.position.y < prepareDestHeight)
        {
            upVelocity += prepareAcceleration * Time.deltaTime;
            transform.SetPositionY(transform.position.y + (upVelocity * Time.deltaTime));
            mainRenderer.size = new Vector2(mainRenderer.size.x, maximumSpriteHeight - transform.position.y);
            yield return null;
        }

        //Debug.Log("FALLING");
        Blurry = true;
        //collidedObjects.Clear();
        //lastCollision = null;

        float currentVelocity = 0f;

        float minimumResponseThreshold = smashSpeed / 1.75f;

        rb.velocity = default;
        rb.isKinematic = false;

        float startTime = Time.time;

        while (rb.velocity.y > -minimumResponseThreshold)
        {
            currentVelocity += smashAcceleration * Time.deltaTime;
            rb.velocity = new Vector2(0f, -currentVelocity);
            mainRenderer.size = new Vector2(mainRenderer.size.x, maximumSpriteHeight - transform.position.y);
            if (Time.time >= startTime + 0.15f)
            {
                CurrentState = State.Smashing;
            }
            yield return null;
        }

        CurrentState = State.Smashing;

        float playerHitDelayTimer = 0f;

        var playerLayer = LayerMask.NameToLayer("Player");
        var heroBoxLayer = LayerMask.NameToLayer("Hero Box");

        while (rb.velocity.y < -smashSpeed / 2f)
        //while (!(transform.position.y < DamageHeightThreshold && rb.velocity.y >= -smashSpeed / 2f) || rb.velocity.y < 0f)
        {
            if (hitDetector.CollidedObjects.Any(obj => obj.gameObject.layer == playerLayer || obj.gameObject.layer == heroBoxLayer))
            {
                playerHitDelayTimer += Time.deltaTime;

                if (playerHitDelayTimer >= 0.1f)
                {
                    break;
                }
            }
            else
            {
                playerHitDelayTimer = 0f;
            }

            float oldVelocity = currentVelocity;
            currentVelocity += smashAcceleration * Time.deltaTime;
            if (currentVelocity > smashSpeed)
            {
                currentVelocity = smashSpeed;
            }
            rb.velocity -= new Vector2(0f,currentVelocity - oldVelocity);
            mainRenderer.size = new Vector2(mainRenderer.size.x, maximumSpriteHeight - transform.position.y);
            yield return null;
        }



        /*do
        {

        } while (rb.velocity.y < -currentVelocity / 2f);*/

        /*while (startingVelocity < smashSpeed)
        {
            startingVelocity += smashAcceleration * Time.deltaTime;
            rb.velocity = new Vector2(0f, -startingVelocity);
            mainRenderer.size = new Vector2(mainRenderer.size.x, bodyRendererHeight - transform.position.y);
            yield return null;
        }

        rb.velocity = new Vector2(0f,-smashSpeed);
        //mainRenderer.sprite = slammingFrame;

        while (rb.velocity.y < -smashSpeed / 2f)
        {
            mainRenderer.size = new Vector2(mainRenderer.size.x, bodyRendererHeight - transform.position.y);
            yield return null;
        }*/

        Blurry = false;

        smashParticles.Play();
        WeaverAudio.PlayAtPoint(smashSound, transform.position, 0.5f);

        var heroPos = HeroController.instance.transform.position;

        //CameraShaker.Instance.Shake(new Vector3(0.1f, 0.1f, 0.1f), 0.2f);

        //WeaverLog.Log("LAST COLLISION = " + lastCollision.gameObject);
        //WeaverLog.Log("LAST COLLISION LAYER = " + lastCollision.gameObject.layer);

        //var heroController = lastCollision.GetComponentInParent<HeroController>();

        //if (heroPos.y >= transform.position.y - 2f && Mathf.Abs(heroPos.x - transform.position.x) <= 2.25f)
        //if (lastCollision.gameObject.GetComponentInParent<HeroController>() != null)

        bool hitSomething = false;

        var hitObjects = hitDetector.CollidedObjects.ToList();

        foreach (var lastCollision in hitObjects)
        {
            if (lastCollision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (lastCollision.gameObject.GetComponentInParent<HeroController>() != null)
                {
                    ForcePlayerDamager.ForceDamage(HeroController.instance, PlayerAttackObject, CollisionSide.top, 2, HazardType.REGULAR_HIT);
                }

                //yield return new WaitForSeconds(0.1f);
                for (float t = 0; t < 0.1f; t += Time.deltaTime)
                {
                    mainRenderer.size = new Vector2(mainRenderer.size.x, maximumSpriteHeight - transform.position.y);
                    yield return null;
                }
                //PlayerAttackObject.SetActive(false);
            }
            else if (lastCollision.gameObject.layer == LayerMask.NameToLayer("Enemies"))
            {
                HitTaker.Hit(lastCollision.transform, gameObject, 99999, AttackType.Nail, CardinalDirection.Down);
                //EnemyAttackObject.SetActive(true);
                //yield return new WaitForSeconds(0.1f);
                for (float t = 0; t < 0.1f; t += Time.deltaTime)
                {
                    mainRenderer.size = new Vector2(mainRenderer.size.x, maximumSpriteHeight - transform.position.y);
                    yield return null;
                }
                //EnemyAttackObject.SetActive(false);
            }
        }

        if (!hitSomething)
        {
            for (float t = 0; t < smashIdleTime; t += Time.deltaTime)
            {
                mainRenderer.size = new Vector2(mainRenderer.size.x, maximumSpriteHeight - transform.position.y);
                yield return null;
            }
            //yield return new WaitForSeconds(smashIdleTime);
        }

        /*if (lastCollision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (lastCollision.GetComponentInParent<HeroController>() != null)
            {
                ForcePlayerDamager.ForceDamage(HeroController.instance, PlayerAttackObject, CollisionSide.top, 2, HazardType.REGULAR_HIT);
            }

            yield return new WaitForSeconds(0.1f);
            //PlayerAttackObject.SetActive(false);
        }
        else if (lastCollision.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            HitTaker.Hit(lastCollision.transform, gameObject, 99999, AttackType.Nail, CardinalDirection.Down);
            //EnemyAttackObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            //EnemyAttackObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(smashIdleTime);
        }*/

        //yield return new WaitUntil(() => rb.velocity.y >= -smashSpeed / 2f);

        //TODO : DO DIFFERENT BEHAVIOUR IF THE LAST COLLIDER IS A PLAYER OR A FLOOR

        //mainRenderer.sprite = defaultFrame;

        //rb.isKinematic = true;
        //rb.velocity = default;

        /*if (lastCollision != null && lastCollision.gameObject.layer == LayerMask.NameToLayer("Player") || lastCollision.gameObject.layer == LayerMask.NameToLayer("Hero Box"))
        {
            PlayerAttackObject.SetActive(true);

            yield return new WaitForSecondsRealtime(0.2f);
            PlayerAttackObject.SetActive(false);
        }
        else
        {
            //PLAY SLAM EFFECTS
            yield return new WaitForSeconds(1f);
        }*/

        RetractTime = 0f;

        CurrentState = State.Retracting;
        //rb.isKinematic = false;

        rb.isKinematic = true;
        rb.velocity = default;

        //Debug.Log("RISING UP");

        while (transform.position.y < heightToReach.Value)
        {
            transform.SetPositionY(transform.position.y + (retractSpeed * Time.deltaTime));
            mainRenderer.size = new Vector2(mainRenderer.size.x, maximumSpriteHeight - transform.position.y);
            RetractTime += Time.deltaTime;
            yield return null;
        }

        //Debug.Log("DONE");
        //yield return new WaitUntil(() => transform.position.y >= heightToReach.Value);

        transform.SetPositionY(heightToReach.Value);
        mainRenderer.size = new Vector2(mainRenderer.size.x, maximumSpriteHeight - transform.position.y);
        CurrentState = State.Idle;
        heightToReach = null;
        smashRoutine = null;

        rb.velocity = default;
        rb.isKinematic = true;
        crystalDestroyer.SetActive(false);

        /*for (float t = 0; t < prepareTime; t += Time.deltaTime)
        {
            transform.SetPositionY()
        }*/
    }

    public bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << obj.layer)) > 0);
    }

    public bool PlayerUnderneathStomper()
    {
        return Player.Player1.transform.position.y <= transform.position.y && Mathf.Abs(Player.Player1.transform.position.x - transform.position.x) <= 2.25f;
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject,detectionMask))
        {
            //lastCollision = collision.collider;
            collidedObjects.Add(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject, detectionMask))
        {
            collidedObjects.Remove(collision);
            //lastCollision = collision.collider;
        }
    }*/
}
