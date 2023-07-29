using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Settings;

public class CrystalMachinistGlobalSettings : GlobalSettings
{
    [Tooltip("How long the stompers will stomp in the first phase. The default value is 10")]
    [SettingField(displayName: "Phase 1 Stomper Time")]
    public float Phase1StomperTime = 10;

    [Tooltip("How long the stompers will stomp in the second phase. The default value is 14")]
    [SettingField(displayName: "Phase 2 Stomper Time")]
    public float Phase2StomperTime = 14f;

    [Tooltip("Should the stompers be enabled? The default value is checked")]
    public bool StompersEnabled = true;


    public override string TabName => "Crystal Machinist";
}
