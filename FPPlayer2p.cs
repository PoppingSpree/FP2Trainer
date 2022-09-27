using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fp2Trainer
{
    public class FPPlayer2p : FPPlayer
    {

        public int maxCharacterID = 4;
        public static int extraPlayerCount = 1;
        public static int currentActivePlayerInstance = 0;
        public static Vector2 spawnOffset = new Vector2(-64, 0);
        public static Vector2 catchupOffset = new Vector2(32, 128);

        public static float catchupDistance = 512f;

        public static Dictionary<string, KeyMapping> customControls;


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
            
            customControls = new Dictionary<string, KeyMapping>();
            customControls.Add("CharTeamSwap", 
                InputControl.setKey("CharTeamSwap", new JoystickInput(JoystickButton.Button8),
                    KeyCode.None, KeyCode.None));
            customControls.Add("CharTeamSpawn", 
                InputControl.setKey("CharTeamSpawn", new JoystickInput(JoystickButton.Button6),
                    KeyCode.None, KeyCode.None));
        }

        public static void ShowPressedButtons()
        {
            foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
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
                Fp2Trainer.Log("NumPlayers: " + Fp2Trainer.fpplayers.Count.ToString());
                Fp2Trainer.Log("CurrentPlayer: " + currentActivePlayerInstance.ToString());
                currentActivePlayerInstance++;
                Fp2Trainer.Log("NumPlayers: " + Fp2Trainer.fpplayers.Count.ToString());
                Fp2Trainer.Log("CurrentPlayer: " + currentActivePlayerInstance.ToString());
                if (currentActivePlayerInstance >= Fp2Trainer.fpplayers.Count)
                {
                    currentActivePlayerInstance = 0;
                }
                Fp2Trainer.Log("NumPlayers: " + Fp2Trainer.fpplayers.Count.ToString());
                Fp2Trainer.Log("CurrentPlayer: " + currentActivePlayerInstance.ToString());
                FPStage.currentStage.SetPlayerInstance(Fp2Trainer.fpplayers[currentActivePlayerInstance]);
                FPCamera.SetCameraTarget(FPStage.currentStage.GetPlayerInstance().gameObject);
                var stageHud = GameObject.Find("Stage HUD");
                if (stageHud != null)
                {
                    stageHud.GetComponent<FPHudMaster>().targetPlayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                }
            }
        }

        public static void CatchupIfPlayerTooFarAway()
        {
            var fppi = FPStage.currentStage.GetPlayerInstance();
            if (fppi != null)
            {
                var DestroyThese = new List<FPPlayer>();
                foreach (FPPlayer fpp in Fp2Trainer.fpplayers)
                 {
                     if (Vector2.Distance(fppi.transform.position, 
                             fpp.transform.position) >= catchupDistance)
                     {
                         fpp.transform.position =
                             fppi.transform.position 
                             + new Vector3(catchupOffset.x, catchupOffset.y, fpp.transform.position.z);
                         fpp.position = fppi.position + catchupOffset;
                     }
                     
                     // This section should probably be in a dedicated update...
                     if (fpp.state == new FPObjectState(fpp.State_KO) || fpp.state == new FPObjectState(fpp.State_CrushKO))
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
            if (InputControl.GetButtonDown(customControls["CharTeamSpawn"]))
            {
                SpawnExtraCharacter();
            }
            if (InputControl.GetButtonDown(customControls["CharTeamSwap"]))
            {
                SwapBetweenActiveCharacters();
                Fp2Trainer.Log("Switching control to " + FPStage.currentStage.GetPlayerInstance().name);
            }
        }

        public static FPPlayer SpawnExtraCharacter()
        {
            FPPlayer newPlayer = null;
            FPPlayer fppi = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            bool playerObjectValidated = false;
            int playerNumModulus = extraPlayerCount % 5;
            newPlayer = FPStage.InstantiateFPBaseObject(FPStage.player[(int)(playerNumModulus)], out playerObjectValidated);
            
            newPlayer.position = fppi.position + (spawnOffset * (playerNumModulus + 1));
            newPlayer.gameObject.transform.position = fppi.transform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0);
            //newPlayer.position = FPStage.currentStage.GetPlayerInstance_FPPlayer().position;
            
            newPlayer.inputMethod = newPlayer.GetInputFromPlayer1; // So... you can totally just replace the input method here to control the character with anything we want.
            newPlayer.collisionLayer = fppi.collisionLayer;

            newPlayer.name = String.Format("Player {0}", extraPlayerCount);

            extraPlayerCount++;
            if (extraPlayerCount % 5 == 2)
            {
                extraPlayerCount++;
            }
            
            Fp2Trainer.fpplayers = Fp2Trainer.GetFPPlayers();
            Fp2Trainer.Log(FPStage.currentStage.GetPlayerInstance().name + " joins the party!");

            //global::Fp2Trainer.Fp2Trainer.CloneHealthBar(newPlayer);

            DestroyMergaCutsceneTriggers();

            return newPlayer;
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