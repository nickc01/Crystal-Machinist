using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using WeaverCore;
using WeaverCore.Utilities;

public class RoarMove : CrystalMachinistMove
{
    [SerializeField]
    float roarTime = 0.75f;

    [SerializeField]
    List<GameObject> CrystalsToSpawn;

    [SerializeField]
    float crystalSpawnDelay = 0.1f;

    [SerializeField]
    float crystalSpawnRate = 0.2f;

    [SerializeField]
    int crystalsToSpawn = 4;

    [SerializeField]
    int crystalsToSpawnPhase2 = 6;

    [SerializeField]
    float crystalSpawnHeight = 24f;

    [SerializeField]
    Vector2 crystalXSpawnRangeMinMax = new Vector2(0f,0f);

    [SerializeField]
    List<Vector2> spawnZones;

    ParticleSystem crystalRain;

    public float LastRoarTime { get; private set; } = 0f;
    //ParticleSystem landDust;


    public override bool MoveEnabled => !HeroController.instance.cState.spellQuake;

    RoarEmitter roarEmitter;

    private void Awake()
    {
        crystalRain = transform.Find("Crystal Rain").GetComponent<ParticleSystem>();
        crystalRain.transform.SetParent(null, true);
        //landDust = transform.Find("Land Dust").GetComponent<ParticleSystem>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position.With(x: crystalXSpawnRangeMinMax.x) + Vector3.up, transform.position.With(x: crystalXSpawnRangeMinMax.y) + Vector3.up);

        foreach (var zone in spawnZones)
        {
            Gizmos.DrawLine(new Vector3(zone.x, crystalSpawnHeight + 1f),new Vector3(zone.x,crystalSpawnHeight - 1f));
            Gizmos.DrawLine(new Vector3(zone.y, crystalSpawnHeight + 1f),new Vector3(zone.y,crystalSpawnHeight - 1f));
        }
    }

    IEnumerator SpawnCrystals()
    {
        int spawnCount = crystalsToSpawn;
        if (Boss.BossStage >= 2)
        {
            spawnCount = crystalsToSpawnPhase2;
        }

        yield return new WaitForSeconds(crystalSpawnDelay);

        var zones = new List<Vector2>(spawnZones);

        List<GameObject> spawnedCrystals = new List<GameObject>();

        for (int i = 0; i < spawnCount; i++)
        {
            var obj = Pooling.Instantiate(CrystalsToSpawn[UnityEngine.Random.Range(0,CrystalsToSpawn.Count)],default(Vector3),Quaternion.Euler(0f,0f,UnityEngine.Random.Range(0f,360f)));
            var collider = obj.GetComponent<PolygonCollider2D>();
            collider.enabled = false;
            obj.GetComponent<CrystalDropping>().Boss = Boss;



            obj.transform.position = new Vector3(UnityEngine.Random.Range(crystalXSpawnRangeMinMax.x, crystalXSpawnRangeMinMax.y),crystalSpawnHeight);

            var zone = zones.GetRandomElement();
            zones.Remove(zone);

            if (obj.transform.position.x > zone.y)
            {
                obj.transform.SetPositionX(zone.y);
            }

            if (obj.transform.position.x < zone.x)
            {
                obj.transform.SetPositionX(zone.x);
            }

            spawnedCrystals.Add(obj);

            if (i > 0)
            {
                obj.SetActive(false);
                StartCoroutine(EnableAfter(obj, crystalSpawnRate * i));
            }

            //yield return new WaitForSeconds(crystalSpawnRate);
        }

        var nearest = spawnedCrystals.OrderBy(g => Vector3.Distance(g.transform.position, Player.Player1.transform.position)).FirstOrDefault();

        if (nearest != null)
        {
            nearest.transform.SetPositionX(Player.Player1.transform.position.x);

            if (nearest.transform.position.x > crystalXSpawnRangeMinMax.y)
            {
                nearest.transform.SetPositionX(crystalXSpawnRangeMinMax.y);
            }

            if (nearest.transform.position.x < crystalXSpawnRangeMinMax.x)
            {
                nearest.transform.SetPositionX(crystalXSpawnRangeMinMax.x);
            }
        }
    }

    IEnumerator EnableAfter(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(true);
    }

    public override IEnumerator DoMove()
    {
        if (Time.time < LastRoarTime + 3f)
        {
            yield break;
        }
        yield return Roar(true,true);
    }

    public IEnumerator Roar(bool spawnCrystals, bool bigRoar)
    {
        LastRoarTime = Time.time;
        Boss.LastMoveRoar = true;
        Animator.PlayAnimation("Roar Start");

        /*if (shotLaser)
        {
            EventManager.BroadcastEvent("LASER SHOOT", gameObject);
        }
        shotLaser = true;*/
        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.BigShake);
        WeaverAudio.PlayAtPoint(Boss.laserBurst, transform.position, 1f);
        WeaverAudio.PlayAtPoint(Boss.radianceScream, transform.position, 0.5f);

        roarEmitter = RoarEmitter.Spawn(transform.position);
        roarEmitter.transform.SetParent(transform);

        if (bigRoar)
        {
            Animator.PlayAnimation("Big Roar Loop");
        }
        else
        {
            Animator.PlayAnimation("Roar Loop");
        }
        if (spawnCrystals)
        {
            StartCoroutine(SpawnCrystals());
        }
        //crystalRain.Play();

        float previousHealth = Boss.HealthComponent.Health;

        for (float t = 0; t < roarTime / (1f + ((previousHealth - Boss.HealthComponent.Health) / 50f)); t += Time.deltaTime)
        {
            yield return null;
        }

        roarEmitter.StopRoaring();
        roarEmitter = null;


        Animator.PlayAnimation("Roar End");
    }

    public override void OnStun()
    {
        if (roarEmitter != null)
        {
            roarEmitter.StopRoaring();
            roarEmitter = null;
        }
    }
}
