using System;
using HarmonyLib;
using MelonLoader;

namespace Fp2Trainer
{
    [HarmonyPatch(typeof(FPStage))]
    [HarmonyPatch("GetPlayerInstance_FPPlayer")]
    public class FP2TrainerPatchGetPlayerInstanceFPPlayer
    {
        public static void Postfix(ref FPPlayer __result)
        {
            // The code inside this method will run after 'PrivateMethod' has executed
            try
            {
                // The code inside this method will run before 'PrivateMethod' is executed
                if (Fp2Trainer.EnableGetPlayerInstanceMultiplayerPatch.Value)
                {
                    if (Fp2Trainer.fpplayers != null)
                    {
                        int count = Fp2Trainer.fpplayers.Count;
                        if (count > 0)
                        {
                            __result = Fp2Trainer.fpplayers[UnityEngine.Random.Range(0, count - 1)];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"{e.ToString()}\n{e.Message}\n{e.StackTrace}");
            }
        }
    }
}