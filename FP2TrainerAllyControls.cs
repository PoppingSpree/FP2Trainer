using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fp2Trainer
{
    public static class FP2TrainerAllyControls
    {
        public static FPPlayer leadPlayer;
        public static List<FPPlayer> allPlayers;
        public static Dictionary<int, FP2TrainerInputQueue> inputQueueForPlayers = new Dictionary<int, FP2TrainerInputQueue>();
        public static AllyControlType preferredAllyControlType = AllyControlType.SINGLE_PLAYER;

        public static float playerFollowMinimumDistanceHorizontal = 32f;
        public static float playerFollowMinimumDistanceVertical = 16f;
        
        public static float targetObjHunterMinAttackDistanceHorizontal = 64f;

        public static bool showAllyControlDebugs = false;
        public static bool needToLoadInputs = false;

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
	        if (!inputQueueForPlayers.ContainsKey(fpp.GetInstanceID()))
	        {
		        
		        inputQueueForPlayers.Add(fpp.GetInstanceID(), new FP2TrainerInputQueue());
	        }

	        
	        ipq = inputQueueForPlayers[fpp.GetInstanceID()];
	        
	        
	        ipq.Add(new TimestampedInputs(fpp.input.up, fpp.input.down, fpp.input.left, fpp.input.right,
		        fpp.input.jumpHold, fpp.input.attackHold, fpp.input.specialHold, fpp.input.guardHold,
		        false));
	        
	        return ipq;
        }

        public static void AddTime(FP2TrainerInputQueue ipq, float deltaTime)
        {
	        ipq.AddTime(deltaTime);
        }

        public static FP2TrainerInputQueue GetInputQueue(FPPlayer fpp)
        {
	        FP2TrainerInputQueue ipq;
	        if (inputQueueForPlayers.ContainsKey(fpp.GetInstanceID()))
	        {
		        
		        ipq = inputQueueForPlayers[fpp.GetInstanceID()];
	        }
	        else
	        {
		        ipq = RecordInput(fpp);
		        inputQueueForPlayers.Add(fpp.GetInstanceID(), ipq);
	        }

	        return ipq;
        }

        public static FP2TrainerInputQueue LoadFileInputQueueForPlayer(FPPlayer fpp)
        {
	        FP2TrainerInputQueue ipq;
	        if (String.IsNullOrEmpty(Fp2Trainer.DEBUG_LoadSpecificGhostFile.Value))
	        {
		        ipq = FP2TrainerInputQueue.LoadQueueFromFileMostRecent();
	        }
	        else
	        {
		        ipq = FP2TrainerInputQueue.LoadQueueFromFile(Fp2Trainer.DEBUG_LoadSpecificGhostFile.Value);
	        }
	        
	        if (inputQueueForPlayers.ContainsKey(fpp.GetInstanceID()))
	        {
		        // Overwrite if present.
		        inputQueueForPlayers[fpp.GetInstanceID()] = ipq;
	        }
	        else
	        {
		        // If it isn't, we need to Add the entry.
		        inputQueueForPlayers.Add(fpp.GetInstanceID(), ipq);
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
	        if (inputQueueForPlayers.ContainsKey(fpp.GetInstanceID()))
	        {
		        var prevInputs = inputQueueForPlayers[fpp.GetInstanceID()].GetPrevious();
		        var latestInputs = inputQueueForPlayers[fpp.GetInstanceID()].GetLatest();

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
	        needToLoadInputs = false;
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
                
                try
                {
	                AddTime(GetInputQueue(fpp), FPStage.deltaTime);
	                //RecordInput(fpp);
	                MapPlayerPressesFromPreviousInputs(fpp);
                }
                catch (Exception e)
                {
	                LogDebugOnly(e.Message);
	                LogDebugOnly(e.ToString());
	                LogDebugOnly(e.StackTrace);
                }
                
            }
            else
            {
                LogDebugOnly("A character is attempting to follow itself as lead. Control types may be misassigned.");
            }

        }
        
        public static void HandleAllyControlsHunter(this FPPlayer fpp)
        {
	        bool isTargetingAlly = false;
	        needToLoadInputs = false;
	        GetUpdatedPlayerList();
	        
	        if (fpp != leadPlayer)
	        {
		        FPBaseObject targetObj = FPStage.FindNearestEnemy(fpp, 512f+64f, string.Empty);
		        if (targetObj == null)
		        {
			        //FPStage.FindNearestPlayer(fpp, 360f); //Doesn't work, it will always return itself.
			        targetObj = leadPlayer;
			        isTargetingAlly = true;
		        }
		        if (targetObj == null)
		        {
			        targetObj = fpp;
		        }

		        if (targetObj == fpp)
		        {
			        LogDebugOnly("A character is attempting to follow itself. Probably could not find a valid target.");
		        }

		        LogDebugOnly(String.Format("Hunter {2} Target set to {0} - ({1}[{3}])", 
			        targetObj.name, targetObj.position, fpp.name, fpp.GetInstanceID()));

		        if (targetObj != null)
		        {
			        FollowTargetObjectHorizontal(fpp, targetObj);
			        FollowTargetObjectVertical(fpp, targetObj);
			        float distToEnemy = Vector2.Distance(fpp.position, targetObj.position); 
			        if (!isTargetingAlly && distToEnemy <= targetObjHunterMinAttackDistanceHorizontal)
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
		        
		        AddTime(GetInputQueue(fpp), FPStage.deltaTime);
		        //RecordInput(fpp);
		        MapPlayerPressesFromPreviousInputs(fpp);
	        }
	        else if (fpp == leadPlayer)
	        {
		        LogDebugOnly("A character is attempting to follow itself as lead. Control types may be misassigned.");
	        }
        }
        
        public static void HandleAllyControlsGhost(this FPPlayer fpp)
        {
	        
	        GetUpdatedPlayerList();

	        if (needToLoadInputs)
	        {
		        LoadFileInputQueueForPlayer(fpp);
		        Fp2Trainer.Log("Need to load inputs from file. Grabbing most recent.");
		        needToLoadInputs = false;
	        }

	        var ipq = GetInputQueue(fpp);
	        if (ipq.charID == (int)fpp.characterID)
	        {
	        }
	        else
	        {
		        Fp2Trainer.Log($"Ghost is expecting characterID {ipq.charID.ToString()}, " +
		                       $"but is controlling a character of charID {((int)fpp.characterID).ToString()}");
	        }

	        if (ipq.stageName.Equals(FPStage.currentStage.stageName))
	        {
	        }
	        else
	        {
		        Fp2Trainer.Log($"Ghost is expecting stage {ipq.stageName}, " +
		                       $"but the current stage appears to be {FPStage.currentStage.stageName}");
	        }

	        var tsi = ipq.GetClosestToTimestamp(ipq.GetTimeElapsed(), 0);
	        tsi.MapInputsToFPPlayer(fpp);
	        
	        Fp2Trainer.Log($"Debug: {tsi.ToString()}");
	        
            
	        if (fpp != leadPlayer)
	        {
	            
	        }
	        else
	        {
		        LogDebugOnly("Ghost player set as leader. Intentional?");
	        }

        }
        
        public static void HandleAllyControlsNetplay(this FPPlayer fpp)
        {
	        
	        GetUpdatedPlayerList();

            
	        if (fpp != leadPlayer)
	        {
		        // ConnectionID
		        TimestampedInputs tsi = null;
		        FP2TrainerInputQueue tiq = GetLatestInputQueueFromNetworkPlayer(0);
		        RollingQueue<TimestampedTransform> transformQueue = GetLatestTransformQueueFromNetworkPlayer(0);
		        
		        // thisplayer, timestampedInputs, currentTime, expectedOffsetTime
		        MapPlayerClosestTimedInputFromInputQueue(fpp, tiq, 0f, 0f);
		        MapPosRotClosestFromTransformQueue(fpp, transformQueue, 0f, 0f);
		        
				/*
		        fpp.input.attackHold = leadPlayer.input.attackHold; 
		        fpp.input.attackPress = leadPlayer.input.attackPress;
                
		        fpp.input.specialHold = leadPlayer.input.specialHold;
		        fpp.input.specialPress = leadPlayer.input.specialPress;
                
		        fpp.input.up = leadPlayer.input.up;
		        fpp.input.upPress = leadPlayer.input.upPress;
                
		        fpp.input.down = leadPlayer.input.down;
		        fpp.input.downPress = leadPlayer.input.downPress;
		        */
                
		        try
		        {
			        MapPlayerPressesFromPreviousInputs(fpp);
		        }
		        catch (Exception e)
		        {
			        LogDebugOnly(e.Message);
			        LogDebugOnly(e.ToString());
			        LogDebugOnly(e.StackTrace);
		        }
	        }
        }
        
        public static void GetAndRecordInputFromPlayer1(this FPPlayer fpp)
        {
	        //int fiveMinutesAsFrames = 60 * 60 * 5;
	        fpp.GetInputFromPlayer1();
	        
	        try
	        {
		        AddTime(GetInputQueue(fpp), FPStage.deltaTime);
		        //GetInputQueue(fpp).SetMaxLength(fiveMinutesAsFrames);
		        RecordInput(fpp);
		        MapPlayerPressesFromPreviousInputs(fpp);

		        if (fpp.state == new FPObjectState(fpp.State_KO) || fpp.state == new FPObjectState(fpp.State_Victory))
		        {
			        string additionalHeader = $"fpp.characterID | {fpp.characterID}\r\n" +
			                                  $"FPStage.currentStage.stageName | {FPStage.currentStage.stageName}\r\n";
			        GetInputQueue(fpp).SaveQueueToFile(fpp, null, additionalHeader);
		        }
	        }
	        catch (Exception e)
	        {
		        LogDebugOnly(e.Message);
		        LogDebugOnly(e.ToString());
		        LogDebugOnly(e.StackTrace);
	        }
        }
        
        private static FP2TrainerInputQueue GetLatestInputQueueFromNetworkPlayer(int connectionId)
        {
	        FP2TrainerInputQueue tiq = null;
	        
	        
	        
	        return tiq;
        }

        private static RollingQueue<TimestampedTransform> GetLatestTransformQueueFromNetworkPlayer(int i)
        {
	        return new RollingQueue<TimestampedTransform>();
	        //throw new NotImplementedException();
        }

        private static void MapPosRotClosestFromTransformQueue(FPPlayer fpp, RollingQueue<TimestampedTransform> transformQueue, float currentTime, float playerToNetworkTimeOffset)
        {
	        var tt = GetClosestItemFromQueueByTimestamp(transformQueue, currentTime, playerToNetworkTimeOffset);
	        fpp.position.x = tt.position.x;
	        fpp.position.y = tt.position.y;

	        fpp.transform.position = new Vector3(tt.position.x, tt.position.y, fpp.transform.position.z);

	        fpp.angle = tt.angle;
        }
        
        private static void MapPlayerClosestTimedInputFromInputQueue(FPPlayer fpp, FP2TrainerInputQueue tiq, 
	        float currentTime, float playerToNetworkTimeOffset)
        {
	        var tsi = tiq.GetClosestToTimestamp(currentTime, playerToNetworkTimeOffset);
	        
        }

        public static TimestampedTransform GetClosestItemFromQueueByTimestamp(RollingQueue<TimestampedTransform> tiq, 
	        float targetTimestamp, float playerToNetworkTimeOffset)
        {
	        TimestampedTransform tt = null;
	        float timestamp = targetTimestamp - playerToNetworkTimeOffset;
	        if (timestamp < 0)
	        {
		        timestamp = 0;
	        }

	        for (int i = 0; i < tiq.Count(); i++)
	        {
		        if (timestamp >= tiq[i].timestamp)
		        {
			        tt = tiq[i];
		        }
		        else
		        {
			        float distToPrev = float.PositiveInfinity;
			        float distToNext = float.PositiveInfinity;
			        if (tt != null)
			        {
				        distToPrev = Mathf.Abs(tt.timestamp - timestamp);
			        }
			        distToNext = Mathf.Abs(tiq[i].timestamp - timestamp);

			        if (distToNext < distToPrev)
			        {
				        tt = tiq[i];
			        }
			        break;
		        }
	        }

	        return tt;
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

            if (preferredAllyControlType == AllyControlType.NPC_GHOST)
            {
	            needToLoadInputs = true;
            }
            else
            {
	            needToLoadInputs = false;
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
		        case AllyControlType.NPC_GHOST:
			        result = fpp.HandleAllyControlsGhost;
			        actually = "Ghost";
			        break;
		        case AllyControlType.NETWORK_MULTIPLAYER:
			        if (Fp2Trainer.EnableNetworking.Value)
			        {
				        result = fpp.HandleAllyControlsNetplay;
				        actually = "Netplay";
			        }
			        else
			        {
				        result = fpp.GetAndRecordInputFromPlayer1;
				        actually = "Player1";
			        }
			        break;
		        default:
			        result = fpp.GetAndRecordInputFromPlayer1;
			        actually = "Player1";
			        break;
	        }
	        Fp2Trainer.Log("Set control type to " + AllyControlTypeName(preferredAllyControlType));
	        Fp2Trainer.Log("(But actually, it's set to" + actually + ")");
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

        public static void DumpAllPlayerVars()
        {
	        var fppVars = "";

	        foreach (var fpp in allPlayers)
	        {
		        /*
		        fppVars += String.Format(
			        "{0}:\r\n" +
			        "{1} = {2}\r\n"));
			        */

		        fppVars += $"{fpp.name}: \r\n" +
		                   $"{nameof(fpp.barTimer)} : {fpp.barTimer}" +
		                   $"{nameof(fpp.inputLock)} : {fpp.inputLock}" +
		                   $"{nameof(fpp.idleTimer)} : {fpp.idleTimer}" +
		                   $"{nameof(fpp.targetGimmick)} : {fpp.targetGimmick}" +
		                   $"{nameof(fpp.targetWaterSurface)} : {fpp.targetWaterSurface}" +
		                   $"{nameof(fpp.chaseMode)} : {fpp.chaseMode}" +
		                   $"{nameof(fpp.swapCharacter)} : {fpp.swapCharacter}" +
		                   $"{nameof(fpp.hideChildObject)} : {fpp.hideChildObject}" +
		                   $"{nameof(fpp.lastGround)} : {fpp.lastGround}" +
		                   $"{nameof(fpp.lastSafePosition)} : {fpp.lastSafePosition}" +
		                   $"{nameof(fpp.hbAttack)} : {fpp.hbAttack}" +
		                   $"{nameof(fpp.hbHurt)} : {fpp.hbHurt}" +
		                   $"{nameof(fpp.hbTouch)} : {fpp.hbTouch}" +
		                   $"{nameof(fpp.interactWithObjects)} : {fpp.interactWithObjects}" +
		                   $"{nameof(fpp.childRender)} : {fpp.childRender}";
		        /*
	        $"{nameof(fpp.barTimer)} : {fpp.barTimer}" +
	        $"{nameof(fpp.barTimer)} : {fpp.barTimer}" +
	        $"{nameof(fpp.barTimer)} : {fpp.barTimer}" +
	        $"{nameof(fpp.barTimer)} : {fpp.barTimer}" +
	        $"{nameof(fpp.barTimer)} : {fpp.barTimer}" +
	        $"{nameof(fpp.barTimer)} : {fpp.barTimer}" +
	        $"{nameof(fpp.barTimer)} : {fpp.barTimer}" +
	        $"{nameof(fpp.barTimer)} : {fpp.barTimer}";*/

	        }
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
    }
}