using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

public class ConveyorBeltController : MonoBehaviour
{
    [SerializeField]
    float maxVolumeSpeed = 5f;

    SurfaceEffector2D effector;
    ConveyorRail[] rails;
    Animator[] animators;
    AudioSource loopSource;

    [SerializeField]
    List<AudioClip> startupSounds;

    [SerializeField]
    List<float> startupVolumes;

    [SerializeField]
    AudioClip switchDirectionSound;

    [SerializeField]
    AnimationCurve speedUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField]
    ParticleSystem startupParticles;

    [SerializeField]
    ParticleSystem switchDirectionParticles;

    public float Speed
    {
        get => effector.speed;
        set
        {
            effector.speed = value;
            SetRailsSpeed(value);
            SetAnimationSpeed(Mathf.Clamp01(value / 4f));
            SetLoopVolume(value);
        }
    }


    private void Awake()
    {
        effector = GetComponent<SurfaceEffector2D>();
        rails = GetComponentsInChildren<ConveyorRail>();
        animators = GetComponentsInChildren<Animator>();
        loopSource = GetComponent<AudioSource>();
        SetRailsSpeed(Speed);
        SetAnimationSpeed(Mathf.Clamp01(Speed / 4f));
        SetLoopVolume(Speed);
    }

#if UNITY_EDITOR
    private void FixedUpdate()
    {
        SetRailsSpeed(effector.speed);
        SetAnimationSpeed(Mathf.Clamp01(Speed / 4f));
    }
#endif

    void SetRailsSpeed(float speed)
    {
        for (int i = rails.GetLength(0) - 1; i >= 0; i--)
        {
            rails[i].Speed = speed;
        }
    }

    void SetAnimationSpeed(float speed)
    {
        for (int i = animators.GetLength(0) - 1; i >= 0; i--)
        {
            animators[i].speed = speed;
        }
    }

    void SetLoopVolume(float speed)
    {
        loopSource.volume = Mathf.Clamp01(Mathf.Abs(speed) / maxVolumeSpeed);
    }

    IEnumerator GraduallySpeedUpRoutine(float oldSpeed, float newSpeed, float time, float delay)
    {
        yield return new WaitForSeconds(delay);

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            Speed = Mathf.Lerp(oldSpeed, newSpeed, speedUpCurve.Evaluate(t / time));
            yield return null;
        }
        Speed = newSpeed;
    }

    public void GraduallySpeedUp(float oldSpeed, float newSpeed, float time, float delay, bool playParticles)
    {
        if (playParticles)
        {
            if (startupParticles != null)
            {
                startupParticles.Play();
            }

            for (int i = 0; i < startupSounds.Count; i++)
            {
                WeaverAudio.PlayAtPoint(startupSounds[i],transform.position, startupVolumes[i]);
            }

            /*if (startupSound != null)
            {
                WeaverAudio.PlayAtPoint(startupSound, transform.position);
            }*/
        }

        StopAllCoroutines();
        StartCoroutine(GraduallySpeedUpRoutine(oldSpeed, newSpeed, time, delay));
    }

    public void SwitchDirection(bool playParticles, float time = 0.85f)
    {
        if (Speed == 0)
        {
            return;
        }

        if (playParticles)
        {
            if (switchDirectionParticles != null)
            {
                switchDirectionParticles.Play();
            }

            if (switchDirectionSound != null)
            {
                WeaverAudio.PlayAtPoint(switchDirectionSound, transform.position,1.15f);
            }
        }

        StopAllCoroutines();
        StartCoroutine(GraduallySpeedUpRoutine(Speed, -Speed, time, 0f));

        //Speed = -Speed;
    }
}
