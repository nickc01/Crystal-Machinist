using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

using UnitySM = UnityEngine.SceneManagement.SceneManager;
using UnityScene = UnityEngine.SceneManagement.Scene;
using System.Linq;
using GlobalEnums;

public class CrystalGuardianSceneExtender : MonoBehaviour
{
    [SerializeField]
    List<string> objectsToRemove;

    [SerializeField]
    float crystalShiftAmount = 1.5f;

    [SerializeField]
    List<int> crystalNumbersToBlock;

    [SerializeField]
    GameObject blockerPrefab;

    private void Awake()
    {
        GameObject.Destroy(GameObject.Find("Zombie Beam Miner Rematch"));

        for (int i = UnitySM.sceneCount - 1; i >= 0; i--)
        {
            var scene = UnitySM.GetSceneAt(i);
            if (scene.name == "GG_Crystal_Guardian_2")
            {
                RemoveObjects(scene);
            }
        }

        var bg = GameObject.Find("gg_mines_bg");

        for (int i = bg.transform.childCount - 1; i >= 0; i--)
        {
            var child = bg.transform.GetChild(i).gameObject;
            if (child.name.Contains("crystal_spike_short") && child.transform.position.z < 3.38f)
            {
                var sprite = child.GetComponent<SpriteRenderer>();
                if (sprite.color != Color.black)
                {
                    if (child.transform.localPosition.x >= 0f)
                    {
                        child.transform.localPosition += new Vector3(crystalShiftAmount, 0f, 0f);
                    }
                    else
                    {
                        child.transform.localPosition += new Vector3(-crystalShiftAmount, 0f, 0f);
                    }
                }
                else
                {
                    if (child.transform.localPosition.x >= 0f)
                    {
                        child.transform.localPosition += new Vector3(crystalShiftAmount / 2f, 0f, 0f);
                    }
                    else
                    {
                        child.transform.localPosition += new Vector3(-crystalShiftAmount / 2f, 0f, 0f);
                    }
                }

                if (child.name.Contains("22"))
                {
                    child.transform.localPosition -= new Vector3(0f,0f,0.58f);
                }

                if (crystalNumbersToBlock.Any(n => child.name.Contains(n.ToString())))
                {
                    GameObject.Instantiate(blockerPrefab, child.transform);
                }
            }
        }

        var spikeCollider = GameObject.Find("Spike Collider");

        spikeCollider.transform.SetPositionX(20.1f);
        spikeCollider.transform.SetScaleY(1.09f);

        if (WeaverCore.Initialization.Environment == WeaverCore.Enums.RunningState.Game)
        {
            var damager = spikeCollider.GetComponent("DamageHero");

            damager.GetType().GetField("hazardType").SetValue(damager, HazardType.SPIKES);
        }
    }

    void RemoveObjects(UnityScene scene)
    {
        var rootObjects = scene.GetRootGameObjects();
        foreach (var name in objectsToRemove)
        {
            foreach (var obj in rootObjects)
            {
                if (obj.name == name)
                {
                    GameObject.Destroy(obj);
                }
            }
        }
    }
}
