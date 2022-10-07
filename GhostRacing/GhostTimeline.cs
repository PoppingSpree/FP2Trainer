using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Fp2Trainer.GhostRacing
{
    public class GhostTimeline
    {
        private AnimatorStateInfo animStateInfo;
        public int characterID = 0;
        public float clearTime = 3600f;
        protected int currentStep = 0;
        protected float currentTimeElapsed = 0f;

        public string gameVersion = String.Empty;
        protected List<GhostState> ghostStates = new List<GhostState>();
        protected GhostState newestGhost;
        public string playerName = String.Empty;
        public string animatorName = String.Empty;
        public string stageName = "Tutorial1";
        private Vector3 tempAngles;

        public void Add(GameObject targetToRecord)
        {
            newestGhost = new GhostState();
            newestGhost.Step = currentStep; //Double check this for order of operations.
            newestGhost.Time = currentTimeElapsed;
            newestGhost.Position = new Vector3(targetToRecord.transform.position.x,
                targetToRecord.transform.position.y,
                targetToRecord.transform.position.z);
            tempAngles = targetToRecord.transform.rotation.eulerAngles;
            newestGhost.Rotation = new Vector3(tempAngles.x, tempAngles.y, tempAngles.z);
            var animator = targetToRecord.GetComponent<Animator>();
            var childAnimator = targetToRecord.GetComponent<Animator>(); //Fix this later.

            if (animator != null)
            {
                animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
                newestGhost.AnimStateNameHash = animStateInfo.shortNameHash;
                newestGhost.AnimStateTime = animStateInfo.normalizedTime;
                newestGhost.AnimStateSpeed = animStateInfo.speed;
            }
            else
            {
                Debug.LogWarning("Target to record ghost data from has no active animator. Mistake?");
            }

            if (childAnimator != null)
            {
                animStateInfo = childAnimator.GetCurrentAnimatorStateInfo(0);
                newestGhost.AnimStateNameHash = animStateInfo.shortNameHash;
                newestGhost.AnimStateSpeed = animStateInfo.speed;
            }

            currentStep++;
            ghostStates.Add(newestGhost);
        }

        public void Add(GhostState gs)
        {
            newestGhost = gs;
            gs.Step = currentStep;
            currentStep++;
            ghostStates.Add(gs);
        }

        private void InitTimeline()
        {
            currentTimeElapsed = 0f;
            currentStep = 0;
            if (ghostStates == null)
            {
                ghostStates = new List<GhostState>();
            }
            else
            {
                ghostStates.Clear();
            }
        }

        public void ResetTimeline()
        {
            InitTimeline();
        }

        public void UpdateTime(float time)
        {
            this.currentTimeElapsed += time;
        }

        public void UpdateTime()
        {
            this.currentTimeElapsed += Time.deltaTime;
        }

        public GhostState GetCurrentStateByTime()
        {
            GhostState currentState = null;
            if (this.ghostStates.Count > 0)
            {
                for (int i = 0; i < ghostStates.Count; i++)
                {
                    if (ghostStates[i].Time <= currentTimeElapsed)
                    {
                        currentState = ghostStates[i];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return currentState;
        }

        public GhostState GetCurrentStateByTimeLerped()
        {
            GhostState lerpedGhostState;
            var currentGhostState = GetCurrentStateByTime();
            var nextGhostState = GetNextGhostState(currentGhostState);

            // (Current Position - Minimum Position) divided by (Range between both positions.)
            var relativePositionBetween =
                (currentTimeElapsed - currentGhostState.Time) / (nextGhostState.Time - currentGhostState.Time);

            var lerpedPosition =
                Vector3.Lerp(currentGhostState.Position, nextGhostState.Position, relativePositionBetween);
            var lerpedRotation =
                Vector3.Lerp(currentGhostState.Rotation, nextGhostState.Rotation, relativePositionBetween);

            lerpedGhostState = currentGhostState.Clone();
            lerpedGhostState.Position = lerpedPosition;
            lerpedGhostState.Rotation = lerpedRotation;

            return lerpedGhostState;
        }

        public GhostState GetCurrentStateByStep()
        {
            GhostState currentState = null;
            if (this.ghostStates.Count > 0)
            {
                for (int i = 0; i < ghostStates.Count; i++)
                {
                    if (ghostStates[i].Step <= currentStep)
                    {
                        currentState = ghostStates[i];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return currentState;
        }

        public GhostState GetStateByStep(int step)
        {
            GhostState chosenState = null;
            if (this.ghostStates.Count > 0)
            {
                // Use shortcuts to try to find this quickly in longer sets.
                // Shortcut 1: Assume step = index.
                if (ghostStates.Count >= step - 1)
                {
                    chosenState = ghostStates[step];
                }

                if (chosenState.Step == step)
                {
                    return chosenState;
                }

                //Shortcut 2: If requested step is greater than the step we pulled, Go back by half. Then If this step is lower, start iterating from there.
                if (step > chosenState.Step)
                {
                    step /= 2;
                }

                // Shortcut 3: Of the requested step was actually _lower_ than what we got with the first shortcut, just iterate from there.

                for (int i = step; i < ghostStates.Count; i++)
                {
                    if (ghostStates[i].Step <= currentStep)
                    {
                        chosenState = ghostStates[i];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return chosenState;
        }

        public GhostState GetNextGhostState(GhostState relativeState)
        {
            GhostState nextState = null;
            int index = ghostStates.IndexOf(relativeState);
            if (index >= 0 &&
                index + 1 < (ghostStates.Count)) // Not an off by one error, we're targeting the NEXT state.
            {
                nextState = ghostStates[index + 1];
            }

            if (index + 1 >= ghostStates.Count)
            {
                nextState = relativeState;
            }

            return nextState;
        }

        public void PlayFromStart()
        {
            currentStep = 0;
            currentTimeElapsed = 0f;
        }

        public new string ToString()
        {
            var text = "";
            int i = 0;
            foreach (var gs in ghostStates)
            {
                text += string.Format("{0}:{1}", i, gs.ToString());
                i++;
            }

            return text;
        }

        public GhostTimeline Clone()
        {
            var clone = new GhostTimeline();

            foreach (var gs in ghostStates)
            {
                clone.Add(gs);
            }

            return clone;
        }

        public void SaveGhostToFile()
        {
            if (Fp2Trainer.SaveGhostFiles.Value)
            {
                string fileText = String.Empty;
                string filename = String.Empty;
                CreateGhostsFolderIfNeeded();
                CreateStageFolderIfNeeded();
                fileText += AddHeaderText();
                fileText += AddGhostStateText();
                filename = GenerateFileName();
                WriteGhostFile(filename, fileText);
                
                /*
                var fileName = "Inputs" + string.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".inputghost"; // Need to make this so that once a file starts writing it appends to the same file.
                if (fpp!= null)
                {
                    if (FPStage.currentStage != null)
                    {
                        fileName = $"{FPStage.currentStage.stageName}-{fileName}";
                    }
                    
                    fileName = $"{fpp.name}-{fpp.GetInstanceID().ToString()}-{fileName}";
                }

                if (FPStage.currentStage != null)
                {
                    fileName = Path.Combine($"{FPStage.currentStage.stageName}\\", fileName);
                    System.IO.Directory.CreateDirectory($"inputghosts\\{FPStage.currentStage.stageName}\\");
                }
                
                fileName = Path.Combine("inputghosts\\", fileName);
                Fp2Trainer.Log($"Finna write {fileName}");

                if (File.Exists(fileName))
                {
                    Fp2Trainer.Log(fileName + " already exists... Appending.");
                    //return;
                }

                var sr = File.AppendText(fileName);
                sr.WriteLine(allTimestampedInputs);
                sr.Close();
                */
            }
        }


        public void PopulateTimelineFromText(string[] lines)
        {
            ghostStates = new List<GhostState>();
            Regex rxInputQueueLine = new Regex(@"^\d+\|[\d\.]+\|\d+");

            foreach (var line in lines)
            {
                if (line.Contains("gameVersion:"))
                {
                    gameVersion = line.Replace("gameVersion:", "").Trim();
                }
                if (line.Contains("playerName:"))
                {
                    playerName = playerName = line.Replace("playerName:", "").Trim();
                }
                if (line.Contains("stageName:"))
                {
                    stageName = stageName = line.Replace("stageName:", "").Trim();
                }
                if (line.Contains("characterID:"))
                {
                    characterID = int.Parse(line.Replace("characterID:", "").Trim());
                }
                if (line.Contains("clearTime:"))
                {
                    clearTime = float.Parse(line.Replace("clearTime:", "").Trim());
                }

                if (line.StartsWith("Step:"))
                {
                    var gs = new GhostState();
                    var stateVars = line.Split('|');
                    foreach (var segment in stateVars)
                    {
                        if (segment.StartsWith("Step:"))
                        {
                            gs.Step = int.Parse(segment.Replace("Step:", ""));
                        }
                        if (segment.StartsWith("Time:"))
                        {
                            gs.Time = float.Parse(segment.Replace("Time:", ""));
                        }
                        if (segment.StartsWith("Pos:"))
                        {
                            gs.Position = Vector3FromString(segment.Replace("Pos:", ""));
                        }
                        if (segment.StartsWith("Rot:"))
                        {
                            gs.Rotation = Vector3FromString(segment.Replace("Rot:", ""));
                        }
                        
                        if (segment.StartsWith("AnimStateNameHash:"))
                        {
                            gs.AnimStateNameHash = int.Parse(segment.Replace("AnimStateNameHash:", ""));
                        }
                        if (segment.StartsWith("AnimStateTime:"))
                        {
                            gs.AnimStateTime = float.Parse(segment.Replace("AnimStateTime:", ""));
                        }
                        
                        if (segment.StartsWith("AnimStateSpeed:"))
                        {
                            gs.AnimStateSpeed = float.Parse(segment.Replace("AnimStateSpeed:", ""));
                        }
                        if (segment.StartsWith("ChildAnimStateNameHash:"))
                        {
                            gs.ChildAnimStateNameHash = int.Parse(segment.Replace("ChildAnimStateNameHash:", ""));
                        }
                        if (segment.StartsWith("ChildPosition:"))
                        {
                            gs.ChildPosition = Vector3FromString(segment.Replace("ChildPosition:", ""));
                        }
                        if (segment.StartsWith("ChildRotation:"))
                        {
                            gs.ChildRotation = Vector3FromString(segment.Replace("ChildRotation:", ""));
                        }
                        
                        
                        
                        
                        /*
                         *string text = string.Format("Step:{0}|Time:{1}|Pos:{2}|Rot:{3}|", Step, Time, Position, Rotation) +
                          string.Format("AnimStateNameHash:{0}|AnimStateTime:{1}|AnimStateSpeed:{2}",
                              AnimStateNameHash, AnimStateTime,
                              AnimStateSpeed);
            if (ChildAnimStateNameHash != -1)
            {
                text += string.Format("|ChildAnimStateNameHash:{0}|ChildPosition:{1}|ChildRotation:{2}",
                    ChildAnimStateNameHash, ChildPosition,
                    ChildRotation);
            }
                         * 
                         */
                    }
                    ghostStates.Add(gs);
                }
            }
        }

        public string GetLatestGhostFile()
        {
            string filePath = String.Empty;
        
            try
            {
                
                var directory = new DirectoryInfo("inputghosts\\{FPStage.currentStage.stageName}\\");
                var ghostFilePaths = directory.GetFiles("*.raceghost");
                var filePathInfo = ghostFilePaths.OrderByDescending(f => f.LastWriteTime).First();
                filePath = filePathInfo.FullName;

            }
            catch (NullReferenceException e)
            {
                Fp2Trainer.Log("Null reference exception when trying to load asset bundles for modding. Canceling.");
                Fp2Trainer.Log(e.StackTrace);
            }

            return filePath;
        }

        public void LoadLatestGhostFile()
        {
            LoadGhostFile(GetLatestGhostFile());
        }

        public Vector3 Vector3FromString(string txt)
        {
            var floats = txt.Replace("(", "")
                .Replace(")", "")
                .Split(',');
            
            return new Vector3(float.Parse(floats[0]), float.Parse(floats[1]), float.Parse(floats[2]));
        }

        public void LoadGhostFile(string filename)
        {
            string[] lines = System.IO.File.ReadAllLines(filename);
            PopulateTimelineFromText(lines);
        }

        private void WriteGhostFile(string filename, string fileText)
        {
            var sr = File.AppendText(filename);
            sr.WriteLine(fileText);
            sr.Close();
        }

        private string GenerateFileName()
        {
            var fileName = string.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".raceghost";
            fileName = FPStage.player[characterID].name + "-" + fileName;
            return fileName;
        }

        private string AddGhostStateText()
        {
            return ToString();
        }

        private string AddHeaderText()
        {
            var txt = string.Empty;
            txt += $"characterID:{characterID}\n";
            txt += $"gameVersion:{gameVersion}\n";
            txt += $"playerName:{playerName}\n";
            txt += $"stageName:{stageName}\n";
            txt += $"clearTime:{clearTime}\n";
            return txt;
        }

        private void CreateStageFolderIfNeeded()
        {
            if (FPStage.currentStage != null)
            {
                System.IO.Directory.CreateDirectory($"inputghosts\\{FPStage.currentStage.stageName}\\");
            }
        }

        private void CreateGhostsFolderIfNeeded()
        {
            System.IO.Directory.CreateDirectory($"inputghosts\\");
        }

        /*
        public void SaveQueueToFile(FPPlayer fpp = null, 
            List<TimestampedInputs> timestampedInputsListToWrite = null, string additionalHeaderText = "")
        {
            if (Fp2Trainer.SaveGhostFiles.Value)
            {

                if (timestampedInputsListToWrite == null)
                {
                    if (usePurgeListForFileContentBuffer)
                    {
                        timestampedInputsListToWrite = this.timestampedInputsPurgedForFileWrite;
                    }
                    else
                    {
                        timestampedInputsListToWrite = this.timestampedInputsList;
                    }
                }

                if (fpp == null && FPStage.currentStage != null)
                {
                    fpp = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                }


                var allTimestampedInputs = "";

                if (!shouldSkipAddingHeader)
                {
                    shouldSkipAddingHeader = true;
                    allTimestampedInputs += $"maxLength|{maxLength}\n";
                    allTimestampedInputs += $"countSteps|{countSteps}\n";
                    allTimestampedInputs += $"elapsedTime|{elapsedTime}\n";
                    
                    if (fpp!= null)
                    {
                        if (FPStage.currentStage != null)
                        {
                            allTimestampedInputs += $"stageName|{FPStage.currentStage.stageName}\n";
                            allTimestampedInputs += $"charID|{(int)fpp.characterID}\n";
                            allTimestampedInputs += "userName|SpeedRunner\n";
                        }
                    }
                
                    allTimestampedInputs += additionalHeaderText;
                }

                foreach (var tsi in timestampedInputsListToWrite)
                {
                    allTimestampedInputs += tsi.ToString() + "\n";
                }

                

                Fp2Trainer.Log("File written and closed.");
            }
            else
            {
                //MelonLogger.Msg("Warped already...");
            }
        }

        public static FP2TrainerInputQueue LoadQueueFromFileMostRecent()
        {
            var result = new FP2TrainerInputQueue();
            result.maxLength = 1_000_000; //Roughly 4 hours. Probably needs more memory than a PC would have.

            if (Fp2Trainer.SaveGhostFiles.Value)
            {
                string fileName = "";

                if (FPStage.currentStage != null)
                {
                    fileName = Path.Combine($"{FPStage.currentStage.stageName}\\", fileName);
                }
                
                fileName = Path.Combine("ghosts\\", fileName);


                try
                {

                    var directory = new DirectoryInfo(fileName);
                    var ghostFilePaths = directory.GetFiles("*.ghost");
                    var latestGhostFile = ghostFilePaths.OrderByDescending(f => f.LastWriteTime).First();

                    int fileCharID = -1;
                    string fileStageName = String.Empty;
                    
                    string[] lines = System.IO.File.ReadAllLines(latestGhostFile.FullName);

                    Regex rxInputQueueLine = new Regex(@"^\d+\|[\d\.]+\|\d+");

                    foreach (var line in lines)
                    {
                        // Level and Character data
                        if (line.Contains("charID"))
                        {
                            result.charID = int.Parse(line.Split('|')[1]);
                        }
                        
                        if (line.Contains("stageName"))
                        {
                            result.stageName = line.Split('|')[1];
                        }
                        
                        if (line.Contains("userName"))
                        {
                            result.userName = line.Split('|')[1];
                        }

                        // Input Data
                        if (rxInputQueueLine.IsMatch(line))
                        {
                            var segments = line.Split('|');
                            var tsi = new TimestampedInputs(
                                Convert.ToInt32(segments[0]),
                                Convert.ToSingle(segments[1]),
                                Convert.ToInt16(segments[2], 2));
                            result.Add(tsi);
                        }
                    }
                }
                catch (NullReferenceException e)
                {
                    Fp2Trainer.Log("Null reference exception when trying to load asset bundles for modding. Canceling.");
                    Fp2Trainer.Log(e.StackTrace);
                }
                
                //asdfasdfasfd
            }
            return result;
        }

        public static FP2TrainerInputQueue LoadQueueFromFile(string filename)
        {
            var result = new FP2TrainerInputQueue();
            try
            {
            
                result.maxLength = 1_000_000; //Roughly 4 hours. Probably needs more memory than a PC would have.
                
                string ghostLevel = "";
                int ghostCharacter = -1;
                string ghostScreenName = "Speedrunner";
                
                if (Fp2Trainer.SaveGhostFiles.Value)
                {
                    try
                    {
                        string[] lines = System.IO.File.ReadAllLines(filename);

                        Regex rxInputQueueLine = new Regex(@"^\d+\|[\d\.]+\|\d+");

                        foreach (var line in lines)
                        {
                            if (line.Contains("charID"))
                            {
                                result.charID = int.Parse(line.Split('|')[1]);
                            }
                        
                            if (line.Contains("stageName"))
                            {
                                result.stageName = line.Split('|')[1];
                            }
                        
                            if (line.Contains("userName"))
                            {
                                result.userName = line.Split('|')[1];
                            }
                            
                            // Input Data
                            if (rxInputQueueLine.IsMatch(line))
                            {
                                var segments = line.Split('|');
                                result.Add(new TimestampedInputs(
                                    Convert.ToInt32(segments[0]),
                                    Convert.ToSingle(segments[1]),
                                        Convert.ToInt16(segments[2], 2)));
                            }
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Fp2Trainer.Log("Null reference exception when trying to load ghost file. Canceling.");
                        Fp2Trainer.Log(e.StackTrace);
                    }
                }
            }
            catch (Exception e)
            {
                Fp2Trainer.Log(e.ToString());
                Fp2Trainer.Log(e.StackTrace);
            }
            
            return result;
        }
        */
    }

    public class GhostState
    {
        public GhostState()
        {
            Step = 0;
            Time = 0;
            Position = Vector3.zero;
            Rotation = Vector3.zero;
            AnimStateNameHash = -1;
            AnimStateTime = -1;
            AnimStateSpeed = 1;

            ChildPosition = Vector3.zero;
            ChildRotation = Vector3.zero;
            ChildAnimStateNameHash = -1;
        }

        public int Step { get; set; }
        public float Time { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public int AnimStateNameHash { get; set; }
        public float AnimStateTime { get; set; }
        public float AnimStateSpeed { get; set; }
        public Vector3 ChildPosition { get; set; }
        public Vector3 ChildRotation { get; set; }
        public int ChildAnimStateNameHash { get; set; }

        public new string ToString()
        {
            string text = string.Format("Step:{0}|Time:{1}|Pos:{2}|Rot:{3}|", Step, Time, Position, Rotation) +
                          string.Format("AnimStateNameHash:{0}|AnimStateTime:{1}|AnimStateSpeed:{2}",
                              AnimStateNameHash, AnimStateTime,
                              AnimStateSpeed);
            if (ChildAnimStateNameHash != -1)
            {
                text += string.Format("|ChildAnimStateNameHash:{0}|ChildPosition:{1}|ChildRotation:{2}",
                    ChildAnimStateNameHash, ChildPosition,
                    ChildRotation);
            }

            text += "\n";

            return text;
        }

        public GhostState Clone()
        {
            var gs = new GhostState();
            gs.Step = this.Step;
            gs.Time = this.Time;

            gs.Position = this.Position;
            gs.Rotation = this.Rotation;

            gs.AnimStateNameHash = this.AnimStateNameHash;

            gs.AnimStateTime = this.AnimStateTime;
            gs.AnimStateSpeed = this.AnimStateSpeed;

            gs.ChildPosition = this.ChildPosition;
            gs.ChildRotation = this.ChildRotation;
            gs.ChildAnimStateNameHash = this.ChildAnimStateNameHash;

            return gs;
        }
    }
}