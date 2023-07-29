using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Utilities;

public class StomperMove : CrystalMachinistMove
{
    public const bool USE_NEW_STOMPERS = true;

    public override bool MoveEnabled => false;

    [SerializeField]
    float ceilingHeight;


    JumpMove jumpMove;

    [SerializeField]
    float stomperStartDelay = 0.4f;

    [SerializeField]
    List<Stomper> stompers;

    [Header("Main Stomping Settings")]
    [SerializeField]
    Vector2 stomperDelayRange = new Vector2(0.3f,0.6f);

    [SerializeField]
    float phase1StompingTime = 20f;

    [SerializeField]
    float phase1FinaleDelay = 0.5f;

    [SerializeField]
    Vector2 phase1FinaleDelayRange = new Vector2(0.6f, 0.4f);

    [SerializeField]
    float phase1FinaleTime = 4f;

    [Space]
    [Header("Phase 2")]
    [SerializeField]
    float phase2StompingTime = 12f;

    [SerializeField]
    Vector2 phase2StomperDelayRange = new Vector2(0.3f, 0.6f);

    [SerializeField]
    float phase2FinaleDelay = 0.5f;

    [SerializeField]
    Vector2 phase2FinaleDelayRange = new Vector2(0.6f, 0.4f);

    [SerializeField]
    float phase2FinaleTime = 4f;


    [SerializeField]
    int extraStompDelay = 2;

    /*[SerializeField]
    float sideSweepDelay = 0.4f;*/

    [SerializeField]
    float retractTimeThreshold = 0.3f;

    [SerializeField]
    float alternatingRetractTimeThreshold = 0.1f;

    [SerializeField]
    float conveyorBeltStartupDelay = 0.15f;

    [SerializeField]
    float conveyorBeltStartupTime = 1f;

    [SerializeField]
    Vector2 ceilingXDropRange = new Vector2(22.61f, 37.3f);



    [Space]
    [Space]
    [Header("New Stomper Mode")]
    [SerializeField]
    float newRandomStompDuration = 3f;

    [SerializeField]
    float newAllExceptOneStompDuration = 0.5f;

    [SerializeField]
    float newRollingStompDuration = 1.5f;

    [SerializeField]
    float rollingStompDelay = 0.1f;

    [SerializeField]
    float newAlternatingStompDuration = 3f;

    [SerializeField]
    float standardHeight;

    [SerializeField]
    float reorientHeight;

    [SerializeField]
    float reorientDelay = 0.75f;

    public float StomperHeight { get; private set; }


    private void Awake()
    {
        if (Boss.forceHardMode || WeaverCore.Features.Boss.Difficulty >= BossDifficulty.Ascended || BossSequenceController.IsInSequence)
        {
            StomperHeight = reorientHeight;
        }
        else
        {
            StomperHeight = standardHeight;
        }
        jumpMove = GetComponent<JumpMove>();
    }

    public override IEnumerator DoMove()
    {
        if (Boss.Settings != null && Boss.Settings.StompersEnabled)
        {
            Boss.HealthComponent.Invincible = true;
            Boss.MainCollider.enabled = false;

            yield return jumpMove.JumpToPosition(transform.position.x, ceilingHeight, jumpMove.DefaultJumpTime, 0f, false);

            for (int i = CrystalDropping.SpawnedDroppings.Count - 1; i >= 0; i--)
            {
                CrystalDropping.SpawnedDroppings[i].FadeOut();
            }

            yield return new WaitForSeconds(stomperStartDelay);

            if (USE_NEW_STOMPERS)
            {
                float stompingTime;
                Vector2 delayRange;
                float timeMultiplier;

                if (Boss.BossStage <= 2)
                {
                    stompingTime = Boss.Settings != null ? Boss.Settings.Phase1StomperTime : phase1StompingTime;
                    delayRange = stomperDelayRange;
                    timeMultiplier = 1f;
                }
                else
                {
                    stompingTime = Boss.Settings != null ? Boss.Settings.Phase2StomperTime : phase2StompingTime;
                    delayRange = phase2StomperDelayRange;
                    timeMultiplier = 0.5f;
                }

                yield return NewStompMove(delayRange, stompingTime, timeMultiplier);
            }
            else
            {
                if (Boss.BossStage <= 2)
                {
                    var stompingTime = Boss.Settings != null ? Boss.Settings.Phase1StomperTime : phase1StompingTime;
                    yield return StartStomping(stomperDelayRange, stompingTime, phase1FinaleDelay, phase1FinaleTime, phase1FinaleDelayRange);
                }
                else
                {
                    var stompingTime = Boss.Settings != null ? Boss.Settings.Phase2StomperTime : phase2StompingTime;
                    yield return StartStomping(phase2StomperDelayRange, stompingTime, phase2FinaleDelay, phase2FinaleTime, phase2FinaleDelayRange);
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
        if (Boss.BossStage <= 2)
        {
            Boss.ConveyorBelt.Speed = 3f;
            Boss.ConveyorBelt.GraduallySpeedUp(0f, Boss.conveyorBeltSpeed, conveyorBeltStartupTime, conveyorBeltStartupDelay, true);
        }
        else
        {
            var newSpeed = Boss.ConveyorBelt.Speed >= 0f ? Boss.conveyorBeltSpeedPhase2 : -Boss.conveyorBeltSpeedPhase2;

            Boss.ConveyorBelt.GraduallySpeedUp(Boss.ConveyorBelt.Speed, newSpeed, conveyorBeltStartupTime, conveyorBeltStartupDelay, true);
        }
        if (Boss.Settings != null && Boss.Settings.StompersEnabled)
        {
            yield return new WaitForSeconds(0.4f);

            if (Player.Player1.transform.position.x <= 30f)
            {
                transform.SetXPosition(ceilingXDropRange.y);
            }
            else
            {
                transform.SetXPosition(ceilingXDropRange.x);
            }

            yield return jumpMove.JumpToPosition(transform.position.x, Boss.FloorY, jumpMove.DefaultJumpTime, 0f, false);

            OnStun();
        }
    }

    /// <summary>
    /// Gets a list of stompers, sorted by farthest from player to closest to player
    /// </summary>
    /// <returns></returns>
    IEnumerable<Stomper> SortByFarthestDistance(Vector3 target)
    {
        return stompers.OrderBy(s => -Mathf.Abs(target.x - s.transform.position.x));
    }

    /// <summary>
    /// Gets a list of stompers, sorted by nearest to player to farthest from player
    /// </summary>
    /// <returns></returns>
    IEnumerable<Stomper> SortByNearestDistance(Vector3 target)
    {
        return stompers.OrderBy(s => Mathf.Abs(target.x - s.transform.position.x));
    }

    IEnumerable<Stomper> SortByFarthestFromPlayer() => SortByFarthestDistance(Player.Player1.transform.position);
    IEnumerable<Stomper> SortByNearestToPlayer() => SortByNearestDistance(Player.Player1.transform.position);


    Stomper GetFarthestStomper(Vector3 target) => SortByFarthestDistance(target).First();
    Stomper GetNearestStomper(Vector3 target) => SortByNearestDistance(target).First();

    Stomper GetMiddleStomper() => stompers[Mathf.FloorToInt(stompers.Count / 2)];

    IEnumerable<Stomper> GetOddStompers() => stompers.Where((s, i) => i % 2 != 0);

    IEnumerable<Stomper> GetEvenStompers() => stompers.Where((s, i) => i % 2 == 0);

    IEnumerable<Stomper> GetRandomStompers() => stompers.OrderBy(s => UnityEngine.Random.Range(0f,1f));

    public override void OnStun()
    {
        Boss.HealthComponent.Invincible = false;
        Boss.MainCollider.enabled = true;
        Boss.RB.isKinematic = false;
    }

    IEnumerator StartStomping(Vector2 stomperDelayRange, float stompingTime, float preFinaleDelay, float finaleTime, Vector2 finaleStompDelayRange)
    {
        TriggerSmash(StompingMode.Farthest, retractTimeThreshold);
        yield return new WaitForSeconds(stomperDelayRange.y);
        TriggerSmash(StompingMode.Nearest, retractTimeThreshold);
        yield return new WaitForSeconds(stomperDelayRange.y);
        TriggerSmash(StompingMode.Nearest, retractTimeThreshold);
        yield return new WaitForSeconds(stomperDelayRange.y);

        float startTime = Time.time;

        List<StompingMode> modes = new List<StompingMode>
        {
            StompingMode.Nearest,
            StompingMode.Nearest,
            StompingMode.Random,
            StompingMode.Farthest
        };

        IEnumerator ExtraStompsRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(extraStompDelay);
                TriggerSmash(StompingMode.Random, retractTimeThreshold);
            }
        }

        //var totalTime = phase1StompingTime + phase1FinaleTime;

        var stompRoutine = StartCoroutine(ExtraStompsRoutine());

        while (Time.time < startTime + stompingTime)
        {
            var mode = modes.GetRandomElement();
            TriggerSmash(mode, retractTimeThreshold);
            yield return new WaitForSeconds(Mathf.Lerp(stomperDelayRange.y,stomperDelayRange.x,Mathf.InverseLerp(startTime, startTime + stompingTime, Time.time)));
        }

        StopCoroutine(stompRoutine);

        yield return new WaitForSeconds(preFinaleDelay);

        var nearest = GetNearestStomper(Player.Player1.transform.position);

        bool runEvens = !GetOddStompers().Contains(nearest);


        startTime = Time.time;

        while (Time.time < startTime + finaleTime)
        {
            if (runEvens)
            {
                RunAllStompers(SortByNearestToPlayer().Intersect(GetEvenStompers()), alternatingRetractTimeThreshold,false);
            }
            else
            {
                RunAllStompers(SortByNearestToPlayer().Intersect(GetOddStompers()), alternatingRetractTimeThreshold, false);
            }

            runEvens = !runEvens;

            var delay = Mathf.Lerp(finaleStompDelayRange.y, finaleStompDelayRange.x, Mathf.InverseLerp(startTime, startTime + finaleTime, Time.time));
            yield return new WaitForSeconds(delay);
        }
    }

    public Stomper TriggerSmash(StompingMode mode, float retractTimeThreshold) => TriggerSmash(mode, Player.Player1.transform.position, retractTimeThreshold);

    public Stomper TriggerSmash(StompingMode mode, Vector3 target, float retractTimeThreshold)
    {
        switch (mode)
        {
            case StompingMode.Nearest:
                return RunOneStomper(SortByNearestDistance(target), retractTimeThreshold);
            case StompingMode.Random:
                List<Stomper> randomized = new List<Stomper>(stompers);
                randomized.RandomizeList();
                return RunOneStomper(randomized, retractTimeThreshold);
            case StompingMode.Farthest:
                return RunOneStomper(SortByFarthestDistance(target), retractTimeThreshold);
        }
        return null;
    }

    Stomper RunOneStomper(IEnumerable<Stomper> potentialStompers, float retractTimeThreshold, bool doNeightborCheck = true)
    {
        if (stompers.Count(s => s.CurrentState == Stomper.State.Smashing || s.CurrentState == Stomper.State.PreSmashing) >= stompers.Count - 1)
        {
            return null;
        }

        var playerStomper = GetNearestStomper(Player.Player1.transform.position);
        foreach (var stomper in potentialStompers)
        {
            if (stomper == playerStomper)
            {
                if (StomperIsReady(stomper, retractTimeThreshold) && (!doNeightborCheck || (doNeightborCheck && NeighborsCleared(stomper, retractTimeThreshold))) && stomper.Smash())
                {
                    return stomper;
                }
            }
            else
            {
                if (StomperIsReady(stomper, retractTimeThreshold) && stomper.Smash())
                {
                    return stomper;
                }
            }
        }
        return null;
    }

    List<bool> RunAllStompers(IEnumerable<Stomper> stompers, float retractTimeThreshold, bool doNeighborCheck = true)
    {
        List<bool> activatedStompers = new List<bool>();
        var playerStomper = GetNearestStomper(Player.Player1.transform.position);
        foreach (var stomper in stompers)
        {
            if (stomper == playerStomper)
            {
                //&& (!doNeighborCheck || (doNeighborCheck && NeighborsCleared(stomper)))
                if (StomperIsReady(stomper, retractTimeThreshold) && stomper.Smash())
                {
                    activatedStompers.Add(true);
                }
                else
                {
                    activatedStompers.Add(false);
                }
            }
            else
            {
                if (StomperIsReady(stomper, retractTimeThreshold) && stomper.Smash())
                {
                    activatedStompers.Add(true);
                }
                else
                {
                    activatedStompers.Add(false);
                }
            }
        }
        return activatedStompers;
    }

    bool NeighborsCleared(Stomper stomper, float retractTimeThreshold)
    {
        var index = stompers.IndexOf(stomper);
        if (index == 0)
        {
            return StomperIsReady(stompers[1], retractTimeThreshold);
        }
        else if (index == stompers.Count - 1)
        {
            return StomperIsReady(stompers[index - 1], retractTimeThreshold);
        }
        else
        {
            return StomperIsReady(stompers[index - 1], retractTimeThreshold) || StomperIsReady(stompers[index + 1], retractTimeThreshold);
        }
    }

    bool StomperIsReady(Stomper stomper, float retractTimeThreshold)
    {
        if (stomper.CurrentState != Stomper.State.Smashing)
        {
            if (stomper.CurrentState == Stomper.State.Retracting && stomper.transform.position.y >= StomperHeight - 1f /*stomper.RetractTime >= retractTimeThreshold*/)
            {
                return true;
            }
            else if (stomper.CurrentState != Stomper.State.Retracting)
            {
                return true;
            }
        }
        return false;
    }

    public enum StompingMode
    {
        Nearest,
        Random,
        Farthest
    }

    float currentStompDelay = 0f;
    uint stompRateCoroutine;

    IEnumerator NewRandomStomp(float timeMultiplier)
    {
        Debug.Log("DOING RANDOM STOMP");
        float startTime = Time.time;

        List<StompingMode> modes = new List<StompingMode>
        {
            StompingMode.Nearest,
            StompingMode.Random,
            StompingMode.Random
        };

        while (Time.time <= startTime + (newRandomStompDuration * timeMultiplier))
        {
            //List<Stomper> randomized = new List<Stomper>(stompers);
            //randomized.RandomizeList();
            //RunOneStomper(randomized, retractTimeThreshold, true);
            TriggerSmash(modes.GetRandomElement(), retractTimeThreshold);

            yield return new WaitForSeconds(currentStompDelay);
        }
    }

    bool StomperAvailable(Stomper stomper, float heightOffset = 1.5f)
    {
        return (stomper.CurrentState == Stomper.State.Retracting || stomper.CurrentState == Stomper.State.Idle) && stomper.transform.position.y >= StomperHeight - heightOffset;
    }

    IEnumerator NewAllExceptOneStomp(float timeMultiplier)
    {
        Debug.Log("DOING ALL EXCEPT ONE STOMP");
        var farthestSmasher = TriggerSmash(StompingMode.Farthest, retractTimeThreshold);
        yield return new WaitForSeconds(currentStompDelay);

        yield return new WaitForSeconds(0.1f);

        float startTime = Time.time;

        List<Stomper> randomized = new List<Stomper>(stompers);

        while (Time.time <= startTime + (newAllExceptOneStompDuration * timeMultiplier))
        {
            Stomper playerStomper;
            while (true)
            {
                playerStomper = GetNearestStomper(Player.Player1.transform.position);
                var playerStompIndex = stompers.IndexOf(playerStomper);

                if (playerStompIndex == 0)
                {
                    if (StomperAvailable(playerStomper) && StomperAvailable(stompers[1]) && StomperAvailable(stompers[2]))
                    {
                        break;
                    }
                }
                else if (playerStompIndex == stompers.Count - 1)
                {
                    if (StomperAvailable(playerStomper) && StomperAvailable(stompers[stompers.Count - 2]) && StomperAvailable(stompers[stompers.Count - 3]))
                    {
                        break;
                    }
                }
                else
                {
                    if (StomperAvailable(playerStomper) && StomperAvailable(stompers[playerStompIndex - 1]) && StomperAvailable(stompers[playerStompIndex + 1]))
                    {
                        break;
                    }
                }

                break;
            }
            //yield return new WaitUntil(() => stompers.Count(s => s.CurrentState == Stomper.State.Idle) >= 4);

            //int maxIndexDifference = UnityEngine.Random.Range(0f, 1f) > 0.5f ? 2 : 1;
            int maxIndexDifference = 1;

            randomized.RandomizeList();

            Stomper selectedStomper = playerStomper;

            for (int i = 0; i < randomized.Count; i++)
            {
                if (randomized[i] != playerStomper && Mathf.Abs(stompers.IndexOf(randomized[i]) - stompers.IndexOf(playerStomper)) == maxIndexDifference && randomized[i] != stompers[0] && randomized[i] != stompers[stompers.Count - 1])
                {
                    selectedStomper = randomized[i];
                    break;
                }
            }

            for (int i = 0; i < stompers.Count; i++)
            {
                if (stompers[i] != selectedStomper && stompers[i] != farthestSmasher && StomperIsReady(stompers[i],retractTimeThreshold))
                {
                    stompers[i].Smash(0.70f, 0.60f);
                }
            }

            yield return new WaitForSeconds(0.65f);

            

            //List<Stomper> randomized = new List<Stomper>(stompers);
            //randomized.RandomizeList();
            //RunOneStomper(randomized, retractTimeThreshold, true);

            //yield return new WaitForSeconds(currentStompDelay);

            selectedStomper.Smash(0.70f, 0.60f);

            yield return new WaitForSeconds(currentStompDelay);
        }

        if (stompRateCoroutine != 0)
        {
            if (farthestSmasher != null)
            {
                farthestSmasher.Smash(1f);
            }
            //TriggerSmash(StompingMode.Farthest, retractTimeThreshold);
            yield return new WaitForSeconds(currentStompDelay);
        }

    }

    IEnumerator NewRollingStomp(float timeMultiplier)
    {
        Debug.Log("DOING ROLLING STOMP");
        float startTime = Time.time;

        TriggerSmash(StompingMode.Farthest, retractTimeThreshold);
        yield return new WaitForSeconds(currentStompDelay);

        while (Time.time <= startTime + (newRollingStompDuration * timeMultiplier))
        {
            //yield return new WaitUntil(() => stompers.Count(s => s.CurrentState == Stomper.State.Idle) >= 3);
            var middleStomper = GetMiddleStomper();

            IEnumerable<Stomper> orderedStompers;

            if (Player.Player1.transform.position.x >= middleStomper.transform.position.x)
            {
                orderedStompers = stompers.OrderByDescending(s => s.transform.position.x);
            }
            else
            {
                orderedStompers = stompers.OrderBy(s => s.transform.position.x);
            }

            var iterator = orderedStompers.GetEnumerator();

            for (int i = 0; i < 4; i++)
            {
                iterator.MoveNext();
                if (StomperAvailable(iterator.Current, 0.25f))
                {
                    iterator.Current.Smash(1f);
                    yield return new WaitForSeconds(rollingStompDelay);
                }
            }

            yield return new WaitForSeconds(currentStompDelay);

            if (stompRateCoroutine != 0)
            {
                TriggerSmash(StompingMode.Farthest, retractTimeThreshold);
                yield return new WaitForSeconds(currentStompDelay);
            }
        }

        /*if (stompRateCoroutine != 0)
        {
            TriggerSmash(StompingMode.Nearest, retractTimeThreshold);
            yield return new WaitForSeconds(currentStompDelay);
        }*/

        yield break;
    }

    IEnumerator NewAlternatingStomp(float timeMultiplier)
    {
        Debug.Log("DOING ALTERNATING STOMP");
        float startTime = Time.time;

        var nearest = GetNearestStomper(Player.Player1.transform.position);

        bool runEvens = !GetOddStompers().Contains(nearest);

        if (runEvens)
        {
            RunOneStomper(GetOddStompers().OrderBy(s => Mathf.Abs(s.transform.position.x - Player.Player1.transform.position.x)), retractTimeThreshold);
            yield return new WaitForSeconds(currentStompDelay);
            RunOneStomper(GetOddStompers().OrderByDescending(s => Mathf.Abs(s.transform.position.x - Player.Player1.transform.position.x)), retractTimeThreshold);
            yield return new WaitForSeconds(currentStompDelay);
        }
        else
        {
            RunOneStomper(GetEvenStompers().OrderBy(s => Mathf.Abs(s.transform.position.x - Player.Player1.transform.position.x)), retractTimeThreshold);
            yield return new WaitForSeconds(currentStompDelay);
            RunOneStomper(GetEvenStompers().OrderByDescending(s => Mathf.Abs(s.transform.position.x - Player.Player1.transform.position.x)), retractTimeThreshold);
            yield return new WaitForSeconds(currentStompDelay);
        }

        while (Time.time <= startTime + (newAlternatingStompDuration * timeMultiplier))
        {
            if (runEvens)
            {
                RunAllStompers(GetEvenStompers(), alternatingRetractTimeThreshold, false);
            }
            else
            {
                RunAllStompers(GetOddStompers(), alternatingRetractTimeThreshold, false);
            }

            runEvens = !runEvens;

            if (Time.time <= startTime + (newAlternatingStompDuration * timeMultiplier))
            {
                yield return new WaitForSeconds((currentStompDelay / 2f) + 0.475f);
            }
        }

        if (stompRateCoroutine != 0)
        {
            yield return new WaitForSeconds(currentStompDelay);
            if (runEvens)
            {
                RunOneStomper(GetEvenStompers().OrderByDescending(s => Mathf.Abs(s.transform.position.x - Player.Player1.transform.position.x)), retractTimeThreshold);
                yield return new WaitForSeconds(currentStompDelay);
                RunOneStomper(GetEvenStompers().OrderBy(s => Mathf.Abs(s.transform.position.x - Player.Player1.transform.position.x)), retractTimeThreshold);
                yield return new WaitForSeconds(currentStompDelay);
            }
            else
            {
                RunOneStomper(GetOddStompers().OrderByDescending(s => Mathf.Abs(s.transform.position.x - Player.Player1.transform.position.x)), retractTimeThreshold);
                yield return new WaitForSeconds(currentStompDelay);
                RunOneStomper(GetOddStompers().OrderBy(s => Mathf.Abs(s.transform.position.x - Player.Player1.transform.position.x)), retractTimeThreshold);
                yield return new WaitForSeconds(currentStompDelay);
            }
        }

        yield break;
    }

    IEnumerator NewStompMove(Vector2 stomperDelayRange, float stompingTime, float timeMultiplier)
    {
        if (stompRateCoroutine != 0)
        {
            Boss.StopBoundRoutine(stompRateCoroutine);
            stompRateCoroutine = 0;
        }

        currentStompDelay = stomperDelayRange.y;

        if (Boss.forceHardMode || WeaverCore.Features.Boss.Difficulty >= BossDifficulty.Ascended || BossSequenceController.IsInSequence)
        {
            foreach (var stomper in stompers)
            {
                stomper.MoveToHeight(reorientHeight);
            }

            yield return new WaitUntil(() => stompers.All(s => !s.Reorienting));

            yield return new WaitForSeconds(reorientDelay);
        }

        TriggerSmash(StompingMode.Farthest, retractTimeThreshold);
        yield return new WaitForSeconds(stomperDelayRange.y);
        TriggerSmash(StompingMode.Nearest, retractTimeThreshold);
        yield return new WaitForSeconds(stomperDelayRange.y);
        TriggerSmash(StompingMode.Nearest, retractTimeThreshold);
        yield return new WaitForSeconds(stomperDelayRange.y);
        stompRateCoroutine = Boss.StartBoundRoutine(NewRateInterpolator(stomperDelayRange.y, stomperDelayRange.x, stompingTime));

        List<Func<float, IEnumerator>> stompModes = new List<Func<float, IEnumerator>>
        {
            NewRandomStomp,
            NewRandomStomp,
            NewAllExceptOneStomp,
            NewRollingStomp,
            NewAlternatingStomp
        };

        List<Func<float, IEnumerator>> validModes = new List<Func<float, IEnumerator>>();

        Func<float, IEnumerator> lastMode = null;

        while (stompRateCoroutine != 0)
        {
            validModes.AddRange(stompModes);
            validModes.RandomizeList();

            if (lastMode != null && validModes[validModes.Count - 1] == lastMode)
            {
                validModes[validModes.Count - 1] = validModes[0];
                validModes[0] = lastMode;
            }

            for (int i = validModes.Count - 1; i >= 0; i--)
            {
                var currentMode = validModes[i];
                validModes.RemoveAt(i);
                lastMode = currentMode;
                yield return currentMode(timeMultiplier);
                if (stompRateCoroutine == 0)
                {
                    break;
                }
            }
        }

        if (Boss.forceHardMode || WeaverCore.Features.Boss.Difficulty >= BossDifficulty.Ascended || BossSequenceController.IsInSequence)
        {
            var randomStompers = new List<Stomper>(stompers);

            randomStompers.RandomizeList();

            for (int i = randomStompers.Count - 1; i >= 0; i--)
            {
                while (true)
                {
                    var currentStomper = RunOneStomper(randomStompers, retractTimeThreshold, true);
                    if (currentStomper == null)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    else
                    {
                        currentStomper.ResetMaxHeight();
                        randomStompers.Remove(currentStomper);
                        currentStomper.Smash(1f, 1f);
                        break;
                    }
                }
                yield return new WaitForSeconds(currentStompDelay);
            }
        }

        //yield return new WaitUntil(() => stompers.All(s => s.transform.position.y >= reorientHeight + 1.5f));
        //yield return new WaitUntil(() => stompers.All(s => s.CurrentState == Stomper.State.Idle));

        /*TriggerSmash(StompingMode.Farthest, retractTimeThreshold);
        yield return new WaitForSeconds(stomperDelayRange.y);
        TriggerSmash(StompingMode.Nearest, retractTimeThreshold);
        yield return new WaitForSeconds(stomperDelayRange.y);
        TriggerSmash(StompingMode.Nearest, retractTimeThreshold);
        yield return new WaitForSeconds(stomperDelayRange.y);
        stompRateCoroutine = Boss.StartBoundRoutine(NewRateInterpolator(stomperDelayRange.y, stomperDelayRange.x, stompingTime));

        validModes.Clear();

        while (stompRateCoroutine != 0)
        {
            validModes.AddRange(stompModes);
            validModes.RandomizeList();

            if (lastMode != null && validModes[validModes.Count - 1] == lastMode)
            {
                validModes[validModes.Count - 1] = validModes[0];
                validModes[0] = lastMode;
            }

            for (int i = validModes.Count - 1; i >= 0; i--)
            {
                var currentMode = validModes[i];
                validModes.RemoveAt(i);
                lastMode = currentMode;
                yield return currentMode(timeMultiplier);
                if (stompRateCoroutine == 0)
                {
                    break;
                }
            }
        }*/

        yield break;
    }

    IEnumerator NewRateInterpolator(float from, float to, float time)
    {
        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            currentStompDelay = Mathf.Lerp(from,to,t / time);
            yield return null;
        }

        currentStompDelay = to;
        stompRateCoroutine = 0;
    }
}
