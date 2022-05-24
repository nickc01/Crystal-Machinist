using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AttackMove : CrystalMachinistMove
{
    public override bool MoveEnabled => true;

    public override IEnumerator DoMove()
    {
        yield break;
    }

    public override void OnStun()
    {
        
    }
}
