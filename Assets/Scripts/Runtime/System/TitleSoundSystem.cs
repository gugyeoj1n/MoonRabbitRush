using UnityEngine;

namespace MoonRabbitRush
{
    public class TitleSoundSystem : MonoBehaviour
    {
        private SoundManager _SoundManager => ManagerRoot.Instance.SoundManager;

        private void Start()
        {
            _SoundManager.PlayBGM("Audio/BGM/Start/Start_01");
        }

        private void OnDestroy()
        {
            _SoundManager.Stop("Audio/BGM/Start/Start_01");
        }
    }
}
