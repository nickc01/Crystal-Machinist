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
    Vector2 alternatingStomperDelayRange = new Vector2(0.2f,0.3f);

    [SerializeField]
    float phase2StompingTime = 12f;


    [SerializeField]
    int sideSweepMoments = 2;

    [SerializeField]
    float sideSweepDelay = 0.4f;

    [SerializeField]
    float retractTimeThreshold = 0.3f;

    private void Awake()
    {
        jumpMove = GetComponent<JumpMove>();
    }

    public override IEnumerator DoMove()
    {
        Boss.Health.Invincible = true;
        Boss.MainCollider.enabled = false;

        yield return jumpMove.JumpToPosition(transform.position.x, ceilingHeight, jumpMove.DefaultJumpTime, 0f);

        for (int i = CrystalDropping.SpawnedDroppings.Count - 1; i >= 0; i--)
        {
            CrystalDropping.SpawnedDroppings[i].FadeOut();
        }

        yield return new WaitForSeconds(stomperStartDelay);

        yield return StartStomping();
        yield return new WaitForSeconds(1f);
        Boss.ConveyorBelt.Speed = Boss.conveyorBeltSpeed;
        yield return new WaitForSeconds(1f);

        yield return jumpMove.JumpToPosition(transform.position.x, Boss.FloorY, jumpMove.DefaultJumpTime, 0f);

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
        Boss.Health.Invincible = false;
        Boss.MainCollider.enabled = true;
        Boss.RB.isKinematic = false;
    }

    IEnumerator StartStomping()
    {
        TriggerSmash(StompingMode.Farthest);
        yield return new WaitForSeconds(stomperDelayRange.y);
        TriggerSmash(StompingMode.Nearest);
        yield return new WaitForSeconds(stomperDelayRange.y);
        TriggerSmash(StompingMode.Nearest);
        yield return new WaitForSeconds(stomperDelayRange.y);

        float startTime = Time.time;

        List<StompingMode> modes = new List<StompingMode>
        {
            StompingMode.Nearest,
            StompingMode.Nearest,
            StompingMode.Random,
            StompingMode.Farthest
        };

        while (Time.time < startTime + phase1StompingTime)
        {
            var mode = modes.GetRandomElement();
            TriggerSmash(mode);
            yield return new WaitForSeconds(Mathf.Lerp(stomperDelayRange.y,stomperDelayRange.x,Mathf.InverseLerp(startTime, startTime + phase1StompingTime,Time.time)));
        }

        /*GetFarthestStomper().Smash();
        yield return new WaitForSeconds(stomperDelayRange.y);
        GetNearestStomper().Smash();
        yield return new WaitForSeconds(stomperDelayRange.y);*/

        /*for (int i = 0; i < 2; i++)
        {

            yield return new WaitForSeconds();
        }*/


    }

    public bool TriggerSmash(StompingMode mode) => TriggerSmash(mode, Player.Player1.transform.position);

    public bool TriggerSmash(StompingMode mode, Vector3 target)
    {
        switch (mode)
        {
            case StompingMode.Nearest:
                return RunStomper(SortByNearestDistance(target));
            case StompingMode.Random:
                List<Stomper> randomized = new List<Stomper>(stompers);
                randomized.RandomizeList();
                return RunStomper(randomized);
            case StompingMode.Farthest:
                return RunStomper(SortByFarthestDistance(target));
        }
        return false;
    }

    bool RunStomper(IEnumerable<Stomper> stompers)
    {
        var playerStomper = GetNearestStomper(Player.Player1.transform.position);
        foreach (var stomper in stompers)
        {
            if (stomper == playerStomper)
            {
                if (StomperIsReady(stomper) && NeighborsCleared(stomper) && stomper.Smash())
                {
                    return true;
                }
            }
            else
            {
                if (StomperIsReady(stomper) && stomper.Smash())
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool NeighborsCleared(Stomper stomper)
    {
        var index = stompers.IndexOf(stomper);
        if (index == 0)
        {
            return StomperIsReady(stompers[1]);
        }
        else if (index == stompers.Count - 1)
        {
            return StomperIsReady(stompers[index - 1]);
        }
        else
        {
            return StomperIsReady(stompers[index - 1]) || StomperIsReady(stompers[index + 1]);
        }
    }

    bool StomperIsReady(Stomper stomper)
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
