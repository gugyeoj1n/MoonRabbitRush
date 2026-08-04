using MoonRabbitRush.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush
{
    public sealed class CurrentWeaponIconView : MonoBehaviour
    {
        [SerializeField] private Image _weaponImage;
        [SerializeField] private TextMeshProUGUI _levelText;

        public void Bind(WeaponData weapon, int level)
        {
            if (_weaponImage != null)
            {
                _weaponImage.sprite = weapon != null ? weapon.Icon : null;
                _weaponImage.enabled = _weaponImage.sprite != null;
            }

            if (_levelText != null)
            {
                _levelText.SetText("{0}", Mathf.Max(1, level));
            }
        }
    }
}
