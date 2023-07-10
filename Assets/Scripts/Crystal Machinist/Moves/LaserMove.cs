using System;
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class LaserMove : CrystalMachinistMove
{
    public override bool MoveEnabled => Mathf.Abs(Player.Player1.transform.position.x - transform.position.x) > 8f/*(HeroController.instance.cState.spellQuake && Mathf.Abs(Player.Player1.transform.position.x - transform.position.x) > 8f)*/;

    [SerializeField]
    Transform arm;

    [SerializeField]
    LaserEmitter emitter;

    [SerializeField]
    float movementSpeed = 5f;

    [SerializeField]
    Transform flipScaler;

    WeaverAnimationPlayer armAnimator;
    SpriteRenderer armRenderer;

    [SerializeField]
    Sprite defaultSprite;

    [SerializeField]
    Vector2 angleRange;

    [SerializeField]
    AudioClip laserAnticSound;

    [SerializeField]
    AudioClip laserFireSound;

    [SerializeField]
    AudioClip laserLoopSound;

    AudioPlayer loopSound;

    [SerializeField]
    float jumpThreshold = 5f;

    [SerializeField]
    float leftJumpTarget;

    [SerializeField]
    float rightJumpTarget;

    JumpMove jumpMove;
    bool jumping = false;

    public SpriteRenderer ArmRenderer => armRenderer;

    private void Awake()
    {
        jumpMove = GetComponent<JumpMove>();
        armAnimator = arm.GetComponent<WeaverAnimationPlayer>();
        armRenderer = arm.GetComponent<SpriteRenderer>();
        //armAnimator.PlayAnimation("")
        armRenderer.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        var flipAdjustment = 180f;

        if (Boss.FacingRight)
        {
            flipAdjustment = 0f;
        }

        Gizmos.DrawLine(arm.position, arm.position + (Vector3)MathUtilities.PolarToCartesian(angleRange.x + flipAdjustment, 2f));
        Gizmos.DrawLine(arm.position, arm.position + (Vector3)MathUtilities.PolarToCartesian(angleRange.y + flipAdjustment, 2f));
    }

    IEnumerator JumpToCorner()
    {
        var playerPos = Player.Player1.transform.position;

        bool jumpInterrupted = false;

        jumping = true;
        if (UnityEngine.Random.Range(0,2) == 1)
        {
            yield return jumpMove.JumpToPosition(playerPos.x, jumpMove.DefaultJumpTime, 0f, true);

            if (jumpMove.LastJumpInterrupted)
            {
                jumpInterrupted = true;
                yield return jumpMove.DefaultEmergencyJump();
            }
        }

        float farthestTarget = 0f;
        float nearestTarget = 0f;

        if (Mathf.Abs(playerPos.x - leftJumpTarget) > Mathf.Abs(playerPos.x - rightJumpTarget))
        {
            farthestTarget = leftJumpTarget;
            nearestTarget = rightJumpTarget;
        }
        else
        {
            farthestTarget = rightJumpTarget;
            nearestTarget = leftJumpTarget;
        }

        if (!jumpInterrupted && !(Mathf.Abs(jumpMove.JumpRange.x - transform.position.x) < 0.1f || Mathf.Abs(jumpMove.JumpRange.y - transform.position.x) < 0.1f))
        {
            yield return jumpMove.JumpToPosition(farthestTarget, jumpMove.DefaultJumpTime, 0f, false);
        }

        yield return Boss.TurnTowardsPlayer();
        jumping = false;
    }

    public IEnumerator PrepareLaserStance()
    {
        flipScaler.SetScaleX(Boss.FacingRight ? -1 : 1);

        var anticClip = Boss.Animator.AnimationData.GetClip("Shoot Antic 2");

        Boss.MainRenderer.sprite = defaultSprite;
        yield return new WaitForSeconds(1f / anticClip.FPS);

        SetEntireRotation(0f);
        armRenderer.enabled = true;

        armAnimator.PlayAnimation("Arm Antic");
        yield return Boss.Animator.PlayAnimationTillDone("Shoot Antic 2");

        armAnimator.PlayAnimation("Arm Loop");
        Boss.Animator.PlayAnimation("Shoot Loop");
    }



    /*public IEnumerator AimLaser(float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            var playerAngle = Mathf.Clamp(GetAngleToPlayer(), angleRange.x, angleRange.y);

            var actualSpeed = speed;

            var playerPos = Player.Player1.transform.position;

            var distanceToPlayer = Vector2.Distance(transform.position, playerPos);

            if (distanceToPlayer < 2f)
            {
                actualSpeed += (2f - distanceToPlayer) * 2f;
            }


            SetEntireRotation(Mathf.LerpAngle(GetEntireRotation(), playerAngle, actualSpeed * Time.deltaTime));

            if (t >= emitter.ChargeUpDuration + Mathf.Clamp(0.2f, 0.1f, emitter.FireDuration))
            {
                if (HeroController.instance.cState.spellQuake || (Boss.FacingRight && !Boss.PlayerRightOfBoss) || (!Boss.FacingRight && Boss.PlayerRightOfBoss) || (Mathf.Abs(playerPos.x - transform.position.x) < 2.5f && playerPos.y - transform.position.y >= 2f))
                {
                    emitter.StopLaser();
                    StopLoopSound();
                    yield return new WaitForSeconds(emitter.EndDuration);
                    break;
                }
            }

            yield return null;
        }
    }*/

    public void EndLaserStanceQuick()
    {
        armRenderer.enabled = false;
    }

    public IEnumerator EndLaserStance()
    {
        var anticClip = Boss.Animator.AnimationData.GetClip("Shoot Antic 2");

        armAnimator.PlayAnimation("Arm End");
        Boss.Animator.PlayAnimation("Shoot End 2");

        var endClip = Boss.Animator.AnimationData.GetClip("Shoot End 2");

        var currentRotation = GetEntireRotation();
        var destRotation = 0f;

        for (int i = 1; i <= endClip.Frames.Count; i++)
        {
            SetEntireRotation(Mathf.Lerp(currentRotation, destRotation, i / (float)endClip.Frames.Count));

            if (i != endClip.Frames.Count)
            {
                yield return new WaitForSeconds(1f / endClip.FPS);
            }
        }

        armRenderer.enabled = false;

        Boss.MainRenderer.sprite = defaultSprite;
        //yield return new WaitForSeconds(1f / anticClip.FPS);
    }

    public override IEnumerator DoMove()
    {
        if (Mathf.Abs(Player.Player1.transform.position.x - transform.position.x) <= jumpThreshold)
        {
            yield return JumpToCorner();
        }

        yield return PrepareLaserStance();

        /*flipScaler.SetScaleX(Boss.FacingRight ? -1 : 1);

        var anticClip = Boss.Animator.AnimationData.GetClip("Shoot Antic 2");

        Boss.MainRenderer.sprite = defaultSprite;
        yield return new WaitForSeconds(1f / anticClip.FPS);

        SetEntireRotation(0f);
        armRenderer.enabled = true;

        armAnimator.PlayAnimation("Arm Antic");
        yield return Boss.Animator.PlayAnimationTillDone("Shoot Antic 2");

        armAnimator.PlayAnimation("Arm Loop");
        Boss.Animator.PlayAnimation("Shoot Loop");*/

        float speed = movementSpeed;

        IEnumerator FireLaser()
        {
            emitter.FireLaser();
            WeaverAudio.PlayAtPoint(laserAnticSound, transform.position);

            yield return new WaitForSeconds(emitter.ChargeUpDuration / 2f);

            speed /= 2f;

            yield return new WaitForSeconds(emitter.ChargeUpDuration / 2f);

            WeaverAudio.PlayAtPoint(laserFireSound, transform.position);
            loopSound = WeaverAudio.PlayAtPointLooped(laserLoopSound, transform.position);

            speed /= 2f;

            yield return new WaitForSeconds(emitter.FireDuration);

            StopLoopSound();
        }

        Boss.StartBoundRoutine(FireLaser());

        float duration = emitter.ChargeUpDuration + emitter.FireDuration + emitter.EndDuration;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            var playerAngle = Mathf.Clamp(GetAngleToPlayer(), angleRange.x, angleRange.y);

            var actualSpeed = speed;

            var playerPos = Player.Player1.transform.position;

            var distanceToPlayer = Vector2.Distance(transform.position, playerPos);

            if (distanceToPlayer < 2f)
            {
                actualSpeed += (2f - distanceToPlayer) * 2f;
            }


            //SetEntireRotation(Mathf.LerpAngle(GetEntireRotation(), playerAngle, actualSpeed * Time.deltaTime));
            ArmRotation = Mathf.LerpAngle(ArmRotation, playerAngle, actualSpeed * Time.deltaTime);

            if (t >= emitter.ChargeUpDuration + Mathf.Clamp(0.2f,0.1f,emitter.FireDuration))
            {
                if (HeroController.instance.cState.spellQuake || (Boss.FacingRight && !Boss.PlayerRightOfBoss) || (!Boss.FacingRight && Boss.PlayerRightOfBoss) || (Mathf.Abs(playerPos.x - transform.position.x) < 2.5f && playerPos.y - transform.position.y >= 2f))
                {
                    emitter.StopLaser();
                    StopLoopSound();
                    yield return new WaitForSeconds(emitter.EndDuration);
                    break;
                }
            }

            yield return null;
        }

        yield return EndLaserStance();
        /*armAnimator.PlayAnimation("Arm End");
        Boss.Animator.PlayAnimation("Shoot End 2");

        var endClip = Boss.Animator.AnimationData.GetClip("Shoot End 2");

        var currentRotation = GetEntireRotation();
        var destRotation = 0f;

        for (int i = 1; i <= endClip.Frames.Count; i++)
        {
            SetEntireRotation(Mathf.Lerp(currentRotation,destRotation,i / (float)endClip.Frames.Count));
            yield return new WaitForSeconds(1f / endClip.FPS);
        }

        armRenderer.enabled = false;

        Boss.MainRenderer.sprite = defaultSprite;
        yield return new WaitForSeconds(1f / anticClip.FPS);*/

        /*for (float t = 0; t < clipDuration; t += Time.deltaTime)
        {
            SetEntireRotation();
        }*/

    }

    void StopLoopSound()
    {
        if (loopSound != null)
        {
            loopSound.StopPlaying();
            loopSound = null;
        }
    }

    float ClampRotation(float angle)
    {
        if (angle > 180)
        {
            return angle - 360f;
        }
        else
        {
            return angle;
        }
    }

    public override void OnStun()
    {
        emitter.StopLaser();
        armRenderer.enabled = false;
        StopLoopSound();

        if (jumping)
        {
            jumpMove.OnStun();
        }
    }

    void SetArmRotation(float angle)
    {
        arm.localRotation = Quaternion.Euler(0f,0f,angle);
    }

    void SetLaserRotation(float angle)
    {
        emitter.transform.localRotation = Quaternion.Euler(0f,0f,angle);
    }

    float GetArmRotation()
    {
        return ClampRotation(arm.localEulerAngles.z);
    }

    float GetLaserRotation()
    {
        return ClampRotation(emitter.transform.localEulerAngles.z);
    }

    public Transform EmitterTransform => emitter.transform;

    public float GetAngleToPlayer()
    {
        return Vector2.Dot(Vector2.down * 90f, (Player.Player1.transform.position - emitter.transform.position).normalized);
    }

    void SetEntireRotation(float angle)
    {
        SetArmRotation(angle / 2f);
        SetLaserRotation(angle / 2f);
    }

    float GetEntireRotation()
    {
        return GetArmRotation() + GetLaserRotation();
    }

    public float ArmRotation
    {
        get => GetEntireRotation();
        set => SetEntireRotation(value);
    }
}
