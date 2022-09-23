using UnityEngine.SceneManagement;

namespace Fp2Trainer
{
    public class SceneNamePair
    {
        public string name = "";
        public Scene scene;
        public string path = "PATH NOT SET";

        public SceneNamePair(Scene theScene, string theName)
        {
            this.name = theName;
            this.scene = theScene;
            this.path = theName;
        }
        public SceneNamePair(Scene theScene, string theName, string thePath)
        {
            this.name = theName;
            this.scene = theScene;
            this.path = thePath;
        }

        public override string ToString()
        {
            return this.name + " ->" + this.path;
        }
    }
}