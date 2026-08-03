using UnityEngine;

namespace MoonRabbitRush.Characters
{
    public static class CharacterSelectionSession
    {
        public static CharacterData SelectedCharacter { get; private set; }

        public static void Select(CharacterData character)
        {
            SelectedCharacter = character;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            SelectedCharacter = null;
        }
    }
}
