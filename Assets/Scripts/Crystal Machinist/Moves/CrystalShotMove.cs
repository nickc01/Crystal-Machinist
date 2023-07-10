using MonoMod.RuntimeDetour;
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class CrystalShotMove : CrystalMachinistMove
{
    [SerializeField]
    WeaverAnimationPlayer laserBallAnimator;

    [SerializeField]
    float fireDelay = 0.25f;

    [SerializeField]
    float fireInterpolationSpeed = 10f;

    [SerializeField]
    Vector2 fireAngleRange = new Vector2(-70f,70f);

    [SerializeField]
    CrystalShot shotPrefab;

    [SerializeField]
    int shots = 3;

    [SerializeField]
    float shotAngleSeparation = 15f;

    [SerializeField]
    float shotVelocity = 15f;

    [SerializeField]
    AudioClip preFireSound;

    [SerializeField]
    AudioClip fireSound;

    [SerializeField]
    float fireKnockbackAngle = -70f;

    [SerializeField]
    float fireKnockBackFPS = 12;

    [SerializeField]
    float fireKnockBackRiseTime = 0.15f;

    [SerializeField]
    float fireKnockBackLowerDelay = 0f;

    [SerializeField]
    float fireKnockbackLowerTime = 0.15f;

    [SerializeField]
    float fireEndDelay = 0.1f;

    [SerializeField]
    float fireKnockbackVelocity = 20f;

    [SerializeField]
    float fireDeccelerationDelay = 0.60f;

    [SerializeField]
    float fireKnockbackDecceleration = 10f;

    [SerializeField]
    float fireKnockbackGravity = 1f;

    [SerializeField]
    AnimationCurve fireKnockbackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField]
    ParticleSystem crystalParticles;

    [SerializeField]
    SpriteFlasher armFlasher;

    [SerializeField]
    ParticleSystem slideDust;

    [SerializeField]
    AudioClip slideSound;

    [SerializeField]
    float slideSoundVolume = 1f;

    [SerializeField]
    float endDelay = 2f / 12f;

    public float FireDelay
    {
        get => fireDelay;
        set => fireDelay = value;
    }


    public override bool MoveEnabled => true;

    LaserMove laserMove;

    bool moving = false;
    float movingVelocity = 0;
    float decellerationTime = 0;

    float lastXPos;

    float originalGravity;

    RoarMove roarMove;

    private void Awake()
    {
        laserMove = GetComponent<LaserMove>();
        roarMove = GetComponent<RoarMove>();
        laserBallAnimator.SpriteRenderer.sprite = null;
    }

    void FixedUpdate()
    {
        if (moving)
        {
            if (Time.time >= decellerationTime)
            {
                if (movingVelocity > 0)
                {
                    movingVelocity -= Time.fixedDeltaTime * fireKnockbackDecceleration;
                    if (movingVelocity < 0)
                    {
                        movingVelocity = 0;
                    }
                }
                else if (movingVelocity < 0)
                {
                    movingVelocity += Time.fixedDeltaTime * fireKnockbackDecceleration;
                    if (movingVelocity > 0)
                    {
                        movingVelocity = 0;
                    }
                }
            }

            float currentPos = transform.position.x;

            if (currentPos - lastXPos == 0f || Mathf.Abs(movingVelocity) <= 0.1f)
            {
                moving = false;
            }
            else
            {
                lastXPos = currentPos;
                Boss.RB.velocity = Boss.RB.velocity.With(x: movingVelocity);
            }
        }
    }

    AudioPlayer slideSoundInstance;

    bool doingRoar = false;

    public override IEnumerator DoMove()
    {
        bool turnAround = UnityEngine.Random.Range(0f, 1f) < 0.25f;

        if (turnAround || (UnityEngine.Random.Range(0f, 1f) < 0.5f && HeroController.instance.cState.onGround && Mathf.Abs(Player.Player1.transform.position.x - transform.position.x) <= 4f))
        {
            yield return new WaitForSeconds(1f / 12f);
            yield return Boss.TurnAround();
        }

        originalGravity = Boss.RB.gravityScale;
        yield return laserMove.PrepareLaserStance();

        laserBallAnimator.PlayAnimation("Ball Antic and Shoot");

        AudioPlayer preSoundInstance = null;

        if (preFireSound != null)
        {
            preSoundInstance = WeaverAudio.PlayAtPoint(preFireSound,transform.position);
        }

        for (float t = 0; t < fireDelay; t += Time.deltaTime)
        {
            laserMove.ArmRotation = Mathf.LerpAngle(laserMove.ArmRotation, Mathf.Clamp(laserMove.GetAngleToPlayer(), fireAngleRange.x,fireAngleRange.y), fireInterpolationSpeed * Time.deltaTime);
            yield return null;
        }

        if (preSoundInstance != null)
        {
            preSoundInstance.StopPlaying();
            preSoundInstance.Delete();
        }

        Shoot();

        Boss.RB.gravityScale = fireKnockbackGravity;
        //Boss.RB.velocity = new Vector2(Boss.FacingRight ? -fireKnockbackVelocity : fireKnockbackVelocity, 0f);
        movingVelocity = Boss.FacingRight ? -fireKnockbackVelocity : fireKnockbackVelocity;
        lastXPos = float.PositiveInfinity;
        decellerationTime = Time.time + fireDeccelerationDelay;
        moving = true;

        if (slideSound != null)
        {
            slideSoundInstance = WeaverAudio.PlayAtPointLooped(slideSound, transform.position, slideSoundVolume);
        }

        slideDust.Play();

        Boss.StartBoundRoutine(StopParticlesWhenStopped());

        if (fireSound != null)
        {
            WeaverAudio.PlayAtPoint(fireSound, transform.position);
        }

        laserBallAnimator.StopCurrentAnimation();
        laserBallAnimator.SpriteRenderer.sprite = null;
        armFlasher.flashWhiteQuick();
        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.AverageShake);

        crystalParticles.Play();

        var startTime = Time.time;

        var oldRotation = laserMove.ArmRotation;

        while (Time.time < startTime + fireKnockBackRiseTime)
        {
            laserMove.ArmRotation = Mathf.Lerp(oldRotation, fireKnockbackAngle, fireKnockbackCurve.Evaluate((Time.time - startTime) / fireKnockBackRiseTime));
            yield return new WaitForSeconds(1f / fireKnockBackFPS);
        }

        yield return new WaitForSeconds(fireKnockBackLowerDelay);

        startTime = Time.time;

        while (Time.time < startTime + fireKnockbackLowerTime)
        {
            laserMove.ArmRotation = Mathf.Lerp(fireKnockbackAngle, 0f, fireKnockbackCurve.Evaluate((Time.time - startTime) / fireKnockbackLowerTime));
            yield return new WaitForSeconds(1f / fireKnockBackFPS);
        }

        yield return new WaitForSeconds(fireEndDelay);

        if (!ReferenceEquals(Boss.PreviousMove, roarMove) && UnityEngine.Random.Range(0f,1f) >= 0.35f && Time.time < roarMove.LastRoarTime + 3f)
        {
            laserMove.EndLaserStanceQuick();
            Boss.StartBoundRoutine(DoRoarMoveWhileSliding());
        }
        else
        {
            yield return laserMove.EndLaserStance();
        }

        yield return new WaitUntil(() => !moving && !doingRoar);
        //yield return new WaitUntil(() => Mathf.Abs(Boss.RB.velocity.x) <= 0.1f);

        Boss.RB.gravityScale = originalGravity;

        yield return new WaitForSeconds(endDelay);
    }

    IEnumerator DoRoarMoveWhileSliding()
    {
        var roar = GetComponent<RoarMove>();
        doingRoar = true;
        yield return roar.DoMove();
        doingRoar = false;
    }

    IEnumerator StopParticlesWhenStopped()
    {
        while (moving)
        {
            if (slideSoundInstance != null)
            {
                slideSoundInstance.transform.position = transform.position;
            }

            yield return null;
        }

        if (slideSoundInstance != null)
        {
            slideSoundInstance.Delete();
            slideSoundInstance = null;
        }

        //yield return new WaitUntil(() => Mathf.Abs(Boss.RB.velocity.x) <= 0.1f);

        slideDust.Stop();
    }

    void Shoot()
    {
        float halfShots = (shots - 1) / 2;

        for (float i = -halfShots; i <= halfShots; i++)
        {
            var extraRotation = Quaternion.Euler(0f, 0f, shotAngleSeparation * i);

            if (!Boss.FacingRight)
            {
                extraRotation = extraRotation * Quaternion.Euler(0f, 0f, 180f);
            }

            var instance = Pooling.Instantiate(shotPrefab, laserMove.EmitterTransform.position, laserMove.EmitterTransform.rotation * extraRotation);

            instance.GetComponent<Rigidbody2D>().velocity = ((laserMove.EmitterTransform.rotation * extraRotation) * Vector2.right) * shotVelocity;
        }
    }

    public override void OnStun()
    {
        if (doingRoar)
        {
            doingRoar = false;
            GetComponent<RoarMove>().OnStun();
        }
        if (slideSoundInstance != null)
        {
            slideSoundInstance.Delete();
            slideSoundInstance = null;
        }
        slideDust.Stop();
        laserBallAnimator.StopCurrentAnimation();
        laserBallAnimator.SpriteRenderer.sprite = null;
        laserMove.ArmRenderer.enabled = false;
        Boss.RB.gravityScale = originalGravity;
        moving = false;
    }
}
