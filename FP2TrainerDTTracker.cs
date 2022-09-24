using UnityEngine;

namespace Fp2Trainer
{
    public class FP2TrainerDTTracker : MonoBehaviour
    {
        public float dt;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void LateUpdate()
        {
            dt = FPStage.frameTime;
            Fp2Trainer.SetFP2TDeltaTime(dt);
            Fp2Trainer.fp2TrainerInstance.OnGameObjectUpdate();
        }
    }
}