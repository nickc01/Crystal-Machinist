using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceTest : MonoBehaviour
{
    [field: SerializeField]
    public float Speed = 1.0f;

    private void Update()
    {
        transform.position += new Vector3(Speed * Time.deltaTime,0f);
    }
}
