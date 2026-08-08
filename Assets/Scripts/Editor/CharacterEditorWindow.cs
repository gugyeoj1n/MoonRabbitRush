using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonRabbitRush.Characters;
using UnityEditor;
using UnityEngine;

namespace MoonRabbitRush.Editor.Characters
{
    public sealed class CharacterEditorWindow : EditorWindow
    {
        private const string DataDirectory = "Assets/Data/Characters";
        private const string CatalogPath =
            DataDirectory + "/SO_CharacterCatalog.asset";
        private const string SelectedAssetSessionKey =
            "MoonRabbitRush.CharacterEditor.SelectedAsset";
        private const float ListWidth = 290f;

        private readonly List<CharacterData> _characters = new();

        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private string _searchText = string.Empty;
        private string _assetName = string.Empty;
        private CharacterData _selectedCharacter;
        private SerializedObject _serializedCharacter;

        [MenuItem("Moon Rabbit Rush/Characters/Character Editor")]
        public static void Open()
        {
            CharacterEditorWindow window = GetWindow<CharacterEditorWindow>();
            window.titleContent = new GUIContent(
                "Character Editor",
                "Create, edit, and validate selectable characters.");
            window.minSize = new Vector2(780f, 520f);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.projectChanged += HandleProjectChanged;
            RefreshCharacters(restoreSelection: true);
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= HandleProjectChanged;
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.BeginHorizontal();
            DrawCharacterList();
            DrawDivider();
            DrawCharacterDetails();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            _searchText = GUILayout.TextField(
                _searchText,
                EditorStyles.toolbarSearchField,
                GUILayout.MinWidth(180f));

            GUILayout.FlexibleSpace();
            GUILayout.Label(
                $"{_characters.Count} Characters",
                EditorStyles.miniLabel);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                RefreshCharacters(restoreSelection: true);
            }

            if (GUILayout.Button("+ Create", EditorStyles.toolbarButton))
            {
                CreateCharacter();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCharacterList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(ListWidth));

            EditorGUILayout.LabelField("Characters", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Select an asset to edit its gameplay and display data.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(4f);

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);

            bool hasVisibleCharacter = false;
            foreach (CharacterData character in GetFilteredCharacters())
            {
                hasVisibleCharacter = true;
                DrawCharacterRow(character);
            }

            if (!hasVisibleCharacter)
            {
                EditorGUILayout.HelpBox(
                    string.IsNullOrWhiteSpace(_searchText)
                        ? "No character data exists. Create the first character."
                        : "No character matches the search text.",
                    MessageType.Info);
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Create Character", GUILayout.Height(30f)))
            {
                CreateCharacter();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCharacterRow(CharacterData character)
        {
            bool isSelected = ReferenceEquals(character, _selectedCharacter);
            Color originalBackground = GUI.backgroundColor;

            if (isSelected)
            {
                GUI.backgroundColor = new Color(0.45f, 0.75f, 1f, 1f);
            }
            else if (!character.IsValid)
            {
                GUI.backgroundColor = new Color(1f, 0.65f, 0.55f, 1f);
            }

            string displayName = string.IsNullOrWhiteSpace(character.DisplayName)
                ? "<Unnamed Character>"
                : character.DisplayName;
            string assetName = Path.GetFileNameWithoutExtension(
                AssetDatabase.GetAssetPath(character));
            string status = character.IsValid ? "Ready" : "Needs Setup";
            Texture portraitThumbnail = character.Portrait != null
                ? AssetPreview.GetMiniThumbnail(character.Portrait)
                : null;

            var content = new GUIContent(
                $"{displayName}\n{assetName}  ·  {status}",
                portraitThumbnail);

            if (GUILayout.Button(content, GUILayout.Height(48f)))
            {
                SelectCharacter(character);
            }

            GUI.backgroundColor = originalBackground;
        }

        private static void DrawDivider()
        {
            Rect divider = EditorGUILayout.GetControlRect(
                false,
                GUILayout.Width(1f),
                GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(divider, new Color(0f, 0f, 0f, 0.25f));
        }

        private void DrawCharacterDetails()
        {
            EditorGUILayout.BeginVertical();

            if (_selectedCharacter == null || _serializedCharacter == null)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox(
                    "Select a character from the list or create a new one.",
                    MessageType.Info);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                return;
            }

            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            _serializedCharacter.Update();

            DrawDetailHeader();
            EditorGUILayout.Space(8f);
            DrawValidation();
            EditorGUILayout.Space(8f);
            DrawDisplaySection();
            EditorGUILayout.Space(8f);
            DrawAnimationSection();
            EditorGUILayout.Space(8f);
            DrawGameplaySection();
            EditorGUILayout.Space(12f);
            DrawAssetSection();

            if (_serializedCharacter != null
                && _serializedCharacter.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_selectedCharacter);
                Repaint();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailHeader()
        {
            EditorGUILayout.BeginHorizontal();

            Rect previewRect = GUILayoutUtility.GetRect(
                96f,
                96f,
                GUILayout.Width(96f),
                GUILayout.Height(96f));
            EditorGUI.DrawRect(previewRect, new Color(0f, 0f, 0f, 0.18f));

            if (_selectedCharacter.Portrait != null)
            {
                Texture portraitPreview =
                    AssetPreview.GetAssetPreview(_selectedCharacter.Portrait)
                    ?? AssetPreview.GetMiniThumbnail(
                        _selectedCharacter.Portrait);
                GUI.DrawTexture(
                    previewRect,
                    portraitPreview,
                    ScaleMode.ScaleToFit,
                    true);

                if (AssetPreview.IsLoadingAssetPreview(
                        _selectedCharacter.Portrait.GetEntityId()))
                {
                    Repaint();
                }
            }
            else
            {
                GUI.Label(previewRect, "No Portrait", CenteredMiniLabel);
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(
                string.IsNullOrWhiteSpace(_selectedCharacter.DisplayName)
                    ? "Unnamed Character"
                    : _selectedCharacter.DisplayName,
                EditorStyles.largeLabel);
            EditorGUILayout.LabelField(
                AssetDatabase.GetAssetPath(_selectedCharacter),
                EditorStyles.wordWrappedMiniLabel);
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping Asset"))
            {
                EditorGUIUtility.PingObject(_selectedCharacter);
                Selection.activeObject = _selectedCharacter;
            }

            if (GUILayout.Button("Duplicate"))
            {
                DuplicateSelectedCharacter();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawValidation()
        {
            if (_selectedCharacter.IsValid)
            {
                EditorGUILayout.HelpBox(
                    _selectedCharacter.Portrait == null
                        ? "Gameplay data is ready. Portrait can be assigned later."
                        : "Character data is ready to use.",
                    _selectedCharacter.Portrait == null
                        ? MessageType.Warning
                        : MessageType.Info);
                return;
            }

            var missingFields = new List<string>();
            if (string.IsNullOrWhiteSpace(_selectedCharacter.DisplayName))
            {
                missingFields.Add("Character Name");
            }

            if (_selectedCharacter.Stats == null)
            {
                missingFields.Add("Player Stats");
            }

            if (_selectedCharacter.StartingWeapon == null)
            {
                missingFields.Add("Starting Weapon");
            }

            if (_selectedCharacter.IdleFrames == null
                || _selectedCharacter.IdleFrames.Length == 0
                || _selectedCharacter.MoveFrames == null
                || _selectedCharacter.MoveFrames.Length == 0
                || _selectedCharacter.DeathFrames == null
                || _selectedCharacter.DeathFrames.Length == 0)
            {
                missingFields.Add("Animation Frames");
            }

            EditorGUILayout.HelpBox(
                $"Required fields: {string.Join(", ", missingFields)}",
                MessageType.Error);
        }

        private void DrawDisplaySection()
        {
            EditorGUILayout.LabelField("Display", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                _serializedCharacter.FindProperty("_displayName"),
                new GUIContent("Character Name"));
            EditorGUILayout.PropertyField(
                _serializedCharacter.FindProperty("_portrait"),
                new GUIContent("Portrait"));
        }

        private void DrawGameplaySection()
        {
            EditorGUILayout.LabelField("Gameplay", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                _serializedCharacter.FindProperty("_stats"),
                new GUIContent("Player Stats"));
            EditorGUILayout.PropertyField(
                _serializedCharacter.FindProperty("_startingWeapon"),
                new GUIContent("Starting Weapon"));

            CharacterData character = _selectedCharacter;
            if (character.StartingWeapon != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                GUILayout.Label(
                    AssetPreview.GetMiniThumbnail(
                        character.StartingWeapon.Icon),
                    GUILayout.Width(42f),
                    GUILayout.Height(42f));
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(
                    character.StartingWeapon.DisplayName,
                    EditorStyles.boldLabel);
                EditorGUILayout.LabelField(
                    $"Max Level {character.StartingWeapon.MaxLevel}",
                    EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawAnimationSection()
        {
            EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                _serializedCharacter.FindProperty("_idleFrames"),
                new GUIContent("Idle Frames"),
                includeChildren: true);
            EditorGUILayout.PropertyField(
                _serializedCharacter.FindProperty("_moveFrames"),
                new GUIContent("Move Frames"),
                includeChildren: true);
            EditorGUILayout.PropertyField(
                _serializedCharacter.FindProperty("_deathFrames"),
                new GUIContent("Death Frames"),
                includeChildren: true);
        }

        private void DrawAssetSection()
        {
            EditorGUILayout.LabelField("Asset Management", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _assetName = EditorGUILayout.TextField("Asset File Name", _assetName);

            using (new EditorGUI.DisabledScope(
                       string.IsNullOrWhiteSpace(_assetName)))
            {
                if (GUILayout.Button("Rename", GUILayout.Width(76f)))
                {
                    RenameSelectedCharacter();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save All", GUILayout.Height(28f)))
            {
                AssetDatabase.SaveAssets();
            }

            Color originalBackground = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.45f, 0.45f, 1f);
            if (GUILayout.Button("Delete Character", GUILayout.Height(28f)))
            {
                DeleteSelectedCharacter();
            }

            GUI.backgroundColor = originalBackground;
            EditorGUILayout.EndHorizontal();
        }

        private IEnumerable<CharacterData> GetFilteredCharacters()
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return _characters;
            }

            string search = _searchText.Trim();
            return _characters.Where(character =>
                character.name.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(character.DisplayName)
                    && character.DisplayName.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase)));
        }

        private void CreateCharacter()
        {
            EnsureDataDirectory();

            string path = EditorUtility.SaveFilePanelInProject(
                "Create Character Data",
                "SO_CharacterDefinition_New",
                "asset",
                "Choose where to save the character data.",
                DataDirectory);

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var character = CreateInstance<CharacterData>();
            AssetDatabase.CreateAsset(character, path);
            AssetDatabase.SaveAssets();
            RefreshCharacters(restoreSelection: false);
            SelectCharacter(character);
        }

        private void DuplicateSelectedCharacter()
        {
            if (_selectedCharacter == null)
            {
                return;
            }

            string sourcePath = AssetDatabase.GetAssetPath(_selectedCharacter);
            string destinationPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(
                        Path.GetDirectoryName(sourcePath) ?? DataDirectory,
                        $"{Path.GetFileNameWithoutExtension(sourcePath)}_Copy.asset")
                    .Replace('\\', '/'));

            if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
            {
                Debug.LogError($"Failed to duplicate character: {sourcePath}");
                return;
            }

            AssetDatabase.SaveAssets();
            RefreshCharacters(restoreSelection: false);
            SelectCharacter(
                AssetDatabase.LoadAssetAtPath<CharacterData>(destinationPath));
        }

        private void RenameSelectedCharacter()
        {
            if (_selectedCharacter == null)
            {
                return;
            }

            string sanitizedName = _assetName.Trim();
            if (sanitizedName.EndsWith(
                    ".asset",
                    StringComparison.OrdinalIgnoreCase))
            {
                sanitizedName = Path.GetFileNameWithoutExtension(sanitizedName);
            }

            if (string.IsNullOrWhiteSpace(sanitizedName)
                || sanitizedName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                EditorUtility.DisplayDialog(
                    "Invalid Asset Name",
                    "Enter a valid file name without path characters.",
                    "OK");
                return;
            }

            string path = AssetDatabase.GetAssetPath(_selectedCharacter);
            string error = AssetDatabase.RenameAsset(path, sanitizedName);
            if (!string.IsNullOrWhiteSpace(error))
            {
                EditorUtility.DisplayDialog("Rename Failed", error, "OK");
                return;
            }

            AssetDatabase.SaveAssets();
            RefreshCharacters(restoreSelection: true);
        }

        private void DeleteSelectedCharacter()
        {
            if (_selectedCharacter == null)
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(_selectedCharacter);
            string displayName = string.IsNullOrWhiteSpace(
                _selectedCharacter.DisplayName)
                ? _selectedCharacter.name
                : _selectedCharacter.DisplayName;

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Character Data",
                $"Delete '{displayName}'?\n\n{path}\n\n"
                + "References to this character will become missing.",
                "Delete",
                "Cancel");
            if (!confirmed)
            {
                return;
            }

            _selectedCharacter = null;
            _serializedCharacter = null;
            SessionState.EraseString(SelectedAssetSessionKey);

            if (!AssetDatabase.DeleteAsset(path))
            {
                EditorUtility.DisplayDialog(
                    "Delete Failed",
                    $"Could not delete {path}.",
                    "OK");
            }

            AssetDatabase.SaveAssets();
            RefreshCharacters(restoreSelection: false);
        }

        private void SelectCharacter(CharacterData character)
        {
            _selectedCharacter = character;
            _serializedCharacter = character != null
                ? new SerializedObject(character)
                : null;
            _assetName = character != null
                ? Path.GetFileNameWithoutExtension(
                    AssetDatabase.GetAssetPath(character))
                : string.Empty;
            _detailScroll = Vector2.zero;

            if (character != null)
            {
                SessionState.SetString(
                    SelectedAssetSessionKey,
                    AssetDatabase.GetAssetPath(character));
            }

            Repaint();
        }

        private void RefreshCharacters(bool restoreSelection)
        {
            string selectedPath = restoreSelection
                ? _selectedCharacter != null
                    ? AssetDatabase.GetAssetPath(_selectedCharacter)
                    : SessionState.GetString(SelectedAssetSessionKey, string.Empty)
                : string.Empty;

            _characters.Clear();
            _characters.AddRange(
                AssetDatabase.FindAssets(
                        "t:CharacterData",
                        new[] { DataDirectory })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<CharacterData>)
                    .Where(character => character != null)
                    .OrderBy(character => character.DisplayName)
                    .ThenBy(character => character.name));

            SynchronizeCatalog();

            CharacterData selected = !string.IsNullOrWhiteSpace(selectedPath)
                ? AssetDatabase.LoadAssetAtPath<CharacterData>(selectedPath)
                : null;
            SelectCharacter(selected);
        }

        private static void EnsureDataDirectory()
        {
            if (AssetDatabase.IsValidFolder(DataDirectory))
            {
                return;
            }

            string[] folders = DataDirectory.Split('/');
            string currentPath = folders[0];
            for (int index = 1; index < folders.Length; index++)
            {
                string nextPath = $"{currentPath}/{folders[index]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[index]);
                }

                currentPath = nextPath;
            }
        }

        private void SynchronizeCatalog()
        {
            CharacterCatalog catalog =
                AssetDatabase.LoadAssetAtPath<CharacterCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = CreateInstance<CharacterCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var serializedCatalog = new SerializedObject(catalog);
            SerializedProperty charactersProperty =
                serializedCatalog.FindProperty("_characters");

            bool isAlreadySynchronized =
                charactersProperty.arraySize == _characters.Count;
            if (isAlreadySynchronized)
            {
                for (int index = 0; index < _characters.Count; index++)
                {
                    if (!ReferenceEquals(
                            charactersProperty
                                .GetArrayElementAtIndex(index)
                                .objectReferenceValue,
                            _characters[index]))
                    {
                        isAlreadySynchronized = false;
                        break;
                    }
                }
            }

            if (isAlreadySynchronized)
            {
                return;
            }

            charactersProperty.arraySize = _characters.Count;
            for (int index = 0; index < _characters.Count; index++)
            {
                charactersProperty
                    .GetArrayElementAtIndex(index)
                    .objectReferenceValue = _characters[index];
            }

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private void HandleProjectChanged()
        {
            RefreshCharacters(restoreSelection: true);
        }

        private static GUIStyle CenteredMiniLabel
        {
            get
            {
                var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    wordWrap = true,
                };
                return style;
            }
        }
    }
}
