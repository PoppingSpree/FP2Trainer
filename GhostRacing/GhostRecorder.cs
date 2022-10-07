using System;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fp2Trainer.GhostRacing
{
    public class GhostRecorder : MonoBehaviour {
        public GameObject objectToRecord;

        public GhostTimeline ghostTimeline;

        public bool saveOnDestroy = true;

        public static float stageTime = -1;
        public static string stageName = String.Empty;

        // Use this for initialization
        public void Start () {
            if (stageName.Equals(SceneManager.GetActiveScene().name))
            {
                MelonLogger.Log("Stage name did not change. Restart / Respawn / Checkpoint?");
            }

        }
	
        // Update is called once per frame
        public void Update () {
            if (ghostTimeline == null)
            {
                ghostTimeline = new GhostTimeline();
            }

            ghostTimeline.UpdateTime();

            if (objectToRecord != null)
            {
                ghostTimeline.Add(objectToRecord);
            }
        }

        public void OnDestroy()
        {
            if (saveOnDestroy)
            {
                if (ghostTimeline != null)
                {
                    ghostTimeline.SaveGhostToFile();
                }
            }
        }
        
        public static void Instantiate()
        {
            var go = new GameObject();
            go.name = "Ghost Recorder " + go.GetInstanceID();
            go.AddComponent<GhostRecorder>();

            var gr = go.GetComponent<GhostRecorder>();
            
            var fpp = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            if (fpp != null)
            {
                gr.objectToRecord = fpp.gameObject;
            }
        }
    }

}