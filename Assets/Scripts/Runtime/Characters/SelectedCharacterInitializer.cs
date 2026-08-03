using MoonRabbitRush.Player;
using MoonRabbitRush.Weapons;
using UnityEngine;

namespace MoonRabbitRush.Characters
{
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(WeaponController))]
    public sealed class SelectedCharacterInitializer : MonoBehaviour
    {
        [SerializeField] private CharacterData _defaultCharacter;

        public CharacterData CurrentCharacter { get; private set; }

        private void Awake()
        {
            CurrentCharacter =
                CharacterSelectionSession.SelectedCharacter != null
                    ? CharacterSelectionSession.SelectedCharacter
                    : _defaultCharacter;

            if (CurrentCharacter == null || !CurrentCharacter.IsValid)
            {
                Debug.LogError("Selected character data is incomplete.", this);
                return;
            }

            GetComponent<PlayerMovement>().Configure(CurrentCharacter.Stats);
            GetComponent<PlayerHealth>().Configure(CurrentCharacter.Stats);
            GetComponent<WeaponController>().ConfigureStartingWeapon(
                CurrentCharacter.StartingWeapon);
        }
    }
}
