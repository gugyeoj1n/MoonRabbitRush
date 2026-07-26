using System.Globalization;
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

        public void Bind(WeaponActiveSlot slot)
        {
            _slot = slot;
            _weaponIcon.sprite = slot.Data.Icon;
            _weaponIcon.enabled = slot.Data.Icon != null;
            _keyBindText.text = slot.KeyLabel;
            Refresh();
        }

        private void Update()
        {
            Refresh();
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
    }
}
