using System.Collections.Generic;

namespace Fp2Trainer
{
    public class FP2TrainerDPSTracker
    {
        public float cutoffTime = 5f;
        private List<DPSDamageInfo> damageInfos;
        public float dps;
        public float lastEmptyTime;
        public float timeElapsed;

        public FP2TrainerDPSTracker()
        {
            Reset();
        }

        public void Update()
        {
            UpdateTimer();
            UpdateTrackAllEnemyDamage();
        }

        private void UpdateTrackAllEnemyDamage()
        {
            //throw new NotImplementedException();

            // Show nearest enemy HP and update DPS.
            /*
            if (FPStage.currentStage != null)
            {
                //var activeEnemies = FPStage.GetActiveEnemies();
                nearestEnemy = FPStage.FindNearestEnemy(fpplayer, 200f);
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
            }
            */
        }

        public void UpdateTimer()
        {
            timeElapsed += FPStage.frameTime / 2;

            // Purge times more than 1 (or 2???) second stale.
            for (var i = 0; i < damageInfos.Count; i++)
                if (damageInfos[i].Time < timeElapsed - cutoffTime)
                {
                    damageInfos.RemoveAt(i);
                    i--;
                }
        }

        public void Reset()
        {
            damageInfos = new List<DPSDamageInfo>();
            timeElapsed = 0;
            dps = 0;
        }

        public void AddDamage(float dmg)
        {
            UpdateLastEmptyTime();
            damageInfos.Add(new DPSDamageInfo(timeElapsed, dmg));
        }

        public void AddDamage(float dmg, string enemyName)
        {
            UpdateLastEmptyTime();
            damageInfos.Add(new DPSDamageInfo(timeElapsed, dmg, enemyName));
        }

        public void UpdateLastEmptyTime()
        {
            if (damageInfos.Count < 1) lastEmptyTime = timeElapsed;
        }

        public float CalculateDPS()
        {
            dps = 0;
            foreach (var dInfo in damageInfos) dps += dInfo.DMG;
            //Fp2Trainer.Log("DPS Calc: " + dps.ToString() + " (+" + dInfo.DMG.ToString() + ")");
            var timeSinceLastEmpty = timeElapsed - lastEmptyTime;
            if (timeSinceLastEmpty > 0 && timeSinceLastEmpty < 1)
                return dps;
            if (timeSinceLastEmpty >= 1 && timeSinceLastEmpty <= cutoffTime)
                return dps / timeSinceLastEmpty;
            if (timeSinceLastEmpty > cutoffTime)
                return dps / cutoffTime;
            if (timeSinceLastEmpty <= 0) return dps;

            Fp2Trainer.Log("Something's funky with the DPS Tracker when calculating DPS???");
            return dps;
        }

        public float GetDPS()
        {
            return dps;
        }

        public string GetDPSBreakdownString()
        {
            var breakdown = "";
            breakdown += "Elapsed Time: " + timeElapsed + "\n";
            breakdown += ToString() + "\n";
            foreach (var dInfo in damageInfos)
            {
                breakdown += "Hit DMG: " + dInfo.DMG + " @ " + dInfo.Time;
                if (dInfo.EnemyName != null) breakdown += " (" + dInfo.EnemyName + ")";
                breakdown += "\n";
            }

            return breakdown;
        }

        public override string ToString()
        {
            return CalculateDPS().ToString();
        }
    }

    public class DPSDamageInfo
    {
        public float DMG;
        public string EnemyName;
        public float Time;

        public DPSDamageInfo(float time, float dmg)
        {
            Time = time;
            DMG = dmg;
            EnemyName = "-----";
        }

        public DPSDamageInfo(float time, float dmg, string enemyName)
        {
            Time = time;
            DMG = dmg;
            EnemyName = enemyName;
        }
    }
}