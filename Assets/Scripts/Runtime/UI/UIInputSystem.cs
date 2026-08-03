using UnityEngine;
using UnityEngine.InputSystem;

namespace MoonRabbitRush
{
    public class UIInputSystem : MonoBehaviour
    {
        [SerializeField] 
        private InputActionReference pauseAction;

        private void OnEnable()
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPause;
        }

        private void OnDisable()
        {
            pauseAction.action.performed -= OnPause;
            pauseAction.action.Disable();
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            var popup = ManagerRoot.Instance.UIManager.GetPopup<PopupSystemUI>();
            if (popup == null)
                return;

            if (popup.gameObject.activeSelf)
            {
                ManagerRoot.Instance.UIManager.DisablePopup<PopupSystemUI>();
                Time.timeScale = 1f;
            }
            else
            {
                ManagerRoot.Instance.UIManager.EnablePopup<PopupSystemUI>();
                Time.timeScale = 0f;
            }            
        }
    }
}
