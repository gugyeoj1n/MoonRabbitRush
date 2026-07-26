using UnityEngine;

namespace MoonRabbitRush.Weapons.Active
{
    public sealed class WeaponActiveSkillHud : MonoBehaviour
    {
        [SerializeField] private WeaponActiveSkillController _controller;
        [SerializeField] private WeaponActiveSkillSlotView _slotPrefab;

        private void Start()
        {
            if (_controller == null)
            {
                _controller =
                    FindAnyObjectByType<WeaponActiveSkillController>();
            }

            if (_controller == null || _slotPrefab == null)
            {
                Debug.LogError("Active skill HUD references are incomplete.", this);
                return;
            }

            foreach (WeaponActiveSlot slot in _controller.Slots)
            {
                CreateSlot(slot);
            }

            _controller.SlotAdded += CreateSlot;
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.SlotAdded -= CreateSlot;
            }
        }

        private void CreateSlot(WeaponActiveSlot slot)
        {
            WeaponActiveSkillSlotView view = Instantiate(
                _slotPrefab,
                transform);
            view.Bind(slot);
        }
    }
}
