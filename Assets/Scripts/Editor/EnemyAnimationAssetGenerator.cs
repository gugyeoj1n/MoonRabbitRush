using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MoonRabbitRush.Editor.Animation
{
    public static class EnemyAnimationAssetGenerator
    {
        private const string RootDirectory = "Assets/Animations/Enemies";
        private const string BaseDirectory = RootDirectory + "/Base";
        private const string BaseClipPath =
            BaseDirectory + "/AN_Enemy_Move_Base.anim";
        private const string BaseControllerPath =
            BaseDirectory + "/AC_Enemy_Base.controller";
        private const float MoveFrameRate = 8f;
        private const int ExpectedFrameCount = 8;

        private static readonly EnemyAnimationDefinition[] Definitions =
        {
            new(
                "Globy",
                "Assets/Art/Enemies/GreenCyclops_Move.png",
                "Assets/Prefabs/Enemies/PF_Enemy_Globy.prefab"),
            new(
                "Inkto",
                "Assets/Art/Enemies/PurpleOctopus_Move.png",
                "Assets/Prefabs/Enemies/PF_Enemy_Inkto.prefab"),
            new(
                "Orbitron",
                "Assets/Art/Enemies/SmallUFO_Move.png",
                "Assets/Prefabs/Enemies/PF_Enemy_Orbitron.prefab"),
        };

        [InitializeOnLoadMethod]
        private static void GenerateMissingAssets()
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(
                    BaseControllerPath) == null)
            {
                EditorApplication.delayCall += Generate;
            }
        }

        [MenuItem("Moon Rabbit Rush/Enemies/Generate Move Animations")]
        public static void Generate()
        {
            EnsureDirectory(BaseDirectory);

            AnimationClip baseClip = CreateOrLoadEmptyBaseClip();
            AnimatorController baseController =
                CreateOrUpdateBaseController(baseClip);

            foreach (EnemyAnimationDefinition definition in Definitions)
            {
                GenerateEnemyAssets(definition, baseController, baseClip);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated independent move animations for all enemies.");
        }

        private static void GenerateEnemyAssets(
            EnemyAnimationDefinition definition,
            AnimatorController baseController,
            AnimationClip baseClip)
        {
            string enemyDirectory = $"{RootDirectory}/{definition.EnemyName}";
            EnsureDirectory(enemyDirectory);

            IReadOnlyList<Sprite> sprites = LoadOrderedSprites(definition);
            string clipPath =
                $"{enemyDirectory}/AN_{definition.EnemyName}_Move.anim";
            string overridePath =
                $"{enemyDirectory}/AOC_{definition.EnemyName}.overrideController";

            AnimationClip moveClip = CreateOrReplaceMoveClip(clipPath, sprites);
            AnimatorOverrideController overrideController =
                CreateOrUpdateOverrideController(
                    overridePath,
                    baseController,
                    baseClip,
                    moveClip);

            BindControllerToPrefab(definition.PrefabPath, overrideController);
        }

        private static IReadOnlyList<Sprite> LoadOrderedSprites(
            EnemyAnimationDefinition definition)
        {
            Sprite[] sprites = AssetDatabase
                .LoadAllAssetsAtPath(definition.SpriteSheetPath)
                .OfType<Sprite>()
                .OrderByDescending(sprite => sprite.rect.y)
                .ThenBy(sprite => sprite.rect.x)
                .ToArray();

            if (sprites.Length != ExpectedFrameCount)
            {
                throw new InvalidOperationException(
                    $"{definition.EnemyName} move sheet must contain exactly " +
                    $"{ExpectedFrameCount} sprites, but found {sprites.Length}: " +
                    definition.SpriteSheetPath);
            }

            return sprites;
        }

        private static AnimationClip CreateOrLoadEmptyBaseClip()
        {
            AnimationClip clip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(BaseClipPath);

            if (clip == null)
            {
                clip = new AnimationClip
                {
                    name = Path.GetFileNameWithoutExtension(BaseClipPath),
                    frameRate = MoveFrameRate,
                };
                AssetDatabase.CreateAsset(clip, BaseClipPath);
            }

            return clip;
        }

        private static AnimationClip CreateOrReplaceMoveClip(
            string path,
            IReadOnlyList<Sprite> sprites)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.name = Path.GetFileNameWithoutExtension(path);
            clip.frameRate = MoveFrameRate;

            var keyframes = new ObjectReferenceKeyframe[sprites.Count];

            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / MoveFrameRate,
                    value = sprites[i],
                };
            }

            var binding = new EditorCurveBinding
            {
                path = string.Empty,
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite",
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            AnimationClipSettings settings =
                AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorController CreateOrUpdateBaseController(
            AnimationClip baseClip)
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(
                    BaseControllerPath);

            if (controller == null)
            {
                controller =
                    AnimatorController.CreateAnimatorControllerAtPath(
                        BaseControllerPath);
            }

            AnimatorStateMachine stateMachine =
                controller.layers[0].stateMachine;
            AnimatorState moveState = stateMachine.states
                .Select(childState => childState.state)
                .FirstOrDefault(state => state.name == "Move");

            if (moveState == null)
            {
                moveState = stateMachine.AddState("Move");
            }

            moveState.motion = baseClip;
            stateMachine.defaultState = moveState;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorOverrideController
            CreateOrUpdateOverrideController(
                string path,
                AnimatorController baseController,
                AnimationClip baseClip,
                AnimationClip moveClip)
        {
            AnimatorOverrideController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);

            if (controller == null)
            {
                controller = new AnimatorOverrideController();
                AssetDatabase.CreateAsset(controller, path);
            }

            controller.runtimeAnimatorController = baseController;
            controller[baseClip] = moveClip;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void BindControllerToPrefab(
            string prefabPath,
            RuntimeAnimatorController controller)
        {
            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                Animator animator = prefabRoot.GetComponent<Animator>();

                if (animator == null)
                {
                    animator = prefabRoot.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private sealed class EnemyAnimationDefinition
        {
            public EnemyAnimationDefinition(
                string enemyName,
                string spriteSheetPath,
                string prefabPath)
            {
                EnemyName = enemyName;
                SpriteSheetPath = spriteSheetPath;
                PrefabPath = prefabPath;
            }

            public string EnemyName { get; }
            public string SpriteSheetPath { get; }
            public string PrefabPath { get; }
        }
    }
}
