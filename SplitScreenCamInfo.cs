using MelonLoader;
using UnityEngine;

namespace Fp2Trainer
{
    public class SplitScreenCamInfo
    {
        public FPCamera FpCamera;
        public Camera RenderCamera;
        public GameObject GoRenderCamera;
        
        //cam order: quartertopleft, quaterbottomleft, quatertopright, quater bottomright;
        public static UnityEngine.Rect TopLeftQuarter = new Rect(0,0.5f, 0.5f, 0.5f);
        public static UnityEngine.Rect BottomLeftQuarter = new Rect(0,0f, 0.5f, 0.5f);
        public static UnityEngine.Rect TopRightQuarter = new Rect(0.5f,0.5f, 0.5f, 0.5f);
        public static UnityEngine.Rect BottomRightQuarter = new Rect(0.5f,0f, 0.5f, 0.5f);
        
        public static UnityEngine.Rect TopHalf = new Rect(0f,0.5f, 1f, 0.5f);
        public static UnityEngine.Rect BottomHalf = new Rect(0f,0f, 1f, 0.5f);
        
        public static UnityEngine.Rect SingleFull = new Rect(0f,0f, 1f, 1f);

        public SplitScreenCamInfo(FPCamera newFpCamScript, GameObject goRenderCamera)
        {
            this.FpCamera = newFpCamScript;
            this.GoRenderCamera = goRenderCamera;
            this.RenderCamera = goRenderCamera.GetComponent<Camera>();
            if (this.RenderCamera == null)
            {
                MelonLogger.Warning("Null render cam in split screen info, try fetching later.");
            }
        }

        public static Rect GetCamRectByPlayerIndexAndCount(int playerIndex = 0, int playerNum = 1)
        {
            if (playerNum <= 1)
            {
                return SingleFull;
            }
            else if (playerNum == 2)
            {
                if (playerIndex == 0)
                {
                    return TopHalf;
                }
                else
                {
                    return TopHalf;
                }
            }

            else if (playerNum == 3)
            {
                if (playerIndex == 0)
                {
                    return TopHalf;
                }
                else if (playerIndex == 1)
                {
                    return BottomLeftQuarter;
                }
                else
                {
                    return BottomRightQuarter;
                }
            }
            else if (playerNum >= 4)
            {
                if (playerIndex == 0)
                {
                    return TopLeftQuarter;
                }
                else if (playerIndex == 1)
                {
                    return BottomLeftQuarter;
                }
                else if (playerIndex == 2)
                {
                    return TopRightQuarter;
                }
                else
                {
                    return BottomRightQuarter;
                }
            }

            return SingleFull;
        }
    }
}