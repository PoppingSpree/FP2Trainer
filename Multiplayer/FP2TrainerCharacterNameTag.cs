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
        public Dictionary<int, FPPlayer> idToPlayers;
        private List<string> placeholderNames;

        public Camera renderCamera = null;
        public GameObject goStageCamera = null;

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
                if (!Fp2Trainer.DisplayNametags.Value)
                {
                    gameObject.SetActive(false);
                    this.enabled = false;
                }
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
            foreach (var nt in goNametags.Values)
            {
                nt.SetActive(Fp2Trainer.DisplayNametags.Value);
            }
            
            if (!Fp2Trainer.DisplayNametags.Value)
            {
                return;
            }
            
            // Make sure cameras exist.
            try
            {
                if (renderCamera == null)
                {
                    renderCamera = GameObject.Find("Render Camera").GetComponent<Camera>();
                }

                if (renderCamera != null)
                {
                    //posRelativeToCam = renderCamera.WorldToScreenPoint(fpp.transform.position);
                }
                else
                {
                    //MelonLogger.Log("dabDABdabDABdabDAB");
                    //posRelativeToCam = Fp2Trainer.GetPositionRelativeToCamera(FPCamera.stageCamera, fpp.transform.position);
                }

                if (goStageCamera == null)
                {
                    goStageCamera = FPCamera.stageCamera.gameObject;
                }
            }
            catch (Exception e)
            {
                Fp2Trainer.Log(e.Message + e.StackTrace);
            }

            
            // Commenting this out because it seems to be breaking the rest of the fucntionality for the time being. Can fix later.
            /*
            foreach (var keyValuePair in idToPlayers)
            {
                if (keyValuePair.Value == null)
                {
                    GameObject.Destroy(goNametags[keyValuePair.Key]);
                    goNametags.Remove(keyValuePair.Key);
                    tmNametags.Remove(keyValuePair.Key);
                    tmNametags.Remove(keyValuePair.Key);
                    break; // Only removes one per frame. Potential optimization here.
                }
            }
            */

            foreach (FPPlayer fpp in Fp2Trainer.fpplayers)
            {
                if (fpp != null && !goNametags.ContainsKey(fpp.GetInstanceID()))
                {
                    GameObject goNametag = InstantiateNewNametag(fpp);
                    goNametags.Add(fpp.GetInstanceID(), goNametag);
                    tmNametags.Add(fpp.GetInstanceID(), goNametag.GetComponent<TextMesh>());
                    idToPlayers.Add(fpp.GetInstanceID(), fpp);
                }

                var go = goNametags[fpp.GetInstanceID()];
                var tm = tmNametags[fpp.GetInstanceID()];
                
                if (goStageCamera != null)
                {
                    posRelativeToCam = fpp.transform.position - goStageCamera.transform.position;
                    posRelativeToCam -= new Vector3(FPCamera.stageCamera.xpos, FPCamera.stageCamera.ypos, 0);
                    //posRelativeToCam += new Vector3(64, 64, 0);
                    
                    // Verticality is reversed here, this needs to subtract the height rather than add to be visible.
                    posRelativeToCam += new Vector3(FPCamera.stageCamera.xSize/2, -FPCamera.stageCamera.ySize/2, 0);
                    // Still has the side camera drift going on. Maybe I shouldn't be "relative to camera" at all if it produces floating affect to not move?
                }
                
                // Move it under the character.
                go.transform.position = posRelativeToCam + new Vector3(0, -128, 0);

                // TODO: This is the actual important text bit, don't forget to uncomment this.
                
                tm.text = Regex.Replace(tm.text, @"\(.+\)",
                    $"({String.Format("{0:0.00}",fpp.health)} / {String.Format("{0:0.00}",fpp.healthMax)})");
                
                
                
                /*
                this.transform.position = new Vector3(renderCamera.transform.position.x + (640f / 2f), 
                    renderCamera.transform.position.y - (360f / 2f), 
                    renderCamera.transform.position.z);
                */

                try
                {
                    var guessedWidth = tm.text.Length * tm.characterSize;
                    var guessedHeight = tm.characterSize;

                    go.transform.position += new Vector3(-guessedWidth / 2, 0, 0);



                    // Still bugged. Probably need to use some other way to gauge the bounds than the camera direction properties.
                    /*
                    if (go.transform.position.x + (guessedWidth / 2) > FPCamera.stageCamera.right)
                    {
                        go.transform.position += new Vector3(FPCamera.stageCamera.right - (guessedWidth / 2f), 0, 0);
                    }
                
                    if (go.transform.position.x - (guessedWidth / 2) < FPCamera.stageCamera.left)
                    {
                        go.transform.position += new Vector3(FPCamera.stageCamera.left + (guessedWidth / 2f), 0, 0);
                    }
                
                    if (go.transform.position.y + (guessedHeight / 2) > FPCamera.stageCamera.top)
                    {
                        go.transform.position += new Vector3(0, FPCamera.stageCamera.top - (guessedHeight / 2f), 0);
                    }
                
                    if (go.transform.position.y - (guessedHeight / 2) < FPCamera.stageCamera.bottom)
                    {
                        go.transform.position += new Vector3(0, FPCamera.stageCamera.bottom + (guessedHeight / 2f), 0);
                    }
                    */
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