using System.Collections;
using WeaverCore;

public class JumpToPlayerMove : CrystalMachinistMove
{
    public override bool MoveEnabled => !HeroController.instance.cState.spellQuake;

    JumpMove jumpMove;

    private void Awake()
    {
        jumpMove = GetComponent<JumpMove>();
    }

    public override IEnumerator DoMove() => jumpMove.JumpToPosition(Player.Player1.transform.position.x);

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

