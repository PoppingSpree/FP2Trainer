using UnityEngine.SceneManagement;

namespace Fp2Trainer
{
    public class SceneNamePair
    {
        public string name = "";
        public Scene scene;

        public SceneNamePair(Scene theScene, string theName)
        {
            this.name = theName;
            this.scene = theScene;
        }
    }
}