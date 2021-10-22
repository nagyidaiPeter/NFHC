using HarmonyLib;

namespace NfhcModel.Patches
{
    public interface INfhcPatch
    {
        void Patch(Harmony instance);
        void Restore(Harmony instance);
    }
}
