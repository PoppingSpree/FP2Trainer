using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fp2Trainer.GhostRacing
{
    public class GhostPlayer : MonoBehaviour
    {
        public GhostTimeline ghostTimeline;

        public float opacity = 0.7f;

        public string targetAnimatorControllerName = "Cursor";
        public SpriteRenderer spriteRenderer;
        public Animator animator;
        public RuntimeAnimatorController animatorController;

        public GhostRecorder cloneTimelineFromThisRecorder;

        public int attempts = 0;

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            DebugCloneTimelineFromRecorder();

            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, opacity);

            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }

            if (animator.runtimeAnimatorController == null || animatorController == null)
            {
                animatorController = GetAnimatorFromLoadedResourcesByName(targetAnimatorControllerName);
                animator.runtimeAnimatorController = animatorController;

                attempts++;
                if (attempts >= 20)
                {
                    Debug.LogWarning("Attempted to fetch an animator 20 or more times, but found nothing." +
                                     "This instance may be assigned an invalid animator target string, " +
                                     "or the relevant animator may not have loaded into memory yet.");
                }
            }
            else
            {
                attempts = 0;
            }
            //PrintDebugThingy();

            UpdateTimeline();
            if (ghostTimeline != null)
            {
                UpdateAnimatorFromGhostState(ghostTimeline.GetCurrentStateByTime());
            }
        }
        
        public static void Instantiate()
        {
            var go = new GameObject();
            go.name = "Ghost Player " + go.GetInstanceID();
            go.AddComponent<GhostPlayer>();
        }

        public RuntimeAnimatorController GetAnimatorFromLoadedResourcesByName(string targetName)
        {
            RuntimeAnimatorController animController = null;
            var runtimeAnimators = Resources.FindObjectsOfTypeAll<RuntimeAnimatorController>();
            foreach (var ac in runtimeAnimators)
            {
                if (ac.name.Equals(targetName))
                {
                    animController = ac;
                    break;
                }
            }

            return animController;
        }

        void PrintDebugThingy()
        {
            //var EditorAnimators = Resources.FindObjectsOfTypeAll<AnimatorController>();
            var line = "----------------------------------------";
            var blah = "";
            
            /*
            var editorAnimators = Resources.FindObjectsOfTypeAll<UnityEditor.Animations.AnimatorController>();
            Debug.Log("----- Editor Animators -----\n" + editorAnimators.Length);
            
            foreach (var item in editorAnimators)
            {
                blah += item.ToString() + " : " + item.name + "\n";
            }

            Debug.Log(blah);
            Debug.Log(line);
            
            */

            var runtimeAnimators = Resources.FindObjectsOfTypeAll<RuntimeAnimatorController>();
            Debug.Log("----- Runtime Animators -----\n" + runtimeAnimators.Length);
            blah = "";
            foreach (var item in runtimeAnimators)
            {
                blah += item.ToString() + " : " + item.name + "\n";
            }

            Debug.Log(blah);
            Debug.Log(line);

            UpdateTimeline();
            if (ghostTimeline != null)
            {
                //Jumpy, but more accurate
                //UpdateAnimatorFromGhostState(ghostTimeline.GetCurrentStateByTime()); 

                //Smooth, but less accurate.Consider making this a toggle?
                UpdateAnimatorFromGhostState(ghostTimeline.GetCurrentStateByTimeLerped());
            }
        }

        public void UpdateTimeline()
        {
            if (ghostTimeline != null)
            {
                ghostTimeline.UpdateTime();
            }
        }

        public void UpdateAnimatorFromGhostState(GhostState ghostState)
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.Play(ghostState.AnimStateNameHash, 0, ghostState.Time);
                animator.speed = ghostState.AnimStateSpeed;
                //animator.loop = ghostState.AnimStateLoop; Loop would come from the State's clip, not the State itself.

                gameObject.transform.SetPositionAndRotation(ghostState.Position,
                    Quaternion.Euler(ghostState.Rotation));
            }
        }

        protected void DebugCloneTimelineFromRecorder()
        {
            if (cloneTimelineFromThisRecorder != null)
            {
                ghostTimeline = cloneTimelineFromThisRecorder.ghostTimeline.Clone();
                ghostTimeline.PlayFromStart();

                cloneTimelineFromThisRecorder = null;
            }
        }
    }
}