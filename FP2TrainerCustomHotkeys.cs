using System;
using System.Reflection;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace Fp2Trainer
{
    public class FP2TrainerCustomHotkeys : MonoBehaviour
    {
        public static Dictionary<MelonPreferences_Entry<string>, KeyMapping> DictHotkeyPrefToKeyMappings;
        public void Start()
        {
            if (DictHotkeyPrefToKeyMappings == null)
            {
                DictHotkeyPrefToKeyMappings = new Dictionary<MelonPreferences_Entry<string>, KeyMapping>();
            }
        }

        public static void Add(MelonPreferences_Entry<string> mpe)
        {
            if (DictHotkeyPrefToKeyMappings == null)
            {
                DictHotkeyPrefToKeyMappings = new Dictionary<MelonPreferences_Entry<string>, KeyMapping>();
            }
            
            DictHotkeyPrefToKeyMappings.Add(mpe, InputControl.setKey(mpe.Value, KeyboardInputFromString(mpe.Value)));
        }

        public static KeyboardInput KeyboardInputFromString(string value)
        {
            if (value == null)
            {
                return null;
            }


            var modifiers = ModifiersFromString(value);
            try
            {
                String baseInput = value;
                if (modifiers != KeyModifier.NoModifier)
                {
                    baseInput = value.Substring(value.LastIndexOf("+") + 1);
                    Fp2Trainer.Log(String.Format("Input base interpreted as {0} -> {1}\n", value, baseInput));
                }

                 
                return new KeyboardInput(KeyCodeFromString(baseInput), modifiers);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static KeyCode KeyCodeFromString(string strKeyCode)
        {
            return (KeyCode) Enum.Parse(typeof(KeyCode), strKeyCode);
        }

        public static KeyModifier ModifiersFromString(string value)
        {
            KeyModifier keyMod = KeyModifier.NoModifier;
            Fp2Trainer.Log("1");

            // KeyModifier modifiers = CustomInput.modifiersFromString(value);
            // Can't call this directly because it's protected. So we're gonna use Reflection.
            
            Type customInputType = typeof(CustomInput);
            Fp2Trainer.Log("2");
            MethodInfo miModifiersFromString = customInputType.GetMethod("modifiersFromString", BindingFlags.NonPublic);
            Fp2Trainer.Log("3b miModifiersFromString NotNull: " + (miModifiersFromString != null).ToString());
            Fp2Trainer.Log("3b miModifiersFromSTring " + miModifiersFromString.ToString());
            Fp2Trainer.Log("3c value " + value);
            keyMod = (KeyModifier)(miModifiersFromString.Invoke(null, new object[] { value }));
            Fp2Trainer.Log("4");
            
            return keyMod;
        }

        public static string GetBindingString()
        {
            string strControlListing = "Current Hotkey Bindings:\n";
            
            // Optimization option: Possible optimization by caching a List version of these pairs instead of using the dictionary.
            foreach (var kvPair in DictHotkeyPrefToKeyMappings)
            {
                strControlListing += String.Format("{0} -> {1}\n", kvPair.Key.Value, kvPair.Value);
            }

            return strControlListing;
        }
    }
}