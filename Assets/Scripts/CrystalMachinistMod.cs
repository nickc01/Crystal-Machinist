using Modding;
using WeaverCore;

public class CrystalMachinistMod : WeaverMod
{
    public CrystalMachinistMod() : base("Crystal Machinist") { }

    public override string GetVersion()
    {
        return "2.0.0.0";
    }

    public override void Initialize()
    {
        ModHooks.LanguageGetHook += GodhomeLanguageHook;
    }

    internal static string GodhomeLanguageHook(string key, string sheetTitle, string res)
    {
        if (key == "NAME_MEGA_BEAM_MINER_2")
        {
            return "Crystal Machinist";
        }
        else
        {
            return res;
        }
    }
}