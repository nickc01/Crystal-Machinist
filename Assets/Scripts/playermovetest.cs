using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermovetest : MonoBehaviour
{
    public float speedtest = 1f;

    public float testIncrements = 0.25f;


    void Start()
    {
        StartCoroutine(MovementTester());
    }

    IEnumerator MovementTester()
    {
        while (true)
        {
            Vector3 start = transform.position;
            yield return new WaitForSeconds(testIncrements);
            Vector3 end = transform.position;

            Debug.Log("Start = " + start);
            Debug.Log("End = " + end);
            float distance = Vector3.Distance(start, end) / testIncrements;
            Debug.Log("Moved Amount = " + distance);
            Debug.Log("Discrepency Amount = " + (speedtest - distance));
        }
    }
}
