using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class JumpMove : CrystalMachinistMove
{
    [SerializeField]
    AudioClip jumpSound;

    [SerializeField]
    AudioClip landSound;

    [SerializeField]
    float jumpTime = 0.5f;

    HashSet<GameObject> floorCollisions = new HashSet<GameObject>();

    GameObject slamEffect;
    ParticleSystem landDust;

    [SerializeField]
    Vector2 jumpRangeMinMax = new Vector2(16.72f, 40.01f);

    public bool OnGround => floorCollisions.Count > 0;

    public float DefaultJumpTime => jumpTime;

    private void Awake()
    {
        slamEffect = transform.Find("Slam Effect").gameObject;
        landDust = transform.Find("Land Dust").GetComponent<ParticleSystem>();
    }

    public override bool MoveEnabled => true;

    public override IEnumerator DoMove()
    {
        if (HeroController.instance.cState.spellQuake)
        {
            if (Mathf.Abs(Player.Player1.transform.position.x - jumpRangeMinMax.x) < Mathf.Abs(Player.Player1.transform.position.x - jumpRangeMinMax.y))
            {
                yield return JumpToPosition(jumpRangeMinMax.x, jumpTime);
            }
            else
            {
                yield return JumpToPosition(jumpRangeMinMax.y, jumpTime);
            }
        }
        else
        {
            var jumpRangeX = UnityEngine.Random.Range(16.72f, 40.01f);
            yield return JumpToPosition(jumpRangeX, jumpTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector3(jumpRangeMinMax.x,transform.position.y,transform.position.z));
        Gizmos.DrawLine(transform.position, new Vector3(jumpRangeMinMax.y,transform.position.y,transform.position.z));
    }

    public IEnumerator JumpToPosition(float xPos, float time, float delay = 0.15f)
    {
        return JumpToPosition(xPos,13.6f,time,delay);
    }

    public IEnumerator JumpToPosition(float xPos, float yPos, float time, float delay = 0.15f)
    {
        if (!Boss.MainCollider.enabled)
        {
            Boss.RB.isKinematic = true;
        }
        var velocity = MathUtilities.CalculateVelocityToReachPoint(transform.position, new UnityEngine.Vector2(xPos, yPos), time, Boss.RB.gravityScale);
        Boss.TurnTowardsPlayerInstant();
        yield return Animator.PlayAnimationTillDone("Jump Antic");

        Animator.PlayAnimation("Jump Air");

        if (!Boss.MainCollider.enabled)
        {
            Boss.RB.isKinematic = false;
        }

        Boss.RB.velocity = velocity;

        Boss.TurnTowardsPlayerInstant();

        WeaverAudio.PlayAtPoint(jumpSound, transform.position);

        //Debug.Log("JUMPING");
        //Wait untill the boss is falling

        while (Boss.RB.velocity.y > 0f)
        {
            Boss.RB.velocity = Boss.RB.velocity.With(x: velocity.x);
            yield return null;
        }
        //yield return new WaitUntil(() => Boss.RB.velocity.y <= 0f);

        //Debug.Log("FALLING");

        //Wait untill coming in contact with the ground

        if (Boss.MainCollider.enabled)
        {
            yield return new WaitUntil(() => floorCollisions.Count > 0);
        }
        else
        {
            yield return new WaitUntil(() => transform.position.y <= yPos);
            transform.SetYPosition(yPos);
        }

        //Debug.Log("LANDING");

        Boss.RB.velocity = default;

        if (!Boss.MainCollider.enabled)
        {
            Boss.RB.isKinematic = true;
        }

        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.AverageShake);

        slamEffect.SetActive(true);

        PlayLandDust();

        WeaverAudio.PlayAtPoint(landSound, transform.position);

        yield return Animator.PlayAnimationTillDone("Jump Land");

        if (delay > 0)
        {
            yield return Boss.DoIdle(delay);
        }
    }

    public void PlayLandDust()
    {
        landDust.Play();
    }

    public IEnumerator JumpToPosition(float xPos) => JumpToPosition(xPos, jumpTime);

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain") && collision.gameObject.transform.position.y <= transform.position.y)
        {
            floorCollisions.Add(collision.gameObject);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            floorCollisions.Remove(collision.gameObject);
        }
    }

    public override void OnStun()
    {
        
    }
}

