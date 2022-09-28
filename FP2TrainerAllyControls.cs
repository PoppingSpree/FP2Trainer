using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fp2Trainer
{
    public static class FP2TrainerAllyControls
    {
        public static FPPlayer leadPlayer;
        public static List<FPPlayer> allPlayers;
        public static AllyControlType preferredAllyControlType = AllyControlType.SINGLE_PLAYER;

        public static float playerFollowMinimumDistanceHorizontal = 32f;
        public static float playerFollowMinimumDistanceVertical = 16f;
        
        public static float targetObjHunterMinAttackDistanceHorizontal = 64f;
        

        public static void GetUpdatedPlayerList()
        {
            allPlayers = Fp2Trainer.fpplayers;
            leadPlayer = Fp2Trainer.fpplayer;
        }

        public static void HandleAllyControlsFollow(this FPPlayer fpp)
        {
            GetUpdatedPlayerList();

            if (fpp != leadPlayer)
            {
                FollowLeadPlayerHorizontal(fpp, leadPlayer);
                FollowLeadPlayerVertical(fpp, leadPlayer);   
            }
            else
            {
                Fp2Trainer.Log("A character is attempting to follow itself as lead. Control types may be misassigned.");
            }

        }
        
        public static void HandleAllyControlsHunter(this FPPlayer fpp)
        {
	        GetUpdatedPlayerList();
	        
	        if (fpp != leadPlayer)
	        {
		        FPBaseObject targetObj = FPStage.FindNearestEnemy(fpp, 360f, string.Empty);
		        if (targetObj == null)
		        {
			        FPStage.FindNearestPlayer(fpp, 360f);
		        }

		        if (targetObj != null)
		        {
			        FollowTargetObjectHorizontal(fpp, targetObj);
			        FollowTargetObjectVertical(fpp, targetObj);

			        if (Vector2.Distance(fpp.position, targetObj.position) <= targetObjHunterMinAttackDistanceHorizontal)
			        {
				        if (fpp.characterID == FPCharacterID.MILLA)
				        {
					        if (!fpp.cubeSpawned)
					        {
						        MashGuard(fpp);
					        }
					        else
					        {
						        ReleaseGuard(fpp);
					        }
				        }

				        if (fpp.energy >= 80 || fpp.canWarp || fpp.millaCubeEnergy > 0)
				        {
					        Fp2Trainer.Log(String.Format("Character position ({0})\nTarget Position ({1})", fpp.position.y, targetObj.position.y));
					        if (EnemyIsAbove(fpp, targetObj))
					        {
						        HoldUp(fpp);
					        }
					        else
					        {
						        ReleaseUp(fpp);
						        
						        if (fpp.characterID == FPCharacterID.NEERA)
						        {
							        int wantsToUseDownSpec = Random.Range(0, 2); //Max is exclusive.
							        if (wantsToUseDownSpec > 0)
							        {
								        HoldDown(fpp);
							        }
							        else
							        {
								        ReleaseDown(fpp);
							        }
						        }
						        else if (fpp.characterID == FPCharacterID.MILLA)
						        {
							        if (EnemyIsLowAndNear(fpp, targetObj))
							        {
								        HoldDown(fpp);
							        }
							        else
							        {
								        ReleaseDown(fpp);
							        }
						        }
					        }

					        MashSpecial(fpp);
				        }
				        else
				        {
					        ReleaseSpecial(fpp);
				        }

				        MashAttack(fpp);
			        }
			        else
			        {
				        ReleaseAttack(fpp);
			        }
		        }
	        }
	        else
	        {
		        Fp2Trainer.Log("A character is attempting to follow itself as lead. Control types may be misassigned.");
	        }

        }

        public static bool EnemyIsAbove(FPPlayer fpp, FPBaseObject targetObj)
        {
	        return (targetObj.position.y - fpp.position.y) > 32f;
        }
        
        public static bool EnemyIsLowAndNear(FPPlayer fpp, FPBaseObject targetObj)
        {
	        float dist = (fpp.position.y - targetObj.position.y);
	        return  (dist > 32f && dist < 96f );
        }

        private static void FollowLeadPlayerVertical(FPPlayer fpp, FPPlayer leadPlayer)
        {
            //throw new System.NotImplementedException();
            if (!leadPlayer.onGround && !leadPlayer.onGrindRail
                && Vector2.Distance(fpp.position, leadPlayer.position) > playerFollowMinimumDistanceVertical)
            {
                // Assume the player is jumping.
                Jump(fpp);
				
            }
        }

        private static void FollowLeadPlayerHorizontal(FPPlayer fpp, FPPlayer leadPlayer)
        {
	        if (Vector2.Distance(leadPlayer.transform.position,
		            fpp.transform.position) > playerFollowMinimumDistanceHorizontal)
	        {
		        FollowTargetObjectHorizontal(fpp, leadPlayer);
	        }
        }
        
        private static void FollowTargetObjectVertical(FPPlayer fpp, FPBaseObject targetObj)
        {
	        //throw new System.NotImplementedException();
	        if (!targetObj.onGround 
	            && Vector2.Distance(fpp.position, targetObj.position) > playerFollowMinimumDistanceVertical)
	        {
		        // Assume the player is jumping.
				Jump(fpp);
	        }
        }

        private static void FollowTargetObjectHorizontal(FPPlayer fpp, FPBaseObject targetObj)
        {
	        if (Vector2.Distance(targetObj.transform.position,
		            fpp.transform.position) > playerFollowMinimumDistanceHorizontal)
	        {
		        if (targetObj.position.x > fpp.position.x)
		        {
			        MoveRight(fpp);
		        }
		        else if (targetObj.position.x < fpp.position.x)
		        {
			        MoveLeft(fpp);
		        }
		        else
		        {
			        MoveLRRelease(fpp);
		        }
	        }
	        else
	        {
		        MoveLRRelease(fpp);
	        }
        }
        
        private static void PressThenHold(ref bool inPress, ref bool inHold)
        {
	        // If you were already pressing this, turn it into a hold.
	        // If you were holding this on a DPad, it would be impossible to press again while holding without releasing.
	        if (inPress)
	        {
		        inPress = false;
		        inHold = true;
	        }
	        else
	        {
		        // Otherwise, just start pressing.
		        inPress = true;
		        inHold = true;
	        }
        }

        private static void MoveRight(FPPlayer fpp)
        {
	        fpp.input.left = false;
	        fpp.input.leftPress = false;
	        PressThenHold(ref fpp.input.rightPress, ref fpp.input.right);
        }
        
        private static void MoveLeft(FPPlayer fpp)
        {
	        fpp.input.right = false;
	        fpp.input.rightPress = false;
	        PressThenHold(ref fpp.input.leftPress, ref fpp.input.left);
        }

        public static void MoveLRRelease(FPPlayer fpp)
        {
	        fpp.input.right = false;
	        fpp.input.left = false;
	        fpp.input.rightPress = false;
	        fpp.input.leftPress = false;

        }

        private static void HoldDown(FPPlayer fpp)
        {
	        fpp.input.up = false;
	        fpp.input.upPress = false;
	        PressThenHold(ref fpp.input.downPress, ref fpp.input.down);
        }
        private static void ReleaseDown(FPPlayer fpp)
        {
	        fpp.input.down = false;
	        fpp.input.downPress = false;
        }
        
        private static void HoldUp(FPPlayer fpp)
        {
	        fpp.input.down = false;
	        fpp.input.downPress = false;
	        PressThenHold(ref fpp.input.upPress, ref fpp.input.up);
        }
        private static void ReleaseUp(FPPlayer fpp)
        {
	        fpp.input.up = false;
	        fpp.input.upPress = false;
        }

        private static void Jump(FPPlayer fpp)
        {
	        if ((fpp.onGround || fpp.onGrindRail) && fpp.input.jumpHold)
	        {
		        fpp.input.jumpPress = false;
		        fpp.input.jumpHold = false;
	        }
	        else if ((!fpp.onGround && !fpp.onGrindRail) && !fpp.jumpAbilityFlag && fpp.velocity.y > 1f)
	        {
		        fpp.input.jumpPress = false;
		        fpp.input.jumpHold = false;
	        }

	        PressThenHold(ref fpp.input.leftPress, ref fpp.input.left);
	        
	        // uhhh reminder to add some kind of timed input queue where you process things a bit later.
        }

        private static void JumpRelease(FPPlayer fpp)
        {
	        fpp.input.jumpPress = false;
	        fpp.input.jumpHold = false;
        }

        public static void MashAttack(FPPlayer fpp)
        {
	        fpp.input.attackPress = !fpp.input.attackPress;
	        fpp.input.attackHold = fpp.input.attackPress;
        }
        
        public static void ReleaseAttack(FPPlayer fpp)
        {
	        fpp.input.attackPress = false;
	        fpp.input.attackHold = false;
        }
        
        public static void MashSpecial(FPPlayer fpp)
        {
	        fpp.input.specialPress = !fpp.input.specialPress;
	        fpp.input.specialHold = fpp.input.specialPress;
        }

        public static void ReleaseSpecial(FPPlayer fpp)
        {
	        fpp.input.specialPress = false;
	        fpp.input.specialHold = false;
        }
        
        public static void MashGuard(FPPlayer fpp)
        {
	        fpp.input.guardPress = !fpp.input.guardPress;
	        fpp.input.guardHold = fpp.input.guardPress;
        }
        public static void ReleaseGuard(FPPlayer fpp)
        {
	        fpp.input.guardPress = false;
	        fpp.input.guardHold = false;
        }

        public static void CyclePreferredAllyControlType()
        {
            preferredAllyControlType++;
            if (preferredAllyControlType > AllyControlType.NETWORK_MULTIPLAYER)
            {
                preferredAllyControlType = AllyControlType.SINGLE_PLAYER;
            }

            string allyControlTypeName = AllyControlTypeName(preferredAllyControlType);
            Fp2Trainer.Log("Preferred Ally Control Type Set To: " + allyControlTypeName);
            
            UpdateFPPlayersControlTypes();
        }

        public static string AllyControlTypeName(AllyControlType act)
        {
	        return Enum.GetName(typeof(AllyControlType), act);
        }
        
        public static FPObjectState GetInputMethodFromPreferredAllyControlType(FPPlayer fpp)
        {
	        FPObjectState result = fpp.GetInputFromPlayer1;
	        string actually = "";
	        switch (preferredAllyControlType)
	        {
		        case AllyControlType.NPC_FOLLOW:
			        result = fpp.HandleAllyControlsFollow;
			        actually = "Follow";
			        break;
		        case AllyControlType.NPC_HUNTER:
			        result = fpp.HandleAllyControlsHunter;
			        actually = "Hunter";
			        break;
		        default:
			        result = fpp.GetInputFromPlayer1;
			        actually = "Player1";
			        break;
	        }
	        Fp2Trainer.Log("Set control type to " + AllyControlTypeName(preferredAllyControlType));
	        Fp2Trainer.Log("(But actually, it's set to" + nameof(result) + ")");
	        return result;
        }

        public static void UpdateFPPlayersControlTypes()
        {
            FPPlayer fppi = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            foreach (var fpp in Fp2Trainer.fpplayers)
            {
                if (fpp != fppi)
                {
                    fpp.inputMethod = GetInputMethodFromPreferredAllyControlType(fpp);
                }
                else
                {
                    fppi.inputMethod = fppi.GetInputFromPlayer1; //Only normal controls.
                }
            }
        }
        
	    private static void ProcessInputControl(FPPlayer fpp)
		{
			float axis = InputControl.GetAxis(Controls.axes.horizontal);
			float axis2 = InputControl.GetAxis(Controls.axes.vertical);
			fpp.input.upPress = false;
			fpp.input.downPress = false;
			fpp.input.leftPress = false;
			fpp.input.rightPress = false;
			if (fpp.IsPowerupActive(FPPowerup.MIRROR_LENS))
			{
				if (axis > InputControl.joystickThreshold)
				{
					if (!fpp.input.left)
					{
						fpp.input.leftPress = true;
					}
					fpp.input.left = true;
				}
				else
				{
					fpp.input.left = false;
				}
				if (axis < 0f - InputControl.joystickThreshold)
				{
					if (!fpp.input.right)
					{
						fpp.input.rightPress = true;
					}
					fpp.input.right = true;
				}
				else
				{
					fpp.input.right = false;
				}
			}
			else
			{
				if (axis > InputControl.joystickThreshold)
				{
					if (!fpp.input.right)
					{
						fpp.input.rightPress = true;
					}
					fpp.input.right = true;
				}
				else
				{
					fpp.input.right = false;
				}
				if (axis < 0f - InputControl.joystickThreshold)
				{
					if (!fpp.input.left)
					{
						fpp.input.leftPress = true;
					}
					fpp.input.left = true;
				}
				else
				{
					fpp.input.left = false;
				}
			}
			if (axis2 > InputControl.joystickThreshold)
			{
				if (!fpp.input.up)
				{
					fpp.input.upPress = true;
				}
				fpp.input.up = true;
			}
			else
			{
				fpp.input.up = false;
			}
			if (axis2 < 0f - InputControl.joystickThreshold)
			{
				if (!fpp.input.down)
				{
					fpp.input.downPress = true;
				}
				fpp.input.down = true;
			}
			else
			{
				fpp.input.down = false;
			}
			fpp.input.jumpPress = InputControl.GetButtonDown(Controls.buttons.jump);
			fpp.input.jumpHold = InputControl.GetButton(Controls.buttons.jump);
			fpp.input.attackPress = InputControl.GetButtonDown(Controls.buttons.attack);
			fpp.input.attackHold = InputControl.GetButton(Controls.buttons.attack);
			fpp.input.specialPress = InputControl.GetButtonDown(Controls.buttons.special);
			fpp.input.specialHold = InputControl.GetButton(Controls.buttons.special);
			fpp.input.guardPress = InputControl.GetButtonDown(Controls.buttons.guard);
			fpp.input.guardHold = InputControl.GetButton(Controls.buttons.guard);
			fpp.input.confirm = (fpp.input.jumpPress | InputControl.GetButtonDown(Controls.buttons.pause));
			fpp.input.cancel = (fpp.input.attackPress | Input.GetKey(KeyCode.Escape));
		}
    }
}