using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Utilities;

namespace AdminTool;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class AdminToolPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public override void Load()
    {
        ReactorCredits.Register<AdminToolPlugin>(ReactorCredits.AlwaysShow);

        Harmony.PatchAll();
    }
}
