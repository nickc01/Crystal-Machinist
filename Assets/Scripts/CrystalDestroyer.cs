using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalDestroyer : MonoBehaviour
{
    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("ActiveRegion");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<CrystalShot>(out var shot))
        {
            shot.DespawnImmediate();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<CrystalShot>(out var shot))
        {
            shot.DespawnImmediate();
        }
    }
}
