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
        private const string BaseControllerPath =
            BaseDirectory + "/AC_Enemy_Base.controller";
        private const string PlayerPrefabPath =
            "Assets/Prefabs/Characters/PF_Character_Rabbit.prefab";
        private const string PlayerDeathSheetPath =
            "Assets/Art/Characters/Player_Die.png";
        private const float FrameRate = 8f;

        private static readonly EnemyAnimationDefinition[] Definitions =
        {
            new(
                "Globy",
                "Assets/Art/Enemies/GreenCyclops_idle.png",
                "Assets/Art/Enemies/GreenCyclops_Move.png",
                null,
                "Assets/Art/Enemies/Slime_Die.png",
                "Assets/Prefabs/Enemies/PF_Enemy_Globy.prefab"),
            new(
                "Inkto",
                "Assets/Art/Enemies/PurpleOctopus_Idle.png",
                "Assets/Art/Enemies/PurpleOctopus_Move.png",
                "Assets/Art/Enemies/PurpleOctopus_Attack.png",
                "Assets/Art/Enemies/PurpleOctopus_Die.png",
                "Assets/Prefabs/Enemies/PF_Enemy_Inkto.prefab"),
            new(
                "Orbitron",
                "Assets/Art/Enemies/SmallUFO_Move.png",
                "Assets/Art/Enemies/SmallUFO_Move.png",
                "Assets/Art/Enemies/SmallUFO_Attack.png",
                "Assets/Art/Enemies/UFO_Die.png",
                "Assets/Prefabs/Enemies/PF_Enemy_Orbitron.prefab"),
            new(
                "BossUFO",
                "Assets/Art/Enemies/Boss_Idle.png",
                "Assets/Art/Enemies/Boss_Idle.png",
                "Assets/Art/Enemies/Boss_Attack.png",
                "Assets/Art/Enemies/Boss_Die.png",
                "Assets/Prefabs/Enemies/PF_Enemy_BossUFO.prefab"),
        };

        [InitializeOnLoadMethod]
        private static void GenerateMissingAssets()
        {
            const string bossControllerPath =
                RootDirectory + "/BossUFO/AOC_BossUFO.overrideController";
            GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Enemies/PF_Enemy_BossUFO.prefab");
            bool isBossBindingMissing =
                bossPrefab == null ||
                bossPrefab.GetComponent<Animator>() == null ||
                bossPrefab.GetComponent<
                    MoonRabbitRush.Enemies.EnemyAnimationController>() == null;

            if (AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(
                    bossControllerPath) == null || isBossBindingMissing)
            {
                EditorApplication.delayCall += Generate;
            }
        }

        [MenuItem("Moon Rabbit Rush/Enemies/Generate Animation Set")]
        public static void Generate()
        {
            EnsureDirectory(BaseDirectory);

            AnimationClip idleBase = CreateOrReplaceClip(
                BaseDirectory + "/AN_Enemy_Idle_Base.anim",
                Array.Empty<Sprite>(),
                true);
            AnimationClip moveBase = CreateOrReplaceClip(
                BaseDirectory + "/AN_Enemy_Move_Base.anim",
                Array.Empty<Sprite>(),
                true);
            AnimationClip attackBase = CreateOrReplaceClip(
                BaseDirectory + "/AN_Enemy_Attack_Base.anim",
                Array.Empty<Sprite>(),
                false);
            AnimatorController baseController = CreateOrUpdateBaseController(
                idleBase,
                moveBase,
                attackBase);

            foreach (EnemyAnimationDefinition definition in Definitions)
            {
                GenerateEnemyAssets(
                    definition,
                    baseController,
                    idleBase,
                    moveBase,
                    attackBase);
            }

            BindPlayerDeathFrames();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated enemy idle, move, attack, and death bindings.");
        }

        private static void GenerateEnemyAssets(
            EnemyAnimationDefinition definition,
            AnimatorController baseController,
            AnimationClip idleBase,
            AnimationClip moveBase,
            AnimationClip attackBase)
        {
            string enemyDirectory = $"{RootDirectory}/{definition.EnemyName}";
            EnsureDirectory(enemyDirectory);

            IReadOnlyList<Sprite> idleSprites =
                LoadOrderedSprites(definition.IdleSheetPath);
            IReadOnlyList<Sprite> moveSprites =
                LoadOrderedSprites(definition.MoveSheetPath);
            IReadOnlyList<Sprite> attackSprites =
                string.IsNullOrEmpty(definition.AttackSheetPath)
                    ? idleSprites
                    : LoadOrderedSprites(definition.AttackSheetPath);
            IReadOnlyList<Sprite> deathSprites =
                LoadOrderedSprites(definition.DeathSheetPath);

            AnimationClip idleClip = CreateOrReplaceClip(
                $"{enemyDirectory}/AN_{definition.EnemyName}_Idle.anim",
                idleSprites,
                true);
            AnimationClip moveClip = CreateOrReplaceClip(
                $"{enemyDirectory}/AN_{definition.EnemyName}_Move.anim",
                moveSprites,
                true);
            AnimationClip attackClip = CreateOrReplaceClip(
                $"{enemyDirectory}/AN_{definition.EnemyName}_Attack.anim",
                attackSprites,
                false);
            AnimatorOverrideController overrideController =
                CreateOrUpdateOverrideController(
                    $"{enemyDirectory}/AOC_{definition.EnemyName}.overrideController",
                    baseController,
                    idleBase,
                    moveBase,
                    attackBase,
                    idleClip,
                    moveClip,
                    attackClip);

            BindPrefab(
                definition.PrefabPath,
                overrideController,
                deathSprites);
        }

        private static IReadOnlyList<Sprite> LoadOrderedSprites(string path)
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderByDescending(sprite => sprite.rect.y)
                .ThenBy(sprite => sprite.rect.x)
                .ToArray();

            if (sprites.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Animation sheet has no sliced sprites: {path}");
            }

            return sprites;
        }

        private static AnimationClip CreateOrReplaceClip(
            string path,
            IReadOnlyList<Sprite> sprites,
            bool loop)
        {
            AnimationClip clip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.name = Path.GetFileNameWithoutExtension(path);
            clip.frameRate = FrameRate;
            var keyframes = new ObjectReferenceKeyframe[sprites.Count];

            for (int index = 0; index < sprites.Count; index++)
            {
                keyframes[index] = new ObjectReferenceKeyframe
                {
                    time = index / FrameRate,
                    value = sprites[index],
                };
            }

            var binding = new EditorCurveBinding
            {
                path = string.Empty,
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite",
            };
            AnimationUtility.SetObjectReferenceCurve(
                clip,
                binding,
                keyframes);
            AnimationClipSettings settings =
                AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorController CreateOrUpdateBaseController(
            AnimationClip idleClip,
            AnimationClip moveClip,
            AnimationClip attackClip)
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

            controller.parameters = new[]
            {
                new AnimatorControllerParameter
                {
                    name = "IsMoving",
                    type = AnimatorControllerParameterType.Bool,
                },
                new AnimatorControllerParameter
                {
                    name = "Attack",
                    type = AnimatorControllerParameterType.Trigger,
                },
            };

            AnimatorStateMachine stateMachine =
                controller.layers[0].stateMachine;
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                stateMachine.RemoveState(childState.state);
            }

            AnimatorState idleState = stateMachine.AddState("Idle");
            AnimatorState moveState = stateMachine.AddState("Move");
            AnimatorState attackState = stateMachine.AddState("Attack");
            idleState.motion = idleClip;
            moveState.motion = moveClip;
            attackState.motion = attackClip;
            stateMachine.defaultState = idleState;

            AddBoolTransition(idleState, moveState, true);
            AddBoolTransition(moveState, idleState, false);

            AnimatorStateTransition attackTransition =
                stateMachine.AddAnyStateTransition(attackState);
            attackTransition.hasExitTime = false;
            attackTransition.duration = 0.03f;
            attackTransition.canTransitionToSelf = false;
            attackTransition.AddCondition(
                AnimatorConditionMode.If,
                0f,
                "Attack");

            AddAttackExitTransition(attackState, moveState, true);
            AddAttackExitTransition(attackState, idleState, false);
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void AddBoolTransition(
            AnimatorState source,
            AnimatorState destination,
            bool whenMoving)
        {
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.AddCondition(
                whenMoving ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                "IsMoving");
        }

        private static void AddAttackExitTransition(
            AnimatorState source,
            AnimatorState destination,
            bool whenMoving)
        {
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = true;
            transition.exitTime = 1f;
            transition.duration = 0.05f;
            transition.AddCondition(
                whenMoving ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                "IsMoving");
        }

        private static AnimatorOverrideController
            CreateOrUpdateOverrideController(
                string path,
                AnimatorController baseController,
                AnimationClip idleBase,
                AnimationClip moveBase,
                AnimationClip attackBase,
                AnimationClip idleClip,
                AnimationClip moveClip,
                AnimationClip attackClip)
        {
            AnimatorOverrideController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);

            if (controller == null)
            {
                controller = new AnimatorOverrideController();
                AssetDatabase.CreateAsset(controller, path);
            }

            controller.runtimeAnimatorController = baseController;
            controller.ApplyOverrides(new List<KeyValuePair<AnimationClip, AnimationClip>>
            {
                new(idleBase, idleClip),
                new(moveBase, moveClip),
                new(attackBase, attackClip),
            });
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void BindPrefab(
            string prefabPath,
            RuntimeAnimatorController controller,
            IReadOnlyList<Sprite> deathFrames)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

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

                if (prefabRoot.GetComponent<
                        MoonRabbitRush.Enemies.EnemyAnimationController>() == null)
                {
                    prefabRoot.AddComponent<
                        MoonRabbitRush.Enemies.EnemyAnimationController>();
                }

                MoonRabbitRush.Enemies.EnemyDeathSpriteAnimation deathAnimation =
                    prefabRoot.GetComponent<
                        MoonRabbitRush.Enemies.EnemyDeathSpriteAnimation>();
                if (deathAnimation == null)
                {
                    deathAnimation = prefabRoot.AddComponent<
                        MoonRabbitRush.Enemies.EnemyDeathSpriteAnimation>();
                }

                var serializedDeathAnimation =
                    new SerializedObject(deathAnimation);
                SerializedProperty framesProperty =
                    serializedDeathAnimation.FindProperty("_deathFrames");
                framesProperty.arraySize = deathFrames.Count;

                for (int index = 0; index < deathFrames.Count; index++)
                {
                    framesProperty.GetArrayElementAtIndex(index)
                        .objectReferenceValue = deathFrames[index];
                }

                serializedDeathAnimation.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void BindPlayerDeathFrames()
        {
            IReadOnlyList<Sprite> deathFrames =
                LoadOrderedSprites(PlayerDeathSheetPath);
            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

            try
            {
                MoonRabbitRush.Player.PlayerSpriteAnimation animation =
                    prefabRoot.GetComponentInChildren<
                        MoonRabbitRush.Player.PlayerSpriteAnimation>(true);
                var serializedAnimation = new SerializedObject(animation);
                SerializedProperty framesProperty =
                    serializedAnimation.FindProperty("_deathFrames");
                framesProperty.arraySize = deathFrames.Count;

                for (int index = 0; index < deathFrames.Count; index++)
                {
                    framesProperty.GetArrayElementAtIndex(index)
                        .objectReferenceValue = deathFrames[index];
                }

                serializedAnimation.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
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
                string idleSheetPath,
                string moveSheetPath,
                string attackSheetPath,
                string deathSheetPath,
                string prefabPath)
            {
                EnemyName = enemyName;
                IdleSheetPath = idleSheetPath;
                MoveSheetPath = moveSheetPath;
                AttackSheetPath = attackSheetPath;
                DeathSheetPath = deathSheetPath;
                PrefabPath = prefabPath;
            }

            public string EnemyName { get; }
            public string IdleSheetPath { get; }
            public string MoveSheetPath { get; }
            public string AttackSheetPath { get; }
            public string DeathSheetPath { get; }
            public string PrefabPath { get; }
        }
    }
}
