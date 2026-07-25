using UnityEngine;
using System;


namespace MoonRabbitRush
{
    public class Bootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var prefab = Resources.Load("ManagerObject");            
            if(prefab != null)
            {
                var obj = Instantiate(prefab);
                DontDestroyOnLoad(obj);
            }
        }
    }
}
