using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MelonLoader;
using Object = UnityEngine.Object;

namespace Fp2Trainer
{
    public class FP2TrainerCharacterNameTag : MonoBehaviour
    {
        public static FP2TrainerCharacterNameTag instance;
        public Dictionary<int, GameObject> goNametags;
        public Dictionary<int, TextMesh> tmNametags;
        private List<string> placeholderNames;

        public Camera renderCamera = null;

        public Vector3 posRelativeToCam = Vector3.zero;
        
        public void Start()
        {
            if (instance != null)
            {
                GameObject.Destroy(this);
            }
            else
            {
                instance = this;
                this.transform.parent = Fp2Trainer.goFP2Trainer.transform;
            }

            try
            {
                renderCamera = GameObject.Find("Render Camera").GetComponent<Camera>();
            }
            catch (Exception e)
            {
                //do something
            }
            
            goNametags = new Dictionary<int, GameObject>();
            tmNametags = new Dictionary<int, TextMesh>();

            placeholderNames = new List<string>();
            placeholderNames.Add("Ann");
            placeholderNames.Add("Dazl");
            placeholderNames.Add("Sparks");
            placeholderNames.Add("Edna");

            if (Fp2Trainer.fpplayers != null)
            {
                foreach (FPPlayer fpp in Fp2Trainer.fpplayers)
                {
                    GameObject goNametag = InstantiateNewNametag(fpp);
                    goNametags.Add(fpp.GetInstanceID(), goNametag);
                    tmNametags.Add(fpp.GetInstanceID(), goNametag.GetComponent<TextMesh>());
                }
            }
        }

        public void Update()
        {
            foreach (FPPlayer fpp in Fp2Trainer.fpplayers)
            {
                if (fpp != null && !goNametags.ContainsKey(fpp.GetInstanceID()))
                {
                    GameObject goNametag = InstantiateNewNametag(fpp);
                    goNametags.Add(fpp.GetInstanceID(), goNametag);
                    tmNametags.Add(fpp.GetInstanceID(), goNametag.GetComponent<TextMesh>());
                }
                else if (fpp == null && goNametags.ContainsKey(fpp.GetInstanceID()))
                {
                    GameObject.Destroy(goNametags[fpp.GetInstanceID()]);
                    goNametags.Remove(fpp.GetInstanceID());
                    tmNametags.Remove(fpp.GetInstanceID());
                }

                var go = goNametags[fpp.GetInstanceID()];
                var tm = tmNametags[fpp.GetInstanceID()];
                
                try
                {
                    if (renderCamera == null)
                    {
                        renderCamera = GameObject.Find("Render Camera").GetComponent<Camera>();
                    }

                    if (renderCamera != null)
                    {
                        posRelativeToCam = renderCamera.WorldToScreenPoint(fpp.transform.position);
                    }
                    else
                    {
                        MelonLogger.Log("dabDABdabDABdabDAB");
                        posRelativeToCam = Fp2Trainer.GetPositionRelativeToCamera(FPCamera.stageCamera, fpp.transform.position);
                    }
                }
                catch (Exception e)
                {
                    Fp2Trainer.Log(e.Message + e.StackTrace);
                }
                
                //go.transform.position = new Vector3(fpp.transform.position.x, fpp.transform.position.y + 32f, go.transform.position.z );
                
                //go.transform.position = new Vector3(64, 64, go.transform.position.z );
                go.transform.position = posRelativeToCam + new Vector3(0, -64, 0);
                Fp2Trainer.Log($"gopos: {go.transform.position} | posrel: {posRelativeToCam}");
                
                /*
                 *goFancyTextPosition.transform.parent = goStageHUD.transform;
                    goFancyTextPosition.transform.localPosition = new Vector3(10, 20, 0);
                 * 
                 */

                tm.text = Regex.Replace(tm.text, @"\(.+\)",
                    $"({Mathf.Round(fpp.health)} / {Mathf.Round(fpp.healthMax)})");

                try
                {
                    var guessedWidth = tm.text.Length * tm.characterSize;
                    var guessedHeight = tm.characterSize;
                    
                    /*
                    if (transform.position.x + (guessedWidth / 2) > FPCamera.stageCamera.right)
                    {
                        transform.position += new Vector3(FPCamera.stageCamera.right - (guessedWidth / 2f), 0, 0);
                    }
                
                    if (transform.position.x - (guessedWidth / 2) < FPCamera.stageCamera.left)
                    {
                        transform.position += new Vector3(FPCamera.stageCamera.left + (guessedWidth / 2f), 0, 0);
                    }
                
                    if (transform.position.y + (guessedHeight / 2) > FPCamera.stageCamera.top)
                    {
                        transform.position += new Vector3(0, FPCamera.stageCamera.top - (guessedHeight / 2f), 0);
                    }
                
                    if (transform.position.y - (guessedHeight / 2) < FPCamera.stageCamera.bottom)
                    {
                        transform.position += new Vector3(0, FPCamera.stageCamera.bottom + (guessedHeight / 2f), 0);
                    }
                    */
                    // Temporarily dummying out the code that's supposed to make nametags stay within the camera confines.
                }

                catch (Exception e)
                {
                    Fp2Trainer.Log(e.Message + e.StackTrace);
                }

                
            }
        }

        public GameObject InstantiateNewNametagBaseObject(FPBaseObject fbo)
        {
            GameObject goNametag = null;
            if (Fp2Trainer.goFancyTextPosition != null)
            {
                goNametag = Object.Instantiate(Fp2Trainer.goFancyTextPosition);

                goNametag.layer = 5;
                goNametag.SetActive(true);
                var tm = goNametag.GetComponent<TextMesh>();
                tm.font = Fp2Trainer.fpMenuFont;
                tm.GetComponent<MeshRenderer>().materials[0] = Fp2Trainer.fpMenuMaterial;
                tm.characterSize = 10;
                tm.anchor = TextAnchor.LowerCenter;

                tm.text = fbo.name;
                
                // Stopgap measure just to make this look more interesting than it actually is:
                if (goNametags != null)
                {
                    int index = goNametags.Count % placeholderNames.Count;
                    tm.text = placeholderNames[index];
                }
            }

            return goNametag;
        }
        
        public GameObject InstantiateNewNametag(FPPlayer fpp)
        {
            GameObject goNametag = null;
            if (Fp2Trainer.goFancyTextPosition != null)
            {
                goNametag = Object.Instantiate(Fp2Trainer.goFancyTextPosition);

                goNametag.layer = 5;
                goNametag.SetActive(true);
                var tm = goNametag.GetComponent<TextMesh>();
                tm.font = Fp2Trainer.fpMenuFont;
                tm.GetComponent<MeshRenderer>().materials[0] = Fp2Trainer.fpMenuMaterial;
                tm.characterSize = 10;
                tm.anchor = TextAnchor.LowerCenter;
                tm.text = $"{fpp.name} ({Mathf.Round(fpp.health)} / {Mathf.Round(fpp.healthMax)})";

                // Stopgap measure just to make this look more interesting than it actually is:
                if (goNametags != null)
                {
                    int index = goNametags.Count % placeholderNames.Count;
                    tm.text = $"{placeholderNames[index]} ({Mathf.Round(fpp.health)} / {Mathf.Round(fpp.healthMax)})";
                    Fp2Trainer.Log($"index {index} - newName {tm.text}\n");
                }
            }

            return goNametag;
        }
    }
}