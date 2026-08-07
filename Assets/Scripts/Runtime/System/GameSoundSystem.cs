using UnityEngine;

namespace MoonRabbitRush
{
    public class GameSoundSystem : MonoBehaviour
    {
        private SoundManager _SoundManager => ManagerRoot.Instance.SoundManager;

        private void Start()
        {
            _SoundManager.PlayBGM("Audio/BGM/Start/Start_02");
        }

        private void OnDestroy()
        {
            _SoundManager.Stop("Audio/BGM/Start/Start_02");
        }


    }
}
