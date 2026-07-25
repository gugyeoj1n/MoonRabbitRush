using UnityEngine;

namespace MoonRabbitRush
{
    public class SceneManager
    {
        private static SceneManager instance;

        public static SceneManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new SceneManager();                

                return instance;
            }
        }

        private SceneManager()
        {
        }
    }
}
