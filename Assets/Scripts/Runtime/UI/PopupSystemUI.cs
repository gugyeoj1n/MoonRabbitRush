using Codice.Utils;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace MoonRabbitRush
{
    public class PopupSystemUI : UIPopup
    {
        private CancellationTokenSource _cancelToken;
        public void OnClickResume()
        {

        }

        public void OnClickHome()
        {
            if (_cancelToken != null && !_cancelToken.IsCancellationRequested)
                return;

            var cancel = new CancellationTokenSource();
            _cancelToken = cancel;

            UniTask.Void(async () =>
            {
                try
                {
                    await ManagerRoot.Instance.SceneManager.TransitionTo(0, cancel.Token);
                }
                finally
                {
                    cancel.Dispose();
                    _cancelToken = null;
                }
            });
        }

        public override void OnClickClose()
        {
            base.OnClickClose();
            Time.timeScale = 1f;
        }
    }
}
