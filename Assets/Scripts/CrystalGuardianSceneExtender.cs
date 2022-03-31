using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnitySM = UnityEngine.SceneManagement.SceneManager;
using UnityScene = UnityEngine.SceneManagement.Scene;

public class CrystalGuardianSceneExtender : MonoBehaviour
{
    [SerializeField]
    List<string> objectsToRemove;

    [SerializeField]
    float crystalShiftAmount = 1.5f;

    private void Awake()
    {
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
                if (child.transform.localPosition.x >= 0f)
                {
                    child.transform.localPosition += new Vector3(crystalShiftAmount, 0f,0f);
                }
                else
                {
                    child.transform.localPosition += new Vector3(-crystalShiftAmount, 0f, 0f);
                }
            }
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
