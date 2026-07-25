using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush
{
    public class PopupForeGround : UIPopup
    {
        [SerializeField]
        private Image FadeImage;
        private RectTransform rect;
        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void OnEnable()
        {
            FadeImage.raycastTarget = false;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;
        }

        public void OnDisable()
        {
            FadeImage.raycastTarget = true;
        }

        public void OnDisableClick()
        {
            FadeImage.raycastTarget = true;
        }

        public void OnEnableClick()
        {
            FadeImage.raycastTarget = false;
        }
        public async UniTask FadeIn(float duration, CancellationToken token)
        {
            var image = FadeImage;
            image.raycastTarget = true;
            Color color = image.color;
            float startAlpha = color.a;
            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;

                    color.a = Mathf.Lerp(startAlpha, 0f, elapsed / duration);

                    image.color = color;

                    await UniTask.NextFrame(token);
                }
            }
            finally
            {
                color.a = 0f;
                image.color = color;
                image.raycastTarget = false;
            }
        }

        public async UniTask FadeOut(float duration, CancellationToken token)
        {
            var image = FadeImage;
            image.raycastTarget = true;
            Color color = image.color;
            float startAlpha = color.a;
            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;

                    color.a = Mathf.Lerp(startAlpha, 1f, elapsed / duration);

                    image.color = color;

                    await UniTask.NextFrame(token);
                }
            }
            finally
            {
                color.a = 1f;
                image.color = color;
                image.raycastTarget = false;
            }
        }
    }
}
