using UnityEngine;

namespace Fp2Trainer
{
    public class TrainerPauseMenu : MonoBehaviour
    {
        public static void GrabAndTweakPauseMenu()
        {
            //var goPauseMenu = GameObject.Find("Hud Pause Menu");
            //var goPauseSettings = GameObject.Find("Pause Icon - Settings");


            GameObject goPauseMenu;
            foreach (FPPauseMenu fpPauseMenu in Resources.FindObjectsOfTypeAll<FPPauseMenu>())
            {
                goPauseMenu = fpPauseMenu.gameObject;
                break;
            }
            /*var goPauseSettings = goPauseMenu.Find("Pause Icon - Settings");

            if (goPauseMenu != null && goPauseSettings != null)
            {
                var goTrainer = GameObject.Instantiate(goPauseSettings);
                goTrainer.transform.position = goPauseSettings.transform.position + new Vector3(0, 32 * 5, 0);
                goTrainer.name = "Pause Icon - Trainer";
                goTrainer.GetComponent<TextMesh>().text = "Trainer Menu";
                goTrainer.transform.SetParent(goPauseMenu.transform);
            }
            */

            
        }
    }
}