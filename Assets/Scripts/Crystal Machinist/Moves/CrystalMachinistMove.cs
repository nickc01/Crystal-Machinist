using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Interfaces;

public abstract class CrystalMachinistMove : MonoBehaviour, IBossMove
{
    CrystalMachinist _boss;
    public CrystalMachinist Boss => _boss ??= GetComponent<CrystalMachinist>();

    public WeaverAnimationPlayer Animator => Boss.Animator;

    public abstract bool MoveEnabled { get; }

    public abstract IEnumerator DoMove();

    public virtual void OnCancel()
    {
        OnStun();
    }

    public virtual void OnDeath()
    {
        OnStun();
    }

    public abstract void OnStun();
}
