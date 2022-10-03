using UnityEngine;
using System.Collections.Generic;

namespace Fp2Trainer
{
    public class FP2TrainerCharacterNameTag : MonoBehaviour
    {
        public static FP2TrainerCharacterNameTag instance;
        public Dictionary<int, GameObject> goNametags;
        public Dictionary<int, TextMesh> tmNametags;
        private List<string> placeholderNames;
        
        [Header("Screen Boundaries")]
        public bool top;

        public bool left;

        public bool right;

        public bool bottom;
        
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
                go.transform.position = new Vector3(fpp.transform.position.x, fpp.transform.position.y + 32f, go.transform.position.z );

                var guessedWidth = tm.text.Length * tm.characterSize;
                var guessedHeight = tm.characterSize;
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
                    tm.text = placeholderNames[index];
                }
            }

            return goNametag;
        }
    }
}