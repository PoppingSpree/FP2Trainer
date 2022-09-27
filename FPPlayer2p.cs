using System;
using UnityEngine;

namespace Fp2Trainer
{
    public class FPPlayer2p : FPPlayer
    {

        public int maxCharacterID = 4;
        int extraPlayerCount = 1;
        protected new void Start()
        {
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

        public FPPlayer SpawnExtraCharacter()
        {
            FPPlayer newPlayer = new FPPlayer();
            extraPlayerCount++;
            extraPlayerCount %= 5;
            return newPlayer;
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