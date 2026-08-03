using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoonRabbitRush.Characters
{
    [CreateAssetMenu(
        fileName = "SO_CharacterCatalog",
        menuName = "Moon Rabbit Rush/Characters/Character Catalog")]
    public sealed class CharacterCatalog : ScriptableObject
    {
        [SerializeField] private CharacterData[] _characters;

        public IReadOnlyList<CharacterData> Characters =>
            _characters ?? Array.Empty<CharacterData>();
    }
}
