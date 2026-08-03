using Codice.Utils;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace MoonRabbitRush
{
    public class PopupSystemUI : UIPopup
    {
        private CancellationTokenSource _tokenSource;
        public void OnClickResume()
        {
            OnClickClose();
        }

        public void OnClickHome()
        {
            if (_tokenSource != null && !_tokenSource.IsCancellationRequested)
                return;

            var cancel = new CancellationTokenSource();
            _tokenSource = cancel;

            UniTask.Void(async token =>
            {
                try
                {
                    await ManagerRoot.Instance.SceneManager.TransitionTo(0, token);
                }
                finally
                {
                    cancel.Dispose();
                    _tokenSource = null;
                }
            }, cancel.Token);
        }

        public override void OnClickClose()
        {
            base.OnClickClose();
            Time.timeScale = 1f;
        }
    }
}
