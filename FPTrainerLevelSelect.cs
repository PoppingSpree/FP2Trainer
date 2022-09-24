using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

namespace Fp2Trainer
{
    public class FPTrainerLevelSelect : UnityEngine.MonoBehaviour
    {
        public int menuSelection = 1;
        public int buttonCount = 0;
        public GameObject[] pfButtons;

        public List<SceneNamePair> availableScenes;
        public int state = 0;
        public float yOffset = 0;
        public float[] targetX;
        public float[] startY;
        public float xOffsetSelected = 0;
        private float xOffsetRegular = 0;

        public void Start()
        {
            menuSelection = 1;
            buttonCount = pfButtons.Length;
        }

        public void ShowLevelSelectMenu()
        {
            menuSelection = 1;
            FPStage.SetStageRunning(pauseFlag: false);
            transform.gameObject.SetActive(true);
        }

        public void UpdateButtonCount()
        {
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
            else if (/*FPStage.menuInput.confirm || */InputControl.GetButtonDown(Controls.buttons.jump))
            {
                FPScreenTransition component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
                component.transitionType = FPTransitionTypes.WIPE;
                component.transitionSpeed = 48f;
                string scenePath = this.availableScenes[menuSelection].path;
                component.sceneToLoad = scenePath;
                //component.sceneToLoad = GetSceneNameByIndex(menuSelection);
                global::Fp2Trainer.Fp2Trainer.Log("Attempting to load scene: "
                                                  + component.sceneToLoad + " => " 
                                                  + this.availableScenes[menuSelection].name + " => " 
                                                  + this.availableScenes[menuSelection].path);
                
                global::Fp2Trainer.Fp2Trainer.Log("Can Load?: "
                                                  + Application.CanStreamedLevelBeLoaded(component.sceneToLoad).ToString());
                component.SetTransitionColor(0f, 0f, 0f);
                component.BeginTransition();
                FPAudio.PlayMenuSfx(3);
                
                FPAudio.PlayMenuSfx(2);
            }
            
            UpdateMenuPosition();
        }
        
        private void ResetY()
        {
            if (state == 0)
            {
                yOffset = (float)(menuSelection - 4 + 1) * 32f;
            }
            else
            {
                yOffset = (float)buttonCount * -8f + 16f;
            }
            for (int i = 0; i < buttonCount; i++)
            {
                float x = targetX[i];
                float y = startY[i] + yOffset;
                Vector3 position = pfButtons[i].transform.position;
                float z = position.z;
                pfButtons[i].transform.position = new Vector3(x, y, z);
            }
        }

        public string GetSceneNameByIndex(int ind)
        {
            return System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility
                .GetScenePathByBuildIndex(ind));
        }
        
        private void UpdateMenuPosition()
		{
			float num = 5f;
			float[] array = new float[4]
			{
				1f,
				1f,
				0.5f,
				0f
			};
			float[] array2 = new float[buttonCount];
			int num2 = menuSelection;
			for (int i = 0; i < array.Length; i++)
			{
				array2[num2] = array[i];
				num2++;
				if (num2 >= buttonCount)
				{
					break;
				}
			}
			num2 = menuSelection;
			for (int j = 0; j < array.Length; j++)
			{
				array2[num2] = array[j];
				num2--;
				if (num2 < 0)
				{
					break;
				}
			}
			if (state == 0)
			{
				yOffset = (float)(menuSelection - 4 + 1) * 32f;
			}
			else
			{
				yOffset = (float)buttonCount * -8f + 16f;
			}
			for (int k = 0; k < buttonCount; k++)
			{
				Vector3 position = pfButtons[k].transform.position;
				float num3 = (position.x * (num - 1f) + targetX[k]) / num;
				Vector3 position2 = pfButtons[k].transform.position;
				float y = (position2.y * (num - 1f) + startY[k] + yOffset) / num;
				Vector3 position3 = pfButtons[k].transform.position;
				float z = position3.z;
				pfButtons[k].transform.position = new Vector3(num3, y, z);
				if (state == 0 && k == menuSelection)
				{
					//cursor.transform.position = new Vector3(num3 - 32f, y, z);
				}
				targetX[k] = 320f + xOffsetRegular;
				if (state == 0)
				{
					
					//pfText[k].textMesh.text = pfText[k].paragraph[0];
					
					if (k == menuSelection)
					{
						//Transform transform = pfTextBox.transform;
						Vector3 position4 = pfButtons[k].transform.position;
						float x = position4.x + 152f;
						Vector3 position5 = pfButtons[k].transform.position;
						float y2 = position5.y;
						//Vector3 position6 = pfTextBox.transform.position;
						//transform.position = new Vector3(x, y2, position6.z);
						targetX[menuSelection] = 320f + xOffsetSelected;
					}
				}
			}
		}
    }
}