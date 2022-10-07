using UnityEngine;

namespace Fp2Trainer
{
    public class FP2TrainerYourPlayerIndicator
    {
        public static GameObject templatePrimCube;
        public static GameObject templateYourCharacterIndicator;
        public static int numIndicatorsCreated = 0;
        public SpriteRenderer spriteRenderer;
        public Sprite sprite;

        public static GameObject CreateFPYourPlayerIndicator(string newName, Vector3 newPosition, Quaternion newRotation, Transform newParent)
        {
            templateYourCharacterIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube); //Replace this with the menu arrow sprite.
            bool stageListPosValidated = false;
            var newPlayerIndicator = FPStage.InstantiateFPBaseObject(templateYourCharacterIndicator, out stageListPosValidated);
            
            numIndicatorsCreated++;
            newPlayerIndicator.name = "Player Indicator " + numIndicatorsCreated.ToString();

            return newPlayerIndicator;
        }
    }
}