using MelonLoader;

using UnityEngine;
//using UnityEngine.InputSystem.Controls;
//using UnityEngine.InputSystem;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using UnityEngine.SceneManagement;

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
        private GameObject stageSelectMenu = null;
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
        private GameObject goStageHUD;
        private TextMesh textmeshFancyTextPosition;

        public Font fpMenuFont;

        private HashSet<string> playerValuesToShow;

        public string sceneToLoad = "";

        public List<AssetBundle> loadedAssetBundles;


        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            MelonLogger.Msg("OnApplicationStart");
            MelonPreferences.Load();
            InitPrefs();

            loadedAssetBundles = new List<AssetBundle>();

            playerValuesToShow = new HashSet<string>();
            playerValuesToShow.Add("Pos");
            playerValuesToShow.Add("Vel");
            playerValuesToShow.Add("Ground Angle");
            playerValuesToShow.Add("Ground Velocity");
            playerValuesToShow.Add("Ceiling Angle");
            playerValuesToShow.Add("Sensor Angle");
            playerValuesToShow.Add("Gravity Angle");
            playerValuesToShow.Add("Gravity Strength");
            playerValuesToShow.Add("HUD Position");
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
            AttemptToFindFPFont();
            AttemptToFindPauseMenu();
            MelonPreferences.Save();
        }

        private void AttemptToFindPauseMenu()
        {
            if (stageSelectMenu == null)
            {
                foreach (FPPauseMenu pauseMenu in Resources.FindObjectsOfTypeAll(typeof(FPPauseMenu)) as FPPauseMenu[])
                {
                    stageSelectMenu = GameObject.Instantiate(pauseMenu.transform.gameObject);
                    Log("Found a pauseMenu to modify.");
                    stageSelectMenu.name = "Ann Stage Select Menu";
                    break;
                }
            }
        }

        private void AttemptToFindFPFont()
        {
            if (fpMenuFont != null)
            {
                return;
            }

            foreach (UnityEngine.TextMesh textMesh in Resources.FindObjectsOfTypeAll(typeof(UnityEngine.TextMesh)) as UnityEngine.TextMesh[])
            {
                if (textMesh.font!= null && textMesh.font.name.Equals("FP Menu Font"))
                {
                    Log("Found the FP Menu Font loaded in memory. Saving reference.");
                    fpMenuFont = textMesh.font;
                    break;
                }
            }
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
            goStageHUD = null;
            textmeshFancyTextPosition = null;
        }

        public void CreateFancyTextObjects()
        {
            goStageHUD = GameObject.Find("Stage HUD");
            //GameObject goStageHUD = GameObject.Find("Hud Pause Menu");
            if (goStageHUD == null)
            {
                return;
            }

            Log("Successfully found HUD to attach text to.");
            goFancyTextPosition = GameObject.Find("Resume Text");
            if (goFancyTextPosition != null)
            {
                Log("Found Resume Text");
                goFancyTextPosition = GameObject.Instantiate(goFancyTextPosition);
                goFancyTextPosition.SetActive(true);
                textmeshFancyTextPosition = goFancyTextPosition.GetComponent<TextMesh>();
                textmeshFancyTextPosition.font = fpMenuFont;
                textmeshFancyTextPosition.characterSize = 10;
                Log("Successfully cloned Resume Text. Attaching to Stage HUD.");
            }
            else if (goStageHUD != null)
            {
                //goStageHUD.energyBarGraphic.transform.parent;
                Log("Looking for Energy Bar");
                var temp = goStageHUD.GetComponent<FPHudMaster>();
                Log("2");
                GameObject temp2;
                Log("3");
                if (temp != null)
                {
                    Log("4");
                    temp2 = temp.pfHudEnergyBar;
                }
                else
                {
                    Log("5");
                    Log("This aint it.");
                    return;
                }
                
                Log("6");

                var energyBarGraphic = UnityEngine.Object.Instantiate(temp2);
                Log("7");
                energyBarGraphic.transform.parent = temp2.transform.parent;
                Log("8");
                energyBarGraphic.transform.localScale *= 2;

                Log("9");
                goFancyTextPosition = energyBarGraphic;
                Log("9a");
                goFancyTextPosition.SetActive(true);
                Log("9b");
                //GameObject.Destroy(goFancyTextPosition.GetComponent<SpriteRenderer>()); // Can't have Sprite Renderer and Mesh Renderer.
                var tempGo = new GameObject();
                tempGo.transform.parent = goFancyTextPosition.transform;
                tempGo.transform.localPosition = Vector3.zero;

                goFancyTextPosition.transform.position += new Vector3(16, -320, 0);
                goFancyTextPosition = tempGo;
                
                textmeshFancyTextPosition = goFancyTextPosition.AddComponent<TextMesh>();
                Log("9c");
                if (textmeshFancyTextPosition != null)
                {
                    Log("Current value of fpMenuFont: " + fpMenuFont);
                    textmeshFancyTextPosition.font = fpMenuFont;
                    Log("9d");
                    textmeshFancyTextPosition.characterSize = 10;
                    Log("9e");
                    textmeshFancyTextPosition.text = "I exist!@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n@@@@@@@@@@@@@@@@@@@@@@@@";
                    Log("Attempting to clone energyBar. Attaching to Stage HUD.");
                    Log("9f");
                }
                else
                {
                    Log("Tried to create textMesh but failed.");
                }

                
            }
            else
            {
                goFancyTextPosition = new GameObject();
                textmeshFancyTextPosition = goFancyTextPosition.AddComponent<TextMesh>();
                textmeshFancyTextPosition.font = fpMenuFont;
                textmeshFancyTextPosition.characterSize = 10;
                Log("Could not clone Resume Text or Energy Bar. Manually creating TextMesh and Attaching to Stage HUD.");
                
                
                
                //Log("Could not clone Resume Text. Canceling.");
                //return;
            }
            
            Log("10a");
            goFancyTextPosition.transform.parent = goStageHUD.transform;
            Log("10b");
            goFancyTextPosition.transform.localPosition = new Vector3(10, 20, 0);
            Log("1c");
            UpdateFancyText();
            Log("10d");
        }

        public void UpdateFancyText()
        {
            if (textmeshFancyTextPosition != null)
            {
                textmeshFancyTextPosition.text = debugDisplay;   
            }
            if (fpplayer != null && goFancyTextPosition != null)
            {
                //goFancyTextPosition.transform.position = new Vector3(fpplayer.position.x - 10, fpplayer.position.y - 10, -1);
                goFancyTextPosition.transform.position += new Vector3(16, -320, 0);
            }
        }

        public override void OnSceneWasInitialized(int buildindex, string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
        {
            MelonLogger.Msg("OnSceneWasInitialized: " + buildindex.ToString() + " | " + sceneName);
            SkipBootIntros();
        }

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
            MelonLogger.Msg("OnApplicationQuit");
            MelonPreferences.Save();
        }

        public override void OnUpdate()
        {
            SkipBootIntros();
            
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
                    
                    if (goStageHUD != null && playerValuesToShow.Contains("HUD Position"))
                    {
                        debugDisplay += "HUD Position: " + goStageHUD.GetComponent<FPHudMaster>().hudPosition.ToString() + "\n";
                        debugDisplay += "HUD Position (base): " + goStageHUD.GetComponent<FPHudMaster>().transform.position.ToString() + "\n";
                    }
                    
                    if (goFancyTextPosition != null)
                    {
                        debugDisplay += "FancyText Position: " + goFancyTextPosition.transform.position.ToString() + "\n";
                        debugDisplay += "FancyText Position (local): " + goFancyTextPosition.transform.localPosition.ToString() + "\n";
                        if (fpplayer != null)
                        {
                            debugDisplay += "hudCrystalIcon: " + goStageHUD.GetComponent<FPHudMaster>().pfHudCrystalIcon.transform.position.ToString() + "\n";
                        }
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
            //if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.attack))
            if (Input.GetKeyUp(KeyCode.F9))
            {
                Log("F9 -> Load Debug Room");
                UnityEngine.SceneManagement.SceneManager.LoadScene("StageDebugMenu", LoadSceneMode.Additive);
            }
            
            if (Input.GetKeyUp(KeyCode.F8))
            {
                Log("F8 -> Main Menu");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
            }
            if (Input.GetKeyUp(KeyCode.F7))
            {
                Log("F7 -> Load Asset Bundles");
                LoadAssetBundlesFromModsFolder();
            }
            if (Input.GetKeyUp(KeyCode.F6))
            {
                Log("F6 -> Level Select");
                List<Scene> availableScenes = new List<Scene>(); 
                for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++) 
                {
                    availableScenes.Add(UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(i));
                    Log(i.ToString() + " | " + availableScenes[i].name);
                }
                ShowLevelSelect(availableScenes);
            }
            if (Input.GetKeyUp(KeyCode.F5))
            {
                Log("F5 -> Toggle Level Select Menu Visibility");
                ToggleLevelSelectVisibility();
            }
            
            
            if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.special))
            {
                fpplayer.position = new Vector2(warpPoint.x, warpPoint.y);
                Log("Hold Guard + Tap Special -> Goto Warp: " + warpPoint.ToString());
            }
            
            if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.jump))
            {
                warpPoint = new Vector2(fpplayer.position.x, fpplayer.position.y);
                Log("Hold Guard + Tap Jump -> Set Warp: "  + warpPoint.ToString());
            }
        }

        public void LoadAssetBundlesFromModsFolder()
        {
            try
            {
                var pathApp = Application.dataPath;
                var pathMod = Path.Combine(Directory.GetParent(pathApp).FullName, "Mods");
                var pathModAssetBundles = Path.Combine(pathMod, "AssetBundles");

                var assetBundlePaths = Directory.GetFiles(pathModAssetBundles, "*.*");
                foreach (var abp in assetBundlePaths)
                {
                    Log(abp);
                    if (abp.Contains("."))
                    {
                        Log("Skipping this file, as it appears to have a " +
                            "file extension (.whatever) at the end, " +
                            "and is probably not an asset bundle.");
                        continue;
                    }
                    
                    var currentAB = AssetBundle.LoadFromFile(abp);

                    if (currentAB == null)
                    {
                        Log("Failed to load AssetBundle. File may be corrupt.");
                        continue;
                    }

                    loadedAssetBundles.Add(currentAB);
                    Log("AssetBundle loaded successfully as loadedAssetBundles[" + (loadedAssetBundles.Count - 1).ToString() + "]:");
                    Log("--------");
                    Log(currentAB.GetAllScenePaths().ToString());
                }
            }
            catch (NullReferenceException e)
            {
                Log("Null reference exception when trying to load asset bundles for modding. Canceling.");
                Log(e.StackTrace);
            }

            
        }

        public void ToggleLevelSelectVisibility()
        {
            if (stageSelectMenu != null)
            {
                stageSelectMenu.SetActive(stageSelectMenu.activeInHierarchy);
            }
        }

        private void ShowLevelSelect(List<Scene> availableScenes)
        {
            if (stageSelectMenu != null)
            {
                Log("Level Select.");
                stageSelectMenu.SetActive(true);
                
                var ssm = stageSelectMenu.GetComponent<FPPauseMenu>();
                GameObject goButton = null;
                
                if (ssm.pfButtons.Length > 3)
                {
                    goButton = ssm.pfButtons[3]; // This is most likely the resume button.
                }
                else if (ssm.pfButtons.Length > 0)
                {
                    goButton = ssm.pfButtons[0];
                }

                int i;
                
                for (i= 0; i < ssm.pfButtons.Length; i++) 
                {
                    if (ssm.pfButtons[i] != goButton)
                    {
                        GameObject.Destroy(ssm.pfButtons[i]);
                    }
                }

                ssm.pfButtons = new GameObject[availableScenes.Count];

                GameObject currentButton = null;
                TextMesh tm = null;
                MenuText mt = null;
                for (i = 0; i < availableScenes.Count; i++)
                {
                    currentButton = GameObject.Instantiate(goButton);
                    currentButton.transform.position += new Vector3(0, 32 * i, 0);
                    
                    tm = currentButton.GetComponent<TextMesh>();
                    mt = currentButton.GetComponent<MenuText>();
                    ssm.pfButtons.SetValue(currentButton, i);
                    if (tm != null)
                    {
                        tm.text = availableScenes[i].name;
                        mt.paragraph[0] = availableScenes[i].name;
                    }
                }

                /*
                ssm.pfText = new MenuText[ssm.pfButtons.Length];
                for (int i = 0; i < buttonCount; i++)
                {
                    pfText[i] = pfButtons[i].GetComponentInChildren<MenuText>();
                }
                */
                //base.transform.position = new Vector3(320f, -180f, 0f);
                
                //ssm.Start()
                ssm.Invoke("Start", 0);
            }
            else
            {
                Log("Attempted to show level select, but the menu has not been prepared.");
            }
        }

        public void PerformStageTransition()
        {
            FPScreenTransition component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            component.transitionType = FPTransitionTypes.LOCAL_WIPE;
            component.transitionSpeed = 48f;
            component.SetTransitionColor(0f, 0f, 0f);
            component.BeginTransition();
            FPAudio.PlayMenuSfx(3);
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

        public void SkipBootIntros()
        {
            return; //This is buggy AF and I just want it off for now.
            if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
            {
                Log("Attempting to Skip Main Menu Intros");
                var temp = GameObject.Find("Screen Transition");
                if (temp != null)
                {
                    var temp2 = temp.GetComponent<FPScreenTransition>();
                    if (temp2 != null)
                    {
                        temp2.transitionSpeed = 100f;
                        temp2.loadingBarDuration = 1;
                    }
                    else
                    {
                        Log("No FPScreenTransition Component.");
                    }
                }
                else
                {
                    Log("No Screen Transition Object");
                }
            }
        }
        
        private IEnumerator LoadAsyncScene()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        public static void Log(String txt)
        {
            MelonLogger.Msg(txt);
        }
    }
}