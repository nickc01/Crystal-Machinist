using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CopySpriteHeight : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer sourceRenderer;

    private void LateUpdate()
    {
        if (sourceRenderer != null)
        {
            transform.SetScaleY(sourceRenderer.size.y);
        }
    }
}
