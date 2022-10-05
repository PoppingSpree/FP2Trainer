using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fp2Trainer
{
    
    public enum AllyControlType
    {
        SINGLE_PLAYER,
        NPC_FOLLOW,
        NPC_HUNTER,
        NPC_GHOST,
        LOCAL_MULTIPLAYER,
        NETWORK_MULTIPLAYER
    }

    public class FPPlayer2p : FPPlayer
    {
        public int maxCharacterID = 4;
        public static int extraPlayerCount = 1;
        public static int currentActivePlayerInstance = 0;
        public static Vector2 spawnOffset = new Vector2(-64, 0);
        public static Vector2 catchupOffset = new Vector2(32, 128);

        public static float catchupDistance = 512f+128f;

        public static Dictionary<string, KeyMapping> customControls;
        
        public static List<FPPlayer> instaswapCharacterInstances = new List<FPPlayer>();

        public static bool ignoreCatchup = false;


        protected new void Start()
        {
            InitCustomControls();

            base.Start();
            this.name = "Player " + extraPlayerCount.ToString();
            Fp2Trainer.Log("Added " + this.name);
            /*
            //rewiredPlayerInput = ReInput.players.GetPlayer(0);
            state = State_Init;
            attackStats = AttackStats_Idle;
            checkCurrentAnimation = false;
            currentAnimationIgnored = false;
            jumpReleaseFlag = false;
            jumpAbilityFlag = false;
            energy = 100f;
            energyRecoverRateCurrent = energyRecoverRate;
            energyRecoverRateMultiplier = 1f;
            energyRecoverPenaltyScore = 0f;
            attackBoostMax = jumpStrength / 2f;
            if (childSprite != null)
            {
                childSprite = UnityEngine.Object.Instantiate(childSprite);
                childSprite.parentObject = this;
                childRender = childSprite.GetComponent<Renderer>();
                childAnimator = childSprite.GetComponent<Animator>();
                if (characterID == FPCharacterID.BIKECAROL)
                {
                    childSprite.GetComponent<SpriteRenderer>().enabled = false;
                }
            }

            animator = GetComponent<Animator>();
            render = GetComponent<Renderer>();
            if (FPCamera.stageCamera.target == null)
            {
                FPCamera.SetCameraTarget("Player 1");
            }

            if (characterID == FPCharacterID.CAROL || characterID == FPCharacterID.BIKECAROL)
            {
                audioChannel = new AudioSource[6];
                for (int i = 0; i < 6; i++)
                {
                    GameObject gameObject = new GameObject("PlayerAudioSource");
                    gameObject.transform.parent = base.gameObject.transform;
                    audioChannel[i] = gameObject.AddComponent<AudioSource>();
                    audioChannel[i].volume = FPSaveManager.volumeSfx;
                    audioChannel[i].playOnAwake = false;
                }

                audioChannel[0].volume = FPSaveManager.volumeVoices;
                audioChannel[4].clip = sfxIdle;
                audioChannel[5].clip = sfxMove;
            }
            else
            {
                audioChannel = new AudioSource[4];
                for (int j = 0; j < 4; j++)
                {
                    GameObject gameObject2 = new GameObject("PlayerAudioSource");
                    gameObject2.transform.parent = base.gameObject.transform;
                    audioChannel[j] = gameObject2.AddComponent<AudioSource>();
                    audioChannel[j].volume = FPSaveManager.volumeSfx;
                }

                audioChannel[0].volume = FPSaveManager.volumeVoices;
            }

            wallClingCollisionTestArray = new RaycastHit2D[3];
            if (potions.Length > 0)
            {
                int num = potions.Length;
                if (2 < num)
                {
                    extraLifeCost -= potions[2] * 12;
                    if (!FPStage.checkpointEnabled)
                    {
                        crystals -= potions[2] * 12;
                    }
                }

                if (6 < num)
                {
                    speedMultiplier += (float)(int)potions[6] * 0.05f;
                }

                if (7 < num)
                {
                    jumpMultiplier += (float)(int)potions[7] * 0.04f;
                }
            }

            if (powerups.Length > 0)
            {
                for (int k = 0; k < powerups.Length; k++)
                {
                    switch (powerups[k])
                    {
                        case FPPowerup.EXTRA_STOCK:
                            if (!IsPowerupActive(FPPowerup.STOCK_DRAIN) && !FPStage.checkpointEnabled)
                            {
                                lives++;
                            }

                            break;
                        case FPPowerup.MINUS_STOCK:
                            if (!IsPowerupActive(FPPowerup.STOCK_DRAIN) && !FPStage.checkpointEnabled)
                            {
                                lives--;
                            }

                            break;
                        case FPPowerup.CHEAPER_STOCKS:
                            extraLifeCost -= 50;
                            if (!FPStage.checkpointEnabled)
                            {
                                crystals -= 50;
                            }

                            break;
                        case FPPowerup.PRICY_STOCKS:
                            extraLifeCost -= 100;
                            if (!FPStage.checkpointEnabled)
                            {
                                crystals -= 100;
                            }

                            break;
                        case FPPowerup.MAX_LIFE_UP:
                            if (!IsPowerupActive(FPPowerup.ONE_HIT_KO))
                            {
                                healthMax += 1f;
                                if (!FPStage.checkpointEnabled)
                                {
                                    health += 1f;
                                }
                            }

                            break;
                        case FPPowerup.MAX_LIFE_DOWN:
                            if (!IsPowerupActive(FPPowerup.ONE_HIT_KO))
                            {
                                healthMax -= 1f;
                                if (!FPStage.checkpointEnabled)
                                {
                                    health -= 1f;
                                }
                            }

                            break;
                        case FPPowerup.STOCK_DRAIN:
                            if (!FPStage.checkpointEnabled)
                            {
                                lives = 0;
                            }

                            break;
                        case FPPowerup.EXPENSIVE_STOCKS:
                            extraLifeCost += 250;
                            if (!FPStage.checkpointEnabled)
                            {
                                crystals += 250;
                            }

                            break;
                        case FPPowerup.ONE_HIT_KO:
                            health = 0f;
                            break;
                        case FPPowerup.SPEED_UP:
                            speedMultiplier += 0.2f;
                            break;
                        case FPPowerup.JUMP_UP:
                            jumpMultiplier += 0.2f;
                            break;
                    }
                }
            }

            if (0 < potions.Length && !FPStage.checkpointEnabled)
            {
                lives += (byte)((int)potions[0] / 4);
                crystals -= (int)Mathf.Floor((float)((potions[0] & 3) * extraLifeCost) * 0.25f);
            }

            logCollisionData = true;
            base.Start();
            classID = FPStage.RegisterObjectType(this, GetType(), 0);
            objectID = classID;
            if (!FPStage.checkpointEnabled)
            {
                FPStage.checkpointPos = position;
            }
            */
        }

        public static void InitCustomControls()
        {
            //Button 5: Saturn Z
            // Button 7: Saturn R
            
            //Some custom gamepad bindings here. Should not be on by default...

            customControls = new Dictionary<string, KeyMapping>();
            /*
            customControls.Add("CharTeamSwap",
                InputControl.setKey("CharTeamSwap", new JoystickInput(JoystickButton.Button8),
                    KeyCode.None, KeyCode.None));
            customControls.Add("CharTeamSpawn",
                InputControl.setKey("CharTeamSpawn", new JoystickInput(JoystickButton.Button6),
                    KeyCode.None, KeyCode.None));
                    */
        }

        public static void ShowPressedButtons()
        {
            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                    global::Fp2Trainer.Fp2Trainer.Log("KeyCode down: " + kcode);
            }
        }

        public new void GetInputFromPlayer1()
        {
            /*
            if (!lockedInput)
            {
                if (FPSaveManager.inputSystem == 0)
                {
                    ProcessInputControl();
                }
                else
                {
                    ProcessRewired();
                }
            }*/
            base.GetInputFromPlayer1();
            GetInputFromPlayer2();
        }

        public void GetInputFromPlayer2()
        {
        }

        public void SwapCharacter(FPCharacterID charID)
        {
            this.characterID = charID;
        }

        public void SwapCharacterFromInt(int charID)
        {
            this.characterID = (FPCharacterID)charID;
        }

        public void SwapCharacter()
        {
            this.characterID++;
            if ((int)this.characterID > this.maxCharacterID)
            {
                this.characterID = FPCharacterID.LILAC;
            }
        }

        public static void SwapBetweenActiveCharacters()
        {
            if (Fp2Trainer.fpplayers != null && Fp2Trainer.fpplayers.Count > 0)
            {
                currentActivePlayerInstance++;
                if (currentActivePlayerInstance >= Fp2Trainer.fpplayers.Count)
                {
                    currentActivePlayerInstance = 0;
                }

                Fp2Trainer.Log("NumPlayers: " + Fp2Trainer.fpplayers.Count.ToString());
                Fp2Trainer.Log("CurrentPlayer: " + currentActivePlayerInstance.ToString());
                
                var fppi = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                fppi.inputMethod = FP2TrainerAllyControls.GetInputMethodFromPreferredAllyControlType(fppi);
                
                SetFPPlayerForFPStageAndTrainer(Fp2Trainer.fpplayers[currentActivePlayerInstance]);

                fppi = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                fppi.inputMethod = fppi.GetInputFromPlayer1;

                FPCamera.SetCameraTarget(fppi.gameObject);
                fppi.Action_PlayerVoiceArrayStart();
                if (Fp2Trainer.goFP2TrainerYourPlayerIndicator != null)
                {
                    Fp2Trainer.goFP2Trainer.transform.position =
                        new Vector3(fppi.transform.position.x,
                            fppi.transform.position.y,
                            Fp2Trainer.goFP2Trainer.transform.position.z);
                }

                var stageHud = GameObject.Find("Stage HUD");
                if (stageHud != null)
                {
                    stageHud.GetComponent<FPHudMaster>().targetPlayer =
                        FPStage.currentStage.GetPlayerInstance_FPPlayer();
                }
            }
            Fp2Trainer.Log("Switching control to " + FPStage.currentStage.GetPlayerInstance_FPPlayer().name);
        }

        private static void SetFPPlayerForFPStageAndTrainer(FPPlayer targetPlayer)
        {
            FPStage.currentStage.SetPlayerInstance(targetPlayer);
            Fp2Trainer.fpplayer = targetPlayer;
        }

        public static void CatchupIfPlayerTooFarAway()
        {
            var fppi = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            if (fppi != null)
            {
                var DestroyThese = new List<FPPlayer>();
                foreach (FPPlayer fpp in Fp2Trainer.fpplayers)
                {
                    if (!ignoreCatchup && Vector2.Distance(fppi.transform.position,
                            fpp.transform.position) >= catchupDistance)
                    {
                        fpp.transform.position =
                            fppi.transform.position
                            + new Vector3(catchupOffset.x, catchupOffset.y, fpp.transform.position.z);
                        fpp.position = fppi.position + catchupOffset;
                        fpp.velocity.x = fppi.velocity.x;
                        fpp.velocity.y = fppi.velocity.y;
                        
                    }

                    // This section should probably be in a dedicated update...
                    if (fpp.state == new FPObjectState(fpp.State_KO) ||
                        fpp.state == new FPObjectState(fpp.State_CrushKO))
                    {
                        if (fpp.genericTimer < 10f && fpp.genericTimer > -1f)
                        {
                            if (Fp2Trainer.fpplayers.Count > 1)
                            {
                                //FPStage.DestroyStageObject(fpp); // Remember not to destroy in a foreach.
                                DestroyThese.Add(fpp);
                                if (fpp == fppi)
                                {
                                    SwapBetweenActiveCharacters();
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < DestroyThese.Count; i++)
                {
                    FPStage.DestroyStageObject(DestroyThese[i]);
                    DestroyThese.RemoveAt(i);
                    Fp2Trainer.fpplayers = Fp2Trainer.GetFPPlayers();
                    FPStage.SetGameSpeed(1f);
                    i--;
                }
            }


            //This last bit is more suited for a separate update method:
            
            /*
            if (InputControl.GetButtonDown(customControls["CharTeamSpawn"]))
            {
                SpawnExtraCharacter();
            }

            if (InputControl.GetButtonDown(customControls["CharTeamSwap"]))
            {
                SwapBetweenActiveCharacters();
                Fp2Trainer.Log("Switching control to " + FPStage.currentStage.GetPlayerInstance_FPPlayer().name);
            }
            */
        }

        public static FPPlayer SpawnExtraCharacter()
        {
            FPPlayer newPlayer = null;
            FPPlayer fppi = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            bool playerObjectValidated = false;

            extraPlayerCount++;
            if (extraPlayerCount % 5 == 2) //Skip Bike Carol.
            {
                extraPlayerCount++;
            }

            int playerNumModulus = extraPlayerCount % 5;

            // Spawn all possible characters before allowing duplicates.
            int baseMaxCharacterCount = 4;
            Fp2Trainer.fpplayers = Fp2Trainer.GetFPPlayers();
            if (Fp2Trainer.fpplayers.Count <= baseMaxCharacterCount)
            {
                List<int> availableCharIDs = new List<int>();
                availableCharIDs.Add(0);
                availableCharIDs.Add(1);
                // Bike Carol is 2. Skip.
                availableCharIDs.Add(3);
                availableCharIDs.Add(4);

                int charID = -1;
                for (int i = 0; i < Fp2Trainer.fpplayers.Count; i++)
                {
                    charID = (int)(Fp2Trainer.fpplayers[i].characterID);
                    if (availableCharIDs.Contains(charID))
                    {
                        availableCharIDs.Remove(charID);
                        Fp2Trainer.Log(String.Format("Removed {0} from available spawns.", charID));
                    }
                }

                if (availableCharIDs.Count > 0)
                {
                    playerNumModulus = availableCharIDs[0];
                }
            }

            newPlayer = FPStage.InstantiateFPBaseObject(FPStage.player[(int)(playerNumModulus)],
                out playerObjectValidated);

            newPlayer.position = fppi.position + (spawnOffset * (playerNumModulus + 1));
            newPlayer.gameObject.transform.position =
                fppi.transform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0);
            //newPlayer.position = FPStage.currentStage.GetPlayerInstance_FPPlayer().position;

            newPlayer.inputMethod =
                newPlayer
                    .GetInputFromPlayer1; // So... you can totally just replace the input method here to control the character with anything we want.
            newPlayer.collisionLayer = fppi.collisionLayer;

            newPlayer.name = String.Format("Player {0}", extraPlayerCount);

            newPlayer.inputMethod = FP2TrainerAllyControls.GetInputMethodFromPreferredAllyControlType(newPlayer);

            newPlayer.oxygenLevel = 1f;
            newPlayer.heatLevel = 0f;
            newPlayer.interactWithObjects = true;

            newPlayer.powerups = fppi.powerups;
            newPlayer.potions = fppi.potions;
            newPlayer.totalCrystals = fppi.crystals;
            newPlayer.hasSpecialItem = fppi.hasSpecialItem;
            //newPlayer.name = playerName;

            Fp2Trainer.fpplayers = Fp2Trainer.GetFPPlayers();
            Fp2Trainer.Log(FPStage.currentStage.GetPlayerInstance_FPPlayer().name + " joins the party!");

            if (FP2TrainerCharacterNameTag.instance != null)
            {
                FP2TrainerCharacterNameTag.instance.InstantiateNewNametag(newPlayer);
            }

            //global::Fp2Trainer.Fp2Trainer.CloneHealthBar(newPlayer);

            //DestroyMergaCutsceneTriggers();

            /*
            if (Fp2Trainer.EnableSplitScreen.Value)
            {
                Fp2Trainer.StartSplitscreen(); // Probably need to include a way to stop this from happening automatically.
            }
            */

            return newPlayer;
        }

        public static FPPlayer SpawnExtraCharacterExperimental1()
        {
            FPPlayer newPlayer = null;
            return newPlayer;
        }

        public static void SpawnExtraCharactersViaSpawnPoint()
        {
            try
            {
                PlayerSpawnPoint spawnPoint = GameObject.FindObjectOfType<PlayerSpawnPoint>();
                if (spawnPoint != null)
                {
                    FPPlayer fppi = FPStage.currentStage.GetPlayerInstance_FPPlayer();

                    bool playerObjectValidated = false;
                    extraPlayerCount++;
                    // DON'T skip bike carol here.
                    int playerNumModulus = extraPlayerCount % 5;

                    // Spawn all possible characters

                    for (int i = 0; i < (int)FPCharacterID.NEERA; i++)
                    {
                        spawnPoint.character = (FPCharacterID)i;
                        Fp2Trainer.Log($"Sending START message to spawn point.");
                        spawnPoint.SendMessage("Start");
                        Fp2Trainer.Log($"SENT START message to spawn point.");
                    }

                    Fp2Trainer.fpplayers = Fp2Trainer.GetFPPlayers();
                    instaswapCharacterInstances.Clear();
                    foreach (var fpp in Fp2Trainer.fpplayers)
                    {
                        instaswapCharacterInstances.Add(fpp);

                        /*
                        if (FP2TrainerCharacterNameTag.instance != null)
                        {
                            FP2TrainerCharacterNameTag.instance.InstantiateNewNametag(fpp);
                        }
                        */

                        if (fpp.characterID == fppi.characterID)
                        {
                            fpp.gameObject.SetActive(true);
                            fpp.activationMode = FPActivationMode.ALWAYS_ACTIVE;
                            fpp.GetComponent<SpriteRenderer>().enabled = true;
                            
                            fpp.position = fppi.position;
                            fpp.collisionLayer = fppi.collisionLayer;
                            FPStage.currentStage.SetPlayerInstance_FPPlayer(fpp);
                            FPCamera.SetCameraTarget(fpp.gameObject);
                            Fp2Trainer.fpplayer = fpp;
                        }
                        else
                        {
                            fpp.gameObject.SetActive(false);
                            fpp.activationMode = FPActivationMode.NEVER_ACTIVE;
                            fpp.GetComponent<SpriteRenderer>().enabled = false;
                        }


                        fppi.gameObject.SetActive(false);
                        fppi.activationMode = FPActivationMode.NEVER_ACTIVE;
                        fppi.GetComponent<SpriteRenderer>().enabled = false;
                        FPStage.DestroyStageObject(fppi);
                        
                        if (Fp2Trainer.EnableSplitScreen.Value)
                        {
                            Fp2Trainer.StartSplitscreen(); // Probably need to include a way to stop this from happening automatically.
                        }
                    }
                }
                else
                {
                    Fp2Trainer.Log("Attempted to spawn additional characters for InstaSwap, but did not find a Spawn Point.");
                }
            }
            catch (Exception e)
            {

                Fp2Trainer.Log(e.Message + e.StackTrace);
            }

        }//SpawnExtraCharactersViaSpawnPoint()

        public static void PerformInstaSwap(FPCharacterID charID)
        {

            try
            {
                if (instaswapCharacterInstances != null)
                {
                    Fp2Trainer.Log($"Instaswap to {charID}");
                    foreach (var fpp in instaswapCharacterInstances)
                    {
                        if (fpp.characterID == charID)
                        {
                            fpp.gameObject.SetActive(true);
                            fpp.activationMode = FPActivationMode.ALWAYS_ACTIVE;
                            fpp.GetComponent<SpriteRenderer>().enabled = true;
                            
                            //fpp.animator = fpp.GetComponent<Animator>();
                            fpp.animator = FPStage.player[(int)charID].GetComponent<Animator>();
                            
                            fpp.position = Fp2Trainer.fpplayer.position;
                            fpp.collisionLayer = Fp2Trainer.fpplayer.collisionLayer;
                            fpp.velocity = Fp2Trainer.fpplayer.velocity;
                            fpp.health = Fp2Trainer.fpplayer.health;
                            fpp.heatLevel = Fp2Trainer.fpplayer.heatLevel;
                            fpp.oxygenLevel = Fp2Trainer.fpplayer.oxygenLevel;
                            fpp.gameObject.transform.position = new Vector3(Fp2Trainer.fpplayer.gameObject.transform.position.x,
                                Fp2Trainer.fpplayer.gameObject.transform.position.y, Fp2Trainer.fpplayer.gameObject.transform.position.z);
                        
                            FPStage.currentStage.SetPlayerInstance_FPPlayer(fpp);
                            FPCamera.SetCameraTarget(fpp.gameObject);
                            Fp2Trainer.fpplayer = fpp;
                            
                            Fp2Trainer.Log($"Instaswap Complete {charID}");

                        }
                        else
                        {
                            fpp.gameObject.SetActive(false);
                            fpp.activationMode = FPActivationMode.NEVER_ACTIVE;
                            fpp.GetComponent<SpriteRenderer>().enabled = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Fp2Trainer.Log("@@@@@@@@@@@@@@@@@@@@@@");
                Fp2Trainer.Log(e.Message + e.StackTrace);
            }
        }

        private static void DestroyMergaCutsceneTriggers()
        {
            var trig = GameObject.Find("Cutscene_Lilac");
            if (trig != null)
            {
                Destroy(trig);
            }

            trig = GameObject.Find("Cutscene_Carol");
            if (trig != null)
            {
                Destroy(trig);
            }

            trig = GameObject.Find("Cutscene_Milla");
            if (trig != null)
            {
                Destroy(trig);
            }

            trig = GameObject.Find("Cutscene_Neera");
            if (trig != null)
            {
                Destroy(trig);
            }
        }

        /*
         * public void Ghost()
		{
			SpriteGhost spriteGhost = (SpriteGhost)FPStage.CreateStageObject(SpriteGhost.classID, position.x, position.y);
			spriteGhost.transform.rotation = base.transform.rotation;
			spriteGhost.SetUp(GetComponent<SpriteRenderer>().sprite, Color.white, new Color(1f, 1f, 1f, 0f), 0.5f, 3f);
			spriteGhost.transform.localScale = base.transform.localScale;
			spriteGhost.maxLifeTime = 0.2f;
			spriteGhost.growSpeed = 0f;
			spriteGhost.activationMode = FPActivationMode.ALWAYS_ACTIVE;
		}
         */
    }
}