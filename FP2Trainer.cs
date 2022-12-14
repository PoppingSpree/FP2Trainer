using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = System.Random;

//using UnityEngine.InputSystem.Controls;
//using UnityEngine.InputSystem;

//using UnityEngine.Tilemaps;

namespace Fp2Trainer
{
    public static class BuildInfo
    {
        public const string Name = "FP2 Trainer"; // Name of the Mod.  (MUST BE SET)

        public const string Description =
            "Training tools for speedrunning Freedom Planet 2: Quick Warps, Quick Resets, Live Tilemap Editing, etc"; // Description for the Mod.  (Set as null if none)

        public const string Author = "Catssandra Ann"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "0.9.1"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class Fp2Trainer : MelonMod
    {
        public static GameObject goFP2Trainer;
        public static PlaneSwitcherVisualizer planeSwitcherVisualizer;
        public static GameObject goFP2TrainerYourPlayerIndicator;
        
        public static DateTime fp2ReleaseDate = new DateTime(2022, 9, 13, 12, 0, 0);
        public static int fp2ReleaseDateInt = 20220913;

        public enum DataPage
        {
            MOVEMENT,
            MOVEMENT_2,
            COMBAT,
            DPS,
            DPS_ALL,
            MULTIPLAYER_DEBUG,
            BOSS,
            NO_CLIP,
            CAMERA,
            CAMERA_ALL,
            NONE
        }

        public enum InstructionPage
        {
            BASICS,
            BACKUPS,
            SPEEDRUN,
            QUICK_RESTART,
            NO_CLIP,
            MULTICHARACTER,
            //CHAR_INSTASWAP,

            //NETPLAY,
            BUGS,
            HOTKEYS_1,
            HOTKEYS_2,
            HOTKEYS_3,
            HOTKEYS_4,
            HOTKEYS_5,
            HOTKEYS_6,
            HOTKEYS_7,
            HOTKEYS_8,
            //HOTKEYS_9,
            //HOTKEYS_10,
            //HOTKEYS_11,
            QUICKBOOT,
            NONE
        }

        public static MelonPreferences_Category fp2Trainer;
        public static MelonPreferences_Entry<bool> enableWarps;
        public static MelonPreferences_Entry<bool> showDebug;
        public static MelonPreferences_Entry<bool> showLevelEditDebug;
        public static MelonPreferences_Entry<bool> enableNoClip;

        public static MelonPreferences_Entry<string> BootupLevel;

        public static MelonPreferences_Entry<string> PHKToggleInstructions;

        public static MelonPreferences_Entry<string> PHKSetWarpPoint;
        public static MelonPreferences_Entry<string> PHKGotoWarpPoint;

        public static MelonPreferences_Entry<string> PHKKOCharacter;
        public static MelonPreferences_Entry<string> PHKKOBoss;
        
        public static MelonPreferences_Entry<string> PHKInvinciblePlayers;
        public static MelonPreferences_Entry<string> PHKInvincibleBoss;

        public static MelonPreferences_Entry<string> PHKToggleNoClip;

        public static MelonPreferences_Entry<string> PHKSpawnExtraChar;
        public static MelonPreferences_Entry<string> PHKSwapBetweenSpawnedChars;
        public static MelonPreferences_Entry<string> PHKToggleMultiCharStart;
        public static MelonPreferences_Entry<string> PHKCyclePreferredAllyControlType;
        public static MelonPreferences_Entry<string> PHKStartSplitscreen;
        public static MelonPreferences_Entry<bool> EnableGetPlayerInstanceMultiplayerPatch;
        public static MelonPreferences_Entry<bool> EnableSplitScreen;
        
        public static MelonPreferences_Entry<string> PHKSwitchCurrentPlayerToLilac;
        public static MelonPreferences_Entry<string> PHKSwitchCurrentPlayerToCarol;
        public static MelonPreferences_Entry<string> PHKSwitchCurrentPlayerToCarolBike;
        public static MelonPreferences_Entry<string> PHKSwitchCurrentPlayerToMilla;
        public static MelonPreferences_Entry<string> PHKSwitchCurrentPlayerToNeera;
        public static MelonPreferences_Entry<string> PHKSwitchCurrentPlayerToNext;
        public static MelonPreferences_Entry<string> PHKSwitchCurrentPlayerToPrev;
        public static MelonPreferences_Entry<bool> UseInstaSwitch;
        
        public static MelonPreferences_Entry<string> PHKStartInputPlayback;
        public static MelonPreferences_Entry<string> PHKToggleLockP1ToGhostFiles;
        public static MelonPreferences_Entry<string> PHKToggleLockP2ToGhostFiles;
        public static MelonPreferences_Entry<string> PHKToggleLockP3ToGhostFiles;
        public static MelonPreferences_Entry<string> PHKToggleLockP4ToGhostFiles;

        public static MelonPreferences_Entry<string> PHKGetOutGetOutGetOut;

        public static MelonPreferences_Entry<string> PHKCameraZoomIn;
        public static MelonPreferences_Entry<string> PHKCameraZoomOut;
        public static MelonPreferences_Entry<string> PHKCameraZoomReset;

        public static MelonPreferences_Entry<string> PHKShowNextDataPage;
        public static MelonPreferences_Entry<string> PHKShowPreviousDataPage;
        public static MelonPreferences_Entry<string> PHKHideDataView;

        public static MelonPreferences_Entry<string> PHKGoToMainMenu;
        public static MelonPreferences_Entry<string> PHKLoadDebugRoom;

        public static MelonPreferences_Entry<string> PHKGoToLevelSelectMenu;

        public static MelonPreferences_Entry<string> PHKLoadAssetBundles;

        //public static MelonPreferences_Entry<string> PHKTogglePauseMenuOrTrainerMenu;
        public static MelonPreferences_Entry<string> PHKGoToLevelAtLastIndex;

        public static MelonPreferences_Entry<string> PHKIncreaseFontSize;
        public static MelonPreferences_Entry<string> PHKDecreaseFontSize;

        public static MelonPreferences_Entry<string> PHKReturnToCheckpoint;
        public static MelonPreferences_Entry<string> PHKRestartStage;

        public static MelonPreferences_Entry<string> PHKTogglePlaneSwitcherVisualizers;
        public static MelonPreferences_Entry<string> PHKToggleShowColliders;

        public static MelonPreferences_Entry<string> PHKToggleRecordGhostData;
        public static MelonPreferences_Entry<string> PHKToggleEnableNetworkPlayers;

        public static MelonPreferences_Entry<string> PHKRebindAllHotkeys;


        public static MelonPreferences_Entry<bool> MultiCharStartLastSetting;
        public static MelonPreferences_Entry<bool> ShowPlaneSwitcherVisualizersLastSetting;
        public static MelonPreferences_Entry<bool> ShowCollidersLastSetting;
        public static MelonPreferences_Entry<string> PreferredAllyControlTypeLastSetting;
        public static MelonPreferences_Entry<bool> ShowInstructionsOnStart;
        public static MelonPreferences_Entry<bool> ShowInputNamesInTerminal;
        public static MelonPreferences_Entry<bool> EnableNetworking;
        public static MelonPreferences_Entry<bool> SaveGhostFiles;
        public static MelonPreferences_Entry<int> MultiCharStartNumChars;

        public static MelonPreferences_Entry<bool> LockP1ToGhostFiles;
        public static MelonPreferences_Entry<string> DEBUG_LoadSpecificGhostFile;
        public static MelonPreferences_Entry<string> DEBUG_LoadSpecificGhostFileP2;
        public static MelonPreferences_Entry<string> DEBUG_LoadSpecificGhostFileP3;
        public static MelonPreferences_Entry<string> DEBUG_LoadSpecificGhostFileP4;
        
        public static MelonPreferences_Entry<bool> EnableInvinciblePlayers;
        public static MelonPreferences_Entry<bool> EnableInvincibleBoss;
        
        public static MelonPreferences_Entry<bool> SixtyFPSHack;
        public static MelonPreferences_Entry<bool> DeterministicMode;
        
        public static MelonPreferences_Entry<bool> DisplayNametags;

        public static bool hotkeysLoaded = false;

        //public static MelonPreferences_Entry<string> PHK;

        // public static MelonPreferences_Entry<string> PHKSaveTrainerData;
        // public static MelonPreferences_Entry<string> PHKLoadTrainerData;


        public static Fp2Trainer fp2TrainerInstance;

        private static float fp2tDeltaTime;
        //private InputHandler inputHandler = null;

        public static int introSkipped = 0;

        public static Font fpMenuFont;
        public static Material fpMenuMaterial;

        public Dictionary<int, float> allActiveEnemiesHealth;
        public Dictionary<int, float> allActiveEnemiesHealthPrevious;
        public Dictionary<int, string> allActiveEnemiesNames;
        private List<FPBossHud> bossHuds;

        public bool noClip;
        public float noClipMoveSpeed = 30f;
        public Vector2 noClipStartPos = Vector2.zero;
        public int noClipCollisionLayer = -0;
        public float noClipGravityStrength = -0.7f;

        private DataPage currentDataPage = DataPage.MOVEMENT;
        private InstructionPage currentInstructionPage = InstructionPage.BASICS;
        public static bool showInstructions = true;

        public static string debugDisplay = "Never Updated";

        public float dps;
        public List<float> dpsHits;
        public double dpsTimer;

        public FP2TrainerDPSTracker dpsTracker;

        public Dictionary<int, string> fpElementTypeNames;
        private List<FPBaseEnemy> fpEnemies;

        public static FPPlayer fpplayer;
        public static List<FPPlayer> fpplayers;

        private FPTrainerLevelSelect fptls;

        private GameObject goDtTracker;

        public static GameObject goFancyTextPosition;
        public GameObject goStageHUD;
        private readonly float howLongToShowWarpInfo = 2f;

        public static List<AssetBundle> loadedAssetBundles;

        private FPBaseEnemy nearestEnemy;
        private FPBaseEnemy nearestEnemyPrevious;
        private float nearestEnemyPreviousHP;
        private FPPauseMenu pauseMenu;

        private GameObject player;

        private HashSet<string> playerValuesToShow;
        private List<FPHudDigit> positionDigits;

        public string sceneToLoad = "";

        public static bool multiplayerStart = false;
        public static bool doneMultiplayerStart = false;

        public bool showVarString = true;
        private GameObject stageHUD;
        private GameObject stageSelectMenu;
        public TextMesh textmeshFancyTextPosition;

        private float timeoutShowWarpInfo;

        private static bool warped;
        private string warpMessage = "";

        private Vector2 warpPoint = new Vector2(211f, 50f);

        public float trainerZoomMin = 0.05f;
        public float trainerZoomMax = 10f;
        public float trainerZoomSpeed = 0.1f;
        public float trainerRequestZoomValue = 1f;

        public float originalZoomMin = 0.5f;
        public float originalZoomMax = 2f;
        public float originalZoomSpeed = 0.1f;
        public GameObject lifePetal;
        public GameObject shield;

        public static Dictionary<string, KeyMapping> customHotkeys;

        public bool planeSwitchVisualizersCreated = false;
        public bool planeSwitchVisualizersVisible = false;
        public List<GameObject> planeSwitchVisualizers;

        public static GameObject cacheGameObjectHunter = null;

        public static bool waitingForNextFrameForSpoilerGimmick = false;

        public FPStage fpStage;
        public FP2TrainerInputQueue p1inputQueue;

        public static bool skipRecording = false;

        public static FPCharacterID currentPreferredCharacter = FPCharacterID.LILAC;

        public static Dictionary<int, string> FPLayerNames;

        public static List<SplitScreenCamInfo> SplitScreenCameraInfos = new List<SplitScreenCamInfo>();

        private bool instaSwitchCharsSpawned = false;

        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            fp2TrainerInstance = this;

            MelonLogger.Msg("OnApplicationStart");
            MelonPreferences.Load();
            InitPrefs();

            loadedAssetBundles = new List<AssetBundle>();

            playerValuesToShow = new HashSet<string>();
            playerValuesToShow.Add("Pos");
            playerValuesToShow.Add("Vel");
            playerValuesToShow.Add("Magnitude");
            playerValuesToShow.Add("InflictedDamage");
            playerValuesToShow.Add("Ground Angle");
            playerValuesToShow.Add("Ground Velocity");
            playerValuesToShow.Add("Ceiling Angle");
            playerValuesToShow.Add("Sensor Angle");
            playerValuesToShow.Add("Gravity Angle");
            playerValuesToShow.Add("Gravity Strength");
            playerValuesToShow.Add("HUD Position");

            fpElementTypeNames = new Dictionary<int, string>();
            fpElementTypeNames.Add(-1, "Normal");
            fpElementTypeNames.Add(0, "Wood");
            fpElementTypeNames.Add(1, "Earth");
            fpElementTypeNames.Add(2, "Water");
            fpElementTypeNames.Add(3, "Fire");
            fpElementTypeNames.Add(4, "Metal");

            bossHuds = new List<FPBossHud>();
            allActiveEnemiesHealth = null;
            allActiveEnemiesHealthPrevious = null;
            dpsTracker = new FP2TrainerDPSTracker();
            introSkipped = 0;

            FPPlayer2p.InitCustomControls();

            multiplayerStart = MultiCharStartLastSetting.Value;
            planeSwitchVisualizersVisible = ShowPlaneSwitcherVisualizersLastSetting.Value;
            FP2TrainerAllyControls.preferredAllyControlType = (AllyControlType)(Enum.Parse(typeof(AllyControlType),
                PreferredAllyControlTypeLastSetting.Value));
            showInstructions = ShowInstructionsOnStart.Value;

            InitFPLayerNames();
        }

        private void InitPrefs()
        {
            fp2Trainer = MelonPreferences.CreateCategory("fp2Trainer");
            enableWarps = fp2Trainer.CreateEntry("enableWarps", true);
            BootupLevel = fp2Trainer.CreateEntry("bootupLevel", "");
            showDebug = fp2Trainer.CreateEntry("showDebug", true);
            enableNoClip = fp2Trainer.CreateEntry("enableNoClip", false);

            MultiCharStartLastSetting = fp2Trainer.CreateEntry("MultiCharStartLastSetting", false);
            ShowPlaneSwitcherVisualizersLastSetting =
                fp2Trainer.CreateEntry("ShowPlaneSwitcherVisualizersLastSetting", false);
            ShowCollidersLastSetting = fp2Trainer.CreateEntry("ShowCollidersLastSetting", true);
            PreferredAllyControlTypeLastSetting = fp2Trainer.CreateEntry("PreferredAllyControlTypeLastSetting",
                FP2TrainerAllyControls.AllyControlTypeName(AllyControlType.SINGLE_PLAYER));
            ShowInstructionsOnStart = fp2Trainer.CreateEntry("ShowInstructionsOnStart", true);
            ShowInputNamesInTerminal = fp2Trainer.CreateEntry("ShowInputNamesInTerminal", false);
            EnableNetworking = fp2Trainer.CreateEntry("EnableNetworking", false);
            SaveGhostFiles = fp2Trainer.CreateEntry("SaveGhostFiles", false);
            LockP1ToGhostFiles = fp2Trainer.CreateEntry("LockP1ToGhostFiles", false);
            MultiCharStartNumChars = fp2Trainer.CreateEntry("MultiCharStartNumChars", 2);
            DEBUG_LoadSpecificGhostFile = fp2Trainer.CreateEntry("DEBUG_LoadSpecificGhostFile", "");
            DEBUG_LoadSpecificGhostFileP2 = fp2Trainer.CreateEntry("DEBUG_LoadSpecificGhostFileP2", "");
            DEBUG_LoadSpecificGhostFileP3 = fp2Trainer.CreateEntry("DEBUG_LoadSpecificGhostFileP3", "");
            DEBUG_LoadSpecificGhostFileP4 = fp2Trainer.CreateEntry("DEBUG_LoadSpecificGhostFileP4", "");
            
            SixtyFPSHack = fp2Trainer.CreateEntry("SixtyFPSHack", false);
            DeterministicMode = fp2Trainer.CreateEntry("DeterministicMode", false);
            
            EnableInvinciblePlayers = fp2Trainer.CreateEntry("EnableInvinciblePlayers", false);
            EnableInvincibleBoss = fp2Trainer.CreateEntry("EnableInvincibleBoss", false);
            
            UseInstaSwitch = fp2Trainer.CreateEntry("UseInstaSwitch", false);
            EnableSplitScreen = fp2Trainer.CreateEntry("EnableSplitScreen", false);
            EnableGetPlayerInstanceMultiplayerPatch = fp2Trainer.CreateEntry("EnableGetPlayerInstanceMultiplayerPatch", false);
            DisplayNametags = fp2Trainer.CreateEntry("DisplayNametags", false);
            // The only real reason that one defaults to false is because this is still meant to be a trainer.
            // If it gets moved into a standalone mod, it'll be made true by default.

            InitPrefsCustomHotkeys();
        }

        private static void InitPrefsCustomHotkeys()
        {
            PHKToggleInstructions = CreateEntryAndBindHotkey("PHKToggleInstructions", "F1");

            PHKSetWarpPoint = CreateEntryAndBindHotkey("PHKSetWarpPoint", "Shift+F4");
            PHKGotoWarpPoint = CreateEntryAndBindHotkey("PHKGotoWarpPoint", "F4");

            PHKKOCharacter = CreateEntryAndBindHotkey("PHKKOCharacter", "Shift+F1");
            PHKKOBoss = CreateEntryAndBindHotkey("PHKKOBoss", "Backspace");
            PHKInvincibleBoss = CreateEntryAndBindHotkey("PHKInvincibleBoss", "Shift+Backspace");
            PHKInvinciblePlayers = CreateEntryAndBindHotkey("PHKInvinciblePlayers", "Ctrl+Backspace");

            PHKToggleNoClip = CreateEntryAndBindHotkey("PHKToggleNoClip", "F2");

            PHKSpawnExtraChar = CreateEntryAndBindHotkey("PHKSpawnExtraChar", "F12");
            PHKSwapBetweenSpawnedChars = CreateEntryAndBindHotkey("PHKSwapBetweenSpawnedChars", "F11");
            PHKToggleMultiCharStart = CreateEntryAndBindHotkey("PHKToggleMultiCharStart", "Shift+F12");
            PHKCyclePreferredAllyControlType =
                CreateEntryAndBindHotkey("F", "Shift+F11");
            PHKStartInputPlayback =
                CreateEntryAndBindHotkey("PHKStartInputPlayback", "Insert");
            PHKToggleLockP1ToGhostFiles =
                CreateEntryAndBindHotkey("PHKToggleLockP1ToGhostFiles", "Shift+Insert");
            
            PHKStartSplitscreen = CreateEntryAndBindHotkey("PHKStartSplitscreen", "Slash");
            
            PHKSwitchCurrentPlayerToLilac = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToLilac", "Alpha0");
            PHKSwitchCurrentPlayerToCarol = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToCarol", "Alpha1");
            PHKSwitchCurrentPlayerToCarolBike = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToCarolBike", "Alpha2");
            PHKSwitchCurrentPlayerToMilla = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToMilla", "Alpha3");
            PHKSwitchCurrentPlayerToNeera = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToNeera", "Alpha4");
            PHKSwitchCurrentPlayerToNext = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToNext", "Alpha9");
            PHKSwitchCurrentPlayerToPrev = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToPrev", "Alpha8");

            PHKGetOutGetOutGetOut = CreateEntryAndBindHotkey("PHKGetOutGetOutGetOut", "Delete");

            PHKCameraZoomIn = CreateEntryAndBindHotkey("PHKCameraZoomIn", "Plus");
            PHKCameraZoomOut = CreateEntryAndBindHotkey("PHKCameraZoomOut", "Minus");
            PHKCameraZoomReset = CreateEntryAndBindHotkey("PHKCameraZoomReset", "Period");

            PHKShowNextDataPage = CreateEntryAndBindHotkey("PHKShowNextDataPage", "PageDown");
            PHKShowPreviousDataPage = CreateEntryAndBindHotkey("PHKShowPreviousDataPage", "PageUp");
            PHKHideDataView = CreateEntryAndBindHotkey("PHKHideDataView", "Backslash");

            PHKGoToMainMenu = CreateEntryAndBindHotkey("PHKGoToMainMenu", "F7");
            PHKLoadDebugRoom = CreateEntryAndBindHotkey("PHKLoadDebugRoom", "F8");

            PHKGoToLevelSelectMenu = CreateEntryAndBindHotkey("PHKGoToLevelSelectMenu", "F9");

            PHKLoadAssetBundles = CreateEntryAndBindHotkey("PHKLoadAssetBundles", "Shift+F9");
            //PHKTogglePauseMenuOrTrainerMenu = CreateEntryAndBindHotkey("PHKTogglePauseMenuOrTrainerMenu", "F1");
            PHKGoToLevelAtLastIndex = CreateEntryAndBindHotkey("PHKGoToLevelAtLastIndex", "BackQuote");
            PHKIncreaseFontSize = CreateEntryAndBindHotkey("PHKIncreaseFontSize", "Shift+Plus");
            PHKDecreaseFontSize = CreateEntryAndBindHotkey("PHKDecreaseFontSize", "Shift+Minus");

            PHKReturnToCheckpoint = CreateEntryAndBindHotkey("PHKReturnToCheckpoint", "R");
            PHKRestartStage = CreateEntryAndBindHotkey("PHKRestartStage", "Shift+R");

            PHKTogglePlaneSwitcherVisualizers = CreateEntryAndBindHotkey("PHKTogglePlaneSwitcherVisualizers", "F3");
            PHKToggleShowColliders = CreateEntryAndBindHotkey("PHKToggleShowColliders", "Shift+F3");

            //PHKNextWarppointSaveSlot = CreateEntryAndBindHotkey("PHKNextWarppointSaveSlot", "F10");
            //PHKPrevWarppointSaveSlot = CreateEntryAndBindHotkey("PHKPrevWarppointSaveSlot", "F9");

            PHKRebindAllHotkeys = CreateEntryAndBindHotkey("PHKRebindAllHotkeys", "Pause");

            hotkeysLoaded = true;
        }

        private static MelonLoader.MelonPreferences_Entry<string> CreateEntryAndBindHotkey(string identifier,
            string default_value)
        {
            var melonPrefEntry = fp2Trainer.CreateEntry(identifier, default_value);
            FP2TrainerCustomHotkeys.Add(melonPrefEntry);
            return melonPrefEntry;
        }

        public static void InitFPLayerNames()
        {
            FPLayerNames = new Dictionary<int, string>();
            int layerAsInt = 0;
            FPLayerNames.Add(layerAsInt, "Default");
            layerAsInt = 1;
            FPLayerNames.Add(layerAsInt, "TransparentFX");
            layerAsInt = 2;
            FPLayerNames.Add(layerAsInt, "Ignore Raycast");
            layerAsInt = 8; // Layer 3 (value =  4) is reserved by Unity but unnamed. Skip to Layer 4.
            FPLayerNames.Add(layerAsInt, "Water");
            layerAsInt = 16;
            FPLayerNames.Add(layerAsInt, "UI");
            
            // Skip two more layers to layer 8. From here, we can rely on powers of 2.
            layerAsInt = 256;
            FPLayerNames.Add(layerAsInt, "FG Plane A");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "FG Plane B");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "FG Plane C");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "FG Plane D");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 0");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 1");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 2");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 3");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 4");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 5");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 6");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 7");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 8");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 9");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 10");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 11");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 12");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 13");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 14");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 15");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "LightingSetup");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "Lighting");
        }

        public override void
            OnSceneWasLoaded(int buildindex,
                string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            MelonLogger.Msg("OnSceneWasLoaded: " + buildindex + " | " + sceneName);
            ResetSceneSpecificVariables();
            AttemptToFindFPFont();
            AttemptToFindPauseMenu();
            GrabAndTweakPauseMenu();
            MelonPreferences.Save();


            if (goDtTracker != null)
            {
            }
            else
            {
                goDtTracker = new GameObject("FP2TrainerDeltaTimeTracker");
                goDtTracker.AddComponent<FP2TrainerDTTracker>();
                Log("Created DeltaTime tracker. Updates will occur on LateUpdate.");
            }
        }

        private void AttemptToFindPauseMenu()
        {
            if (stageSelectMenu == null)
                foreach (var pauseMenu in Resources.FindObjectsOfTypeAll(typeof(FPPauseMenu)) as FPPauseMenu[])
                {
                    this.pauseMenu = pauseMenu;
                    //stageSelectMenu = GameObject.Instantiate(pauseMenu.transform.gameObject);
                    stageSelectMenu = new GameObject("Stage Select Menu");
                    stageSelectMenu.transform.position = new Vector3(-376, -192, 0);
                    var resumeIcon =
                        Object.Instantiate(this.pauseMenu.transform.Find("Pause Icon - Resume").gameObject);
                    if (resumeIcon != null)
                    {
                        resumeIcon.name = "AnnStagePlayIcon";
                        resumeIcon.transform.parent = stageSelectMenu.transform;
                        resumeIcon.transform.localPosition = new Vector3(-112, -64, -4);
                    }

                    Log("Found a pauseMenu to modify.");
                    Log("...But created a new GameObject instead anyway. The frame was annoying.");
                    stageSelectMenu.name = "Ann Stage Select Menu";
                    break;
                }
        }

        private void AttemptToFindFPFont()
        {
            if (fpMenuFont != null) return;

            foreach (var textMesh in Resources.FindObjectsOfTypeAll(typeof(TextMesh)) as TextMesh[])
                if (textMesh.font != null && textMesh.font.name.Equals("FP Menu Font"))
                    //if (textMesh.font!= null && textMesh.font.name.Equals("FP Small Font Light"))
                {
                    Log("Found the FP Menu Font loaded in memory. Saving reference.");
                    //Log("Found the FP Small Font loaded in memory. Saving reference.");
                    fpMenuFont = textMesh.font;
                    fpMenuMaterial = textMesh.GetComponent<MeshRenderer>().materials[0];
                    break;
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

            doneMultiplayerStart = false;

            instaSwitchCharsSpawned = false;

            if (goFP2Trainer == null)
            {
                CreateFP2TrainerGameObject();
            }

            if (planeSwitcherVisualizer)
            {
                planeSwitcherVisualizer.Reset();
                planeSwitcherVisualizer.SpawnVisualizers();
            }

            planeSwitchVisualizersCreated = false;
            planeSwitchVisualizersVisible = ShowPlaneSwitcherVisualizersLastSetting.Value;

            if (FP2TrainerAllyControls.inputQueueForPlayers != null)
            {
                FP2TrainerAllyControls.inputQueueForPlayers.Clear();
            }
            else
            {
                FP2TrainerAllyControls.inputQueueForPlayers = new Dictionary<int, FP2TrainerInputQueue>();
            }

            skipRecording = LockP1ToGhostFiles.Value;
            FP2TrainerAllyControls.needToLoadInputs = LockP1ToGhostFiles.Value;
            
            SplitScreenCameraInfos.Clear();
        }

        private static void CreateFP2TrainerGameObject()
        {
            goFP2Trainer = new GameObject("FP2Trainer");
            GameObject.DontDestroyOnLoad(goFP2Trainer);
            goFP2Trainer.AddComponent<FP2TrainerCustomHotkeys>();
            planeSwitcherVisualizer = goFP2Trainer.AddComponent<PlaneSwitcherVisualizer>();
            goFP2Trainer.AddComponent<FP2TrainerCharacterNameTag>();
            if (SixtyFPSHack.Value)
            {
                goFP2Trainer.AddComponent<ForceRenderRate>();
            }
        }

        public static Font GetFPMenuFont()
        {
            return fpMenuFont;
        }

        public void CreateFancyTextObjects()
        {
            goStageHUD = GameObject.Find("Stage HUD");
            //GameObject goStageHUD = GameObject.Find("Hud Pause Menu");
            if (goStageHUD == null) return;

            Log("Successfully found HUD to attach text to.");

            //Why is this here??
            var tempHudMaster = goStageHUD.GetComponent<FPHudMaster>();
            this.lifePetal = tempHudMaster.pfHudLifePetal;
            this.shield = tempHudMaster.pfHudShield;

            goFancyTextPosition = GameObject.Find("Resume Text");
            if (goFancyTextPosition != null)
            {
                Log("Found Resume Text");
                goFancyTextPosition = Object.Instantiate(goFancyTextPosition);
                goFancyTextPosition.SetActive(true);
                textmeshFancyTextPosition = goFancyTextPosition.GetComponent<TextMesh>();
                textmeshFancyTextPosition.font = fpMenuFont;
                textmeshFancyTextPosition.GetComponent<MeshRenderer>().materials[0] = fpMenuMaterial;
                textmeshFancyTextPosition.characterSize = 10;
                textmeshFancyTextPosition.anchor = TextAnchor.UpperLeft;
                Log("Successfully cloned Resume Text. Attaching to Stage HUD.");
            }
            else if (goStageHUD != null)
            {
                //goStageHUD.energyBarGraphic.transform.parent;
                Log("Looking for Energy Bar");
                var temp = goStageHUD.GetComponent<FPHudMaster>();
                GameObject temp2;
                if (temp != null)
                {
                    temp2 = temp.pfHudEnergyBar;
                }
                else
                {
                    Log("This aint it.");
                    return;
                }


                var energyBarGraphic = Object.Instantiate(temp2, temp2.transform.parent);

                energyBarGraphic.transform.localScale *= 2;

                goFancyTextPosition = energyBarGraphic;
                goFancyTextPosition.SetActive(true);
                //GameObject.Destroy(goFancyTextPosition.GetComponent<SpriteRenderer>()); // Can't have Sprite Renderer and Mesh Renderer.
                var tempGo = new GameObject();
                tempGo.transform.parent = goFancyTextPosition.transform;
                tempGo.transform.localPosition = Vector3.zero;

                goFancyTextPosition.transform.position = new Vector3(16, -80, 0);
                goFancyTextPosition = tempGo;

                textmeshFancyTextPosition = goFancyTextPosition.AddComponent<TextMesh>();
                if (textmeshFancyTextPosition != null)
                {
                    Log("Current value of fpMenuFont: " + fpMenuFont);
                    textmeshFancyTextPosition.font = fpMenuFont;
                    textmeshFancyTextPosition.GetComponent<MeshRenderer>().materials[0] = fpMenuMaterial;
                    textmeshFancyTextPosition.characterSize = 10;
                    textmeshFancyTextPosition.anchor = TextAnchor.UpperLeft;
                    textmeshFancyTextPosition.text =
                        "I exist!@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n@@@@@@@@@@@@@@@@@@@@@@@@";
                    Log("Attempting to clone energyBar. Attaching to Stage HUD.");
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
                textmeshFancyTextPosition.GetComponent<MeshRenderer>().materials[0] = fpMenuMaterial;
                textmeshFancyTextPosition.characterSize = 10;
                textmeshFancyTextPosition.anchor = TextAnchor.UpperLeft;
                Log(
                    "Could not clone Resume Text or Energy Bar. Manually creating TextMesh and Attaching to Stage HUD.");


                //Log("Could not clone Resume Text. Canceling.");
                //return;
            }

            goFancyTextPosition.transform.parent = goStageHUD.transform;
            goFancyTextPosition.transform.localPosition = new Vector3(10, 20, 0);
            UpdateFancyText();
        }

        public static GameObject CloneHealthBar(FPPlayer targetPlayer)
        {
            GameObject newHud = null;
            var huds = GameObject.FindObjectsOfType<FPHudMaster>();
            if (huds.Length > 0)
            {
                newHud = GameObject.Instantiate(huds[0].gameObject,
                    (huds[0].transform.position + new Vector3(0f, -128f, 0f)), huds[0].transform.rotation);
                newHud.name = "Stage HUD " + huds.Length.ToString();

                var hudScript = newHud.GetComponent<FPHudMaster>();
                //hudScript.onlyShowHealth = true;
                hudScript.targetPlayer = targetPlayer;
            }


            return newHud;
        }

        public void UpdateFancyText()
        {
            if (textmeshFancyTextPosition != null && showVarString)
                textmeshFancyTextPosition.text = debugDisplay;
            else if (!showVarString) textmeshFancyTextPosition.text = "";

            if (fpplayer != null && goFancyTextPosition != null)
                //goFancyTextPosition.transform.position = new Vector3(fpplayer.position.x - 10, fpplayer.position.y - 10, -1);
                goFancyTextPosition.transform.position = new Vector3(16, -80, 0);
        }

        public override void
            OnSceneWasInitialized(int buildindex,
                string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
        {
            if (DeterministicMode.Value)
            {
                UnityEngine.Random.InitState(fp2ReleaseDateInt);
                Log($"Seeding RNG with {fp2ReleaseDate.ToString()} ({fp2ReleaseDateInt})");
            }

            MelonLogger.Msg("OnSceneWasInitialized: " + buildindex + " | " + sceneName);
            SkipBootIntros();
            GrabAndTweakPauseMenu();
            GrabAndUpdateCameraDetails();
            VisualizePlaneSwitchers();

            if (goFP2TrainerYourPlayerIndicator == null)
            {
                goFP2TrainerYourPlayerIndicator = FP2TrainerYourPlayerIndicator.CreateFPYourPlayerIndicator(
                    "YourPlayer", Vector3.zero, Quaternion.identity, goFP2Trainer.transform);
            }
        }

        private void VisualizePlaneSwitchers()
        {
            if (planeSwitcherVisualizer != null)
            {
                Fp2Trainer.Log("PSV Not Null");
                planeSwitcherVisualizer.SpawnVisualizers();
                planeSwitcherVisualizer.SetActiveOfSpawnedVisualizers(planeSwitchVisualizersVisible);
            }
            else
            {
                if (!planeSwitchVisualizersCreated)
                {
                    Fp2Trainer.Log("PSV Is Null");
                    planeSwitchVisualizers = new List<GameObject>();

                    List<PlaneSwitcher> planeSwitchers;
                    planeSwitchers = new List<PlaneSwitcher>((GameObject.FindObjectsOfType<PlaneSwitcher>()));
                    Debug.Log(System.String.Format("Found {0} PlaneSwitchers. Attempting to visualize.\n",
                        planeSwitchers.Count));
                    GameObject goCube;
                    Renderer renCube;
                    foreach (PlaneSwitcher ps in planeSwitchers)
                    {
                        Fp2Trainer.Log(System.String.Format("Adding Cube to {0} PlaneSwitchers.\n", ps.name));
                        goCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        goCube.name = ("Visualizer " + ps.name);
                        goCube.transform.position = new Vector3(ps.transform.position.x, ps.transform.position.y,
                            ps.transform.position.z);
                        goCube.transform.localScale = new Vector3(ps.xsize, ps.ysize, 1f);
                        renCube = goCube.GetComponent<Renderer>();

                        //renCube.material.color = new Color(1, 0, 0, 0.7f);
                        renCube.material.color = new Color(1, 0, 0, 1f);
                        goCube.SetActive(planeSwitchVisualizersVisible);
                        planeSwitchVisualizers.Add(goCube);
                        //renCube.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        //renCube.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    }

                    planeSwitchVisualizersCreated = true;
                }
            }
        }

        public void ShowPlaneSwitchVisualizers()
        {
            planeSwitchVisualizersVisible = true;

            if (planeSwitcherVisualizer != null)
            {
                planeSwitcherVisualizer.SetActiveOfSpawnedVisualizers(planeSwitchVisualizersVisible);
                return;
            }

            if (planeSwitchVisualizers == null)
            {
                return;
            }

            foreach (var psv in planeSwitchVisualizers)
            {
                psv.SetActive(planeSwitchVisualizersVisible);
            }
        }

        public void HidePlaneSwitchVisualizers()
        {
            planeSwitchVisualizersVisible = false;

            if (planeSwitcherVisualizer != null)
            {
                planeSwitcherVisualizer.SetActiveOfSpawnedVisualizers(planeSwitchVisualizersVisible);
                return;
            }

            if (planeSwitchVisualizers == null)
            {
                return;
            }

            foreach (var psv in planeSwitchVisualizers)
            {
                psv.SetActive(planeSwitchVisualizersVisible);
            }
        }

        public void TogglePlaneSwitchVisualizers()
        {
            if (planeSwitcherVisualizer)
            {
                /*planeSwitcherVisualizer.AnnihilateGameObjects("AM_BG0 (0)", "AM_BG0 (1)", "AM_BG0 (2)", "AM_BG0 (3)", "AM_BG0 (4)", "AM_BG0 (5)",
                    "AM_BG1");*/
            }

            planeSwitchVisualizersVisible = !planeSwitchVisualizersVisible;
            if (planeSwitchVisualizersVisible)
            {
                ShowPlaneSwitchVisualizers();
            }
            else
            {
                HidePlaneSwitchVisualizers();
            }
        }

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
            MelonLogger.Msg("OnApplicationQuit");
            MelonPreferences.Save();
        }

        public void OnGameObjectUpdate()
        {
            try
            {
                if (DeterministicMode.Value)
                {
                    //UnityEngine.Random.InitState(fp2ReleaseDateInt);
                    //Log($"Seeding RNG with {fp2ReleaseDate.ToString()} ({fp2ReleaseDateInt})");
                }
                
                if (introSkipped < 1)
                {
                    SkipBootIntros();
                }

                if (waitingForNextFrameForSpoilerGimmick)
                {
                    GetReferencesToSpoilerGimmickPart2();
                }

                if (dpsTracker != null) dpsTracker.Update();

                if (timeoutShowWarpInfo > 0) timeoutShowWarpInfo -= FPStage.frameTime;
                if (timeoutShowWarpInfo < 0) timeoutShowWarpInfo = 0;
                try
                {
                    if (player == null)
                    {
                        player = GetFirstPlayerGameObject();
                        fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                        fpStage = FPStage.currentStage;

                        if (player != null) MelonLogger.Msg("Trainer found a Player Object: ");
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
                            for (var i = 0; i < 10; i++)
                            {
                                positionDigits.Add(stageHUD.AddComponent<FPHudDigit>());
                                if (i < 5)
                                    positionDigits[i].transform.position = new Vector3(i * 16, 64,
                                        positionDigits[i].transform.position.z);
                                else
                                    positionDigits[i].transform.position = new Vector3(i * 16 + 16, 64,
                                        positionDigits[i].transform.position.z);
                            }
                        }
                    }

                    debugDisplay = "";

                    if (fpplayer != null)
                    {
                        if (hotkeysLoaded)
                        {
                            HandleHotkeys();
                        }
                        
                        try
                        {
                            //This should probably be in its own script:

                            if (Fp2Trainer.UseInstaSwitch.Value && !instaSwitchCharsSpawned)
                            {
                                Fp2Trainer.Log("Finna spawn extra chars for instaswap");
                                //FPPlayer2p.SpawnExtraCharactersViaSpawnPoint();
                                FPPlayer2p.SpawnExtraCharacters();
                                instaSwitchCharsSpawned = true;
                            }
                        }
                        catch (Exception e)
                        {
                            Fp2Trainer.Log(e.Message + e.StackTrace);
                        }

                        if (multiplayerStart && !doneMultiplayerStart)
                        {
                            currentDataPage = DataPage.MULTIPLAYER_DEBUG;
                            while (fpplayers.Count < MultiCharStartNumChars.Value)
                            {
                                FPPlayer2p.SpawnExtraCharacter();
                            }
                            if (Fp2Trainer.EnableSplitScreen.Value)
                            {
                                StartSplitscreen(); // Probably need to include a way to stop this from happening automatically.
                            }

                            doneMultiplayerStart = true;
                        }

                        if (showInstructions)
                        {
                            HandleInstructionPageDisplay();
                        }

                        else
                        {
                            UpdateDPS();
                            if (timeoutShowWarpInfo > 0) debugDisplay += warpMessage + "\n";

                            if (fptls != null)
                            {
                                var snp = fptls.availableScenes[fptls.menuSelection];
                                debugDisplay += "Warp to: " + fptls.menuSelection + " | " + snp.name + "\n";
                                debugDisplay += "Level Select Parent Pos: " + stageSelectMenu.transform.position + "\n";
                                var tempGoButton = stageSelectMenu.transform.Find("AnnStagePlayIcon").gameObject;
                                debugDisplay += "Level Select Button Pos: " + tempGoButton.transform.position + "\n";
                                debugDisplay += "Level Select Button LocalPos: " +
                                                tempGoButton.transform.localPosition +
                                                "\n";
                            }


                            HandleDataPageDisplay();
                            //FPPlayer2p.ShowPressedButtons();
                        }

                        FPPlayer2p.CatchupIfPlayerTooFarAway();
                    }

                    debugDisplay = FP2TrainerAllyControls.funky + "\n" + Fp2Trainer.debugDisplay;

                    if (goFancyTextPosition != null)
                        UpdateFancyText();
                    else
                        CreateFancyTextObjects();

                    HandleInvincibility();
                    
                    if (ShowInputNamesInTerminal.Value)
                    {
                        FPPlayer2p.ShowPressedButtons();
                    }
                }

                catch (Exception e)
                {
                    Log("Trainer Error During Update: " + e.Message + "(" + e.InnerException?.Message + ") @@" +
                        e.StackTrace);
                }

                if (fpStage != null)
                {
                    FPStage.showColliders = ShowCollidersLastSetting.Value;
                    EnforceTenMinuteTimerPenalty();
                }

                if (!skipRecording)
                {
                    RecordPlayer1Input();
                }

                if (LockP1ToGhostFiles.Value && fpplayer != null)
                {
                    //This is probably resource intensive. Maybe there's a better way to prevent this value from being overwritten.
                    fpplayer.inputMethod = fpplayer.HandleAllyControlsGhost; 
                }

                if (EnableSplitScreen.Value)
                {
                    UpdateSplitScreens();
                }

                // Players without a proper validated stageListPos can't interact with many gameplay objects.
                // So we want to try to make sure they get validated.
                // Calling this every frame may be a signficant performance penalty though...
                ValidateAllFPPlayers();
            }
            catch (Exception e)
            {
                Log("EXCEPTION\n");
                Log(e.ToString());
                Log(e.Message);
                Log(e.StackTrace);
            }
        }

        private void RecordPlayer1Input()
        {
            if (fpplayer != null && FP2TrainerAllyControls.inputQueueForPlayers != null)
            {
                FP2TrainerAllyControls.GetInputQueue(fpplayer).AddTime(FPStage.deltaTime);
                var ipq = FP2TrainerAllyControls.RecordInput(fpplayer);
                if (!FP2TrainerAllyControls.inputQueueForPlayers.ContainsKey(fpplayer.GetInstanceID()))
                {
                    FP2TrainerAllyControls.inputQueueForPlayers.Add(fpplayer.GetInstanceID(), ipq);
                }
            }
        }

        private void EnforceTenMinuteTimerPenalty()
        {
            if (fpStage.minutes < 10)
            {
                fpStage.minutes += 10;
            }
        }

        private void HandleDataPageDisplay()
        {
            var collisionLayerName = fpplayer.collisionLayer.ToString();
            if (fpplayer.collisionLayer >= 0)
            {
                //collisionLayerName = $"{collisionLayerName}: {LayerMask.LayerToName(fpplayer.collisionLayer)}";
                collisionLayerName = $"{collisionLayerName}: {FPLayerNames[fpplayer.collisionLayer]}";
            }

            if (currentDataPage == DataPage.NO_CLIP)
            {
                debugDisplay += "NoClip Enabled: " + noClip.ToString() + "\n";
                debugDisplay += "Position: " + fpplayer.position.ToString() + "\n";
                debugDisplay += "Terrain Collision: " + fpplayer.terrainCollision.ToString() + "\n";
                debugDisplay += "Physics Enabled: " + fpplayer.enablePhysics.ToString() + "\n";
                debugDisplay += "Collision Layer: " + collisionLayerName + "\n";
                debugDisplay += "PlaneSwitcherVisualizers: " + planeSwitchVisualizersVisible.ToString() + "\n";
                debugDisplay += "Show Debug Colliders: " + ShowCollidersLastSetting.Value.ToString() + "\n";
            }

            string pad = "                    ";
            if (currentDataPage == DataPage.CAMERA)
            {
                foreach (var cam in GameObject.FindObjectsOfType<Camera>())
                {
                    if (cam.gameObject.name.Contains("Render"))
                    {
                        var strCamsAreJank = $"{pad}{cam}\n{pad} => {cam.transform.position}:TargText{cam.targetTexture}\n"; 
                        debugDisplay += strCamsAreJank;
                        Log(strCamsAreJank);
                    }
                }

                debugDisplay += "Position: " + fpplayer.position.ToString() + "\n";
            }
            if (currentDataPage == DataPage.CAMERA_ALL)
            {
                var strCamsAreJank = "";
                foreach (var cam in GameObject.FindObjectsOfType<Camera>())
                {
                    strCamsAreJank = $"{pad}{cam}\n{pad} => {cam.transform.position}:d:{cam.depth}\n"; 
                    debugDisplay += strCamsAreJank;
                    Log(strCamsAreJank);
                }

                debugDisplay += "Position: " + fpplayer.position.ToString() + "\n";
            }

            if (noClip)
            {
                debugDisplay += "NoClip: " + fpplayer.position.ToString() + "\n";
            }

            if (currentDataPage == DataPage.MOVEMENT)
            {
                debugDisplay += "Movement (1/2): \n";
                if (playerValuesToShow.Contains("Pos")) debugDisplay += "Pos: " + fpplayer.position + "\n";
                if (playerValuesToShow.Contains("Vel")) debugDisplay += "Vel: " + fpplayer.velocity + "\n";
                if (playerValuesToShow.Contains("Magnitude"))
                {
                    debugDisplay += "Acceleration: " + fpplayer.acceleration + "\n";
                    debugDisplay += "Magnitude: " + fpplayer.velocity.magnitude + "\n";
                    debugDisplay += "Accel: " + fpplayer.acceleration + "\n";
                    debugDisplay += "Air Accel: " + fpplayer.airAceleration + "\n";
                    debugDisplay += "Air Drag: " + fpplayer.airDrag + "\n";
                }

                debugDisplay += "Collision Layer: " + collisionLayerName + "\n";
                debugDisplay += "PlaneSwitcherVisualizers: " + planeSwitchVisualizersVisible.ToString() + "\n";
                debugDisplay += "Show Debug Colliders: " + ShowCollidersLastSetting.Value.ToString() + "\n";
            }
            else if (currentDataPage == DataPage.MOVEMENT_2)
            {
                debugDisplay += "Movement (2/2): \n";

                debugDisplay += "Collision Layer: " + collisionLayerName + "\n";
                debugDisplay += "PlaneSwitcherVisualizers: " + planeSwitchVisualizersVisible.ToString() + "\n";
                debugDisplay += "Show Debug Colliders: " + ShowCollidersLastSetting.Value.ToString() + "\n";

                if (playerValuesToShow.Contains("Ground Angle"))
                    debugDisplay += "Ground Angle: " + fpplayer.groundAngle + "\n";
                if (playerValuesToShow.Contains("Ground Velocity"))
                    debugDisplay += "Ground Velocity: " + fpplayer.groundVel + "\n";
                if (playerValuesToShow.Contains("Ceiling Angle"))
                    debugDisplay += "Ceiling Angle: " + fpplayer.ceilingAngle + "\n";
                if (playerValuesToShow.Contains("Sensor Angle"))
                    debugDisplay += "Sensor Angle: " + fpplayer.sensorAngle + "\n";
                if (playerValuesToShow.Contains("Gravity Angle"))
                    debugDisplay += "Gravity Angle: " + fpplayer.gravityAngle + "\n";
                if (playerValuesToShow.Contains("Gravity Strength"))
                    debugDisplay += "Gravity Strength: " + fpplayer.gravityStrength + "\n";
            }
            else if (currentDataPage == DataPage.COMBAT)
            {
                debugDisplay += "Combat: \n";
                debugDisplay += "Health: " + fpplayer.health + "\n";

                var tempDmgType = fpplayer.damageType;
                if (tempDmgType > 4) tempDmgType = -1;
                debugDisplay += "Hurt Damage Element: " + fpElementTypeNames[tempDmgType] + "\n";

                if (nearestEnemy != null)
                    debugDisplay += nearestEnemy.name + " Health: " + nearestEnemy.health + "\n";

                if (dpsTracker != null) debugDisplay += "DPS: " + dpsTracker + "\n";

                debugDisplay += "Energy: " + fpplayer.energy + "\n";
                debugDisplay += "Energy Recover Current: " + fpplayer.energyRecoverRateCurrent + "\n";
                debugDisplay += "Energy Recover: " + fpplayer.energyRecoverRate + "\n";
                debugDisplay += "Faction: " + fpplayer.faction + "\n";
                debugDisplay += "Attack Power: " + fpplayer.attackPower + "\n";
                debugDisplay += "Attack Hitstun: " + fpplayer.attackHitstun + "\n";
                debugDisplay += "Attack Knockback: " + fpplayer.attackKnockback + "\n";
                if (playerValuesToShow.Contains("InflictedDamage"))
                    debugDisplay += "InflictedDamage: " + fpplayer.damageInflicted + "\n";
                debugDisplay += "Guard Time: " + fpplayer.guardTime + "\n";
                debugDisplay += "ATK NME INV TIM: " + fpplayer.attackEnemyInvTime + "\n";
                debugDisplay += "Hit Stun: " + fpplayer.hitStun + "\n";
                debugDisplay += "Invul Time: " + fpplayer.invincibilityTime + "\n";
            }
            else if (currentDataPage == DataPage.DPS)
            {
                debugDisplay += "DPS: \n";
                if (dpsTracker != null)
                {
                    if (nearestEnemy != null && nearestEnemyPrevious != null)
                    {
                        debugDisplay += "Previous Nearest Enemy: " + nearestEnemyPrevious.name + "\n";
                        debugDisplay += "Prev Health: " + nearestEnemyPreviousHP + "\n";
                    }
                    else if (nearestEnemy == null)
                    {
                        debugDisplay += "Nearest Enemy Not Found\n";
                    }
                    else if (nearestEnemy == null)
                    {
                        debugDisplay += "Previous Nearest Enemy Not Found\n";
                    }

                    debugDisplay += dpsTracker.GetDPSBreakdownString();
                }
                else
                {
                    debugDisplay += "No DPS Tracker found?";
                }
            }
            else if (currentDataPage == DataPage.DPS_ALL)
            {
                if (dpsTracker != null)
                {
                    debugDisplay += "DPS ALL: \n";
                    debugDisplay += dpsTracker.GetDPSBreakdownString();
                }
                else
                {
                    debugDisplay += "No DPS Tracker found?";
                }
            }
            else if (currentDataPage == DataPage.MULTIPLAYER_DEBUG)
            {
                debugDisplay += "Multiplayer Debug: \n";
                var tempDmgType = -1;
                string isLeader = "";
                foreach (var mp_fpplayer in fpplayers)
                {
                    if (mp_fpplayer == FPStage.currentStage.GetPlayerInstance_FPPlayer())
                    {
                        isLeader = " - Leader";
                    }
                    else
                    {
                        isLeader = "";
                    }

                    debugDisplay += mp_fpplayer.name + isLeader + "\n";
                    debugDisplay += String.Format("{0:000.00}/{1:000.00} HP {2:000.00}/{3:000.00} EN\n",
                        mp_fpplayer.health, mp_fpplayer.healthMax,
                        mp_fpplayer.energy, 100f);

                    debugDisplay += mp_fpplayer.position.ToString() + "\n";
                    //debugDisplay += mp_fpplayer.name + " Energy: " + mp_fpplayer.energy + "\n";
                }
            }
            else if (currentDataPage == DataPage.BOSS)
            {
                debugDisplay += "Boss: \n";
                fpEnemies.Clear();
                foreach (var bh in bossHuds)
                {
                    if (!bh.transform.gameObject.activeInHierarchy)
                    {
                        ReacquireBossHuds();
                        break;
                    }

                    if (bh.targetBoss != null) fpEnemies.Add(bh.targetBoss);
                }

                if (fpEnemies.Count > 0)
                    foreach (var ene in fpEnemies)
                    {
                        if (ene == null) continue;

                        debugDisplay += ene.name + " Health: " + ene.health + "\n";
                        debugDisplay += ene.name + " Freeze Timer: " + ene.freezeTimer + "\n";
                        //debugDisplay += mp_fpplayer.name + " Energy Recover: " + mp_fpplayer.energyRecoverRate.ToString() + "\n";
                        //debugDisplay += mp_fpplayer.name + " Energy Recover Current: " + mp_fpplayer.energyRecoverRateCurrent.ToString() + "\n";
                        debugDisplay += ene.name + " Is Harmless: " + ene.isHarmless + "\n";
                        debugDisplay += ene.name + " Cannot Be Killed: " + ene.cannotBeKilled + "\n";
                        debugDisplay += ene.name + " Cannot Be Frozen: " + ene.cannotBeFrozen + "\n";
                        debugDisplay += ene.name + " Last Received Damage: " +
                                        ene.lastReceivedDamage + "\n";
                        debugDisplay += ene.name + " LRD (Unmodified): " +
                                        ene.lastReceivedDamageUnmodified + "\n";
                    }
                else
                    debugDisplay +=
                        "Unable to find relevant enemies.\nTry switching to this view while the healthbar is visible.\n";
            }
        }

        public void HandleInstructionPageDisplay()
        {
            if (showInstructions && currentDataPage == DataPage.NONE)
            {
                currentDataPage = DataPage.MOVEMENT;
            }

            int numHotkeyLinesPerPage = 7;
            debugDisplay += String.Format("--[Instructions: ({0} / {1})]--\n",
                (int)(currentInstructionPage + 1), ((int)(InstructionPage.NONE) + 1));

            switch (currentInstructionPage)
            {
                /*
                 *"\"Milla's Toybox\" (or \"FP2 Trainer\" if you prefer),\n"
                 * This particular example line is roughly 48 characters long,
                 * any longer than that, and the text won't fit on-screen.
                 */
                case InstructionPage.BASICS:
                    debugDisplay += "**Basics**\n" +
                                    "\"Milla's Toybox\" (or \"FP2 Trainer\" if you prefer),\n" +
                                    "is a speedrun-focused trainer toolkit \nmod for Freedom Planet 2.\n" +
                                    "It is primarily used for gaining \nadditional information about how the game works\n" +
                                    "and experimenting with the physics and \nmechanics of the game to find new\n" +
                                    "techniques in the pursuit of going FAST.\n" +
                                    "But of course, there's other toys too. But first...\n" +
                                    String.Format("(Press {0} to continue)\n", PHKShowNextDataPage.Value);
                    break;
                case InstructionPage.BACKUPS:
                    debugDisplay += "**Backups**\n" +
                                    "It's highly recommended that you back up \nyour entire FP2 folder\n" +
                                    "and only install mods on a separate \ncopy to prevent this training\n" +
                                    "from unexpectedly breaking due to \ngame version updates.\n" +
                                    "You may backup your save files too, if you like.\n" +
                                    "Your save files can typically be found at:\n" +
                                    "C:\\Users\\<YOUR username here>\\AppData\n\\LocalLow\\GalaxyTrail\\Freedom Planet 2\n" +
                                    "(That's **LocalLOW**, not Local. A common mistake.)\n";
                    //"Your save files are currently stored at:\n{0}", Application.persistentDataPath;
                    break;
                case InstructionPage.SPEEDRUN:
                    debugDisplay += "**Speedrun Tools**\n" +
                                    "Data data DATA! Your speed, position, damage,\n" +
                                    "collision layer, dps, boss info, and more!\n" +
                                    String.Format("View DataViewer Next Page: {0}\n" +
                                                  "View DataViewer Previous Page: {1}\n" +
                                                  "Hide DataViewer{7}\n" +
                                                  "Set Warp Point: {2}\n" +
                                                  "Teleport to Warp Point: {3}\n" +
                                                  "Toggle PlaneSwitcherVisualizers: {4}\n\n" +
                                                  "Toggle Show All Debug Colliders: {6}\n\n" +
                                                  "Load ANY Stage Menu: {5}\n" +
                                                  "Confirm Stage Menu Choice: (Jump Button)\n",
                                        PHKShowNextDataPage.Value, PHKShowPreviousDataPage.Value, PHKSetWarpPoint.Value,
                                        PHKGotoWarpPoint.Value, PHKTogglePlaneSwitcherVisualizers.Value,
                                        PHKGoToLevelSelectMenu.Value, PHKToggleShowColliders.Value, 
                                        PHKHideDataView.Value);
                    break;
                case InstructionPage.QUICK_RESTART:
                    debugDisplay += "**Quick Restart**\n" +
                                    "For when going to the pause menu is too slow.\n" +
                                    String.Format("Reset to Checkpoint: {0}\n" +
                                                  "Restart Stage: {1}\n",
                                        PHKReturnToCheckpoint.Value, PHKRestartStage.Value);
                    break;
                case InstructionPage.NO_CLIP:
                    debugDisplay += "**NoClip**\n" +
                                    "This allows you to fly freely through the \nmap ignoring gravity and walls!\n" +
                                    "Most things can't touch you, but \nthe camera can still be locked in place.\n" +
                                    "And some crush-triggers may still KO you.\n" +
                                    String.Format("Toggle NoClip Mode: {0}\n" +
                                                  "Cancel NoClip and Return to Start:\n  (Attack Button)\n" +
                                                  "Exit NoClip at Current Position:\n  (Jump Button) or {0}\n",
                                        PHKToggleNoClip.Value);
                    break;
                case InstructionPage.MULTICHARACTER:
                    debugDisplay += "**Multi-Character**\n" +
                                    "Play as multiple characters at the same time!\n" +
                                    "Be warned, this is very buggy. \nThe game is not designed to support \nmore than one character at a time.\n" +
                                    "If you have multiple characters, KOed characters \n" +
                                    "are removed from play immediately until \n" +
                                    "there is only one left.\n" +
                                    String.Format("Spawn Additional Character: {0}\n" +
                                                  "Switch to Next Remaining Character: {1}\n" +
                                                  "Toggle Ally-spawn on level-start: {2}\n" +
                                                  "Cycle Ally Playstyle: {3}\n" +
                                                  "Insta-KO Current Character: {4}\n" +
                                                  "Insta-KO Current Boss: {5}\n" +
                                                  "Start Splitscreen: {6}\n",
                                        PHKSpawnExtraChar.Value, PHKSwapBetweenSpawnedChars.Value,
                                        PHKToggleMultiCharStart.Value, PHKCyclePreferredAllyControlType.Value,
                                        PHKKOCharacter.Value, PHKKOBoss.Value, PHKStartSplitscreen.Value);
                    break;
                /*
                case InstructionPage.CHAR_INSTASWAP:
                    debugDisplay += "**Character Insta-swap**\n" +
                                    "WIP. Switch to any character on the fly." +
                                    "Defaults to being set via numpad numbers." +
                                    "If you're a gamepad player, you'll want to remap those.\n" +
                                    String.Format("PHKSwitchCurrentPlayerToLilac: {0}\n" +
                                                  "PHKSwitchCurrentPlayerToCarol: {1}\n" +
                                                  "PHKSwitchCurrentPlayerToCarolBike: {2}\n" +
                                                  "PHKSwitchCurrentPlayerToMilla: {3}\n" +
                                                  "PHKSwitchCurrentPlayerToNeera: {4}\n" +
                                                  "PHKSwitchCurrentPlayerToNext: {5}\n" +
                                                  "PHKSwitchCurrentPlayerToPrev: {6}\n",
                                        PHKSwitchCurrentPlayerToLilac.Value, PHKSwitchCurrentPlayerToCarol.Value, 
                                        PHKSwitchCurrentPlayerToCarolBike.Value, PHKSwitchCurrentPlayerToMilla.Value,
                                        PHKSwitchCurrentPlayerToNeera.Value,
                                        PHKSwitchCurrentPlayerToNext.Value,
                                        PHKSwitchCurrentPlayerToPrev.Value);
                    break;
                    */
                /*case InstructionPage.NETPLAY:
                    debugDisplay += "**Basics**\n";
                    // PHKStartInputPlayback
                    break;*/
                case InstructionPage.BUGS:
                    debugDisplay += "**Bugs**\n" +
                                    "tbh I need sleep, I'll probably fill this up \nwith something useful later.\n+" +
                                    "If you have this version of the trainer,\nyou probably already know how to contact me.\n\n\n" +
                                    String.Format("GET OUT GET OUT GET OUT\n{0}\n",
                                        PHKGetOutGetOutGetOut.Value);
                    break;
                case InstructionPage.HOTKEYS_1:
                    debugDisplay += "**How to Rebind Hotkeys (1/2)**\n" +
                                    "If you want to change your \nHotkey Bindings, you can edit them\n" +
                                    "at <FP2 Install Dir>/UserData/MelonPreferences.cfg\n" +
                                    "If you don't see a config file there, \nlaunch the game for a few seconds,\n" +
                                    "then close it and check again for \na regenerated config file.\n";
                    break;
                case InstructionPage.HOTKEYS_2:
                    debugDisplay += "**How to Rebind Hotkeys (2/2)**\n" +
                                    "Hotkey Keybinds are Case-Sensitive and can \nonly be changed while the game is NOT RUNNING.\n" +
                                    "Edits made to the config file \nwhile the game is running\n" +
                                    "will be OVERWRITTEN!\n" +
                                    "If you need help setting it up, please ask.\n" +
                                    "If you make a mistake, don't worry!\n" +
                                    "Delete the file and relaunch the game \nto regenerate a new default config file.\n";
                    break;
                case InstructionPage.HOTKEYS_3:
                    debugDisplay += "**Current Hotkeys**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1, 1 + numHotkeyLinesPerPage);
                    break;
                case InstructionPage.HOTKEYS_4:
                    debugDisplay += "**More Current Hotkeys**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 1),
                        1 + (numHotkeyLinesPerPage * 2));
                    break;
                case InstructionPage.HOTKEYS_5:
                    debugDisplay += "**Even More Current Hotkeys**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 2),
                        1 + (numHotkeyLinesPerPage * 3));
                    break;
                case InstructionPage.HOTKEYS_6:
                    debugDisplay += "**Even MORE More Current Hotkeys**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 3),
                        1 + (numHotkeyLinesPerPage * 4));
                    break;
                case InstructionPage.HOTKEYS_7:
                    debugDisplay += "**Soooo many More Current Hotkeys**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 4),
                        1 + (numHotkeyLinesPerPage * 5));
                    break;
                case InstructionPage.HOTKEYS_8:
                    debugDisplay += "**Current Hotkeys Do I Have To Keep Naming These**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 5),
                        1 + (numHotkeyLinesPerPage * 6));
                    break;
                /*
                case InstructionPage.HOTKEYS_9:
                    debugDisplay += "**Current Hotkeys Nine**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 6),
                        1 + (numHotkeyLinesPerPage * 7));
                    break;
                case InstructionPage.HOTKEYS_10:
                    debugDisplay += "**Current Hotkeys Ten**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 7),
                        1 + (numHotkeyLinesPerPage * 8));
                    break;
                case InstructionPage.HOTKEYS_11:
                    debugDisplay += "**Current Hotkeys Eleven**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 8),
                        1 + (numHotkeyLinesPerPage * 9));
                    break;
                    */
                case InstructionPage.QUICKBOOT:
                    debugDisplay += "**QuickBoot**\n";
                    debugDisplay +=
                        "Do you find yourself opening and closing\nthe game often and wish you had a faster way to\n" +
                        "immediately start a stage?\n" +
                        "By setting the \"bootupLevel\" in your config file\n" +
                        "to the name of a valid level in-game (CaseSensitive!)\n" +
                        "The game will immediately drop Lilac \n" +
                        "into that Level after the first set of Logos.\n\n" +
                        "(Tip: Change the value to Empty Quotes (\"\")\nif you want to continue booting to the Main Menu.)\n";
                    break;
                case InstructionPage.NONE:
                    debugDisplay += "That's all, folks!\n" +
                                    String.Format("When you're ready, press {0} to close this guide.\n",
                                        PHKToggleInstructions.Value) +
                                    "If you need more info, reach out \nto me either on GitHub,\n" +
                                    "GalaxyTrail Discord, or \nthe Freedom Planet Speedrunning Discord.\n" +
                                    "Please be sure that you've ACTUALLY \nread the readme and these instructions first\n" +
                                    "And are prepared to explain what you tried.\n" +
                                    "I spent hours writing these instructions,\n" +
                                    "so I'll be a little bit _unkind_ \n" +
                                    "if you didn't read before contacting me.\n";
                    break;
                default:
                    debugDisplay += "this is bugged. i have no idea how you got here.\n";
                    break;
            }

            debugDisplay += String.Format(
                "{0} / {1} -> View Next / Prev Page.\nPress {2} to toggle Instructions on or off. {3}: Close Dataviewer.\n",
                PHKShowNextDataPage.Value, PHKShowPreviousDataPage.Value, PHKToggleInstructions.Value, PHKHideDataView.Value);
        }

        public void UpdateDPS()
        {
            if (dpsTracker != null) dpsTracker.Update();

            UpdateDPSNearestEnemy();

            if (currentDataPage == DataPage.DPS_ALL) UpdateDPSALLEnemies();
        }

        private void UpdateDPSNearestEnemy()
        {
            // Show nearest enemy HP and update DPS.
            if (FPStage.currentStage != null)
            {
                //var activeEnemies = FPStage.GetActiveEnemies();
                nearestEnemy = FPStage.FindNearestEnemy(fpplayer, 2000f);
                if (nearestEnemy != null)
                {
                    if (nearestEnemy == nearestEnemyPrevious
                        && nearestEnemy.health < nearestEnemyPreviousHP)
                    {
                        dpsTracker.AddDamage(nearestEnemyPreviousHP - nearestEnemy.health);
                    }
                    else if (nearestEnemy != nearestEnemyPrevious)
                    {
                        // Do we want to reset on target changed??
                    }

                    nearestEnemyPreviousHP = nearestEnemy.health;
                }
                // Add toggle option to check against damage done to ALL enemies instead of just nearest
                // If adding, give warning that this may cause slowdown.

                nearestEnemyPrevious = nearestEnemy;
            }
        }

        private void UpdateDPSALLEnemies()
        {
            if (currentDataPage == DataPage.DPS_ALL)
            {
                var tempCachedEnemyList = FPStage.GetActiveEnemies();
                InitializeActiveEnemyList();
                PopulateTrainerActiveEnemyList(tempCachedEnemyList);

                if (allActiveEnemiesHealth != null
                    && allActiveEnemiesHealthPrevious != null
                    && allActiveEnemiesHealth.Count > 0)
                    foreach (var ene in allActiveEnemiesHealth)
                        if (allActiveEnemiesHealthPrevious.ContainsKey(ene.Key))
                        {
                            var dmg = allActiveEnemiesHealthPrevious[ene.Key] - ene.Value;
                            if (dmg > 0) dpsTracker.AddDamage(dmg, allActiveEnemiesNames[ene.Key]);
                        }

                allActiveEnemiesHealthPrevious = new Dictionary<int, float>(allActiveEnemiesHealth);
            }
        }

        public void InitializeActiveEnemyList()
        {
            /*
            if (allActiveEnemiesHealth == null)
            {
                allActiveEnemiesHealth = new Dictionary<int, float>();
                allActiveEnemiesNames = new Dictionary<int, string>();
            }
            else
            {
                allActiveEnemiesHealth.Clear();
                allActiveEnemiesNames.Clear();
            }
            */

            allActiveEnemiesHealth = new Dictionary<int, float>();
            allActiveEnemiesNames = new Dictionary<int, string>();
        }

        private void PopulateTrainerActiveEnemyList(List<FPBaseEnemy> tempCachedEnemyList)
        {
            foreach (var ene in tempCachedEnemyList)
                try
                {
                    allActiveEnemiesHealth.Add(ene.objectID, ene.health);
                    allActiveEnemiesNames.Add(ene.objectID, ene.name);
                }
                catch (ArgumentException e)
                {
                    Log(e.ToString());
                    Log(e.StackTrace);
                }
        }

        public static List<FPPlayer> GetFPPlayers()
        {
            var listPlayers =  new List<FPPlayer>(GameObject.FindObjectsOfType<FPPlayer>());
            foreach (var player in listPlayers)
            {
                FPStage.ValidateStageListPos(player);
            }
            return listPlayers.OrderBy(fpp => fpp.characterID).ToList();
        }

        public static void ValidateAllFPPlayers()
        {
            if (fpplayer != null)
            {
                FPStage.ValidateStageListPos(fpplayer);
            }

            foreach (var player in fpplayers)
            {
                FPStage.ValidateStageListPos(player);
            }
        }

        private GameObject GetFirstPlayerGameObject()
        {
            GameObject playerGameObject = null;
            if (FPStage.player == null) return playerGameObject;

            MelonLogger.Msg("Number of Stage Players: " + FPStage.player.Length);
            if (FPStage.currentStage != null && FPStage.currentStage.GetPlayerInstance_FPPlayer() != null)
                playerGameObject = FPStage.currentStage.GetPlayerInstance_FPPlayer().gameObject;
            else if (FPStage.player.Length > 0) playerGameObject = FPStage.player[0].gameObject;

            return playerGameObject;
        }

        public void HandleHotkeys()
        {
            //if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.attack))
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKLoadDebugRoom))
            {
                Log("Load Debug Room");
                //FP2TrainerAllyControls.DumpAllPlayerVars();
                SceneManager.LoadScene("StageDebugMenu", LoadSceneMode.Additive);
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKGoToMainMenu))
            {
                Log("Main Menu");
                //UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                GoToMainMenuNoLogos();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKLoadAssetBundles))
            {
                Log("Load Asset Bundles");
                LoadAssetBundlesFromModsFolder();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKGoToLevelSelectMenu))
            {
                Log("Level Select");
                if (showInstructions)
                {
                    ToggleShowInstructions();
                }

                var availableScenes = new List<SceneNamePair>();
                var i = 0;
                for (i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    var sceneName =
                        Path.GetFileNameWithoutExtension(SceneUtility
                            .GetScenePathByBuildIndex(i));
                    availableScenes.Add(new SceneNamePair(SceneManager.GetSceneByBuildIndex(i), sceneName));
                }

                for (i = 0; i < loadedAssetBundles.Count; i++)
                    foreach (var scenePath in loadedAssetBundles[i].GetAllScenePaths())
                    {
                        var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                        availableScenes.Add(new SceneNamePair(SceneManager.GetSceneByPath(scenePath), sceneName,
                            scenePath));
                    }

                for (i = 0; i < availableScenes.Count; i++) Log(i + " | " + availableScenes[i].name);

                ShowLevelSelect(availableScenes);
                pauseMenu.gameObject.SetActive(false);
                //GameObject.Destroy(this.pauseMenu);
            }

            if (false /*FP2TrainerCustomHotkeys.GetButtonDown(PHKasdfasfd)*/)
            {
                Log("F5 -> Toggle Level Select Menu Visibility");
                ToggleLevelSelectVisibility();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKShowNextDataPage))
            {
                if (showInstructions)
                {
                    IncrementInstructionPage();
                    Log("Next Instruction Page (" + Enum.GetName(typeof(InstructionPage), currentInstructionPage) +
                        ")");
                }
                else
                {
                    ToggleVariableDisplay();
                    Log("Next Data Page (" + Enum.GetName(typeof(DataPage), currentDataPage) + ")");
                }
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKShowPreviousDataPage))
            {
                if (showInstructions)
                {
                    DecrementInstructionPage();
                    Log("Previous Instruction Page (" + Enum.GetName(typeof(InstructionPage), currentInstructionPage) +
                        ")");
                }
                else
                {
                    ToggleVariableDisplayPrevious();
                    Log("Previous Data Page (" + Enum.GetName(typeof(DataPage), currentDataPage) + ")");
                }
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKHideDataView))
            {
                if (currentDataPage != DataPage.NONE)
                {
                    currentDataPage = DataPage.NONE;
                    Log("Hide Inspector");
                }
                else
                {
                    currentDataPage = DataPage.MOVEMENT;
                    Log("Reveal Inspector");
                }
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKGoToLevelAtLastIndex))
            {
                Log("Load last located scene: ");
                if (fptls != null)
                {
                    var iWantToGoToBed = fptls.availableScenes[fptls.availableScenes.Count - 1];
                    Log(iWantToGoToBed.name);
                    SceneManager.LoadScene(iWantToGoToBed.name);
                }
                else
                {
                    Log("...But the Level Selector hasn't been created yet... (Press F6?)");
                }
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKKOCharacter))
            {
                //TestDamageNumberPopups();

                if (fpplayer != null)
                {
                    Log("KO the Player");
                    InstaKOPlayer();
                }
                else
                {
                    Log("Attempted to KO the player, but no FPPlayer instance was found");
                }
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKKOBoss))
            {
                //TestDamageNumberPopups();

                ReacquireBossHuds();
                if (fpEnemies.Count > 0)
                {
                    Log("KO Boss");
                    foreach (var enemy in fpEnemies)
                    {
                        enemy.health = 0;
                    }
                }
                else
                {
                    Log("Attempted to KO all bosses, but no bosses were found... (Check for visible HUDs?)");
                }
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKInvincibleBoss))
            {
                EnableInvincibleBoss.Value = !EnableInvincibleBoss.Value;
                Log($"Toggled Invincible Bosses: {!EnableInvincibleBoss.Value} => {EnableInvincibleBoss.Value}");

                /*
                ReacquireBossHuds();
                if (fpEnemies.Count > 0)
                {
                    Log("KO Boss");
                    foreach (var enemy in fpEnemies)
                    {
                        enemy.health = 0;
                    }
                }
                else
                {
                    Log("Attempted to KO all bosses, but no bosses were found... (Check for visible HUDs?)");
                }
                */
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKInvinciblePlayers))
            {
                EnableInvinciblePlayers.Value = !EnableInvinciblePlayers.Value;
                Log($"Toggled Invincible Players: {!EnableInvinciblePlayers.Value} => {EnableInvinciblePlayers.Value}");
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKToggleInstructions))
            {
                //TestDamageNumberPopups();

                ToggleShowInstructions();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKToggleNoClip))
            {
                Log("NoClip Toggle");
                ToggleNoClip();
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKReturnToCheckpoint))
            {
                Log("Return to Checkpoint");
                ReturnToCheckpoint();
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKRestartStage))
            {
                Log("Restart Stage");
                RestartLevel();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKTogglePlaneSwitcherVisualizers))
            {
                TogglePlaneSwitchVisualizers();
                ShowPlaneSwitcherVisualizersLastSetting.Value = planeSwitchVisualizersVisible;
                Log(String.Format("Toggle PlaneSwitcher Visualizers: {0} -> {1}", !planeSwitchVisualizersVisible,
                    planeSwitchVisualizersVisible));
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKToggleShowColliders))
            {
                ToggleShowColliders();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKGetOutGetOutGetOut))
            {
                Log("GET OUT GET OUT GET OUT");
                SpawnSpoilerGimmick();
            }

            HandleMultiplayerSpawnHotkeys();
            HandleInstaSwapHotkeys();
            HandleResizeFontHotkeys();
            HandleCameraHotkeys();


            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKGotoWarpPoint))
            {
                fpplayer.position = new Vector2(warpPoint.x, warpPoint.y);
                Log("Goto Warp: " + warpPoint);
                warpMessage = "Warping to " + warpPoint;
                timeoutShowWarpInfo = howLongToShowWarpInfo;
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSetWarpPoint))
            {
                warpPoint = new Vector2(fpplayer.position.x, fpplayer.position.y);
                Log("Set Warp: " + warpPoint);
                warpMessage = "Set warp at " + warpPoint;
                timeoutShowWarpInfo = howLongToShowWarpInfo;
            }

            // I'd like to preserve the gamepad version of this somehow...

            /*
            if (InputControl.GetButton(Controls.buttons.pause) && InputControl.GetButtonDown(Controls.buttons.special))
            {
                ToggleNoClip();
            }

            if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.special))
            {
                ToggleNoClip();
            }
            */

            HandleNoClip();
        }

        public static void ToggleShowColliders()
        {
            ShowCollidersLastSetting.Value = !ShowCollidersLastSetting.Value;
            Log(String.Format("Toggle Show Debug Colliders: {0} -> {1}", !ShowCollidersLastSetting.Value,
                ShowCollidersLastSetting.Value));
        }

        private void ToggleShowInstructions()
        {
            if (fpplayer != null)
            {
                Log(String.Format("Toggle Instructions: ({0}) -> ({1})", showInstructions, !showInstructions));
                showInstructions = !showInstructions;

                ShowInstructionsOnStart.Value = showInstructions;

                if (showInstructions)
                {
                    currentInstructionPage = InstructionPage.BASICS;
                }
            }
        }

        private void HandleMultiplayerSpawnHotkeys()
        {
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSpawnExtraChar))
            {
                //TestDamageNumberPopups();

                if (fpplayer != null)
                {
                    Log("Shift + F2 -> Enable MultiCharacter");
                    /*
                    var goNewPlayer = GameObject.Instantiate(fpplayer.gameObject);
                    goNewPlayer.transform.position = new Vector3(fpplayer.position.x - 64, fpplayer.position.y,
                        fpplayer.gameObject.transform.position.z);
                    */

                    FPPlayer2p.SpawnExtraCharacter();
                    fpplayers = GetFPPlayers();
                    currentDataPage = DataPage.MULTIPLAYER_DEBUG;
                }
                else
                {
                    Log("Shift + F2 -> Attempted to start 2P but could not find 1P");
                }
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKToggleMultiCharStart))
            {
                multiplayerStart = !multiplayerStart;
                MultiCharStartLastSetting.Value = multiplayerStart;
                Log(String.Format("Toggle Multiplayer Start ({0} -> {1})", !multiplayerStart, multiplayerStart));
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKStartSplitscreen))
            {
                Log("Start Splitscreen");
                StartSplitscreen();
                Log("Start Splitscreen");
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwapBetweenSpawnedChars))
            {
                Log("Attempting to dump character info.");
                foreach (var fpp in fpplayers)
                {
                    DumpAllPlayerVarsAndComponents(fpp);
                } //DELETEME

                Log("Dumped character info.");

                FPPlayer2p.SwapBetweenActiveCharacters();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKCyclePreferredAllyControlType))
            {
                FP2TrainerAllyControls.CyclePreferredAllyControlType();
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKStartInputPlayback))
            {
                Log("Attempting to force player to replay ghost.");
                skipRecording = true;
                fpplayer.inputMethod = fpplayer.HandleAllyControlsGhost;
                FP2TrainerAllyControls.needToLoadInputs = true;
                
                foreach (var fpp in fpplayers)
                {
                    DumpAllPlayerVarsAndComponents(fpp);
                }
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKToggleLockP1ToGhostFiles))
            {
                LockP1ToGhostFiles.Value = !LockP1ToGhostFiles.Value;
                Log($"Toggle Lock Player1 To Ghost Files: {!LockP1ToGhostFiles.Value} => {LockP1ToGhostFiles.Value}");

                if (LockP1ToGhostFiles.Value)
                {
                    Log("Attempting to force player to replay ghost.");
                    skipRecording = true;
                    fpplayer.inputMethod = fpplayer.HandleAllyControlsGhost;
                    FP2TrainerAllyControls.needToLoadInputs = true;
                
                    foreach (var fpp in fpplayers)
                    {
                        DumpAllPlayerVarsAndComponents(fpp);
                    }
                }
            }
        }

        private void HandleInstaSwapHotkeys()
        {
            if (!UseInstaSwitch.Value)
            {
                return;
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToLilac))
            {
                currentPreferredCharacter = FPCharacterID.LILAC;
                fpplayer.characterID = FPCharacterID.LILAC;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToCarol))
            {
                currentPreferredCharacter = FPCharacterID.CAROL;
                fpplayer.characterID = FPCharacterID.CAROL;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToCarolBike))
            {
                currentPreferredCharacter = FPCharacterID.BIKECAROL;
                fpplayer.characterID = FPCharacterID.BIKECAROL;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToMilla))
            {
                currentPreferredCharacter = FPCharacterID.MILLA;
                fpplayer.characterID = FPCharacterID.MILLA;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToNeera))
            {
                currentPreferredCharacter = FPCharacterID.NEERA;
                fpplayer.characterID = FPCharacterID.NEERA;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToNext))
            {
                if (fpplayer.characterID >= FPCharacterID.NEERA)
                {
                    fpplayer.characterID = FPCharacterID.LILAC;
                }
                else
                {
                    fpplayer.characterID++;
                }
                
                currentPreferredCharacter = fpplayer.characterID;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToPrev))
            {
                if (fpplayer.characterID <= 0)
                {
                    fpplayer.characterID = FPCharacterID.NEERA;
                }
                else
                {
                    fpplayer.characterID--;
                }
                
                currentPreferredCharacter = fpplayer.characterID;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }

        }

        private void HandleResizeFontHotkeys()
        {
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKIncreaseFontSize))
            {
                Log("Shift + Plus -> Increase Font Size: ");
                if (textmeshFancyTextPosition != null) textmeshFancyTextPosition.characterSize++;
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKDecreaseFontSize))
            {
                Log("Shift + Minus -> Decrease Font Size: ");
                if (textmeshFancyTextPosition != null) textmeshFancyTextPosition.characterSize--;
            }
        }

        private void HandleCameraHotkeys()
        {
            if (FP2TrainerCustomHotkeys.GetButton(PHKCameraZoomOut))
            {
                if (FPCamera.stageCamera != null)
                {
                    Log(String.Format("{0} -> Camera Zoom Out: {1} / {2}", PHKCameraZoomOut.Value,
                        FPCamera.stageCamera.GetZoom(), FPCamera.stageCamera.zoomMax));
                    trainerRequestZoomValue += 0.1f;
                    FPCamera.stageCamera.RequestZoom(trainerRequestZoomValue, FPCamera.ZoomPriority_VeryHigh);
                }
            }

            if (FP2TrainerCustomHotkeys.GetButton(PHKCameraZoomIn))
            {
                Log("Minus -> Camera Zoom In: ");
                if (FPCamera.stageCamera != null)
                {
                    Log(String.Format("{0} -> Camera Zoom In: {1} / {2}", PHKCameraZoomIn.Value,
                        FPCamera.stageCamera.GetZoom(), FPCamera.stageCamera.zoomMin));
                    trainerRequestZoomValue -= 0.1f;
                    FPCamera.stageCamera.RequestZoom(trainerRequestZoomValue, FPCamera.ZoomPriority_VeryHigh);
                }
            }

            if (FP2TrainerCustomHotkeys.GetButton(PHKCameraZoomReset))
            {
                Log("Numpad Period . -> Camera Reset: ");
                if (FPCamera.stageCamera != null)
                {
                    trainerRequestZoomValue = 1f;
                    FPCamera.stageCamera.RequestZoom(trainerRequestZoomValue);
                }
            }

            FPCamera.stageCamera.RequestZoom(trainerRequestZoomValue);
        }

        public void HandleInvincibility()
        {
            if (EnableInvinciblePlayers.Value)
            {
                if (fpplayers != null)
                {
                    if (fpplayers.Count < 1)
                    {
                        fpplayers = GetFPPlayers();
                    }

                    foreach (var fpp in fpplayers)
                    {
                        fpp.health = fpp.healthMax;
                    }
                }

            }
            
            if (EnableInvincibleBoss.Value)
            {
                ReacquireBossHuds();
                foreach (var enemy in FPStage.GetActiveEnemies())
                {
                    enemy.health = 100;
                }
                
                if (bossHuds != null)
                {
                    foreach (var bossHud in bossHuds)
                    {
                        bossHud.targetBoss.health = bossHud.maxHealth;
                    }
                }

            }
            
            if (fpEnemies != null)
            {
                foreach (var boss in fpEnemies)
                {
                    boss.cannotBeKilled = EnableInvincibleBoss.Value;
                }
            }

        }

        private static bool InputGetKeyAnyShift()
        {
            return (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }

        private void ToggleNoClip()
        {
            if (noClip)
            {
                EndNoClip();
            }
            else
            {
                noClip = true;
                fpplayer.terrainCollision = false;
                noClipStartPos = fpplayer.position;
                noClipCollisionLayer = fpplayer.collisionLayer;
                noClipGravityStrength = fpplayer.gravityStrength;
            }
        }

        private void InstaKOPlayer()
        {
            if (fpplayer)
            {
                fpplayer.hurtKnockbackX = fpplayer.velocity.x;
                fpplayer.hurtKnockbackY = 0f;
                fpplayer.nextAttack = 0;
                fpplayer.genericTimer = -20f;
                fpplayer.superArmor = false;
                fpplayer.superArmorTimer = 0f;
                fpplayer.invincibilityTime = 200f;
                FPSaveManager.perfectRun = false;
                FPSaveManager.KOs++;
                fpplayer.recoveryTimer = 0f;
                fpplayer.state = fpplayer.State_KO;
                fpplayer.velocity.y = 8f;
                fpplayer.velocity.x *= 2f;
                fpplayer.Action_PlaySoundUninterruptable(fpplayer.sfxKO);
                fpplayer.Action_PlayVoice(fpplayer.vaKO);
            }
        }

        private void SimulateDPSDamageAdd()
        {
            Log("F2 -> Simulate DPS Damage Add: ");
            dpsTracker.AddDamage(5, "FP2 Trainer HotKey");
        }

        private void TestDamageNumberPopups()
        {
            Log("F1 -> Test Damage Number: ");
            if (fpplayer != null)
            {
                var fpcam = FPCamera.stageCamera;
                Log("1");
                if (fpcam != null)
                {
                    Log("2");
                    var relativePos = new Vector3(fpplayer.position.x - fpcam.xpos,
                        fpplayer.position.y - fpcam.ypos, fpplayer.gameObject.transform.position.z);
                    Log("3");
                    Log(relativePos.ToString());
                    Log("4");
                    var goDmgTest = FP2TrainerDamageNumber.CreateDMGNumberObject(relativePos, 5);
                    Log("5");
                }
            }
            else
            {
                Log("No player???");
            }
        }

        private void HandleNoClip()
        {
            //fpplayer.enablePhysics = false;

            if (noClip && fpplayer != null)
            {
                fpplayer.collisionLayer = -999;
                fpplayer.invincibilityTime = 100f;
                fpplayer.gravityStrength = 0;
                fpplayer.hitStun = -1;

                fpplayer.velocity.x = 0;
                fpplayer.velocity.y = 0;

                float modifiedNoClipMoveSpeed = noClipMoveSpeed;
                if (InputControl.GetButton(Controls.buttons.special))
                {
                    modifiedNoClipMoveSpeed *= 4f;
                }

                fpplayer.velocity = Vector2.zero;
                if (fpplayer.input.up
                    || InputControl.GetAxis(Controls.axes.vertical) > 0.2f)
                {
                    fpplayer.position.y += modifiedNoClipMoveSpeed * 1;
                }

                if (fpplayer.input.down
                    || InputControl.GetAxis(Controls.axes.vertical) < -0.2f)
                {
                    fpplayer.position.y -= modifiedNoClipMoveSpeed * 1;
                }

                if (fpplayer.input.right
                    || InputControl.GetAxis(Controls.axes.horizontal) > 0.2f)
                {
                    fpplayer.position.x += modifiedNoClipMoveSpeed * 1;
                }

                if (fpplayer.input.left
                    || InputControl.GetAxis(Controls.axes.horizontal) < -0.2f)
                {
                    fpplayer.position.x -= modifiedNoClipMoveSpeed * 1;
                }


                if (InputControl.GetButtonDown(Controls.buttons.attack))
                {
                    EndNoClip();
                }

                if (InputControl.GetButtonDown(Controls.buttons.jump))
                {
                    EndNoClipAndReturnToStartPosition();
                }
            }
        }

        private void EndNoClip()
        {
            fpplayer.invincibilityTime = 0f;
            fpplayer.gravityStrength = noClipGravityStrength;
            fpplayer.hitStun = 0f;
            fpplayer.collisionLayer = noClipCollisionLayer;
            fpplayer.terrainCollision = true;

            /*
            if (currentDataPage == DataPage.NO_CLIP)
            {
                currentDataPage++;
            }
            */

            noClip = false;

            //fpplayer.enablePhysics = true;
        }

        private void EndNoClipAndReturnToStartPosition()
        {
            fpplayer.position = noClipStartPos;
            EndNoClip();
        }

        private void ToggleVariableDisplay()
        {
            if (currentDataPage == DataPage.NONE)
                currentDataPage = DataPage.MOVEMENT;
            else
                currentDataPage++;

            UpdateAfterDataPageChange();
        }

        private void ToggleVariableDisplayPrevious()
        {
            if (currentDataPage == DataPage.MOVEMENT)
                currentDataPage = DataPage.NONE;
            else
                currentDataPage--;

            UpdateAfterDataPageChange();
        }

        private void IncrementInstructionPage()
        {
            if (currentInstructionPage < InstructionPage.NONE)
            {
                currentInstructionPage++;
            }

            UpdateAfterDataPageChange();
        }

        private void DecrementInstructionPage()
        {
            if (currentInstructionPage > InstructionPage.BASICS)
            {
                currentInstructionPage--;
            }

            UpdateAfterDataPageChange();
        }

        private void ResetInstructionPage()
        {
            currentInstructionPage = InstructionPage.BASICS;
            UpdateAfterDataPageChange();
        }

        private void UpdateAfterDataPageChange()
        {
            // After incrementing.
            if (currentDataPage == DataPage.NONE && !showInstructions && !(stageSelectMenu == null))
                showVarString = false;
            else
                showVarString = true;

            if (currentDataPage == DataPage.BOSS) ReacquireBossHuds();
            if (currentDataPage == DataPage.MULTIPLAYER_DEBUG)
            {
                fpplayers = GetFPPlayers();
            }
        }

        public void ReacquireBossHuds()
        {
            bossHuds = new List<FPBossHud>(Object.FindObjectsOfType<FPBossHud>());
            fpEnemies = new List<FPBaseEnemy>();
            foreach (var fpbh in bossHuds)
                if (fpbh != null && fpbh.targetBoss != null)
                    fpEnemies.Add(fpbh.targetBoss);
        }

        public static void LoadAssetBundlesFromModsFolder()
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
                        Log("Failed to load AssetBundle. Bundle may already be loaded, or the file may be corrupt.");
                        continue;
                    }

                    //currentAB.LoadAllAssets(); //Uncomment if the scenes are still unloadable?
                    loadedAssetBundles.Add(currentAB);
                    Log("AssetBundle loaded successfully as loadedAssetBundles[" + (loadedAssetBundles.Count - 1) +
                        "]:");
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
                stageSelectMenu.SetActive(!stageSelectMenu.activeInHierarchy);
            // finna delete
            /*
                var ssm = stageSelectMenu.GetComponent<FPPauseMenu>();
                fptls = stageSelectMenu.AddComponent<FPTrainerLevelSelect>();
                if (ssm != null)
                {
                    UnityEngine.Object.Destroy(ssm);
                }
                if (fptls != null)
                {
                    UnityEngine.Object.Destroy(ssm);
                }
                */
            else
                Log("Attempted to toggle Level Select Visibility while it is not accessible.");
        }

        private void ShowLevelSelect(List<SceneNamePair> availableScenes)
        {
            if (stageSelectMenu != null)
            {
                Log("Level Select.");
                //stageSelectMenu.SetActive(true);

                fptls = stageSelectMenu.AddComponent<FPTrainerLevelSelect>();
                fptls.availableScenes = availableScenes;
                GameObject goButton = null;

                var tempGoButton = stageSelectMenu.transform.Find("AnnStagePlayIcon").gameObject;
                if (tempGoButton != null) goButton = tempGoButton;

                int i;

                fptls.pfButtons = new GameObject[availableScenes.Count];

                GameObject currentButton = null;
                TextMesh tm = null;
                MenuText mt = null;
                for (i = 0; i < availableScenes.Count; i++)
                {
                    currentButton = Object.Instantiate(goButton, stageSelectMenu.transform);
                    currentButton.transform.localPosition = new Vector3(0, -32 - 32 * i, 0);

                    tm = currentButton.GetComponent<TextMesh>();
                    mt = currentButton.GetComponent<MenuText>();
                    fptls.pfButtons.SetValue(currentButton, i);
                    if (tm != null)
                    {
                        tm.text = availableScenes[i].name;
                        mt.paragraph[0] = availableScenes[i].name;
                    }
                }

                Log("fptls button count: " + fptls.pfButtons.Length);
            }
            else
            {
                Log("Attempted to show level select, but the menu has not been prepared.");
            }

            PauseGameWithoutPauseMenu();
        }

        private void PauseGameWithoutPauseMenu()
        {
            FPStage.UpdateMenuInput();
            FPStage.SetStageRunning(false);
            FPAudio.ResumeSfx();
            FPAudio.PlayMenuSfx(2);
        }

        public void PerformStageTransition()
        {
            var component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            component.transitionType = FPTransitionTypes.LOCAL_WIPE;
            component.transitionSpeed = 48f;
            component.SetTransitionColor(0f, 0f, 0f);
            component.BeginTransition();
            FPAudio.PlayMenuSfx(3);
        }

        private static void WriteSceneObjectsToFile()
        {
            if (!warped)
            {
                warped = true;

                var allObjects = "";
                GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();

                foreach (var obj in objs)
                {
                    allObjects += obj.name + " | " + obj.activeInHierarchy + "\r\n";
                    foreach (UnityEngine.MonoBehaviour mb in obj.GetComponents<MonoBehaviour>())
                    {
                        allObjects += "+MonoBehaviors: " + mb.GetType().Name + " | " + mb.isActiveAndEnabled + "\r\n";
                    }
                }
                // UMFGUI.AddConsoleText(allObjects);

                var fileName = "SceneObjects.txt";
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

        private void WriteAllAudioclipsToFile()
        {
            if (!warped)
            {
                warped = true;

                var allAudioClips = "";
                Object[] acs = Resources.FindObjectsOfTypeAll<AudioClip>();

                foreach (AudioClip ac in acs) allAudioClips += ac.name + "\r\n";
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

        public bool GetKeyPressed(string s)
        {
            try
            {
                var sTrim = s.Split('/')[1];
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
                var sTrim = s.Split('/')[1];
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
            var result = "NULL";
            //f (copyTile != null) { result = copyTile.name; }
            return result;
        }

        public GameObject GetStageHUD()
        {
            var goHud = GameObject.Find("Stage HUD");
            if (goHud) MelonLogger.Msg("Found a Stage HUD.");
            //GameObject goPauseHud = GameObject.Find("Hud Pause Menu");

            return goHud;
        }

        public void SkipBootIntros()
        {
            var splash = Object.FindObjectOfType<MenuSplashScreen>();
            /*
            if (splash != null)
            {
                var propTimer = splash.GetType().GetField("timer", System.Reflection.BindingFlags.NonPublic
                                                                | System.Reflection.BindingFlags.Instance);
                propTimer.SetValue(splash, 9999);
            }*/

            if (introSkipped < 1)
            {
                string level = BootupLevel.Value;
                Log("BootupLevel: " + BootupLevel.Value);
                if (level != null && !level.Equals(""))
                {
                    GoToCustomBootLevelImmediate(level);
                }
                else
                {
                    GoToMainMenuNoLogos();
                }
            }

            /*
            if (introSkipped == 1)
            {
                string level = BootupLevel.Value;
                Log("BootupLevel: " + BootupLevel.Value);
                Log("level: " + level);
                if (level != null && !level.Equals(""))
                {
                    Log("1");
                    GoToCustomBootLevelImmediate(level);
                    
                }
                else
                {
                    Log("2");
                    GoToMainMenuNoLogos();
                }
            }
            */
        }

        public static void GoToMainMenuNoLogos()
        {
            GoToCustomBootLevel("MainMenu");
        }

        public static void GoToCustomBootLevel(string level)
        {
            Log("Now Loading Custom Boot: " + level);
            var component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            if (component != null)
            {
                component.transitionType = FPTransitionTypes.WIPE;
                component.transitionSpeed = 48f;
                component.sceneToLoad = level;
                FPSaveManager.menuToLoad = 2; // This is how we skip the intros.

                introSkipped++;
            }
        }

        public static void GoToCustomBootLevelImmediate(string level)
        {
            Log("Now Loading Custom Boot Immediate: " + level);
            var component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            if (component != null)
            {
                //component.transitionType = FPTransitionTypes.WIPE;
                //component.transitionSpeed = 48f;
                //component.sceneToLoad = level;
                //FPSaveManager.menuToLoad = 2; // This is how we skip the intros.

                SceneManager.LoadSceneAsync(level);

                introSkipped++;
            }
        }

        private IEnumerator LoadAsyncScene()
        {
            var asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
            while (!asyncLoad.isDone) yield return null;
        }

        public static void SetFP2TDeltaTime(float dt)
        {
            fp2tDeltaTime = dt;
        }

        public static float GetFP2TDeltaTime()
        {
            return fp2tDeltaTime;
        }

        public void GrabAndUpdateCameraDetails()
        {
            if (FPCamera.stageCamera != null)
            {
                originalZoomMin = FPCamera.stageCamera.zoomMin;
                originalZoomMax = FPCamera.stageCamera.zoomMax;
                originalZoomSpeed = FPCamera.stageCamera.zoomSpeed;

                FPCamera.stageCamera.zoomMin = trainerZoomMin;
                FPCamera.stageCamera.zoomMax = trainerZoomMax;
                FPCamera.stageCamera.zoomSpeed = trainerZoomSpeed;
            }
        }

        public void GrabAndTweakPauseMenu()
        {
            TrainerPauseMenu.GrabAndTweakPauseMenu();
        }

        public void SpawnSpoilerGimmick()
        {
            if (cacheGameObjectHunter == null)
            {
                Log("~~~~1");
                GetReferencesToSpoilerGimmick();
                Log("~~~~2");
            }
            else
            {
                GetReferencesToSpoilerGimmickPart3();
            }
        }

        private static void GetReferencesToSpoilerGimmick()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            //SceneManager.LoadScene("Bakunawa5", LoadSceneMode.Additive);
            Log("@@@@@+");
            LoadAssetBundlesFromModsFolder();
            Log("@@@@@0");
            ListScenesToLog();
            SceneManager.LoadScene("SpoilerGimmick", LoadSceneMode.Additive);
            // The scenes will appear as available immediately, but will not actually load until the next update frame.
            waitingForNextFrameForSpoilerGimmick = true;


            /*
            Log("@@@@@3");
            if (goHunter != null)
            {
                Log("See you.");
                /*
                SceneManager.MoveGameObjectToScene(goHunter, SceneManager.GetActiveScene());
                SceneManager.MoveGameObjectToScene(goHunterKO, SceneManager.GetActiveScene());
                SceneManager.MoveGameObjectToScene(goArc, SceneManager.GetActiveScene());
                SceneManager.MoveGameObjectToScene(goMeter, SceneManager.GetActiveScene());
                */ /*
                
                GameObject.DontDestroyOnLoad(goHunter);
                GameObject.DontDestroyOnLoad(goHunterKO);
                GameObject.DontDestroyOnLoad(goArc);
                GameObject.DontDestroyOnLoad(goMeter);

                goHunter.transform.parent = cacheGameObjectHunter.transform;
                goHunterKO.transform.parent = cacheGameObjectHunter.transform;
                goArc.transform.parent = cacheGameObjectHunter.transform;
                goMeter.transform.parent = cacheGameObjectHunter.transform;
            }
            else
            {
                Log("@@@@@4");
                Log("Didn't find Hunter. Trying Approach 2.\n");
                foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (go.name.Equals("Syntax Hunter"))
                    {
                        Log("Found Hunter.");
                        goHunter = go;
                    }
                    else if (go.name.Equals("HunterKOScreen"))
                    {
                        Log("Found HunterKO.");
                        goHunterKO = go;
                    }
                    else if (go.name.Equals("arc"))
                    {
                        Log("Found arc.");
                        goArc = go;
                    }
                    else if (go.name.Equals("Hud Stealth Meter"))
                    {
                        Log("Found Meter.");
                        goMeter = go;
                    }

                    if (goHunter && goHunterKO && goArc && goMeter)
                    {
                        Log("Found all.");
                        break;
                    }
                }
                Log("@@@@@5");
                Log(String.Format("Gimmick Objects: ({0},{1},{2},{3})\n", goHunter, goHunterKO, goArc, goMeter));
                Log("@@@@@6");
            }
            */
            //ListScenesToLog();
        }

        public static void GetReferencesToSpoilerGimmickPart2()
        {
            waitingForNextFrameForSpoilerGimmick = false;
            ListScenesToLog();
            Log("Are scenes loaded? " + SceneManager.GetSceneAt(0).isLoaded.ToString() +
                SceneManager.GetSceneAt(1).isLoaded.ToString());
            SceneManager.MergeScenes(SceneManager.GetSceneAt(0), SceneManager.GetSceneAt(1));

            //SceneManager.SetActiveScene(currentScene);

            Log("@@@@@1");
            ListScenesToLog();
            Log("@@@@@2");

            WriteSceneObjectsToFile();

            GameObject goHunter = GameObject.Find("Syntax Hunter");
            GameObject goHunterKO = GameObject.Find("HunterKO");
            GameObject goHunterKOScreen = GameObject.Find("HunterKOScreen");
            GameObject goArc = GameObject.Find("arc");
            GameObject goMeter = GameObject.Find("Hud Stealth Meter");

            Log("@@@@@3");
            Log(String.Format("Gimmick Objects: ({0},{1},{2},{3},{4})\n", goHunter, goHunterKO, goArc, goMeter,
                goHunterKOScreen));
            Log("@@@@@4");

            cacheGameObjectHunter = new GameObject("cacheHunter");

            goHunter.transform.parent = cacheGameObjectHunter.transform;
            goHunterKO.transform.parent = cacheGameObjectHunter.transform;
            goHunterKOScreen.transform.parent = cacheGameObjectHunter.transform;
            goArc.transform.parent = cacheGameObjectHunter.transform;
            goMeter.transform.parent = cacheGameObjectHunter.transform;

            //move this to part 2
            cacheGameObjectHunter.SetActive(false);
            Log("~~~~3");
            GameObject.DontDestroyOnLoad(cacheGameObjectHunter);
            Log("~~~~4");

            var temp = goHunter.transform.GetComponent<BFSyntaxHunt>();
            GameObject.Destroy(temp);

            var temp2 = goHunter.AddComponent<BFSyntaxHunt>();
            temp2.hudStealthMeter = goMeter.GetComponent<SpriteRenderer>();
            temp2.hudStealthMeterBar = goMeter.transform.Find("bar").GetComponent<SpriteRenderer>();
            temp2.koParent = goHunterKOScreen;
            temp2.body = goHunterKO;
            temp2.playerBody = goHunterKO.transform.Find("players").gameObject;
            temp2.dbWarn = goHunterKOScreen.transform.Find("DBWarn1").GetComponent<SpriteRenderer>();
            temp2.dbWarn2 = goHunterKOScreen.transform.Find("DBWarn2").GetComponent<SpriteRenderer>();


            GetReferencesToSpoilerGimmickPart3();
        }

        public static void GetReferencesToSpoilerGimmickPart3()
        {
            Log("~~~~5");
            var goNewHunter = GameObject.Instantiate(cacheGameObjectHunter);
            Log("~~~~6");
            goNewHunter.SetActive(true);
            //var goHunter = goNewHunter.transform.Find("Syntax Hunter").gameObject;
            //goHunter.gameObject.SetActive(true);
            Log("~~~~7");
            for (int i = 0; i < goNewHunter.transform.childCount; i++)
            {
                Transform child = goNewHunter.transform.GetChild(i);
                Log("~~~~8");
                Log(child.name + " " + child.transform.position + " " + child.gameObject.activeInHierarchy);
            }

            Log("~~~~9");
        }

        private static IEnumerable Suffering()
        {
            Log("tired");
            var ao = SceneManager.UnloadSceneAsync("Bakunawa5");
            yield return ao;
            Cursor.visible = false;
            //GameControl.player.SetActive(true);
            Log("Still Tired.");
            ListScenesToLog();
        }

        private static void ListScenesToLog()
        {
            Log("Scenes: vv look below vv");
            var qwer = SceneManager.sceneCount;
            for (int asdf = 0; asdf < qwer; asdf++)
            {
                Log(SceneManager.GetSceneAt(asdf).name);
            }
        }


        public static void StartMultiplayerHud(FPPlayer fpp)
        {
            //This SHOULD work with some tweaking. Dummied out so I can go to sleep.

            /*
            var gameObject = fpp.gameObject;
            var num = 0f;
            var maxPetals = Mathf.FloorToInt(fpp.healthMax);
            var hudLifePetals = new FPHudDigit[maxPetals];
            var isHudPetalFlashing = new bool[maxPetals];
            for (int i = 0; i < hudLifePetals.Length; i++)
            {
                gameObject = UnityEngine.Object.Instantiate(pfHudLifePetal, new Vector3(num, -31f, 0f), default(Quaternion));
                gameObject.transform.parent = base.transform; //Base in this case refers to the HUD Base.
                hudLifePetals[i] = gameObject.GetComponent<FPHudDigit>();
                num += 36f - (float)maxPetals * 2f;
            }
            float num2 = 88f - (float)maxPetals * 5f;
            var maxShields = maxPetals * 2;
            var hudShields = new FPHudDigit[maxShields];
            for (int j = 0; j < hudShields.Length; j++)
            {
                gameObject = UnityEngine.Object.Instantiate(pfHudShield, new Vector3(num2, -31f, -1f - (float)j * 0.1f), default(Quaternion));
                gameObject.transform.parent = base.transform;
                hudShields[j] = gameObject.GetComponent<FPHudDigit>();
                num2 += 18f - (float)maxPetals * 1f;
            }
            */
        }

        public void ReturnToCheckpoint()
        {
            if (FPStage.checkpointEnabled /*&& FPStage.currentStage.GetPlayerInstance_FPPlayer().state !=
                new FPObjectState(FPStage.currentStage.GetPlayerInstance_FPPlayer().State_KO)*/)
            {
                sceneToLoad = SceneManager.GetActiveScene().name;
                FPAudio.PlayMenuSfx(2);

                FPSaveManager.currentSave.Local_Restart();
                FPStage.checkpointEnabled = false;
                FPStage.checkpointPos = new Vector2(0f, 0f);
                FPSaveManager.stageDoorFlags = new bool[10];
                FPSaveManager.activatedDialogZones = new bool[10];
                FPSaveManager.bossRushId = 0;
            }
            else
            {
                RestartLevel();
            }
        }

        public void RestartLevel()
        {
            sceneToLoad = SceneManager.GetActiveScene().name;
            FPAudio.PlayMenuSfx(2);

            FPScreenTransition component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            component.transitionType = FPTransitionTypes.WIPE;
            component.transitionSpeed = 48f;
            component.sceneToLoad = sceneToLoad;
            component.SetTransitionColor(0f, 0f, 0f);
            component.BeginTransition();
            FPAudio.PlayMenuSfx(3);
        }

        public static void Log(string txt)
        {
            MelonLogger.Msg(txt);
        }

        public static void DumpSpriteRendererVars(GameObject go)
        {
            var fppVars = "";
            var sren = go.GetComponent<SpriteRenderer>();


            /*
            fppVars += String.Format(
                "{0}:\r\n" +
                "{1} = {2}\r\n"));
                */

            /*
            fppVars += $"{go.name}: \r\n" +
                       $"{nameof(sren.blah)} : {sren.barTimer}" +
                       $"{nameof(go.inputLock)} : {go.inputLock}" +
                       $"{nameof(go.idleTimer)} : {go.idleTimer}" +
                       $"{nameof(go.targetGimmick)} : {go.targetGimmick}" +
                       $"{nameof(sren.targetWaterSurface)} : {sren.targetWaterSurface}" +
                       $"{nameof(sren.chaseMode)} : {sren.chaseMode}" +
                       $"{nameof(sren.swapCharacter)} : {sren.swapCharacter}" +
                       $"{nameof(sren.hideChildObject)} : {sren.hideChildObject}" +
                       $"{nameof(sren.lastGround)} : {sren.lastGround}" +
                       $"{nameof(sren.lastSafePosition)} : {sren.lastSafePosition}";
                       
                       */

            // UMFGUI.AddConsoleText(allObjects);

            var fileName = "fppVars.txt";
            if (File.Exists(fileName))
            {
                Debug.Log(fileName + " already exists.");
                return;
            }

            var sr = File.CreateText(fileName);
            sr.WriteLine(fppVars);
            sr.Close();
        }

        public static void DumpAllPlayerVarsAndComponents(FPPlayer fpp)
        {
            var fppVars = "";
            var components = fpp.gameObject.GetComponents<Component>();

            fppVars += $"{fpp.name} - {fpp.GetInstanceID()}\n";
            foreach (var component in components)
            {
                fppVars += $"-----({component.name})-----\n";
                var fields = component.GetType().GetFields(BindingFlags.NonPublic |
                                                           BindingFlags.Instance);
                foreach (var field in fields)
                {
                    fppVars += $"{field.Name} = {field.GetValue(component)}\n";
                }

                fppVars += "\n";
            }

            var fileName = "fppVarsAndComponents.txt";
            if (File.Exists(fileName))
            {
                Debug.Log(fileName + " already exists.");
                return;
            }

            var sr = File.AppendText(fileName);
            sr.WriteLine(fppVars);
            sr.Close();
        }

        public static void ToggleSplitScreen()
        {
            EnableSplitScreen.Value = !EnableSplitScreen.Value;
            Log("Toggle Splitscreen... NOT IMPLEMENTED YET.");
        }

        public static void StartSplitscreen()
        {
            foreach (var ssci in SplitScreenCameraInfos)
            {
                ssci.SplitCamRenderTexture.Release();
            }

            SplitScreenCameraInfos.Clear();

            try
            {
                var numPlayers = fpplayers.Count;
                var sortedFPPlayers = fpplayers.OrderBy(fpp => fpp.characterID).ToList();
                EnableSplitScreen.Value = true;
                
                // Memo to go back and dispose these objects since we're creating new ones for player 1 as well...
                // Would also be nice to duplicate the HUD to give everyone their own...
                var goStageCamera = GameObject.Find("Stage Camera"); Log($"{goStageCamera}");
                var goRenderCamera = GameObject.Find("Render Camera"); Log($"{goRenderCamera}");
                var goPixelArtTarget = GameObject.Find("Pixel Art Target");  Log($"{goPixelArtTarget}");// has render cam as child object.
            
                var stageCamera = goStageCamera.GetComponent<FPCamera>(); Log($"{stageCamera}");
                var renderCamera = goRenderCamera.GetComponent<FPCameraFit>(); Log($"{renderCamera}");
                var pixelArtTarget = goPixelArtTarget.GetComponent<MeshRenderer>(); Log($"{pixelArtTarget}");
                
                
                
                for (int p = 0; p < numPlayers; p++) 
                {
                    var cameraRect = SplitScreenCamInfo.GetCamRectByPlayerIndexAndCount(p, numPlayers);
                    Log($"Rect: {cameraRect}");
                    // Short verison for first player.
                    /*
                    if (p == 0)
                    {
                        SplitScreenCameraInfos.Add(new SplitScreenCamInfo(stageCamera, goRenderCamera, stageCamera.renderTarget)); // First cam is pretty much guarenteed.
                        stageCamera.target = fpplayers[p];
                        stageCamera.targetPlayer = fpplayers[p];
                        goRenderCamera.GetComponent<Camera>().rect = new Rect(cameraRect);
                        continue;
                    }
                    */

                    var goSplitScreenPixelArtTarget = GameObject.Instantiate(goPixelArtTarget);  Log($"{goSplitScreenPixelArtTarget}");//shouldn't we be using the FPStage instantiate instead???
                    //var goSplitScreenRenderCamera = goSplitScreenPixelArtTarget.transform.Find("Render Camera (Clone)"); Log($"{goSplitScreenRenderCamera}");
                    //var goSplitScreenRenderCamera = GameObject.Find("Render Camera (Clone)"); Log($"{goSplitScreenRenderCamera}");
                    var goSplitScreenRenderCamera = goSplitScreenPixelArtTarget.transform.GetChild(0); Log($"{goSplitScreenRenderCamera}");
                    /*
                    for (int i = 0; i < goSplitScreenPixelArtTarget.transform.childCount; i++)
                    {
                        Log($"New Pixel Art Target Children: {goSplitScreenPixelArtTarget.transform.GetChild(i).gameObject.ToString()}");
                    }*/

                    var goSplitScreenStageCamera = GameObject.Instantiate(goStageCamera);
                    
                    //var splitScreenRenderCamera = goSplitScreenRenderCamera.GetComponent<FPCameraFit>(); 
                
                    var splitScreenStageCamera = goSplitScreenStageCamera.GetComponent<FPCamera>();

                    SplitScreenCameraInfos.Add(new SplitScreenCamInfo(splitScreenStageCamera, goSplitScreenRenderCamera.gameObject, splitScreenStageCamera.renderTarget));
                    
                    /*
                     *SplitScreenCameraInfos.Add(new SplitScreenCamInfo(stageCamera, goRenderCamera.GetComponent<Camera>()));
                    SplitScreenCameraInfos.Add(new SplitScreenCamInfo(splitScreenStageCamera, goSplitScreenRenderCamera.GetComponent<Camera>()));
                     * 
                     */
                    
                    splitScreenStageCamera.renderTarget = new RenderTexture(stageCamera.renderTarget.width, stageCamera.renderTarget.height, stageCamera.renderTarget.depth, stageCamera.renderTarget.format);
                    // Reminder: RenderTextures are not auto-disposed. I should probably create and cache these at the start for reuse throughout the game rather than creating them on the fly.
                    
                    
                    // Move down to not overlap.
                    goSplitScreenPixelArtTarget.transform.position +=
                        new Vector3(0, goPixelArtTarget.transform.localScale.y * (p + 1), 0); Log($"{goSplitScreenPixelArtTarget}");
                
                    // Set the material on the new render target to be unique and use the new renderTexture we just made.
                    goSplitScreenPixelArtTarget.GetComponent<MeshRenderer>().material.mainTexture =
                        splitScreenStageCamera.renderTarget;
                    
                    // Set the targets to the players
                    if (numPlayers > 1)
                    {
                        // StageCamera has a SetCameraTarget method, but it's static and assumes one camera so we don't use it.
                        splitScreenStageCamera.target = sortedFPPlayers[p];
                        splitScreenStageCamera.targetPlayer = sortedFPPlayers[p];
                        Log($"Set new target to sortedFPPlayers[p] p:{p} fpp: {splitScreenStageCamera.target} : {splitScreenStageCamera.target.name}");
                    }
                    else
                    {
                        splitScreenStageCamera.target = stageCamera.target; Log($"{splitScreenStageCamera}");
                        Log($"Set target to {splitScreenStageCamera.target}, the original player.");
                    }

                    //cameraRect = SplitScreenCamInfo.GetCamRectByPlayerIndexAndCount(p, numPlayers);

                    goSplitScreenRenderCamera.GetComponent<Camera>().rect = new Rect(cameraRect);
                    
                }

                if (numPlayers > 2)
                {
                    FPSaveManager.SetResolution(640 * 2, 360 * 2);
                }
                else if (numPlayers == 2)
                {
                    FPSaveManager.SetResolution(640, 360 * 2);
                }
                else
                {
                    FPSaveManager.SetResolution(640, 360 * 1);
                }

                /*goSplitScreenPixelArtTarget.transform.position +=
                    new Vector3(0, 360, 0); Log($"{goSplitScreenPixelArtTarget}");*/
                
                    //DEBUG
                    //goRenderCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1); Log($"{goRenderCamera.GetComponent<Camera>().rect}");
                    //goSplitScreenRenderCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1); Log($"{goSplitScreenRenderCamera.GetComponent<Camera>().rect}");

                    //END DEBUG
                
                // FPCamera.CreateNewCamera is used to make Lighting cameras, but I don't know when or where it's used so for now it's not factored into this. Fix later.

            }
            catch (Exception e)
            {
                MelonLogger.Error($"{e.ToString()}\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static void UpdateSplitScreens()
        {
            try
            {
                return;
                foreach (var camInfo in SplitScreenCameraInfos)
                {
                    camInfo.RenderCamera = camInfo.GoRenderCamera.GetComponent<Camera>();
                    MelonLogger.Msg($"UpdateRenderCam: {camInfo.RenderCamera}");
                    if (camInfo.FpCamera.lightingCamera != null)
                    {
                        camInfo.FpCamera.lightingCamera.rect = camInfo.RenderCamera.rect;
                        camInfo.FpCamera.lightingCamera.targetTexture = camInfo.SplitCamRenderTexture;
                    }


                    ParallaxLayer pl = null;
                    var highestLayerDepth = -1f;
                    int indexOfHighestLayerCam = -1;
                    bool flag = false;
                    
                    for (int pli = 0; pli < camInfo.FpCamera.parallaxLayers.Length; pli++)
                    {
                        // Set the camera rects to match player rect.
                        pl = camInfo.FpCamera.parallaxLayers[pli];
                        if (pl != null && pl.cam != null)
                        {
                            pl.cam.rect = camInfo.RenderCamera.rect;
                            pl.cam.targetTexture = camInfo.SplitCamRenderTexture; //Causes both views to stop clearing properly...

                            // Imitate CameraStart for handling Lighting and Foreground
                            if (pl.layerMask != StageLayerIDs.LIGHTING)
                            {
                                pl.cam.targetTexture = camInfo.FpCamera.renderTarget;
                                pl.cam.clearFlags = CameraClearFlags.Nothing;
                            }
                            else
                            {
                                pl.cam.targetTexture = camInfo.FpCamera.lightingTarget;
                                pl.cam.clearFlags = CameraClearFlags.Color;
                                pl.cam.backgroundColor = camInfo.FpCamera.shadowTint;
                                camInfo.FpCamera.lightingCamera = pl.cam;
                                flag = true;
                            }
                            if (pl.layerMask == StageLayerIDs.FG_PLANE)
                            {
                                pl.cam.cullingMask = 3856;
                            }
                            
                            // Get layer with highest depth.
                            if ( pl.cam.depth > highestLayerDepth)
                            {
                                highestLayerDepth = pl.cam.depth;
                                indexOfHighestLayerCam = pli;
                            }
                            
                            //UI Cam is affected by Lighting flag?
                            //... except the UI cam isn't accessible.
                            /*
                            if (!flag)
                            {
                                camInfo.FpCamera.uiCam.targetTexture = camInfo.FpCamera.renderTarget;
                                camInfo.FpCamera.uiCam.clearFlags = CameraClearFlags.Nothing;
                            }
                            else
                            {
                                camInfo.FpCamera.uiCam.targetTexture = camInfo.FpCamera.uiTarget;
                                camInfo.FpCamera.uiCam.clearFlags = CameraClearFlags.Color;
                                camInfo.FpCamera.uiCam.backgroundColor = Color.clear;
                            }
                            */
                        }
                        else
                        {
                            Log("funky parallax null");
                        }
                    }
                    pl = camInfo.FpCamera.parallaxLayers[indexOfHighestLayerCam];
                    pl.cam.clearFlags = CameraClearFlags.Color;
                    
                    
                    int num = -1;
                    float num2 = -1f;
                    for (int j = 0; j < camInfo.FpCamera.parallaxLayers.Length; j++)
                    {
                        //var pl = camInfo.FpCamera.parallaxLayers[j];
                        //pl.cam.rect = camInfo.RenderCamera.rect;
                        //pl.cam.targetTexture = camInfo.RenderCamera.targetTexture; //Causes both views to stop clearing properly...
                        
                        if (camInfo.FpCamera.parallaxLayers[j].layerDepth > num2)
                        {
                            num2 = camInfo.FpCamera.parallaxLayers[j].layerDepth;
                            num = j;
                        }
                    }
                    if (num > -1)
                    {
                        camInfo.FpCamera.parallaxLayers[num].cam.clearFlags = CameraClearFlags.Color;
                    }
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"{e}\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static Vector3 GetPositionRelativeToCamera(FPCamera cam, Vector3 pos)
        {
            return pos - cam.transform.position;
        }

        /*
        public void OverwriteDeltaTime(float replacementDeltaTime)
        {
            var props = typeof(Time).GetProperties(BindingFlags.NonPublic |
                                                       BindingFlags.Instance);
            PropertyInfo prop = typeof(Time).GetProperty("deltaTime", BindingFlags.NonPublic |
                                                  BindingFlags.Instance);
            Log($"before prop (deltaTime): {prop.ToString()}");
            prop.SetValue(null, replacementDeltaTime);
            Log($"after prop (deltaTime): {prop.ToString()}");
        }
        */
    }
}