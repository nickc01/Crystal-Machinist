using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class CrystalShot : MonoBehaviour, IHittable, IOnPool
{
    [SerializeField]
    AudioClip growthSound;

    [SerializeField]
    List<AudioClip> breakSounds;

    [SerializeField]
    Vector2 breakSoundsPitchRange = new Vector2(0.8f, 1.2f);


    GameObject growthObject;
    ParticleSystem hitCrystals;
    PolygonCollider2D growthCollider;
    BoxCollider2D mainCollider;
    SpriteRenderer mainRenderer;
    Rigidbody2D rb2d;
    WeaverAnimationPlayer animator;
    CollisionCounter terrainDetector;
    EnemyDamager damager;

    SpriteFlasher growthFlasher;

    //bool hitTerrain = false;
    bool hitTerrain => terrainDetector.CollidedObjectCount > 0;
    bool nailHit = false;
    bool despawnImmediate = false;

    HitInfo lastHit;

    int enemyDamage = 0;

    public bool Hit(HitInfo hit)
    {
        lastHit = hit;
        nailHit = true;

        return true;
    }

    private void Awake()
    {
        if (rb2d == null)
        {
            growthObject = transform.Find("Growth").gameObject;
            growthFlasher = growthObject.GetComponent<SpriteFlasher>();
            animator = growthObject.GetComponent<WeaverAnimationPlayer>();
            hitCrystals = GetComponentInChildren<ParticleSystem>();
            growthCollider = GetComponent<PolygonCollider2D>();
            mainRenderer = GetComponent<SpriteRenderer>();
            mainCollider = GetComponent<BoxCollider2D>();
            rb2d = GetComponent<Rigidbody2D>();
            terrainDetector = GetComponentInChildren<CollisionCounter>();
            damager = GetComponent<EnemyDamager>();
        }

        enemyDamage = damager.damage;
        damager.damage = 0;
        nailHit = false;

        growthObject.SetActive(false);
        growthCollider.enabled = false;
        mainRenderer.enabled = true;
        mainCollider.enabled = true;

        rb2d.isKinematic = false;
        transform.localScale = Vector3.one;
        hitCrystals.Stop();


        StartCoroutine(MainRoutine());
    }

    IEnumerator MainRoutine()
    {
        float lastHitTime = float.NegativeInfinity;

        while (true)
        {
            while (!hitTerrain && !nailHit)
            {
                //Vector2 velocity = rb2d.velocity;
                //float z = Mathf.Atan2(velocity.y, velocity.x) * (180f / Mathf.PI) + 90f;
                //transform.localEulerAngles = new Vector3(0f, 0f, z);

                float velocityAngle = MathUtilities.CartesianToPolar(rb2d.velocity).x;

                transform.SetZLocalRotation(velocityAngle + 90f);

                /*instance.GetComponent<Rigidbody2D>().velocity = ((laserMove.EmitterTransform.rotation * extraRotation) * Vector2.right) * shotVelocity;*/

                yield return null;
            }
            //yield return new WaitUntil(() => hitTerrain || nailHit);

            if (hitTerrain)
            {
                nailHit = false;
                break;
            }
            else if (nailHit)
            {
                nailHit = false;

                if (Time.time >= lastHitTime + 0.15f)
                {
                    var x = 30f * Mathf.Cos(lastHit.Direction * (Mathf.PI / 180f));
                    var y = 30f * Mathf.Sin(lastHit.Direction * (Mathf.PI / 180f));
                    rb2d.velocity = new Vector2(x, y);

                    float velocityAngle = MathUtilities.CartesianToPolar(rb2d.velocity).x;

                    transform.SetZLocalRotation(velocityAngle + 90f);

                    lastHitTime = Time.time;
                }

                //yield return new WaitForSeconds(0.15f);
            }
            else if (despawnImmediate)
            {
                break;
            }
        }

        yield return LandRoutine();



        yield break;
    }

    IEnumerator LandRoutine()
    {
        growthObject.SetActive(true);
        mainRenderer.enabled = false;

        if (!despawnImmediate && growthSound != null)
        {
            WeaverAudio.PlayAtPoint(growthSound, transform.position);
        }

        if (!despawnImmediate)
        {
            animator.PlayAnimation("Crystal Grow");
        }

        rb2d.velocity = default;
        rb2d.isKinematic = true;

        transform.SetRotationZ(UnityEngine.Random.Range(0f,360f));

        var scale = UnityEngine.Random.Range(0.9f, 1.3f);

        transform.localScale = new Vector3(scale, scale, scale);

        yield return new WaitForSeconds(0.13f);

        growthCollider.enabled = true;
        growthFlasher.flashFocusHeal();

        var waitTime = UnityEngine.Random.Range(10f, 12f);

        for (float t = 0; t < waitTime; t += Time.deltaTime)
        {
            if (nailHit)
            {
                if (lastHit.Damage > 0)
                {
                    break;
                }
                else
                {
                    nailHit = false;
                }
            }

            if (despawnImmediate)
            {
                break;
            }
            yield return null;
        }

        if (nailHit)
        {
            CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);
        }

        foreach (var sound in breakSounds)
        {
            var instance = WeaverAudio.PlayAtPoint(sound, transform.position);
            instance.AudioSource.pitch = breakSoundsPitchRange.RandomInRange();
        }

        Pooling.Instantiate(EffectAssets.NailStrikePrefab, growthObject.transform.position, Quaternion.identity);

        hitCrystals.Play();

        growthCollider.enabled = false;
        mainCollider.enabled = false;

        yield return animator.PlayAnimationTillDone("Crystal Smash");

        hitCrystals.Stop();

        growthObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        Pooling.Destroy(this);



        /*if (nailHit)
        {
            //COLLIDE
        }
        else
        {
            //SMASH
        }*/

        yield break;
    }

    public void DespawnImmediate()
    {
        despawnImmediate = true;
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            hitTerrain = true;
        }
    }*/

    void IOnPool.OnPool()
    {
        damager.damage = enemyDamage;
    }
}
