using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fp2Trainer
{
    public class PlaneSwitcherVisualizer : MonoBehaviour
    {
        public static List<PlaneSwitcher> planeSwitchers;
        public static bool hasSpawnedVisualizers = false;
        List<GameObject> planeSwitchersVisualizers;
        GameObject goCube;
        Renderer renCube;
        Sprite pixelSprite;

        // Use this for initialization
        public void SpawnVisualizers()
        {
            if (hasSpawnedVisualizers)
            {
                Fp2Trainer.Log("Tried to spawn visualizers, but flag suggest status was not Reset.");
                return;
            }

            Fp2Trainer.Log("Started Plane Switcher Visualizer");
            planeSwitchersVisualizers = new List<GameObject>();
            planeSwitchers = new List<PlaneSwitcher>((GameObject.FindObjectsOfType<PlaneSwitcher>()));
            
            /*
            Debug.Log(
                System.String.Format("Found {0} PlaneSwitchers. Attempting to visualize.\n", planeSwitchers.Count));
                */


            Fp2Trainer.Log("Configuring visualizer properties.");
            Texture2D redPixel = new Texture2D(1, 1);
            redPixel.SetPixel(0, 0, new Color(1, 0, 0, 1f));

            pixelSprite = Sprite.Create(redPixel, new Rect(0.0f, 0.0f, redPixel.width, redPixel.height),
                new Vector2(0.5f, 0.5f), 1.0f);
            
            Fp2Trainer.Log("Spawning PlaneSwitchVisualizer Objects.");

            foreach (PlaneSwitcher ps in planeSwitchers)
            {
                //Debug.Log(System.String.Format("Adding Sprite Indicators to {0} PlaneSwitchers.\n", ps.name));
                Fp2Trainer.Log(System.String.Format("Adding Sprite Indicators to {0} PlaneSwitchers.\n", ps.name));

                goCube = new GameObject();
                var spriteRenderer = goCube.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = pixelSprite;

                goCube.name = ("Visualizer " + ps.name);
                goCube.transform.position = new Vector3(ps.transform.position.x, ps.transform.position.y,
                    ps.transform.position.z);
                goCube.transform.localScale = new Vector3(ps.xsize, ps.ysize, 1f);

                renCube = goCube.GetComponent<SpriteRenderer>();

                renCube.material.color = new Color(1, 0, 0, 0.7f);
                
                planeSwitchersVisualizers.Add(goCube);
            }
            hasSpawnedVisualizers = true;
        }

        public void Reset()
        {
            hasSpawnedVisualizers = false;
            if (planeSwitchersVisualizers != null)
            {
                foreach (GameObject go in planeSwitchersVisualizers)
                {
                    GameObject.Destroy(go);
                }
                planeSwitchersVisualizers.Clear();
            }
        }

        public void SetActiveOfSpawnedVisualizers(bool planeSwitchVisualizersVisible)
        {
            foreach (GameObject go in planeSwitchersVisualizers)
            {
                go.SetActive(planeSwitchVisualizersVisible);
            }
        }
    }
}