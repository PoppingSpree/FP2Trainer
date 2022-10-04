using MelonLoader;
using UnityEngine;

namespace Fp2Trainer
{
    public class SplitScreenCamInfo
    {
        public FPCamera FpCamera;
        public Camera RenderCamera;
        public GameObject GoRenderCamera;

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
    }
}