using System;
using System.Threading;
using UnityEngine;

namespace MoonRabbitRush
{
    public class UIPopup : MonoBehaviour
    {
        protected UIManager Manager => ManagerRoot.Instance.UIManager;        

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
