using System;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace Fp2Trainer
{
    //[HarmonyPatch(typeof(FPStage), "UpdateRealTimeValues", new Type[] {typeof(int)})]
    [HarmonyPatch(typeof(FPStage))]
    [HarmonyPatch("UpdateRealTimeValues")]
    public static class FP2TrainerPatchFPStageUpdateRealTimeValues
    {
        public static FieldInfo FieldInfoDeltaTimeBacking = null;
        public static float artificialTimeDeltaTime = 0.016f;
        private static void Prefix()
        {
            /*
            try
            {
                // The code inside this method will run before 'PrivateMethod' is executed
                Log($"About to overwrite deltaTime...");
                OverwriteDeltaTime();
            }
            catch (Exception e)
            {
                Log($"{e.ToString()}\n{e.Message}\n{e.StackTrace}");
            }
            */
        }
    
        private static void Postfix()
        {
            // The code inside this method will run after 'PrivateMethod' has executed
            try
            {
                // The code inside this method will run before 'PrivateMethod' is executed
                if (Fp2Trainer.DeterministicMode.Value)
                {
                    OverwriteDeltaAndFrameTimeFPStage();
                }
            }
            catch (Exception e)
            {
                Log($"{e.ToString()}\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static void OverwriteDeltaAndFrameTimeFPStage()
        {
            FPStage.deltaTime = artificialTimeDeltaTime * 60;
            FPStage.frameTime = artificialTimeDeltaTime;
            
            FPStage.frame = ((Time.timeScale == 0f) ? (60f / artificialTimeDeltaTime) : (artificialTimeDeltaTime / Time.timeScale * 60f)); //This seems like funky math...
            FPStage.frameScale = ((FPStage.frame == 0f) ? (artificialTimeDeltaTime / 60f) : (1f / FPStage.frame)); // Oh, it's the inverse of frame...

        }
        public static void OverwriteDeltaTime(float replacementDeltaTime = 0.016f)
        {
            if (FieldInfoDeltaTimeBacking == null)
            {
                Type type = typeof(Time);
                var fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static);
                Log($"NumFields: {fieldInfos.Length}");
                foreach (var fi in fieldInfos)
                {
                    Log(fi.Name);
                    if (fi.Name.Contains("BackingField") && fi.Name.Contains("<deltaTime>"))
                    {
                        FieldInfoDeltaTimeBacking = fi;
                        //break;
                    }
                }
                var fieldInfo = type.GetField(_getBackingFieldName("deltaTime"), BindingFlags.NonPublic | BindingFlags.Static);
                Log(fieldInfo.Name);
            }

            if (FieldInfoDeltaTimeBacking != null)
            {
                Log("222");
                Log($"before FieldInfoDeltaTimeBacking (deltaTime): {FieldInfoDeltaTimeBacking.ToString()}");
                Log("333");
                FieldInfoDeltaTimeBacking.SetValue(null, replacementDeltaTime);
                Log("444");
                Log($"after FieldInfoDeltaTimeBacking (deltaTime): {FieldInfoDeltaTimeBacking.ToString()}");
                Log("555");
                
                Log($"Overwrote successfully: {Time.deltaTime}");
            }
            else
            {
                Log("Didn't find it");
            }
        }

        public static void Log(string txt)
        {
            MelonLogger.Msg(txt);
        }
        
        private static string _getBackingFieldName(string propertyName)
        {
            return string.Format("<{0}>k__BackingField", propertyName);
        }

        private static FieldInfo _getBackingField(object obj, string propertyName)
        {
            return obj.GetType().GetField(_getBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}