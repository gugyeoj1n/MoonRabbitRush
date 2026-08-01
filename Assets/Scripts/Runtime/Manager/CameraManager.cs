using UnityEngine;

namespace MoonRabbitRush
{
    public class CameraMaanger : MonoBehaviour
    {
        public Camera MainCamera { get; private set; }

        private void Awake()
        {
            MainCamera = GetComponent<Camera>();

            if (MainCamera == null)
            {
                MainCamera = GetComponentInChildren<Camera>();
            }
        }
    }
}
