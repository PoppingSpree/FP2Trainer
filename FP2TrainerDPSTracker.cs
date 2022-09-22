using System;
using System.Collections.Generic;

namespace Fp2Trainer
{
    public class FP2TrainerDPSTracker
    {
        private List<DPSDamageInfo> damageInfos;
        public float timeElapsed;
        public float dps;

        public FP2TrainerDPSTracker()
        {
            this.Reset();
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
            timeElapsed += UnityEngine.Time.deltaTime;

            // Purge times more than 1 second stale.
            for (int i = 0; i < damageInfos.Count; i++)
            {
                if (damageInfos[i].Time < (timeElapsed - 100f))
                {
                    damageInfos.RemoveAt(i);
                    i--;
                }
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
            damageInfos.Add(new DPSDamageInfo(timeElapsed, dmg));
        }
        
        public void AddDamage(float dmg, string enemyName)
        {
            damageInfos.Add(new DPSDamageInfo(timeElapsed, dmg, enemyName));
        }

        public float CalculateDPS()
        {
            dps = 0;
            foreach (DPSDamageInfo dInfo in damageInfos)
            {
                dps += dInfo.DMG;
                Fp2Trainer.Log("DPS Calc: " + dps.ToString() + " (+" + dInfo.DMG.ToString() + ")");
            }

            return dps;
        }
        
        public float GetDPS()
        {
            return dps;
        }

        public string GetDPSBreakdownString()
        {
            var breakdown = "";
            breakdown += "Elapsed Time: " + timeElapsed.ToString() + "\n";
            breakdown += this.ToString() + "\n";
            foreach (DPSDamageInfo dInfo in damageInfos)
            {
                breakdown += "Hit DMG: " + dInfo.DMG.ToString() + " @ " + dInfo.Time.ToString() ;
                if (dInfo.EnemyName != null)
                {
                    breakdown += " (" + dInfo.EnemyName + ")";
                }
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
        public float Time;
        public float DMG;
        public string EnemyName;

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