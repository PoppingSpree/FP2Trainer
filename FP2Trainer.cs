using MelonLoader;

using UnityEngine;
//using UnityEngine.InputSystem.Controls;
//using UnityEngine.InputSystem;
using System.IO;
using System;
using System.Collections.Generic;
using System.Net.Configuration;

//using UnityEngine.Tilemaps;

namespace Fp2Trainer
{
    public static class BuildInfo
    {
        public const string Name = "FP2 Trainer"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Training tools for speedrunning Freedom Planet 2: Quick Warps, Quick Resets, Live Tilemap Editing, etc"; // Description for the Mod.  (Set as null if none)
        public const string Author = "Catssandra Ann"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "0.1.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class Fp2Trainer : MelonMod
    {
        public static MelonPreferences_Category fp2Trainer;
        public static MelonPreferences_Entry<bool> enableWarps;
        public static MelonPreferences_Entry<bool> showDebug;
        public static MelonPreferences_Entry<bool> showLevelEditDebug;

        public static MelonPreferences_Entry<string> inputLETileCopy;
        public static MelonPreferences_Entry<string> inputLETilePaste;
        public static MelonPreferences_Entry<string> inputLETileLayer;

        private GameObject player = null;
        private FPPlayer fpplayer = null;
        private List<FPPlayer> fpplayers = null;
        //private InputHandler inputHandler = null;

        private GameObject crosshair = null;
        private GameObject stageHUD = null;
        private List<FPHudDigit> positionDigits = null;
        // Tilemap tm = null;
        //private Tilemap[] tms = null;
        //private TileBase copyTile = null;
        private int selectedTileLayer = 0;

        bool warped = false;

        string debugDisplay = "Never Updated";
        string warpMessage = "";

        float timeoutShowWarpInfo = 0f;
        float howLongToShowWarpInfo = 2f;

        public float dps = 0f;
        public List<float> dpsHits;
        public double dpsTimer = 0d;

        Vector2 warpPoint = new Vector2(211f, 50f);
        private bool showAllPlayers = false;

        private GameObject goFancyTextPosition;
        private TextMesh textmeshFancyTextPosition;

        private HashSet<string> playerValuesToShow;
        

        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            MelonLogger.Msg("OnApplicationStart");
            MelonPreferences.Load();
            InitPrefs();

            playerValuesToShow = new HashSet<string>();
            playerValuesToShow.Add("Pos");
            playerValuesToShow.Add("Vel");
            playerValuesToShow.Add("Ground Angle");
            playerValuesToShow.Add("Ground Velocity");
            playerValuesToShow.Add("Ceiling Angle");
            playerValuesToShow.Add("Sensor Angle");
            playerValuesToShow.Add("Gravity Angle");
            playerValuesToShow.Add("Gravity Strength");
        }

        private void InitPrefs()
        {
            fp2Trainer = MelonPreferences.CreateCategory("fp2Trainer");
            enableWarps = (MelonPreferences_Entry<bool>)fp2Trainer.CreateEntry<bool>("enableWarps", true);
            showDebug = (MelonPreferences_Entry<bool>)fp2Trainer.CreateEntry<bool>("showDebug", true);
            showLevelEditDebug = (MelonPreferences_Entry<bool>)fp2Trainer.CreateEntry<bool>("showLevelEditDebug", true);
            inputLETileCopy = (MelonPreferences_Entry<string>)fp2Trainer.CreateEntry<string>("inputLETileCopy", "<Gamepad>/buttonNorth");
            inputLETilePaste = (MelonPreferences_Entry<string>)fp2Trainer.CreateEntry<string>("inputLETilePaste", "<Gamepad>/buttonEast");
            inputLETileLayer = (MelonPreferences_Entry<string>)fp2Trainer.CreateEntry<string>("inputLETileLayer", "<Gamepad>/leftShoulder");
        }

        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            MelonLogger.Msg("OnSceneWasLoaded: " + buildindex.ToString() + " | " + sceneName);
            ResetSceneSpecificVariables();
            MelonPreferences.Save();
        }

        private void ResetSceneSpecificVariables()
        {
            player = null;
            fpplayers = new List<FPPlayer>();
            fpplayer = null;
            stageHUD = null;

            dps = 0;
            dpsHits = new List<float>();
            dpsTimer = 0;

            goFancyTextPosition = null;
            textmeshFancyTextPosition = null;
        }

        public void CreateFancyTextObjects()
        {
            GameObject goStageHUD = GameObject.Find("Stage HUD");
            if (goStageHUD == null)
            {
                return;
            }

            goFancyTextPosition = GameObject.Find("Resume Text");
            if (goFancyTextPosition != null)
            {
                goFancyTextPosition = GameObject.Instantiate((goFancyTextPosition));
                textmeshFancyTextPosition = goFancyTextPosition.GetComponent<TextMesh>();
            }
            else
            {
                goFancyTextPosition = new GameObject();
                textmeshFancyTextPosition = goFancyTextPosition.AddComponent<TextMesh>();
            }
            
            goFancyTextPosition.transform.parent = goStageHUD.transform;
            goFancyTextPosition.transform.localPosition = new Vector3(10, 110, 0);
            UpdateFancyText();
        }

        public void UpdateFancyText()
        {
            textmeshFancyTextPosition.text = debugDisplay;
        }

        public override void OnSceneWasInitialized(int buildindex, string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
        {
            MelonLogger.Msg("OnSceneWasInitialized: " + buildindex.ToString() + " | " + sceneName);
        }

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
            MelonLogger.Msg("OnApplicationQuit");
            MelonPreferences.Save();
        }

        public override void OnUpdate()
        {

            if (timeoutShowWarpInfo > 0) { timeoutShowWarpInfo -= Time.deltaTime; }
            if (timeoutShowWarpInfo < 0) { timeoutShowWarpInfo = 0; }
            try
            {

                if (player == null)
                {
                    UpdateDPS();
                    
                    player = GetFirstPlayerGameObject();
                    fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();

                    fpplayers = GetFPPlayers();
                    if (player != null)
                    {
                        MelonLogger.Msg("Trainer found a Player Object: ");
                    }
                }

                if (stageHUD != null)
                {
                    
                }
                else
                {
                    stageHUD = GetStageHUD();
                    if (stageHUD != null)
                    {
                        positionDigits = new List<FPHudDigit>();
                        for (int i = 0; i < 10; i++)
                        {
                            positionDigits.Add(stageHUD.AddComponent<FPHudDigit>());
                            if (i < 5)
                            {
                                positionDigits[i].transform.position = new Vector3(i * 16, 64,
                                    positionDigits[i].transform.position.z);
                            }
                            else
                            {
                                positionDigits[i].transform.position = new Vector3(i*16 + 16, 64, positionDigits[i].transform.position.z);
                            }
                        }
                    }
                }

                /*
                if (inputHandler == null)
                {
                    inputHandler = LevelManager.currentLevel.GetComponent<InputHandler>();
                    if (inputHandler != null)
                    {
                        MelonLogger.Msg("Trainer found the Input Handler.");
                    }
                }
                */

                
                UpdateLevelEditingInfo();
                //HandleTileEditControls();
                
                debugDisplay = "";
                if (player != null)
                {
                    debugDisplay += player.transform.ToString() + "\n";
                }

                if (fpplayer != null)
                {
                    HandleWarpControls();

                    if (playerValuesToShow.Contains("Pos"))
                    {
                        debugDisplay += "Pos: " + fpplayer.position.ToString() + "\n";
                    }
                    if (playerValuesToShow.Contains("Vel"))
                    {
                        debugDisplay += "Vel: " + fpplayer.velocity.ToString() + "\n";
                    }
                    if (playerValuesToShow.Contains("Ground Angle"))
                    {
                        debugDisplay += "Ground Angle: " + fpplayer.groundAngle.ToString() + "\n";
                    }
                    if (playerValuesToShow.Contains("Ground Velocity"))
                    {
                        debugDisplay += "Ground Velocity: " + fpplayer.groundVel.ToString() + "\n";
                    }
                    if (playerValuesToShow.Contains("Ceiling Angle"))
                    {
                        debugDisplay += "Ceiling Angle: " + fpplayer.ceilingAngle.ToString() + "\n";
                    }
                    if (playerValuesToShow.Contains("ensor Angle"))
                    {
                        debugDisplay += "Sensor Angle: " + fpplayer.sensorAngle.ToString() + "\n";
                    }
                    if (playerValuesToShow.Contains("Gravity Angle"))
                    {
                        debugDisplay += "Gravity Angle: " + fpplayer.gravityAngle.ToString() + "\n";
                    }
                    if (playerValuesToShow.Contains("Gravity Strength"))
                    {
                        debugDisplay += "Gravity Strength: " + fpplayer.gravityStrength.ToString() + "\n";
                    }
                }

                //debugDisplay += "Pos: " + player.transform.position.ToString() + "\n";
                if (showAllPlayers)
                {
                    foreach (FPPlayer fpplayer in fpplayers)
                    {
                        debugDisplay += "Pos: " + fpplayer.position.ToString() + "\n";
                        debugDisplay += "Vel: " + fpplayer.velocity.ToString() + "\n";
                    }
                }

                if (goFancyTextPosition != null)
                {
                    UpdateFancyText();
                }
                else
                {
                    CreateFancyTextObjects();
                }



            }
            catch (Exception e)
            {
                Fp2Trainer.Log("Trainer Error During Update: " + e.Message + "(" + e.InnerException?.Message + ") @@" + e.StackTrace);
            }
        }

        public void UpdateDPS()
        {
            dpsTimer += UnityEngine.Time.deltaTime;
        }

        private List<FPPlayer> GetFPPlayers()
        {
            if (FPStage.player != null && FPStage.player.Length > 0)
            {
                return new List<FPPlayer>(FPStage.player);
            }

            return null;
        }

        private GameObject GetFirstPlayerGameObject()
        {
            GameObject playerGameObject = null;
            if (FPStage.player == null) return playerGameObject;
            
            MelonLogger.Msg("Number of Stage Players: " + FPStage.player.Length.ToString());
            if (FPStage.currentStage != null && FPStage.currentStage.GetPlayerInstance_FPPlayer() != null)
            {
                playerGameObject = FPStage.currentStage.GetPlayerInstance_FPPlayer().gameObject;
            }
            else if (FPStage.player.Length > 0)
            {
                playerGameObject = FPStage.player[0].gameObject;
            }

            return playerGameObject;
        }

        public void HandleWarpControls()
        {
            if (InputControl.GetButtonDown(Controls.buttons.pause) && InputControl.GetButton(Controls.buttons.special))
            {
                fpplayer.position = new Vector2(warpPoint.x, warpPoint.y);
                debugDisplay += "Pause + Jump -> Goto Warp: " + warpPoint.ToString();
            }
            
            if (InputControl.GetButtonDown(Controls.buttons.pause) && InputControl.GetButton(Controls.buttons.jump))
            {
                warpPoint = new Vector2(fpplayer.position.x, fpplayer.position.y);
                debugDisplay += "Pause + Jump -> Set Warp: "  + warpPoint.ToString();
            }
        }

        public void HandleTileEditControls()
        {
            //Log("FP2Trainer v" + UMFMod.GetModVersion().ToString() + " - Starting FixedUpdate.", true);
            try
            {
                if (player != null && player.transform != null /*&& inputHandler != null*/)
                {
                    debugDisplay = "";

                    debugDisplay += "(inputLETileCopy) KeyPressed " + GetKeyPressed(inputLETileCopy.Value).ToString();
                    debugDisplay += "\r\n(inputLETileCopy) KeyDown " + GetKeyDown(inputLETileCopy.Value).ToString();

                    debugDisplay = "";
                    debugDisplay += "(inputLETilePaste) KeyPressed " + GetKeyPressed(inputLETilePaste.Value).ToString();
                    debugDisplay += "\r\n(inputLETilePaste) KeyDown " + GetKeyDown(inputLETilePaste.Value).ToString();

                    debugDisplay = "";
                    debugDisplay += "(inputLETileLayer) KeyPressed " + GetKeyPressed(inputLETileLayer.Value).ToString();
                    debugDisplay += "\r\n(inputLETileLayer) KeyDown " + GetKeyDown(inputLETileLayer.Value).ToString();

                    /*
                    debugDisplay += "CJump KeyPressed " + inputHandler.GetKeyPressed("CJump").ToString();
                    debugDisplay += "\r\nCJump KeyDown " + inputHandler.GetKeyDown("CJump").ToString();

                    debugDisplay += "Jump KeyPressed " + inputHandler.GetKeyPressed("Jump").ToString();
                    debugDisplay += "\r\nJump KeyDown " + inputHandler.GetKeyDown("Jump").ToString();
                    debugDisplay += "\r\nMenu KeyPressed " + inputHandler.GetKeyPressed("Menu").ToString();
                    debugDisplay += "\r\nMenu KeyDown " + inputHandler.GetKeyDown("Menu").ToString();
                    */

                    /*
                    if (tms != null && tms.Length > 0 && CrosshairIsValid())
                    {
                        HandleLevelEditorInputs();
                        if (selectedTileLayer >= tms.Length) { selectedTileLayer = 0; };
                        if (selectedTileLayer < 0) { selectedTileLayer = tms.Length; };
                    }
                    */

                    /*
                    if (
                        (inputHandler.GetKeyPressed("CDown")
                        && inputHandler.GetKeyPressed("CJump") //KEY PRESSED AND KEY DOWN ARE DIFFERENT THAN I'M USED TO HERE.
                        && inputHandler.GetKeyPressed("CPrimary")
                        && inputHandler.GetKeyPressed("CSecondary")
                        && inputHandler.GetKeyPressed("CSpecial")
                        ) // Cassie's Combo Input
                        ||
                        (inputHandler.GetKeyPressed("Down")
                        && inputHandler.GetKeyPressed("Jump") //KEY PRESSED AND KEY DOWN ARE DIFFERENT THAN I'M USED TO HERE.
                        && inputHandler.GetKeyPressed("Primary")
                        && inputHandler.GetKeyPressed("Secondary")
                        && inputHandler.GetKeyPressed("Special")
                        ) // Alpha's Combo Input

                        ||
                        (
                            inputHandler.GetKeyPressed("Menu")
                            && (inputHandler.GetKeyPressed("CSpecial") || inputHandler.GetKeyPressed("Special"))
                        ) // Alternate Pause + Special warp.
                        ) //  Down + A + E + RB + RT
                    {
                        player.transform.position = warpPoint;
                        debugDisplay += "/r/nWarping to " + warpPoint.ToString();

                        warpMessage = "Warping to " + warpPoint.ToString();
                        timeoutShowWarpInfo = howLongToShowWarpInfo;

                        //WriteSceneObjectsToFile();
                        //WriteAllAudioclipsToFile();
                    }
                    */

                    // Set warp point to current position.  A + Start
                    /*
                    if (inputHandler.GetKeyPressed("Menu") && (inputHandler.GetKeyPressed("CJump") || inputHandler.GetKeyPressed("Jump")))
                    {
                        warpPoint = player.transform.position;
                        debugDisplay += "/r/nWarp Point Set. " + warpPoint.ToString();

                        warpMessage = "Warp Point Set." + warpPoint.ToString();
                        timeoutShowWarpInfo = howLongToShowWarpInfo;
                    }
                    */

                    // Reset to Default Warp. X + Start
                    /*
                    if (inputHandler.GetKeyPressed("Menu") && (inputHandler.GetKeyPressed("CPrimary") || inputHandler.GetKeyPressed("Primary")))
                    {
                        warpPoint = new Vector2(211f, 50f);
                        debugDisplay += "/r/nWarp Point Cleared. " + warpPoint.ToString();

                        warpMessage = "Warp Point Cleared." + warpPoint.ToString();
                        timeoutShowWarpInfo = howLongToShowWarpInfo;
                    }
                    */
                }
                //Log("FP2Trainer v" + UMFMod.GetModVersion().ToString() + " - End of FixedUpdate.", true);
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Trainer Error During FixedUpdate: " + e.Message + "(" + e.InnerException?.Message + ") @@" + e.StackTrace);
            }
        }

        public override void OnGUI() // Can run multiple times per frame. Mostly used for Unity's IMGUI.
        {
            if (showDebug.Value)
            {
                Rect r = new Rect(10, 110, 500, 200);
                GUI.Box(r, debugDisplay);
            }

            if (timeoutShowWarpInfo > 0)
            {
                Rect r = new Rect(10, 10, 200, 32);
                GUI.Box(r, warpMessage);
            }

            DrawLevelEditingInfo();
        }

        void WriteSceneObjectsToFile()
        {
            if (!warped)
            {
                warped = true;

                string allObjects = "";
                UnityEngine.Object[] objs = UnityEngine.GameObject.FindObjectsOfType<GameObject>();

                foreach (UnityEngine.Object obj in objs)
                {
                    allObjects += obj.name + "\r\n";
                }
                // UMFGUI.AddConsoleText(allObjects);

                string fileName = "SceneObjects.txt";
                if (File.Exists(fileName))
                {
                    Debug.Log(fileName + " already exists.");
                    return;
                }
                var sr = File.CreateText(fileName);
                sr.WriteLine(allObjects);
                sr.Close();
            }
            else
            {
                MelonLogger.Msg("Warped already...");
            }
        }

        void WriteAllAudioclipsToFile()
        {
            if (!warped)
            {
                warped = true;

                string allAudioClips = "";
                UnityEngine.Object[] acs = Resources.FindObjectsOfTypeAll<AudioClip>();

                foreach (AudioClip ac in acs)
                {
                    allAudioClips += ac.name + "\r\n";
                }
                // UMFGUI.AddConsoleText(allObjects);

                var fileName = "AllAvailableAudioClips.txt";
                if (File.Exists(fileName))
                {
                    Debug.Log(fileName + " already exists.");
                    return;
                }
                var sr = File.CreateText(fileName);
                sr.WriteLine(allAudioClips);
                sr.Close();
            }
            else
            {
                MelonLogger.Msg("Warped already...");
            }
        }

        public void UpdateLevelEditingInfo()
        {
            
        }

        private bool CrosshairIsValid()
        {
            return crosshair != null && crosshair.transform != null && crosshair.transform.position != null;
        }

        public void DrawLevelEditingInfo()
        {
            
        }

        public void SetInputHandlerLevelEditorKeys()
        {
        }

        public void HandleLevelEditorInputs()
        {
            if (GetKeyPressed(inputLETileLayer.Value))
            {
                // Increase and decrease layer by tapping the copy and past buttons while this is held.
                if (GetKeyDown(inputLETileCopy.Value))
                {
                    selectedTileLayer += 1;
                }
                if (GetKeyDown(inputLETilePaste.Value))
                {
                    selectedTileLayer -= 1;
                }

            }
            else
            {
                // Copy and paste tiles when the Layer button is not held.
                if (GetKeyDown(inputLETileCopy.Value))
                {
                    /*
                    if (crosshair != null && crosshair.transform != null)
                    {
                        Vector3Int cellPosition = tms[selectedTileLayer].WorldToCell(crosshair.transform.position);
                        //String tileName = "NULL";
                        copyTile = tms[selectedTileLayer].GetTile(cellPosition);
                    }
                    */
                }
                if (GetKeyDown(inputLETilePaste.Value))
                {
                    /*
                    if (crosshair != null && crosshair.transform != null)
                    {
                        Vector3Int cellPosition = tms[selectedTileLayer].WorldToCell(crosshair.transform.position);
                        //String tileName = "NULL";
                        tms[selectedTileLayer].SetTile(cellPosition, copyTile);
                    }
                    */
                }
            }


        }

        public bool GetKeyPressed(string s)
        {
            try
            {
                String sTrim = s.Split('/')[1];
                //if (inputHandler != null)
                {
                    return false;
                    /*if (s.Contains("<Mouse>"))
                    {
                        return ((ButtonControl)Mouse.current[sTrim]).isPressed;
                    }
                    if (s.Contains("<Keyboard>"))
                    {
                        return ((KeyControl)Keyboard.current[sTrim]).isPressed;
                    }
                    if (s.Contains("<Gamepad>"))
                    {
                        return ((ButtonControl)Gamepad.current[sTrim]).isPressed;
                    }*/
                }
            }
            catch (Exception e)
            {
                // I should probably do a log here.
                MelonLogger.Msg(e.Message);
            }
            return false;
        }

        public bool GetKeyDown(string s)
        {
            try
            {
                String sTrim = s.Split('/')[1];
                //if (inputHandler != null)
                {
                    return false;
                    
                    /*
                    if (s.Contains("<Mouse>"))
                    {
                        return ((ButtonControl)Mouse.current[sTrim]).wasPressedThisFrame;
                    }
                    if (s.Contains("<Keyboard>"))
                    {
                        return ((KeyControl)Keyboard.current[sTrim]).wasPressedThisFrame;
                    }
                    if (s.Contains("<Gamepad>"))
                    {
                        return ((ButtonControl)Gamepad.current[sTrim]).wasPressedThisFrame;
                    }*/
                }
            }
            catch (Exception e)
            {
                // I should probably do a log here.
                MelonLogger.Msg(e.Message);
            }
            return false;
        }

        public string GetCopiedTileName()
        {
            string result = "NULL";
            //f (copyTile != null) { result = copyTile.name; }
            return result;
        }

        public GameObject GetStageHUD()
        {
            GameObject goHud = GameObject.Find("Stage HUD");
            if (goHud)
            {
                MelonLogger.Msg("Found a Stage HUD.");
            }
            //GameObject goPauseHud = GameObject.Find("Hud Pause Menu");

            return goHud;
        }

        public static void Log(String txt)
        {
            MelonLogger.Msg(txt);
        }
    }
}