using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace MoonRabbitRush
{
    public class PopupSelectCharacter : UIPopup
    {
        private CancellationTokenSource _cancelToken;
        public void OnClickStart()
        {
            if (_cancelToken != null)
                return;

            _cancelToken = new CancellationTokenSource();

            UniTask.Void(async () =>
            {
                try
                {
                    await ManagerRoot.Instance.SceneManager.TransitionTo(1, _cancelToken.Token);
                }
                finally
                {
                    _cancelToken.Dispose();
                    _cancelToken = null;
                }
            });
        }

    }
}
