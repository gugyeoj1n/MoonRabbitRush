using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush
{
    public class PopupTitle : UIPopup
    {
        bool IsEnter;        
        float duration = 1f;
        [SerializeField]
        private GameObject BtnStart;
        [SerializeField]
        private GameObject BtnHowToPlay;
        [SerializeField]
        private GameObject BtnClickToStart;
        [SerializeField]
        private GameObject BtnSetting;
        [SerializeField]
        private GameObject FadeObject;
        [SerializeField]
        private TextMeshProUGUI TextClickToStart;
        private Image[] ImgClickToStart;


        private void Awake()
        {
            ImgClickToStart = FadeObject.GetComponentsInChildren<Image>(true);
        }

        private void Start()
        {
            if (IsEnter)
                return;

            FadeInOutText().Forget();
        }
        public void OnClickToStart()
        {
            IsEnter = true;
            BtnClickToStart.SetActive(false);
            BtnStart.SetActive(true);
            BtnHowToPlay.SetActive(true);
            BtnSetting.SetActive(true);
        }

        public void OnClickSelectCharacter()
        {
            Manager.EnablePopup<PopupSelectCharacter>();
        }

        public void OnClickHowToPlay()
        {

        }

        public void OnClickSetting()
        {
            Manager.EnablePopup<PopupSetting>();
        }

        private async UniTaskVoid FadeInOutText()
        {
            try
            {
                while (!IsEnter)
                {
                    List<UniTask> tasks = new List<UniTask>();
                    foreach(var img in ImgClickToStart)
                    {
                        tasks.Add(FadeOutImage(duration, img));
                    }
                    tasks.Add(FadeOutText(duration, TextClickToStart));
                    await UniTask.WhenAll(tasks);
                    tasks.Clear();                    
                    foreach (var img in ImgClickToStart)
                    {
                        tasks.Add(FadeInImage(duration, img));
                    }
                    tasks.Add(FadeInText(duration, TextClickToStart));
                    await UniTask.WhenAll(tasks);
                }
            }
            catch
            {

            }            
        }

        private async UniTask FadeInImage(float duration, Image image)
        {
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

                    await UniTask.NextFrame();
                }
            }
            finally
            {
                color.a = 1f;
                image.color = color;
            }
        }

        private async UniTask FadeOutImage(float duration, Image image)
        {
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

                    await UniTask.NextFrame();
                }
            }
            finally
            {
                color.a = 0f;
                image.color = color;
            }
        }

        private async UniTask FadeOutText(float duration, TextMeshProUGUI text)
        {
            Color color = text.color;
            float startAlpha = color.a;
            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;

                    color.a = Mathf.Lerp(startAlpha, 0f, elapsed / duration);

                    text.color = color;

                    await UniTask.NextFrame();
                }
            }
            finally
            {
                color.a = 0f;
                text.color = color;
            }
        }

        private async UniTask FadeInText(float duration, TextMeshProUGUI text)
        {
            Color color = text.color;
            float startAlpha = color.a;
            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;

                    color.a = Mathf.Lerp(startAlpha, 1f, elapsed / duration);

                    text.color = color;

                    await UniTask.NextFrame();
                }
            }
            finally
            {
                color.a = 1f;
                text.color = color;
            }
        }
    }
}
