using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
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
        public const string Version = "0.5.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class Fp2Trainer : MelonMod
    {
        public enum DataPage
        {
            MOVEMENT,
            COMBAT,
            DPS,
            DPS_ALL,
            BATTLESPHERE,
            BOSS,
            NO_CLIP,
            NONE
        }

        public static MelonPreferences_Category fp2Trainer;
        public static MelonPreferences_Entry<bool> enableWarps;
        public static MelonPreferences_Entry<bool> showDebug;
        public static MelonPreferences_Entry<bool> showLevelEditDebug;
        public static MelonPreferences_Entry<bool> enableNoClip;
        
        public static MelonPreferences_Entry<string> BootupLevel;

        public static MelonPreferences_Entry<string> inputLETileCopy;
        public static MelonPreferences_Entry<string> inputLETilePaste;
        public static MelonPreferences_Entry<string> inputLETileLayer;

        public static Fp2Trainer fp2TrainerInstance;

        private static float fp2tDeltaTime;
        //private InputHandler inputHandler = null;

        public static bool introSkipped;

        public static Font fpMenuFont;
        public static Material fpMenuMaterial;

        public Dictionary<int, float> allActiveEnemiesHealth;
        public Dictionary<int, float> allActiveEnemiesHealthPrevious;
        public Dictionary<int, string> allActiveEnemiesNames;
        private List<FPBossHud> bossHuds;

        public bool noClip;
        public float noClipMoveSpeed = 30f;
        public Vector2 noClipStartPos = Vector2.zero;
        public int noClipCollisionLayer = -999;
        public float noClipGravityStrength = -0.7f;

        private readonly GameObject crosshair = null;

        private DataPage currentDataPage = DataPage.MOVEMENT;

        private string debugDisplay = "Never Updated";

        public float dps;
        public List<float> dpsHits;
        public double dpsTimer;

        public FP2TrainerDPSTracker dpsTracker;

        public Dictionary<int, string> fpElementTypeNames;
        private List<FPBaseEnemy> fpEnemies;

        private FPPlayer fpplayer;
        private List<FPPlayer> fpplayers;

        private FPTrainerLevelSelect fptls;

        private GameObject goDtTracker;

        public GameObject goFancyTextPosition;
        public GameObject goStageHUD;
        private readonly float howLongToShowWarpInfo = 2f;

        public List<AssetBundle> loadedAssetBundles;

        private FPBaseEnemy nearestEnemy;
        private FPBaseEnemy nearestEnemyPrevious;
        private float nearestEnemyPreviousHP;
        private FPPauseMenu pauseMenu;

        private GameObject player;

        private HashSet<string> playerValuesToShow;
        private List<FPHudDigit> positionDigits;

        public string sceneToLoad = "";


        // Tilemap tm = null;
        //private Tilemap[] tms = null;
        //private TileBase copyTile = null;
        private int selectedTileLayer;
        public bool showVarString = true;
        private GameObject stageHUD;
        private GameObject stageSelectMenu;
        public TextMesh textmeshFancyTextPosition;

        private float timeoutShowWarpInfo;

        private bool warped;
        private string warpMessage = "";

        private Vector2 warpPoint = new Vector2(211f, 50f);


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
            introSkipped = false;
        }

        private void InitPrefs()
        {
            fp2Trainer = MelonPreferences.CreateCategory("fp2Trainer");
            enableWarps = fp2Trainer.CreateEntry("enableWarps", true);
            BootupLevel = fp2Trainer.CreateEntry("bootupLevel", "ZaoLand");
            showDebug = fp2Trainer.CreateEntry("showDebug", true);
            enableNoClip = fp2Trainer.CreateEntry("enableNoClip", false);
            showLevelEditDebug = fp2Trainer.CreateEntry("showLevelEditDebug", true);
            inputLETileCopy = fp2Trainer.CreateEntry("inputLETileCopy", "<Gamepad>/buttonNorth");
            inputLETilePaste = fp2Trainer.CreateEntry("inputLETilePaste", "<Gamepad>/buttonEast");
            inputLETileLayer = fp2Trainer.CreateEntry("inputLETileLayer", "<Gamepad>/leftShoulder");
        }

        public override void
            OnSceneWasLoaded(int buildindex,
                string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            MelonLogger.Msg("OnSceneWasLoaded: " + buildindex + " | " + sceneName);
            ResetSceneSpecificVariables();
            AttemptToFindFPFont();
            AttemptToFindPauseMenu();
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
            MelonLogger.Msg("OnSceneWasInitialized: " + buildindex + " | " + sceneName);
            SkipBootIntros();
        }

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
            MelonLogger.Msg("OnApplicationQuit");
            MelonPreferences.Save();
        }

        public void OnGameObjectUpdate()
        {
            SkipBootIntros();
            if (dpsTracker != null) dpsTracker.Update();

            if (timeoutShowWarpInfo > 0) timeoutShowWarpInfo -= FPStage.frameTime;
            if (timeoutShowWarpInfo < 0) timeoutShowWarpInfo = 0;
            try
            {
                if (player == null)
                {
                    player = GetFirstPlayerGameObject();
                    fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();

                    fpplayers = GetFPPlayers();
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

                if (fpplayer != null)
                {
                    UpdateDPS();
                    HandleWarpControls();

                    if (timeoutShowWarpInfo > 0) debugDisplay += warpMessage + "\n";

                    if (fptls != null)
                    {
                        var snp = fptls.availableScenes[fptls.menuSelection];
                        debugDisplay += "Warp to: " + fptls.menuSelection + " | " + snp.name + "\n";
                        debugDisplay += "Level Select Parent Pos: " + stageSelectMenu.transform.position + "\n";
                        var tempGoButton = stageSelectMenu.transform.Find("AnnStagePlayIcon").gameObject;
                        debugDisplay += "Level Select Button Pos: " + tempGoButton.transform.position + "\n";
                        debugDisplay += "Level Select Button LocalPos: " + tempGoButton.transform.localPosition + "\n";
                    }

                    
                    if (currentDataPage == DataPage.NO_CLIP)
                    {
                        debugDisplay += "NoClip Enabled: " + noClip.ToString() + "\n";
                        debugDisplay += "Position: " + fpplayer.position.ToString() + "\n";
                        debugDisplay += "Terrain Collision: " + fpplayer.terrainCollision.ToString() + "\n";
                        debugDisplay += "Physics Enabled: " + fpplayer.enablePhysics.ToString() + "\n";
                        debugDisplay += "Collision Layer: " + fpplayer.collisionLayer.ToString() + "\n";
                    }
                    

                    if (noClip)
                    {
                        debugDisplay += "NoClip: " + fpplayer.position.ToString() + "\n";
                    }

                    if (currentDataPage == DataPage.MOVEMENT)
                    {
                        debugDisplay += "Movement: \n";
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
                    else if (currentDataPage == DataPage.BATTLESPHERE)
                    {
                        debugDisplay += "Battlesphere: \n";
                        var tempDmgType = -1;
                        foreach (var mp_fpplayer in fpplayers)
                        {
                            debugDisplay += mp_fpplayer.name + " Health: " + mp_fpplayer.health
                                            + " / " + mp_fpplayer.healthMax + "\n";
                            debugDisplay += mp_fpplayer.name + " Energy: " + mp_fpplayer.energy + "\n";

                            tempDmgType = mp_fpplayer.damageType;
                            if (tempDmgType > 4) tempDmgType = -1;
                            debugDisplay += mp_fpplayer.name + " Last Hurt Element: " +
                                            fpElementTypeNames[tempDmgType] + "\n";
                            //debugDisplay += mp_fpplayer.name + " Energy Recover: " + mp_fpplayer.energyRecoverRate.ToString() + "\n";
                            //debugDisplay += mp_fpplayer.name + " Energy Recover Current: " + mp_fpplayer.energyRecoverRateCurrent.ToString() + "\n";
                            debugDisplay += mp_fpplayer.name + " Attack Power: " + mp_fpplayer.attackPower + "\n";
                            debugDisplay += mp_fpplayer.name + " Attack Hitstun: " + mp_fpplayer.attackHitstun + "\n";
                            debugDisplay += mp_fpplayer.name + " Attack Knockback: " + mp_fpplayer.attackKnockback +
                                            "\n";
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


                if (goFancyTextPosition != null)
                    UpdateFancyText();
                else
                    CreateFancyTextObjects();
            }

            catch (Exception e)
            {
                Log("Trainer Error During Update: " + e.Message + "(" + e.InnerException?.Message + ") @@" +
                    e.StackTrace);
            }
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

        private List<FPPlayer> GetFPPlayers()
        {
            if (FPStage.player != null && FPStage.player.Length > 0) return new List<FPPlayer>(FPStage.player);

            return null;
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

        public void HandleWarpControls()
        {
            //if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.attack))
            if (Input.GetKeyUp(KeyCode.F9))
            {
                Log("F9 -> Load Debug Room");
                SceneManager.LoadScene("StageDebugMenu", LoadSceneMode.Additive);
            }

            if (Input.GetKeyUp(KeyCode.F8))
            {
                Log("F8 -> Main Menu");
                //UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                GoToMainMenuNoLogos();
            }

            if (Input.GetKeyUp(KeyCode.F7))
            {
                Log("F7 -> Load Asset Bundles");
                LoadAssetBundlesFromModsFolder();
            }

            if (Input.GetKeyUp(KeyCode.F6))
            {
                Log("F6 -> Level Select");
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

            if (Input.GetKeyUp(KeyCode.F5))
            {
                Log("F5 -> Toggle Level Select Menu Visibility");
                ToggleLevelSelectVisibility();
            }

            if (Input.GetKeyUp(KeyCode.F4))
            {
                ToggleVariableDisplay();
                Log("F4 -> Toggle DataPage (" + Enum.GetName(typeof(DataPage), currentDataPage) + ")");
            }

            if (Input.GetKeyUp(KeyCode.F3))
            {
                Log("F3 -> Load last located scene: ");
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

            if (Input.GetKeyUp(KeyCode.F2))
            {
                Log("F2 -> NoClip Toggle");
                ToggleNoClip();
            }

            if (Input.GetKeyUp(KeyCode.F1))
            {
                //TestDamageNumberPopups();

                if (fpplayer != null)
                {
                    InstaKOPlayer();
                }
            }

            if (Input.GetKeyUp(KeyCode.KeypadPlus)
                || Input.GetKeyUp(KeyCode.Plus))
            {
                Log("Numpad Plus -> Increase Font Size: ");
                if (textmeshFancyTextPosition != null) textmeshFancyTextPosition.characterSize++;
            }

            if (Input.GetKeyUp(KeyCode.KeypadMinus)
                || Input.GetKeyUp(KeyCode.Minus))
            {
                Log("Numpad Minus -> Decrease Font Size: ");
                if (textmeshFancyTextPosition != null) textmeshFancyTextPosition.characterSize--;
            }


            if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.special))
            {
                fpplayer.position = new Vector2(warpPoint.x, warpPoint.y);
                Log("Hold Guard + Tap Special -> Goto Warp: " + warpPoint);
                warpMessage = "Warping to " + warpPoint;
                timeoutShowWarpInfo = howLongToShowWarpInfo;
            }

            if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.jump))
            {
                warpPoint = new Vector2(fpplayer.position.x, fpplayer.position.y);
                Log("Hold Guard + Tap Jump -> Set Warp: " + warpPoint);
                warpMessage = "Set warp at " + warpPoint;
                timeoutShowWarpInfo = howLongToShowWarpInfo;
            }
            
            if (InputControl.GetButton(Controls.buttons.pause) && InputControl.GetButtonDown(Controls.buttons.special))
            {
                ToggleNoClip();
            }
            
            if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.special))
            {
                ToggleNoClip();
            }

            HandleNoClip();
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
                    Log("up");
                    fpplayer.position.y += modifiedNoClipMoveSpeed * 1;
                }

                if (fpplayer.input.down
                    || InputControl.GetAxis(Controls.axes.vertical) < -0.2f)
                {
                    Log("down");
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

            // After incrementing.
            if (currentDataPage == DataPage.NONE)
                showVarString = false;
            else
                showVarString = true;

            if (currentDataPage == DataPage.BOSS) ReacquireBossHuds();
        }

        public void ReacquireBossHuds()
        {
            bossHuds = new List<FPBossHud>(Object.FindObjectsOfType<FPBossHud>());
            fpEnemies = new List<FPBaseEnemy>();
            foreach (var fpbh in bossHuds)
                if (fpbh != null && fpbh.targetBoss != null)
                    fpEnemies.Add(fpbh.targetBoss);
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

        public override void OnGUI() // Can run multiple times per frame. Mostly used for Unity's IMGUI.
        {
            /*
            if (showDebug.Value)
            {
                Rect r = new Rect(10, 110, 500, 200);
                GUI.Box(r, debugDisplay);
            }
            */

            /*
            if (timeoutShowWarpInfo > 0)
            {
                Rect r = new Rect(10, 10, 200, 32);
                GUI.Box(r, warpMessage);
            }
            */

            DrawLevelEditingInfo();
        }

        private void WriteSceneObjectsToFile()
        {
            if (!warped)
            {
                warped = true;

                var allObjects = "";
                Object[] objs = Object.FindObjectsOfType<GameObject>();

                foreach (var obj in objs) allObjects += obj.name + "\r\n";
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
                if (GetKeyDown(inputLETileCopy.Value)) selectedTileLayer += 1;
                if (GetKeyDown(inputLETilePaste.Value)) selectedTileLayer -= 1;
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

            if (!introSkipped)
            {
                string level = BootupLevel.Value;
                Log("BootupLevel: " + BootupLevel.Value);
                if (level != null && !level.Equals(""))
                {
                    GoToMainMenuNoLogos();
                }
                else
                {
                    GoToCustomBootLevel(level);
                }
            }

                
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

                introSkipped = true;
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

        public static void Log(string txt)
        {
            MelonLogger.Msg(txt);
        }
    }
}