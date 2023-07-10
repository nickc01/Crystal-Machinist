using System.Collections;
using WeaverCore;

public class JumpToPlayerMove : CrystalMachinistMove
{
    public override bool MoveEnabled => HeroController.instance.cState.freezeCharge || !HeroController.instance.cState.spellQuake;

    JumpMove jumpMove;

    private void Awake()
    {
        jumpMove = GetComponent<JumpMove>();
    }

    public override IEnumerator DoMove()
    {
        yield return jumpMove.JumpToPosition(Player.Player1.transform.position.x);

        if (jumpMove.LastJumpInterrupted)
        {
            yield return jumpMove.DefaultEmergencyJump();
        }
    }

    public override void OnStun()
    {
        jumpMove.OnStun();
    }

    public override void OnCancel()
    {
        jumpMove.OnCancel();
    }

    public override void OnDeath()
    {
        jumpMove.OnDeath();
    }
}

