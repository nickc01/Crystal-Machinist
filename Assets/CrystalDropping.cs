using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class CrystalDropping : MonoBehaviour
{
    [SerializeField]
    float activationHeight = 22f;

    [SerializeField]
    Vector2 xRangeMinMax = new Vector2(0f,0f);

    [SerializeField]
    float phase1Lifetime;

    [SerializeField]
    float maxLifetime = 10f;


    [Space]
    [SerializeField]
    float shrinkTime = 0.5f;

    [SerializeField]
    AnimationCurve shrinkCurve;

    public CrystalMachinist Boss { get; set; }

    public bool FadingOut { get; private set; }

    float spawnTime;
    float currentLifeTime;

    public static List<CrystalDropping> SpawnedDroppings = new List<CrystalDropping>();


    private void Start()
    {
        SpawnedDroppings.Add(this);
        StartCoroutine(MainRoutine());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position.With(x: xRangeMinMax.x), transform.position.With(x: xRangeMinMax.y));
    }

    IEnumerator MainRoutine()
    {
        spawnTime = Time.time;

        currentLifeTime = maxLifetime;

        if (Boss.BossStage == 1)
        {
            currentLifeTime = phase1Lifetime;
        }

        yield return new WaitUntil(() => transform.position.y <= activationHeight);

        var collider = GetComponent<PolygonCollider2D>();
        collider.enabled = true;

        while (true)
        {
            var xPos = transform.GetPositionX();

            if (xPos < xRangeMinMax.x || xPos > xRangeMinMax.y)
            {
                break;
            }
            else if (Time.time >= spawnTime + currentLifeTime)
            {
                break;
            }
            else
            {
                yield return null;
            }
        }

        FadingOut = true;
        SpawnedDroppings.Remove(this);

        yield return FadeOutRoutine();
    }

    IEnumerator FadeOutRoutine()
    {
        GetComponent<PlayerDamager>().damageDealt = 0;
        GetComponentInChildren<ParticleSystem>().Stop();

        var rb = GetComponent<Rigidbody2D>();

        if (Time.time >= spawnTime + currentLifeTime)
        {
            rb.isKinematic = true;
        }

        var oldScale = transform.localScale.x;

        for (float i = 0; i < shrinkTime; i += Time.deltaTime)
        {
            var newScale = Mathf.Lerp(oldScale, 0f, shrinkCurve.Evaluate(i / shrinkTime));
            transform.SetLocalScaleXY(newScale, newScale);
            yield return null;
        }


        yield return new WaitForSeconds(1f);

        transform.position = new Vector3(999, 999);
        transform.SetLocalScaleXY(oldScale, oldScale);
        rb.velocity = default;
        rb.isKinematic = false;

        Pooling.Destroy(this);
    }

    public void FadeOut()
    {
        if (!FadingOut)
        {
            SpawnedDroppings.Remove(this);
            FadingOut = true;
            StopAllCoroutines();
            StartCoroutine(FadeOutRoutine());
        }
    }

    private void OnDestroy()
    {
        SpawnedDroppings.Remove(this);
    }
}
