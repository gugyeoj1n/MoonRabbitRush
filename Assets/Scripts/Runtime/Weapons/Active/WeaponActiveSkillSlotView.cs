using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush.Weapons.Active
{
    public sealed class WeaponActiveSkillSlotView : MonoBehaviour
    {
        [SerializeField] private Image _weaponIcon;
        [SerializeField] private GameObject _cooldownOverlay;
        [SerializeField] private TMP_Text _cooldownText;
        [SerializeField] private TMP_Text _keyBindText;

        private WeaponActiveSlot _slot;
        private CancellationTokenSource _refreshCts;

        public void Bind(WeaponActiveSlot slot)
        {
            _slot = slot;
            _weaponIcon.sprite = slot.Data.Icon;
            _weaponIcon.enabled = slot.Data.Icon != null;
            _keyBindText.text = slot.KeyLabel;
            Refresh();
            RestartRefreshLoop();
        }

        private void OnEnable()
        {
            RestartRefreshLoop();
        }

        private void OnDisable()
        {
            CancelRefreshLoop();
        }

        private void Refresh()
        {
            if (_slot == null)
            {
                return;
            }

            bool isCoolingDown = _slot.IsCoolingDown;
            _cooldownOverlay.SetActive(isCoolingDown);

            if (isCoolingDown)
            {
                _cooldownText.text = _slot.CooldownRemaining.ToString(
                    "00.00",
                    CultureInfo.InvariantCulture);
            }
        }

        private void RestartRefreshLoop()
        {
            if (!isActiveAndEnabled || _slot == null)
            {
                return;
            }

            CancelRefreshLoop();
            _refreshCts = CancellationTokenSource.CreateLinkedTokenSource(
                destroyCancellationToken);
            RefreshLoopAsync(_refreshCts.Token).Forget();
        }

        private void CancelRefreshLoop()
        {
            if (_refreshCts == null)
            {
                return;
            }

            _refreshCts.Cancel();
            _refreshCts.Dispose();
            _refreshCts = null;
        }

        private async UniTaskVoid RefreshLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Refresh();
                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    cancellationToken);
            }
        }
    }
}
