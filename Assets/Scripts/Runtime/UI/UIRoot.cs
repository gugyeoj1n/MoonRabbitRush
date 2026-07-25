using UnityEngine;

namespace MoonRabbitRush
{
    public class UIRoot : MonoBehaviour
    {
        private UIManager Manager => ManagerRoot.Instance.UIManager;
        private void Start()
        {
            var group = GetComponentsInChildren<UIPopup>(true);
            foreach(var ui in group)
            {
                Manager.RegisterUI(ui);
            }
        }
    }
}
