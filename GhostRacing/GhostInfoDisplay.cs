using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fp2Trainer.GhostRacing
{
    public class GhostInfoDisplay : MonoBehaviour
    {
        public GameObject gotm;
        public TextMesh tm;
        public GhostRecorder ghostRecorder;
        public GhostPlayer ghostPlayer;
        public GhostTimeline ghostTimeline;

        // Use this for initialization
        void Start()
        {
            if (tm == null)
            {
                gotm = new GameObject();
                gotm.name = "Text Mesh for Ghost Info Display";

                tm = gotm.AddComponent<TextMesh>();
                tm.characterSize = 32;
            }

            FetchTimelineFromGhosts();
        }

        private void FetchTimelineFromGhosts()
        {
            if (ghostRecorder != null)
            {
                ghostTimeline = ghostRecorder.ghostTimeline;
            }
            else if (ghostPlayer != null)
            {
                ghostTimeline = ghostPlayer.ghostTimeline;
            }
        }

        // Update is called once per frame
        void Update()
        {
            FetchTimelineFromGhosts();
            if (tm != null)
            {
                if (ghostTimeline != null)
                {
                    tm.text = ghostTimeline.ToString();
                }
                else
                {
                    tm.text = "No Ghost Data Set.";
                }
            }
        }
    }
}