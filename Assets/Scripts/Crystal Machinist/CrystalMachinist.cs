using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class CrystalMachinist : Boss
{
    [NonSerialized]
    SpriteRenderer _mainRenderer;
    public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponent<SpriteRenderer>();

    [NonSerialized]
    WeaverAnimationPlayer _animator;
    public WeaverAnimationPlayer Animator => _animator ??= GetComponent<WeaverAnimationPlayer>();

    [NonSerialized]
    BoxCollider2D _mainCollider;
    public BoxCollider2D MainCollider => _mainCollider ??= GetComponent<BoxCollider2D>();

    Rigidbody2D _rb;
    public Rigidbody2D RB => _rb ??= GetComponent<Rigidbody2D>();

    SpriteFlasher flasher;

    bool shotLaser;

    public AudioClip radianceScream;
    public AudioClip laserBurst;


    bool firstIdle;

    [Header("Main Config")]
    [SerializeField]
    int attunedHealth = 1500;

    [SerializeField]
    int ascendedHealth = 1600;

    [SerializeField]
    int radiantHealth = 1700;

    [SerializeField]
    float firstStunMilestone = 1f - 0.4f;

    [SerializeField]
    float secondStunMilestone = 1f - 0.65f;

    [Header("Stunning Sequence")]
    [SerializeField]
    Sprite knockoutSprite;

    [SerializeField]
    Sprite stunSprite;

    [SerializeField]
    AudioClip stunLandSound;

    [SerializeField]
    float stunTime = 1.75f;

    [SerializeField]
    float stunAwakeTime = 0.5f;

    [SerializeField]
    float stunGravityScale = 4f;

    [SerializeField]
    Vector2 stunVelocity;


    [Header("Death Sequence")]
    [SerializeField]
    ParticleSystem corpseFlame;

    [SerializeField]
    ParticleSystem corpseSteam;

    [SerializeField]
    ParticleSystem bubCloud;

    [SerializeField]
    ParticleSystem sporeCloud;

    [SerializeField]
    ParticleSystem crystalShort;

    [SerializeField]
    AudioClip deathSound;

    [SerializeField]
    Vector2 deathSoundPitchMinMax = new Vector2(0.85f,1.15f);

    [SerializeField]
    float jitterAmount = 0.15f;

    [SerializeField]
    Vector2 deathVelocity;

    [SerializeField]
    Collider2D deathCollider;

    [SerializeField]
    ConveyorBeltController _conveyorBelt;
    public ConveyorBeltController ConveyorBelt => _conveyorBelt;

    [SerializeField]
    [Tooltip("The conveyor belt speed that is applied when the next phase begins")]
    public float conveyorBeltSpeed = 5f;

    protected override void Awake()
    {
        //ConveyorBelt = 
        base.Awake();
        deathCollider.enabled = false;
        flasher = GetComponent<SpriteFlasher>();

        switch (Boss.Difficulty)
        {
            case WeaverCore.Enums.BossDifficulty.Attuned:
                Health.Health = attunedHealth;
                break;
            case WeaverCore.Enums.BossDifficulty.Ascended:
                Health.Health = ascendedHealth;
                break;
            default:
                Health.Health = radiantHealth;
                break;
        }

        AddStunMilestone((int)(Health.Health * firstStunMilestone));
        AddStunMilestone((int)(Health.Health * secondStunMilestone));

        StartBoundRoutine(StartupRoutine());
    }

    public bool FacingRight => MainRenderer.flipX;

    public bool PlayerRightOfBoss => Player.Player1.transform.position.x >= transform.position.x;

    public float FloorY { get; private set; }

    public bool DoStompersNext { get; private set; } = false;

    IEnumerator StartupRoutine()
    {
        Health.Invincible = true;
        TurnTowardsPlayerInstant();
        Animator.PlayAnimation("Idle");
        yield return new WaitForSeconds(1f);

        FloorY = transform.position.y;

        Health.Invincible = false;
        var title = AreaTitle.Spawn("Crystal", "Machinist");
        title.Position = WeaverCore.Enums.AreaTitlePosition.BottomRight;

        Music.ApplyMusicSnapshot(Music.SnapshotType.Action, 0f, 1f);

        yield return MainRoutine(false);

        /*if (firstIdle)
        {
            //IDLE
        }
        else
        {
            firstIdle = true;
            Animator.PlayAnimation("Idle");
            yield return new WaitForSeconds(0.5f);
            //FINISHED
        }*/
    }

    IEnumerator MainRoutine(bool spawnCrystals)
    {
        var roarMove = GetComponent<RoarMove>();
        //ROAR START
        yield return roarMove.Roar(spawnCrystals, false);

        var moves = GetComponents<CrystalMachinistMove>().ToList();

        /*if (moves.Count > 0 && moves[0] is RoarMove)
        {
            moves.Remove(roarMove);
            moves.Add(roarMove);
        }*/

        CrystalMachinistMove lastMove = roarMove;

        var stomperMove = GetComponent<StomperMove>();

        while (true)
        {
            if (DoStompersNext)
            {
                DoStompersNext = false;
                yield return RunMove(stomperMove);
            }

            moves.RandomizeList();

            if (moves.Count > 0 && moves[0] == lastMove)
            {
                moves.Remove(lastMove);
                moves.Add(lastMove);
            }

            for (int i = 0; i < moves.Count; i++)
            {
                yield return TurnTowardsPlayer();
                if (moves[i].MoveEnabled)
                {
                    yield return RunMove(moves[i]);
                }
            }

            yield return null;
        }
    }

    public IEnumerator TurnTowardsPlayer() => TurnTowardsObject(Player.Player1.gameObject);

    public IEnumerator TurnTowardsObject(GameObject obj)
    {
        if (obj.transform.position.x >= transform.position.x)
        {
            if (!MainRenderer.flipX)
            {
                yield return Animator.PlayAnimationTillDone("Turn");
                MainRenderer.flipX = true;
            }
        }
        else
        {
            if (MainRenderer.flipX)
            {
                yield return Animator.PlayAnimationTillDone("Turn");
                MainRenderer.flipX = false;
            }
        }
    }

    public IEnumerator DoIdle(float duration)
    {
        Animator.PlayAnimation("Idle");
        yield return new WaitForSeconds(duration);
    }

    public void TurnTowardsPlayerInstant()
    {
        MainRenderer.flipX = Player.Player1.transform.position.x >= transform.position.x;
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        StartBoundRoutine(DeathRoutine());
    }

    IEnumerator FlashLoop()
    {
        while (true)
        {
            flasher.flashInfectedLoop();
            yield return new WaitForSeconds(0.2f + 0.2f + 0.9f + 0.01f);
        }
    }

    IEnumerator DeathRoutine()
    {
        MainCollider.enabled = false;
        deathCollider.enabled = true;
        Animator.PlayAnimation("Death Stun");
        var sound = WeaverAudio.PlayAtPoint(deathSound, transform.position);
        sound.AudioSource.pitch = UnityEngine.Random.Range(deathSoundPitchMinMax.x, deathSoundPitchMinMax.y);

        corpseSteam.Play();
        crystalShort.Play();
        var flashLoop = StartCoroutine(FlashLoop());
        //flasher.flashInfectedLoop();

        var prevVelocity = RB.velocity;
        RB.isKinematic = true;
        RB.velocity = default;

        Vector3 startPos = transform.position;
        var playerPos = Player.Player1.transform.position;

        for (float t = 0; t < 0.75f; t += Time.deltaTime)
        {
            transform.position = startPos + new Vector3(RandomCoordValue(jitterAmount), RandomCoordValue(jitterAmount));
            yield return null;
        }

        transform.position = startPos;

        StopCoroutine(flashLoop);

        DeathWave.Spawn(transform.position, 2f);

        RB.gravityScale = 1f;
        RB.isKinematic = false;
        RB.velocity = deathVelocity;

        if (playerPos.x >= transform.position.x)
        {
            RB.velocity = deathVelocity.With(x: -deathVelocity.x);
        }

        Animator.PlayAnimation("Death Air");

        var jumpMove = GetComponent<JumpMove>();

        yield return new WaitUntil(() => jumpMove.OnGround);

        Animator.PlayAnimation("Death Land");

        yield return new WaitForSeconds(1f);

        corpseSteam.Stop();

        yield return new WaitForSeconds(0.5f);

        Boss.EndBossBattle();



        yield break;
    }

    protected override void OnStun()
    {
        base.OnStun();

        flasher.flashInfected();

        StartBoundRoutine(StunRoutine());
    }

    IEnumerator StunRoutine()
    {
        Animator.StopCurrentAnimation();
        yield return null;

        MainRenderer.sprite = knockoutSprite;

        StunEffect.Spawn(transform.position);

        TurnTowardsPlayerInstant();

        var oldScale = RB.gravityScale;

        RB.gravityScale = stunGravityScale;
        RB.isKinematic = false;

        var newVelocity = stunVelocity;

        var playerPos = Player.Player1.transform.position;

        if (Health.LastAttackDirection == CardinalDirection.Left)
        {
            newVelocity = newVelocity.With(x: -stunVelocity.x);
        }
        else if (Health.LastAttackDirection == CardinalDirection.Up || Health.LastAttackDirection == CardinalDirection.Down)
        {
            if (playerPos.x >= transform.position.x)
            {
                newVelocity = newVelocity.With(x: -stunVelocity.x);
            }
        }

        RB.velocity = newVelocity;


        yield return null;

        RB.velocity = newVelocity;

        yield return null;

        var jumpMove = GetComponent<JumpMove>();

        while(RB.velocity.y > 0f)
        {
            RB.velocity = new Vector2(newVelocity.x, RB.velocity.y);
            yield return null;
        }


        yield return new WaitUntil(() => jumpMove.OnGround && transform.position.y <= FloorY + 0.1f);
        //yield return new WaitUntil(() => RB.velocity.y <= 0f);

        //yield return new WaitUntil(() => RB.velocity.y <= 0f && jumpMove.OnGround && transform.position.y <= FloorY + 0.1f);
        /*while (!(RB.velocity.y <= 0f && jumpMove.OnGround && transform.position.y <= FloorY + 0.1f))
        {
            RB.velocity = new Vector2(newVelocity.x,RB.velocity.y);
            yield return null;
        }*/

        MainRenderer.sprite = stunSprite;

        jumpMove.PlayLandDust();

        WeaverAudio.PlayAtPoint(stunLandSound, transform.position);

        var currentHealth = Health.Health;

        for (float t = 0; t < stunTime; t += Time.deltaTime)
        {
            if (Health.Health < currentHealth)
            {
                break;
            }
            yield return null;
        }

        RB.gravityScale = oldScale;

        yield return Animator.PlayAnimationTillDone("Stun Awake");

        yield return new WaitForSeconds(stunAwakeTime);

        DoStompersNext = true;

        StartBoundRoutine(MainRoutine(true));


    }

    static float RandomCoordValue(float range)
    {
        return UnityEngine.Random.Range(range, -range);
    }
}
