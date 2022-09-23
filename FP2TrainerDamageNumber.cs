using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fp2Trainer
{
    public class FP2TrainerDamageNumber : MonoBehaviour
    {
        public float dmg = 0;
        public bool showAsInt = true;
        public float dt = 0;
        public float timeRemainingMax = 2f;
        public float timeRemaining = 2f;

        public Vector2 pos = new Vector2(0, 0);
        public Vector2 vel = new Vector2(0, 1);
        public Vector2 grav = new Vector2(0, 0.34f);

        public TextMesh tm = null;
        public void Start()
        {
            timeRemaining = timeRemainingMax;
        }

        public void Update()
        {
            timeRemaining -= FPStage.frameTime;

            if (timeRemaining <= 0)
            {
                GameObject.Destroy(this.gameObject);
            }
            else
            {
                dt = FPStage.frameTime;
                vel -= (grav * dt);
                Vector3 p = this.transform.position;
                this.transform.position = new Vector3(p.x + (vel.x * dt), p.y + (vel.y * dt), p.z);
            }
        }

        public static GameObject CreateDMGNumberObject(Vector3 newPos, float dmgNum)
        {
            GameObject goDmgNum = null;
            var goStageHUD = GameObject.Find("Stage HUD");
            
            if (goStageHUD != null)
            {
                //goStageHUD.energyBarGraphic.transform.parent;
                global::Fp2Trainer.Fp2Trainer.Log("Looking for Energy Bar");
                var temp = goStageHUD.GetComponent<FPHudMaster>();
                GameObject temp2;
                global::Fp2Trainer.Fp2Trainer.Log("6");
                if (temp != null)
                {
                    global::Fp2Trainer.Fp2Trainer.Log("7");
                    temp2 = temp.pfHudEnergyBar;
                }
                else
                {
                    global::Fp2Trainer.Fp2Trainer.Log("8");
                    global::Fp2Trainer.Fp2Trainer.Log("This aint it.");
                    return goDmgNum;
                }
                
                
                global::Fp2Trainer.Fp2Trainer.Log("9");
                var energyBarGraphic = UnityEngine.Object.Instantiate(temp2, temp2.transform.parent);
                
                energyBarGraphic.transform.localScale *= 2;
                
                var goNewDmgNum = energyBarGraphic;
                goNewDmgNum.SetActive(true);
                //GameObject.Destroy(goNewDmgNum.GetComponent<SpriteRenderer>()); // Can't have Sprite Renderer and Mesh Renderer.
                var tempGo = new GameObject();
                tempGo.transform.parent = goNewDmgNum.transform;
                tempGo.transform.localPosition = Vector3.zero;
                
                global::Fp2Trainer.Fp2Trainer.Log("10");

                goNewDmgNum.transform.position = new Vector3(16, -160, 0);
                goNewDmgNum = tempGo;
                
                global::Fp2Trainer.Fp2Trainer.Log("11");
                
                var textMeshDmgNum = goNewDmgNum.AddComponent<TextMesh>();
                if (textMeshDmgNum != null)
                {
                    global::Fp2Trainer.Fp2Trainer.Log("12");
                    var fpMenuFont = Fp2Trainer.fpMenuFont;
                    global::Fp2Trainer.Fp2Trainer.Log("Current value of fpMenuFont: " + fpMenuFont);
                    textMeshDmgNum.font = fpMenuFont;
                    textMeshDmgNum.characterSize = 10;
                    global::Fp2Trainer.Fp2Trainer.Log("13");
                     
                    //textMeshDmgNum.text = "I exist!@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n@@@@@@@@@@@@@@@@@@@@@@@@";
                    //global::Fp2Trainer.Fp2Trainer.LogLog("Creating dmg number Attaching to Stage HUD.");
                }
                else
                {
                    global::Fp2Trainer.Fp2Trainer.Log("Tried to create textMesh but failed.");
                }
                
                global::Fp2Trainer.Fp2Trainer.Log("14");

                var fp2Tdmg = goNewDmgNum.AddComponent<FP2TrainerDamageNumber>();
                fp2Tdmg.tm = textMeshDmgNum;
                fp2Tdmg.dmg = dmgNum;
                fp2Tdmg.tm.text = fp2Tdmg.dmg.ToString();

                global::Fp2Trainer.Fp2Trainer.Log("15");
                goDmgNum = goNewDmgNum;
                goDmgNum.transform.position = newPos;
            }
            else
            {
                global::Fp2Trainer.Fp2Trainer.Log("16");
                goDmgNum = new GameObject();
            }
            
            global::Fp2Trainer.Fp2Trainer.Log("17");

            return goDmgNum;
        }
    }
}