using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MoonRabbitRush
{
    public class UIManager : MonoBehaviour
    {
        public Canvas MainCanvas => _Canvas;
        private Canvas _Canvas;
        Dictionary<Type, UIPopup> PopupHash = new Dictionary<Type, UIPopup>();        

        public UIPopup GetPopup(Type type) => PopupHash[type];
        public T GetPopup<T>() where T : UIPopup
        {
            if (PopupHash.TryGetValue(typeof(T), out var popup))
            {
                return popup as T;
            }

            return null;
        }

        private void Awake()
        {
            _Canvas = GetComponent<Canvas>();
            _Canvas.overrideSorting = true;
            _Canvas.sortingOrder = 100;
            var group = GetComponentsInChildren<UIPopup>(true);
            foreach (var ui in group)
            {
                RegisterUI(ui);
            }
        }

        public void RegisterUI(UIPopup popup)
        {
            var type = popup.GetType();
            PopupHash[type] = popup;
        }

        public void EnablePopup(Type type)
        {
            PopupHash[type].gameObject.SetActive(true);
        }

        public void DisablePopup(Type type)
        {
            PopupHash[type].gameObject.SetActive(false);
        }
    }
}
