using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace MoonRabbitRush
{
    public class PopupTest : UIPopup
    {
        private CancellationTokenSource _cancelToken;
        
        public void OnClickTest()
        {            
            if (_cancelToken != null)
                return;

            Debug.Log("On Click Test");

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
