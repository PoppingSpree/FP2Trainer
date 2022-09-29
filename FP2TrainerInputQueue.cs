using System;
using System.Collections.Generic;
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
    
    public class FP2TrainerInputQueue
    {
        // 9 bools, an index, and a timestamp.
        protected List<TimestampedInputs> timestampedInputsList;
        protected int maxLength = 30;
        protected int countSteps = 0;
        protected float elapsedTime;

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

        public TimestampedInputs Add(TimestampedInputs timestampedInputs)
        {
            TimestampedInputs purgedInputs = null; 
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
    }

    public class TimestampedInputs
    {
        public int numStep = 0;
        public double timestamp = 0f;
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

        override public string ToString()
        {
            return String.Format("{0} | {1} | {2,3} | {3:G}\n", numStep, timestamp, bitwiseInputs, (BitwiseInputState)bitwiseInputs);
        }
    }
}