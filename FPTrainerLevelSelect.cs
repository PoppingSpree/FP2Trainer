using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fp2Trainer
{
    public class FPTrainerLevelSelect : MonoBehaviour
    {
        public int menuSelection = 1;
        public int buttonCount;
        public GameObject[] pfButtons;
        public int state;
        public float yOffset;
        public float[] targetX;
        public float[] startY;
        public float xOffsetSelected;

        public List<SceneNamePair> availableScenes;
        private readonly float xOffsetRegular = 0;

        public void Start()
        {
            menuSelection = 1;
            buttonCount = pfButtons.Length;
        }

        public void Update()
        {
            if (FPStage.menuInput.cancel || InputControl.GetButtonDown(Controls.buttons.pause))
            {
                //FPStage.SetStageRunning(pauseFlag: true, showPauseMenu: false);
                /*
                if (transform.gameObject.activeInHierarchy)
                {
                    FPStage.SetStageRunning(pauseFlag: true, showPauseMenu: false);
                    transform.gameObject.SetActive(!transform.gameObject.activeInHierarchy);
                }
                */
            }
            else if (FPStage.menuInput.up || InputControl.GetButtonDown(Controls.buttons.up))
            {
                menuSelection--;
                if (menuSelection < 0)
                {
                    menuSelection += buttonCount;
                    ResetY();
                }

                FPAudio.PlayMenuSfx(1);
            }
            else if (FPStage.menuInput.down || InputControl.GetButtonDown(Controls.buttons.down))
            {
                menuSelection++;
                if (menuSelection >= buttonCount)
                {
                    menuSelection -= buttonCount;
                    ResetY();
                }

                FPAudio.PlayMenuSfx(1);
            }
            else if ( /*FPStage.menuInput.confirm || */InputControl.GetButtonDown(Controls.buttons.jump))
            {
                var component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
                component.transitionType = FPTransitionTypes.WIPE;
                component.transitionSpeed = 48f;
                var scenePath = availableScenes[menuSelection].path;
                component.sceneToLoad = scenePath;
                //component.sceneToLoad = GetSceneNameByIndex(menuSelection);
                Fp2Trainer.Log("Attempting to load scene: "
                               + component.sceneToLoad + " => "
                               + availableScenes[menuSelection].name + " => "
                               + availableScenes[menuSelection].path);

                Fp2Trainer.Log("Can Load?: "
                               + Application.CanStreamedLevelBeLoaded(component.sceneToLoad));
                component.SetTransitionColor(0f, 0f, 0f);
                component.BeginTransition();
                FPAudio.PlayMenuSfx(3);

                FPAudio.PlayMenuSfx(2);
            }

            UpdateMenuPosition();
        }

        public void ShowLevelSelectMenu()
        {
            menuSelection = 1;
            FPStage.SetStageRunning(false);
            transform.gameObject.SetActive(true);
        }

        public void UpdateButtonCount()
        {
            buttonCount = pfButtons.Length;
        }

        private void ResetY()
        {
            if (state == 0)
                yOffset = (menuSelection - 4 + 1) * 32f;
            else
                yOffset = buttonCount * -8f + 16f;
            for (var i = 0; i < buttonCount; i++)
            {
                var x = targetX[i];
                var y = startY[i] + yOffset;
                var position = pfButtons[i].transform.position;
                var z = position.z;
                pfButtons[i].transform.position = new Vector3(x, y, z);
            }
        }

        public string GetSceneNameByIndex(int ind)
        {
            return Path.GetFileNameWithoutExtension(SceneUtility
                .GetScenePathByBuildIndex(ind));
        }

        private void UpdateMenuPosition()
        {
            var num = 5f;
            var array = new float[4]
            {
                1f,
                1f,
                0.5f,
                0f
            };
            var array2 = new float[buttonCount];
            var num2 = menuSelection;
            for (var i = 0; i < array.Length; i++)
            {
                array2[num2] = array[i];
                num2++;
                if (num2 >= buttonCount) break;
            }

            num2 = menuSelection;
            for (var j = 0; j < array.Length; j++)
            {
                array2[num2] = array[j];
                num2--;
                if (num2 < 0) break;
            }

            if (state == 0)
                yOffset = (menuSelection - 4 + 1) * 32f;
            else
                yOffset = buttonCount * -8f + 16f;
            for (var k = 0; k < buttonCount; k++)
            {
                var position = pfButtons[k].transform.position;
                var num3 = (position.x * (num - 1f) + targetX[k]) / num;
                var position2 = pfButtons[k].transform.position;
                var y = (position2.y * (num - 1f) + startY[k] + yOffset) / num;
                var position3 = pfButtons[k].transform.position;
                var z = position3.z;
                pfButtons[k].transform.position = new Vector3(num3, y, z);
                if (state == 0 && k == menuSelection)
                {
                    //cursor.transform.position = new Vector3(num3 - 32f, y, z);
                }

                targetX[k] = 320f + xOffsetRegular;
                if (state == 0)
                    //pfText[k].textMesh.text = pfText[k].paragraph[0];
                    if (k == menuSelection)
                    {
                        //Transform transform = pfTextBox.transform;
                        var position4 = pfButtons[k].transform.position;
                        var x = position4.x + 152f;
                        var position5 = pfButtons[k].transform.position;
                        var y2 = position5.y;
                        //Vector3 position6 = pfTextBox.transform.position;
                        //transform.position = new Vector3(x, y2, position6.z);
                        targetX[menuSelection] = 320f + xOffsetSelected;
                    }
            }
        }
    }
}