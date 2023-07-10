using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class StomperMove : CrystalMachinistMove
{
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

    private void Awake()
    {
        jumpMove = GetComponent<JumpMove>();
    }

    public override IEnumerator DoMove()
    {
        Boss.HealthComponent.Invincible = true;
        Boss.MainCollider.enabled = false;

        yield return jumpMove.JumpToPosition(transform.position.x, ceilingHeight, jumpMove.DefaultJumpTime, 0f, false);

        for (int i = CrystalDropping.SpawnedDroppings.Count - 1; i >= 0; i--)
        {
            CrystalDropping.SpawnedDroppings[i].FadeOut();
        }

        yield return new WaitForSeconds(stomperStartDelay);

        if (Boss.BossStage <= 2)
        {
            yield return StartStomping(stomperDelayRange, phase1StompingTime, phase1FinaleDelay, phase1FinaleTime, phase1FinaleDelayRange);
        }
        else
        {
            yield return StartStomping(phase2StomperDelayRange, phase2StompingTime, phase2FinaleDelay, phase2FinaleTime, phase2FinaleDelayRange);
        }
        yield return new WaitForSeconds(1f);
        if (Boss.BossStage <= 2)
        {
            Boss.ConveyorBelt.Speed = 3f;
            Boss.ConveyorBelt.GraduallySpeedUp(0f, Boss.conveyorBeltSpeed, conveyorBeltStartupTime, conveyorBeltStartupDelay, true);
        }
        else
        {
            //Boss.ConveyorBelt.Speed = Boss.conveyorBeltSpeedPhase2;

            var newSpeed = Boss.ConveyorBelt.Speed >= 0f ? Boss.conveyorBeltSpeedPhase2 : -Boss.conveyorBeltSpeedPhase2;

            Boss.ConveyorBelt.GraduallySpeedUp(Boss.ConveyorBelt.Speed, newSpeed, conveyorBeltStartupTime, conveyorBeltStartupDelay, true);
        }
        yield return new WaitForSeconds(0.75f);

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

    IEnumerable<Stomper> GetOddStompers() => stompers.Where((s, i) => i % 2 != 0);

    IEnumerable<Stomper> GetEvenStompers() => stompers.Where((s, i) => i % 2 == 0);

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

    public bool TriggerSmash(StompingMode mode, float retractTimeThreshold) => TriggerSmash(mode, Player.Player1.transform.position, retractTimeThreshold);

    public bool TriggerSmash(StompingMode mode, Vector3 target, float retractTimeThreshold)
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
        return false;
    }

    bool RunOneStomper(IEnumerable<Stomper> potentialStompers, float retractTimeThreshold, bool doNeightborCheck = true)
    {
        if (stompers.Count(s => s.CurrentState == Stomper.State.Smashing || s.CurrentState == Stomper.State.PreSmashing) >= stompers.Count - 1)
        {
            return false;
        }

        var playerStomper = GetNearestStomper(Player.Player1.transform.position);
        foreach (var stomper in potentialStompers)
        {
            if (stomper == playerStomper)
            {
                if (StomperIsReady(stomper, retractTimeThreshold) && (!doNeightborCheck || (doNeightborCheck && NeighborsCleared(stomper, retractTimeThreshold))) && stomper.Smash())
                {
                    return true;
                }
            }
            else
            {
                if (StomperIsReady(stomper, retractTimeThreshold) && stomper.Smash())
                {
                    return true;
                }
            }
        }
        return false;
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
            if (stomper.CurrentState == Stomper.State.Retracting && stomper.RetractTime >= retractTimeThreshold)
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
}
