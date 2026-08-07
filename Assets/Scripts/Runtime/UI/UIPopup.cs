using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoonRabbitRush
{
    public class UIPopup : MonoBehaviour
    {
        protected UIManager Manager => ManagerRoot.Instance.UIManager;

        private readonly UnityAction playClickSound = () => ManagerRoot.Instance.SoundManager.Play("Audio/SFX/Click/Click_01");

        protected virtual void Awake()
        {
            RegisterButtonSound();
        }

        private void RegisterButtonSound()
        {
            var buttons = GetComponentsInChildren<Button>(true);

            var soundManager = ManagerRoot.Instance.SoundManager;
            
            foreach (var button in buttons)
            {
                button.onClick.RemoveListener(playClickSound);
                button.onClick.AddListener(playClickSound);
            }
        }

        public void EnablePopup(Type type)
        {
            var popup = Manager.GetPopup(type);
            if (popup != null)
            {
                popup.gameObject.SetActive(true);
            }
        }

        public void DisablePopup(Type type)
        {
            var popup = Manager.GetPopup(type);
            if (popup != null)
            {
                popup.gameObject.SetActive(false);
            }
        }        

        public virtual void OnClickClose()
        {
            gameObject.SetActive(false);            
        }                
    }
}
