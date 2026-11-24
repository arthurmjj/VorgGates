using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace VorgGates.EditorTools
{
    /// <summary>Imports rigged external boss models as visual-only children of the existing entities.</summary>
    internal static class VorgBossModels
    {
        private const string KnightPath = VorgArchitecture.Root + "/Knight/knight.fbx";
        private const string ControllerPath = VorgArchitecture.Root + "/Knight/AC_Knight.controller";

        public static void Prepare()
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(KnightPath);
            var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            var needsLoop = clips.Any(c => !c.loopTime);
            if (importer.animationType != ModelImporterAnimationType.Generic || importer.addCollider || importer.importCameras || importer.importLights || needsLoop)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.addCollider = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.importAnimation = true;
                foreach (var clip in clips)
                {
                    clip.loopTime = true;
                    clip.lockRootPositionXZ = true;
                    clip.keepOriginalPositionXZ = true;
                }

                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null) AssetDatabase.DeleteAsset(ControllerPath);
            var assets = AssetDatabase.LoadAllAssetsAtPath(KnightPath).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();
            var idle = assets.First(c => c.name.Contains("idle"));
            var walk = assets.First(c => c.name.Contains("walk"));
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            var machine = controller.layers[0].stateMachine;
            var idleState = machine.AddState("Idle");
            idleState.motion = idle;
            var walkState = machine.AddState("Walk");
            walkState.motion = walk;
            machine.defaultState = idleState;
            Transition(idleState, walkState, AnimatorConditionMode.If);
            Transition(walkState, idleState, AnimatorConditionMode.IfNot);
            EditorUtility.SetDirty(controller);
        }

        /// <summary>Adds the knight model with its base at the parent's origin and returns its Animator.</summary>
        public static Animator PlaceKnight(Transform parent, float height, Material material)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(KnightPath);
            var instance = Object.Instantiate(source, parent, false);
            instance.name = "SentinelModel";
            var scale = height / 1.96f;
            instance.transform.localScale = Vector3.Scale(source.transform.localScale * scale, new Vector3(1.32f, 1f, 1.25f));
            instance.transform.localPosition = Vector3.up * (0.06f * scale);
            instance.transform.localRotation = source.transform.localRotation;
            foreach (var renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                renderer.sharedMaterial = material;
                renderer.updateWhenOffscreen = false;
                renderer.localBounds = new Bounds(Vector3.up, new Vector3(2f, 2.4f, 2f));
            }

            foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            if (!instance.TryGetComponent<Animator>(out var animator)) animator = instance.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            return animator;
        }

        private static void Transition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.16f;
            transition.AddCondition(mode, 0f, "Moving");
        }
    }
}
