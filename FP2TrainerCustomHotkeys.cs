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
        
        public static bool GetButtonDown(MelonPreferences_Entry<string> mpe)
        {
            if (mpe == null)
            {
                Fp2Trainer.Log(String.Format("mpe appears to be null: {0}", mpe));
                return false;
            }
            else if (mpe.Value == null)
            {
                Fp2Trainer.Log(String.Format("mpe's is set, but value appears to be null: {0} -> {1}", mpe.Identifier, mpe.Value));
                return false;
            }

            if (InputControl.GetButtonDown(DictHotkeyPrefToKeyMappings[mpe]))
            {
                Fp2Trainer.Log(String.Format("Button {0} just pressed.", mpe.Value));
            }

            return InputControl.GetButtonDown(DictHotkeyPrefToKeyMappings[mpe]);
        }
        
        public static bool GetButton(MelonPreferences_Entry<string> mpe)
        {
            
            return InputControl.GetButton(DictHotkeyPrefToKeyMappings[mpe]);
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

            if (value == null)
            {
                return keyMod;
            }

            int maxModifiers = 7;
            var strCtrlP = "Ctrl+";
            var strAltP = "Alt+";
            var strShiftP = "Shift+";
            
            for (int i = 0; i < maxModifiers; i++)
            {
                if (value.Contains(strCtrlP))
                {
                    value = value.Replace(strCtrlP, "");
                    keyMod |= KeyModifier.Ctrl;
                    continue;
                }
                if (value.Contains(strAltP))
                {
                    value = value.Replace(strAltP, "");
                    keyMod |= KeyModifier.Alt;
                    continue;
                }
                if (value.Contains(strShiftP))
                {
                    value = value.Replace(strShiftP, "");
                    keyMod |= KeyModifier.Shift;
                    continue;
                }
                break;
            }

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