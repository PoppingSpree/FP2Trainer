using System;
using Harmony;
using MelonLoader;

namespace Fp2Trainer
{
    [HarmonyPatch(typeof(FPStage))]
    [HarmonyPatch("GetPlayerInstance")]
    public class FP2TrainerPatchGetPlayerInstance
    {
        public static void Postfix(ref FPBaseObject __result)
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
                            __result = (FPBaseObject)(Fp2Trainer.fpplayers[UnityEngine.Random.Range(0, count - 1)]);
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