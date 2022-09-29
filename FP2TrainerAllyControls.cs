using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fp2Trainer
{
    public static class FP2TrainerAllyControls
    {
        public static FPPlayer leadPlayer;
        public static List<FPPlayer> allPlayers;
        public static Dictionary<FPPlayer, FP2TrainerInputQueue> inputQueueForPlayers;
        public static AllyControlType preferredAllyControlType = AllyControlType.SINGLE_PLAYER;

        public static float playerFollowMinimumDistanceHorizontal = 32f;
        public static float playerFollowMinimumDistanceVertical = 16f;
        
        public static float targetObjHunterMinAttackDistanceHorizontal = 64f;

        public static bool showAllyControlDebugs = false;

        public static void LogDebugOnly(string str)
        {
	        showAllyControlDebugs = Fp2Trainer.multiplayerStart;

	        if (showAllyControlDebugs)
	        {
		        Fp2Trainer.Log(str + "\n");
	        }
        }

        public static FP2TrainerInputQueue RecordInput(FPPlayer fpp)
        {
	        FP2TrainerInputQueue ipq;
	        if (!inputQueueForPlayers.ContainsKey(fpp))
	        {
		        inputQueueForPlayers.Add(fpp, new FP2TrainerInputQueue());
	        }

	        ipq = inputQueueForPlayers[fpp];
	        
	        ipq.Add(new TimestampedInputs(fpp.input.up, fpp.input.down, fpp.input.left, fpp.input.right,
		        fpp.input.jumpHold, fpp.input.attackHold, fpp.input.specialHold, fpp.input.guardHold,
		        false));
	        
	        LogDebugOnly(ipq.ToString());
	        
	        return ipq;
        }

        public static void AddTime(FP2TrainerInputQueue ipq, float deltaTime)
        {
	        ipq.AddTime(deltaTime);
        }

        public static FP2TrainerInputQueue GetInputQueue(FPPlayer fpp)
        {
	        FP2TrainerInputQueue ipq;
	        if (inputQueueForPlayers.ContainsKey(fpp))
	        {
		        ipq = inputQueueForPlayers[fpp];
	        }
	        else
	        {
		        ipq = RecordInput(fpp);
		        inputQueueForPlayers.Add(fpp, ipq);
	        }

	        return ipq;
        }

        public static bool HasFlag(BitwiseInputState theFlags, BitwiseInputState flagCondition)
        {
	        return (theFlags & flagCondition) == flagCondition;
        }
        
        public static void SetBoolIfFlagJustSet(out bool toBeSet, 
	        BitwiseInputState flagPrev, BitwiseInputState flagLatest,
			BitwiseInputState conditionFlags)
        {
	        toBeSet = !HasFlag(flagPrev, conditionFlags) 
	                  && HasFlag(flagLatest, conditionFlags);
        }

        public static void MapPlayerPressesFromPreviousInputs(FPPlayer fpp)
        {
	        if (inputQueueForPlayers.ContainsKey(fpp))
	        {
		        var prevInputs = inputQueueForPlayers[fpp].GetPrevious();
		        var latestInputs = inputQueueForPlayers[fpp].GetLatest();

		        SetBoolIfFlagJustSet(out fpp.input.upPress,
			        prevInputs.bitwiseInputs, 
			        latestInputs.bitwiseInputs,
			        BitwiseInputState.UP);
		        
		        SetBoolIfFlagJustSet(out fpp.input.downPress,
			        prevInputs.bitwiseInputs, 
			        latestInputs.bitwiseInputs,
			        BitwiseInputState.DOWN);
		        
		        SetBoolIfFlagJustSet(out fpp.input.leftPress,
			        prevInputs.bitwiseInputs, 
			        latestInputs.bitwiseInputs,
			        BitwiseInputState.LEFT);
		        
		        SetBoolIfFlagJustSet(out fpp.input.rightPress,
			        prevInputs.bitwiseInputs, 
			        latestInputs.bitwiseInputs,
			        BitwiseInputState.RIGHT);
		        
		        SetBoolIfFlagJustSet(out fpp.input.jumpPress,
			        prevInputs.bitwiseInputs, 
			        latestInputs.bitwiseInputs,
			        BitwiseInputState.JUMP);
		        
		        SetBoolIfFlagJustSet(out fpp.input.attackPress,
			        prevInputs.bitwiseInputs, 
			        latestInputs.bitwiseInputs,
			        BitwiseInputState.ATTACK);
		        
		        SetBoolIfFlagJustSet(out fpp.input.specialPress,
			        prevInputs.bitwiseInputs, 
			        latestInputs.bitwiseInputs,
			        BitwiseInputState.SPECIAL);
		        
		        SetBoolIfFlagJustSet(out fpp.input.guardPress,
			        prevInputs.bitwiseInputs, 
			        latestInputs.bitwiseInputs,
			        BitwiseInputState.GUARD);
		        
		        /*
		        SetBoolIfFlagJustSet(out fpp.input.pause,
			        prevInputs.bitwiseInputs, 
			        latestInputs.bitwiseInputs,
			        BitwiseInputState.PAUSE);
			    */
	        }
        }

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

                fpp.input.attackHold = leadPlayer.input.attackHold;
                fpp.input.attackPress = leadPlayer.input.attackPress;
                
                fpp.input.specialHold = leadPlayer.input.specialHold;
                fpp.input.specialPress = leadPlayer.input.specialPress;
                
                fpp.input.up = leadPlayer.input.up;
                fpp.input.upPress = leadPlayer.input.upPress;
                
                fpp.input.down = leadPlayer.input.down;
                fpp.input.downPress = leadPlayer.input.downPress;
                
                AddTime(GetInputQueue(fpp), Time.deltaTime);
                RecordInput(fpp);
                MapPlayerPressesFromPreviousInputs(fpp);
            }
            else
            {
                LogDebugOnly("A character is attempting to follow itself as lead. Control types may be misassigned.");
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
			        //FPStage.FindNearestPlayer(fpp, 360f); //Doesn't work, it will always return itself.
			        targetObj = leadPlayer;
		        }
		        if (targetObj == null)
		        {
			        targetObj = fpp;
		        }

		        if (targetObj == fpp)
		        {
			        LogDebugOnly("A character is attempting to follow itself. Probably could not find a valid target.");
		        }

		        LogDebugOnly(String.Format("Hunter {2} Target set to {0} - ({1})", 
			        targetObj.name, targetObj.position, fpp.name));

		        if (targetObj != null)
		        {
			        FollowTargetObjectHorizontal(fpp, targetObj);
			        FollowTargetObjectVertical(fpp, targetObj);
			        float distToEnemy = Vector2.Distance(fpp.position, targetObj.position); 
			        if ( distToEnemy <= targetObjHunterMinAttackDistanceHorizontal)
			        {
				        LogDebugOnly(String.Format("Attempting to attack. Dist to Target: {0}.", distToEnemy));
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
					        LogDebugOnly(String.Format("Character position ({0})\nTarget Position ({1})", fpp.position.y, targetObj.position.y));
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
		        
		        AddTime(GetInputQueue(fpp), Time.deltaTime);
		        RecordInput(fpp);
		        MapPlayerPressesFromPreviousInputs(fpp);
	        }
	        else if (fpp == leadPlayer)
	        {
		        LogDebugOnly("A character is attempting to follow itself as lead. Control types may be misassigned.");
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
	        float dist = Vector2.Distance(fpp.position, leadPlayer.position);
            if (!leadPlayer.onGround && !leadPlayer.onGrindRail
                && dist > playerFollowMinimumDistanceVertical)
            {
                // Assume the player is jumping.
                Jump(fpp);
				
            }
            else
            {
	            ReleaseJump(fpp);
            }
        }

        private static void FollowLeadPlayerHorizontal(FPPlayer fpp, FPPlayer leadPlayer)
        {
	        FollowTargetObjectHorizontal(fpp, leadPlayer);
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
	        float dist = Vector2.Distance(targetObj.transform.position,
		        fpp.transform.position);
	        if (dist > playerFollowMinimumDistanceHorizontal)
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
	        inPress = true;
	        inHold = true;
        }
        
        public static void ClearPresses(FPPlayer fpp)
        {
	        // If you were already pressing this, turn it into a hold.
	        // If you were holding this on a DPad, it would be impossible to press again while holding without releasing.
	        
	        fpp.input.upPress = false;
	        fpp.input.downPress = false;
	        fpp.input.leftPress = false;
	        fpp.input.rightPress = false;
	        
	        fpp.input.jumpPress = false;
	        fpp.input.attackPress = false;
	        fpp.input.specialPress = false;
	        fpp.input.guardPress = false;

        }
        
        public static void DetectPresses(FPPlayer fpp)
        {
	        MapPlayerPressesFromPreviousInputs(fpp);
        }

        private static void MoveRight(FPPlayer fpp)
        {
	        fpp.input.left = false;
	        PressThenHold(ref fpp.input.rightPress, ref fpp.input.right);
        }
        
        private static void MoveLeft(FPPlayer fpp)
        {
	        fpp.input.right = false;
	        PressThenHold(ref fpp.input.leftPress, ref fpp.input.left);
        }

        public static void MoveLRRelease(FPPlayer fpp)
        {
	        fpp.input.right = false;
	        fpp.input.left = false;

        }

        private static void HoldDown(FPPlayer fpp)
        {
	        fpp.input.up = false;
	        PressThenHold(ref fpp.input.downPress, ref fpp.input.down);
        }
        private static void ReleaseDown(FPPlayer fpp)
        {
	        fpp.input.down = false;
        }
        
        private static void HoldUp(FPPlayer fpp)
        {
	        fpp.input.down = false;
	        PressThenHold(ref fpp.input.upPress, ref fpp.input.up);
        }
        private static void ReleaseUp(FPPlayer fpp)
        {
	        fpp.input.up = false;
        }

        private static void Jump(FPPlayer fpp)
        {
	        LogDebugOnly("wanna jump");
	        if ((fpp.onGround || fpp.onGrindRail) && fpp.input.jumpHold)
	        {
		        LogDebugOnly("jump - grounded and holding jump");
		        ReleaseJump(fpp);
	        }
	        else if ((fpp.onGround || fpp.onGrindRail) && !fpp.input.jumpHold)
	        {
		        LogDebugOnly("jump - grounded and not holding jump");
		        PressThenHold(ref fpp.input.jumpPress, ref fpp.input.jumpHold);
	        }
	        else if ((!fpp.onGround && !fpp.onGrindRail) && !fpp.jumpAbilityFlag && fpp.velocity.y < -1f)
	        {
		        LogDebugOnly("jump - In air, falling, and can use Air Action");
		        JumpRelease(fpp);
	        }
	        else if ((!fpp.onGround && !fpp.onGrindRail)&& fpp.velocity.y > -1f)
	        {
		        LogDebugOnly("jump - In air, Rising");
	        }
	        else
	        {
		        ReleaseJump(fpp);
	        }
        }

        public static void ReleaseJump(FPPlayer fpp)
        {
	        fpp.input.jumpPress = false;
	        fpp.input.jumpHold = false;
        }

        private static void JumpRelease(FPPlayer fpp)
        {
	        fpp.input.jumpPress = false;
	        fpp.input.jumpHold = false;
        }

        public static void MashAttack(FPPlayer fpp)
        {
	        fpp.input.attackHold = !fpp.input.attackHold;
        }
        
        public static void ReleaseAttack(FPPlayer fpp)
        {
	        fpp.input.attackPress = false;
	        fpp.input.attackHold = false;
        }
        
        public static void MashSpecial(FPPlayer fpp)
        {
	        fpp.input.specialHold = !fpp.input.specialHold;
        }

        public static void ReleaseSpecial(FPPlayer fpp)
        {
	        fpp.input.specialPress = false;
	        fpp.input.specialHold = false;
        }
        
        public static void MashGuard(FPPlayer fpp)
        {
	        fpp.input.guardHold = !fpp.input.guardHold;
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
            Fp2Trainer.PreferredAllyControlTypeLastSetting.Value = allyControlTypeName;
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
    }
}