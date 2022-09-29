using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fp2Trainer
{
    [Flags]
    public enum BitwiseInputState : short
    {
        NONE = 0,
        UP = 1,
        DOWN = 2,
        LEFT = 4,
        RIGHT = 8,
        
        JUMP = 16,
        ATTACK = 32,
        SPECIAL = 64,
        GUARD = 128,
        
        PAUSE = 256
    };
    
    [Serializable]
    public class FP2TrainerInputQueue
    {
        // 9 bools, an index, and a timestamp.
        protected List<TimestampedInputs> timestampedInputsList;
        //protected int maxLength = 30;
        protected int maxLength = 60 * 60 * 5;
        protected int countSteps = 0;
        protected float elapsedTime;
        
        protected bool savedInputQueue = false;

        public FP2TrainerInputQueue()
        {
            timestampedInputsList = new List<TimestampedInputs>();
            timestampedInputsList.Capacity = this.maxLength;
            elapsedTime = 0f;
        }
        
        public FP2TrainerInputQueue(int maxLength)
        {
            timestampedInputsList = new List<TimestampedInputs>();
            this.maxLength = maxLength;
            timestampedInputsList.Capacity = this.maxLength;
            elapsedTime = 0f;
        }

        public void AddTime(float amountOfTime)
        {
            elapsedTime += amountOfTime;
        }
        
        public void SetMaxLength(int newMaxLength)
        {
            maxLength += newMaxLength;
            timestampedInputsList.Capacity = maxLength;
        }

        public TimestampedInputs Add(TimestampedInputs timestampedInputs)
        {
            TimestampedInputs purgedInputs = null;
            if (timestampedInputsList.Count >= maxLength && maxLength > 0)
            {
                SaveQueueToFile(); // only wrote this once using the flag to check.
            }

            while (timestampedInputsList.Count >= maxLength && maxLength > 0)
            {
                purgedInputs = timestampedInputsList[0];
                //Fp2Trainer.Log("Purging: \n" + purgedInputs.ToString());
                timestampedInputsList.RemoveAt(0);
            }

            if (timestampedInputs.timestamp == 0f)
            {
                timestampedInputs.timestamp = elapsedTime;
            }

            if (timestampedInputs.numStep == -1)
            {
                timestampedInputs.numStep = countSteps;
                countSteps++;
            }
            else
            {
                countSteps = timestampedInputs.numStep + 1;
            }

            timestampedInputsList.Add(timestampedInputs);

            return purgedInputs;
        }
        
        public TimestampedInputs GetLatest()
        {
            TimestampedInputs ti = null;
            int count = timestampedInputsList.Count;
            if (count > 0)
            {
                ti = timestampedInputsList[count - 1];
            }

            return ti;
        }
        
        public TimestampedInputs GetPrevious()
        {
            TimestampedInputs ti = null;
            int count = timestampedInputsList.Count;
            if (count > 1)
            {
                ti = timestampedInputsList[count - 2];
            }
            else if (count > 0)
            {
                ti = timestampedInputsList[count - 1];
            }

            return ti;
        }
        
        public TimestampedInputs GetClosestToTimestamp(float targetTimestamp, float playerToNetworkTimeOffset)
        {
            float timestamp = targetTimestamp - playerToNetworkTimeOffset;
            if (timestamp < 0)
            {
                timestamp = 0;
            }
            
            TimestampedInputs ti = null;
            for (int i = 0; i < timestampedInputsList.Count; i++)
            {
                if (timestamp >= timestampedInputsList[i].timestamp)
                {
                    ti = timestampedInputsList[i];
                }
                else
                {
                    float distToPrev = float.PositiveInfinity;
                    float distToNext = float.PositiveInfinity;
                    if (ti != null)
                    {
                        distToPrev = Mathf.Abs(ti.timestamp - timestamp);
                    }
                    distToNext = Mathf.Abs(timestampedInputsList[i].timestamp - timestamp);

                    if (distToNext < distToPrev)
                    {
                        ti = timestampedInputsList[i];
                    }
                    break;
                }
            }

            return ti;
        }

        override public string ToString()
        {
            string str = "---{InputQueue}---";
            foreach (var tsi in timestampedInputsList)
            {
                str += tsi.ToString() + "\n";
            }
            str += "---{EndInputQueue}---";

            return str;
        }

        public void SaveQueueToFile(string additionalHeaderText = "")
        {
            if (Fp2Trainer.SaveGhostFiles.Value && !savedInputQueue)
            {
                savedInputQueue = true;

                var allTimestampedInputs = "";

                allTimestampedInputs += $"maxLength | {maxLength}\r\n";
                allTimestampedInputs += $"countSteps | {countSteps}\r\n";
                allTimestampedInputs += $"elapsedTime | {elapsedTime}\r\n";
                
                allTimestampedInputs += additionalHeaderText;
                
                foreach (var tsi in timestampedInputsList)
                {
                    allTimestampedInputs += tsi.ToString() + "\r\n";
                }

                var fileName = "LastInputRecords" + string.Format("{0:yyyy-MM-dd_HH-mm-ss-fff}", DateTime.Now) + ".ghost";
                if (File.Exists(fileName))
                {
                    Debug.Log(fileName + " already exists... Overwriting.");
                    //return;
                }

                var sr = File.CreateText(fileName);
                sr.WriteLine(allTimestampedInputs);
                sr.Close();
            }
            else
            {
                //MelonLogger.Msg("Warped already...");
            }
        }
    }

    public class TimestampedInputs
    {
        public int numStep = 0;
        public float timestamp = 0f;
        public BitwiseInputState bitwiseInputs = BitwiseInputState.NONE;

        public TimestampedInputs()
        {
            numStep = -1;
            timestamp = 0f;
            bitwiseInputs = BitwiseInputState.NONE;
        }
        
        public TimestampedInputs(bool u, bool d, bool l, bool r, 
            bool j, bool a, bool s, bool g, 
            bool p ) : this(0f, u, d, l, r, j, a, s, g, p)
        {
        }

        public TimestampedInputs(float timestamp, bool u, bool d, bool l, bool r,
            bool j, bool a, bool s, bool g,
            bool p) : this(-1, 0f, u, d, l, r, j, a, s, g, p)
        {
        }

        public TimestampedInputs(int numStep, float timestamp, bool u, bool d, bool l, bool r,
            bool j, bool a, bool s, bool g,
            bool p)
        {
            this.numStep = numStep;
            this.timestamp = timestamp;
            this.bitwiseInputs = BitwiseInputState.NONE;

            this.bitwiseInputs |= u ? BitwiseInputState.UP : BitwiseInputState.NONE;
            this.bitwiseInputs |= d ? BitwiseInputState.DOWN : BitwiseInputState.NONE;
            this.bitwiseInputs |= l ? BitwiseInputState.LEFT : BitwiseInputState.NONE;
            this.bitwiseInputs |= r ? BitwiseInputState.RIGHT : BitwiseInputState.NONE;
            
            this.bitwiseInputs |= j ? BitwiseInputState.JUMP : BitwiseInputState.NONE;
            this.bitwiseInputs |= a ? BitwiseInputState.ATTACK : BitwiseInputState.NONE;
            this.bitwiseInputs |= s ? BitwiseInputState.SPECIAL : BitwiseInputState.NONE;
            this.bitwiseInputs |= g ? BitwiseInputState.GUARD : BitwiseInputState.NONE;
            
            this.bitwiseInputs |= p ? BitwiseInputState.PAUSE : BitwiseInputState.NONE;
        }
        
        public static bool HasFlag(BitwiseInputState theFlags, BitwiseInputState flagCondition)
        {
            return (theFlags & flagCondition) == flagCondition;
        }
        
        public void MapInputsToFPPlayer(FPPlayer fpp, BitwiseInputState bitwiseInputState)
        {
            fpp.input.up = HasFlag(bitwiseInputs, BitwiseInputState.UP);
            fpp.input.down = HasFlag(bitwiseInputs, BitwiseInputState.DOWN);
            fpp.input.left = HasFlag(bitwiseInputs, BitwiseInputState.LEFT);
            fpp.input.right = HasFlag(bitwiseInputs, BitwiseInputState.RIGHT);
            
            fpp.input.jumpHold = HasFlag(bitwiseInputs, BitwiseInputState.JUMP);
            fpp.input.attackHold = HasFlag(bitwiseInputs, BitwiseInputState.ATTACK);
            fpp.input.specialHold = HasFlag(bitwiseInputs, BitwiseInputState.SPECIAL);
            fpp.input.guardHold = HasFlag(bitwiseInputs, BitwiseInputState.GUARD);
        }

        override public string ToString()
        {
            return String.Format("{0} | {1} | {2,3} | {3:G}\n", numStep, timestamp, bitwiseInputs, (BitwiseInputState)bitwiseInputs);
        }
    }
}