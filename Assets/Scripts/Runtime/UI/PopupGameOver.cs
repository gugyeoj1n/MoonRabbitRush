using UnityEngine;

namespace MoonRabbitRush
{
    public class PopupGameOver : UIPopup
    {
        private void OnEnable()
        {
            ManagerRoot.Instance.SoundManager.Play("Audio/SFX/GameOver/GameOver_01");
            ManagerRoot.Instance.SoundManager.Stop("Audio/BGM/Start/Start_02");
        }
    }
}
